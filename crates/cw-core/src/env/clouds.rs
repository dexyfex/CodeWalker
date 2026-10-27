//! Clouds: the cloud hats (what to draw) and the cloud settings per weather (how it is lit), as
//! parameters. The hats' drawables are `.ydr` files; only their names are kept here.
//!
//! Ported from `CodeWalker.Core/World/Clouds.cs` (`CloudHatManager`, `CloudHatFrag`, `CloudHatFragLayer`,
//! `CloudSettingsMap`, `CloudSettingsMapItem`, `CloudSettingsMapCloudList`, `CloudSettingsMapKeyData`).
//! The tool's six "UV offset" sliders (`CloudAnimSetting`) are window state, not game data, and are left
//! to the renderer.
//!
//! Both files are plain XML as far as the C# reads them. `common.rpf/data/clouds.xml` (the hats):
//!
//! ```xml
//! <CloudHatManager>                                   <!-- the root's name is not read -->
//!   <mCloudHatFrags>
//!     <Item>
//!       <mPosition x="0" y="0" z="1500"/> <mRotation x="0" y="0" z="0"/> <mScale x="1" y="1" z="1"/>
//!       <mName>Altostratus</mName>
//!       <mLayers><Item> <mFilename>cloudhat_alto_01</mFilename> <mCostFactor value="1"/> ... </Item></mLayers>
//!       <mTransitionAlphaRange value="0.5"/> <mTransitionMidPoint value="0.5"/> <mEnabled value="true"/>
//!       <mAngularVelocity x="0" y="0" z="0"/> <mAnimBlendWeights x="1" y="0" z="0"/>
//!       <mUVVelocity>0.1<tab>0.2<newline>...</mUVVelocity>
//!       <mAnimMode>0<newline>1</mAnimMode>
//!       <mShowLayer><Item value="true"/></mShowLayer> <mEnableAnimations value="true"/>
//!     </Item>
//!   </mCloudHatFrags>
//!   <mDesiredTransitionTimeSec value="30"/> <mCamPositionScaler x="0" y="0" z="0"/> <mAltitudeScrollScaler value="0"/>
//! </CloudHatManager>
//! ```
//!
//! and `cloudkeyframes.xml` (`update/update.rpf/common/data` over `common.rpf/data`), the settings each
//! weather type names in its `CloudSettingsName`: a list of `KeyframeTimes` and, per name, which hats may
//! appear and eleven colour/constant curves over those times.

use super::{
    Result,
    xml::{self, N},
};

/// One texture layer of a cloud hat.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct CloudHatLayer {
    /// The drawable's name (`mFilename`), a `.ydr` without its extension.
    pub filename: String,
    pub cost_factor: f32,
    pub rotation_scale: f32,
    pub cam_position_scaler_adjust: f32,
    pub transition_in_time_percent: f32,
    pub transition_out_time_percent: f32,
    pub transition_in_delay_percent: f32,
    pub transition_out_delay_percent: f32,
    /// Camera height the layer appears at (`mHeightTigger` in the file, which misspells it).
    pub height_trigger: f32,
    pub height_fade_range: f32,
}

/// One cloud hat (`CloudHatFrag`).
#[derive(Debug, Clone, PartialEq, Default)]
pub struct CloudHatFrag {
    pub position: [f32; 3],
    pub rotation: [f32; 3],
    pub scale: [f32; 3],
    pub name: String,
    pub layers: Vec<CloudHatLayer>,
    pub transition_alpha_range: f32,
    pub transition_mid_point: f32,
    pub enabled: bool,
    pub angular_velocity: [f32; 3],
    pub anim_blend_weights: [f32; 3],
    pub uv_velocity: Vec<[f32; 2]>,
    pub anim_mode: Vec<u8>,
    pub show_layer: Vec<bool>,
    pub enable_animations: bool,
}

/// `clouds.xml`.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct CloudHatManager {
    pub frags: Vec<CloudHatFrag>,
    pub desired_transition_time_sec: f32,
    pub cam_position_scaler: [f32; 3],
    pub altitude_scroll_scaler: f32,
}

/// Lines of a text value: split on `\n`, trimmed. The text as a whole is trimmed first, so an empty
/// value is one empty line, as in the C#.
fn lines(text: &str) -> Vec<&str> {
    text.trim().split('\n').map(str::trim).collect()
}

impl CloudHatManager {
    pub fn parse(bytes: &[u8]) -> Result<CloudHatManager> {
        xml::with_root(bytes, |root| Ok(Self::read(root)))
    }

