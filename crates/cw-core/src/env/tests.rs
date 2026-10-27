use super::*;

/// A `time.xml` written by hand the way the C# reads it (`Timecycle.Init`), with an element and a
/// comment it does not know. The samples are at hours 0, 6, 12 and 18; the first holds for 2 hours and the
/// third for 4.
const TIME_XML: &str = r#"<?xml version="1.0" encoding="UTF-8"?>
<time version="1">
  <!-- not read -->
  <suninfo sun_roll="122" sun_yaw="0"/>
  <mooninfo moon_roll="-122" moon_wobble_freq="2" moon_wobble_amp="0.2" moon_wobble_offset="0.375"/>
  <other thing="1"/>
  <sample name="MIDNIGHT" hour="0" duration="2" uw_tc_mod="underwater_a"/>
  <sample name="DAWN" hour="6" duration="0" uw_tc_mod="underwater_a"/>
  <sample name="NOON" hour="12" duration="4" uw_tc_mod="underwater_b"/>
  <sample name="DUSK" hour="18" duration="0"/>
  <region name="GLOBAL"/>
  <region name="URBAN"/>
</time>
"#;

fn time() -> Timecycle {
    Timecycle::parse(TIME_XML.as_bytes()).unwrap()
}

/// `weather.xml` as the C# reads it (`Weather.Init`): two types, one effect, one cycle.
const WEATHER_XML: &str = r#"<?xml version="1.0" encoding="UTF-8"?>
<CWeatherInfoMgr>
  <WeatherGpuFx>
    <Item>
      <Name>rain</Name>
      <SystemType>RAIN</SystemType>
      <diffuseName>rain_streak</diffuseName>
      <distortionTexture>rain_distort</distortionTexture>
      <diffuseSplashName>splash</diffuseSplashName>
      <driveType>DRIVE_WIND</driveType>
      <windInfluence value="0.5"/>
      <gravity value="-9.8"/>
      <emitterSettingsName>emit</emitterSettingsName>
      <renderSettingsName>render</renderSettingsName>
    </Item>
  </WeatherGpuFx>
  <WeatherTypes>
    <Item>
      <Name>CLEAR</Name>
      <Sun value="1"/>
      <Cloud value="0.25"/>
      <WindMin value="0.5"/>
      <WindMax value="2"/>
      <Rain value="0"/>
      <TimeCycleFilename>common:/data/timecycle/w_clear.xml</TimeCycleFilename>
      <CloudSettingsName>Clear</CloudSettingsName>
    </Item>
    <Item>
      <Name>RAIN</Name>
      <Sun value="0.1"/>
      <Rain value="0.8"/>
      <Fog value="0.4"/>
      <OceanWaveAmplitude value="1.5"/>
      <TimeCycleFilename>common:/data/timecycle/w_rain.xml</TimeCycleFilename>
      <CloudSettingsName>Rain</CloudSettingsName>
    </Item>
  </WeatherTypes>
  <WeatherCycles>
    <Item>
      <CycleName>cycle_a</CycleName>
      <TimeMult value="4"/>
    </Item>
  </WeatherCycles>
</CWeatherInfoMgr>
"#;

/// A keyframe file: four values per entry, one per time sample.
fn cycle_xml(name: &str, hdr: [f32; 4], regions: &[&str]) -> String {
    let mut s = format!(
        "<?xml version=\"1.0\"?>\n<timecycle>\n  <cycle name=\"{name}\" regions=\"{}\">\n",
        regions.len()
    );
    for r in regions {
        s += &format!("    <region name=\"{r}\">\n");
        s += &format!("      <sky_hdr>{} {} {} {}</sky_hdr>\n", hdr[0], hdr[1], hdr[2], hdr[3]);
        s += "      <sky_sun_col_r>1 1 1 1</sky_sun_col_r>\n      <sky_sun_col_g>0.5 0.5 0.5 0.5</sky_sun_col_g>\n";
        s += "    </region>\n";
    }
    s + "  </cycle>\n</timecycle>\n"
}

