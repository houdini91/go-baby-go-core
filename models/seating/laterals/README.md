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
