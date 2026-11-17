# GbG pommel v0.7: print guide for the first prototype (Bambu P1S)

> **Status: PRE-FIRST-PRINT.** Nothing here has been printed or tested on a child yet. Fits and strength figures are CAD/hand estimates; the two sliced weights below are real slicer output.

CAD: Onshape "GbG Pommel add-on (for GbG_chair_forth)", version **v0.7.2 pre-first-print**. Chair: GbG_chair_forth **Main, version V11** has the old 6 mm recess. The v0.7.3 FeatureScript (1.5 mm blended recess, new fits) isn't in Onshape yet; load it on a chair branch first.

**Fits (P1S, PETG; FDM holes print 0.1–0.3 mm small):**
- Sliding fits get at least 0.2 mm per side.
- M3 clearance holes are Ø3.5. Countersinks are Ø6.4 at 90° (head Ø6.0).
- Chair rod holes are Ø6.5 (Ø6.0 rod).
- Pommel rod channels are Ø6.5 with 3 crush ribs 0.30 mm high (rib crest Ø5.9).
- The saddle recess has 0.5 mm clearance per side.

## 1. Files
| File (in `stl/`) | What | Sliced (PETG, settings below) |
|---|---|---|
| `gbg_pommel_v07_rod_fit_test.stl` | 96 × 15 × 14 mm bar. Left 3 holes: chair rod-hole sizes 6.4 / 6.5 / 6.6. Right 3: Ø6.5 pommel channel with crush ribs 0.2 / 0.3 / 0.4 mm high (crest Ø6.1 / 5.9 / 5.7). **The committed STL is still v0.7.2 (6.3 / 6.4 / 6.5, ribs .15 / .25 / .35); re-export it from v0.7.3.** | **12.9 g, 28 min** (v0.7.2), no supports |
| `gbg_pommel_v07_S23A_todays_chair.stl` | Short pommel S23A, made for a chair **without** the recess (an older chair print). The first prototype. Still v0.7.2 (Ø3.4 holes, heads flush); re-export from v0.7.3. | **113.7 g, 3 h 40 min**, with slicer supports (106 g / 3 h without) |

Other sizes and the recess versions: export them from the Onshape Part Studio (feature "GbG Pommel v0.7 2", Print set) after the fit test has confirmed the hole sizes.

## 2. Material: PETG (PLA only for a throwaway fit check)
| | PETG | PLA |
|---|---|---|
| Heat (car parked in the sun, Israel: inside 60–80 °C) | OK to about 75 °C | Softens at about 55–60 °C, creeps under the bolts and rods |
| Failure under a hard knee or a fall | Bends, whitens | Snaps into sharp pieces |

Use PLA only to check fit and shape. Don't put a child on a PLA pommel.

## 3. Slicer settings (Bambu Studio, P1S, 0.4 nozzle)
- Profile "0.20 mm Standard", Bambu PETG filament profile. Textured PEI plate. On a smooth PEI plate, use glue stick as a release layer.
- Orientation: as exported, flat bottom on the plate. Don't rotate the part; the rod holes are teardrops with the peak pointing up.
- **Walls 4** (about 1.7 mm). Top layers 5, bottom layers 4.
- **Infill: gyroid 15%.**
- **Long sizes (K23/K45), optional:** for extra margin, add a modifier box over the neck (from the hook to the start of the knee pad) with 6 walls and 60–100% infill (see section 7).
- **Supports: ON, tree (auto), "On build plate only", threshold 30°.** The only overhang is the flat underside of the saddle, which is hidden in the chair recess, so support marks there don't matter. Check in the preview that no support grows inside the rod channels or the countersinks.
  - Optional: the Onshape feature can also generate a built-in snap-off rib support ("Print set: add built-in snap-off supports"). It's off by default; normal slicer supports are simpler.
- Seam: Back or aligned, kept off the knee pad. No brim needed.
- Dry the PETG first (65 °C, 4–6 h) if the spool has been open for weeks.

## 4. Weight and time for the other sizes (estimates)
These come from CAD volume and surface area, at 4 walls and 15% gyroid, plus about 7 g of supports. Expect ±20%. The S23A estimate (129 g) came out 12% above its real slice (114 g), so the real figures are probably a little lower.

| Part | Size (W × L × H mm) | PETG | Time | Rods |
|---|---|---|---|---|
| S23B short | 105 × 135 × 81 | ~165 g | ~6 h | – |
| K23A long | 80 × 170 × 87 | ~170 g | ~6 h | 2 × 155 mm |
| K23B long | 105 × 170 × 87 | ~215 g | ~7.5 h | 2 × 155 mm |
| K45A long | 80 × 215 × 100 | ~220 g | ~7.5 h | 2 × 200 mm |
| K45B long | 125 × 215 × 100 | ~285 g | ~10 h | 2 × 200 mm |

All of them fit the P1S plate (256 × 256 mm).

