# Side pads v1.0: design review against the headrest standard (2026-11-18)

The headrest went through rounds v1.0 → v1.2c. Each round left a rule, and this review checks the side-pad kit against all of them. The checks run on Onshape's own geometry (branch `sidepads-v1`), using the review harness in `featurescript/review/`. It builds the kit on the **real chair parts** and reports:
- part volumes;
- clashes between every kit part and every chair part;
- screw-length windows;
- whether the bolt is free when pressed.

## Lessons carried over from the headrest
| Headrest lesson | Side-pad check | Result |
|---|---|---|
| Tolerance block (P1S/PETG, 2026-11-16): holes +0.4–0.5, nut traps AF +0.3 / T +0.4, sliding fits ≥ 0.2 per side | Same block at the top of the FS. M3 clearance was 3.4, now 3.5. Dovetail had 0 clearance, now 0.2 per side + 0.3 end gaps. Nose ribs had 0.15 in the grooves, now 0.25 | fixed |
| Every part one closed solid | `oneBody()` on every part | pass |
| No clash between parts (checked on the real chair, not a stand-in) | Harness: all kit pairs + kit × chair | see matrix |
| Screw tips never break out (v1.1 tenon M3×30 broke out) | Pad screws: tip must end past the nut and inside the nut pocket. The harness picks the stock length that does: T0 → M5×16; tilt 15° → M5×30 | pass |
| ≥ 1.2 mm plastic under a nut, ≥ 1.2 mm round a hex | Shell nut floor 1.2 under the seat; hexes 1.7 apart. The tongue M3 nuts broke into the bolt bore and were moved (t ±9, 1.4 wall) | fixed |
| Countersinks leave a land | Bolt washer was 1.6 thick with the csk going right through; now 2.0 with a 0.5 land | fixed |
| Shape carries the load, screws only lock | Comb ribs = height; slot walls = lean twist; dovetail = joint; foot nose + ribs in the seat grooves = pad. **The bolt is the only part resisting slide-out, and a child pushing the pad outward loads it at 61 % of the push.** Its land in the wall is now a full 3 mm cylinder (bearing about 14 MPa at a 300 N knock) | changed |
| Body counts, not just feature status (the 7d39d8c lesson) | Harness reports every part and its volume | pass |
| Things a carer touches must be easy and safe | **The old Ø6 dome sat 6.4 mm down a Ø12 trench, out of a finger's reach.** Now the wing tip outer wall is 6 mm thick, with a 16 mm wide, 3 mm deep finger channel (45° sides). The dome top sits flush with the wing surface. Press about 3.5 mm and slide; a 0.8 × 45° chamfer on the inside edge of each hole cams the dome down | fixed |
| Teardrops / corner-up nuts per print orientation | Not done yet. Print orientation needs the support study (as headrest_ORIENT) first | **open** |
| Fit-test coupon first | C6 snap-button coupon not made yet | **open** |

## Defects found in the v1.0 FeatureScript and fixed
1. **Lock not finger-pressable** (see the last row above).
2. **The tilt shim overlapped the foot's nose** (no seat recess). The pad screws ran square to the foot but the nuts were square to the tilted shell (up to 15° off). **Fix:**
   - The shim now has a seat on the foot side and a nose on the pad side.
   - Its screw holes follow the shell's axis.
   - A printed **wedge plate** (one part, both screws) sits under the heads.
   - The foot's screw slots allow +2.5 mm for the tilt.
3. **The pad-screw heads had nothing to sit on:** the flare at y 9 ends 3 mm behind the head. **Fix:** a flat screw land (rounded rectangle, x xs..xs+7) on the foot. The screws moved to the pad's centre height at y 9 / 19, clear of the neck, so a hex key reaches them from outside.
4. **Neck swept through the slot's inner wall at the mouth.** **Fix:**
   - a 14 mm straight run before the bend;
   - tongue size held until the neck is clear of the slot;
   - height held until it is out of the wing;
   - a 3 mm × 45° lead-in on the inner wall.
5. **Wing tip block ran into the arm rest** (arm rest top z 197.8). **Fix:** the tip starts at z 198 and the slot floor at z 201. **All 8 height steps move up 5 mm** (T_POS0 227).
6. **Slot was 0.08 mm from the wing bracket** (support_left_wing). **Fix:**
   - the whole socket moves 2 mm outward (D_SHIFT);
   - the tip block is cut around neighbouring chair parts.
7. Washer touched its pocket floor (0.05 lift); the depth-piece neck overlapped the dovetail by 0.02.

## Open before "ready to print"
- Print orientation study → teardrops on the horizontal holes in that orientation.
- C6 snap-button test coupon (wing wall + tongue stub + bolt + washer + your spring) before the real wings.
- Spring: OD ≤ 6, free length 11–16, **solid length ≤ 4.5 mm** (6.4 mm bolt travel).
- Load test: 300 N outward on the pad at the top step (bolt shear), 150 N down.
- PT/OT fit on a child: pad top ≥ 25 mm below the armpit.
