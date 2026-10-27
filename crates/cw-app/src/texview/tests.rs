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
    let first = v.next_job(&s).expect("nothing decoded yet");
    assert_eq!((first.level, first.texture.width), (0, 8));
    assert!(v.next_job(&s).is_none(), "same texture and mip: nothing new to ask");
    assert!(v.is_decoding());

    v.set_level(2);
    let job = v.next_job(&s).unwrap();
    assert_eq!((job.level, job.key), (2, first.key + 1));
    v.select(1);
    assert_eq!((v.level, v.fit), (0, true), "another texture starts at its largest mip");
    assert_eq!(v.next_job(&s).unwrap().texture.width, 4);

    // asking for a mip the texture does not have clamps to its last one: one decode, not two
    v.select(0);
    assert_eq!(v.next_job(&s).unwrap().level, 0);
    v.set_level(9);
    assert_eq!(v.next_job(&s).unwrap().level, 2);
    v.set_level(8);
    assert!(v.next_job(&s).is_none(), "clamps to the mip that is already shown");
    // and an index past the end asks for nothing
    v.index = 5;
    assert!(v.next_job(&s).is_none());
}

#[test]
fn only_the_latest_answer_is_kept() {
    let s = set(&[texture(8, 8, 1), texture(4, 4, 1)]);
    let mut v = TexView::default();
    v.reset(2);
    let slow = v.next_job(&s).unwrap();
    v.select(1);
    let fast = v.next_job(&s).unwrap();

    v.decoded(fast.key, fast.run());
    assert_eq!(v.size(), Some((4, 4)));
    assert!(!v.is_decoding());
    v.decoded(slow.key, slow.run()); // the earlier request finishes late
    assert_eq!(v.size(), Some((4, 4)), "a stale answer does not replace the newer picture");

    let bad = {
        v.index = 0;
        v.level = 0;
        v.next_job(&s).unwrap()
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
    let job = v.next_job(&s).unwrap();
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
    let job = v.next_job(&s).unwrap();
    v.decoded(job.key, job.run());

    v.reset(3);
    assert_eq!((v.index, v.level, v.channels, v.invert, v.fit), (0, 0, Channels::Rgba, false, true));
    assert_eq!(v.thumbs.len(), 3);
    assert!(v.shown().is_none() && v.size().is_none());
    assert!(v.next_job(&s).is_some(), "the first texture of the new file is decoded again");
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
    let job = v.next_job(&s).unwrap();
    v.decoded(job.key, job.run());
    assert!(!v.fit && v.view.zoom == 1.0, "opens at 1:1");
    assert!(!v.start_at_actual, "and only that once");
}
