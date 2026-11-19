# Side pads Round 5: industrial-design review of the built CAD

Reviewer: designer. Date: 2026-11-19. Branch `laterals-v1` (read-only; nothing committed, no CAD or doc edits).

**Inputs read:**
- `ROUND5_PLAN.md`;
- `REVIEW.md` (the "Round 5 CAD" section);
- `featurescript/gbg_sidepad_r5.fs` (latch, wing tools, carrier, depth piece and pad read in full);
- `viewer/review_r5/cad/PROGRESS.md` and `PRINT_GUIDE.md`;
- every image in `viewer/review_r5/cad/shots/` (01–10) and `viewer/r5mesh/renders/` (0–10);
- `button_design/fig2_favourite_spec.png`.

**Caveat:** the `r5mesh` STLs and renders come from the earlier mesh study (tongue z 214.3–239.7, old pocket above the nose still open, tongue 25 tall). They are not the final build, and the final build has no STL export. Where the two differ, my numbers come from the FS constants. Where I needed geometry, I took sections of the original wing mesh (`chair_left_wing.stl`) in the FS's S frame.

**Renders and scripts** are in `viewer/review_r5/final/designer_renders/`:

| File | Shows |
|---|---|
| `D1_latch_plan_section_z235.png` (`latch_section.py`) | Slot, mouth chamfer, catch groove, tooth and blank-cap bump over the real wing section |
| `D2_foot_face_grooves.png` (`foot_face.py`) | Depth-piece foot outline vs the two dovetail grooves |
| `D3_button_face_and_stroke.png` (`button_face.py`) | Walls round the button stack, and the release stroke vs thumb reach |

## Verdict

**The look is close.** Seen from the parent's side, the nose reads as the wing's own front edge. The two hairlines at z 207 and z 247 are the only tell, and the S-arm grows out of the nose with a clean R3.5 root (shots 01, 02, 10). There is no block, socket or rail any more, so the owner's "completes the chair" goal is met for the fitted state.

**Not ready to print, for 3 reasons.** These are function problems that the owner will feel, not see:
- the latch seat in the wing;
- the release stroke vs a thumb;
- the blank cap.

**One more needs a confirming look in Onshape:** the dovetail grooves on the depth-piece foot.

None of these is caught by the 0/80 clash check, because they are wall, stroke and strain problems, not overlaps.

---

## Findings

### Blockers

#### B1. Wing (WG): the latch seat ("shoulder") in front of the catch groove is too thin and is loaded in bending

