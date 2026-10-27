// Draws one image into the target with pan and zoom, over a checkerboard
// that shows through transparent pixels. Image bytes are already display
// colours, so nothing here converts between colour spaces.

struct Params {
    target_size: vec2<f32>,
    image_size: vec2<f32>,
    // top-left corner of the image, in target pixels
    offset: vec2<f32>,
    zoom: f32,
    pad: f32,
    ground: vec4<f32>,
    check_a: vec4<f32>,
    check_b: vec4<f32>,
    check_px: f32,
    pad0: f32,
    pad1: f32,
    pad2: f32,
};

@group(0) @binding(0) var<uniform> p: Params;
@group(0) @binding(1) var img: texture_2d<f32>;
// nearest or bilinear, chosen by the host: one texture cannot be read through two samplers
@group(0) @binding(2) var smp: sampler;

struct VsOut {
    @builtin(position) pos: vec4<f32>,
};

// one oversized triangle covers the target
@vertex
fn vs(@builtin(vertex_index) i: u32) -> VsOut {
    let x = f32((i << 1u) & 2u);
    let y = f32(i & 2u);
    var o: VsOut;
    o.pos = vec4<f32>(x * 2.0 - 1.0, 1.0 - y * 2.0, 0.0, 1.0);
    return o;
}

@fragment
fn fs(in: VsOut) -> @location(0) vec4<f32> {
    let px = in.pos.xy;
    let ip = (px - p.offset) / p.zoom;
    if (ip.x < 0.0 || ip.y < 0.0 || ip.x >= p.image_size.x || ip.y >= p.image_size.y) {
        return p.ground;
    }
    let uv = ip / p.image_size;
    let c = textureSampleLevel(img, smp, uv, 0.0);
    let cell = floor(px / p.check_px);
    let odd = ((i32(cell.x) + i32(cell.y)) & 1) == 1;
    let chk = select(p.check_a, p.check_b, odd);
    return vec4<f32>(mix(chk.rgb, c.rgb, c.a), 1.0);
}
