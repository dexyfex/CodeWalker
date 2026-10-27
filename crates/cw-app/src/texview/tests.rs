use cw_core::texture::{Format, chain_size};

use super::*;
use crate::preview::TexEntry;

fn texture(w: u32, h: u32, levels: u32) -> Texture {
    let n = chain_size(Format::A8R8G8B8, w, h, levels);
    Texture { width: w, height: h, levels, format: Format::A8R8G8B8, data: (0..n).map(|i| i as u8).collect() }
}

fn set(textures: &[Texture]) -> TexSet {
    TexSet {
        entries: textures
            .iter()
            .enumerate()
            .map(|(i, t)| TexEntry { name: format!("t{i}"), texture: Arc::new(t.clone()), thumb: None })
            .collect(),
    }
}

#[test]
fn a_decode_is_asked_for_once_per_change() {
    let s = set(&[texture(8, 8, 3), texture(4, 4, 1)]);
    let mut v = TexView::default();
    v.reset(2);
    let first = v.next_job(&s, None).expect("nothing decoded yet");
    assert_eq!((first.level, first.texture.width), (0, 8));
    assert!(v.next_job(&s, None).is_none(), "same texture and mip: nothing new to ask");
    assert!(v.is_decoding());

    v.set_level(2);
    let job = v.next_job(&s, None).unwrap();
    assert_eq!((job.level, job.key), (2, first.key + 1));
    v.select(1);
    assert_eq!((v.level, v.fit), (0, true), "another texture starts at its largest mip");
    assert_eq!(v.next_job(&s, None).unwrap().texture.width, 4);

    // asking for a mip the texture does not have clamps to its last one: one decode, not two
    v.select(0);
    assert_eq!(v.next_job(&s, None).unwrap().level, 0);
    v.set_level(9);
    assert_eq!(v.next_job(&s, None).unwrap().level, 2);
    v.set_level(8);
    assert!(v.next_job(&s, None).is_none(), "clamps to the mip that is already shown");
    // and an index past the end asks for nothing
    v.index = 5;
    assert!(v.next_job(&s, None).is_none());
}

#[test]
fn only_the_latest_answer_is_kept() {
    let s = set(&[texture(8, 8, 1), texture(4, 4, 1)]);
    let mut v = TexView::default();
    v.reset(2);
    let slow = v.next_job(&s, None).unwrap();
    v.select(1);
    let fast = v.next_job(&s, None).unwrap();

    v.decoded(fast.key, fast.run());
    assert_eq!(v.size(), Some((4, 4)));
    assert!(!v.is_decoding());
    v.decoded(slow.key, slow.run()); // the earlier request finishes late
    assert_eq!(v.size(), Some((4, 4)), "a stale answer does not replace the newer picture");

    let bad = {
        v.index = 0;
        v.level = 0;
        v.next_job(&s, None).unwrap()
    };
    v.decoded(bad.key, Err("no such format".into()));
    assert_eq!(v.error.as_deref(), Some("no such format"));
    assert!(v.size().is_none() && v.shown().is_none() && !v.is_decoding());
}

#[test]
fn filters_follow_the_settings_and_share_what_they_leave_alone() {
    let s = set(&[texture(300, 200, 1)]);
    let mut v = TexView::default();
    v.reset(1);
    let job = v.next_job(&s, None).unwrap();
    v.decoded(job.key, job.run());
    let base_px = v.pixel(5, 7).unwrap();
    assert_eq!(v.pixel(300, 0), None);

    v.ensure_filtered();
    assert_eq!(v.shown().unwrap().pixel(5, 7), base_px, "no filter: the picture as decoded");
    let tiles = v.shown().unwrap().tile_count();
    assert_eq!(v.shown().unwrap().shared_tiles(v.base.as_ref().unwrap()), tiles, "and not even copied");

    v.channels = Channels::Green;
    v.ensure_filtered();
    assert_eq!(v.shown().unwrap().pixel(5, 7), [base_px[1], base_px[1], base_px[1], 255]);

    v.invert = true;
    v.ensure_filtered();
    let g = 255 - base_px[1];
    assert_eq!(v.shown().unwrap().pixel(5, 7), [g, g, g, 255]);
    assert_eq!(v.pixel(5, 7).unwrap(), base_px, "the source stays unfiltered");

    v.channels = Channels::Rgba;
    v.invert = false;
    v.ensure_filtered();
    assert_eq!(v.shown().unwrap().pixel(5, 7), base_px, "settings can be taken back");
}

#[test]
fn opening_another_file_starts_clean() {
    let s = set(&[texture(8, 8, 1)]);
    let mut v = TexView::default();
    v.reset(1);
    v.channels = Channels::Alpha;
    v.invert = true;
    v.fit = false;
    let job = v.next_job(&s, None).unwrap();
    v.decoded(job.key, job.run());

    v.reset(3);
    assert_eq!((v.index, v.level, v.channels, v.invert, v.fit), (0, 0, Channels::Rgba, false, true));
    assert_eq!(v.thumbs.len(), 3);
    assert!(v.shown().is_none() && v.size().is_none());
    assert!(v.next_job(&s, None).is_some(), "the first texture of the new file is decoded again");
}

