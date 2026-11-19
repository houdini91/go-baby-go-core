# Side pads Round 5b: print guide (P1S, PETG)

Use the STLs named by part number (`GBG-SP-<code>-<variant>-r5.stl`), exported from the Onshape Part Studio **"Side pads r5 - kit (print layout + coupon)"**, where every part is already in its print orientation. Measurement copies of the final parts are in `viewer/r5cad/` (local only, git-ignored): `fitted/` (Part Studio 1, chair coordinates, both sides with the wings), `print_layout/` (the left set as printed, plus the C7 coupon), `all_parts/` (every variant). They are Onshape tessellations at 0.02-0.05 mm for reviewers, not print files.

| Part | Orientation | Supports | Notes |
|---|---|---|---|
| AR carrier | on the nose's outer face (button pocket on the bed) | tree support under the arm bend only | the tongue's bending stresses run in the layer plane; the bar channel and tooth pocket open upward (no support) |
| BT button-catch | on its inner face (block, bar and tooth share the plane d' 13; the cap stands up) | none | the tooth's 6° face and 45° lead-in print as walls; the bar bends in the layer plane |
| TS tell sleeve | flat ring | none | orange PETG; the 1.0 split is printed open |
| DP depth piece | foot face down (tilted feet: the tilted face down) | none | dovetail flanks at 60° print clean; the nut pockets open on the bed side |
| PD pad shell | child side down | tree supports on the child side only | |
| RG clip ring | flat side down, or print flat and bend in | tree supports if printed curved | |
| CP blank cap | outer face down | none | crush ribs on the stub; nail notch under the rear edge |
| WG wing reprint | as the chair's wing; check the notch's bridged ceiling and the groove's roof in the slicer | slicer supports | the swell behind the cheek is solid: give it the same walls/infill as the cheek |

**Slicer modifiers (internal strength, no extra parts):** use 6 perimeters and 100 % infill in modifier volumes at
- the carrier's tongue (whole tongue, t' 1-25.5) and the arm root (first 15 mm),
- the button's bar and tooth,
- the wing's slot walls and the shoulder in front of the groove (outer wall t' 2-21, z 214-247),
- the tenon (the defined weak point, see REVIEW.md: keep it as specified so it breaks where intended).

**Assembly order:** bond the 4 Ø2 x 29 pins into the tenon bores with high-Tg epoxy (they stop 2 mm short of the tenon tip). Press the M5 nut into the tenon's side slot (crush ribs). Drop the spring (OD 6, 8 coils, L0 16) into its slot and counterbore. Press the button-catch in and slide it down from the nose's top face into its channels until the flange clicks past the barb. Release it: the cap pops out into the lip. Snap the orange sleeve in under the lip (the split goes to the back). Press the two M5 nuts into the foot's pockets (crush ribs). Slide the pad's rail into a groove from the back until the snap bump clicks into its notch, then fit the M5 x 10 from the child side. Put the depth piece on the tenon and fit the M5 x 12. Fit the foam and the cover; press the clip ring in until the barb clicks.

**Taking the button out (service):** pry the sleeve out at its split with a pick, press the button, push the flange past the barb with the pick and lift the button out of the top.

**Fits to check on the coupon first:** sleeve snap (0.3/side), the latch click and the cam-in of the 45° lead-in, the barb, the crush ribs (0.3), the rail's snap bump, the nut crush ribs.