fn weather() -> Weather {
    let mut w = Weather::parse(WEATHER_XML.as_bytes()).unwrap();
    let files = [
        ("common:/data/timecycle/w_clear.xml", cycle_xml("clear", [1.0, 2.0, 3.0, 4.0], &["GLOBAL"])),
        (
            "common:/data/timecycle/w_rain.xml",
            cycle_xml("rain", [10.0, 20.0, 30.0, 40.0], &["GLOBAL", "URBAN"]),
        ),
    ];
    let problems = w
        .load_cycles(&mut |name| files.iter().find(|(n, _)| *n == name).map(|(_, x)| x.clone().into_bytes()));
    assert!(problems.is_empty(), "{problems:?}");
    w
}

fn close(a: f32, b: f32) {
    assert!((a - b).abs() < 1e-5, "{a} != {b}");
}

#[test]
fn time_xml_reads_what_the_original_reads() {
    let t = time();
    assert_eq!((t.sun_roll, t.sun_yaw, t.moon_roll), (122.0, 0.0, -122.0));
    assert_eq!((t.moon_wobble_freq, t.moon_wobble_amp, t.moon_wobble_offset), (2.0, 0.2, 0.375));
    let samples: Vec<_> = t.samples.iter().map(|s| (s.name.as_str(), s.hour, s.duration)).collect();
    assert_eq!(
        samples,
        [("MIDNIGHT", 0.0, 2.0), ("DAWN", 6.0, 0.0), ("NOON", 12.0, 4.0), ("DUSK", 18.0, 0.0)]
    );
    assert_eq!(t.samples[2].uw_tc_mod, "underwater_b");
    assert_eq!(t.samples[3].uw_tc_mod, "", "an attribute that is not there reads as empty");
    assert_eq!(t.regions, ["GLOBAL", "URBAN"]);
}

#[test]
fn built_files_read_back_to_the_same_values() {
    let t = time();
    assert_eq!(Timecycle::parse(t.to_xml().as_bytes()).unwrap(), t);
    let w = weather();
    let mut again = Weather::parse(w.to_xml().as_bytes()).unwrap();
    assert!(again.types.iter().all(|t| t.time_cycle.is_none()), "the keyframes are separate files");
    again.types.iter_mut().zip(&w.types).for_each(|(a, b)| a.time_cycle = b.time_cycle.clone());
    assert_eq!(again, w);
    for t in &w.types {
        let data = t.time_cycle.as_ref().unwrap();
        assert_eq!(CycleData::parse(data.to_xml().as_bytes()).unwrap().as_ref(), Some(data));
    }
}

#[test]
fn weather_types_carry_their_numbers_and_names() {
    let w = weather();
    let names: Vec<_> = w.types.iter().map(|t| t.name.as_str()).collect();
    assert_eq!(names, ["CLEAR", "RAIN"]);
    let rain = w.find("rain").expect("case does not matter when there is no exact match");
    assert_eq!((rain.sun, rain.rain, rain.fog, rain.ocean_wave_amplitude), (0.1, 0.8, 0.4, 1.5));
    assert_eq!(rain.name_hash, crate::hash::joaat("rain"));
    assert_eq!(rain.cloud_settings_name, "Rain");
    assert_eq!(rain.time_cycle_filename, "common:/data/timecycle/w_rain.xml");
    let clear = w.find("CLEAR").unwrap();
    assert_eq!((clear.cloud, clear.wind_min, clear.wind_max, clear.sandstorm), (0.25, 0.5, 2.0, 0.0));
    assert_eq!(clear.values().len(), 31);
    assert_eq!(clear.values()[1], ("Cloud", 0.25));
    assert_eq!(w.gpu_fx[0].name, "rain");
    assert_eq!((w.gpu_fx[0].wind_influence, w.gpu_fx[0].gravity), (0.5, -9.8));
    assert_eq!(w.cycles, [WeatherCycle { cycle_name: "cycle_a".into(), time_mult: 4.0 }]);
}

