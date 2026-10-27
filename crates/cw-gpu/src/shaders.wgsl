// Colours are written in gamma space: the target is Rgba8Unorm and egui
// composes it without a colour-space conversion.

struct Globals {
    view_proj: mat4x4<f32>,
    camera: vec4<f32>,   // xyz position
    grid: vec4<f32>,     // x spacing, y half-extent
};
@group(0) @binding(0) var<uniform> g: Globals;

// ---------------------------------------------------------------- boxes

struct BoxIn {
    @location(0) pos: vec3<f32>,
    @location(1) normal: vec3<f32>,
    @location(2) m0: vec4<f32>,
    @location(3) m1: vec4<f32>,
    @location(4) m2: vec4<f32>,
    @location(5) m3: vec4<f32>,
    @location(6) color: vec4<f32>,
};
struct BoxOut {
    @builtin(position) clip: vec4<f32>,
    @location(0) normal: vec3<f32>,
    @location(1) color: vec4<f32>,
};

@vertex
fn vs_box(i: BoxIn) -> BoxOut {
    let model = mat4x4<f32>(i.m0, i.m1, i.m2, i.m3);
    var o: BoxOut;
    o.clip = g.view_proj * (model * vec4<f32>(i.pos, 1.0));
    o.normal = normalize((model * vec4<f32>(i.normal, 0.0)).xyz);
    o.color = i.color;
    return o;
}

@fragment
fn fs_box(i: BoxOut) -> @location(0) vec4<f32> {
    let light = normalize(vec3<f32>(0.4, 0.3, 0.85));
    let ndl = max(dot(normalize(i.normal), light), 0.0);
    return vec4<f32>(i.color.rgb * (0.36 + 0.64 * ndl), 1.0);
}

// ---------------------------------------------------------------- lines

struct LineIn {
    @location(0) pos: vec3<f32>,
    @location(1) color: vec4<f32>,
};
struct LineOut {
    @builtin(position) clip: vec4<f32>,
    @location(0) color: vec4<f32>,
};

@vertex
fn vs_line(i: LineIn) -> LineOut {
    var o: LineOut;
    o.clip = g.view_proj * vec4<f32>(i.pos, 1.0);
    o.color = i.color;
    return o;
}

@fragment
fn fs_line(i: LineOut) -> @location(0) vec4<f32> {
    return i.color;
}

// ----------------------------------------------------------------- grid

struct GridOut {
    @builtin(position) clip: vec4<f32>,
    @location(0) world: vec2<f32>,
};

@vertex
fn vs_grid(@builtin(vertex_index) vi: u32) -> GridOut {
    var corners = array<vec2<f32>, 6>(
        vec2<f32>(-1.0, -1.0), vec2<f32>(1.0, -1.0), vec2<f32>(1.0, 1.0),
        vec2<f32>(-1.0, -1.0), vec2<f32>(1.0, 1.0), vec2<f32>(-1.0, 1.0));
    let xy = g.camera.xy + corners[vi] * g.grid.y;
    var o: GridOut;
    o.clip = g.view_proj * vec4<f32>(xy, 0.0, 1.0);
    o.world = xy;
    return o;
}

fn line_mask(coord: vec2<f32>) -> f32 {
    let w = fwidth(coord);
    let d = abs(fract(coord - 0.5) - 0.5) / max(w, vec2<f32>(1e-6));
    return 1.0 - min(min(d.x, d.y), 1.0);
}

@fragment
fn fs_grid(i: GridOut) -> @location(0) vec4<f32> {
    let spacing = g.grid.x;
    let minor = line_mask(i.world / spacing);
    let major = line_mask(i.world / (spacing * 10.0));
    let dist = distance(i.world, g.camera.xy);
    let fade = 1.0 - smoothstep(g.grid.y * 0.25, g.grid.y * 0.95, dist);

    // world axes: X red, Y green
    let aw = fwidth(i.world);
    let on_x = 1.0 - min(abs(i.world.y) / max(aw.y, 1e-6), 1.0);
    let on_y = 1.0 - min(abs(i.world.x) / max(aw.x, 1e-6), 1.0);

    var rgb = vec3<f32>(0.30, 0.30, 0.34);
    var a = minor * 0.30 + major * 0.35;
    rgb = mix(rgb, vec3<f32>(0.85, 0.35, 0.38), on_x);
    rgb = mix(rgb, vec3<f32>(0.35, 0.78, 0.50), on_y);
    a = max(a, max(on_x, on_y) * 0.9);
    return vec4<f32>(rgb, a * fade);
}
