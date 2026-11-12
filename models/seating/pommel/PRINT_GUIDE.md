# GbG pommel v0.7: print guide for the first prototype (Bambu P1S)

> **Status: PRE-FIRST-PRINT.** Nothing here has been printed or tested on a child yet. Fits and strength figures are CAD/hand estimates; the two sliced weights below are real slicer output.

CAD: Onshape "GbG Pommel add-on (for GbG_chair_forth)", version **v0.7.2 pre-first-print**. Chair: GbG_chair_forth **Main, version V11** (includes the 6 mm saddle recess and the 2 rod holes).

## 1. Files
| File (in `stl/`) | What | Sliced (PETG, settings below) |
|---|---|---|
| `gbg_pommel_v07_rod_fit_test.stl` | 96 × 15 × 14 mm bar. Left 3 holes: chair rod-hole sizes 6.3 / 6.4 / 6.5. Right 3: pommel channel with crush ribs pressing .15 / .25 / .35 mm. | **12.9 g, 28 min**, no supports |
| `gbg_pommel_v07_S23A_todays_chair.stl` | Short pommel S23A, made for a chair **without** the recess (an older chair print). The first prototype. | **113.7 g, 3 h 40 min**, with slicer supports (106 g / 3 h without) |

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
- 3 × M3 countersunk screws (DIN 7991 / ISO 10642) into the chair's existing nut pockets.
  - The head tops sit 22 / 23 / 25 mm above the bottom of the seat pan (rear / middle / front hole). On the older chair without the recess, add 6 mm.
  - Pick lengths so no tip sticks out under the seat, where the tilt mechanism is.
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
3. **Full system:** print the chair seat from Main V11 (recess and rod holes) and a long size (K23A or K45A), recess version. Press on the pad by hand. If K45 feels too springy, switch it to 8 mm rods.

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

## 8. Assembly (long sizes)
1. Push both rods into the pommel channels until they stop. About 57 mm stays sticking out behind the hook.
2. Slide the pommel backwards into the chair: the rods go into the chair holes and the saddle slides along the recess until the hook touches the seat front.
3. Fit the 3 countersunk M3 screws from the top. Tighten them snugly, not hard; PETG creeps.
4. To remove: undo the 3 screws and slide the pommel forward.

## 9. Checks after printing
- Run a finger over every edge. Leg-contact edges should be rounded 10 mm.
- The rods enter the channels and the chair holes cleanly.
- The screw heads sit flush or slightly below the surface.
- Rock the pommel by hand: no movement at the hook or the saddle.
- Load test before use: 300 N down, 200 N sideways, 150 N up. No cracks, under 3 mm movement.
- Have a PT/OT fit it on the child before regular use.
