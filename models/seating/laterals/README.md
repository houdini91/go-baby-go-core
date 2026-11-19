# Side pads (lateral trunk supports): Round 5 (built in Onshape, awaiting owner review)

FeatureScript: `featurescript/gbg_sidepad_r5.fs` (v1.1 kept as `gbg_sidepad_v1.fs` for history). It is uploaded into the Feature Studio **"GbG Side Pads v1"** on branch `sidepads-v1` (same exported feature names, so Part Studio 1 picked it up). Nothing is merged to Main.

Onshape views (branch `sidepads-v1`):
- **Part Studio 1**: chair + kit, left long LOW lower pad, right short HIGH upper pad, 20 mm foam.
- **Side pads r5 - kit (print layout + coupon)**: the left set in print orientation and the C7 latch coupon.
- **Side pads r5 - all parts**: every part and variant in a grid, named by part number.
- **Side pads r5** (assembly): Part Studio 1 inserted and fixed.
- "Side pads v1" (old kit-only studio) now shows an error: the r5 kit needs the chair and the wing feature.

Progress log and screenshots: `viewer/review_r5/cad/PROGRESS.md`, `viewer/review_r5/cad/shots/`.

## What it is
- **Wing reprint (WG):** the v1.1 tip block is gone. A 40 mm notch in the wing's front edge (z 207-247) takes the kit's nose. Behind it: a 14.1 mm slot (z 214.1-246.6), a 3.3 x 45° mouth chamfer, and a catch groove 4.0 deep in the outer wall (6° locking face). The old tongue pocket, the slit behind the cheek, the dead lower M5 pocket and (owner decision) the open pocket and screw seats above the nose (z 247-270) are filled with the wing's own sections, flush. The blade lean used for the fills is measured on the wing (LD 0.053, LT 0.069 per mm). Strap slits S1/S2, the teardrop channel, the hole column and the z 186 hole are untouched.
- **Blank cap (CP):** the cut-out piece of wing plus a stub tongue with a snap finger. It closes the notch when the kit is off.
- **Carrier (AR):** the nose (the wing's own front section, 0.3 hairlines), a tongue 13.7 x 32.1 (was 25.4 tall), and one smooth 18 x 28 loft arm (short S drop 22, long L drop 44) with an R3.5 root fillet and a hidden tenon. Tongue details: R3 root, a 1.5°/side wedge at the tip (zero play once latched), bearing pads top and bottom (0.3 relief between), and 2 crush ribs (0.2) top and bottom near the tip.
- **Latch A (BT + TS):** one sliding button-catch: 12 x 24 stadium cap, flush with the nose's outer face, 0.8 thumb scoop, 0.4 rim round. It has a block and a tooth 22.5 tall x 5.0 (with a 0.5 root gusset). Travel is 4.9 to a hard stop. The tooth's 6° face hooks the groove; a 45° lead-in gives the click on entry. The button drops in from the nose top (a hidden opening), and the orange tell sleeve snaps in under the 1.6 lip and holds its flange, so it is captive. Hidden 0.4 slits in the tooth make the slicer add internal walls. Release: press and pull forward, as before.
- **Carbon:** one Ø6 rod in a Ø6.5 teardrop bore (below the latch, through tongue and nose into the arm root), and 4 x Ø2 tenon pins.
- **Depth piece (DP):** one depth, D0 (pad back x 104); the width comes from the foam. The tilt is in the foot: T0, 8° and 15° as A/B mirror pairs. The tenon sleeve ends at the tenon tip (the hidden step: an outward push goes into plastic). The foot face has two dovetail grooves (pad positions ±11), closed at the front, each with an M5 nut pocket under the groove.
- **Pad (PD), ring (RG), foam (FM), cover (CV):** the plain curved shell with one dovetail rail and the pad-screw counterbore on the child side. The cover groove is 3.2-7.6 x 2.5 deep with a snap undercut to 8.0, and the clip ring band is 5.6-7.6 x 2.1 with a 0.4 barb. Shell and ring are z-symmetric, so one part serves both sides. The reference foam is 5/10/20/30 (FOAM parameter, default 20) under a 3 mm neoprene cover.

## Why the flip became 4 carriers per arm length
The wing tip leans (+3.2° in d, +3.95° in t). A z-mirrored nose would sit 2.3 / 2.8 mm off the wing at the notch ends, so one carrier cannot serve LOW and HIGH. Each carrier is built for its side and LOW/HIGH: AR-{L,R}-{S,L}-{LO,HI} = 8 STLs. A child set still needs 2 carriers.

## Part list (numbers per ROUND5_PLAN §8, scheme awaiting owner OK; engraved 0.4 deep on a hidden face)
| Part no. | Part | Qty per side | Engraving |
|---|---|---|---|
| GBG-SP-WG-L/R-r5 | Wing reprint (chair part; Onshape keeps the name left_wing/right_wing) | 1 | notch ceiling |
| GBG-SP-CP-L/R-r5 | Blank cap | 1 (kit off) | cap bottom |
| GBG-SP-AR-{L,R}-{S,L}-{LO,HI}-r5 | Carrier | 1 | nose bottom face |
| GBG-SP-BT-L/R-r5 | Button-catch (same part for LOW and HIGH) | 1 | block inner face |
| GBG-SP-TS-r5 | Orange tell sleeve | 1 | none (0.6 wall, too thin) |
| GBG-SP-DP-D0-{T0, T8A, T8B, T15A, T15B}-r5 | Depth piece (A = left top-in = right top-out) | 1 | foot face between the grooves |
| GBG-SP-PD-r5 | Pad shell (both sides) | 1 | child face |
| GBG-SP-RG-r5 | Clip ring (both sides) | 1 | none yet (band too small) |
| GBG-SP-FM-{5,10,20,30}-r5 | Foam | 1 | - |
| GBG-SP-CV-r5 | Neoprene cover 3 mm, no-sew | 1 | - |
| GBG-SP-CF6-r5 | Carbon rod Ø6 x **35.0** | 1 | - |
| GBG-SP-CF2-r5 | Carbon pin Ø2 x **21.0** | 4 | - |
| GBG-SP-SP-r5 | Spring SS304, wire 0.55, OD 6, L0 14, 6 + 2 coils (≈0.81 N/mm: 3.2 N latched, 7.2 N pressed, solid 4.4) | 1 | - |
| GBG-SP-DW-r5 | Steel dowel tooth: **optional, not used**. The printed catch meets the target alone (see REVIEW) | 0 | - |

## Fasteners (home stock)
| Joint | Fastener |
|---|---|
| Tenon ↔ depth piece | M5 x 12 countersunk (DIN 7991) from the back face + M5 nut in the tenon |
| Depth piece ↔ pad | M5 x 10 button head (ISO 7380) from the child side (counterbore under the foam) through the rail + M5 nut dropped into the foot's pocket. It clamps the rail and only locks the slide; the dovetail takes the loads |

## Carbon, foam, cover
- Carbon: Ø6 rod x 35.0 (one length for every carrier) and Ø2 pins x 21.0. Cut them in the rod cutting gauge v2 (`models/seating/headrest/generator/rod_cutting_gauge.py`, constants `SIDEPAD_ROD6_L` / `SIDEPAD_PIN2_L`). Bond with high-Tg epoxy (Tg > 80 °C).
- Foam from the listed stock: 5 = one 5 mm Super2 EVA sheet, 10 = one 10 mm sheet, 20 (standard) = 2 x 10 EVA or 5 EVA + 15 HD top foam, 30 = 5 EVA + 25 HD. Cover: 3 mm SBR neoprene (listed 4000031372319), one layer down the groove's outer wall, squeezed to 2.4 by the clip ring.

See [PRINT_GUIDE.md](PRINT_GUIDE.md) and [REVIEW.md](REVIEW.md) (Round 5 section) for numbers, checks and open items.

---

# Archive: v1.1

# Side pads (lateral trunk supports) v1.1: remodelled, pre-first-print

FeatureScript: `featurescript/gbg_sidepad_v1.fs`. Onshape branch `sidepads-v1` of GbG_chair_forth:
- **Part Studio 1:** the chair with the reshaped wings and the kit fitted on both sides.
- **Side pads v1:** the kit on its own.

Two features:
- **GbG Side Pad Wing (chair):** the wing reprint.
  - Each wing tip gets a vertical slot with one **rail** per height position on its inner wall.
  - The outer wall gets a **catch groove** per position, plus lead-ins at the mouth.
  - Removes the modelled print-support plates (`support_*_wing`).
- **GbG Side Pad v1:** the kit, built in place.
  - Options: side, LOW/HIGH, height position 1–3, depth piece D0–D3, release parts, and a REFERENCE cushion.
  - A "check all 24 settings" switch reports every clash, kit against kit and kit against the real chair, and the kit's max x.

## What changed from v1.0 (owner review, 18–19 Nov)
The v1.0 arm could not be built as one smooth part: its hook needed a 5 mm bend for an 18 mm arm, and that fold was the "layer lines". v1.1 is a remodel to the owner's brief: **assembled, it reads as one flowing piece.**
- **Arm:**
  - The tongue tapers 50 → 32 tall *inside* the slot.
  - It leaves the mouth 16 wide, then becomes one 18 × 28 pill-section arm (grown toward the inside of the bend only).
  - It follows a circular plan arc (R ≈ 26.6) plus a two-arc vertical S, ending level and heading inboard.
  - One loft through sections square to the path. Fold margin about 1.3; max x 163 (arm rests at 165).
- **Release:** a flush pill button (7 × 9) on the side of the arm. Its cap is the arm's own surface. It drives the printed shuttle catch (coupons C1–C5) with your own spring.
  - Rod travel 3.5, cap floor = stop at 3.9, shuttle retracts 4.5.
  - Spring: OD ≤ 6, free length 11–12, **solid length ≤ 5.2**, as stiff as fits.
- **Height:** 3 fixed tongue positions (z 227 / 259.5 / 292) × LOW/HIGH flip (±47.5) = 6 pad heights.
  - Each position has a rail in the wing, and the tongue's groove rides on it (rail lever about 36 mm; the old rib-in-notch had 5.5).
  - The comb can come back later.
  - Flip = rotate the carrier 180° about the front–back axis and use it on the other side.
- **Joint:** a hidden tenon (13 × 20 × 13, pinned by 4 × Ø2 carbon rods across its root) on the arm end slides into the depth piece's sleeve. The 18 × 28 skin is continuous, so only a hairline shows.
  - One M5×12 countersunk screw from the back face (toward the chair back) into a nut inside the tenon.
- **Depth piece:** one loft from the hairline into an 18 mm trumpet that flares into a pill foot. The foot follows the pad back; a 20 × 3.5 key sits in a groove.
  - Pad screw: M5×10 **button head** inside the depth piece's hollow (long 3 mm key from the joint end). It goes into a nut in a boss (R 7) on the shell back, put in from the child side before the foam.
  - No screws or nuts are visible.
  - Depth set D0–D3: pad back at x 104 / 92 / 80 / 68 (with 17 / 30 / 43 mm foam).
- **Pad:** a plain curved plate.
  - The cover wraps its edge and is trapped by a **flush** clip ring in a groove in the back face.
  - The REFERENCE cushion body shows the assembled look and is not printed.
- **Carbon:** Ø4 × 62 and Ø3 × 59 rods, slid in from the tongue's rear face and bonded. They run across the wing mouth into the arm root.
- **Deferred** (they brought back clutter or gaps): tilt shims, the ±6.25 fine seat adjustment, and the comb.

| Joint | Fastener (from stock) |
|---|---|
| Carrier tenon ↔ depth piece | M5×12 countersunk + M5 nut (DIN 934 or ISO 4032; pockets 5.1 deep) |
| Depth piece ↔ pad shell | M5×10 button head (ISO 7380) + M5 nut, from inside the depth piece |

See [REVIEW.md](REVIEW.md) for the review rounds (design, mechanical, strength) and the open items.

**Test first:** the C6 latch coupon (feature "GbG Side Pad latch coupon" in "Side pads v1"), then one carrier with a 300 N / 150 N load test. Do not print production parts before both pass.

Carbon to buy (one line, same listing as your 6/8 mm rods): Ø4 × 250 (one rod per side) and Ø3 × 250 (one per side) and Ø2 × 250 (the 4 tenon pins, 21 mm each, per side).
