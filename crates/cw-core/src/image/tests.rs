use super::*;

fn gradient(w: usize, h: usize) -> Vec<u8> {
    (0..w * h)
        .flat_map(|i| {
            let (x, y) = (i % w, i / w);
            [(x % 251) as u8, (y % 241) as u8, ((x + y) % 239) as u8, 255]
        })
        .collect()
}

#[test]
fn round_trips_odd_sizes() {
    for (w, h) in [(1, 1), (7, 3), (128, 128), (129, 127), (300, 200), (1, 500)] {
        let src = gradient(w, h);
        let img = TiledImage::from_rgba(w, h, &src);
        assert_eq!(img.to_rgba(), src, "{w}x{h}");
        assert_eq!(img.tile_count(), w.div_ceil(TILE) * h.div_ceil(TILE));
    }
}

#[test]
fn tile_rects_cover_the_image_exactly_once() {
    let img = TiledImage::from_rgba(300, 200, &gradient(300, 200));
    let mut covered = vec![0u8; 300 * 200];
    for i in 0..img.tile_count() {
        let (x, y, w, h) = img.tile_rect(i);
        assert!(w <= TILE && h <= TILE && x + w <= 300 && y + h <= 200);
        for yy in y..y + h {
            for xx in x..x + w {
                covered[yy * 300 + xx] += 1;
            }
        }
    }
    assert!(covered.iter().all(|&c| c == 1));
}

#[test]
fn cloning_shares_every_tile() {
    let a = TiledImage::from_rgba(300, 200, &gradient(300, 200));
    let b = a.clone();
    assert_eq!(a.shared_tiles(&b), a.tile_count());
}

#[test]
fn a_filter_that_changes_nothing_allocates_nothing() {
    let a = TiledImage::from_rgba(256, 256, &gradient(256, 256)); // already opaque
    let rgb = a.apply(&channel_filter(Channels::Rgb));
    assert_eq!(a.shared_tiles(&rgb), a.tile_count(), "forcing alpha to 255 on opaque pixels changes no tile");
    assert_eq!(rgb.to_rgba(), a.to_rgba());
}

#[test]
fn filters_share_the_tiles_they_do_not_touch() {
    // translucent only in the top-left tile
    let (w, h) = (256, 256);
    let mut px = gradient(w, h);
    for y in 0..TILE {
        for x in 0..TILE {
            px[(y * w + x) * 4 + 3] = 100;
        }
    }
    let a = TiledImage::from_rgba(w, h, &px);
    let opaque = a.apply(&channel_filter(Channels::Rgb));
    assert!(!a.shares_tile(&opaque, 0), "the translucent tile changed");
    assert_eq!(a.shared_tiles(&opaque), a.tile_count() - 1, "the other three tiles are shared, not copied");
    assert!(opaque.to_rgba().chunks(4).all(|p| p[3] == 255));
}

#[test]
fn channel_views_are_correct() {
    let src = vec![10, 20, 30, 40, 200, 150, 100, 0];
    let img = TiledImage::from_rgba(2, 1, &src);
    let view = |c| img.apply(&channel_filter(c)).to_rgba();
    assert_eq!(view(Channels::Rgba), src);
    assert_eq!(view(Channels::Rgb), [10, 20, 30, 255, 200, 150, 100, 255]);
    assert_eq!(view(Channels::Red), [10, 10, 10, 255, 200, 200, 200, 255]);
    assert_eq!(view(Channels::Green), [20, 20, 20, 255, 150, 150, 150, 255]);
    assert_eq!(view(Channels::Blue), [30, 30, 30, 255, 100, 100, 100, 255]);
    assert_eq!(view(Channels::Alpha), [40, 40, 40, 255, 0, 0, 0, 255]);
}

#[test]
fn invert_keeps_alpha_and_is_an_involution() {
    let img = TiledImage::from_rgba(2, 1, &[0, 100, 255, 77, 1, 2, 3, 4]);
    let once = img.apply(&invert_filter);
    assert_eq!(once.to_rgba(), [255, 155, 0, 77, 254, 253, 252, 4]);
    assert_eq!(once.apply(&invert_filter).to_rgba(), img.to_rgba());
}

#[test]
fn editing_one_tile_leaves_history_untouched() {
    let before = TiledImage::from_rgba(256, 256, &gradient(256, 256));
    let mut after = before.clone();
    after.edit_tile(3, |t| t[0] = t[0].wrapping_add(1));
    assert_eq!(before.shared_tiles(&after), before.tile_count() - 1);
    assert!(!before.shares_tile(&after, 3));
    assert_ne!(before.tile_data(3)[0], after.tile_data(3)[0]);
    // an edit that changes nothing keeps sharing
    let mut same = before.clone();
    same.edit_tile(0, |_| {});
    assert!(same.shares_tile(&before, 0));
}

#[test]
fn parallel_filtering_matches_a_serial_reference() {
    let (w, h) = (700, 333);
    let src = gradient(w, h);
    let img = TiledImage::from_rgba(w, h, &src);
    let got = img.apply(&channel_filter(Channels::Green)).to_rgba();
    let want: Vec<u8> = src.chunks(4).flat_map(|p| [p[1], p[1], p[1], 255]).collect();
    assert_eq!(got, want);
}

#[test]
fn pixels_can_be_read_back_one_at_a_time() {
    let (w, h) = (300, 200);
    let src = gradient(w, h);
    let img = TiledImage::from_rgba(w, h, &src);
    for (x, y) in [(0, 0), (127, 127), (128, 0), (299, 199), (200, 150)] {
        let i = (y * w + x) * 4;
        assert_eq!(img.pixel(x, y), [src[i], src[i + 1], src[i + 2], src[i + 3]], "({x}, {y})");
    }
    assert_eq!(img.pixel(300, 0), [0; 4]);
    assert_eq!(img.pixel(0, 200), [0; 4]);
}

#[test]
fn png_output_decodes_back_to_the_same_pixels() {
    for (w, h) in [(1, 1), (3, 2), (300, 7)] {
        let src = gradient(w, h);
        let png = TiledImage::from_rgba(w, h, &src).to_png();
        assert_eq!(&png[..8], b"\x89PNG\r\n\x1a\n");
        let back = ::image::load_from_memory_with_format(&png, ::image::ImageFormat::Png).unwrap().to_rgba8();
        assert_eq!((back.width() as usize, back.height() as usize), (w, h));
        assert_eq!(back.into_raw(), src, "{w}x{h}");
    }
}

#[test]
fn channel_names_round_trip_and_accept_long_forms() {
    for c in Channels::ALL {
        assert_eq!(Channels::parse(c.label()), Some(c));
    }
    assert_eq!(Channels::parse(" Green "), Some(Channels::Green));
    assert_eq!(Channels::parse("ALPHA"), Some(Channels::Alpha));
    assert_eq!(Channels::parse("purple"), None);
}