**What is wrong:**
- The shoulder is the wing material between the notch face (t' 2.0) and the groove's 6° face (t' 4.5). It carries the whole pull-out load of the pad: 688–901 N in the corner-knock case.
- It is a ledge 2.5 mm thick at the slot side and 3.0 mm at its root (d' 22.2), 23 mm long. Nothing is behind it: the groove sits there.
- So it bends like a cantilever: 6·F·e / (L·t²) with e ≈ 2.0 mm gives **≈ 52–74 MPa at 900 N** (≈ 17–25 MPa at 300 N). PETG along the layers is ≈ 50 MPa, so SF ≈ 0.7–1.0, and lower across the layers.
- REVIEW.md checked this part only in shear (11.8 MPa, "SF 2.1").
- On top of that, deviation 7 (the 3.3 × 45° mouth chamfer on the **outer** wall) runs to t' 5.3, which is past the groove face. For its first 1.3 mm the shoulder is a knife edge: 0.08 mm thick at d' 18.45, 0.7 at d' 19, 1.8 at d' 20 (D1).
- The wing is the most expensive part to reprint, and a crushed shoulder means the pad can come out.

**Fix** (the latch package, together with B2):
- Move the catch groove back 2 mm: `GR_T` 4.5 → **6.5**, groove to t' 11.0. The block and tooth move back with it: `BLK_T` −4..9 → **−2..11**. Keep ≥ 3 mm of the outer wedge face (t' 3–6.5) and ≥ 3 mm to the cheek's back (t' ≈ 14.3).
  - Shoulder: 4.5 thick → ≈ **19 MPa at 900 N (SF ≈ 2.7)**.
- Outer side only: mouth chamfer 3.3 → **1.0 × 45°**, with the tongue-root fillet on that edge R3 → **R0.8** (`tgRo`). Keep the 3.3 lead-in on the inner side, top and bottom.
- Pull the groove bottom in: `GR_D` 22.2 → **21.2**. This also thickens the skin behind the groove from 2.4 to **3.4**, which was an open coupon item.

Ask the strength reviewer to confirm.

#### B2. Button (BT) + carrier: the stroke needed to release is more than a thumb can follow into the opening

**What is wrong:**
- The tooth reaches 4.35 mm into the groove (d' 17.65 → 22.0), so the button must sink ≈ 3.6 mm (as built, with the chamfer camming the rest) up to 4.4 mm before the carrier can be pulled. The hard stop is at 4.9 (the plan said 5; fig2 said 3.5).
- The button is flush, inside a 13.2 × 25.2 lip opening. An adult thumb pad is 16–20 mm wide and typically follows only ≈ 2.5–3.5 mm into a 13 mm opening (D3).
- The 0.8 thumb scoop adds 0.8 more at the centre.
- A wedge seated by the latch (crush ribs plus zero-clearance wedge) also preloads the 6° face, so the press force rises just when the parent is also pulling forward.
- **Risk:** parents need a key or a pen to release it. That fails "big, easy to press".

**Fix** (with B1, same coupon):
- Tooth reach 4.35 → **3.35** (`TO_D[1]` 22.0 → 21.0, `GR_D` 21.2). The bearing on the shoulder is then 18.15–21.0 = 2.85 × 22.5 = 64 mm², ≈ 14 MPa (SF ≈ 1.8 vs 25 MPa).
- Hard stop **4.0**.
- Replace the 0.8 concave scoop with a **0.8 convex crown**. The rim stays flush, so flush = latched still reads true, and the thumb gains 0.8.
- The thumb then needs to follow only ≈ 2.6 mm.
- **Coupon test (add to 5.5):** full release with a bare thumb by 5 adults including small hands. Release press ≤ 15 N, measured with the wedge seated **and** with 30 N of forward pull applied.

#### B3. Blank cap (CP): it snaps in by yielding, its bump plows the latch seat, and it cannot be taken out

**What is wrong** (D1, green):
- **Strain on the way in:** the snap finger is 2.05 thick and 6.4 long, freed by the 1.8 relief. Its bump stands to d' 19.0. To pass the shoulder it must deflect ≈ 0.55 mm, a bending strain of **≈ 4–16 %** depending on the effective lever. PETG yields at ≈ 4 %, so the finger cracks or takes a set.
- **Damage to the wing:** on the way in, the bump drags over the same knife-edge shoulder the carrier tooth needs (B1).
- **No way out:** its retaining face is 12.5° off square. It self-locks for any μ > 0.22 (PETG on PETG is ≈ 0.3–0.5). The cap is flush with nothing to grip, so it cannot be removed without breaking it or the wing.
- This is the part the owner sees every time the kit is off.

**Fix:**
- Delete the snap finger, the relief and the bump.
- Retain the cap like the carrier tongue: **2 + 2 crush ribs** (R0.4, 0.2 interference) on the stub's top and bottom faces.
- Add a hidden **8 × 1.2 × 1.0 fingernail notch** in the cap's bottom face at the front edge (z 207.3, t' −14..−6), invisible from above.
- Target: pull-out 10–20 N by hand. Check it on the C7 coupon.

#### B4. Depth piece (DP): the ±11 dovetail grooves run out of the foot's round top and bottom (confirm in Onshape)

**What is wrong** (D2):
- The foot is lofted from `sec(w 27, h 40, r 14.5)`. Because r ≥ w/2, `rrProfile` makes it a **stadium**: semicircular top and bottom, r 13.5, centred at z ±6.5.
- The grooves sit at z ±11 and are 12.4 wide at full depth (z 4.8–17.2).
- Result:
  - **Dovetail flank:** the full flank (≥ 1.2 mm wall) exists only from y 26.2 to 38.8, i.e. **12.7 of the 24 mm**. Elsewhere the groove breaks out through the round top and bottom of the foot.
  - **Closed front stop:** it covers the rail end only up to |z| 15.0, so about 60 % of the rail end bears.
- This undoes plan fix 2.24a ("plastic is the stop, the screw only locks"): an outward push on the pad goes back into the M5 pad screw. It also leaves open scallops on top of the foot, which is exactly where a parent looks down. The white notches at the right of shot 03 are consistent with this.
- **Confirm** with a section at the pad-back plane in Onshape before acting.

**Fix:**
- Foot section **27 × 48 with R10 corners** (`FOOT_HZ` 20 → 24, `FOOT_R` 14.5 → 10). This gives a full flank from y 22.0 to 44.9 and a closed front that covers the whole rail end (|z| up to 21).
- Lengthen `FLARE_L` 18 → 22 so the trumpet still reads as one smooth flare from the 18 × 28 arm.
- Also run a min-wall check (≥ 1.2) on the nut pockets and the pad-screw hole at all 5 tilts. Rotating the pad frame by 15° moves the upper pocket ≈ 2.8 mm outward into the narrower trumpet.

### Should-fix

#### S1. Tell sleeve (TS): orange shows while latched

- **What is wrong:** the lip opening is 13.2 but the sleeve ID is 12.4. A 0.4 mm orange band lies 1.6 mm below the face, right under the 0.6 gap round the button. The r5mesh renders 4 and 7 show these orange slivers in the latched state, which contradicts "orange only when NOT latched".
- **Fix:**
  - Lip opening **12.6 × 24.6** (gap 0.3 per side; the sleeve ID stays 12.4, so the lip covers it).
  - **0.3 × 45° chamfer** on the sleeve's inner top edge, so no orange face squares to the eye.
  - Check: no orange visible at 30° off-axis when latched.

#### S2. Carrier (AR): walls round the button stack are 1.0–1.3 mm

- **What is wrong** (D3):
  - The sleeve counterbore is 14.0 wide (t' −13.3..0.7). It leaves **0.97 mm** to the nose front and **1.0 mm** to the seam, over 26 mm of height.
  - The flange pocket and drop-in channel (13.4) leave 1.27 / 1.30.
  - This is the corner the thumb pushes on, and on the front side it is where the arm-root load enters.
- **Fix:**
  - Sleeve wall 0.6 → 0.5, OD 13.4, counterbore **13.6**.
  - Flange `FL_W` 13.0 → 12.8, pocket **13.2**.
  - That gives walls ≥ 1.2, i.e. 3 perimeters at 0.4.
- **For strength:** the latch channels (bch/tch/cch/spr all run up to the nose top) hollow out about the outer 60 % of the nose above z 223. Recheck the arm-root SF on the real section, not the v1.1 one.

#### S3. Tell sleeve (TS): retention

- **What is wrong:** the sleeve is held by a 0.3/side snap past a 1.6 lip, and the 6.5 N spring loads it outward forever, at car heat. If it creeps out, the button and the OD 6 spring become loose small parts: a choke hazard.
- **Fix:**
  - 45° lead-in on the sleeve's bottom outer edge, **square (90°) retaining shoulder** on top, square lip underside, 0.4 interference.
  - A drop of CA glue in the assembly order.
  - Coupon pull-out ≥ 50 N.

#### S4. Depth piece: M5 nuts are not captive

- **What is wrong:** the nuts drop into an AF 8.3 pocket and fall out whenever the pad is off.
- **Fix:** pocket **AF 8.0 with 2 × 0.15 crush ribs** (press the nut in once), for both pad-nut pockets. The tenon nut's side-entry slot gets the same treatment.

#### S5. Pad (PD) + foam: changing the pad height means stripping the cushion

- **What is wrong:** the M5 × 10 head sits under the foam and cover (deviation 4).
- **Fix:**
  - **Ø12 access hole in the foam** at each screw position (y 32.5, z ±11 about the pad centre). The cover hides it, and a hex key goes through the peeled-back cover.
  - Add a line to the print guide.
  - With the 5 mm foam option, also check that the Ø9.5 head (0.25 below the shell face) is not felt.

#### S6. Thumb room vs the car's side wall (OPEN 4.4)

- **What is wrong:** the button faces along d, which is (0.80, 0.61, 0): outboard **and ~37° rearward**. Kit |x| max is 163 against the arm rest at 165.
- **Fix:** before printing, measure in the car that there is ≥ 30 mm clear along d in front of the button and room for a hand to grip the arm root from the front.

#### S7. Blank cap: no stow (plan 3.7, pending 4.6)

- **What is wrong:** with the kit on, the cap is a loose part.
- **Fix** (if the owner keeps the cap): a hidden **Ø3 lanyard hole** through the stub tongue, tied with cord through S1 or stored in the kit bag. Owner's call with 4.6.

### Polish

#### P1. Pad back reads as stacked rings

- **What is wrong:** shell edge, cover edge, groove and clip ring give 3–4 concentric outlines (shot 02). This is the "stack of plates" note from round 1 again.
- **Fix:** end the cover wrap inside the groove so only the ring's outer edge shows, and print the ring in the shell colour.

#### P2. Sharp vertical edges at hand height

- **What is wrong:**
  - The nose copies the wing's front-outer edge, which is near-square (D1 section).
  - The notch's front horizontal edges (z 207 / 247) are a shear edge when the nose slides in.
- **Fix:**
  - R1.0 on the wing reprint's front-outer and front-inner vertical edges over their full height, so the nose and cap inherit it.
  - R0.5 on the notch's front horizontal edges. The hairline look is unchanged.

#### P3. Foot back face

- **What is wrong** (shot 03):
  - The M5 countersink is a cone on a curved surface, so it leaves a crescent crater.
  - The unused groove's open end shows.
- **Fix:**
  - A flat **Ø11 spot-face, 0.6 deep**, for the countersunk head.
  - 0.6 chamfer round the groove mouths.

#### P4. Telling the 4 carriers apart

- **What is wrong:** the variant code is engraved on the nose's bottom face, which is hidden.
- **Fix:** emboss an **"L▼" / "L▲" / "R▼" / "R▲"** (side + pad below/above) 0.3 high on the tongue's top face. It is seen when the carrier is in your hand and hidden when it is fitted.

#### P5. Child access

- **What is wrong:** press-and-pull with one hand round the outside of the wing is hard for a 2–4 y child but possible for a 5 y child.
- **Fix:** none needed if 6.5 N plus pull-forward stays. Note it for the ergonomics reviewer and the child-fit test (5.11).

---

## Deviations from the plan: design and usability verdicts

| # | Deviation | Verdict |
|---|---|---|
| 1 | 4 carriers per arm length (L/R × LO/HI), no flip pair | **Accept.** The 2.3 / 2.8 mm mismatch at the notch ends would show as a step in the seam, which is worse than one more SKU. A child still uses 2 carriers. Wrong-side fitting is prevented by the mirrored tongue. Add the visible handedness mark (P4). |
| 2 | Button drops in from the nose top (opening hidden in the notch); sleeve snaps under the lip; travel 4.9 | **Accept the drop-in** (invisible when fitted, keeps the tongue's rear walls). **Do not accept travel 4.9** (B2: go to 4.0 with tooth reach 3.35) or the sleeve retention as drawn (S3). A wrong-hand button cannot drop in, because its block would extend down to z 208 below the channel floor at z 223.2. That is good poka-yoke; confirm it on the coupon. |
| 3 | Ø6 rod below the latch (z 218.75), 35 mm, only ≈ 8 mm into the arm | **Accept for looks** (invisible). **Flag for strength:** the rod now ends right where the R3.5 root fillet ends, which concentrates stress at the arm root. The plan ran it toward t' −29. If strength needs more, a second short Ø6 rod in the HIGH and LOW arms' own straight run is invisible too. |
| 4 | Pad screw from the child side into a nut in the foot's groove | **Accept, with conditions.** The tip points away from the child: it ends at x_b + 8.5, inside the Ø5.5 hole that stops at x_b + 9, so it cannot protrude. Both nuts are hidden. Conditions: B4 (grooves inside the foot), S4 (captive nuts), S5 (foam access), P3 (groove mouths). |
| 5 | One button per side; one pad shell and one clip ring for both sides | **Accept.** Fewer parts to mix up, and the shared pad and ring are a plus. |
| 6 | Button face is a flat leaned plane flush with the nose, not the wing skin | **Accept.** The original wing's outer face at the nose measures d' 24.48–24.62 over z 215–245 (from sections of `chair_left_wing.stl`), so the flat face is flush within ±0.07 everywhere. That is invisible, and it keeps "flush = latched" honest. |
| 7 | Mouth chamfer 3.3 × 45° all round the slot | **Accept on the inner side, top and bottom** (a good lead-in, hidden behind the nose). **Reject on the outer, latch side:** it runs past the groove face and knife-edges the latch seat (B1). Use 1.0 × 45° there with an R0.8 tongue-root edge. |

## What works (keep)

- The nose is the wing's own section with 0.3 hairlines. Fitted, the mount disappears, and the arm root R3.5 is smooth (shot 02).
- The button is symmetric on the nose (centre t' −6.3 = nose centre −6.29; z 227 = nose centre), 12 × 24, and square to its push. The latch holds by shape, not by the spring.
- Captive button through the sleeve; wrong-side carriers cannot fit; the pad screw tip faces away from the child.
- The old pocket above the nose is closed (the r5mesh renders still show it open), and the dead M5 pocket is filled inside the band.
- 0/40 + 0/40 clashes and |x| 163 against the arm rest at 165.

## Suggested order

1. Put the B1 + B2 latch package and B3 cap retention into the **C7 coupon**. Print two tooth variants (as built vs reach 3.35 / groove 6.5 / chamfer 1.0) and test thumb release, press force and pull-out.
2. Confirm B4 with an Onshape section, then change the foot to 27 × 48 R10.
3. Apply S1–S4 in the FS, and re-run the clash matrix plus a **min-wall check** (≥ 1.2) on the carrier, the depth piece and the wing near the latch. The clash check alone cannot see B1, B2, B3 or B4.