#[test]
fn a_weather_name_listed_twice_keeps_the_first_slot_and_the_last_values() {
    let xml = "<w><WeatherTypes><Item><Name>A</Name><Sun value=\"1\"/></Item><Item><Name>B</Name></Item>\
               <Item><Name>A</Name><Sun value=\"2\"/></Item></WeatherTypes></w>";
    let w = Weather::parse(xml.as_bytes()).unwrap();
    assert_eq!(
        w.types.iter().map(|t| (t.name.as_str(), t.sun)).collect::<Vec<_>>(),
        [("A", 2.0), ("B", 0.0)]
    );
}

#[test]
fn hours_fall_into_samples_and_blend_towards_the_next() {
    let t = time();
    let at = |h: f32| {
        let p = t.locate(h).unwrap();
        (p.index, p.next_blend)
    };
    // sample 0 (hour 0, held 2 h, then blends over the 4 h until hour 6)
    assert_eq!(at(0.0), (0, 0.0));
    assert_eq!(at(2.0), (0, 0.0));
    close(at(4.0).1, 0.5);
    assert_eq!(at(4.0).0, 0);
    assert!(at(5.999).1 > 0.999);
    // sample 1 starts exactly at its hour and blends over all 6 h
    assert_eq!(at(6.0), (1, 0.0));
    close(at(9.0).1, 0.5);
    // sample 2 holds for 4 h
    assert_eq!(at(15.9), (2, 0.0));
    close(at(17.0).1, 0.5);
    // the last sample blends into the first one 24 h later, through midnight
    assert_eq!(at(18.0), (3, 0.0));
    close(at(21.0).1, 0.5);
    assert_eq!(at(21.0).0, 3);
    assert!(at(23.9999).1 > 0.999);
    let p = t.locate(21.0).unwrap();
    close(p.current_blend + p.next_blend, 1.0);
}

#[test]
fn the_hour_wraps_around_the_day_in_both_directions() {
    let t = time();
    let same = |a: f32, b: f32| assert_eq!(t.locate(a), t.locate(b), "{a} and {b}");
    same(24.0, 0.0);
    same(27.0, 3.0);
    same(-3.0, 21.0);
    same(48.5, 0.5);
    assert_eq!(t.locate(f32::NAN), t.locate(0.0), "not a number is midnight");
    assert_eq!(t.locate(f32::INFINITY), t.locate(0.0));
    assert!(Timecycle::default().locate(12.0).is_none(), "no samples, no position");
    assert!(Timecycle::is_night(3.0) && Timecycle::is_night(22.0) && Timecycle::is_night(-1.0));
    assert!(!Timecycle::is_night(6.0) && !Timecycle::is_night(20.0) && !Timecycle::is_night(12.0));
}

#[test]
fn a_sample_as_long_as_its_span_never_blends() {
    let xml = "<t><sample name=\"a\" hour=\"0\" duration=\"12\"/><sample name=\"b\" hour=\"12\" duration=\"12\"/></t>";
    let t = Timecycle::parse(xml.as_bytes()).unwrap();
    for h in [0.0, 5.0, 11.99, 12.0, 18.0, 23.9] {
        assert_eq!(t.locate(h).unwrap().next_blend, 0.0, "{h}");
    }
}

#[test]
fn values_blend_between_samples_and_wrap_at_midnight() {
    let (t, w) = (time(), weather());
    let hdr = |h: f32| t.evaluate(&w, &EvalQuery::new(h, "CLEAR")).sky_hdr;
    // clear: [1, 2, 3, 4] at hours 0, 6, 12, 18
    close(hdr(0.0), 1.0);
    close(hdr(2.0), 1.0);
    close(hdr(4.0), 1.5); // halfway through the blend from sample 0 to 1
    close(hdr(6.0), 2.0);
    close(hdr(9.0), 2.5);
    close(hdr(12.0), 3.0);
    close(hdr(18.0), 4.0);
    close(hdr(21.0), 2.5); // sample 3 (4) towards sample 0 (1), halfway
    close(hdr(24.0), hdr(0.0));
    close(hdr(-3.0), hdr(21.0));
    assert!((hdr(23.9999) - hdr(0.0)).abs() < 1e-3, "continuous across midnight");
    // never outside the keyframes
    for i in 0..96 {
        let v = hdr(i as f32 * 0.25);
        assert!((1.0..=4.0).contains(&v), "{i}: {v}");
    }
}