#[test]
fn the_screenshot_hook_sets_what_it_names_and_ignores_the_rest() {
    let mut v = TexView::default();
    v.apply_settings("index=3, level=2,channels=alpha,invert=1,bogus=9,nonsense,level=x");
    assert_eq!((v.index, v.level, v.channels, v.invert), (3, 2, Channels::Alpha, true));
    v.apply_settings("invert=0,channels=purple");
    assert_eq!((v.invert, v.channels), (false, Channels::Alpha), "bad values change nothing");

    // "actual" is used up by the first picture that arrives
    let s = set(&[texture(8, 8, 1)]);
    let mut v = TexView::default();
    v.reset(1);
    v.apply_settings("actual=1");
    let job = v.next_job(&s, None).unwrap();
    v.decoded(job.key, job.run());
    assert!(!v.fit && v.view.zoom == 1.0, "opens at 1:1");
    assert!(!v.start_at_actual, "and only that once");
}

#[test]
fn a_mip_bigger_than_the_limit_is_shown_shrunk_and_says_so() {
    let s = set(&[texture(64, 32, 1)]);
    let mut v = TexView::default();
    v.reset(1);
    let job = v.next_job(&s, Some(16)).unwrap();
    assert_eq!(job.max_side, 16);
    v.decoded(job.key, job.run());
    assert_eq!((v.size(), v.scale()), (Some((16, 8)), 4));
    assert_eq!(v.base.as_ref().unwrap().to_rgba().len(), 16 * 8 * 4);

    // 1:1 means the real mip's pixels, so the shrunk copy is magnified by its scale
    let actual = v.actual_view().unwrap();
    assert_eq!(actual.zoom, 4.0);

    // a picture that fits is untouched
    v.index = 0;
    v.level = 0;
    v.requested = None;
    let job = v.next_job(&s, Some(64)).unwrap();
    v.decoded(job.key, job.run());
    assert_eq!((v.size(), v.scale()), (Some((64, 32)), 1));
    assert_eq!(v.actual_view().unwrap().zoom, 1.0);
}

#[test]
fn the_device_limit_can_only_make_the_picture_smaller() {
    let s = set(&[texture(8, 8, 1)]);
    let mut v = TexView::default();
    v.reset(1);
    assert_eq!(v.next_job(&s, None).unwrap().max_side, MAX_VIEW_SIDE);
    v.requested = None;
    assert_eq!(v.next_job(&s, Some(2048)).unwrap().max_side, 2048);
    v.requested = None;
    assert_eq!(v.next_job(&s, Some(u32::MAX)).unwrap().max_side, MAX_VIEW_SIDE, "never above our own cap");
}

#[test]
fn a_mip_too_big_to_decode_is_refused_before_any_memory_is_spent() {
    // the pixel data is not even there: the size alone is enough to say no
    let huge =
        Texture { width: 20_000, height: 20_000, levels: 1, format: Format::A8R8G8B8, data: Vec::new() };
    let s = set(&[huge]);
    let mut v = TexView::default();
    v.reset(1);
    let job = v.next_job(&s, None).unwrap();
    let err = job.run().err().unwrap();
    assert!(err.contains("20000") && err.contains("too large"), "{err}");
    v.decoded(job.key, Err(err));
    assert!(v.error.is_some() && v.size().is_none());
}

#[test]
fn the_decoding_flag_covers_a_change_of_choice_not_just_the_first_picture() {
    let s = set(&[texture(8, 8, 1), texture(4, 4, 1)]);
    let mut v = TexView::default();
    v.reset(2);
    assert!(!v.is_decoding(), "nothing asked yet");
    let first = v.next_job(&s, None).unwrap();
    assert!(v.is_decoding());
    v.decoded(first.key, first.run());
    assert!(!v.is_decoding() && v.size() == Some((8, 8)));

    v.select(1);
    let second = v.next_job(&s, None).unwrap();
    assert!(v.is_decoding(), "the 8x8 on screen is the old choice, and the view must say so");
    assert_eq!(v.size(), Some((8, 8)), "which stays up meanwhile, instead of flashing empty");
    v.decoded(second.key, second.run());
    assert!(!v.is_decoding() && v.size() == Some((4, 4)));

    // a failed answer ends the wait too
    v.select(0);
    let third = v.next_job(&s, None).unwrap();
    v.decoded(third.key, Err("nope".into()));
    assert!(!v.is_decoding());
}

#[test]
fn an_answer_for_the_previous_file_cannot_pass_for_the_new_one() {
    let (old, new) = (set(&[texture(8, 8, 1)]), set(&[texture(4, 4, 1)]));
    let mut v = TexView::default();
    v.reset(1);
    let late = v.next_job(&old, None).unwrap();

    v.reset(1);
    let current = v.next_job(&new, None).unwrap();
    assert_ne!(late.key, current.key, "keys keep counting across files");
    v.decoded(late.key, late.run());
    assert!(v.size().is_none() && v.is_decoding(), "the old file's picture is ignored");
    v.decoded(current.key, current.run());
    assert_eq!(v.size(), Some((4, 4)));
}

#[test]
fn leaving_a_texture_file_frees_the_pictures_and_drops_the_answer_on_its_way() {
    let s = set(&[texture(8, 8, 1)]);
    let mut v = TexView::default();
    v.reset(1);
    let done = v.next_job(&s, None).unwrap();
    v.decoded(done.key, done.run());
    assert!(v.size().is_some() && v.shown().is_some());

    v.release();
    assert!(v.size().is_none() && v.shown().is_none() && v.thumbs.is_empty() && !v.is_decoding());

    // a decode still running when the preview moved on lands on nothing
    v.reset(1);
    let late = v.next_job(&s, None).unwrap();
    v.release();
    v.decoded(late.key, late.run());
    assert!(v.size().is_none(), "released, so the late picture is dropped");

    v.release(); // and doing it twice is harmless
}
