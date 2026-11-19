# Side-pad quick-release mount: making it stiff in twist with carbon (desk research, r5)

Date 2026-11-19. Scope: the tongue-in-blade-edge quick release for the lateral pad. Goal: yaw SF ≥ 3 at a 300 N lean, printed only (PETG), using the Ø2 / Ø6 / Ø8 carbon rods and tubes in stock. Screws only lock.
Companion files: `joint_tricks.md` (r5 tricks T1–T10), `final/strength.md` (independent FE check), `final/mech.md`.

## 0 Answer first

**Top 3 (SF at the 300 N lean, 19 N·m yaw; the second number uses strength.md's 24 N·m):**

| Rank | Option | What limits it | SF at 300 N (19 / 24 N·m) | SF at the 27.5 N·m knock |
|---|---|---|---|---|
| 1 | **"Three-prong plug"**: a solid tongue plus two Ø8 carbon rods (prongs) above and below it, parallel to the slide, in blind bores in the blade | PETG bearing in the blade bores | **≈ 2.5–5 (mid grade ≈ 2.8 conservative, ≈ 3.5 with 8 mm effective bearing) / 2.2–4** | ≈ 1.9–3.4 |
| 2 | **Vertical cross-rod key**: a vertical Ø8 rod through the tongue tip, with its ends running in grooves in the slot ceiling and floor (cruciform slot), plus Ø2 tongue rods | Rod transverse shear (depends on rod grade) | **≈ 2.5–3.5 / 2.0–2.8** | ≈ 1.7–2.4 |
| 3 | **Ø2 carbon "spine" rods** at the tongue's corners, running deep into the nose and arm (no wing change) | The blade's slot walls, not the tongue | Tongue alone 4–10. **System ≈ 2–2.4** (wall-limited, strength.md fixes 2 + 8) | ≈ 1.4–1.7 |

What drives this:
- A printed tongue's yaw strength scales with h·w². The width w is capped by the blade (29 mm at the tip leaves w ≈ 16 with 6 mm walls). So a **solid** PETG tongue tops out near SF 2.8 at 19 N·m (2.2 at 24 N·m), whatever its shape. See §1.
- To pass 3 you either:
  - put stiff carbon in the tongue's extreme fibres (option 3), or
  - add load paths beside the tongue that bear on thick blade, not on the 6 mm slot walls (options 1 and 2).
- The current Ø6 rod sits on the yaw neutral axis, so it adds nothing (strength.md). Put carbon **away** from the axis.
- **Option 1 + the Ø2 spine** is the recommended build. Option 1 carries ~35–70 % of the yaw in the prongs, so the tongue drops to ≥ 4.4 and the 6 mm slot walls see a third to two thirds of today's couple.
- **Prerequisite for every option:** the tongue section must be solid. Today's SF ~1–1.5 comes mostly from the latch channels cutting the tongue to a 4.15 mm strip (strength.md fix 8, mech.md S3). Carbon cannot rescue a hollowed section.

## 1 Mechanics and assumptions (used everywhere below)

- **Loads:**
  - Yaw at the mount: M = 19 N·m (300 N lean, per brief). strength.md's contact model gives 24 N·m; multiply every SF by 0.79 for that.
  - Knock 27.5 N·m: multiply by 0.69.
- **PETG** (strength.md basis):
  - 40 MPa along layers, 18 across;
  - 22 in-plane shear, 18 interlayer shear;
  - 25 bearing;
  - E ≈ 2.0 GPa.
  - Hot car (sustained, 60 °C): roughly ÷3. That holds for every option; carbon does not creep, PETG around it does.
- **Carbon: the grade matters more than any geometry choice here.** Datasheets for "pultruded carbon rod" range widely:

  | Grade | Tensile (MPa) | E (GPa) | Flexural (MPa) | Shear, transverse/ILSS (MPa) | Compressive widthways (MPa) |
  |---|---|---|---|---|---|
  | Easy Composites hobby pultrusion TDS | 400–500 | 28–40 | 250–400 | 30–40 (short beam) | 60–100 |
  | Rock West 0.25" T700 rod | 1720 (min) | 138 | 1830 | 41 (ultimate, method unclear) | — |

  - Max service temperature: 80 °C (Easy Composites). Tg is 100–120 °C (Rock West). Both are fine for a hot car.
  - I ran three grades: low (E 30, flex 300, τ 30), mid (E 100, flex 1000, τ 41, the brief's "1000+"), and high (E 130, flex 1800, τ 41).
  - **Before relying on the stock rods, do a 3-point bend coupon on one.** It is a 5-minute test that tells you which row you own.
- **Rods vs tubes:**
  - Use solid rods for anything loaded across its axis (pins, prongs). Tubes ovalise and crush under line bearing, and they have low transverse shear area.
  - Clever stock trick: **a Ø6 rod pushed into a Ø8×6 tube is a near-solid Ø8** (wick thin epoxy in).
  - Never drill or notch the carbon. Holes greatly weaken tubes (ArduPilot thread). Carry it by bearing and shape.
- **Bond:** I found no lap-shear data for epoxy on PETG. Carbon is "slippery" to printed plastic: a Thingiverse 6 mm tube peg needed CA to stop spinning. A UT Austin CF-to-aluminium push-rod test reached only ~32 % of the expected bond. So:
  - design so bearing (shape) carries the load;
  - use the bond only to retain the rods;
  - wherever a bond length is quoted below, it uses 4 MPa (strength.md's assumption).
- **Couple formula:** F = M / a, bearing σ = F / (d·l), double-shear τ = (F/2) / A.
  - A rod in a blind bore acts as a short pile. Its moment is reacted by two end bearings, F ≈ M/(L − l_b), with l_b = 6 mm (conservative).
  - Hand calcs are in `/tmp/claude-1000/-home-mikey-mikey/361a8116-886a-46f6-82ed-6fdd40aedccb/scratchpad/calc.py` (scratch, not committed).

Solid tongue 15.8 × 32, yaw about z: I = 10 518 mm⁴, Z = 1 331 mm³, so σ = 19 000 / 1 331 = **14.3 MPa → SF 2.8** (2.2 at 24 N·m).
The bending stress runs along t. The carrier prints with its layer normal along d, so that stress is in the layer plane, which is good.

---

## 2 Options

### O1. Three-prong plug: two Ø8 carbon prongs beside the tongue — **RANK 1**

- **How it works:**
  - The nose is 69 tall; the tongue is 32 in the middle.
  - Two solid Ø8 carbon rods are fixed in the nose: one centred ~24 mm above the tongue centre, one ~24 mm below (4 mm web plus radius).
  - They stick out rearward, parallel to the tongue, by about the tongue depth (~22–24.5).
  - They slide into two Ø8.1–8.2 blind bores in the blade, along t, in full-thickness blade (~20–29 thick, so ≥ 6–10 mm walls).
  - Think of a three-pin mains plug, or a cine-rig LWS rod pair (15 mm rods at 60 mm centres carry heavy cantilevered cameras).
  - Yaw is shared between the tongue and the two prongs in bending. The prongs react it as end bearings in thick blade, not on the 6 mm slot walls.
  - **Roll** (about t) becomes a couple across the prongs' 48 mm spacing: very effective. Pitch is shared too.
- **Yaw capacity (19 N·m; share by EI; tight fits assumed):**

  | Grade | Prong share | Prong σ_bend → SF | Blade-bore bearing (6 mm ends) → SF | Tongue SF |
  |---|---|---|---|---|
  | Low | 36 % | 69 MPa → 4.4 | 5.0 MPa → 5.0 | 4.4 |
  | Mid | 66 % | 124 MPa → 8.1 | 9.1 MPa → **2.8** | 8.2 |
  | High | 71 % | 135 MPa → 13 | 9.9 MPa → **2.5** | 9.8 |

  - Rod transverse shear: (≈ 300 N lateral share) / 50 mm² ≈ 6 MPa, negligible.
  - The governing item is **PETG bearing at the bore mouth and tip**. Ways to raise it:
    - 6 mm of end bearing is conservative for an elastic foundation. With 8 mm it is 3.3–3.7 at mid/high grade.
    - Deeper bores help, if S1 allows. S1 runs z 215–270 behind the slot, and its web to the slot is 6.4 mm (mech.md K6), so the upper prong is limited to about the same depth as the tongue.
    - The lower prong can go deeper if nothing is behind it. At 35 mm, bearing SF goes to ~4.
    - In the z-up blade print, the bore load is along d, which is horizontal and in-plane, so the full 25 MPa allowable applies.
  - Counter-intuitive but real: **stiffer rods move more load to the bearing**, so the tongue SF rises while the bore SF dips. The system minimum is ≈ 2.5–3.5 across the grades, and ≈ 3+ with 8 mm effective bearing or a deeper lower prong.
  - Add the Ø2 spine (O3) and the tongue is never the weak link.
- **Fit is everything.** Load sharing needs near-zero clearance:
  - At a 9 mm half-arm, 0.1 mm of play means the tongue takes the first ~0.6° of rotation alone.
  - Use 3 crush ribs (0.15–0.2 mm) in each blade bore (joint_tricks T5), or model the bore at 8.0 and run an 8 mm drill through it by hand.
  - Model the bores as a teardrop with a flat roof (mech.md S4 lesson) or bridge them.
- **Fixing the rods in the nose:**
  - Pause-print embed (M601): print a half channel, drop the rods in, resume. The rods lie along t, in the carrier's layer plane, so they are fully enclosed and bear in shape.
  - A drop of epoxy for retention only.
  - Embed ≥ 30 mm into the nose/arm. This also helps the arm root, which is at SF 1.74–2.09 (strength.md).
- **Look:**
  - Fully hidden when fitted: the prongs sit inside the 69 mm nose and blade.
  - Detached, the carrier shows two black rods behind the nose, like a plug. Round the tips: 1 mm chamfer, then sand.
  - The wing gets only two round holes in its front edge, under the nose's footprint.
- **Quick release:**
  - Unchanged direction (front to back) and the same latch.
  - Better feel: make the rods 2–3 mm longer than the tongue, so they act as **alignment pins** and engage first. Insertion self-centres, and the tongue cannot cock in the slot.
  - The spring-button catch still takes pull-out only.
- **Risks / checks:**
  - Bore clearance to S1 and to the filled upper M5 clamp hole (z 254–266, d' −5.8…0) is the first CAD check. "Existing slits and holes cannot move" may set the upper prong z.
  - The blade wall to the face must be ≥ 6 at Ø8.
  - Coupon test: one blade-edge stub with bore plus one prong, 300 N lateral at 80 mm.
- **Sources:**
  - cine 15 mm rod-pair practice and an anti-rotation pin plus screw: Holdan / Kondor Blue M12-to-15 mm mount, https://www.holdan.co.uk/pdfSetup/KON14A12M
  - locating pins against spin: https://www.ulanzi.com/blogs/knowledges/locating-pins-prevent-camera-spin-mechanics-workflow-roi
  - pin-bearing of FDM coupons (ASTM D5961), Khosravani et al., *Polymers* 2023: https://www.mdpi.com/2073-4360/15/7/1660
  - printed-in holes give +31 % bearing (CSUS): https://scholars.csus.edu/esploro/outputs/graduate/Using-additive-manufacturing-techniques-to-increase/99257831365101671
  - pause-and-embed rods (JanTech): https://hackaday.com/2025/12/21/pause-print-add-hardware-and-enjoy-strength , https://www.3printr.com/youtuber-demonstrates-methods-for-reinforcing-fdm-3d-prints-1885964/
  - rod data: https://www.rockwestcomposites.com/47317-2.html , https://media.easycomposites.eu/datasheets/EC-TDS-Carbon-Fibre-Pultrusions.pdf

### O2. Vertical cross-rod key in a cruciform slot — **RANK 2**

- **How it works:**
  - A vertical solid Ø8 rod, 52 long, sits in a bore through the tongue near its tip (t ≈ 20, 4 mm of PETG behind it).
  - It sticks out 10 mm above and 10 mm below the tongue.
  - The slot becomes cruciform: 8.2 wide × 10 deep grooves in the slot ceiling and floor, running the full slot depth from the mouth.
  - As the tongue slides in, the rod ends ride in the grooves.
  - The yaw couple is now:
    - the rod ends bearing on the groove walls (≥ 10 mm thick, solid blade, load in-plane for the z-up print), against
    - the mouth reaction.
  - Optional: a **second rod at t ≈ 4**, so both ends of the couple are rods and the 6 mm slot walls only guide. Lever a ≈ 16, so F = 1 188 N per rod.
  - All the carbon is in the removable carrier, and the wing gets printed geometry only (no inserts). A broken rod means reprinting the cheap part.
- **Yaw capacity (19 N·m, a = 16, F = 1 188 N, each end 594 N):**
  - Rod transverse shear at the tongue face: 594 / 50.3 = 11.8 MPa. SF 2.5 (τ 30) / 3.5 (τ 41).
    - This is the governing term, and it rests on the poorly defined datasheet "shear". True fibre-cutting transverse shear (ASTM D7617) is normally well above ILSS, so 2.5 is likely pessimistic. **Unverified.**
  - Rod bending (594 N at 5 mm): 59 MPa, SF ≥ 5.
  - Groove-wall bearing: 594 / (8 × 10) = 7.4 MPa, SF 3.4.
  - Shear-out of the rod from the tongue: two planes 3.9 × 32, 4.8 MPa. This is interlayer, because the carrier's layer normal is d. SF 3.8.
  - **But the tongue mouth still carries the full M** (SF 2.8 / 2.2). Pair it with O3, which gives ≥ 4.
  - System ≈ 2.5–3.5.
- **Look:** hidden when fitted. Detached, two short black stubs show above and below the tongue tip.
- **Quick release:** unchanged.
- **Risks:**
  - The vertical bore in the tongue prints as a horizontal hole (teardrop). Use mech.md S4's flat-roof teardrop.
  - The ceiling and floor grooves cut into the wing near the latch groove (outer wall, z > 222.9) and toward S1. CAD check needed.
  - It changes the slot section, so the slot coupons (14.1 / 14.3 / 14.5) get redone.
- **Sources:**
  - a printed PETG clevis with a carbon-rod axle (Printables "Gabelkopf"): https://www.printables.com/model/537495-gabelkopf-clevis-fur-20-mm-mit-m2-gewinde (MakerWorld copy https://makerworld.com/models/831658)
  - drone arm clamps that hold carbon tubes by shape in printed sockets: https://goodrobots.github.io/devFrame/docs/frame-details , https://makerworld.com/models/2459685 , https://makerworld.com/models/2349183

### O3. Ø2 carbon spine rods in the tongue's extreme fibres — **RANK 3** (cheapest; also the add-on for O1/O2)

- **How it works:**
  - Put 4–6 Ø2 rods along t at d = ±5.4 (1.5 mm cover), at z = ±13.5 (and 0).
  - Run them from 2 mm short of the tongue tip through the nose into the arm's straight run (≥ 40 mm).
  - These sit where the yaw bending stress peaks: the opposite of today's Ø6 rod, which is on the neutral axis.
  - The four corners also serve pitch bending.
  - Embed at a pause in printed channels; epoxy for retention.
- **Capacity (19 N·m), transformed section with n = E_c/E_p:**

  | Grade | Rods | I gain | PETG σ → SF | Rod σ → SF | Rod force / bond length at 4 MPa |
  |---|---|---|---|---|---|
  | Low | 4 | ×1.5 | 9.6 → 4.2 | 98 → 3.1 | 309 N / 12 mm |
  | Low | 6 | ×1.7 | 8.2 → 4.9 | 84 → 3.6 | 265 N / 11 mm |
  | Mid | 4 | ×2.7 | 5.3 → 7.6 | 180 → 5.6 | 566 N / 23 mm |
  | Mid | 6 | ×3.6 | 4.0 → 10 | 137 → 7.3 | 430 N / 17 mm |
  | High | 6 | ×4.3 | 3.3 → 12 | 146 → 12 | 458 N / 18 mm |

  - Development length fits: the tongue is 24.5 deep and its moment falls to 0 at the tip, and the nose side is long.
  - **The tongue stops being the problem. The blade's slot walls do not change.** With strength.md fixes 2 + 8 the outer wall is SF ≈ 2.0–2.4 at 300 N, so the system stays ≈ 2–2.4 unless O1 or O2 unloads the walls.
- **Look / release:** invisible, no change.
- **Risks:**
  - The 1.5 mm cover over Ø2 must be checked against the teardrop (the channels are horizontal in the print).
  - The Ø2 rods must stay clear of the latch block.
  - **Cutting a channel without filling it with a rod weakens the tongue**, so make the pause step part of the print guide.
- **Sources:**
  - CNC Kitchen's glued-carbon hook test (modest gains; epoxy cracked first): https://hackaday.com/2019/12/28/stronger-3d-prints-glue-or-carbon-fiber
  - JanTech pause-embed with pultruded rods (claims up to 10×, no published data): links in O1
  - Hackaday "carbon-fibre epidermis" (pause-embedded strands helped but less than cloth): https://hackaday.com/2026/08/04/strengthening-3d-prints-with-a-carbon-fiber-epidermis/
  - laser-cut metal endoskeleton: https://hackaday.com/2025/01/24/laser-cut-metal-endoskeleton-beefs-up-3d-prints

### O4. Fixed vertical pin in the wing plus a forked tongue tip (rank 4)

- **How it works:**
  - A vertical Ø8 rod crosses the slot at its deep end. It is embedded in the slot ceiling and floor; easy at a pause in the z-up wing print, since the rod is vertical.
  - The tongue tip has a rear-facing U-notch (depth r + 1, with a lead-in) that slides over it.
  - Couple lever a ≈ 17 to the mouth, F = 1 118 N.
  - **Bonus:** the pin can be the catch target for the spring tooth. That is joint_tricks T2's steel dowel, made with carbon, and it removes the 2.4 mm groove skin problem.
- **Capacity:**
  - Pin double shear 11 MPa: SF 2.7–3.7.
  - Pin bending (simply supported, 42 span): 145 MPa: SF 2.1 (low grade) to 7+.
  - **Fork tine shear 9 MPa: interlayer, SF 2.0.** The 3.9 mm tines next to a Ø8 notch in a 15.8 tongue are the weak link.
  - With Ø6: tines SF 2.5, but the pin shear falls to SF 1.5–2.1.
  - System ≈ 2.0 (tongue mouth still at 2.8, as in O2).
- **Look / release:** hidden; unchanged release.
- **Downside:** the carbon lives in the big, slow wing print. If it is damaged, you reprint the wing.
- **Source:** clevis/pin practice above; joint_tricks T2.

### O5. Twin Ø8 rods instead of a printed tongue

- **How it works:** two Ø8 rods along t (z ±10), no tongue.
- **Capacity:**
  - Rod bending 189 MPa: SF 1.6 (low) to 9.5 (high).
  - Bore bearing 12 MPa: SF 2.0.
  - Worse than O1, because nothing shares the load. Listed only to show why O1 keeps the tongue.

### O6. Full-height vertical dovetail / T-slot (69 mm) in the blade edge

- **How it works:** the mount drops down a vertical dovetail or keyhole in the front edge. This is the hook-and-drop family (`hookdrop/`).
- **Capacity:**
  - The neck that crosses the edge is ≤ 9–10 wide (it has to leave undercut walls in a 29 mm blade). Its yaw Z = 69 × 10² / 6 ≈ 1 150 mm³, which is **no better than the 15.8 × 32 tongue (1 331)**.
  - Height buys only linearly; width buys as a square. SF ≈ 2.5–2.8.
  - It also needs a 69 mm cut plus an entry window in the blade edge, which conflicts with the existing slits.
- **Look:** good. **Release:** lift-and-pull, which is fine for a parent.
- **Verdict:** not worth it for yaw.
- **Source:** Arca-type 45° dovetail and the Peak Design Capture clip (square, four-way plate): https://shuttermuse.com/peak-design-capture-pro-camera-clip-review/ , https://www.bhphotovideo.com/c/product/831911-REG/Peak_Design_Capture_Camera_Clip_System.html

### O7. Twin tongues / a fork straddling S1

- **How it works:** two tongues pass above and below S1 (or either side of it) to reach deeper. The couple lever goes from ~17 to ~40, which more than halves the wall forces.
- **Capacity:**
  - The **tongue mouth moment is unchanged**. Each tongue carries its share in a section no wider than today's.
  - The wall-bearing SF roughly doubles (2 → 4). The tongue stays at ~2.8 without O3.
- **Downsides:** more wing cutting around a protected slit, a 4-sided fit, and harder coupons.
- **Verdict:** rank behind O1, which gets a similar wall relief with two drilled holes.

### O8. Tapered / self-centring seats (Arca 45° dovetail, Manfrotto RC0 hexagon, Peak Design)

- **How it works:**
  - The camera systems resist twist by **shape plus preload**:
    - Arca uses a 45° dovetail clamped from the sides;
    - Manfrotto's older RC0 plate is a hexagon, and its "anti-twist" plates add a lip;
    - the RC2 plates also lean on a lip;
    - Peak Design uses a square plate clamped on four sides.
  - Preload removes play, so peak loads stop being impacts and the contacts spread over the full face.
- **Capacity:** no gain in static section strength. It raises the *effective* SF by keeping all contacts engaged (strength.md's seated-vs-gap spread is about 2×).
- **Apply:** joint_tricks T3 (a 1.5°/side wedge over the last 10 mm). Mech.md S1 warns that a 1.5° wedge in PETG self-locks and will seat early: open the wedge to 0.20/side.
- **Sources:**
  - RC0 hexagonal anti-twist plate: https://www.bhphotovideo.com/c/product/554223-REG/Manfrotto_030ARCH_14_030ARCH_14_Architectural_Quick_Release.html
  - RC2 anti-twist plate with lip: https://www.adorama.com/bg3157nr.html

### O9. Lessons from commercial wheelchair lateral and prosthetic hardware

- **Wheelchair laterals:**
  - Medifab Spex quick-release laterals use a lever release plus a **detent pin for transport**.
  - The mount is aluminium, clamped to the back shell or the Ø25 cane, so torsion is carried by metal well away from the pad.
  - The swing-away hardware (JAY, Spex axial) uses a vertical pivot with a slider-pin lock, i.e. a two-point couple.
  - Lesson: the commercial parts take yaw with a **two-point couple far apart in metal**, not with a single tongue. O1/O2 imitate that in carbon.
  - Sources: https://medifab.com/wp-content/uploads/2022/10/Spex-Quick-Release-Lateral-Support-Flyer-20210604.pdf , https://medifab.com/au/wp-content/uploads/2024/09/Spex-User-Manual-Lateral20240731_EN_v1.pdf , https://www.sunrisemedical.eu/seating/jay/jay-positioning/swing-away-lateral-support , patents https://patents.google.com/patent/US9717637 , https://patents.google.com/patent/US5678798 , https://patents.google.com/patent/US5362082
- **Prosthetic pyramid adapter:**
  - A 4-sided inverted pyramid is held by 4 set screws at 90°. Torque is carried by the **pyramid flats bearing on the screw tips** (shape), not by friction.
  - This is a metal, screw-preloaded system, which breaks the "screws only lock" rule here. But the idea of a faceted (non-round) seat for anti-rotation is what O1's two prongs achieve.
  - Sources: https://image-ppubs.uspto.gov/dirsearch-public/print/downloadPdf/8252066 , https://www.manualslib.com/manual/1858073/Otto-Bock-5r9.html?page=11
- **Bike frames with printed lugs and carbon tubes:** in the printed-titanium / stainless lug frames, the lug carries the load, with long overlaps and the bond in shear. The forum view is "the lugs carry it, the epoxy holds tubes in place". This supports designing the carbon to bear by shape.
  - Sources: https://theradavist.com/bastian-cycles-uses-3d-printed-titanium-lugs-with-carbon-tubes , https://www.bikeforums.net/framebuilders/600697-glued-frames-printthread.html , https://www.instructables.com/Custom-3D-Printed-Carbon-Fiber-Bike-Frame

### O10. Vertical tube socket ("seatpost / flag-pole", or a rudder pintle-and-gudgeon)

- **How it works:** the mount drops onto vertical tube(s) fixed in a notch in the blade edge.
- **Problems:**
  - One pintle is a yaw *hinge*, so it is useless alone.
  - It needs two pintles spaced along t (a ≈ 15, F ≈ 1.3 kN, similar to O2). That means a 69 × ~24 notch cut out of the blade tip, which weakens the tip and hits the protected slits.
  - Lift-off release also needs its own latch against an upward push.
- **Verdict:** not recommended. O2 gets the same couple without the notch.

### O11. Horizontal (across-the-blade) quick-release pin

- A ball-lock or detent pin along d through the blade and tongue: the Spex-style transport pin.
- **This is the wrong axis for yaw.** A pin along d resists t and z forces at the centreline, so it adds no yaw couple. It is visible on both blade faces, and it would *be* the load path, which breaks the screw/lock rule in spirit.
- It is good only as the optional pull-out lock.

### O12. Printed-in metal inserts (optional upgrade)

- The same geometry as O1/O2/O4 with steel dowels: Ø8 h6 steel has ~10× the transverse shear and E 210.
- In O2 and O4 this lifts the rod-shear term out of the picture (SF ≫ 3, then limited by PETG bearing, SF 3.4).
- Treat it as the optional upgrade for a family that wants margin. Carbon stays the base.

### O13. Print orientation, z-pinning and annealing

- **Orientation (free, keep):**
  - The carrier prints with layer normal d, so yaw bending stress (along t) stays in-plane.
  - Remaining interlayer terms: the O2/O4 shear-out (τ_dt) and the arm root.
  - Use slicer modifiers with 6 perimeters and 100 % infill locally (joint_tricks T6).
- **Z-pinning (clever, cheap):**
  - Short Ø2 rods along **d** through the tongue, either side of any vertical bore (O2/O4), cross the layer interfaces and pin them. This is the composite z-pin idea; it raises interlayer shear-out strength.
  - Unquantified; check it in a coupon.
- **Brick layers (staggered layer heights):** reported ~10 % gain for PETG in CNC Kitchen's test (secondary sources). Small; only if the slicer supports it easily.
  - Sources: https://www.3printr.com/youtuber-shows-3d-printing-method-for-more-stable-components-5069209/ , https://jlc3dp.com/blog/brick-layer-strong-prints
- **Annealing PETG: not recommended for this part.**
  - Prusa found gains only above ~110 °C, with warping, and *loss* at 70–90 °C.
  - A peer-reviewed study found +6–18 % at 75 °C.
  - The "3× stronger" PETG in CNC Kitchen's plaster/salt remelting is a pack-and-remelt process. It distorts a sliding fit and is not practical for a toleranced tongue and slot.
  - Sources: https://blog.prusa3d.com/how-to-improve-your-3d-prints-with-annealing , https://hackaday.com/2020/09/24/plaster-annealing-3d-prints-for-strength , https://pmc.ncbi.nlm.nih.gov/articles/PMC10342851 , https://hackaday.com/2026/06/22/investigating-annealing-as-fix-for-poor-cf-adhesion-in-3d-prints/

### Already-considered alternatives, for completeness

- **Saddle on both faces:** this is what option (c) in strength.md is. The thin jaws bend unless they are 12–14 mm thick, so it fails the look. O1 gets the "two points far apart" benefit while staying inside the blade.
- **Stay to the arm rest:** it removes the 80 mm lever entirely and is the only route to the 900 N target. That is a separate decision.
- **Steel dowel as a requirement:** rejected. O12 keeps steel optional.

---

## 3 Recommended build (rank 1 + rank 3)

1. Solid tongue: strength.md fix 8 and mech.md S3, with no latch channels through the tongue section.
2. Two solid Ø8 carbon prongs (or Ø6 rod inside an Ø8×6 tube), ≥ 30 mm into the nose by pause-embed, sticking out 2–3 mm past the tongue tip.
   - Centred ±24 from the tongue centre. The nose stays ≤ 69 tall.
   - Blind Ø8.1 bores in the blade with crush ribs. The lower one goes deeper if S1 allows.
3. 4–6 Ø2 spine rods in the tongue corners, running into the arm.
4. Keep the spring-button catch for pull-out only. Optionally aim its tooth at a printed-in carbon or steel cross-pin (O4 bonus, joint_tricks T2).
5. Coupons before CAD freeze:
   - a 3-point bend on one stock Ø8 rod and one Ø2 rod (to find the grade);
   - a blade-stub with one prong, 300 N lateral at 80 mm;
   - a full-joint 300 N lean, then 27.5 N·m.

**Estimated SF at the 300 N lean:**
- 19 N·m: **≈ 3** (2.5–4, bore-bearing-limited, grade- and fit-dependent).
- 24 N·m: ≈ 2.4–3.2.
- 27.5 N·m knock: ≈ 2–2.7.
- Hot-car sustained: ÷3 on the PETG terms. That is the same caveat as every printed option.

## 4 What I could not verify

- **No primary pages:**
  - No CNC Kitchen primary page (blocked earlier).
  - No numbers from JanTech's video: the "10×" claim is unsupported.
  - No Slant 3D, Teaching Tech, Maker's Muse or Thomas Sanladerer content specific to carbon-rod torsion joints turned up. They are not cited.
- **Missing data:**
  - No published epoxy-to-PETG lap-shear value.
  - The transverse shear of hobby pultruded rods is defined inconsistently (30–41 MPa "shear" vs much higher D7617 fibre-cutting values). O2 and O4's rod-shear SF depends on it.
- **Geometry:** the clearances of the prong bores and cruciform grooves to S1, the latch groove and the filled M5 hole were not checked in CAD. That is the first task.

## 5 Other sources consulted

- Carbon rod shear / D7617: https://www.scribd.com/document/720072241/ASTM-D7617-D7617M-11-2017 , https://zoltek.com/?p=333 , https://www.tapplastics.com/image/pdf/Pultruded_Carbon_Rods__Tubes.pdf , https://www.rockwestcomposites.com/47316-l96.html
- Carbon tube torsion and drilling cautions: https://rcexplorer.se/product/10x10mm-woven-carbon-fiber-arm/page/3/ , https://discuss.ardupilot.org/t/cnc-cutting-carbon-tube/92306
- Slippery carbon in printed sockets: https://test.3dsearch.net/model/parametric-peg-tv3214320 ; CF bond tests: https://cloud.wikis.utexas.edu/wiki/spaces/LHRSOLAR/pages/220234798 , https://fsae.com/forums/archive/index.php/t-3667.html
- Carbon-tube printed clamps: https://makerworld.com/models/1197423 , https://stlfinder.com/model/25mm-carbon-tube-clamp-Zw1TU8ia/1355633
