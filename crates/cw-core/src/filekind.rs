//! Classification of game files by extension, used to pick a viewer.

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum FileKind {
    Rpf,
    TextureDict,
    Drawable,
    DrawableDict,
    Fragment,
    MapData,
    MapTypes,
    Bounds,
    Paths,
    NavMesh,
    ClipDict,
    ExpressionDict,
    Particles,
    Cloth,
    VehicleRecord,
    Waypoints,
    Script,
    Scenario,
    Audio,
    Gxt2,
    Dds,
    Image,
    Xml,
    Text,
    Meta,
    Unknown,
}

impl FileKind {
    pub fn from_name(name_lower: &str) -> Self {
        let ext = name_lower.rsplit_once('.').map(|(_, e)| e).unwrap_or("");
        match ext {
            "rpf" => Self::Rpf,
            "ytd" => Self::TextureDict,
            "ydr" => Self::Drawable,
            "ydd" => Self::DrawableDict,
            "yft" => Self::Fragment,
            "ymap" => Self::MapData,
            "ytyp" => Self::MapTypes,
            "ybn" => Self::Bounds,
            "ynd" => Self::Paths,
            "ynv" => Self::NavMesh,
            "ycd" => Self::ClipDict,
            "yed" => Self::ExpressionDict,
            "ypt" => Self::Particles,
            "yld" => Self::Cloth,
            "yvr" => Self::VehicleRecord,
            "ywr" => Self::Waypoints,
            "ysc" => Self::Script,
            "ymt" => Self::Scenario,
            "awc" => Self::Audio,
            "gxt2" => Self::Gxt2,
            "dds" => Self::Dds,
            "png" | "jpg" | "jpeg" | "bmp" | "tga" | "gif" => Self::Image,
            "xml" | "meta" | "gfx" | "ymf" | "ide" | "ipl" | "cutxml" | "cfg" | "html" | "json" | "xsd" => {
                if ext == "meta" || ext == "ymf" {
                    Self::Meta
                } else {
                    Self::Xml
                }
            }
            "txt" | "dat" | "lua" | "ini" | "csv" | "log" | "md" | "nametable" | "rel" => Self::Text,
            _ => Self::Unknown,
        }
    }

    /// Stored as a GTA "resource" (RSC7) rather than as a plain binary.
    pub fn is_resource(self) -> bool {
        matches!(
            self,
            Self::TextureDict
                | Self::Drawable
                | Self::DrawableDict
                | Self::Fragment
                | Self::MapData
                | Self::MapTypes
                | Self::Bounds
                | Self::Paths
                | Self::NavMesh
                | Self::ClipDict
                | Self::ExpressionDict
                | Self::Particles
                | Self::Cloth
                | Self::VehicleRecord
                | Self::Waypoints
                | Self::Script
                | Self::Scenario
        )
    }

    pub fn label(self) -> &'static str {
        match self {
            Self::Rpf => "Archive",
            Self::TextureDict => "Texture dictionary",
            Self::Drawable => "Drawable",
            Self::DrawableDict => "Drawable dictionary",
            Self::Fragment => "Fragment",
            Self::MapData => "Map data",
            Self::MapTypes => "Map types",
            Self::Bounds => "Collision bounds",
            Self::Paths => "Traffic paths",
            Self::NavMesh => "Nav mesh",
            Self::ClipDict => "Clip dictionary",
            Self::ExpressionDict => "Expression dictionary",
            Self::Particles => "Particles",
            Self::Cloth => "Cloth",
            Self::VehicleRecord => "Vehicle record",
            Self::Waypoints => "Waypoints",
            Self::Script => "Script",
            Self::Scenario => "Scenario / metadata",
            Self::Audio => "Audio container",
            Self::Gxt2 => "Text table",
            Self::Dds => "DDS texture",
            Self::Image => "Image",
            Self::Xml => "XML",
            Self::Text => "Text",
            Self::Meta => "Meta",
            Self::Unknown => "File",
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn classifies_by_extension() {
        assert_eq!(FileKind::from_name("adder.yft"), FileKind::Fragment);
        assert_eq!(FileKind::from_name("x64a.rpf"), FileKind::Rpf);
        assert_eq!(FileKind::from_name("noext"), FileKind::Unknown);
        assert!(FileKind::from_name("a.ytd").is_resource());
        assert!(!FileKind::from_name("a.xml").is_resource());
    }
}
