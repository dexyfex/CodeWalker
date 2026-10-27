//! Outline icons drawn with the painter on a 16x16 grid. One stroke weight,
//! no fills, no emoji: icons stay grey until active (see docs/DESIGN.md).

use cw_core::filekind::FileKind;
use eframe::egui::{Color32, Painter, Pos2, Rect, Shape, Stroke, pos2, vec2};

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Icon {
    Folder,
    Archive,
    File,
    Image,
    Textures,
    Cube,
    Map,
    Text,
    Audio,
    Code,
    ChevronRight,
    ChevronDown,
    Back,
    Forward,
    Up,
    Search,
    Close,
    Layers,
    FolderOpen,
    Extract,
    Copy,
    Plus,
    Check,
    Alert,
    Undo,
    Redo,
    Frame,
}

pub fn for_kind(kind: FileKind) -> Icon {
    match kind {
        FileKind::Rpf => Icon::Archive,
        FileKind::TextureDict => Icon::Textures,
        FileKind::Drawable
        | FileKind::DrawableDict
        | FileKind::Fragment
        | FileKind::Bounds
        | FileKind::Cloth => Icon::Cube,
        FileKind::MapData
        | FileKind::MapTypes
        | FileKind::Paths
        | FileKind::NavMesh
        | FileKind::Waypoints => Icon::Map,
        FileKind::Audio => Icon::Audio,
        FileKind::Script => Icon::Code,
        FileKind::Xml | FileKind::Text | FileKind::Meta | FileKind::Gxt2 | FileKind::Scenario => Icon::Text,
        FileKind::Image | FileKind::Dds => Icon::Image,
        _ => Icon::File,
    }
}