    fn read(root: N) -> CloudHatManager {
        CloudHatManager {
            frags: xml::items(root, "mCloudHatFrags").into_iter().map(read_frag).collect(),
            desired_transition_time_sec: xml::value_f32(root, "mDesiredTransitionTimeSec"),
            cam_position_scaler: xml::vec3(root, "mCamPositionScaler"),
            altitude_scroll_scaler: xml::value_f32(root, "mAltitudeScrollScaler"),
        }
    }

    /// The hat called `name` (`CloudHatManager.FindFrag`, an exact match).
    pub fn find(&self, name: &str) -> Option<&CloudHatFrag> {
        self.frags.iter().find(|f| f.name == name)
    }

    /// A file [`parse`](Self::parse) reads back to the same hats.
    pub fn to_xml(&self) -> String {
        let v3 = |tag: &str, v: [f32; 3]| format!("<{tag} x=\"{}\" y=\"{}\" z=\"{}\"/>", v[0], v[1], v[2]);
        let mut s = format!("{}<CloudHatManager>\n  <mCloudHatFrags>\n", xml::HEADER);
        for f in &self.frags {
            s += "    <Item>\n";
            for (tag, v) in [("mPosition", f.position), ("mRotation", f.rotation), ("mScale", f.scale)] {
                s += &format!("      {}\n", v3(tag, v));
            }
            s += &format!("      <mName>{}</mName>\n      <mLayers>\n", xml::esc(&f.name));
            for l in &f.layers {
                s += &format!("        <Item>\n          <mFilename>{}</mFilename>\n", xml::esc(&l.filename));
                for (tag, v) in [
                    ("mCostFactor", l.cost_factor),
                    ("mRotationScale", l.rotation_scale),
                    ("mCamPositionScalerAdjust", l.cam_position_scaler_adjust),
                    ("mTransitionInTimePercent", l.transition_in_time_percent),
                    ("mTransitionOutTimePercent", l.transition_out_time_percent),
                    ("mTransitionInDelayPercent", l.transition_in_delay_percent),
                    ("mTransitionOutDelayPercent", l.transition_out_delay_percent),
                    ("mHeightTigger", l.height_trigger),
                    ("mHeightFadeRange", l.height_fade_range),
                ] {
                    s += &format!("          <{tag} value=\"{v}\"/>\n");
                }
                s += "        </Item>\n";
            }
            s += "      </mLayers>\n";
            s += &format!("      <mTransitionAlphaRange value=\"{}\"/>\n", f.transition_alpha_range);
            s += &format!("      <mTransitionMidPoint value=\"{}\"/>\n", f.transition_mid_point);
            s += &format!("      <mEnabled value=\"{}\"/>\n", f.enabled);
            s += &format!("      {}\n", v3("mAngularVelocity", f.angular_velocity));
            s += &format!("      {}\n", v3("mAnimBlendWeights", f.anim_blend_weights));
            let uv: Vec<String> = f.uv_velocity.iter().map(|v| format!("{}\t{}", v[0], v[1])).collect();
            s += &format!("      <mUVVelocity>\n{}\n      </mUVVelocity>\n", uv.join("\n"));
            let modes: Vec<String> = f.anim_mode.iter().map(u8::to_string).collect();
            s +=
                &format!("      <mAnimMode>\n{}\n      </mAnimMode>\n      <mShowLayer>\n", modes.join("\n"));
            for v in &f.show_layer {
                s += &format!("        <Item value=\"{v}\"/>\n");
            }
            s += "      </mShowLayer>\n";
            s += &format!("      <mEnableAnimations value=\"{}\"/>\n    </Item>\n", f.enable_animations);
        }
        s += "  </mCloudHatFrags>\n";
        s += &format!("  <mDesiredTransitionTimeSec value=\"{}\"/>\n", self.desired_transition_time_sec);
        s += &format!("  {}\n", v3("mCamPositionScaler", self.cam_position_scaler));
        s += &format!(
            "  <mAltitudeScrollScaler value=\"{}\"/>\n</CloudHatManager>\n",
            self.altitude_scroll_scaler
        );
        s
    }
}

