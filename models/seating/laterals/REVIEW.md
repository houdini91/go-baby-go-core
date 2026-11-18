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
| No clash between parts (checked on the real chair, not a stand-in) | Harness: all kit pairs + kit × chair | **LOW kit: 12 of 12 clean** (steps 0 and 2; depths N0/N12/N36; tilt none/±15°): no clash, carrier max x 161.9, screw tip inside its window, bolt free when pressed. **HIGH kit: not yet run** (Onshape API limit, see below) |
| Screw tips never break out (v1.1 tenon M3×30 broke out) | Pad screws: tip must end past the nut and inside the nut pocket. The harness picks the stock length that does: T0 → M5×16; tilt 15° → M5×30 | pass |
| ≥ 1.2 mm plastic under a nut, ≥ 1.2 mm round a hex | Shell nut floor 1.2 under the seat; hexes 1.7 apart. The tongue M3 nuts broke into the bolt bore and were moved (t ±9, 1.4 wall) | fixed |
| Countersinks leave a land | Bolt washer was 1.6 thick with the csk going right through; now 2.0 with a 0.5 land | fixed |
| Shape carries the load, screws only lock | Comb ribs = height; slot walls = lean twist; dovetail = joint; foot nose + ribs in the seat grooves = pad. **The bolt is the only part resisting slide-out, and a child pushing the pad outward loads it at 61 % of the push.** Its land in the wall is now a full 3 mm cylinder (bearing about 14 MPa at a 300 N knock) | changed |
| Body counts, not just feature status (the 7d39d8c lesson) | Harness reports every part and its volume | pass |
| Things a carer touches must be easy and safe | **The old Ø6 dome sat 6.4 mm down a Ø12 trench, out of a finger's reach.** Now the wing tip outer wall is 6 mm thick, with a 16 mm wide, 3 mm deep finger channel (45° sides). The dome top sits flush with the wing surface. Press about 3.5 mm and slide; a 0.8 × 45° chamfer on the inside edge of each hole cams the dome down | fixed |
| Teardrops / corner-up nuts per print orientation | First orientation study done (`~/research/.../laterals/v1_review/orient.py`, 52 directions, ≥ 80 mm² flat on the bed). Teardrops to be set once you confirm the orientations | **open** |
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
8. **Seat recess pinched the nose's corners at seat UP/DOWN** (it was one taller oval). It is now the nose shape (+0.2) at each of the 3 positions.
9. **The pad's back edge (y 51) can reach the wing bracket (y ≥ 47.8) in the HIGH kit with a tilt shim** (seen at HIGH step 0, N12, 8°, seat DOWN). Trimming the pad to y 46 is not possible: its cover bead then hits the neck (y ≤ 43.1). **Open: owner decision** (see the matrix result).
10. **15° tilt + seat offset:**
    - the tilted screw left the foot's slot (allowance now +5.5 each way);
    - the wedge plate reached the neck (radius 6 → 5.5);
    - the shim's screw hole stopped 0.6 short of its face.
11. **One screw length per shim, safe at every seat position.** The nut boss is deeper (8.5) so the tip window is 4–10 mm past the nut floor:
    - no shim → M5×16 (tip 6.3);
    - 8° → M5×25 (tip 5.7–7.5);
    - 15° → **M5×30** (tip 6.4–9.8; 0.5 mm from the child face at seat DOWN, the tightest case).
12. A typo commented out PAD_H/SHELL_T. The studio "compiled" with no ERROR notice, but the feature failed. **Lesson: upload notices are not enough; the feature has to build in a Part Studio.**

## Open before "ready to print"
- **Finish the settings matrix (HIGH kit, LOW at the top step, seat UP/DOWN extremes).** On 2026-11-18 at 20:40 Onshape stopped answering Part Studio API calls (429, retry-after ~18.5 h, so until about 15:20 on 19 Nov). The 27 remaining settings are listed in the session notes. Item 9 (pad vs bracket) waits on this.
- Print orientation study → teardrops on the horizontal holes in that orientation.
- C6 snap-button test coupon (wing wall + tongue stub + bolt + washer + your spring) before the real wings.
- Spring: OD ≤ 6, free length 11–16, **solid length ≤ 4.5 mm** (6.4 mm bolt travel).
- Load test: 300 N outward on the pad at the top step (bolt shear), 150 N down.
- PT/OT fit on a child: pad top ≥ 25 mm below the armpit.

## Print orientation (first pass: support area from the Onshape meshes; to be confirmed by real slices like the headrest)
| Part | Candidate | Support | Note |
|---|---|---|---|
| Washer | flat, countersinks up | ~55 mm² (the two csk cones) | 2 mm plate |
| Wedge plate | land side down | 0 | |
| Bolt | standing on the shoulder end, dome up | ~34 mm² (spring-pocket roof, Ø6.6 bridge) | dome prints round |
| Depth piece | joint end (x J1) down, foot up | ~900 mm² | nose, ribs and land print clean on top; the dovetail profile prints as vertical walls |
| Carrier | tongue tip down (up = −t) | ~1100 mm² | tongue faces print as walls (good sliding surface). The swept neck needs tree support. **Check:** layers run across the tongue's bending direction under an outward push; compare with z-down (~1500 mm²) on a slice |
| Tilt shim | foot side down | ~1050 mm² | the seat recess roof needs support (or a 45° chamfered recess) |
| Pad shell | child side down, tree supports | no flat face | supports only on the hidden child side (as the headrest pad) |
| Clip ring | flat side down, tree supports | no flat face | thin curved strip |