pub fn draw(p: &Painter, rect: Rect, icon: Icon, color: Color32) {
    let s = rect.width().min(rect.height()) / 16.0;
    let o = rect.center() - vec2(8.0, 8.0) * s;
    let pt = |x: f32, y: f32| pos2(o.x + x * s, o.y + y * s);
    let st = Stroke::new((1.25 * s).max(1.0), color);
    let line = |pts: &[(f32, f32)]| {
        p.add(Shape::line(pts.iter().map(|&(x, y)| pt(x, y)).collect(), st));
    };
    let closed = |pts: &[(f32, f32)]| {
        p.add(Shape::closed_line(pts.iter().map(|&(x, y)| pt(x, y)).collect(), st));
    };
    let ellipse = |cx: f32, cy: f32, rx: f32, ry: f32| {
        let pts: Vec<Pos2> = (0..32)
            .map(|i| {
                let a = i as f32 / 32.0 * std::f32::consts::TAU;
                pt(cx + rx * a.cos(), cy + ry * a.sin())
            })
            .collect();
        p.add(Shape::closed_line(pts, st));
    };

    match icon {
        Icon::Folder => closed(&[(1.5, 4.0), (6.0, 4.0), (7.5, 5.5), (14.5, 5.5), (14.5, 13.0), (1.5, 13.0)]),
        Icon::FolderOpen => {
            closed(&[(1.5, 4.0), (6.0, 4.0), (7.5, 5.5), (13.0, 5.5), (13.0, 7.0)]);
            closed(&[(1.5, 13.0), (3.5, 7.0), (15.0, 7.0), (13.0, 13.0)]);
        }
        Icon::Archive => {
            closed(&[(2.0, 3.0), (14.0, 3.0), (14.0, 13.0), (2.0, 13.0)]);
            line(&[(2.0, 6.5), (14.0, 6.5)]);
            line(&[(6.5, 9.5), (9.5, 9.5)]);
        }
        Icon::File => {
            closed(&[(3.5, 1.5), (9.5, 1.5), (12.5, 4.5), (12.5, 14.5), (3.5, 14.5)]);
            line(&[(9.5, 1.5), (9.5, 4.5), (12.5, 4.5)]);
        }
        Icon::Text => {
            closed(&[(3.5, 1.5), (9.5, 1.5), (12.5, 4.5), (12.5, 14.5), (3.5, 14.5)]);
            line(&[(6.0, 8.0), (10.0, 8.0)]);
            line(&[(6.0, 10.5), (10.0, 10.5)]);
            line(&[(6.0, 13.0), (8.5, 13.0)]);
        }
        Icon::Image => {
            closed(&[(2.0, 3.0), (14.0, 3.0), (14.0, 13.0), (2.0, 13.0)]);
            line(&[(2.5, 11.5), (6.0, 8.0), (8.5, 10.5), (10.5, 8.5), (13.5, 12.0)]);
            ellipse(10.5, 6.0, 1.2, 1.2);
        }
        Icon::Textures => {
            closed(&[(2.0, 2.0), (7.0, 2.0), (7.0, 7.0), (2.0, 7.0)]);
            closed(&[(9.0, 2.0), (14.0, 2.0), (14.0, 7.0), (9.0, 7.0)]);
            closed(&[(2.0, 9.0), (7.0, 9.0), (7.0, 14.0), (2.0, 14.0)]);
            closed(&[(9.0, 9.0), (14.0, 9.0), (14.0, 14.0), (9.0, 14.0)]);
        }
        Icon::Cube => {
            closed(&[(8.0, 1.5), (14.0, 4.8), (14.0, 11.2), (8.0, 14.5), (2.0, 11.2), (2.0, 4.8)]);
            line(&[(2.0, 4.8), (8.0, 8.2), (14.0, 4.8)]);
            line(&[(8.0, 8.2), (8.0, 14.5)]);
        }
        Icon::Map => {
            closed(&[
                (1.5, 3.5),
                (5.5, 2.5),
                (10.5, 4.0),
                (14.5, 3.0),
                (14.5, 12.5),
                (10.5, 13.5),
                (5.5, 12.0),
                (1.5, 13.0),
            ]);
            line(&[(5.5, 2.5), (5.5, 12.0)]);
            line(&[(10.5, 4.0), (10.5, 13.5)]);
        }
        Icon::Audio => {
            closed(&[(2.0, 6.0), (5.0, 6.0), (8.5, 3.0), (8.5, 13.0), (5.0, 10.0), (2.0, 10.0)]);
            line(&[(10.5, 6.0), (11.5, 8.0), (10.5, 10.0)]);
            line(&[(12.5, 4.5), (14.0, 8.0), (12.5, 11.5)]);
        }
        Icon::Code => {
            line(&[(5.5, 4.0), (1.5, 8.0), (5.5, 12.0)]);
            line(&[(10.5, 4.0), (14.5, 8.0), (10.5, 12.0)]);
            line(&[(9.0, 3.0), (7.0, 13.0)]);
        }
        Icon::ChevronRight => line(&[(6.0, 3.5), (10.5, 8.0), (6.0, 12.5)]),
        Icon::ChevronDown => line(&[(3.5, 6.0), (8.0, 10.5), (12.5, 6.0)]),
        Icon::Back => {
            line(&[(13.0, 8.0), (3.0, 8.0)]);
            line(&[(7.5, 3.5), (3.0, 8.0), (7.5, 12.5)]);
        }
        Icon::Forward => {
            line(&[(3.0, 8.0), (13.0, 8.0)]);
            line(&[(8.5, 3.5), (13.0, 8.0), (8.5, 12.5)]);
        }
        Icon::Up => {
            line(&[(8.0, 13.0), (8.0, 3.0)]);
            line(&[(3.5, 7.5), (8.0, 3.0), (12.5, 7.5)]);
        }
        Icon::Search => {
            ellipse(7.0, 7.0, 4.5, 4.5);
            line(&[(10.4, 10.4), (14.0, 14.0)]);
        }
        Icon::Close => {
            line(&[(3.5, 3.5), (12.5, 12.5)]);
            line(&[(12.5, 3.5), (3.5, 12.5)]);
        }
        Icon::Layers => {
            closed(&[(8.0, 2.0), (14.5, 5.5), (8.0, 9.0), (1.5, 5.5)]);
            line(&[(1.5, 8.5), (8.0, 12.0), (14.5, 8.5)]);
            line(&[(1.5, 11.0), (8.0, 14.5), (14.5, 11.0)]);
        }
        Icon::Extract => {
            line(&[(8.0, 2.5), (8.0, 10.5)]);
            line(&[(4.5, 7.0), (8.0, 10.5), (11.5, 7.0)]);
            line(&[(2.5, 13.5), (13.5, 13.5)]);
        }
        Icon::Copy => {
            closed(&[(5.5, 5.5), (13.5, 5.5), (13.5, 13.5), (5.5, 13.5)]);
            line(&[(2.5, 10.5), (2.5, 2.5), (10.5, 2.5)]);
        }
        Icon::Plus => {
            line(&[(8.0, 3.0), (8.0, 13.0)]);
            line(&[(3.0, 8.0), (13.0, 8.0)]);
        }
        Icon::Check => line(&[(3.0, 8.5), (6.5, 12.0), (13.0, 4.5)]),
        Icon::Undo => {
            line(&[(5.0, 3.5), (2.5, 6.5), (5.0, 9.5)]);
            line(&[(2.5, 6.5), (9.5, 6.5), (13.0, 8.5), (13.0, 12.5)]);
        }
        Icon::Redo => {
            line(&[(11.0, 3.5), (13.5, 6.5), (11.0, 9.5)]);
            line(&[(13.5, 6.5), (6.5, 6.5), (3.0, 8.5), (3.0, 12.5)]);
        }
        Icon::Frame => {
            line(&[(2.0, 5.5), (2.0, 2.0), (5.5, 2.0)]);
            line(&[(10.5, 2.0), (14.0, 2.0), (14.0, 5.5)]);
            line(&[(14.0, 10.5), (14.0, 14.0), (10.5, 14.0)]);
            line(&[(5.5, 14.0), (2.0, 14.0), (2.0, 10.5)]);
            ellipse(8.0, 8.0, 2.0, 2.0);
        }
        Icon::Alert => {
            closed(&[(8.0, 2.0), (14.5, 13.5), (1.5, 13.5)]);
            line(&[(8.0, 6.5), (8.0, 9.5)]);
            line(&[(8.0, 11.5), (8.0, 11.6)]);
        }
    }
}