fn read_frag(n: N) -> CloudHatFrag {
    CloudHatFrag {
        position: xml::vec3(n, "mPosition"),
        rotation: xml::vec3(n, "mRotation"),
        scale: xml::vec3(n, "mScale"),
        name: xml::child_string(n, "mName"),
        layers: xml::items(n, "mLayers")
            .into_iter()
            .map(|l| CloudHatLayer {
                filename: xml::child_string(l, "mFilename"),
                cost_factor: xml::value_f32(l, "mCostFactor"),
                rotation_scale: xml::value_f32(l, "mRotationScale"),
                cam_position_scaler_adjust: xml::value_f32(l, "mCamPositionScalerAdjust"),
                transition_in_time_percent: xml::value_f32(l, "mTransitionInTimePercent"),
                transition_out_time_percent: xml::value_f32(l, "mTransitionOutTimePercent"),
                transition_in_delay_percent: xml::value_f32(l, "mTransitionInDelayPercent"),
                transition_out_delay_percent: xml::value_f32(l, "mTransitionOutDelayPercent"),
                height_trigger: xml::value_f32(l, "mHeightTigger"),
                height_fade_range: xml::value_f32(l, "mHeightFadeRange"),
            })
            .collect(),
        transition_alpha_range: xml::value_f32(n, "mTransitionAlphaRange"),
        transition_mid_point: xml::value_f32(n, "mTransitionMidPoint"),
        enabled: xml::value_bool(n, "mEnabled"),
        angular_velocity: xml::vec3(n, "mAngularVelocity"),
        anim_blend_weights: xml::vec3(n, "mAnimBlendWeights"),
        // one "x<tab>y" per line; a line that is not two numbers is a zero velocity
        uv_velocity: lines(&xml::child_string(n, "mUVVelocity"))
            .into_iter()
            .map(|l| match l.split('\t').collect::<Vec<_>>()[..] {
                [x, y] => [xml::parse_f32(x), xml::parse_f32(y)],
                _ => [0.0; 2],
            })
            .collect(),
        anim_mode: lines(&xml::child_string(n, "mAnimMode"))
            .into_iter()
            .map(|l| l.parse().unwrap_or(0))
            .collect(),
        show_layer: xml::items(n, "mShowLayer").into_iter().map(|i| xml::attr_bool(i, "value")).collect(),
        enable_animations: xml::value_bool(n, "mEnableAnimations"),
    }
}

/// Which hats a cloud setting may show: `probability[i]` is the chance of the i-th group and `bits[i]`
/// the hats in it, one bit per hat in `clouds.xml` order.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct CloudList {
    pub probability: Vec<i32>,
    pub bits: Vec<i32>,
}

/// A curve over the day: the value (four numbers) at each key time.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct CloudKeyData {
    /// The count the file declares (`numKeyEntries`); `entries` is what is actually listed.
    pub num_key_entries: i32,
    /// `(time, value)` in file order; a time listed twice keeps the later value in the earlier slot.
    pub entries: Vec<(f32, [f32; 4])>,
}

impl CloudKeyData {
    fn read(parent: N, name: &str) -> CloudKeyData {
        let Some(key) = xml::child(parent, name).and_then(|n| xml::child(n, "keyData")) else {
            return CloudKeyData::default();
        };
        let mut data =
            CloudKeyData { num_key_entries: xml::value_i32(key, "numKeyEntries"), entries: Vec::new() };
        for line in lines(&xml::child_string(key, "keyEntryData")) {
            // time and four numbers, tab separated in the game's files; a short line is skipped
            let cols: Vec<f32> = line.split_ascii_whitespace().map(xml::parse_f32).collect();
            if let [t, x, y, z, w, ..] = cols[..] {
                match data.entries.iter_mut().find(|e| e.0 == t) {
                    Some(slot) => slot.1 = [x, y, z, w],
                    None => data.entries.push((t, [x, y, z, w])),
                }
            }
        }
        data
    }

    fn write(&self, tag: &str) -> String {
        let rows: Vec<String> = self
            .entries
            .iter()
            .map(|(t, v)| format!("{t}\t{}\t{}\t{}\t{}", v[0], v[1], v[2], v[3]))
            .collect();
        format!(
            "        <{tag}>\n          <keyData>\n            <numKeyEntries value=\"{}\"/>\n            <keyEntryData>\n{}\n            </keyEntryData>\n          </keyData>\n        </{tag}>\n",
            self.num_key_entries,
            rows.join("\n")
        )
    }
}

