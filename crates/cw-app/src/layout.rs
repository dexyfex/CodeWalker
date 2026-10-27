//! Width arithmetic for the window chrome: what shows, and how wide, in the room there is.
//! Numbers in, numbers out, no egui, so the rules are unit-tested; the views measure their text,
//! ask these functions and paint what they decide.

use std::ops::Range;

/// The fixed widths a breadcrumb is built from.
#[derive(Clone, Copy, Debug)]
pub struct CrumbGaps {
    /// The separator between two crumbs.
    pub sep: f32,
    /// The `…` crumb that stands in for hidden folders.
    pub fold: f32,
    /// The least room the current location may be squeezed into before the first crumb gives way.
    pub min_last: f32,
}

/// One slot of a fitted breadcrumb, left to right.
#[derive(Clone, Debug, PartialEq)]
pub enum Crumb {
    /// Crumb `index`, drawn `width` wide: less than it asked for when its name is truncated.
    Shown { index: usize, width: f32 },
    /// The `…` crumb standing in for these crumbs; it opens a menu of them.
    Folded(Range<usize>),
}

/// Fits a path whose crumbs are `widths` wide into `avail`.
///
/// Everything fits: everything shows. Otherwise the first crumb stays, the middle folds into one
/// `…`, and as many of the last crumbs as fit follow it. When even the last crumb alone does not
/// fit it is truncated, down to `min_last`; below that the first crumb folds as well. The last
/// crumb is the current location, so it is always there.
pub fn fit_crumbs(widths: &[f32], avail: f32, gaps: CrumbGaps) -> Vec<Crumb> {
    let CrumbGaps { sep, fold, min_last } = gaps;
    let n = widths.len();
    let shown = |index: usize| Crumb::Shown { index, width: widths[index] };
    if n == 0 {
        return Vec::new();
    }
    let total = widths.iter().sum::<f32>() + sep * (n - 1) as f32;
    if total <= avail {
        return (0..n).map(shown).collect();
    }
    let last = n - 1;
    if n == 1 {
        return vec![Crumb::Shown { index: 0, width: avail.max(0.0) }];
    }

    // first, `…`, then the longest run of last crumbs that fits. Showing them all was too wide, so
    // at least one is hidden: the run starts at the third crumb at the earliest.
    let head = widths[0] + sep + fold + sep;
    let mut tail = 0.0;
    let mut start = None;
    for s in (2..n).rev() {
        tail += widths[s] + if s < last { sep } else { 0.0 };
        if head + tail > avail {
            break;
        }
        start = Some(s);
    }
    if let Some(s) = start {
        let mut out = vec![shown(0), Crumb::Folded(1..s)];
        out.extend((s..n).map(shown));
        return out;
    }

    // the first crumb stays while the current location keeps `min_last`
    if n > 2 && avail - head >= min_last {
        return vec![shown(0), Crumb::Folded(1..last), Crumb::Shown { index: last, width: avail - head }];
    }
    let beside_first = avail - widths[0] - sep;
    if n == 2 && beside_first >= min_last {
        return vec![shown(0), Crumb::Shown { index: last, width: beside_first }];
    }
    let room = (avail - fold - sep).max(0.0);
    vec![Crumb::Folded(0..last), Crumb::Shown { index: last, width: widths[last].min(room) }]
}

/// One way to make a row narrower. [`squeeze`] tries them in order.
#[derive(Clone, Copy, Debug)]
pub enum Give {
    /// Any amount up to this many pixels (a field that shrinks).
    UpTo(f32),
    /// This many pixels or none (labels that give way to icons).
    All(f32),
}