## 5. Hardware
- 3 × **M3×25** countersunk screws (DIN 7991 / ISO 10642) into the chair's existing nut pockets, the same on both chairs.
  - Every head top seats 25.0 mm above the bottom of the seat pan (z 25), on both chairs and every size. Where the saddle is taller, the head sits in a Ø6.4 counterbore with a chamfered mouth: 0.6 / 1.5 / 2.6 mm deep at the rear / middle / front screw on the recess chair, 2.1 / 3.0 / 4.1 mm on the plain chair.
  - The tip ends flush with the pan bottom (z 0), so nothing sticks out under the seat, where the tilt mechanism is. On the recess chair (mount feature, hole audit 2026-11-17) the nut pocket is 2.8 deep: the nut pulls up against the pocket roof and the tip runs 0.4 mm past it. On the plain chair the pocket is 2.4 deep and the tip ends level with the nut's lower face. The screw length tolerance is ±0.4 mm: check from below after fitting.
  - Plastic between the saddle floor and the head top: 6.5 mm (recess chair) or 5.0 mm (plain chair).
  - Nut pockets: the chair's own pockets are **5.5 AF × 2.4** (the bare nut, measured on the chair B-rep; earlier notes said 5.7). The v0.7.3 mount feature now re-cuts them to **5.8 AF × 2.8** (policy AF +0.3, depth +0.4). On a plain chair (no mount) the nuts need pressing in or a soldering iron on low; see "Known items" in the README.
- Long sizes only: 2 × Ø6 mm solid carbon rod, cut to 155 mm (K23) or 200 mm (K45), ±1.5 mm.
  - Cut with a Dremel cut-off disc, tape on the cut line. Wear an FFP2/FFP3 mask and glasses; work wet or outdoors, away from electronics.
  - Bevel both ends.
- Foam: 10–15 mm closed-cell foam on the knee-pad faces.

## 6. Order for the first prototype
1. **Rod fit test.** Push a rod into each hole.
   - Pick the chair hole the rod slides into by hand without wobble.
   - Pick the channel that needs a firm push and then holds the rod when shaken.
   - Report the two numbers; `CHAIR_BORE_D` and `RIB_I` in the FeatureScript get set from them.
2. **S23A for today's chair** (`gbg_pommel_v07_S23A_todays_chair.stl`). Bolt it through the 3 existing holes, with the hook over the seat front. Check the fit, the hook, the screws and the edges.
3. **Full system:** print the chair seat with the v0.7.3 mount (shallow recess and rod holes) and a long size (K23A or K45A), recess version. Press on the pad by hand. If K45 feels too springy, switch it to 8 mm rods.

## 7. Strength (hand estimates, untested)
On the long sizes the neck is 46 mm wide and its top stays 10 mm above the seat, level with the hook lip, with no dip. It sits in the wedge-shaped space under and between the thighs; near their undersides the thighs curve apart, leaving roughly 54 mm for the smallest child. It flows straight from the hook into the knee pad.

- v0.7–v0.7.1 had a 36 mm-wide neck that dipped to 1 mm above the seat. It looked pinched and was about half as strong.
- A separately added base under the neck (v0.7) didn't flow into the part and was removed.

Worst case is K45, at the base of the neck. Load cases come from the expert-panel review.

| Load | Safety factor (15% infill) | Safety factor (neck printed nearly solid) |
|---|---|---|
| 300 N down on the pad tip (an adult leaning on it) | ~6 | ~9 |
| 200 N sideways knee push | ~12 | ~18 |

- The carbon rods sit low in the neck and are the fail-safe: if the printed neck ever cracked, the rods would still hold the pad to the chair.
- There's about 8 mm of plastic beside each rod channel.
- K23 is stronger than K45, because its pad sits closer to the chair.

**Saddle (v0.7.3, shallow recess), hand estimate:**
- Groin zone, height above the seat at 45 / 64 / 75 / 89 mm: recess chair 5.8 / 7.0 / 7.8 / 8.9 mm (max about 9.7 at the lip); plain chair 7.2 / 8.5 / 9.3 / 10.0 mm (about 10 at the lip).
- The saddle floor moved up from z 14 to z 18.5, and the top only rose 0–4 mm to keep the groin zone low. So the saddle is thinner:
  - 7.3 / 9.3 / 10.4 mm at the rear screw / front screw / hook, down from 8 / 11 / 14.5 mm;
  - bending stress there under a 150 N pull-up on the pad is roughly 1.4–1.9× higher.
- The neck and pad figures above are unchanged.
- Optional: a 100% infill modifier over the saddle (behind the hook, up to z 30) gives back more than that margin.
- Plain chair (no recess): the saddle now has the same thickness as the recess version, within 0.2 mm. Before, it was 11 mm over the front screw and 13–14 mm high at the lip. So it has the same strength as the recess version, and the first-prototype S23A load test also checks the recess saddle. It is about 1.4× more stressed than the v0.7.2 plain saddle, with the same infill-modifier option.
- The 6 mm recess wall used to take sideways and twisting knee loads by shape. The shallow ramp barely does, so these go through the hook and the 3 screws (plus the rods on K sizes), as on the older chair without a recess. Check this in the 200 N side-load test on an S size.

## 8. Assembly (long sizes)
1. Push both rods into the pommel channels until they stop. About 57 mm stays sticking out behind the hook.
2. Slide the pommel backwards into the chair: the rods go into the chair holes and the saddle slides along the recess until the hook touches the seat front.
3. Fit the 3 countersunk M3 screws from the top. Tighten them snugly, not hard; PETG creeps.
4. To remove: undo the 3 screws and slide the pommel forward.

## 9. Checks after printing
- Run a finger over every edge. Leg-contact edges should be rounded 10 mm. Run a hand over the bare chair seat too: you shouldn't feel the recess edge.
- The rods enter the channels and the chair holes cleanly.
- The screw heads sit at the bottom of their counterbores. From below, the screw tips are flush with the pan underside and nothing sticks out.
- Rock the pommel by hand: no movement at the hook or the saddle.
- Load test before use: 300 N down, 200 N sideways, 150 N up. No cracks, under 3 mm movement.
- Have a PT/OT fit it on the child before regular use.
