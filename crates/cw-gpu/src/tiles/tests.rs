use super::*;

fn gradient(w: usize, h: usize) -> Vec<u8> {
    (0..w * h)
        .flat_map(|i| {
            let (x, y) = (i % w, i / w);
            [(x % 251) as u8, (y % 241) as u8, ((x + y) % 239) as u8, 255]
        })
        .collect()
}

/// Needs a GPU/software adapter; skipped where none exists.
fn device() -> Option<(wgpu::Device, wgpu::Queue)> {
    let instance = wgpu::Instance::new(wgpu::InstanceDescriptor::new_without_display_handle());
    let adapter =
        pollster::block_on(instance.request_adapter(&wgpu::RequestAdapterOptions::default())).ok()?;
    pollster::block_on(adapter.request_device(&wgpu::DeviceDescriptor::default())).ok()
}

#[test]
fn gpu_upload_sends_only_changed_tiles_and_matches_the_cpu_image() {
    let Some((device, queue)) = device() else {
        eprintln!("no wgpu adapter available; skipping GPU test");
        return;
    };
    let (w, h) = (300, 200); // 3x2 tiles with ragged edges
    let a = TiledImage::from_rgba(w, h, &gradient(w, h));
    let mut tex = TileTexture::new(&device, w, h);
    assert_eq!(tex.sync(&queue, &a), a.tile_count(), "first sync uploads everything");
    assert_eq!(tex.read_back(&device, &queue), a.to_rgba(), "GPU contents equal the CPU image");
    assert_eq!(tex.sync(&queue, &a), 0, "nothing changed, nothing sent");

    let mut b = a.clone();
    b.edit_tile(4, |t| t.iter_mut().for_each(|v| *v = 255 - *v));
    assert_eq!(tex.sync(&queue, &b), 1, "exactly the edited tile is uploaded");
    assert_eq!(tex.read_back(&device, &queue), b.to_rgba());

    // a whole-image filter that touches every tile
    let g = b.apply(&channel_filter(Channels::Red));
    let sent = tex.sync(&queue, &g);
    assert!(sent >= 5 && sent <= g.tile_count());
    assert_eq!(tex.read_back(&device, &queue), g.to_rgba());
}
