# Side pads r5: independent strength check (2026-11-19)

Scope: carrier + latch A + wing notch as defined by `featurescript/gbg_sidepad_r5.fs` (the truth). Blade outline from `viewer/r5mesh/wing_r5.stl`. Arm sections from `nose_arm_r5_{S,L}.stl`. The builder's table in REVIEW.md "Round 5 CAD" was not used as input. Work files: `strength_work/` (scripts, json, figures; `README` = this file §8).

## 0 Verdict (failures first)

**The joint fails the targets by a wide margin: at 300 N, at 600 N and at 900 N.** The cause is not one weak detail.

The pad sits about 80 mm in front of the slot and 87 mm inboard of it. A 300 N outward lean therefore puts **24 N·m of yaw** on the mount (moment about the vertical axis). A 900 N corner knock puts **103 N·m** on it, with up to 64 N·m of roll and pitch on top.

The builder's notes send this yaw to the "U-walls". The U-walls were never sized. Inside the slot, only a short couple can carry the yaw:
- the inner wall, deep in the slot, pushing outboard;
- the outer wall, near the mouth, pushing inboard.

The outer-wall contact is only **34 mm²**. It is 4.0 mm deep in t' because the 3.3 mm mouth chamfer removes the first 3.3 mm. It is 8.6 mm tall because the latch groove and channels remove everything above z 222.9.

The upper 23.5 mm of the tongue is cut down to a **4.15 mm strip** by the button channels. Its yaw section modulus is about 1/3 of the "13.7 × 32.1" the table assumes.

Items under target (SF < 3 at 300 N, or SF < 2 at 900 N). Ranges run from the seated case (all CAD gaps closed) to the as-built CAD gaps:

| Item | SF at 300 N | SF at 900 N |
|---|---|---|
| Outer slot wall bearing (34 mm²) | **0.28–0.36** | 0.07–0.09 |
| 2.4 mm skin behind the groove | **0.22–0.53** | 0.05–0.11 |
| Shoulder in front of the groove (wing) | **0.18–0.44** | 0.04–0.09 |
| Tooth root, as built with the 2 slits | **0.24–0.60** (the slits cut the root strength ×3) | 0.06–0.12 |
| Tongue at the mouth (bending only) | **0.4–0.8** | 0.11–0.18 |
| Rear wall behind the button block | **0.3–0.8** | 0.08–0.2 |
| Notch floor and ceiling at the mouth | **0.2–0.4** (sensitive to the model) | — |
| Tooth bearing (80 mm² backed, not 91) | 1.2–3.0 | 0.3–0.6 |
| Arm, 18 × 28 | **1.74 (L) / 2.09 (S)** | 0.32–0.41 |
| Tenon root / nut-slot section | **2.08 / 1.52** | 0.17 / 0.20 |
| Inner slot wall / web to S1 | 1.1–2.0 | 0.3–0.5 |
| Dovetail pad mount | ≫ 3 | ≈ 0.6–1.0 |

On the dovetail: a +y push on the pad is held **only by the M5 screw**.

