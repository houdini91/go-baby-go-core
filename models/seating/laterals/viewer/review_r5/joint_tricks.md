# Joint tricks for the carrier tongue/slot + latch A (desk research, r5)

Scope: the architecture stays the same (PETG carrier nose + tongue sliding back into the wing slot,
held by latch A, a sliding button whose tooth sits in a groove in the wing's outer wall). The aim is
to find proven, low-effort changes that add strength and remove rattle.

## Sources and how much to trust them

12 web searches. cnckitchen.com and the Covestro PDFs refused automated fetches (403 / blocked),
so **no CNC Kitchen number below was read from a primary page**. Each item is tagged:
- [V] verified in a search result or datasheet in this session
- [S] secondary summary of a video or article (attributed, but the numbers were not checked)
- [E] standard engineering reasoning or hand calculation (mine, not from a source)

Key sources:
- Prusament PETG TDS v1.1 (Prusa Research, 2022): **HDT 68 °C at 0.45 MPa (ISO 75)** [V].
  https://storage.googleapis.com/prusa3d-content-prod-14e8-wordpress-prusament-prod/2023/10/9f229471-tds_prusament-petg_n_es.pdf
- CNC Kitchen, "What's STRONGER - 3D Prints or Wood?!": a 100 % infill PETG hook held 110 kg; hooks printed
  flat are much stronger than hooks printed upright [V for the 110 kg, S for the orientation remark].
  https://www.cnckitchen.com/blog/whats-stronger-3d-prints-or-wood
- CNC Kitchen orientation video (summarised on the Bambu forum): parts are stronger along the layers than across them [S].
  https://forum.bambulab.com/t/print-orientation-angled-3d-printing-can-make-parts-stronger/224404
- Design guide that puts PETG Z strength at about 75 % of in-plane with default profiles [S, rough].
  https://rigid-prints.pages.dev/design-guide/orientation-and-strength
- CNC Kitchen heat-set insert tests ("Are our heat-set insert datasheets wrong?"): oversized holes and thin walls
  cut pull-out strength a lot, and the screw should pull the insert *into* the plastic [S, no N values found].
  https://www.jawstec.com/?p=76921 , https://yt.dorper.me/v/yt/eNMAFTeowFs
- BASF Snap-Fit Design Guide and Bayer/Covestro "Snap-Fit Joints for Plastics": ε = 3Yh/2L²; a steep (up to 90°)
  retention face gives a joint that comes apart only by a manual release; minimum root radius about 0.5 × beam
  thickness [S, Scribd copies only]. https://www.scribd.com/document/781414276/BASF-Snap-Fit-Design-Guide ,
  https://www.plasticstoday.com/injection-molding/injection-molding-design-fundamentals-snap-fits-plastic-parts
  Bayer stress-concentration chart, from memory and not re-fetched: K ≈ 3 at r/t = 0.1, falling to ≈ 1.5 at r/t ≈ 0.6
  and flattening after that [E/memory].
- Slant 3D snap guidance (secondary): two-way snaps concentrate stress at layer boundaries; keep the flex direction
  in the layer plane; fillet the roots [S]. https://www.jawstec.com/?p=75150 , https://www.sovol3d.com/blogs/news/3d-printed-snap-fit-joints-how-to-design-clips-that-work
- Crush ribs: Dan Royer via Hackaday, "Adding crush ribs to 3D printed parts for a better press fit"
  (three small domes in the bore) [V]; Protolabs crush-rib guide, about 0.25 mm interference per rib [V].
  https://hackaday.com/2020/10/15/adding-crush-ribs-to-3d-printed-parts-for-a-better-press-fit ,
  https://www.protolabs.com/resources/design-tips/adding-crush-ribs-to-molded-parts/
- Tapered dovetail that tightens as it seats (Fabbaloo, OpenSCAD dovetails) [S]; Thomas Sanladerer (Made with
  Layers), multi-part joining with dowel pins and dovetails [S]. https://fabbaloo.com/?p=228294 ,
  https://www.3printr.com/youtuber-shows-how-to-produce-large-parts-with-small-3d-printers-3167071/
- Prusa KB, "Modeling with 3D printing in mind": zero-clearance fits do not slide together; orient structural parts
  for strength [V]. https://help.prusa3d.com/article/modeling-with-3d-printing-in-mind_164142
- No Maker's Muse primary source was found. Maker's Muse content is not cited.

## Hand numbers that drive the ranking [E]

- **Hot car**: 60–70 °C is *at* PETG's HDT (68 °C at 0.45 MPa). Any printed face that carries a constant preload,
  or a peak stress above a few MPa, will creep or set. Steel should carry the high-stress contacts. Plastic
  contact pressures should be kept at roughly ≤ 5–10 MPa.
- **Tongue prying**: a 300 N knock with a 60 mm lever gives 18 N·m. The slot resists it with a force couple whose
  arm is the engagement depth d, so F ≈ 18 000 / d:
  d = 8 mm gives 2250 N; d = 20 mm gives 900 N; **d = 30 mm gives 600 N**; d = 40 mm gives 450 N.
  This is the largest single effect. Depth ≥ 30 mm is worth more than any detail trick.
- **Tooth on the groove at 900 N**: the 3 mm shoulder × about 12 mm width is 36 mm² of bearing, which gives
  **≈ 25 MPa** on PETG. That is above the hot-car creep threshold, and the shoulder will dent and round over
  (rattle first, then release). A Ø5 steel pin bearing over 12 mm has a projected area of 60 mm², about 15 MPa.
  A Ø6 pin over 14 mm is 84 mm², about 11 MPa.
- **Tooth shear**: if the tooth root is about 4 mm × 12 mm (48 mm²), shear is about 19 MPa. That is acceptable in-plane
  only if the root is not a layer seam.

---

## Tricks (each: what / source / evidence / application / effort / risk)

### T1. Deepen the tongue and give it bearing pads at the top and bottom (compression, not prying) — **TOP 1**
- What: the moment is carried by a long engagement, and two small raised pads at each end of the tongue
  load the slot walls in compression, so the tongue does not lever on its root.
- Source: basic beam and couple mechanics [E]; Prusa KB orientation guidance [V].
- Evidence: the table above. Going from 8 mm to 30 mm cuts the wall reaction by 3.75×.
- Apply: tongue depth ≥ 30 mm (35 if the wing allows). Add 1.0 mm tall × 5 mm long contact pads on the top and
  bottom faces (the 14 mm wide faces) at the **tip** and at the **mouth**. Give the rest of the tongue
  0.3 mm clearance so only the pads touch, which makes the fit predictable. Fillet the tongue-to-nose root
  at R ≥ 3 mm (see T4). Chamfer the slot mouth 1 × 45° as a lead-in.
- Effort: minor to moderate (the wing slot must be deepened to match).  Risk: low. Check the wing wall behind the slot.

### T2. Steel dowel as the catch surface in the wing groove (the tooth hooks a pin, not a printed edge) — **TOP 2**
- What: put a bought steel dowel across the groove so the latch tooth's locking face bears on steel. The pin
  spreads the 900 N over a long line in a deep printed pocket, and the tooth wears against a hard surface.
- Source: University of Wisconsin ME design lab, "Printing a strong part" (embedding metal raises strength
  "by a very significant amount") [S], https://medesignlab.me.wisc.edu/?p=160 ; creep numbers above [E].
- Evidence: bearing pressure falls from about 25 MPa to about 11–15 MPa, and the load moves off the creep-prone shoulder edge.
- Apply: a Ø5 × 20 (or Ø6 × 20) steel dowel runs vertically through the wing's outer wall boss, parallel to the
  groove, and forms the groove's back face. The pocket is a blind hole with **3 crush ribs** (T5), 0.15–0.2 mm
  interference, and its open end is closed by the carrier when assembled, so the pin is captive. Keep ≥ 3 mm of
  plastic around the pin (more wall means more pull-out and bearing strength, per CNC Kitchen's insert findings [S]).
  Reshape the tooth's locking face as a 6° flat that is tangent to the pin. The tooth then cams the pin
  slightly *into* engagement under load.
- Effort: moderate (a wing boss change plus a tooth profile change).  Risk: low to medium. The pin must not be able to
  walk out. Use a blind hole and capture it from the other end.

### T3. Wedge (tapered) seating that pulls tight as it goes home — **TOP 3**
- What: taper the tongue's top and bottom (or its sides) by about 1–2° so it wedges in the last few mm. The
  latch tooth then holds it against that wedge, which gives zero play without tight tolerances.
- Source: tapered dovetail that "tightens as it aligns" (Fabbaloo) [S]; machine-tool taper practice [E].
- Evidence: qualitative only. A taper turns tolerance error into extra insertion depth instead of gap.
- Apply: taper the tongue and slot by 1.5° per side on the 26 mm faces, over the last 10 mm of travel only (the first
  20 mm stays a clearance slide). Set the latch groove position so the tooth engages when the taper is
  about 0.2 mm short of full seat. The spring button and the 6° locking face then draw it tight.
  Keep the taper above 1° per side so it does not self-lock in hot PETG and stays removable.
- Effort: minor (two drafts on each part).  Risk: medium. If the taper is too shallow and the wedge too tight, it can
  stick after a hot soak. Print a test coupon.

### T4. Root fillets plus print orientation that keeps the tooth and tongue roots inside layers — **TOP 4**
- What: (a) fillet every inside corner that carries load; (b) orient the part so the tooth's bending and shear plane,
  and the tongue-to-nose root, run *along* the layers and are not a layer seam.
- Source: BASF/Bayer snap-fit guides, root radius ≥ 0.5 t [S]; CNC Kitchen hook and orientation tests [V/S];
  Slant 3D's note on stress at layer boundaries [S].
- Evidence: K ≈ 3 at r/t 0.1 and ≈ 1.5 at r/t 0.6 [memory]. Interlayer PETG strength is about 75 % or less of in-plane [S].
- Apply: tooth root R ≥ 1.5–2 mm (tooth about 4 mm thick). Tongue-to-nose root R ≥ 3 mm. Groove inner corners
  in the wing R 1 mm. The carrier prints on its outer face, so the tongue and tooth lie in the XY plane and
  the 900 N tooth shear runs **in-plane**. Confirm that the tooth's locking load is not lifting a layer: the load
  should be parallel to the bed. If the button tooth is a separate part, print it on its side so the tooth's
  shear plane cuts across many extrusion lines.
- Effort: minor.  Risk: very low.

### T5. Crush ribs or sacrificial bumps for zero rattle (and for captive pins) — **TOP 5**
- What: small ridges that flatten once on first assembly. They take up print tolerance without a tight overall fit.
- Source: Dan Royer / Hackaday crush ribs (three domes) [V]; Protolabs (about 0.25 mm interference) [V]; Prusa KB on
  zero-tolerance fits not assembling [V].
- Apply: on the tongue's two 26 mm side faces, add 2 vertical ribs per side (0.8 mm wide triangles, **0.2 mm**
  interference), placed in the last 10 mm of the tongue. Use the same ribs in the dowel pockets (T2).
  Ribs only remove rattle. The pads in T1 and the wedge in T3 carry the load.
- Effort: minor.  Risk: low. In a hot car the ribs relax (creep), so expect some rattle to return over the years.
  T3's spring-driven wedge is the lasting cure, and ribs are the cheap first step.

### T6. Local perimeters and infill via a modifier (the tooth, the groove boss, the tongue root)
- What: in the slicer, make the high-stress zones effectively solid (6+ perimeters, 100 % infill) without
  making the whole wing heavy.
- Source: CNC Kitchen tests show that wall count matters more than infill [S, widely repeated]; the 110 kg PETG hook was
  100 % infill [V].
- Apply: PrusaSlicer/Bambu modifier boxes around (a) the wing groove and pin boss, 20 mm around; (b) the tongue root and
  first 15 mm of the tongue; (c) the button tooth. Set 6 perimeters and 100 % infill. Include it in the owner's print
  profile (3MF) so it is not left to chance.
- Effort: minor (slicer only).  Risk: very low. A large owner-printed wing makes the 3MF instructions important.

### T7. Steel faces on the tooth itself (pin-in-tooth)
- What: instead of (or as well as) T2, put a Ø2 or Ø3 steel pin or carbon rod across the button tooth so that metal
  meets metal or plastic at the locking face.
- Apply: a Ø3 steel dowel through the tooth tip, parallel to the groove, captive in the button body. Because Ø2
  carbon is weak in bearing and splits under point loads, use **steel**.
- Effort: moderate.  Risk: medium. It adds a small part in the button. That part is captive but needs a check. T2 alone is
  usually enough.

### T8. Ø6 carbon rod as a tongue spine (anti-prying stiffener)
- What: a Ø6 carbon rod pressed lengthwise into the tongue so that bending of a deep tongue is carried by the rod.
- Source: embedding metal and fibre reinforcement [S, UW]. [E] With depth 30, the tongue is short and stiff anyway.
- Apply: only if the tongue stays shallow or thin. A Ø6 hole along the tongue's mid-height, ≥ 3 mm walls (the
  tongue is 14 wide, which leaves 4 mm walls, so it fits). Use crush ribs, and capture the rod with the nose.