/// Declares [`CloudSettingsItem`] with its eleven curves tied to the element each is read from.
macro_rules! cloud_settings_item {
    ($($field:ident => $tag:literal,)*) => {
        /// The settings of one weather's clouds (`CloudSettingsMapItem`).
        #[derive(Debug, Clone, PartialEq, Default)]
        pub struct CloudSettingsItem {
            pub name: String,
            pub cloud_list: CloudList,
            $(pub $field: CloudKeyData,)*
        }

        impl CloudSettingsItem {
            /// The curves by their element names, in file order.
            pub fn curves(&self) -> Vec<(&'static str, &CloudKeyData)> {
                vec![$(($tag, &self.$field),)*]
            }

            fn read(n: N) -> CloudSettingsItem {
                let settings = xml::child(n, "Settings");
                let list = settings.and_then(|s| xml::child(s, "CloudList"));
                let cloud_list = CloudList {
                    probability: lines(&list.map(|l| xml::child_string(l, "mProbability")).unwrap_or_default())
                        .into_iter()
                        .map(xml::parse_i32)
                        .collect(),
                    bits: lines(&list.map(|l| xml::child_string(l, "mBits")).unwrap_or_default())
                        .into_iter()
                        .map(parse_bits)
                        .collect(),
                };
                CloudSettingsItem {
                    name: xml::child_string(n, "Name"),
                    cloud_list,
                    $($field: settings.map(|s| CloudKeyData::read(s, $tag)).unwrap_or_default(),)*
                }
            }

            fn write(&self) -> String {
                let mut s = format!("    <Item>\n      <Name>{}</Name>\n      <Settings>\n", xml::esc(&self.name));
                let probability: Vec<String> = self.cloud_list.probability.iter().map(i32::to_string).collect();
                let bits: Vec<String> = self.cloud_list.bits.iter().map(|b| format!("{:08X}", *b as u32)).collect();
                s += &format!(
                    "        <CloudList>\n          <mProbability>\n{}\n          </mProbability>\n          <mBits>\n{}\n          </mBits>\n        </CloudList>\n",
                    probability.join("\n"),
                    bits.join("\n")
                );
                $(s += &self.$field.write($tag);)*
                s + "      </Settings>\n    </Item>\n"
            }
        }
    };
}

cloud_settings_item! {
    cloud_color => "CloudColor",
    cloud_light_color => "CloudLightColor",
    cloud_ambient_color => "CloudAmbientColor",
    cloud_sky_color => "CloudSkyColor",
    cloud_bounce_color => "CloudBounceColor",
    cloud_east_color => "CloudEastColor",
    cloud_west_color => "CloudWestColor",
    cloud_scale_fill_colors => "CloudScaleFillColors",
    cloud_density_shift_scale_scattering_const_scale => "CloudDensityShift_Scale_ScatteringConst_Scale",
    cloud_piercing_light_power_strength_normal_strength_thickness
        => "CloudPiercingLightPower_Strength_NormalStrength_Thickness",
    cloud_scale_diffuse_fill_ambient_wrap_amount => "CloudScaleDiffuseFillAmbient_WrapAmount",
}

/// `Convert.ToInt32(text, 16)`, which accepts a `0x` prefix and reads eight digits as two's complement.
/// Anything else is zero (the C# throws, which stops the whole file).
fn parse_bits(text: &str) -> i32 {
    let t = text.trim();
    let t = t.strip_prefix("0x").or_else(|| t.strip_prefix("0X")).unwrap_or(t);
    u32::from_str_radix(t, 16).map_or(0, |v| v as i32)
}

/// `cloudkeyframes.xml`.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct CloudSettingsMap {
    pub keyframe_times: Vec<f32>,
    /// In file order; a name listed twice keeps the later entry in the earlier one's place.
    pub items: Vec<CloudSettingsItem>,
}

impl CloudSettingsMap {
    pub fn parse(bytes: &[u8]) -> Result<CloudSettingsMap> {
        xml::with_root(bytes, |root| {
            let mut map = CloudSettingsMap {
                keyframe_times: lines(&xml::child_string(root, "KeyframeTimes"))
                    .into_iter()
                    .map(xml::parse_f32)
                    .collect(),
                items: Vec::new(),
            };
            for n in xml::items(root, "SettingsMap") {
                let item = CloudSettingsItem::read(n);
                match map.items.iter_mut().find(|i| i.name == item.name) {
                    Some(slot) => *slot = item,
                    None => map.items.push(item),
                }
            }
            Ok(map)
        })
    }

    /// The settings called `name` (exact match, as a weather type's `CloudSettingsName` gives it).
    pub fn find(&self, name: &str) -> Option<&CloudSettingsItem> {
        self.items.iter().find(|i| i.name == name)
    }

    /// A file [`parse`](Self::parse) reads back to the same settings.
    pub fn to_xml(&self) -> String {
        let times: Vec<String> = self.keyframe_times.iter().map(f32::to_string).collect();
        let mut s = format!(
            "{}<CloudKeyframes>\n  <KeyframeTimes>\n{}\n  </KeyframeTimes>\n  <SettingsMap>\n",
            xml::HEADER,
            times.join("\n")
        );
        for i in &self.items {
            s += &i.write();
        }
        s + "  </SettingsMap>\n</CloudKeyframes>\n"
    }
}

/// Both cloud files.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct Clouds {
    pub hats: CloudHatManager,
    pub settings: CloudSettingsMap,
}

#[cfg(test)]
mod tests;
