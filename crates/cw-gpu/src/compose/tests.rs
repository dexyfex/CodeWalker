use super::*;
use crate::tiles::TiledImage;

const VP: (f32, f32) = (400.0, 300.0);

#[test]
fn fit_shows_everything_and_never_enlarges_small_images() {
    let v = ImageView::fit((800, 600), VP);
    assert_eq!(v.zoom, 0.5);
    assert_eq!(v.offset(VP), (0.0, 0.0), "fitted exactly");
    let small = ImageView::fit((16, 16), VP);
    assert_eq!(small.zoom, 1.0, "not blown up past 1:1");
    assert_eq!(small.offset(VP), (192.0, 142.0), "and centred");
    let tall = ImageView::fit((100, 6000), VP);
    assert!((tall.zoom - 0.05).abs() < 1e-6);
    assert_eq!(ImageView::fit((0, 0), VP).zoom, 1.0, "an empty image does not divide by zero");
}

#[test]
fn zooming_keeps_the_pixel_under_the_cursor_still() {
    let mut v = ImageView::fit((800, 600), VP);
    let at = (120.0, 80.0);
    let before = v.to_image(at, VP);
    for factor in [2.0, 2.0, 1.5, 0.25, 8.0] {
        v.zoom_at(at, VP, factor);
        let now = v.to_image(at, VP);
        assert!((now.0 - before.0).abs() < 1e-3 && (now.1 - before.1).abs() < 1e-3, "{now:?} vs {before:?}");
    }
    v.zoom_at(at, VP, 1e9);
    assert_eq!(v.zoom, MAX_ZOOM);
    v.zoom_at(at, VP, 1e-12);
    assert_eq!(v.zoom, MIN_ZOOM);
}

#[test]
fn panning_moves_the_image_with_the_pointer() {
    let mut v = ImageView { zoom: 2.0, center: (100.0, 100.0) };
    let o = v.offset(VP);
    v.pan(30.0, -10.0);
    let n = v.offset(VP);
    assert_eq!((n.0 - o.0, n.1 - o.1), (30.0, -10.0), "dragging right moves the picture right");
    v.pan(1e6, 1e6);
    v.clamp((256, 256));
    assert_eq!(v.center, (0.0, 0.0), "cannot be dragged out of reach");
}

#[test]
fn pixel_at_reports_the_pixel_or_nothing() {
    let v = ImageView { zoom: 4.0, center: (2.0, 2.0) }; // a 4x4 image drawn 16x16 in the middle
    let (cx, cy) = (VP.0 / 2.0, VP.1 / 2.0);
    assert_eq!(v.pixel_at((cx - 7.0, cy - 7.0), VP, (4, 4)), Some((0, 0)));
    assert_eq!(v.pixel_at((cx + 7.0, cy + 7.0), VP, (4, 4)), Some((3, 3)));
    assert_eq!(v.pixel_at((cx + 9.0, cy), VP, (4, 4)), None);
    assert_eq!(v.pixel_at((cx - 8.5, cy), VP, (4, 4)), None);
}

fn device() -> Option<(wgpu::Device, wgpu::Queue)> {
    let instance = wgpu::Instance::new(wgpu::InstanceDescriptor::new_without_display_handle());
    let adapter =
        pollster::block_on(instance.request_adapter(&wgpu::RequestAdapterOptions::default())).ok()?;
    pollster::block_on(adapter.request_device(&wgpu::DeviceDescriptor::default())).ok()
}

fn px(data: &[u8], width: u32, x: u32, y: u32) -> [u8; 4] {
    let i = ((y * width + x) * 4) as usize;
    [data[i], data[i + 1], data[i + 2], data[i + 3]]
}

fn near(a: [u8; 4], b: [u8; 4]) -> bool {
    a.iter().zip(b).all(|(x, y)| x.abs_diff(y) <= 1)
}

#[test]
fn the_compositor_places_pixels_checkers_and_ground_where_the_view_says() {
    let Some((device, queue)) = device() else {
        eprintln!("no wgpu adapter available; skipping GPU test");
        return;
    };
    // red | green over blue | fully transparent
    let rgba = [255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 9, 9, 9, 0];
    let image = TiledImage::from_rgba(2, 2, &rgba);
    let mut tex = TileTexture::new(&device, 2, 2);
    tex.sync(&queue, &image);

    let target = ComposeTarget::new(&device, 8, 8);
    let comp = Compositor::new(&device);
    let (ground, a, b) = ([0.2, 0.4, 0.6, 1.0], [1.0, 1.0, 1.0, 1.0], [0.0, 0.0, 0.0, 1.0]);
    let mut params = ViewParams {
        view: ImageView { zoom: 2.0, center: (1.0, 1.0) }, // the 2x2 image becomes 4x4, in the middle
        smooth: false,
        ground,
        checker: (a, b),
        checker_px: 1.0,
    };
    comp.render(&device, &queue, &target, &tex, &params);
    let out = read_target(&device, &queue, &target);
    assert_eq!(out.len(), 8 * 8 * 4);

    assert!(
        near(px(&out, 8, 0, 0), [51, 102, 153, 255]),
        "outside the image: ground, got {:?}",
        px(&out, 8, 0, 0)
    );
    assert!(near(px(&out, 8, 7, 7), [51, 102, 153, 255]));
    assert_eq!(px(&out, 8, 2, 2), [255, 0, 0, 255], "red texel, top-left");
    assert_eq!(px(&out, 8, 5, 2), [0, 255, 0, 255], "green texel, top-right");
    assert_eq!(px(&out, 8, 2, 5), [0, 0, 255, 255], "blue texel, bottom-left");
    // the transparent texel shows the checker: cell (4,4) is even => colour a
    assert!(near(px(&out, 8, 4, 4), [255, 255, 255, 255]), "transparent texel, got {:?}", px(&out, 8, 4, 4));
    assert!(near(px(&out, 8, 5, 4), [0, 0, 0, 255]), "the next checker cell is the other colour");
    assert_eq!(px(&out, 8, 3, 3), [255, 0, 0, 255], "nearest keeps texel edges hard");

    // smooth filtering blends across the red/green edge instead
    params.smooth = true;
    comp.render(&device, &queue, &target, &tex, &params);
    let out = read_target(&device, &queue, &target);
    let edge = px(&out, 8, 3, 2);
    assert!(edge[0] < 255 && edge[1] > 0, "blended, got {edge:?}");

    // panning changes where it lands, not what it is
    params.smooth = false;
    params.view.center = (1.0 + 1.0, 1.0); // look one pixel to the right: the picture moves left by 2
    comp.render(&device, &queue, &target, &tex, &params);
    let out = read_target(&device, &queue, &target);
    assert_eq!(px(&out, 8, 0, 2), [255, 0, 0, 255]);
    assert_eq!(px(&out, 8, 3, 2), [0, 255, 0, 255]);
}
