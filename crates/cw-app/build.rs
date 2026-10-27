//! Gives the Windows executable its icon (the window title bar and the taskbar get theirs at
//! run time, on every platform).

fn main() {
    println!("cargo:rerun-if-changed=assets/app.rc");
    println!("cargo:rerun-if-changed=assets/icon.ico");
    if std::env::var("CARGO_CFG_TARGET_OS").as_deref() == Ok("windows") {
        // an unavailable resource compiler leaves the exe without an icon; it does not fail the build
        let _ = embed_resource::compile("assets/app.rc", embed_resource::NONE).manifest_optional();
    }
}
