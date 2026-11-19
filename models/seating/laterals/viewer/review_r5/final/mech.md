# Side pads r5: mechanical review (latch A, assembly, tolerances)

Reviewer: mechanical / FDM snap-fit. Date 2026-11-19. Branch `laterals-v1` (read-only).
Sources: `ROUND5_PLAN.md`, `REVIEW.md` ("Round 5 CAD"), `featurescript/gbg_sidepad_r5.fs` (the geometry of record), shots in `viewer/review_r5/cad/shots/`, the original wing mesh (`chair_mesh/chair_left_wing.stl`, repaired with `r5mesh/wingfix.py`).
The `r5mesh/*.stl` kit parts are the earlier mesh study: tongue z 214.3–239.7, groove 3.5 deep, travel 5.0. They are **not** the FS build, so kit numbers below come from the FS constants. The mesh was used only for the wing (S1, walls, fills).
Work files: `mech_work/` (`s_sections.py`, `s_walls.py`, `s_web.py`, `s_plot.py`, `wing_sections_S.json`, `walls_S.json`, `section_slot_S1.png`).

Frame: S (d' + outboard, t' + into the wing, lean removed), mm. Verification tags: **FS** = read in the FS; **calc** = computed from FS numbers; **mesh** = measured on the wing mesh.

---

## Blockers

### B1. Tongue wedge faces at 0.05/side: the stack cannot be printed, and the latch may not engage
- **Part:** WG slot + AR tongue (`SLOT` / `TONGUE`, FS l.48–50).
- **Problem:** The 1.5°/side wedge faces have a 0.05 normal gap: inner t' 8.5–18.5, outer t' 3–9.3. The project's own tolerance block says sliding fits need ≥ 0.2/side.
  - On a 1.5° taper, every 0.01 of width error per side moves the seating point by 0.01 / tan 1.5° = **0.38 mm** axially.
  - The latch only engages inside a narrow axial window. The tooth tip is d' 22.0 and the shoulder tip is d' 18.4, so the 6° face can absorb only (22.0 − 18.4) × tan 6° = **0.38 mm**. The nose stop allows **+0.3**. The window is therefore about −0.3…+0.38 mm.
  - So the latch clicks only if the combined width error is below about **+0.04 mm per side** (0.05 gap less 0.3 × tan 1.5°).
  - FDM slots shrink and bosses grow by 0.05–0.2 per side. The likely outcome is a wedge that seats 2–6 mm early. The tooth then stays on the shoulder with the orange showing, and the carer forces the self-locking 1.5° taper (it self-locks above μ 0.03) with ≥ 50 N.
  - If the slot prints large instead, the wedge never touches. In CAD it never touches either: the nose stop at +0.3 comes before wedge contact at +1.9.
