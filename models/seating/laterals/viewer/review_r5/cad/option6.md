# Option 6: socket in the child-side (inner) face, concept check (2026-11-20, no Onshape build)

**Verdict first.** A flush child-side socket is fine for seating, but it fails on strength. The plug can only be as thick as the pocket is deep, and the 20 mm blade allows at most ~10 mm (6 mm must stay on the outer face). The mount's yaw moment has to pass through that 10 mm plug, so the plug's SF is about 0.8–1.0 at a 300 N lean: no better than today's tongue. **My earlier "plug SF 5–8" in mount_options.md was wrong.** I used the plug's 28 mm length as its bending depth. The yaw (about vertical) actually bends the plug across its ~10 mm thickness. I have corrected that line in mount_options.md.

## 1. Geometry the concept would use (S frame of the FS; d' + outboard, t' + into the wing, z up)
- **Blade at the socket:** the clean 20 mm blade. Inner face d' −4.8 (t' 0) to −4.2 (t' 10); outer face d' 15.3–15.8.
- **Pocket:**
  - On the inner face, t' −14 (open at the front edge) to +13.
  - S1 starts at t' ≈ 17, so the pocket stays ≥ 3 clear of S1.
  - z 207–247 (the current mount band; 40 tall).
- **Pocket depth:** 9.8 (floor at d' +5.0), leaving a 10.3 mm outer wall. A plain straight pocket cannot hold against yaw: the plug's back end would lift out with about 0.7 kN at a 300 N lean. So the top and bottom walls need a 15° undercut (dovetail, 2.6 mm), and the plug **slides in from the front edge (+t)**, not from the child side. Sliding in from the child side (+d) would need the latch alone to hold that ~0.7 kN.
- **Pocket lip:** R2.5 all round, rounded into the inner face (exhaust-tip lip). Floor corners R1.5.
- **Carrier:**
  - Today's nose (in the 40 mm front-edge notch, 0.3 hairlines), plus a plug 27 × 40 × 9.6 that fills the pocket flush with the inner face, plus today's arm. The arm leaves the nose's front face, outside the child space.
  - Latch A as now: button 12 × 24 with a 0.8 crown on the nose's outer (parent-side) face, 4.0 travel, split orange tell sleeve, barb, 8-coil spring. The tooth hooks a rounded groove in the front of the pocket floor, so pull-out goes to solid blade.
  - Connect: slide on backwards until it clicks; the orange tell disappears. Release: press and pull forward. Same motion as today.
  - The child cannot reach the button: it is on the outer face of the wing.

## 2. Seating, both states
| | Kit OFF (bare pocket) | Kit ON (flush plug) |
|---|---|---|
| Anything proud of the inner face? | No. The pocket is recessed 9.8 with R2.5 lips. | No, the plug is flush. Only the 0.3 hairline round the plug and the nose shows. |
| Seat width | Unchanged. | Unchanged. The arm runs where today's arm runs (in front of the wing, x ≥ 128). |
| Pinch / catch | Open-front pocket 40 × 27 × 9.8. A child's finger can enter it (R2.5 lips, no sharp edge, nothing moves). A blank cover is optional if the owner wants it closed. | No moving parts on the child side. The button is on the parent side. |
| Harness strap (S1) | The strap leaves S1 at t' 17–24 and runs forward across the inner face, over the pocket. The R2.5 lips do not cut, but the pocket's front lip is a ledge the strap can drop into. | The strap lies on a flush surface: no effect. |

**Clearance to the child** (inner-face line, child centred; shoulder width and chest from ranges5.json):
- The socket region of the inner face lies at world x 121 (back end) to 138 (front end).
- At z 207–247 the band is below the axilla for every size except 2y P5 (axilla 240). There the upper arm's outside is at x ≈ B/2 + 40 ≈ 109, so the clearance is ≥ 12 mm.
- For bigger children the band is at elbow/waist height (5y P95: half chest 95 plus the elbow ≈ 140). They may already touch the bare wing there today.
- **Kit OFF and kit ON (flush) give the same clearance as the bare wing at every pad height**, because nothing on the child side changes. The pad itself is the only part in the child space, as today.
- Not verified against real meshes of the child (none exist in the repo). The numbers above come from the breadth tables.

## 3. Strength (sf_r5b.py root moments; section = the plug's root, 9.8 thick × 40 tall; PETG 40 MPa along / 22 MPa shear)
Root moments are taken from the r5b contact model: lean 300 N yaw 19.1, twist 14.9, pitch 6.6 N·m; corner knock ×1.44; 900 N ×3. The stress concentration is 1.3 at the R1.5 root.

| Item | Lean 300 N | Corner 300 N | Lean 900 N | Corner 900 N |
|---|---|---|---|---|
| Plug root (concentration / nominal) | **0.83 / 0.97** | **0.57 / 0.67** | 0.28 / 0.32 | 0.19 / 0.22 |
| Wing pocket dovetail flanks: 2 × 27 × 2.6 = 140 mm² bearing, about 0.7 kN lift-off at 300 N lean | SF ≈ 5 | SF ≈ 3.5 | ≈ 1.7 | ≈ 1.2 |
| Pocket floor (10.3 wall, supported all round; lean pushes in) | SF > 5 | > 5 | > 2 | ≈ 2 |
| Latch tooth (pull-out only) | as r5b, about 1.8 / 1.3 | | | |

The plug is the limit. Reaching SF 3 at 300 N would need a plug about 17 mm thick at 40 tall, or 13 mm at 69 tall. The blade (20 mm, 6 mm outer wall) cannot give that flush.

## 4. Comparison
| | Front-edge tongue (as built, r5b) | Option 4: tongue + 2 Ø8 prongs | Option 6: flush child-side socket |
|---|---|---|---|
| SF lean / corner 300 N | 0.95 / 0.65 (1.49 / 1.01 nominal) | ≈ 1.6–2.6 / 1.1–1.8 (prongs take 35–70 % of the yaw) | 0.83 / 0.57 (0.97 / 0.67 nominal) |
| Wing dents first? | yes, slot mouth from ≈ 170–240 N | less (prong bores in thick blade) | no (flanks and floor are fine), but the plug breaks first |
| Look, kit OFF | 40 mm notch + slot (or blank cap); swell behind the cheek | as tongue + two Ø8 rounded holes | front notch + a 40 × 27 rounded pocket on the child-side face |
| Look, kit ON | nose flush, hairlines | same | nose + flush plug, hairlines on both faces |
| Seating impact | none | none | none if flush; open pocket when the kit is off (blank optional) |
| Ease of use | slide back, click; press, pull forward | same, slightly stiffer insertion (bores) | same motion |
| Parts | carrier, button, sleeve | + 2 carbon rods bonded into the nose | carrier (nose + plug), button, sleeve; wing pocket with undercut walls |
| Print risk | slot ceiling bridge; wedge/ribs fit | + bore straightness and bond | undercut pocket overhangs on the wing (print the wing with the pocket's 15° faces as overhangs, or a slicer support); thin 10 mm plug |

## 5. Recommendation
- Do not build Option 6 as the strength fix. It is clean for seating, but the blade's 20 mm thickness caps the plug, and its SF is lower than today's tongue.
- The thing that actually raises the yaw capacity is the **section of the connector**. PETG at SF 3 needs a section modulus of about 1800–2700 mm³: for example a 15.8 mm wide tongue 64–69 mm tall (inside a cheek-width envelope), or the tongue plus Ø8 prongs.
- Of the flush, child-safe options, I would test **Option 4 (prongs)** next, or a **tall tongue** (15.8 × 64 in the old mount's envelope). Checked properly in tall_tongue.md: SF 1.57 / 1.09 at 300 N (lean / corner), so it needs the prongs too.
