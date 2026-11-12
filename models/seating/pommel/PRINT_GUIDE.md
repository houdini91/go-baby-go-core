# GbG pommel v0.7: print guide for the first prototype (P1S)

> **Status: PRE-FIRST-PRINT.** Nothing here has been printed or tested on a child yet. Fits, weights and strength figures are CAD estimates.

Written 2026-11-12. CAD: Onshape "GbG Pommel add-on (for GbG_chair_forth)", Feature Studio "GbG Pommel v0.7"; chair branch "pommel-mount", version V10.

## 1. What changed in the final CAD (update 2026-11-12, round 2: 10 mm leg-edge rounding, groove-free seat fill, teardrop rod holes, built-in snap-off support)
- Chair rod holes are now Ø6.4 (were 6.2). They are 60 mm deep (3 mm slack past the rod end) and have a lead-in chamfer.
- Pommel rod channels are Ø6.4. The crush ribs press 0.25 mm per rib (was 0.5). Ribs sit only in two 20 mm zones, one behind the hook and one at the far end, so the rod pushes in by hand. The bottom rib prints cleanly, and the channel mouth has a lead-in chamfer.
- The size code (e.g. "K23A") is now debossed into the flat bottom face. It is no longer on the supported underside.
- New "Rod fit test" bar: 96 × 15 × 14 mm, about 11 g, about 25 min.
  - Left three holes: chair-hole sizes 6.3 / 6.4 / 6.5.
  - Right three holes: pommel channel with ribs at .15 / .25 / .35.
- New feature option "Print set" lays out all 6 sizes plus the fit test on z 0, ready to export.
- Self-test in Onshape: all 6 sizes and the fit test build as single solid bodies.

## 2. Material: PETG (PLA only for a throwaway fit check)
| | PETG | PLA |
|---|---|---|
| Heat (car parked in the sun, Israel: inside 60–80 °C) | OK to about 75 °C | Softens about 55–60 °C, creeps under the bolts and rods |
| Failure mode under a hard knee or a fall | Bends, whitens | Snaps into sharp pieces |
| Ease on a P1S | Easy (Bambu PETG profile) | Easiest |

Use PLA only to check fit and shape. Don't put a child on a PLA pommel.

## 3. Slicer settings (Bambu Studio, P1S, 0.4 nozzle)
- Profile: "0.20 mm Standard" with the Bambu PETG (HF or Basic) filament profile.
- Plate: Textured PEI, with no glue needed. On a smooth PEI plate, use glue stick as a release layer, because PETG can tear the sheet.
- Orientation: as exported. The flat bottom goes on the plate. Do not rotate or flip the part.
- **Walls: 4** (about 1.7 mm). The walls carry the load, so don't go below 4.
- Top layers 5, bottom layers 4.
- **Infill: gyroid 15%.** Use 20% for K45B or for a strong or spastic child. Higher infill adds weight and time but little strength.
- **Slicer supports: OFF.** The support is built into the model: a thin rib grid called `pommel_<size>_support` sits under the saddle tail, the only overhang. It leaves a 0.25 mm (one-layer) gap, so it peels off by hand, and the face it touches is hidden in the chair recess.
  - Export each pommel **together with its `_support` part** as one STL/3MF.
  - If Bambu asks whether to load the file as a single object with multiple parts, answer **Yes**.
- The rod holes are **teardrops** with a 45° peak pointing up, so they print without sagging and without support. Keep the parts in the exported orientation, or the peak ends up pointing the wrong way.
- Edges the legs touch are rounded at **10 mm** (6.5 mm on K45A). The bed edge has a 1–2 mm 45° chamfer.
- Seam: "Back" or aligned. Keep it off the knee pad, painting it onto the hook if needed.
- Brim: not needed, because the footprint is large.
- Dry the PETG first (65 °C, 4–6 h) if the spool has been open for weeks. Wet PETG strings and prints weaker.