**Ø6 × 35 rod:** it is not useful where it is.
- It ends 7.2 mm past the nose face. That gives ≤ 540 N of bond at u = 0 and none from u ≥ 7.2. The arm's weakest sections are at u ≈ 6–10 (300/600 N) and u ≈ 40–50 (900 N).
- At the tongue mouth it sits on the yaw neutral axis (d' 8.6 vs centroid 8.43).
- Its teardrop peak leaves **0.25 → 0.12 mm** of PETG under the inner bearing wedge face, and **breaks out at t' ≈ 18.5**. The builder's "≥ 1.2 wall" was measured with a round probe, so it missed the teardrop.

**The builder's "900 N" is a different load.** It is the *tooth* load from a **300 N** pad corner knock (`review_r5/hookdrop/loads_out.txt`: "corner knock 300 N … element 901 N"). My contact model reproduces it: 865 N with CAD gaps. With 900 N on the pad, as specified here, every load is about 3× higher. Even at the builder's own load, the shoulder and skin come out at SF ≈ 0.3–0.4, not 2.1. The table's tooth and shoulder areas also ignore the 3.3 mm mouth chamfer.

**SF ≥ 2 at 900 N, printed only, is not reachable with one tongue in the blade edge.** That target needs about 200 N·m of yaw capacity at the mount. The blade is 29 mm wide (d' −4.4…24.6), and S1 limits the depth. §6 lists:
- the fixes that move the 300 N and 600 N numbers to about target;
- the change in architecture that the 900 N target needs.

## 1 Assumptions

- **Material:** PETG on a P1S. Basis is the r4 strength review, for continuity. Allowables:
  - 40 MPa along the layers, 18 across them, 22 in-plane shear;
  - 18 for interlayer shear (conservative);
  - 25 bearing.
  - These are room-temperature values. Sustained at 60 °C in a car they are about 1/3 of this, so every SF below drops about 3× for a sustained hot load.
  - PLA+ is stronger at room temperature but creeps above about 50 °C, so it is not used as the basis.
- **Print orientation** (PRINT_GUIDE):
  - Carrier on the nose's outer face. The layer normal is d, so the tongue's bending stress (along t) is in-plane. The rear wall bends across the layers. The tenon and the far arm cross the layers at sin θ = 0.79–0.92, which puts the Tsai-Hill allowable at 20–25 MPa.
  - Button standing, so the tooth bends in the layer plane.
  - Wing standing (z up), so σ_zz is across the layers.
  - Depth piece foot-face down.
  - I checked printing the carrier z-up instead. It does not help: the L arm rises at up to 73° and ends up across the layers.
- **Loads on the pad, worst over all 8 configurations** (S/L × LOW/HIGH × pad ±11, D0, T0):
  - **300 N**: outward lean at the pad centre.
  - **600 N**: proof, same direction.
  - **900 N**: corner knock at the worst of the 4 corners (y 14 ± 34.4, z_c ± 33.6) in ±x and +y.
  - **Forward pull**: −y at the pad centre.
  - The pad centre is (x ≈ 87, y 14, z_c). The tongue reference is QS(10.6, 5, 227), at world (138.7, 93.6).
  - So an outward lean gives F(d, t) = (0.796 P, −0.608 P): it pulls the tongue out. It also gives Mz = +80 mm × P.
- **Contact model** (`loads.py`):
  - Rigid carrier on compression-only (Winkler) contacts, k = 400 N/mm³.
  - Gaps from the FS: wedges 0.05; inner wall at the mouth 0.2; rear faces 0.17; tooth 0.25 (0.02 at the face plus 0.3 of channel play); nose back face 0.3; tongue top and bottom 0.2, or 0.5 on the reliefs; ribs 0; nose top and bottom 0.3.
  - Each case is run twice: once with the CAD gaps and once fully seated (all gaps 0). Ranges are reported between the two.
  - Friction is ignored. On the side walls, friction would take part of the pull-out off the tooth, but it does not reduce the yaw couple.
- **Members** (each script is listed in §8):
  - The tongue section at the mouth is integrated on a pixel grid of the FS section, including the channels and the teardrop.
  - The arm uses mesh sections square to the path, with Kt 1.45 at the R3.5 root.
  - The tooth, groove, shoulder, skin and wing tip are linear voxel or pixel FE. The SF is taken at the 99th percentile, which drops the singular corners. The 95th percentile is in the json files.

## 2 Load path, pad → wing (300 N lean, worst = long arm, HIGH, upper pad; seated / CAD gaps)

1. **Pad.** The pad rail sits in the foot's dovetail groove; the pad back face bears on the foot face.
2. **Depth piece to tenon.** The depth piece sleeve carries the load onto the 13 × 20 tenon. The joint face (JGAP 0.3) only engages after about 1.9° of rotation, so the tenon root carries the whole moment.
3. **Arm.** The 18 × 28 arm carries it to the nose (R3.5 root).
4. **Nose and tongue.** At the mount the resultant is F(d,t,z) = (239, −182, 0) N and M = (10, 13, **24**) N·m.

How the mount reacts it:

| Reaction | Seated | CAD gaps |
|---|---|---|
| Outer slot wall (34 mm²) | **2.34 kN** | 3.0 kN |
| Inner slot wall | 2.25 kN | 3.1 kN |
| Tooth | 1.63 kN | 0.66 kN |
| Nose back face on the notch face | 1.2 kN | 0 |
| Nose top / bottom (roll) | ~1.8 kN each | 0.3 kN |

The yaw is carried as a d-couple of about 2.3–3 kN, with only about 6–8 mm of lever between the outer wall (t' 5.3–9.3) and the inner wedge (t' 8.5–18.5).

The latch is on the wrong side for this case. An outward lean pulls the **inboard** edge of the tongue out. The tooth is on the outboard edge, which is being pushed in.

Other cases:
- **900 N corner, out:** M = (48, 64, **103**) N·m. The outer wall takes 9.5–11.4 kN and the tooth 3.3–6.7 kN.
- **Knock inward or backward at a corner:** the tooth (outboard, +t) and the inboard nose back face pair up with about a 22 mm lever. These cases are much kinder.

## 3 Safety factors

SF = allowable / stress. "s–c" is the seated to CAD-gap range. **Bold** = under target: < 3 at 300 N, or < 2 at 900 N. Values at 600 N and in the forward-pull column are shown for information.

| # | Item (governing location) | 300 N lean | 600 N proof | 900 N corner | Fwd pull 300 N | Note |
|---|---|---|---|---|---|---|
| 1 | Outer slot wall bearing, carrier and wing (t' 5.3–9.3 × z 214.3–222.9 = 34 mm²) | **0.28–0.36** | **0.14–0.18** | **0.07–0.09** | **0.51–0.65** | 3D FE wing tension/shear: 0.29 at 300 N |
| 2 | Tongue at the slot mouth, t' 4.7 (L-section A 178 mm², Izz 3050 vs 6880 solid) | **0.4–0.8** | **0.2–0.4** | **0.11–0.18** | **0.7–1.2** | Bending + shear. Adding the model's tongue torsion gives 0.17–0.3 |
| 3 | Rear wall behind the button block (t' 9.3 → diagonal, 4.15 → 0 mm thick, bends across the layers) | **0.3–0.8** | **0.16–0.4** | **0.08–0.2** | 0.5–1.2 | 2D slice FE `fig_zero_*_latch_carrier.png` |
| 4 | Tooth bearing on the groove face (backed 3.55 × 22.5 = 80 mm²; d' 17.96–18.45 has no wing behind it because of the mouth chamfer) | 1.2–3.0 | **0.6–1.6** | **0.3–0.6** | 2.0–4.3 | |
| 5 | Tooth root bending, as built (2 × 0.4 slits split the root into 1.9 / 0.9 / 1.2 laminae) | **0.24–0.60** | **0.12–0.31** | **0.06–0.12** | **0.39–0.86** | 2D FE: 100 MPa per kN with slits vs 31 without |
| 5a | Same, slits removed | **0.78–1.9** | 0.39–0.99 | **0.19–0.39** | 1.3–2.8 | |
| 6 | Tooth root shear (4.78 × 22.5) | **1.0–2.4** | 0.5–1.2 | **0.24–0.48** | 1.6–3.4 | |
| 7 | Shoulder in front of the groove (wing). The 3.3 chamfer makes it 0 mm at d' 18.45 and 2.85 at 20.95, not "2.5 + 0.5" | **0.18–0.44** | **0.09–0.22** | **0.04–0.09** | **0.29–0.63** | 3D FE: FI = 1 at 292 N tooth load |
| 8 | 2.4 mm skin behind the groove: eccentric tension, plate bending in t, σ ≈ 88 MPa per kN | **0.22–0.53** | **0.11–0.27** | **0.05–0.11** | **0.35–0.76** | 3D FE: FI = 1 at 352 N. With the outer-wall load added (wing3d): 0.15–0.21 at 300 N |
| 9 | Inner slot wall (blade inner skin d' −4.4…3.55) / web to S1 | 1.1–1.7 / 1.3–2.0 | 0.55–0.85 / 0.65–1.0 | **0.27–0.42 / 0.32–0.5** | – | 3D FE `wing3d.py` (seated) |
| 10 | Notch floor and ceiling at the mouth (nose top/bottom roll reactions) | **0.2–0.4** | **0.1–0.2** | **0.05–0.1** | – | Driven by the roll; sensitive to the model |
| 11 | Nose back face on the notch face (384 mm²) | 8 | 4 | **1.9** | 16 | Compression |
| 12 | Inner wall bearing (424 mm²) | 3.4–4.7 | **1.8–2.4** | **0.9–1.1** | 5.5–6.8 | |
| 13 | Arm 18 × 28, weakest section (S: u ≈ 8; L: u ≈ 8; at 900 N: u 40–50, across layers) | **2.09 (S) / 1.74 (L)** | **1.04 / 0.87** | **0.32 / 0.35** | **1.29 / 1.45** | Bond-limited rod included |
| 14 | Arm root at the nose, R3.5, Kt 1.45 | 4.9 / 4.1 | 2.25 / 1.92 | **0.87 / 0.79** | 4.8 / 3.9 | |
| 15 | Tenon root, 13 × 20, 4 × Ø2 pins (bond 201 N each, 8 mm in the arm), allowable 24.7 | **2.08** | **0.86** | **0.17** | **1.21** | S arm worst |
| 16 | Tenon at the nut slot (x_e − 6.5: −y half removed over z ±4.8, plus the Ø5.5 hole) | **1.52** | **0.76** | **0.20** | **1.26** | Taken at half the root moment |
| 17 | Dovetail pad mount: rail pull-out by corner tilt (about 2.4 kN at 900 N); lip shear 137 mm² (interlayer); outer groove lip only 2.85 mm thick | ≫ 3 | ≫ 3 | **≈ 0.6–1.0** | ≫ 3 | Estimate |
| 17a | Pad +y (push back): held **only by the M5 × 10 shank**. Bearing 7 × 5 mm in PETG | **2.9** | 1.4 | **1.0** | – | Breaks the "screws only lock" rule |

## 4 Item notes

- **Tongue engagement.**
  - Outboard: nominally 7.6 mm. Only t' 5.3–9.3 (4.0 mm) bears, because of the mouth chamfer. It bears only below z 222.9: above that the tongue's outer side is cut by `tch` (d' 12.05–22.6) and the wing wall is cut by the groove.
  - Inboard: 16–19 mm nominal, with the wedge at t' 8.5–18.5. It is fine in bearing (item 12), but the yaw couple's lever is the gap between the two contacts, about 6–8 mm.
  - The 1.5°/side wedge pushes the tongue *out* (−t) when it is loaded sideways. Friction keeps it self-holding.
- **Rear wall (item 3).** The whole latch load enters the carrier through the button block's back face (t' 9.0 → 9.3, d' 13–17.2). The tongue material behind it is a wedge, 4.15 mm thick at d' 13 and 0 at d' 17.3. It cantilevers from the 4.15 mm strip (d' 3.75–7.9) and bends across the layers.
- **Tooth.**
  - Lever 3.0 mm, root depth 4.78 mm.
  - The slits (d' 14.4–19.0, t' 5.9–6.3 and 7.2–7.6) run along the cantilever, across its shear-flow planes. The 3 laminae bend separately, so the root stress rises from 31 to 100 MPa per kN. Slicer walls round a 0.4 slit do not bond across it.
  - The 6° face back-drives at μ < 0.105. Do not use dry PTFE (BOM 7.11).
  - There is 0.2 mm (flange pocket) / 0.3 mm (block channel) of pull-out play that the taper cannot take up. The tooth has only 0.2 mm of d-travel to the groove bottom, which is 0.02 mm in t.
- **Shoulder and skin (items 7–8).** The tooth force acts at d' ≈ 20.2. The only connection back into the wing in the groove's z-range is the skin (d' 22.2–24.6). The load therefore goes through the skin as tension with about 3.2 mm of eccentricity, bending the 2.4 mm plate.
  - 3D FE, as built: failure at 0.29–0.35 kN of tooth load.
  - Mouth chamfer 0.5 outboard + skin 4.0: 0.49–0.59 kN.
  - Shoulder 6 mm (groove face at t' 8.0) + skin 4.0 + chamfer 0.5: **1.16–1.23 kN**.
  - The coupon will find this before the 2.4 mm "fit" check does.
- **Arm.**
  - Between u 4 and 30 (both arms), lean and proof govern. The section is in-plane near the root and crosses the layers further out.
  - At 900 N the far half governs. There the arm runs at sin θ 0.8–0.92 to the layers, giving an allowable of about 20 MPa.
  - Meeting SF 3 at 300 N needs about 1.75× the section modulus.
- **Ø6 rod** (t' 13.5 → −21.5, 7.2 mm past the nose face t' −14.27):
  - At the arm root, the bond limit (π·6·a·4 MPa) gives ≤ 4.5 N·m at u 0, ≤ 2 N·m at u 4 and 0 from u 7.2. That is ≤ 9 % of the section moment where it matters, and nothing at the weakest sections.
  - Dowel action needs about 2–3 d of embedment each side, and it has 1.2 d.
  - Plan §3.2 had it running to t ≈ −29 (u ≈ 15). The r4 benefit "1.20 → 1.50 at u 13" assumed that length, and is gone at −22.
  - In the tongue it is on the yaw neutral axis.
  - **Not useful as placed. It also costs the 0.25 → 0 mm skin under the inner bearing face.**
- **Tenon.**
  - The axis (−x) is 52° out of the layer plane.
  - The pins' 8 mm embedment in the arm caps them at about 200 N each, ≤ 6 N·m for the pair, against 6.6 / 13 / 64 N·m at the root.
  - The nut slot removes 40 % of the section at mid-tenon.
- **Dovetail.**
  - With centre loads the pad is pressed onto the foot face, so the rail sees little.
  - Corner loads tilt the pad about the foot edge (lever about 39 mm in y, 25 mm in z) and pull the rail out.
  - The groove is open at the back (y > 46), so +y is held by the screw alone.

## 5 Builder's table vs this check (at the builder's own load, a 300 N corner knock)

| Builder | Claimed | This check | Why |
|---|---|---|---|
| Bearing 4.05 × 22.5 = 91 mm², 9.9 MPa, SF 2.5 | 2.5 | 2.3 (80 mm², 865 N) | Mouth chamfer removes the wing behind d' 17.96–18.45 |
| Tooth root bending, lever 2.6, SF 2.0 vs 50 MPa | 2.0 | **0.46** (with slits, 40 MPa) / 1.5 without | Slits ×3; lever 3.0; allowable 40 (r4) |
| Shoulder shear 76 mm², SF 2.1 | 2.1 | **0.34** shoulder / **0.41** skin | Chamfer; the skin carries the load in eccentric tension; skin not checked |
| Tongue 13.7 × 32.1 | – | Izz 3050 vs 6880 | Latch channels cut d' 7.9–17.45 above z 222.9 |
| Yaw/roll "→ U-walls" | not checked | outer wall SF 0.36 at 300 N lean | 34 mm², 6–8 mm couple |
| Rod wall ≥ 1.2 | 1.2 | **0.25 → −0.01** at the teardrop peak | Round probe used |

## 6 Fixes (mm). None of them move S1, S2, the teardrop channel, the hole column or the hole at z 186

### Local fixes: do all of them; they are cheap

1. **Tooth slits: delete both.** Give the tooth a solid root with R1 into the block. Root ×3.2 (item 5 → 5a).
2. **Mouth chamfer, outboard side: 3.3 → 0.5 × 45°.** Keep 3.3 on the inboard side, floor and ceiling. On the tongue, the outer root fillet becomes R0.5 plus a 0.6 × 0.6 relief groove. Outer contact goes t' 5.3 → 2.5, area 34 → 58 mm², outer wall SF 0.36 → 0.63 at 300 N. The backed tooth face becomes d' 17.96–22.0.
3. **Groove and skin:**
   - groove bottom GR_D 22.2 → 20.6, so the skin is 4.0;
   - move the groove front face from t' 4.5 to **t' 8.0**, so the shoulder is 6 mm (with fix 2);
   - the tooth and block move +3.5 in t.
   - Capacity 0.29–0.35 → **1.2 kN** of tooth load (3D FE).
4. **Ø6 rod and bore:** delete them from the tongue and nose and leave PETG solid. If carbon is kept, keep the bore ≥ 1.2 from every face *at the teardrop peak* (ROD d' 8.6 → 9.6, or a 0.4 flat-top bore), and count it as zero in the SF.
5. **Button block channel clearance in t:** 0.3 → 0.1 (play). Spec "no PTFE on the 6° face".
6. **Tenon:**
   - 13 × 20 → **16 × 24** (R3). The depth-piece sleeve wall stays ≥ 2.5 inside the 18 × 28 trumpet.
   - Move the nut slot to x_e − 9.5 (out of the root half).
   - Pins Ø2 × 21 → Ø2 × 29, 16 mm into the arm, bond 400 N each.
   - Estimated tenon SF: 3.7 / 1.5 / 0.3.
7. **Dovetail:**
   - Add a plastic +y stop: a 1.5 mm snap bump on the rail top at y 44–46 into a 1.6 × 2.0 notch in the groove floor, so the screw only locks.
   - Thicken the outer groove lip 2.85 → ≥ 4.0 (FOOT_HZ 20 → 21.5).

### Changes needed for the 300 N target (SF 3)

8. **Take the latch out of the tongue's section.** No channel through the tongue for t' > 1.4: the button block stays in the nose, t' −4…+1.2. Make the tongue **solid and widened to d' 2.0–18.2** (16.2 × 32.1) for t' 1.7–14. That leaves ≥ 6.0 mm walls to the blade faces at d' −4.4 / 24.6. The inner-back diagonal and the 6 mm web to S1 stay as they are.
   - Tongue yaw Z: 355 → about 1560 mm³. Tongue SF about 3.5–4.4 / 1.8–2.2 / 0.8–1.0 at 300 / 600 / 900 N (seated moments).
   - With fix 2 the outer wall then bears over the full height: about 215 mm², SF ≈ 2.0 / 1.0 / 0.5.
   - Extending the straight outer face from t' 9.3 to 11.0 (≥ 5.5 mm to the boss round at d' 21, t' 14.5) adds about 50 mm², SF ≈ 2.4 at 300 N.
   - The catch then needs a pull-out face backed by solid blade, not a skin: the plan's option B hook-and-drop lips in the slot ceiling/floor, or fix 3's 6 mm shoulder reached by a tooth on a bar outboard of the tongue.
9. **Arm:** 18 × 28 → **22 × 32** for u 0–30, grown inward and up (kit x max 163 vs arm rest 165). Z_weak ×1.7, giving lean SF about 3.0 (L) / 3.6 (S) at 300 N and about 0.55 at 900 N.

### The 900 N target (SF 2, printed only)

The 900 N target is **out of reach for a single tongue in the blade edge**, even with 1–9 (about 0.5–1.0 at 900 N). It needs about 200 N·m of yaw capacity at the mount: a couple of about 5 kN over ≥ 40 mm in t, or a ≥ 30 × 40 PETG section in the arm. Options, for the owner:

- **(a)** Add a second load path: a stay from the depth-piece foot to the seat or arm rest, so the mount stops seeing the pad's 80 mm lever.
- **(b)** Re-scope 900 N. The pad (x ≤ 104) sits inside the arm rest (x 165), so a doorframe can hardly reach it outboard. The real knock is +y on the front corner, or a hit on the arm near the wing (short lever). Make the tenon/depth piece a defined, reprintable fuse at about 400–500 N that protects the wing.
- **(c)** Change the mount from a tongue in the edge to a saddle that clamps both faces of the blade over ≥ 40 mm in t. This is the "bolted-on block" look the owner rejected.

## 7 Load tests that would show these (before the child fit)

- 300 N outward at the pad centre, long arm, HIGH, upper pad. Watch:
  - the outer slot wall at the mouth (z 214–223);
  - the 2.4 mm skin behind the groove;
  - the tongue strip at the mouth.
- 150 N forward pull. The tooth and the rear wall go first.
- Expect the first damage below 300 N on the as-built r5. A 600 N proof will not pass.

## 8 Files (`strength_work/`)

The venv is `viewer/.venv` (numpy, scipy, trimesh, shapely). All scripts are run from `strength_work/`.

| Script | Output | What it does |
|---|---|---|
| `geom.py` | – | FS constants, frames, arm path, materials |
| `sec_wing.py`, `sec2.py` | `wing_sections.png`, `wing_sections.json` | S-frame slices of `wing_r5.stl`, with the FS tongue, slot and groove overlaid |
| `loads.py`, `cases.py` | `loads_{cad,zero}.json`, `cases_*.pkl` | Contact model, all configurations, both gap modes |
| `tongue.py` | – | Tongue mouth section and contact bearing (items 1, 2, 4, 11, 12) |
| `slices.py` + `fe2d.py` | `fig_zero_*_carrier.png` | 2D carrier slices (item 3). The 2D wing slices are not used: in 2D the tip becomes an artificial cantilever |
| `fe3d.py`, `groove3d.py`, `groove3d_v.py` | `groove3d_*.json` | 3D voxel FE of the groove region. Variants: as built / fix / shoulder 6 mm |
| `wing3d.py` | `wing3d_zero_lean300.json` | 3D voxel FE of the wing tip under all contact loads, 0.6 mm voxels |
| `tooth2d.py` | – | Tooth with and without slits |
| `arm.py`, `arm_zup.py` | `arm_out.json` | Arm sections (and the z-up print check) |
| `tenon.py` | – | Tenon root, nut slot, dovetail estimate |
| `fixcheck.py` | – | What fixes 2 and 8 buy |

Limits:
- A rigid carrier overstates load sharing to stiff contacts, and its roll couple overstates the tongue torsion and the notch floor/ceiling loads (items 2 and 10). The seated and CAD-gap ranges bracket this.
- Linear-elastic, isotropic FE with orientation-aware allowables.
- Room temperature only.
- The STLs predate the FS: the arm, blade and S1 are the same; the tongue, slot and latch were taken from the FS.