- **Fix:**
  1. Open the wedge faces to **0.20/side** (SLOT inner [3.55, 8.5]→[3.81, 18.5]; outer [17.65, 3.0]→[17.485, 9.3]). This makes the nose the axial datum.
  2. Add **3 stop pads, Ø3 × 0.3**, on the nose back face (t' 1.7→2.0, hidden). The visible seam stays 0.3 and the stop is a defined plastic face.
  3. Get lateral zero-play from crush ribs instead: **2 per side** on the tongue inner and outer faces at t' 15/17, r 0.5, 0.3 interference, like the existing top/bottom ribs.
  4. Prove it on coupon C7 with slot widths 14.1 / 14.3 / 14.5.
- **Verified:** FS (polygon coordinates), calc.

---

## Should-fix

### S1. Zero rattle is not achieved in CAD, and "flush = latched" conflicts with the taper
- **Part:** BT tooth vs WG groove (FS l.677–680).
- **Problem:** The tooth front is set 0.02 *behind* the groove face, so the button reaches its outer stop (flange on sleeve) before the 6° faces touch.
  - Nominal axial play is 0.02 + 0.3 (nose seam) = **0.32 mm**, and the 6° face never preloads.
  - If you add preload instead, every 0.1 mm of axial preload sinks the cap by 0.1 / tan 6° = **0.95 mm**. Tolerance would then show as a sunken cap or a false "not latched" orange.
  - The spring can drive the cam only against ≈ F_s / (tan 6° + μ) ≈ **2.8 × F_s ≈ 10–14 N** of carrier friction (μ 0.25), not the ~30 N an ideal taper suggests.
- **Fix:** Accept up to 0.3 axial play: the plan's wobble limit of 0.5 at 60 mm is still met. Pull the carrier home onto the B1 stop pads, and use the crush ribs for rattle.
  - If preload is wanted, set the tooth front **0.15 ahead** of the groove face at the nose-stop position.
  - Then lower the sleeve top to **FD − 3.0**, so a sink of up to about 2.8 mm shows no orange and the 4.5 mm "not latched" sink still shows about 1.5 mm.
- **Verified:** FS, calc.

### S2. Pull-out load path: the 0.6 flange is hit before the block
- **Part:** BT flange / AR channels.
- **Problem:** When the carrier is pulled forward, the button is pushed by whichever carrier wall it meets first.
  - The flange (13.0 wide, t' −12.8…0.2) has **0.2** to the `fp`/`cch` rear wall (t' 0.4).
  - The block (rear face t' 9.0) has **0.3** to the `bch` rear wall (t' 9.3).
  - So the 0.6-thick flange edge meets a 1.3-thick nose skin (t' 0.4–1.7) first: about 8–15 mm² carries the 900 N. Only after crushing does the block bear on its 94.5 mm².
- **Fix:** `bch` rear wall to BLK_T[1] + **0.15** (t' 9.15). Flange pockets `fp`/`cch` to FL_W + **1.0** (0.5/side). The cap-to-lip gap stays 0.6.
- **Verified:** FS l.687–693, calc.

### S3. Catch shoulder is a cantilever ledge, and REVIEW's latch numbers are stale
- **Part:** WG groove shoulder, BT tooth.
- **Problem:** The 3.3 × 45° mouth chamfer (deviation 7) runs to t' 5.3, past the groove front (t' 4.0–4.98), on the outer side too. The shoulder becomes a triangle with its tip at about (d' 18.4, t' 4.55); see the plot. Recomputed at 900 N:

| Item | REVIEW | Recomputed |
|---|---|---|
| Bearing area | 4.05 × 22.5 = 91 mm² | 3.6 × 22.5 = **81 mm²** → 11 MPa (OK) |
| Shoulder, shear only | 11.8 MPa | 13.4 MPa |
| Shoulder root bending (root at the groove bottom d' 22.2, thickness 2.98, lever 2.0) | not checked | **≈ 54 MPa** (cantilever estimate; the z-end support lowers it by about 10–20 %) vs ~50 MPa PETG |
| Tooth root (lever 3.0 instead of 2.6) | 25 MPa | **≈ 31 MPa**, SF 1.6 |

  The groove skin is **2.37** (mesh), which matches REVIEW's 2.4.
- **Fix:**
  - Keep the mouth chamfer: it gives the release margin in K1.
  - Thicken the shoulder: GR_T 4.5 → **6.0** and move the tooth/block +1.5 in t' (BLK_T −2.5…10.5; `bch`/`tch` to t' 10.8, still ≥ 3.5 to the tongue back at d' 13). Root bending drops to about 24 MPa.
  - Otherwise prove ≥ 600 N on C7 (test 5.5) before production.
  - This is flagged to the strength reviewer.
- **Verified:** FS, calc, plot `mech_work/section_slot_S1.png`.

### S4. The tongue root section is much smaller than "13.7 × 32.1"
- **Part:** AR tongue.
- **Problem:** The drop-in channels `bch`/`tch` are open from z 223.2 through the tongue top. At t' 1–9.3 only these remain:
  - an inner wall d' 3.75–7.9 (4.15 × 23.2 ≈ 96 mm²);
  - the bottom slab z 214.3–223.2 minus the Ø6.5 bore (≈ 86 mm²), plus the carbon rod.

  That is about **183 mm² of plastic, against 440 claimed**. The `bch` rear wall, which carries the catch load, bends along d', which is the print Z axis: interlayer, estimated 12–29 MPa at 900 N.
- **Fix:** The strength reviewer should re-run the tongue root and rear wall with the real section. Cheap gain: stop `bch`/`tch` **1.2 below the tongue top** (z 245.2) and keep a 1.2 roof bridge. The button already drops through the nose part; the tooth/block need the opening only over t' −4.3…1.7, so close t' 1.7…9.3 at the top.
- **Verified:** FS l.690–693, calc.

### S5. Ø6.5 teardrop leaves a 0.25 → 0.0 mm skin on the tongue's inner bearing face
- **Part:** AR rod bore (FS l.696).
- **Problem:** The peak points to −d (it is up when printed). Peak d' = 8.6 − 3.25√2 = **4.00**. The tongue inner face is at d' 3.75 (t' 1–8.5), then wedges to 4.012 at t' 18.5. The skin is **0.25 at the mouth, 0.12 at t' 13.5, and 0.01 at t' 18.5**: one layer or less, on the inner wedge/bearing face.
- REVIEW's "≥ 1.2 in the tongue" came from a *round* r 3.25 probe (FS l.739–740), so it misses the peak.
- **Fix:** Truncate the teardrop with a flat roof at r + 0.4 (d' 4.95), and move the rod to **d' 9.0**. The skin becomes ≥ 1.3 (t' ≤ 8.5) and ≥ 1.1 (t' 18.5). The rod top z 222.0 keeps 1.2 to the `bch` floor; the rod side d' 12.25 keeps 2.85 to `cch`. Re-run the wall probe as a teardrop.
- **Verified:** FS, calc.

### S6. Spring: 9-coil spring nearly coil-binds, and its floor end is not located
- **Part:** SP + AR spring well (FS l.64, 674, 691).
- **Problem:** Installed length is 10.0 (floor d' 10 to cap underside d' 20). Fully pressed it is **5.1**.
  - BOM "L0 16, 9 coils" (7 active): solid = 9 × 0.55 = **4.95**, a 0.15 margin. k = G d⁴ / 8 D³ n = 69 000 × 0.0915 / (8 × 5.45³ × 7) = **0.70 N/mm**, giving 4.2 N at rest and 7.6 N pressed. That is below the "~9 N" target.
  - The FS comment says L0 14; the BOM says L0 16. They disagree.
  - The well is open toward +z (the drop-in route), and only the button end sits on a Ø4.2 × 2 pin. The floor end can walk up the 19 mm open channel.
  - Buckling is fine: L0/D = 2.9 with parallel ends.
- **Fix:**
  - Specify **OD 6, d 0.55, SS302/304, L0 16, 8 coils total (6 active), ends closed and ground**. That gives k ≈ 0.81 N/mm, **4.9 N at rest and 8.9 N fully pressed**, solid 4.4 (0.7 margin). Corrected τ ≈ 0.9 GPa at solid, which is acceptable for SS spring temper with set removed.
  - Add a **Ø6.5 × 1.0 counterbore** in the well floor at (t' −8.5, z 231). The spring end snaps into it at the end of the drop-in; if you do this, move SPR floor to d' 9.0 so the pressed length stays 5.1.
  - Check against 5.7 (kitchen scale).
- **Verified:** FS, calc.

### S7. The button and spring fall out of a loose carrier, and the spring is a small part
- **Part:** AR drop-in channels.
- **Problem:** Once fitted, the notch ceiling (0.3) traps the button. A loose carrier is different: press the button at least 4.3 and slide it toward the nose top, and the button and spring leave through the drop-in route. The sleeve does not stop this. This conflicts with plan 2.8 "small parts captive". The spring, Ø6 × 16, is a choke-size part for ages 2–5.
- **Fix:** Add a **one-way barb in `cch`** at z 241–243: 0.4 interference against the flange's round end, with a 30° lead-in going down and a 90° face going up. The flange clicks past it once during assembly, and service is by pushing the barb aside with a pick through the nose-top opening. Alternative: a press-fit printed plug (0.15 interference) in the nose-top opening.
- **Verified:** FS (dropBox z → 262), calc.

### S8. Tell sleeve: the closed-ring 0.3/side snap is hard to fit and cannot be serviced
- **Part:** TS sleeve vs AR lip (FS l.63, 682–688).
- **Problem:** A closed stadium ring 13.8 × 25.8, 0.7 wall, 2.4 tall, must pass a 13.2 × 25.2 lip.
  - Each round end must lose about 1.0 mm of arc. The straight sides then have to bow inward about **2 mm**, a bending strain of about **3.6 %** (PETG yield is 4–5 %).
  - The lip prints on the bed, so elephant's foot makes the overlap 0.4–0.5.
  - Fitting needs the button held down ≥ 4.0 by a narrow pusher (≤ 8 wide) while a second tool pushes the sleeve. That calls for a jig.
  - There is no removal path without destroying the sleeve, so the button cannot be serviced.
  - Cosmetic: when latched, a **0.4-wide orange band** shows 1.6 deep in the 0.6 cap–lip gap.
- **Fix:**
  - Make the sleeve **split**: a 1.0 gap on one straight side (hidden at t' −0.3 toward the notch), overlap 0.4/side, 30° lead-in chamfer on the sleeve's top outer edge, and a 1 × 1 pick notch in the counterbore wall at the split.
  - Lip opening **12.6 × 24.6** (cap gap 0.3), with the sleeve inner 12.6, so no orange shows when latched.
  - Add a printed assembly jig (pusher 11 × 23 stadium plus a sleeve driver) to the kit (GA tooling).
- **Verified:** FS, calc.

### S9. Wall behind the slot to the outer air is 5.19, against the plan's guardrail of back ≥ 8
- **Part:** WG.
- **Problem:** Measured on the wing with the FS fill: slot back edge (d' 10.7, t' 17.0) to the outer air behind the cheek (d' 15.35, t' 19.26) is **5.19** (5.39 from the tongue). Other walls: outer wall **6.13**, inner **6.68**, groove skin **2.37**.
- **Fix:** This is an owner/strength decision (OPEN 4.8): either a faired local swell of about 3 mm there, or accept 5.2 and load-test.
- **Verified:** mesh (`s_walls.py`, `s_web.py`, plot).

---

## Notes

### K1. Latch kinematics: they work, with margin from the mouth chamfer
- **Insertion:** the 45° tooth lead-in meets the 45° mouth chamfer face-to-face. The tooth only has to pass the shoulder tip at d' 18.4, so it needs **3.6 press of 4.9 (margin 1.3)**. In the groove's z range the tooth never touches the slot wall (d' 17.46).
  - Insertion force ≈ F_s(pressed) × (1 + μ) / (1 − μ) ≈ 1.67 × 7.5 ≈ **12–15 N**, plus 4 crush ribs. Within the ≤ 25 N target.
  - The click is sharp: the tooth's front-outer corner drops off the shoulder tip.
- **Release:** a full press (flange on the `fp` floor, d' 15.1) puts the tooth tip at 17.1, which **clears the shoulder tip by 1.3**. Pull forward needs the crush-rib friction plus the latch, about 10–25 N.
  - Without the outer mouth chamfer this margin would be only 0.36. Keep it.
- **Cocked or off-centre press:**
  - The tooth (z 223.5–246) sits above the cap centre (z 227), and the spring (z 231) is above it too.
  - Pressing the **lower** end pivots the button on the upper flange/sleeve contact. The tooth moves only about 1–3 mm, so it does **not release**. There is no jam (the cap's tilted z-extent of 24.4 is under the 25.2 lip; the flange 25.0 sits in a 25.4 pocket), only a non-release.
  - A ≥ 10 N centre press levels the button. The 1.3 margin tolerates about 3.9° of residual tilt at the tooth top.
  - Fix: centre the thumb scoop **3 mm higher** (z 230) or mark "press here".
- **6° face back-drive:** tan 6° = 0.105 against μ. It holds dry (μ 0.2–0.3). With PTFE (μ 0.1) the net inward force is about 0.005 F, i.e. 4.5 N at 900 N, comparable to the 4.9 N spring. So **do not PTFE-lube the catch face**: lube the slot walls only (test 5.5).
- The "taper reverses with the flip" problem is gone: deviation 1 removes the flip.
- **Verified:** FS, calc, plot.

### K2. Travel stop is a bridged ceiling
The carrier prints on the nose's outer face, so the `fp`/`cch` floor (d' 15.1, the hard stop) and the `bch` and spring floors are 13.4 / 13.6 / 6.5 wide bridges. A sag of 0.2–0.4 reduces travel. That is acceptable against the K1 margin of 1.3, but with S6's spring keep pressed length ≥ 4.8. Coupon: measure travel. **Verified:** FS (print layout l.1362–1366), calc.

### K3. Button print orientation
Standing on the block's end face is right for the tooth root, since bending stays in the layer plane. But the cap, flange and connector (d' 17.2–24.6) start **6.5 mm above the bed**, with the round stadium end down, so they need tree support on the hidden underside. Add it to the PRINT_GUIDE. **Verified:** FS l.1367, calc.

### K4. Screw tips: none protrude
- **Joint M5 × 12 csk** from the back face (yb = YAX + 9): tip at YAX − 3 is inside the tenon's nut slot (to YAX − 7.5), which the DP sleeve covers. The nut has full engagement (+0.55 past it).
- **Pad M5 × 10 button head** from the child side: the head sits 0.25 below the shell face in a Ø10.5 × 3 counterbore. The tip at xb + 8.25 is inside the Ø5.5 hole, which ends at xb + 9 inside the foot.
- The nut drops through the 8.7-wide groove mouth (nut AF 8.3, 0.2/side) **before** the rail slides in, with the groove facing up. Tightening pulls the dovetail flanks together (0.15/side → contact after 0.25), so plastic carries the load ✓.
- With the 5 mm foam the steel head sits under only 5 mm + 3 mm neoprene. Recommend **foam ≥ 10** at the screw, or a 1 mm printed cap disk.
- **Verified:** FS, calc.

### K5. Carbon
- Ø6 in Ø6.5 teardrop: 0.25 radial ✓ (subject to S5).
- Ø2 pins in Ø2.3 bores, 21.5 long for 21 pins, flush at the tenon tip. Walls ≥ 1.45 (top) / 1.85 (side) / 1.5 (corner) ✓.
- The rod ends 7.7 past the nose front face, right in the R3.5 arm-root fillet zone: a stiffness step at the highest-moment section. Strength should check it (deviation 3).
- **Verified:** FS, calc.

### K6. Wing's existing slots and holes
- **S1** normal web from the slot's inner tip edge = **6.4** ✓. The FS lean-prism fills reproduce S1 to about 0.1 mm at every z from 215 to 270 (sliver differences ≤ 2 mm²).
- **S2, the hole column and the z 186 hole** lie outside the fill/cut zones (t' > 26 or z < 207) ✓.
- `closePocket` (default on) fills the old receiver pocket and the **upper M5 clamp-bolt hole** (d' −5.8…0, t' −6…7, z ≈ 254–266) and the old slit. This was owner-approved as "old mount remnants", but **confirm that bolt is truly dead** (it is not in the 2.7 protected list).
- **Verified:** mesh (`s_sections.py` → `wing_sections_S.json`).

### K7. Pinch points
- **Nose into the notch:** a closing gap of about 22 → 0.3 mm during insertion. Low energy and done by an adult, with the last ~0.3 pulled in by the spring cam at ≤ 14 N. Warn in the guide: "keep fingers off the notch face".
- **Button:** gaps 0.2 (cap–sleeve) to 0.6 (cap–lip) stay constant. Below the 5 mm finger threshold, and the spring pushes the finger out. OK.
- **Kit-to-arm-rest:** 2 mm static gap (|x| 163 vs 165). Nothing moves, so no entrapment; OK.
- **Verified:** FS, calc.

### K8. Crush ribs
r 0.4 bumps on faces that print vertical: one extrusion width, 0.2 nominal interference with about ±0.15 XY error, so the fit could be 0.05–0.35. Make them **r 0.5, 0.3 interference**, and test on C7. **Verified:** FS l.744–753.

---

## Assembly sequence (as the FS allows) and what to change

| # | Step | Note |
|---|---|---|
| 1 | Bond the Ø6 × 35 rod (bore from the tongue tip side) and 4 × Ø2 × 21 pins, high-Tg epoxy; cure | Tape the bore exit so epoxy stays out of the `bch` floor |
| 2 | Put the spring on the button's Ø4.2 pin; press the button to the stop (≈ 8.9 N) and slide it down the nose-top opening (cap in `cch`, spring along the well floor) to z 231; release | Needs a pusher; S7 barb clicks here; S6 counterbore locates the spring |
| 3 | Press the button ≥ 4.0 with the inner pusher; push the tell sleeve through the lip with the driver | S8 split sleeve plus jig |
| 4 | Joint: M5 nut into the tenon slot; depth piece sleeve over the tenon; M5 × 12 csk from the back | ✓ |
| 5 | Drop the pad M5 nut into the chosen groove's pocket (groove up); slide the pad rail in from the back to the closed front; M5 × 10 BH from the child side | ✓ (glue dab on the nut if it is lost when the pad is off) |
| 6 | Clip ring, foam, cover | — |
| 7 | On the chair: remove the cap; push the carrier in until it clicks (cap flush, no orange); pull test | B1 stop pads define "home" |
| Remove | Press the scoop (upper half), pull forward | K1 |

---

## Deviations from the plan: mechanical view

| # | Deviation | Verdict |
|---|---|---|
| 1 | 4 carriers per arm length (no flip pair) | **OK.** It also removes plan 4.7/4.10 (taper reversal). Cost: 8 carriers per two-sided kit if all heights are wanted; a child set is still 2. |
| 2 | Button drops in from the nose top; sleeve snaps under the lip; travel 4.9 | **OK with fixes S7 (escape), S8 (sleeve), S6 (spring), S2 (load path).** Travel 4.9 has 1.3 margin (K1). |
| 3 | Ø6 rod below the latch, 35 long, about 8 past the nose face | **OK for packaging, but S5 (0.25 skin)** and K5 (rod end in the root fillet zone) need attention. |
| 4 | Pad screw from the child side, nuts hidden in the foot grooves | **OK.** Tips buried, dovetail clamps (K4). Foam ≥ 10 over the head. The open back end of the unused groove is cosmetic only. |
| 5 | One button per side; one pad shell and ring for both sides | **OK.** The pad is z-symmetric and the sleeve is symmetric. |
| 6 | Button face is a flat leaned plane flush with the nose | **OK.** Moving the scoop up 3 mm helps release (K1). |
| 7 | Mouth chamfer 3.3 × 45° all round | **Keep** (it is the release margin), but **update the REVIEW latch numbers** (S3): bearing 81 mm², shoulder root bending about 54 MPa, tooth lever 3.0. |

## Coupon C7 additions
Slot width variants 14.1 / 14.3 / 14.5 (B1); measure travel to the stop (K2); measure button-sink vs orange (S1/S8); spring 8-coil rate (S6); 600 N pull dry with no PTFE on the catch face (S3, K1); press at the lower end, centre and upper end (K1); remove and refit the button 5× (S7/S8).