- Effort: minor.  Risk: low. It is probably unnecessary once T1 is done.

### T9. Heat-set or metal liner in the groove
- What: a bent steel strip or a heat-set insert at the groove face.
- Source: CNC Kitchen insert tests [S].
- Assessment: a heat-set insert is a threaded part, so it is the wrong part for a bearing face. A bent sheet liner is not
  standard stock. T2 (a dowel) gives the same benefit with a bought part. **Not recommended.**

### T10. Retention face angle check
- Source: BASF/PlasticsToday: a steep face gives a joint that releases only by manual release [S].
- Assessment: 6° off square (a 96° face) is correct for "release only by pressing the button". Do not
  make it shallower to make removal easier. If removal is stiff, increase button travel or add the lead-in. Under
  a 900 N load, check that the 6° face cannot cam the button back. The spring force needed to resist this is about
  900 × tan(6° − friction angle). With μ ≈ 0.2 the friction angle is 11°, so the face self-holds. [E]

## Top 5 easy wins
1. T1: tongue ≥ 30 mm deep with top and bottom pads at the tip and mouth. This cuts wall reaction 3–4× and turns prying into compression.
2. T2: a steel dowel in a captive crush-rib pocket forms the groove's catch face. This takes about 25 MPa of creep-prone PETG bearing down to about 11–15 MPa on steel.
3. T4: root fillets (tooth R ≥ 1.5–2, tongue root R ≥ 3) with in-plane load paths. This is free, and K drops from about 3 to about 1.5.
4. T3: a 1.5° per side wedge over the last 10 mm, pulled home by the latch. This gives lasting zero play that does not depend on rib creep.
5. T5 + T6: 0.2 mm crush ribs for first-fit rattle, plus slicer modifiers (6 perimeters, 100 %) at the tooth, groove and tongue root.