#[test]
fn two_weathers_blend_by_the_factor() {
    let (t, w) = (time(), weather());
    let at = |blend: f32| {
        let q = EvalQuery { next_weather: Some("RAIN"), blend, ..EvalQuery::new(6.0, "CLEAR") };
        t.evaluate(&w, &q).sky_hdr
    };
    // hour 6 is exactly sample 1: clear 2, rain 20
    close(at(0.0), 2.0);
    close(at(0.25), 6.5);
    close(at(0.5), 11.0);
    close(at(1.0), 20.0);
    close(at(5.0), 20.0); // a factor past 1 stops at the next weather
    close(at(-1.0), 2.0);
    close(at(f32::NAN), 2.0);
    // the second weather alone does nothing: the factor only matters when there is a weather to blend to
    let q = EvalQuery { blend: 1.0, ..EvalQuery::new(6.0, "CLEAR") };
    close(t.evaluate(&w, &q).sky_hdr, 2.0);
}

#[test]
fn colours_vectors_and_unknown_names() {
    let (t, w) = (time(), weather());
    let p = t.evaluate(&w, &EvalQuery::new(0.0, "CLEAR"));
    assert_eq!(p.sky_sun_col, [1.0, 0.5, 0.0], "an entry the file lacks is zero");
    assert_eq!(p.sky_zenith_col, [0.0; 4]);
    let q = EvalQuery::new(0.0, "CLEAR");
    close(t.evaluate_named(&w, &q, "sky_sun_col_g"), 0.5);
    assert_eq!(t.evaluate_named(&w, &q, "fog_start"), 0.0);
    // a weather nobody defined gives nothing at all
    assert_eq!(t.evaluate(&w, &EvalQuery::new(12.0, "NOPE")), TimeCycleParams::default());
    // without samples the first one is used
    let p = Timecycle::default().evaluate(&w, &EvalQuery::new(12.0, "CLEAR"));
    close(p.sky_hdr, 1.0);
}

#[test]
fn a_region_that_is_missing_falls_back_to_the_first() {
    let (t, w) = (time(), weather());
    let mut urban = w.clone();
    urban.types[1].time_cycle.as_mut().unwrap().regions[1].entries.insert("sky_hdr".into(), vec![99.0]);
    let q = |region| EvalQuery { region, ..EvalQuery::new(0.0, "RAIN") };
    close(t.evaluate(&urban, &q("URBAN")).sky_hdr, 99.0);
    close(t.evaluate(&urban, &q("GLOBAL")).sky_hdr, 10.0);
    close(t.evaluate(&urban, &q("NOWHERE")).sky_hdr, 10.0);
    // CLEAR has only GLOBAL, so URBAN is answered by it
    close(t.evaluate(&w, &EvalQuery { region: "URBAN", ..EvalQuery::new(0.0, "CLEAR") }).sky_hdr, 1.0);
}

#[test]
fn a_value_list_shorter_than_the_samples_clamps_and_wraps() {
    let region = CycleRegion { name: "G".into(), entries: [("x".to_string(), vec![5.0, 7.0])].into() };
    close(region.value_at("x", 0, 1.0), 5.0);
    close(region.value_at("x", 0, 0.5), 6.0);
    close(region.value_at("x", 1, 0.5), 6.0); // the one after the last is the first
    close(region.value_at("x", 9, 1.0), 7.0); // past the end uses the last
    close(region.value_at("x", 9, 0.0), 5.0);
    assert_eq!(region.value_at("y", 0, 0.5), 0.0);
}

