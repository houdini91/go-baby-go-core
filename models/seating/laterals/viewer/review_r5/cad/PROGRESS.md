# Round 5 CAD progress (FeatureScript on Onshape branch sidepads-v1)

- 13:49 started: reading spec, v1.1 FS, r5mesh, review_r5
- 14:33 r5 FS written (gbg_sidepad_r5.fs, ~1250 lines); uploaded to Feature Studio 'GbG Side Pads v1' (v1.1 contents saved first); now debugging in Part Studio 1
- 14:52 FS builds in Part Studio 1 (wing feature + kit, both sides, 33 parts). Open: arm root fillet R3.5 fails (warning shown), visual check pending
- 15:42 owner decisions applied in FS: old pocket z 247-270 + old mount remnants CLOSED by default (fill = wing's own z 240 / z 215.2 sections, lean measured on the wing; parameter dropped); tongue taller 214.3-246.4 (was 214.3-239.7), 1.5 deg/side wedge at the tip, bearing pads (0.3 relief between), 2 crush ribs top+bottom, R3 tongue root + 3.3 mouth chamfer, R3.5 arm root; latch tooth 22.5 tall (was 19.6) x 4.5 + 0.5 root gusset, groove 4.0 deep, travel 4.3->4.9; hidden 0.4 slits in the tooth (internal perimeters). Steel dowel: NOT used (printed-only target); see numbers below.
- 15:42 engagement (S1 web 6 mm and 5.5 mm outer wall bound it): inboard 16.0-19.0 mm, outboard 7.6 mm; >= 30 mm not reachable without a visible swell of the blade (S1 slit and the blade's outer back face). Bearing face 4.05 x 22.5 mm = 91 mm2 -> 9.9 MPa at 900 N (SF 2.5 vs 25 MPa); tooth root (5.0 x 22.5 section, lever 2.6) 25 MPa -> SF 2.0 (50 MPa along layers, button printed standing); shoulder shear ~76 mm2 -> 11.8 MPa SF 2.1.
- 15:52 Part Studio 1 builds clean (no warnings): left long LOW lower, right short HIGH upper, foam 20; rods Ø6 x 39 (LOW) / x 35 (HIGH)
- 16:27 clash debugging: latch connector, tooth gusset, wedge (0.05 CAD gap) and slot-mouth chamfer fixed; fine-tessellation clash check (left, fitted): only the 4 intended crush ribs (0.2 interference) remain
- 16:39 CHECK (in Onshape, real chair): left 40 settings (arm S/L x LOW/HIGH x pad upper/lower x tilt 0/+-8/+-15) 0 clashes; right 40 settings 0 clashes; kit |x| max 163 (arm rest 165). Pressed button also checked against the chair. (Crush ribs excluded from the check: they are a deliberate 0.2 interference.)
- 16:39 rod walls (fine tessellation, LOW long carrier): >= 1.2 in tongue/nose, >= 3.2 in the arm root; bottom pad relief kept away from the rod. One Ø6 rod length for every carrier: 35.0 mm (bore t' 13.5 -> -22). Pins Ø2 x 21.0. Gauge constants SIDEPAD_ROD6_L 35.0 / SIDEPAD_PIN2_L 21.0, STL + PNG regenerated.
- 16:48 views: 'Side pads r5 - kit (print layout + coupon)' (11 parts, left set in print orientation + C7 latch coupon) and 'Side pads r5 - all parts' (34 bodies) created as duplicates of Part Studio 1 with View = print / all; shots 08, 09
- 16:52 assembly 'Side pads r5' (Part Studio 1 inserted, 33 instances fixed); docs: README round 5, REVIEW round 5 (numbers, deviations, open items), PRINT_GUIDE.md (new), shopping_list pommel line -> gauge v2
- 16:53 done for this session; tab closed, local server stopped

## Round 5b fix pass (designer / mech / strength reviews)
- 20:15 started r5b: read designer.md, mech.md, strength.md and the strength scripts; 4 plan changes marked AGREED (REVIEW.md, ROUND5_PLAN.md)
- 20:45 geometry: solid tongue 15.8 x 32.1 (0.2/side), swell behind the cheek (cheek section cut live at t' 9, carried to t' 26.5, fairs into the blade at t' 36; S1 >= 3.4 away), "deep catch" latch (bar on the tongue top, tooth at the tongue's rear, groove behind a 15 mm shoulder, skin 4.0), outer mouth chamfer 0.5, arm 22 x 31/30, tenon 16 x 24 + pins Ø2 x 29, foot 27 x 48 R10, Ø6 rod removed, travel 4.0 + crown 0.8, barb, split sleeve, 8-coil spring + counterbore, stop pads, side crush ribs, cap crush ribs + nail notch, nut crush ribs, dovetail snap bump
- 20:53 builds in Part Studio 1 (31 parts); fixed: outer mouth tool was not cut (loop over 4 cuts), plan prism double delete
- 21:00 arm root fillet: R3.5 no longer fits on the long arm (R2.5 L / R3 S) -> ROOT_R 2.5 for both, no warnings
- 21:05 CHECK left 40/40 clashing -> stop pads (cylinder end vs the leaned notch face, 0.08) built in the leaned frame; snap notch lengthened for the bump's ramp. CHECK left 0/40, right 0/40, kit |x| max 163
- 21:10 bar channel opened to the tongue's outer face (no 0.1 sliver); button print orientation on its d' 13 face
- 21:12 strength (strength_r5b/sf_r5b.py, reviewer's contact model): SF at 300 N lean ~1.0-1.5 at the tongue, 0.65-1.0 corner; tenon weak point 350-410 N corner; SF 3 at 300 N not reachable with one tongue (owner decision: second load path)
- 21:15 gauge: SP-6 removed, SP-2 29; STL/PNG regenerated; headrest README + shopping list updated
- 21:22 STLs of the fitted parts -> viewer/r5cad/ (+ print layout and all-parts grid)