## 4. Weight and time (estimated from CAD volume and surface area; real slice pending)
Settings for these figures: 4 walls, gyroid 15%, PETG. The built-in snap-off support adds about 9 g per pommel. Time assumes about 28 g/h, so expect ±20%.

| Part | Size (W × L × H mm) | PETG @15% | @20% | Time @15% | Rods |
|---|---|---|---|---|---|
| Rod fit test | 96 × 15 × 14 | 11 g | 12 g | ~0.4 h | — |
| S23A short | 80 × 135 × 81 | 129 g | 146 g | ~4.5 h | — |
| S23B short | 105 × 135 × 81 | 165 g | 190 g | ~6 h | — |
| K23A long | 80 × 170 × 87 | 167 g | 189 g | ~6 h | 2 × 155 mm |
| K23B long | 105 × 170 × 87 | 212 g | 243 g | ~7.5 h | 2 × 155 mm |
| K45A long | 80 × 215 × 100 | 214 g | 244 g | ~7.5 h | 2 × 200 mm |
| K45B long | 125 × 215 × 100 | 280 g | 325 g | ~10 h | 2 × 200 mm |

All of them fit the P1S plate (256 × 256 mm).

## 5. Hardware
- 3 × M3 countersunk screws (DIN 7991 / ISO 10642) into the chair's existing nut pockets.
  - The head tops sit 22 / 23 / 25 mm above the bottom of the seat pan (rear / middle / front hole).
  - Use M3×20 in the rear and middle holes and M3×25 in the front hole.
  - Check that no screw tip sticks out under the seat, because the tilt mechanism is there.
- Long sizes only: 2 × Ø6 mm solid carbon rod, cut to 155 mm (K23) or 200 mm (K45). Cutting ±1.5 mm is fine.
  - Cut with a Dremel cut-off disc, with tape on the cut line. Wear an FFP2/FFP3 mask and glasses. Work wet or outdoors, away from electronics.
  - Sand a small bevel on both ends.
- Foam: 10–15 mm closed-cell foam on the knee pad faces, glued or Velcro. The clinical panel asked for this.

## 6. Order for the first prototype
1. **Rod fit test** (25 min). Push a rod into each hole.
   - Pick the chair hole the rod slides into by hand without wobble.
   - Pick the channel that needs a firm push and then holds the rod when shaken.
   - Report the two numbers, and the values in the CAD get updated to match.
2. **Short pommel S23A with "Chair has the recess" OFF.** This version fits today's chair with no chair changes: it bolts through the 3 existing holes and hooks over the seat's front edge. It checks fit, comfort, the hook and the bolts.
3. **Then the full system:**
   - Print the seat part from the chair branch "pommel-mount", which has the 6 mm recess and the 2 rod holes.
   - Print a long size (K23A, or K45A for an older child) with "Chair has recess" ON.
   - Press on the pad by hand. If the K45 flex (about 5–6 mm estimated) feels too springy, switch the K45 sizes to 8 mm rods.

## 7. Assembly (long sizes)
1. Push both rods into the pommel channels until they stop. About 57 mm of each rod stays sticking out behind the hook.
2. Slide the pommel backwards into the chair: the rods go into the chair holes and the saddle slides along the recess until the hook touches the seat front.
3. Fit the 3 countersunk M3 screws from the top into the existing nuts. Tighten them snugly, not hard, because PETG creeps.
4. To remove: undo the 3 screws and slide the pommel forward. The rods come out with it.

## 8. Checks after printing
- No sharp edges on the pad or neck. Run a finger over every edge: leg-contact edges are rounded 10 mm (6.5 mm on K45A); hidden edges 1.5 mm.
- Rods enter the channels and the chair holes cleanly.
- The screw heads sit flush or slightly below the surface, never proud.
- Rock the pommel by hand: no movement at the hook or the saddle.
- Have a PT/OT check it with the child before regular use.
