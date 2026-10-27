# Design skills (vendored)

Reference skills used to guide the UI/UX of the Rust port (`cw-*` crates). They are
web-oriented; here they are applied as **principles** (motion, hierarchy, density,
accessibility, anti-"AI slop" rules) to a native egui/wgpu app, not as code to run.

| Folder | Upstream | Commit | License |
| --- | --- | --- | --- |
| `ui-ux-pro-max/` | https://github.com/nextlevelbuilder/ui-ux-pro-max-skill (`.claude/skills/ui-ux-pro-max`, tests removed) | `28b3b8d` | MIT |
| `emil-*/` | https://github.com/emilkowalski/skills (subset: design-eng, animate, animation-performance, animation-vocabulary, review-animations, improve-animations, find-animation-opportunities, apple-design, break-ui, prototype; `name:` prefixed with `emil-`) | `2d22863` | MIT |
| `taste-skill/` | https://github.com/Leonxlnx/taste-skill (`skills/taste-skill`, without `assets/` and `evals/`) | `12af471` | MIT |
| `impeccable/` | https://github.com/pbakaus/impeccable (`.claude/skills/impeccable`: `SKILL.md` + `reference/` only) | `d34c87e` | Apache-2.0 |

Notes

- `impeccable/scripts/` (its launcher downloads and runs a binary on first use) was
  deliberately **not** vendored. Its `SKILL.md` documents a launcher-less mode.
- Swift/Expo/React-specific Emil skills (`write-swift`, `mobile-native`, `animate-expo`,
  `ask-sonner`, `pick-ui-library`) were left out as irrelevant to a Rust app.
- Upstream licenses apply to each folder; see the upstream repositories for the full text.
- Project-specific conclusions drawn from these skills live in `docs/DESIGN.md`.