#[test]
fn the_sun_climbs_until_early_afternoon_and_sets_below_the_horizon() {
    let t = time();
    let z = |h: f32| t.sun_moon(h).sun_direction[2];
    for i in 0..96 {
        let h = i as f32 * 0.25;
        let sm = t.sun_moon(h);
        for v in [sm.sun_direction, sm.moon_direction, sm.moon_axis] {
            close((v[0] * v[0] + v[1] * v[1] + v[2] * v[2]).sqrt(), 1.0);
        }
    }
    // up from 6:00 to 20:00, level with the horizon at both ends
    close(z(6.0), 0.0);
    close(z(20.0), 0.0);
    // monotonic: rising to 13:00, falling after it
    let mut prev = z(6.0);
    for i in 1..=28 {
        let v = z(6.0 + i as f32 * 0.25);
        assert!(v > prev, "rising at {}", 6.0 + i as f32 * 0.25);
        prev = v;
    }
    for i in 1..=28 {
        let v = z(13.0 + i as f32 * 0.25);
        assert!(v < prev, "falling at {}", 13.0 + i as f32 * 0.25);
        prev = v;
    }
    // the peak is the sine of the tilt: sun_roll 122 degrees
    close(z(13.0), (122.0f32).to_radians().sin());
    assert!((0..=23).all(|h| z(h as f32 + 0.5) < z(13.0)));
    // below the horizon at midnight, and through the night either side of it
    for h in [0.0, 1.0, 2.5, 4.0, 5.5, 21.0, 22.0, 23.0, 23.99, 24.0] {
        assert!(z(h) < 0.0, "sun at {h}h should be below the horizon: {}", z(h));
    }
    assert!(z(0.0) < -0.5, "well below it at midnight");
    // sunrise is in the east, sunset in the west
    assert!(t.sun_moon(6.0).sun_direction[0] > 0.99);
    assert!(t.sun_moon(20.0).sun_direction[0] < -0.99);
}

#[test]
fn the_moon_is_up_while_the_sun_is_not() {
    let t = time();
    let moon_z = |h: f32| t.sun_moon(h).moon_direction[2];
    for h in [22.0, 23.0, 0.0, 1.0, 2.0] {
        assert!(moon_z(h) > 0.0, "{h}");
    }
    assert!(moon_z(12.0) < 0.0, "below the horizon at noon");
    // the tilt is the other way round: -moon_roll
    let axis = t.sun_moon(0.0).moon_axis;
    close(axis[1], (122.0f32).to_radians().cos());
    close(axis[2], (122.0f32).to_radians().sin());
}

#[test]
fn light_comes_from_the_sun_by_day_and_the_moon_at_night_and_never_from_below() {
    let t = time();
    let noon = GlobalLights::from_time(&t, 12.0, None, false, false);
    assert_eq!(noon.light_direction, noon.sun_direction);
    let night = GlobalLights::from_time(&t, 3.0, None, false, false);
    let moon = t.sun_moon(3.0).moon_direction;
    assert_eq!(night.light_direction, moon);
    // after 21:00 it is the moon, which is up
    let late = GlobalLights::from_time(&t, 21.5, None, false, false);
    assert_eq!(late.light_direction, t.sun_moon(21.5).moon_direction);
    // at 5:30 the sun is still below the horizon: its direction with the height raised to zero
    let dawn = GlobalLights::from_time(&t, 5.5, None, false, false);
    assert!(dawn.sun_direction[2] < 0.0);
    assert_eq!(dawn.light_direction, [dawn.sun_direction[0], dawn.sun_direction[1], 0.0]);
    for i in 0..96 {
        assert!(GlobalLights::from_time(&t, i as f32 * 0.25, None, false, false).light_direction[2] >= 0.0);
    }
    let swapped = GlobalLights::from_time(&t, 12.0, None, false, true);
    assert_eq!(swapped.sun_direction[1], -noon.sun_direction[1]);
    assert_eq!(noon.dir_colour, [1.0; 4]);
    assert_eq!(
        noon.dir_ambient_colour,
        [0.5, 0.5, 0.5, 1.0],
        "without time-cycle values: white light, grey ambient"
    );
}

