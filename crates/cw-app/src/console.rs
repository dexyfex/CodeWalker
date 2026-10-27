//! Console access for the command-line modes on Windows.
//!
//! The release executable is a GUI-subsystem program so that opening the window never flashes a
//! console. Such a program starts with no standard handles, so `println!` goes nowhere when it is
//! run from a terminal as `codewalker rpf.list`. This attaches to the terminal that started it,
//! for the modes that print. Elsewhere there is nothing to do.

#[cfg(not(windows))]
pub fn attach() {}

#[cfg(windows)]
pub fn attach() {
    use std::ffi::c_void;

    unsafe extern "system" {
        fn AttachConsole(process_id: u32) -> i32;
        fn GetStdHandle(which: u32) -> *mut c_void;
        fn SetStdHandle(which: u32, handle: *mut c_void) -> i32;
        fn CreateFileW(
            name: *const u16,
            access: u32,
            share: u32,
            security: *const c_void,
            disposition: u32,
            flags: u32,
            template: *mut c_void,
        ) -> *mut c_void;
    }
    const ATTACH_PARENT_PROCESS: u32 = u32::MAX;
    const STD_INPUT_HANDLE: u32 = -10i32 as u32;
    const STD_OUTPUT_HANDLE: u32 = -11i32 as u32;
    const STD_ERROR_HANDLE: u32 = -12i32 as u32;
    const GENERIC_READ: u32 = 0x8000_0000;
    const GENERIC_WRITE: u32 = 0x4000_0000;
    const FILE_SHARE_READ: u32 = 1;
    const FILE_SHARE_WRITE: u32 = 2;
    const OPEN_EXISTING: u32 = 3;
    let invalid = usize::MAX as *mut c_void;

    // SAFETY: plain Win32 calls with valid arguments; handles are only compared and passed back.
    unsafe {
        let missing = |which: u32| {
            let h = GetStdHandle(which);
            h.is_null() || h == invalid
        };
        let wanted =
            [(STD_INPUT_HANDLE, "CONIN$"), (STD_OUTPUT_HANDLE, "CONOUT$"), (STD_ERROR_HANDLE, "CONOUT$")];
        if !wanted.iter().any(|(which, _)| missing(*which)) {
            return; // redirected to a pipe or a file: already connected
        }
        if AttachConsole(ATTACH_PARENT_PROCESS) == 0 {
            return; // started from Explorer: there is no terminal to print to
        }
        for (which, device) in wanted {
            if !missing(which) {
                continue;
            }
            let name: Vec<u16> = device.encode_utf16().chain([0]).collect();
            let h = CreateFileW(
                name.as_ptr(),
                GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                std::ptr::null(),
                OPEN_EXISTING,
                0,
                std::ptr::null_mut(),
            );
            if h != invalid && !h.is_null() {
                SetStdHandle(which, h);
            }
        }
    }
}
