# CodeWalker UI design

CodeWalker is a dense professional tool (an **Operate** surface, in impeccable's terms):
people open it to find a file, look at it and act on it, hundreds of times a day. The
interface should disappear into that work. These rules were distilled from the vendored
skills in `.claude/skills/` (emil-design-eng, impeccable, taste-skill, ui-ux-pro-max),
keeping only what transfers to a native immediate-mode UI (egui on wgpu, no web view).

## Principles

1. **Neutral chrome, colour only for data and state.** One accent hue, used for focus,
   selection and the single primary action; under ~3 % of pixels.
2. **Elevation by lightness, not by decoration.** Panels differ from their ground by one
   step. No gradients, glow, blur or glass. A hairline *or* a shadow, never both; shadows
   only on floating things (menus, popovers, dialogs).
3. **Motion by frequency.** Anything used hundreds of times a day (keyboard navigation,
   selection, tab switch, palette toggle, preview swap) has **no** animation. See *Motion*.
4. **Keyboard first, pointer friendly.** Every action is a command and reachable from the
   command palette (`Ctrl+K`), a visible control and, when frequent, a shortcut. No
   hover-only or drag-only paths.
5. **Honest states.** Empty, loading, error and first-run all exist, live inside the block
   that owns them, and say what happened and what to do.
6. **Real data, always.** Zero prints as `0`; missing prints as a grey `—`; plurals use a
   rule (`1 file`, `2 files`); totals are computed, never typed.

## Tokens (dark first)

| Token | Value | Use |
| --- | --- | --- |
| `ground` | `#0D0D0E` | window background, viewport letterbox |
| `panel` | `#141416` | side panels, lists |
| `raised` | `#1C1C1F` | hovered rows, menus, popovers |
| `active` | `#26262B` | selected row |
| `border` | `#26262A` | hairlines between panels / rows |
| `control` | `#66666E` | control outlines, icons (≥ 3:1 on `panel`) |
| `text` | `#F2F2F3` | primary text |
| `muted` | `#B8B8BE` | secondary text |
| `faint` | `#8F8F98` | metadata (≥ 4.5:1 on `panel`) |
| `accent` | `#6EA8FE` | focus ring, selection line, primary action |
| `positive` / `negative` / `warning` | `#3FC07A` / `#E5566F` / `#E0A43A` | status, always with a word or icon too |

`ui::theme` unit-tests these ratios (text ≥ 4.5:1, controls ≥ 3:1), so a palette edit that
breaks accessibility fails CI.

## Type

- UI: **Inter** (variable font, default instance). Data identifiers (hashes, offsets, hex,
  archive keys): **JetBrains Mono**. Nothing else; mono is never used as a "technical" label voice.
- Sizes: 11 (badges) · 12 (meta, headers) · 13 (dense body, rows) · 14 (default body) ·
  16 (panel title) · 20 (rare headline figure). Nothing below 11.
- Weights: egui renders the default instance, so hierarchy comes from size and colour
  (`text` → `muted` → `faint`), not weight.
- Numbers right-aligned in columns, fixed decimals, thousands separators. Sizes use binary
  units (`KiB`, `MiB`).
- Long names **truncate in the middle** (siblings often differ at the end); the full value is
  in a tooltip and the details panel. Never truncate numbers.

## Layout & density

- 4 px base: 4, 8, 12, 16, 24, 32.
- Row pitch: **28 px** lists and tree, **24 px** hex rows, controls 28 px, toolbar 40 px.
  Density moves row height, padding and type together.
- Hit targets ≥ 24 × 24 px; whole rows are clickable.
- Three resizable regions: tree (240–320 px), list (flexible), preview/details (320–480 px).
  Panels share the `panel` fill separated by a 1 px `border`; no nested cards.
- Lists over ~50 rows are virtualised (they are: egui `show_rows`). Headers stay visible.
  Counts read "1–50 of 12,483".

## Motion

| Frequency | Rule |
| --- | --- |
| 100+/day: keyboard navigation, selection, palette toggle, tab and preview switch | none |
| tens/day: hover, tree expand/collapse, menus | ≤ 100 ms or instant |
| occasional: dialogs, toasts, progress | 150–250 ms ease-out |

- Curves: ease-out `cubic-bezier(0.23, 1, 0.32, 1)`; in-out `cubic-bezier(0.77, 0, 0.175, 1)`.
  Never ease-in, never bounce on UI.
- Exits are ~65 % of the entrance. Dialogs fade + scale from 0.97, never from 0.
- 3D camera: 1:1 direct manipulation; momentum only on release (decel 0.998). Keyboard
  camera moves are not animated.
- Repaint only while something animates. `Settings → Reduce motion` (and the OS setting
  where available) removes travel and scale and keeps opacity/colour changes.

## Interaction states

Every control has default, hover, focus (2 px accent ring, 2 px offset), pressed, disabled
(with a reason in a tooltip), and loading where relevant. Destructive actions name the
object and are undoable where possible.

## Resolved skill conflicts

| Topic | Chosen |
| --- | --- |
| Dark palette | neutral greys (no navy/slate), semantic hues from the dev-tool palettes |
| Text contrast | 4.5:1 floor, aim for 7:1 on primary text |
| Control outline | 3:1 (the taste-skill outlines measure 1.6–2.2:1 and are not used) |
| Body size | 13–14 px dense, 16 px for prose/help |
| Loading | flat grey bars, no shimmer; spinner only after 300 ms |
| Easing / durations | emil-design-eng values, dialogs 200–250 ms |
| Stagger, press-scale on rows | none |

## Checklist before shipping a screen

Screenshot at 1280 × 800 and at the smallest supported width, then check: neutral ground and
one-step panels · accent only on focus/selection/primary · no gradients/glow/blur, no
border-plus-shadow · contrast ≥ 4.5:1 text, ≥ 3:1 controls and focus ring · numbers aligned,
zero vs missing · ≤ 6 type sizes, none < 11 px · long names truncated in the middle ·
virtualised lists with visible headers · empty / loading / error / first-run states exist ·
hover, pressed, selected, disabled visible · nothing frequent animates, nothing over 300 ms ·
every action reachable by keyboard and by a visible control.
