use super::*;
use crate::env::EnvError;

/// `clouds.xml` written by hand to the schema `CloudHatManager.Init` reads (tabs separate the two numbers
/// of a velocity, newlines separate lines).
const CLOUDS_XML: &str = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<CloudHatManager>
  <mCloudHatFrags>
    <Item>
      <mPosition x=\"10\" y=\"-20\" z=\"1500.5\"/>
      <mRotation x=\"0\" y=\"0\" z=\"1.5\"/>
      <mScale x=\"2\" y=\"2\" z=\"1\"/>
      <mName>Altostratus</mName>
      <mLayers>
        <Item>
          <mFilename>cloudhat_alto_01</mFilename>
          <mCostFactor value=\"1.5\"/>
          <mRotationScale value=\"0.25\"/>
          <mCamPositionScalerAdjust value=\"0.5\"/>
          <mTransitionInTimePercent value=\"10\"/>
          <mTransitionOutTimePercent value=\"20\"/>
          <mTransitionInDelayPercent value=\"30\"/>
          <mTransitionOutDelayPercent value=\"40\"/>
          <mHeightTigger value=\"900\"/>
          <mHeightFadeRange value=\"150\"/>
        </Item>
        <Item>
          <mFilename>cloudhat_alto_02</mFilename>
        </Item>
      </mLayers>
      <mTransitionAlphaRange value=\"0.5\"/>
      <mTransitionMidPoint value=\"0.4\"/>
      <mEnabled value=\"True\"/>
      <mAngularVelocity x=\"0\" y=\"0\" z=\"0.01\"/>
      <mAnimBlendWeights x=\"1\" y=\"0.5\" z=\"0\"/>
      <mUVVelocity>
0.1\t0.2
-0.3\t0.4
not two numbers
      </mUVVelocity>
      <mAnimMode>
0
2
x
      </mAnimMode>
      <mShowLayer>
        <Item value=\"true\"/>
        <Item value=\"false\"/>
      </mShowLayer>
      <mEnableAnimations value=\"false\"/>
    </Item>
    <Item>
      <mName>Cumulus</mName>
      <mEnabled value=\"false\"/>
    </Item>
  </mCloudHatFrags>
  <mDesiredTransitionTimeSec value=\"30\"/>
  <mCamPositionScaler x=\"1\" y=\"2\" z=\"3\"/>
  <mAltitudeScrollScaler value=\"0.75\"/>
</CloudHatManager>
";

fn key_data(entries: &str, count: u32) -> String {
    format!("<keyData><numKeyEntries value=\"{count}\"/><keyEntryData>\n{entries}\n</keyEntryData></keyData>")
}

/// `cloudkeyframes.xml` by hand, following `CloudSettingsMap.Init` and `CloudSettingsMapItem.Init`.
fn keyframes_xml() -> String {
    let curve =
        |name: &str, entries: &str, count: u32| format!("<{name}>{}</{name}>", key_data(entries, count));
    let item = |name: &str, bits: &str| {
        let mut s = format!(
            "<Item><Name>{name}</Name><Settings><CloudList><mProbability>\n40\n60\nx\n</mProbability><mBits>\n{bits}\n</mBits></CloudList>"
        );
        s += &curve(
            "CloudColor",
            "0\t1\t0.5\t0.25\t1\n0.5\t0\t0\t0\t0\n0\t9\t9\t9\t9\nshort\t1\t2\n1\t1\t1\t1\t1",
            4,
        );
        for tag in [
            "CloudLightColor",
            "CloudAmbientColor",
            "CloudSkyColor",
            "CloudBounceColor",
            "CloudEastColor",
            "CloudWestColor",
            "CloudScaleFillColors",
            "CloudDensityShift_Scale_ScatteringConst_Scale",
            "CloudPiercingLightPower_Strength_NormalStrength_Thickness",
            "CloudScaleDiffuseFillAmbient_WrapAmount",
        ] {
            s += &curve(tag, "0\t0.1\t0.2\t0.3\t0.4", 1);
        }
        s + "</Settings></Item>"
    };
    format!(
        "<?xml version=\"1.0\"?><CloudKeyframes><KeyframeTimes>\n0\n0.25\n0.5\n1\n</KeyframeTimes><SettingsMap>{}{}{}</SettingsMap></CloudKeyframes>",
        item("Clear", "0x0000001F\n000000F0"),
        item("Rain", "FFFFFFFF"),
        item("Clear", "00000003")
    )
}

