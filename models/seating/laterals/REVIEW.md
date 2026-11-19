# Round 5b fix pass (2026-11-19): designer / mech / strength reviews applied

Reviews: `viewer/review_r5/final/{designer,mech,strength}.md`. Owner decisions: do all of strength fixes 1-9; drop the Ø6 rod; pad screw from the child side with a plastic stop; remove everything of the old mount from the wing; do every designer and mech blocker; the 4 plan changes are accepted; grow into the old mount's envelope where strength needs it.

## What changed (r5 → r5b)
| Area | r5 | r5b |
|---|---|---|
| Tongue | 13.7 x 32.1 with latch channels through it (d' 7.9-17.45 above z 222.9), wedge 0.05 | **solid 15.8 x 32.1** (d' 2.0-17.8), 0.2 clearance all round (mech B1), back edge straight at t' 25.48 (6.25 from S1). Only a 4.4 x 5.2 bar channel along its top (open to the outer face, d' ≥ 8.8, z ≥ 241.2) and the tooth pocket at its rear (t' 16.7-21.1) |
| Engagement | inboard 16-19, **outboard 7.6** | **24.5 over the whole width** (t' 1 → 25.5) |
| Wing swell (old envelope) | - | the cheek's own section (cut live at t' 9) carried straight back to t' 26.5, fairing into the blade at t' 36; its end chamfers continue the cheek's. Slot walls ≥ 5.8 all round (outer 6.6, back corner 6.2) |
| Old-mount remnants | filled inside the band; recess behind the cheek above z 248 still open | rebuilt from the wing's own sections + the swell: pocket, slit, recess, M5 pockets, screw seats, boss. S1/S2, teardrop, hole column, z 186 hole untouched (swell ≥ 3.4 from S1) |
| Mouth chamfer | 3.3 all round | 3.3 inner/floor/ceiling, **0.5 outer** (tongue outer root R0.5 sits inside it) |
| Catch | tooth 22.5 x 5.0 at t' 4.5, shoulder 2.5 + 0.5 chamfer, skin 2.4, 2 hidden slits | **"deep catch"**: tooth 29.5 tall at the tongue's rear (t' 17-21), reach 2.4, solid (no slits), hooks a groove behind a **15 mm solid shoulder**, skin **4.0**. Pull-out: pocket rear wall (0.1) → tooth → groove face. Block in the nose (t' -4..1.2, 0.4 behind), flange pockets 0.5/side: neither takes pull-out (mech S4, K8) |
| Button | travel 4.9, 0.8 scoop, flat face | **travel 4.0** to a hard stop, **0.8 convex crown** (two crossing cylinders; rim proud ≤ 0.22 at the stadium's diagonals), rim R0.4, lip 12.6 x 24.6 (designer B1/B2), FL_W 12.8 |
| Assembly | drop-in from the nose top | same, plus a **one-way barb** above the flange (it cannot fall out) and a **split tell sleeve** (1.0 gap at the back, a pick takes it out: serviceable) |
| Spring | 6 coils | OD 6 x 0.55 wire, **8 coils**, L0 16, seated in a Ø6.5 x 1 floor counterbore; installed 11, pressed 7, solid ≈ 5.5 |
| Nose | seats on its back face (0.3 seam) | **3 stop pads Ø3** on the back face (built on the leaned notch face, 0.02 short), side crush ribs r 0.5 (0.3 interference) on both tongue faces, 2 top + 2 bottom |
| Blank cap | stub with a snap finger into the groove | stub with **2 + 2 crush ribs** and a **hidden nail notch** 8 x 1.2 x 1.0 under the nose's rear edge (designer B3) |
| Arm | 18 x 28, root R3.5 | **22 x 31** (u ≤ L/2) → 22 x 30 at the joint, grown inward (outer face kept: kit x max 163), **root R2.5** (R3.5 no longer fits on the long arm's nose face; R3 fitted the short arm only, so one value for both) |
| Tenon | 13 x 20, nut at x_e − 6.5, pins Ø2 x 21 | **16 x 24 R3, 15 long, nut slot at x_e − 9.5, pins Ø2 x 29** (13 in the tenon, 16 in the arm). Sleeve wall 2.8 |
| Ø6 rod | Ø6 x 35 in a teardrop bore | **gone** (bore, part, gauge slot, docs). Carbon = 4 x Ø2 x 29 pins only. Nothing stale remains: the "Ø6 x 39" in the part list was the LOW carrier's rod in an earlier r5 build (before one 35 mm length was set); the current part lists have no Ø6 part |
| Depth piece | foot 27 x 40 R14.5, lip 2.85 | **foot 27 x 48 R10** (dovetail whole, outer lip ≥ 4), flare 22. Nut pockets AF 8.0 + 2 crush ribs 0.15 (designer S2) |
| Pad mount | screw from the child side | + **0.4 snap bump** on the rail (y 44.3-45.8) into a notch in the groove floor: the plastic takes +y, the screw only locks |
| Print | button standing | button on its inner face (d' 13 plane shared by block, bar and tooth) |

Not done (cheap enough to skip, or open): engraved L▼/L▲ orientation marks (designer P-item); 0.6 x 0.6 relief groove at the tongue's outer root (the R0.5 sits inside the 0.5 chamfer instead); R1 at the tooth root (the tooth root is not the limit, see below); a fillet where the swell's back meets the blade (a crease, ~45°).

## Checks run in Onshape (branch `sidepads-v1`, real chair)
- **Clash matrix: left 0/40, right 0/40** (arm S/L x LOW/HIGH x pad upper/lower x tilt 0/±8/±15; kit vs kit, kit vs every chair part, pressed button vs chair). **Kit |x| max 163 (arm rest 165).** The crush ribs are left out (deliberate 0.3 interference).
- Part Studio 1 builds with **no warnings**: tongue root R3 (outer R0.5), arm root R2.5, button rim R0.4.
- Offline (fine tessellation, fitted left side): the only carrier ↔ wing overlaps are the crush ribs. STLs of the final build are in `viewer/r5cad/` (`fitted/` in chair coordinates with the wings, `print_layout/`, `all_parts/` with every variant; Onshape tessellations at 0.02-0.05 mm, a few not watertight at seams; for measuring, not printing).

## Strength (honest numbers; builder's check with the strength reviewer's method)
Script: `viewer/review_r5/cad/strength_r5b/sf_r5b.py` (output `out_zero.txt` seated, `out_cad.txt` with CAD gaps). It reuses the reviewer's contact model (`final/strength_work/loads.py`: rigid carrier on unilateral Winkler springs, K 400 N/mm³, all 8 configs S/L x LOW/HIGH x pad ±11, worst one kept), the same pad load points and the same allowables (PETG: 40 MPa along the layers, 18 across, 22 shear, 25 bearing). New: the r5b contact patches, the tongue section at the mouth with its real moments **including torsion about the tongue axis**, analytic 22 x 31 arm sections, the 16 x 24 tenon with 4 pins (16 mm bond). Seated ("zero gap") numbers:

| Item (SF = allowable / stress) | lean 300 centre | corner knock 300 (out) | lean 600 | corner 900 | load at SF 1 |
|---|---|---|---|---|---|
| Carrier tongue at the mouth, with Kt 1.8 (outer R0.5) / 1.3 | **0.95** | **0.65** | 0.47 | 0.22 | ≈ 285 N lean, **≈ 195 N corner** |
| same, Kt 1 (nominal) | 1.49 | 1.01 | 0.74 | 0.34 | ≈ 450 N lean, ≈ 300 N corner |
| Wing slot outer wall, peak bearing (Winkler peak at the mouth edge) | 0.80 | 0.57 | 0.40 | 0.19 | ≈ 240 / **170 N** (local indent) |
| Wing slot outer wall, mean bearing | 1.77 | 1.27 | 0.89 | 0.42 | ≈ 530 / 380 N |
| Tooth bearing (2.4 x 29.5) | 1.8 | 1.3 | 0.9 | 0.4 | ≈ 540 / 390 N |
| Wing shoulder in front of the groove (15 x 30.6 shear) | 10 | 7.6 | 5.2 | 2.5 | > 2 kN |
| Tongue pocket rear wall (shear) | 3.9 | 2.8 | 1.9 | 0.9 | ≈ 840 N corner |
| Arm (22 x 31, worst section, Kt 1.55 at the root) | 1.90 | 1.24 | 0.95 | 0.41 | ≈ 570 / 370 N |
| Tenon root (pins bond-limited) | 3.55 | 1.36 | 1.77 | 0.35 | ≈ 1070 / **410 N** |
| Tenon at the nut slot | 2.87 | 1.16 | 1.43 | 0.39 | ≈ 860 / **350 N** |

What the old envelope bought (same method family; r5 numbers from the strength review): outboard engagement 7.6 → 24.5; outer-wall contact 34 → 500 mm² (mean-bearing SF at 300 N lean 0.36 → 1.77); tongue yaw section modulus 355 → 1169 mm³ (x 3.3); catch shoulder 2.5 → 15 mm and skin 2.4 → 4.0 (shoulder SF at 300 N 0.34 → 7.6; the tooth root is no longer a limit).

**Conclusion, stated plainly:**
- **SF ≥ 3 at 300 N is not reachable with one tongue in the blade edge**, even solid, widened, 24.5 deep and with the swell. The limit is the mount's yaw + torsion moment (≈ 19 N·m yaw and 15 N·m twist at 300 N lean; 28 / 23 N·m at a 300 N corner knock) on a 15.8 mm wide tongue whose walls are fixed by S1 and the blade faces. Real SF at 300 N: **≈ 1.0-1.5 (lean), 0.65-1.0 (corner knock)**. The strength review's 3.5-4.4 estimate for fix 8 left out the torsion and used smaller root moments.
- **The tenon is a defined, reprintable weak point at ≈ 350 N (nut slot) to 410 N (root) in a corner knock**, but it is **not** the first thing to give: the carrier tongue (also reprintable) reaches SF 1 first in corner knocks (≈ 200-300 N), and the wing's slot mouth starts to dent locally (peak bearing) from ≈ 170 N corner / 240 N lean. Mean bearing on the wing wall (≈ 380 N corner) and the wing shoulder (> 2 kN) stay above the fuse. So nothing in the wing *breaks* before the fuse, but the wing can *dent* at the mouth before the carrier or the fuse fails. A tenon fuse cannot protect the mount: its lever is always shorter than the mount's, and making it weak enough would fail the 300 N lean.
- To get SF 3 at 300 N the mount needs a second load path (strength review option (a): a stay from the depth-piece foot to the seat or arm rest), or the saddle clamp the owner rejected. **Owner decision needed.**

## Plan changes (all four accepted by the owner, 2026-11-19): AGREED
1. **4 carriers per arm length (no flip pair).** AR-{L,R}-{S,L}-{LO,HI}. AGREED.
2. **Button drops in from the nose top** (opening hidden in the notch); the split orange sleeve snaps in under the lip and holds the flange; serviceable with a pick. AGREED.
3. **Ø6 rod** (was: below the latch, 35 long): AGREED as changed — **dropped entirely in r5b**; carbon = 4 Ø2 x 29 pins.
4. **Pad screw from the child side** (M5 x 10 button head under the foam, nut in the foot), now with a plastic +y snap stop. AGREED.

## Open items
- **Owner: second load path or accept SF ≈ 1-1.5 at 300 N lean** (see above). Then load tests 5.3/5.4 on a printed carrier decide.
- Coupon C7 first: drop-in past the barb, sleeve snap and pick-out, click and cam-in of the 45° lead-in, 6° face self-holding, crush-rib feel (0.3), the rail's snap bump, the nut crush ribs.
- The button crown has a shallow X-crease (two crossing cylinders) and its rim stands ≤ 0.22 proud at the stadium diagonals; a lofted dome would remove both.
- The swell's back meets the blade in a ~45° concave crease (no fillet); the swell's lower chamfer ends against the blade behind the cheek. Look at it in Onshape before printing the wing.
- Clip ring and sleeve carry no engraving (too thin). The wing part keeps its Onshape name (left_wing/right_wing).

---

# Round 5 CAD (2026-11-19): what was built and checked

Built from ROUND5_PLAN.md plus owner decisions given during the build: close the old pocket; make the joint beefier, printed only (steel dowel optional); add the joint tricks: pads, root fillets, wedge, crush ribs, slicer modifiers.

## Checks run in Onshape (branch `sidepads-v1`, real chair)
- **Clash matrix:** arm S/L x LOW/HIGH x pad upper/lower x tilt 0/±8/±15 = 40 settings per side, both sides. Each setting checks kit vs kit and kit vs every chair part, plus the pressed button (travel 4.9) vs the chair. Result: **left 0/40, right 0/40 clashes. Kit |x| max 163 (arm rest 165).** The crush ribs are left out of the check; they are a deliberate 0.2 interference.
- Fitted Part Studio 1 builds with **no warnings**: arm root R3.5 and tongue root R3 fillets applied, button rim 0.4.
- Offline cross-check on Onshape's fine tessellation (chord 0.01 mm): the only carrier ↔ wing overlaps are the 4 crush ribs.
- Rod Ø6 bore wall (measured on the built carrier): ≥ 1.2 in the tongue and nose, ≥ 3.2 in the arm root.

## Joint strength (r5): withdrawn
The r5 strength table that stood here was wrong and over-optimistic (it misread the load case, ignored the mouth chamfer behind the tooth, the slits' effect on the tooth root, the shoulder bending and the skin). The independent check (`viewer/review_r5/final/strength.md`) found SF 0.34-0.46 at 300 N for the r5 catch. See the Round 5b section above for the rebuilt joint and honest numbers.

## Deviations from the plan (r5; items 1-4 accepted by the owner, see Round 5b)
1. **4 carriers per arm length (no flip pair).** The leaned nose cannot be z-mirrored (2.3 / 2.8 mm off at the notch ends). AR-{L,R}-{S,L}-{LO,HI}. A child set still needs 2 carriers.
2. **Latch A assembly:** the button drops in from the nose's top face (an opening hidden in the notch). The orange sleeve snaps in under the lip and holds the button's flange. A one-piece button could not be put in from outside with load-bearing rear walls in the tongue. Release is unchanged (press, pull forward). Travel 4.9 (plan 5, fig2 3.5).
3. **The Ø6 rod runs below the latch** (z 218.75), from the tongue's rear face to t' -22, 35 mm long. It reaches about 8 mm past the nose face; the HIGH arm rises, so a longer rod would break out. Pins Ø2 x 21.
4. **Pad screw from the child side:** M5 x 10 button head, under the foam, into a nut dropped into the foot's dovetail groove. With two pad positions, both nuts can then hide inside the foot. The pad has one rail; the foot has two grooves (±11), closed at the front. The unused groove's open back end shows on the foot's back face.
5. **One button part per side** (LO/HI share it). One pad shell and one clip ring for both sides.
6. Button face: a flat leaned plane flush with the nose's outer face, not the wing skin itself (the skin there is split into small faces).
7. Mouth chamfer 3.3 x 45° all round the slot (lead-in; clears the R3 tongue root).

## Open items
- **Coupon first** (Side pads r5 - kit: the C7 latch coupon): snap of the tell sleeve past the lip (0.3/side), drop-in assembly, click, 6° face self-holding dry/PTFE, wedge seating, crush-rib feel, 2.4 mm skin behind the groove.
- Engagement ≥ 30: needs a visible local swell of the blade, or moving S1. Owner to decide.
- Clip ring and sleeve carry no engraving (too thin).
- The wing part keeps its Onshape name (left_wing/right_wing). The part number WG-L/R is engraved and listed.
- The old "Side pads v1" Part Studio now errors (kit without chair). The old v1 coupon feature is retired.
- Load tests 5.3/5.4 and the child fit (5.11) as planned.

---

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
| 3 | Mechanical: latch kinematics **verified** (full release; cap floor is the stop at 3.9, release needs ~3.4; insertion OK). Remaining: two 0.44 mm walls against the rail groove; Ø4 bore grazing the pill pocket; old coupon script out of date. Strength: root fine printed on the outer tongue face (SF ~1.8–2.6 at the mouth, ~1.4 in the pocket zone); tenon ~1.0–1.5 across layers. Printing upright is worse (root flank shear SF ~1.0). | Rail 2.5 proud, so walls are ≥ 1.2. Shuttle base raised. Latch 0.5 mm inboard (pocket-zone SF est. ≥ 1.7). Ø4 rod ends at t −29.5. **Tenon 13 wide + 4 × Ø2 carbon pins across its root** (8 mm into the arm; ≥1.45 mm walls), carrying the joint bending. Final checks: strength "OK for test print + load test" (pins bonded with **high-Tg epoxy**, Tg > 80 °C; load-test to 600 N or 300 N at 60 °C, watch the pocket edge at u 12–15 and the tenon root); mechanical "OK to print the C6 coupon" (separate the 4 coupon parts in the slicer; print the block on its cut bottom face). New **C6 latch coupon feature** cut from the real geometry. |

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