#[test]
fn light_colours_are_multiplied_by_their_intensity_and_capped_without_hdr() {
    let t = time();
    let p = TimeCycleParams {
        light_dir_col: [2.0, 1.0, 0.5, 0.8],
        light_dir_amb_col: [1.0, 0.5, 0.25, 0.5],
        light_dir_amb_intensity_mult: 4.0,
        light_natural_amb_up: [1.0, 1.0, 1.0, 0.5],
        light_natural_amb_up_intensity_mult: 2.0,
        light_natural_amb_down: [0.4, 0.4, 0.4, 0.5],
        light_artificial_ext_up: [3.0, 3.0, 3.0, 1.0],
        light_artificial_ext_down: [0.2, 0.2, 0.2, 0.5],
        sky_hdr: 3.0,
        ..Default::default()
    };
    let hdr = GlobalLights::from_time(&t, 12.0, Some(&p), true, false);
    for (got, want) in [
        (hdr.dir_colour, [1.6, 0.8, 0.4, 0.64]),
        (hdr.dir_ambient_colour, [1.0, 0.5, 0.25, 0.5].map(|v| v * 0.5 * 4.0)),
        (hdr.natural_ambient_up, [1.0, 1.0, 1.0, 0.5]),
        (hdr.natural_ambient_down, [0.2, 0.2, 0.2, 0.25]),
        (hdr.artificial_ambient_up, [3.0, 3.0, 3.0, 1.0]),
    ] {
        got.iter().zip(want).for_each(|(a, b)| close(*a, b));
    }
    close(hdr.hdr_intensity, 3.0);
    // without hdr: at least half intensity for the directional light, then everything is capped
    let sdr = GlobalLights::from_time(&t, 12.0, Some(&p), false, false);
    sdr.dir_colour.iter().zip([1.0, 0.8, 0.4, 0.64]).for_each(|(a, b)| close(*a, b));
    assert!(
        sdr.dir_ambient_colour
            .iter()
            .chain(&sdr.natural_ambient_up)
            .chain(&sdr.artificial_ambient_up)
            .all(|v| *v <= 0.5)
    );
    close(sdr.hdr_intensity, 1.0);
    // a weak light is raised to the minimum multiplier without hdr
    let weak = TimeCycleParams { light_dir_col: [1.0, 1.0, 1.0, 0.1], ..p };
    close(GlobalLights::from_time(&t, 12.0, Some(&weak), false, false).dir_colour[0], 0.5);
    close(GlobalLights::from_time(&t, 12.0, Some(&weak), true, false).dir_colour[0], 0.1);
}

#[test]
fn a_hand_chosen_light_direction_matches_the_viewers_formula() {
    let l = GlobalLights::manual(0.0, 0.0, None, false);
    assert_eq!(l.light_direction[2], 0.0);
    close(l.light_direction[1], -1.0);
    let up = GlobalLights::manual(0.3, std::f32::consts::FRAC_PI_2, None, false);
    close(up.light_direction[2], 1.0);
    assert_eq!(up.moon_direction, up.light_direction.map(|v| -v));
    let p = TimeCycleParams { sky_hdr: 2.0, ..Default::default() };
    let hdr = GlobalLights::manual(0.0, 0.0, Some(&p), true);
    assert_eq!(hdr.dir_colour, [2.0, 2.0, 2.0, 1.0]);
    close(hdr.dir_ambient_colour[0], 0.35);
}

#[test]
fn sky_constants_follow_the_original_shader_setup() {
    let t = time();
    let p = TimeCycleParams {
        sky_hdr: 5.0,
        sky_sun_hdr: 9.0,
        sky_sun_col: [1.0, 0.5, 0.25],
        sky_sun_mie: [2.0, 3.0, 4.0],
        sky_sun_influence_radius: 1000.0,
        sky_stars_iten: 0.2,
        sky_sun_disc_col: [1.0, 1.0, 0.0],
        sky_sun_disc_size: 7.0,
        ..Default::default()
    };
    let lights = GlobalLights::from_time(&t, 12.0, Some(&p), true, false);
    let hdr = SkyParams::new(&p, &lights, true);
    assert_eq!((hdr.hdr_intensity, hdr.sun_color_hdr), (5.0, [9.0, 4.5, 2.25, 9.0]));
    let sdr = SkyParams::new(&p, &lights, false);
    assert_eq!((sdr.hdr_intensity, sdr.sun_color_hdr[3]), (1.8, 4.8));
    assert_eq!(sdr.sun_constants, [6.0, 3.0, 0.03, 4.0]);
    assert_eq!(sdr.sun_direction, lights.sun_direction.map(|v| -v));
    close(sdr.starfield_intensity, 1.0);
    assert_eq!(sdr.sun_disc_color_hdr, [1.0, 1.0, 0.0, 7.0]);
}