#[test]
fn cloud_hats_read_what_the_original_reads() {
    let m = CloudHatManager::parse(CLOUDS_XML.as_bytes()).unwrap();
    assert_eq!(m.frags.len(), 2);
    assert_eq!(
        (m.desired_transition_time_sec, m.cam_position_scaler, m.altitude_scroll_scaler),
        (30.0, [1.0, 2.0, 3.0], 0.75)
    );
    let f = m.find("Altostratus").unwrap();
    assert_eq!((f.position, f.rotation, f.scale), ([10.0, -20.0, 1500.5], [0.0, 0.0, 1.5], [2.0, 2.0, 1.0]));
    assert_eq!((f.transition_alpha_range, f.transition_mid_point), (0.5, 0.4));
    assert!(f.enabled && !f.enable_animations, "`True` counts as true");
    assert_eq!((f.angular_velocity, f.anim_blend_weights), ([0.0, 0.0, 0.01], [1.0, 0.5, 0.0]));
    assert_eq!(
        f.uv_velocity,
        [[0.1, 0.2], [-0.3, 0.4], [0.0, 0.0]],
        "a line that is not two numbers is a zero velocity"
    );
    assert_eq!(f.anim_mode, [0, 2, 0]);
    assert_eq!(f.show_layer, [true, false]);
    assert_eq!(f.layers.len(), 2);
    let l = &f.layers[0];
    assert_eq!(l.filename, "cloudhat_alto_01");
    assert_eq!((l.cost_factor, l.rotation_scale, l.cam_position_scaler_adjust), (1.5, 0.25, 0.5));
    assert_eq!(
        (
            l.transition_in_time_percent,
            l.transition_out_time_percent,
            l.transition_in_delay_percent,
            l.transition_out_delay_percent
        ),
        (10.0, 20.0, 30.0, 40.0)
    );
    assert_eq!((l.height_trigger, l.height_fade_range), (900.0, 150.0));
    assert_eq!(f.layers[1].filename, "cloudhat_alto_02");
    assert_eq!(f.layers[1].cost_factor, 0.0, "missing values are zero");
    // a hat with almost nothing in it still reads
    let c = m.find("Cumulus").unwrap();
    assert!(!c.enabled && c.layers.is_empty());
    assert_eq!(
        (c.uv_velocity.len(), c.anim_mode.len()),
        (1, 1),
        "an empty list is one empty line, as in the C#"
    );
    assert!(m.find("altostratus").is_none(), "FindFrag compares exactly");
}

#[test]
fn cloud_settings_read_lists_and_curves() {
    let map = CloudSettingsMap::parse(keyframes_xml().as_bytes()).unwrap();
    assert_eq!(map.keyframe_times, [0.0, 0.25, 0.5, 1.0]);
    assert_eq!(map.items.iter().map(|i| i.name.as_str()).collect::<Vec<_>>(), ["Clear", "Rain"]);
    let clear = map.find("Clear").unwrap();
    // the name listed twice: the later one, in the first one's place
    assert_eq!(clear.cloud_list.bits, [3]);
    let rain = map.find("Rain").unwrap();
    assert_eq!(rain.cloud_list.probability, [40, 60, 0], "`x` is not a number");
    assert_eq!(rain.cloud_list.bits, [-1], "FFFFFFFF is a set of 32 hats");
    assert_eq!(parse_bits("0x0000001F"), 31);
    assert_eq!(parse_bits("000000f0"), 240);
    assert_eq!(parse_bits("not hex"), 0);
    // curves: the line for time 0 is given twice (the later wins), the short one is skipped
    let c = &rain.cloud_color;
    assert_eq!(c.num_key_entries, 4);
    assert_eq!(c.entries, [(0.0, [9.0, 9.0, 9.0, 9.0]), (0.5, [0.0; 4]), (1.0, [1.0; 4])]);
    assert_eq!(rain.curves().len(), 11);
    assert_eq!(rain.curves()[10].0, "CloudScaleDiffuseFillAmbient_WrapAmount");
    assert_eq!(rain.cloud_west_color.entries, [(0.0, [0.1, 0.2, 0.3, 0.4])]);
    assert!(map.find("clear").is_none());
}

#[test]
fn built_cloud_files_read_back_to_the_same_values() {
    let m = CloudHatManager::parse(CLOUDS_XML.as_bytes()).unwrap();
    assert_eq!(CloudHatManager::parse(m.to_xml().as_bytes()).unwrap(), m);
    let s = CloudSettingsMap::parse(keyframes_xml().as_bytes()).unwrap();
    assert_eq!(CloudSettingsMap::parse(s.to_xml().as_bytes()).unwrap(), s);
    let none = CloudHatManager::default();
    assert_eq!(CloudHatManager::parse(none.to_xml().as_bytes()).unwrap().frags.len(), 0);
}

#[test]
fn a_settings_entry_with_nothing_in_it_is_all_empty() {
    let map =
        CloudSettingsMap::parse(b"<k><SettingsMap><Item><Name>Bare</Name></Item></SettingsMap></k>").unwrap();
    let bare = map.find("Bare").unwrap();
    assert!(bare.curves().iter().all(|(_, c)| c.entries.is_empty() && c.num_key_entries == 0));
    assert_eq!(map.keyframe_times, [0.0], "no KeyframeTimes element is one empty line");
}

#[test]
fn wrong_kinds_of_files_are_refused_and_damaged_ones_never_panic() {
    assert!(matches!(CloudHatManager::parse(b"RBF0\0\0"), Err(EnvError::Binary(_))));
    assert!(matches!(CloudSettingsMap::parse(b""), Err(EnvError::Empty)));
    let originals = [CLOUDS_XML.to_string(), keyframes_xml()];
    let mut seed = 0x1234_5678_9ABC_DEF1u64;
    let mut next = move || {
        seed ^= seed << 13;
        seed ^= seed >> 7;
        seed ^= seed << 17;
        seed
    };
    for (n, original) in originals.iter().enumerate() {
        let parse = |bytes: &[u8]| {
            let _ = if n == 0 {
                CloudHatManager::parse(bytes).map(|_| ())
            } else {
                CloudSettingsMap::parse(bytes).map(|_| ())
            };
        };
        for len in 0..original.len() {
            parse(&original.as_bytes()[..len]);
        }
        for _ in 0..200 {
            let mut bytes = original.clone().into_bytes();
            for _ in 0..1 + next() % 5 {
                let i = (next() % bytes.len() as u64) as usize;
                bytes[i] = next() as u8;
            }
            parse(&bytes);
        }
    }
}
