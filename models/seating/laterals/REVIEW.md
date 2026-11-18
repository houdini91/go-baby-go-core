# Side pads v1.1: remodel and independent reviews (2026-11-19, overnight)

## Why v1.1
The owner rejected the v1.0 Onshape model:
- layer lines on the arm and the pad base;
- a neck that narrowed to a snapping point;
- the release not integrated as sketched;
- busy, bulging joints;
- overhangs and gaps;
- not "one piece".

Root cause: the v1.0 arm route needed a ~5 mm bend for an 18 mm arm inside the chair width. The loft failed, and the code silently fell back to piecewise lofts (the "layer lines"). Pad curves were 60-segment polylines.

Rules now in force:
- one loft per shape;
- splines only;
- any failure is a visible error;
- no unapproved design changes;
- compare against the approved design with the wing in view before showing.

## Owner decisions (19 Nov)
- **Arm route:** knee first, then superseded by the flowing S route under the "one piece" brief.
- **Height:** 3 fixed positions × flip (comb later).
- **Release:** pill in the side of the arm.
- **Depth:** keep the dovetail idea, done as a hidden tenon in a sleeve.
- **Look:** one flowing piece, no gaps or overhangs, elegant pad mount, high-quality bar.

## Review rounds (independent agents: industrial designer, mechanical/DFM engineer, strength engineer)
| Round | Main findings | Fixed |
|---|---|---|
| 1 | Design: pad back read as a stack of plates; collar plus two seams at the joint; plunger-like flare; taper crease outside the mouth; badge-like pill; open slot. Mechanical: the shuttle could only move 1 mm; peg clashed with and couldn't pass the wall; spring had no seat; carbon rods were sealed voids in the wrong place; pad screw/nut unusable; rib needed a lead-in. Strength: the rib-in-notch carried pitch at SF 0.2–0.6; pill pocket at the root SF 0.8; tenon 1.1. | Rails replace the rib (lever 5.5 → 36 mm). Arm 18 × 28, grown inward. Latch channels, window and slot reworked. Pad screw reversed (head in the depth piece hollow, nut in a shell boss). Taper inside the slot. Flush clip ring. Tenon 11 × 20. |
| 2 | Design: pad edge still stepped; flare lopsided; two seams at the joint; pill distorted; countersink visible. Mechanical: the rod jammed at the spring bore (0.4 mm travel); the peg was the stop; hollow nearly broke through; carbon bores 0.65 mm apart; thin walls. Strength: root SF 1.4–2.0 along layers; tenon ~1 across layers. | Plain pad plate with the cover groove in its back face. Single-loft depth piece, 18 mm flare, foot within 3.5 mm of the axis. Square rod channel to t 15.4. Cap floor is the stop at 3.9. Groove floor +0.3. Rod channel inboard to d 10. Rails from t −12. Arm stays 16 wide past the mouth, then 18. Taper end 32 tall. Carbon Ø4/Ø3 extended past the mouth. Boss R 7. Key inside the flare. |
| 3 | Mechanical: latch kinematics **verified** (full release; cap floor is the stop at 3.9, release needs ~3.4; insertion OK). Remaining: two 0.44 mm walls against the rail groove; Ø4 bore grazing the pill pocket; old coupon script out of date. Strength: root fine printed on the outer tongue face (SF ~1.8–2.6 at the mouth, ~1.4 in the pocket zone); tenon ~1.0–1.5 across layers. Printing upright is worse (root flank shear SF ~1.0). | Rail 2.5 proud, so walls are ≥ 1.2. Shuttle base raised. Latch 0.5 mm inboard (pocket-zone SF est. ≥ 1.7). Ø4 rod ends at t −29.5. **Tenon 13 wide + 4 × Ø3 carbon pins across its root**, 12 mm into each side, carrying the joint bending. New **C6 latch coupon feature** cut from the real geometry. |

## Checks run in Onshape (real chair, branch `sidepads-v1`)
- The kit's "check all 24 settings" (LOW/HIGH × positions 1–3 × D0–D3, slot blanks included) reports **0 interference**, kit against kit and kit against chair, with **kit max x 163.0** (arm rests 165). It does not check motion or wall thickness; those were reviewed by reading the code (rounds 2–3).
- Every part builds as one solid; no finish step is skipped (skips now show as a feature warning).
- Visual pass on every view (iso, front, back, top, bottom, both sides, on the chair) for breakouts, slits and stray seams: the last slit (foot key) was fixed.

## Open before "ready to print"
1. **Carrier print orientation:** print it **on its outer tongue face** (d down, rail groove up; tree support under the first ~45° of the arm). Strength is SF ≥ 1.7–2.6 along the layers at the root. The tenon lies across the layers and is carried by the 4 carbon pins. The pinned tenon's SF is not yet recomputed: **verify with the load test.**
2. **Latch coupon first:** use the **C6 coupon** in the "Side pads v1" Part Studio (feature "GbG Side Pad latch coupon"). It is cut from the real geometry: tongue stub with the latch, pill button + rod, shuttle, and a wing-tip block. The old `latch_coupon.py` STLs (C1–C5) are outdated: do not print them. Test release force, snap-in over 100 cycles, and play.
3. **Spring:** OD ≤ 6, free length 11–12, solid length ≤ 5.2, as stiff as fits.
4. **Pad nut:** dab of glue in its child-face pocket (it is pushed out as the screw starts).
5. **Clip ring:** print flat and bend it into the groove, or confirm a supported print.
6. **Slot blanks:** modelled (option "Slot blanks", on by default). Two identical 32.6 mm push-in blanks per side ride the rails and close the unused slot at every position; their front is flush with the wing tip. Retention is by friction on the rail, so check the fit on the coupon.
7. **Deferred features:** tilt shims, ±6.25 fine height, the comb.
8. **Load test:** 300 N outward and 150 N down on a printed carrier before use; PT/OT fit check (pad top ≥ 25 mm below the armpit).


---

# Archive: v1.0 review (superseded)

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
| No clash between parts (checked on the real chair, not a stand-in) | Harness: all kit pairs + kit × chair. **39 settings** (LOW/HIGH, steps 0–7, depths N0/N12/N36, tilt none/8°/±15°, seat UP/MID/DOWN) | **pass.** Kit parts never clash with each other, with the wing, arm rests or back. Carrier max x 161.9 (arm rest 165); slot wall 2.08; screw tip inside its window in every setting; bolt free when pressed. The only clashes (HIGH, steps 0–2) are with `support_left_wing`, which is a modelled **print support** (see item 9) |
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
9. **The pad's back edge reaches `support_left/right_wing` in the HIGH kit at steps 0–2.** Those parts are 1.5 mm plates touching the wing along one 10 mm line: modelled print supports, not brackets (laterals/chair_interfaces.md). Every clash box in the matrix lies inside that plate (x 104.6–134.8, y 47.8–74.2, z 268.5–304.4). **Fix:** the wing feature removes them (option "Remove the modelled wing print supports", on by default), since the reprinted wing uses slicer supports. *Owner to confirm that the printed wings had them snapped off.* The 2 mm socket shift (item 6) was also made for this plate and could be undone.
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
- Matrix finished in the browser (batch mode: `REVIEW_BATCH` in harness.fs runs 7 settings per rebuild inside startFeature/abortFeature, with no per-setting REST calls; useful when the REST API is rate-limited).
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
