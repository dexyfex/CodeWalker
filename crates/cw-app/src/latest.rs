//! One background thread that runs only the newest job it was given.
//!
//! Previews are requested on every selection change. Arrow-keying through a
//! list would otherwise start a thread per row, each extracting a whole file;
//! here the rows that were passed over are never loaded at all.

use std::{
    panic::{AssertUnwindSafe, catch_unwind},
    sync::{Arc, Condvar, Mutex},
    thread,
};

struct State<J> {
    pending: Option<J>,
    closed: bool,
}

struct Shared<J> {
    state: Mutex<State<J>>,
    wake: Condvar,
}

pub struct LatestWins<J> {
    shared: Arc<Shared<J>>,
}

impl<J: Send + 'static> LatestWins<J> {
    /// Starts the worker. `handler` runs on it, one job at a time; a job that panics
    /// is dropped and the worker carries on with the next.
    pub fn start(mut handler: impl FnMut(J) + Send + 'static) -> Self {
        let shared = Arc::new(Shared {
            state: Mutex::new(State { pending: None, closed: false }),
            wake: Condvar::new(),
        });
        let worker = shared.clone();
        thread::Builder::new()
            .name("latest-wins".into())
            .spawn(move || {
                loop {
                    let job = {
                        let mut st = worker.state.lock().unwrap();
                        loop {
                            if st.closed {
                                return;
                            }
                            if let Some(job) = st.pending.take() {
                                break job;
                            }
                            st = worker.wake.wait(st).unwrap();
                        }
                    };
                    let _ = catch_unwind(AssertUnwindSafe(|| handler(job)));
                }
            })
            .expect("could not start a worker thread");
        Self { shared }
    }

    /// Queues `job`, replacing one that has not started yet.
    pub fn submit(&self, job: J) {
        self.shared.state.lock().unwrap().pending = Some(job);
        self.shared.wake.notify_one();
    }
}

impl<J> Drop for LatestWins<J> {
    fn drop(&mut self) {
        self.shared.state.lock().unwrap().closed = true;
        self.shared.wake.notify_all();
    }
}

#[cfg(test)]
mod tests {
    use std::{sync::mpsc, time::Duration};

    use super::*;

    #[test]
    fn jobs_passed_over_while_the_worker_is_busy_never_run() {
        let (done_tx, done_rx) = mpsc::channel();
        let (gate_tx, gate_rx) = mpsc::channel::<()>();
        let gate_rx = Mutex::new(gate_rx);
        let w = LatestWins::start(move |n: u32| {
            if n == 1 {
                gate_rx.lock().unwrap().recv().unwrap(); // job 1 holds the worker
            }
            done_tx.send(n).unwrap();
        });
        w.submit(1);
        thread::sleep(Duration::from_millis(50)); // let it pick job 1 up
        for n in [2, 3, 4] {
            w.submit(n);
        }
        gate_tx.send(()).unwrap();
        let ran: Vec<u32> = done_rx.iter().take(2).collect();
        assert_eq!(ran, [1, 4], "2 and 3 were replaced before they started");
        assert!(done_rx.recv_timeout(Duration::from_millis(100)).is_err(), "nothing else is queued");
    }

    #[test]
    fn a_panicking_job_does_not_stop_the_worker() {
        let (tx, rx) = mpsc::channel();
        let w = LatestWins::start(move |n: u32| {
            assert!(n != 0, "boom");
            tx.send(n).unwrap();
        });
        w.submit(0);
        thread::sleep(Duration::from_millis(50));
        w.submit(7);
        assert_eq!(rx.recv_timeout(Duration::from_secs(2)).unwrap(), 7);
    }

    #[test]
    fn dropping_the_handle_ends_the_thread() {
        let (tx, rx) = mpsc::channel::<()>();
        let w = LatestWins::start(move |_: ()| {
            let _keep = &tx; // the handler owns the sender; it is freed when the thread exits
        });
        drop(w);
        assert!(
            rx.recv_timeout(Duration::from_secs(2)).is_err_and(|e| e == mpsc::RecvTimeoutError::Disconnected)
        );
    }
}