/// How many pixels each step gives back so that a row `over` pixels too wide fits. Steps are
/// taken in order and only as far as needed; when all of them are not enough, all are taken.
pub fn squeeze(over: f32, steps: &[Give]) -> Vec<f32> {
    let mut left = over;
    steps
        .iter()
        .map(|step| {
            if left <= 0.0 {
                return 0.0;
            }
            let given = match *step {
                Give::UpTo(most) => left.min(most.max(0.0)),
                Give::All(px) => px.max(0.0),
            };
            left -= given;
            given
        })
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;

    const GAPS: CrumbGaps = CrumbGaps { sep: 14.0, fold: 28.0, min_last: 96.0 };
    const SEP: f32 = GAPS.sep;
    const FOLD: f32 = GAPS.fold;

    /// Width of a fitted breadcrumb as drawn.
    fn drawn(fit: &[Crumb]) -> f32 {
        let slots: f32 = fit
            .iter()
            .map(|c| match c {
                Crumb::Shown { width, .. } => *width,
                Crumb::Folded(_) => FOLD,
            })
            .sum();
        slots + SEP * fit.len().saturating_sub(1) as f32
    }

    fn indices(fit: &[Crumb]) -> Vec<String> {
        fit.iter()
            .map(|c| match c {
                Crumb::Shown { index, .. } => index.to_string(),
                Crumb::Folded(r) => format!("…{}-{}", r.start, r.end - 1),
            })
            .collect()
    }

    // "Game files › update › x64 › dlcpacks › patchday25ng › dlc.rpf › x64 › models › cdimages ›
    // patchday25ng.rpf", measured roughly as the toolbar does (text plus 14 px of padding)
    const DEEP: [f32; 10] = [84.0, 62.0, 40.0, 76.0, 104.0, 60.0, 40.0, 60.0, 78.0, 130.0];

    #[test]
    fn a_path_that_fits_is_shown_whole() {
        let fit = fit_crumbs(&DEEP, 2000.0, GAPS);
        assert_eq!(indices(&fit), ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"]);
        assert!(fit.iter().all(|c| matches!(c, Crumb::Shown { index, width } if *width == DEEP[*index])));
        // exactly as wide as the room is still whole
        let exact = DEEP.iter().sum::<f32>() + SEP * 9.0;
        assert_eq!(fit_crumbs(&DEEP, exact, GAPS).len(), 10);
        let folded = fit_crumbs(&DEEP, exact - 1.0, GAPS);
        assert_eq!(
            indices(&folded),
            ["0", "…1-1", "2", "3", "4", "5", "6", "7", "8", "9"],
            "a pixel less folds"
        );
    }

    #[test]
    fn a_deep_path_keeps_the_first_and_the_last_crumbs_and_folds_the_middle() {
        let fit = fit_crumbs(&DEEP, 500.0, GAPS);
        assert_eq!(indices(&fit), ["0", "…1-5", "6", "7", "8", "9"]);
        assert!(drawn(&fit) <= 500.0);
        // one crumb more would not have fitted
        let wider = DEEP[0] + FOLD + DEEP[5..].iter().sum::<f32>() + SEP * 6.0;
        assert!(wider > 500.0);

        let fit = fit_crumbs(&DEEP, 300.0, GAPS);
        assert_eq!(indices(&fit), ["0", "…1-8", "9"]);
    }

    #[test]
    fn the_last_crumb_is_truncated_before_the_first_one_gives_way() {
        // room for the first crumb, `…` and 100 px of the 130 px location
        let head = DEEP[0] + SEP + FOLD + SEP;
        let fit = fit_crumbs(&DEEP, head + 100.0, GAPS);
        assert_eq!(
            fit,
            vec![
                Crumb::Shown { index: 0, width: 84.0 },
                Crumb::Folded(1..9),
                Crumb::Shown { index: 9, width: 100.0 }
            ]
        );
        assert_eq!(drawn(&fit), head + 100.0);

        // a bit less than `min_last` for it: now the first crumb folds and the location is whole
        let fit = fit_crumbs(&DEEP, head + 95.0, GAPS);
        assert_eq!(fit, vec![Crumb::Folded(0..9), Crumb::Shown { index: 9, width: 130.0 }]);
    }

    #[test]
    fn when_nothing_else_fits_the_location_alone_is_squeezed() {
        let fit = fit_crumbs(&DEEP, 120.0, GAPS);
        assert_eq!(fit, vec![Crumb::Folded(0..9), Crumb::Shown { index: 9, width: 120.0 - FOLD - SEP }]);
        assert!(drawn(&fit) <= 120.0);

        // no room at all still names the location, with nothing negative
        let fit = fit_crumbs(&DEEP, 10.0, GAPS);
        assert_eq!(fit[1], Crumb::Shown { index: 9, width: 0.0 });
    }

    #[test]
    fn short_paths_fold_and_truncate_the_same_way() {
        assert!(fit_crumbs(&[], 100.0, GAPS).is_empty());
        assert_eq!(fit_crumbs(&[300.0], 100.0, GAPS), vec![Crumb::Shown { index: 0, width: 100.0 }]);
        // two crumbs: the first stays while the second keeps `min_last`, else it folds
        assert_eq!(
            fit_crumbs(&[84.0, 400.0], 300.0, GAPS),
            vec![Crumb::Shown { index: 0, width: 84.0 }, Crumb::Shown { index: 1, width: 202.0 }]
        );
        assert_eq!(indices(&fit_crumbs(&[84.0, 400.0], 180.0, GAPS)), ["…0-0", "1"]);
        // a very long last crumb after a short path
        let fit = fit_crumbs(&[84.0, 62.0, 900.0], 400.0, GAPS);
        assert_eq!(
            fit,
            vec![
                Crumb::Shown { index: 0, width: 84.0 },
                Crumb::Folded(1..2),
                Crumb::Shown { index: 2, width: 400.0 - 84.0 - SEP - FOLD - SEP }
            ]
        );
    }

    #[test]
    fn every_width_gives_a_breadcrumb_that_fits_names_the_location_and_keeps_the_order() {
        for path in [&DEEP[..], &DEEP[..2], &DEEP[..3], &[84.0, 900.0, 40.0, 700.0]] {
            let last = path.len() - 1;
            for avail in (0..1400).step_by(7).map(|w| w as f32) {
                let fit = fit_crumbs(path, avail, GAPS);
                if avail >= FOLD + SEP {
                    assert!(drawn(&fit) <= avail + 1e-3, "{avail}: {fit:?}");
                }
                assert!(matches!(fit.last(), Some(Crumb::Shown { index, .. }) if *index == last), "{avail}");
                // shown and folded crumbs cover the path once, in order, with at most one `…`
                let mut next = 0;
                for c in &fit {
                    match c {
                        Crumb::Shown { index, width } => {
                            assert_eq!(*index, next);
                            assert!(*width >= 0.0 && *width <= path[*index]);
                            next += 1;
                        }
                        Crumb::Folded(r) => {
                            assert_eq!(r.start, next);
                            assert!(r.end > r.start);
                            next = r.end;
                        }
                    }
                }
                assert_eq!(next, path.len());
                assert!(fit.iter().filter(|c| matches!(c, Crumb::Folded(_))).count() <= 1);
            }
        }
    }

    #[test]
    fn squeezing_takes_steps_in_order_and_only_as_far_as_needed() {
        let steps = [Give::UpTo(80.0), Give::All(100.0), Give::All(150.0)];
        assert_eq!(squeeze(0.0, &steps), [0.0, 0.0, 0.0]);
        assert_eq!(squeeze(-40.0, &steps), [0.0, 0.0, 0.0], "room to spare");
        assert_eq!(squeeze(30.0, &steps), [30.0, 0.0, 0.0], "a field shrinks only as needed");
        assert_eq!(squeeze(81.0, &steps), [80.0, 100.0, 0.0], "labels go all at once");
        assert_eq!(squeeze(180.0, &steps), [80.0, 100.0, 0.0]);
        assert_eq!(squeeze(181.0, &steps), [80.0, 100.0, 150.0]);
        assert_eq!(squeeze(1000.0, &steps), [80.0, 100.0, 150.0], "not enough: everything is taken");
        assert_eq!(squeeze(10.0, &[Give::UpTo(-5.0), Give::All(-1.0)]), [0.0, 0.0]);
    }
}