#[test]
fn files_that_are_not_what_they_claim_are_refused_with_a_reason() {
    assert!(matches!(Timecycle::parse(b""), Err(EnvError::Empty)));
    assert!(matches!(Weather::parse(b"RBF0\x01"), Err(EnvError::Binary(_))));
    assert!(matches!(CycleData::parse(b"<a><b>"), Err(EnvError::Xml(_))));
    assert_eq!(CycleData::parse(b"<a/>").unwrap(), None, "no cycle in it");
    let mut w = Weather::parse(WEATHER_XML.as_bytes()).unwrap();
    let problems = w.load_cycles(&mut |name| {
        if name.ends_with("w_clear.xml") { Some(b"junk".to_vec()) } else { None }
    });
    assert_eq!(problems.len(), 2);
    assert_eq!((problems[0].weather.as_str(), problems[1].reason.as_str()), ("CLEAR", "file not found"));
    assert!(w.types.iter().all(|t| t.time_cycle.is_none()));
    // a weather with no cycle gives a zero everywhere, and the others still work
    assert_eq!(time().evaluate(&w, &EvalQuery::new(0.0, "CLEAR")), TimeCycleParams::default());
}

/// Cheap deterministic noise for the damage tests.
struct Noise(u64);

impl Noise {
    fn next(&mut self) -> u64 {
        self.0 ^= self.0 << 13;
        self.0 ^= self.0 >> 7;
        self.0 ^= self.0 << 17;
        self.0
    }
}

#[test]
fn damaged_files_are_errors_or_data_and_never_a_panic() {
    let w = weather();
    let originals: Vec<(&str, Vec<u8>)> = vec![
        ("time", time().to_xml().into_bytes()),
        ("weather", w.to_xml().into_bytes()),
        ("cycle", w.types[1].time_cycle.as_ref().unwrap().to_xml().into_bytes()),
    ];
    let parse_all = |name: &str, bytes: &[u8]| match name {
        "time" => {
            let _ =
                Timecycle::parse(bytes).map(|t| t.evaluate(&Weather::default(), &EvalQuery::new(9.0, "x")));
        }
        "weather" => {
            let _ = Weather::parse(bytes);
        }
        _ => {
            let _ = CycleData::parse(bytes);
        }
    };
    let mut noise = Noise(0x9E37_79B9_7F4A_7C15);
    for (name, original) in &originals {
        for len in 0..original.len() {
            parse_all(name, &original[..len]);
        }
        for _ in 0..300 {
            let mut bytes = original.clone();
            for _ in 0..1 + noise.next() % 6 {
                let i = (noise.next() % bytes.len() as u64) as usize;
                bytes[i] = noise.next() as u8;
            }
            parse_all(name, &bytes);
        }
    }
}

#[test]
fn hostile_numbers_do_not_leak_into_the_results() {
    let cycle = "<t><c name=\"x\" regions=\"1\"><r name=\"GLOBAL\"><sky_hdr>NaN inf -inf 1e999 3</sky_hdr><e></e></r></c></t>";
    let data = CycleData::parse(cycle.as_bytes()).unwrap().unwrap();
    assert_eq!(data.regions[0].entries["sky_hdr"], [0.0, 0.0, 0.0, 0.0, 3.0]);
    assert_eq!(
        data.regions[0].entries["e"],
        [0.0],
        "an entry with no numbers is a single zero, as in the C#"
    );
    let t = Timecycle::parse(b"<t><sample hour=\"NaN\" duration=\"inf\"/><sample hour=\"12\"/></t>").unwrap();
    for h in [0.0, 6.0, 12.0, 23.0] {
        let p = t.locate(h).unwrap();
        assert!(p.current_blend.is_finite() && p.next_blend.is_finite());
    }
}
