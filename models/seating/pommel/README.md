# Pommel (medial knee support) for GbG_chair_forth — v0.7.3

> ⚠️ **STATUS: PRE-FIRST-PRINT. Work in progress.**
> Nothing here has been printed, load-tested or fitted on a child yet. All fits, weights and strength figures are CAD/hand estimates.
> Do not use with a child until it has been printed, tested and checked by a PT/OT.

A printed pommel that keeps a seated child's knees apart (alignment, or abduction when a PT prescribes it). It bolts to the GbG_chair_forth seat insert and is made for ages 2–5.

![Pommel v0.7 on the chair (K23A + carbon rods)](../../../assets/pommel_v07_on_chair_K23A.jpg)

## How it works
- **Saddle + hook.** A saddle plate sits in a very shallow recess in the seat (1.5 mm deep, 14° ramped wall, rim rounded r20), held by 3 M3×25 countersunk screws into the chair's nut pockets. Every head seats at z 25 (25 mm above the pan bottom) on both chairs and every size, so each tip ends flush with the pan underside: it fills the whole 2.4 mm nut pocket and nothing sticks out under the seat, where the tilt mechanism is. Where the saddle is taller than z 25 the head sits in a Ø6.4 counterbore (0.6–2.6 mm deep on the recess chair, 2.1–4.1 mm on the plain chair). A 90° hook wraps down the seat's front face, so pushing down on the pommel presses the hook into the chair instead of levering the screws.
- **Seat without the pommel.** The recess is shallow and blended so a child sitting on the bare seat shouldn't feel it: no edge, slopes of 14° at most. Until v0.7.2 it was 6 mm deep with square edges. See `review/` for before/after pictures.
- **Short sizes (S):** one printed part. The knee pad starts at the seat edge.
- **Long sizes (K):** the same saddle, plus a low neck and a knee pad further out. Two Ø6 mm solid carbon rods run from holes in the chair, through the hook and into the pommel.
- **Low over the screws on purpose.** That area is under the child's groin, so the saddle stays low. The tall pad sits at the knees.
  - Recess chair: 5.8 / 7.0 / 7.8 / 8.9 mm above the seat at 45 / 64 / 75 / 89 mm from the chair origin, toward the front; at most about 9.7 mm at the lip (it was about 2 / 3.6 / 5 / 8.5 mm with the 6 mm recess).
  - Plain chair (today's chair without the recess, e.g. the first S23A prototype): 7.2 / 8.5 / 9.3 / 10.0 mm, and about 10 mm at the lip. It has the same saddle thickness as the recess version (within 0.2 mm), and the same z 30 lip. Before this fix the plain version stood 7.7 / 9.6 / 11.0 / 12.6 mm, and 13–14 mm at the lip.
  - So: under 10 mm on the recess chair; about 10 mm (not under) at the front of the saddle on the plain chair. The plain chair can't go lower without making the saddle thinner than the recess version, because there the saddle sits on the seat top instead of 1.5 mm down in a recess. See the [body measurements reference](../../../docs/pommel/body_measurements_reference.html).
- **Leg-friendly.** Every edge a leg can touch is rounded 10 mm, with smaller radii only where 10 mm doesn't fit. Inside corners are filleted. The bed edge and the saddle underside get 45° chamfers, and the screw and rod holes have chamfered mouths.
- **Long sizes:** a 46 mm-wide neck, 10 mm above the seat (level with the hook lip), runs under the thighs and flows into the knee pad. See `PRINT_GUIDE.md` section 7 for strength.

![Long sizes v0.7.2](../../../assets/pommel_v072_long_sizes.png)

## Sizes
| Code | Age | Use | Width thigh → knee (mm) | Pad height above seat | Pad position past seat edge | Rods |
|---|---|---|---|---|---|---|
| S23A | 2–3 | alignment | 58 → 70 | 60 | 0–75 | – |
| S23B | 2–3 | abduction (PT only) | 80 → 105 | 60 | 0–75 | – |
| K23A | 2–3 | alignment | 58 → 70 | 65 | 40–110 | 2 × 155 mm |
| K23B | 2–3 | abduction (PT only) | 80 → 105 | 65 | 40–110 | 2 × 155 mm |
| K45A | 4–5 | alignment | 68 → 80 | 78 | 85–155 | 2 × 200 mm |
| K45B | 4–5 | abduction (PT only) | 100 → 125 | 78 | 85–155 | 2 × 200 mm |

The long sizes have a reach adjustment of ±15 or ±30 mm.

## Files
| File | What |
|---|---|
| `featurescript/gbg_pommel_v07.fs` | Onshape FeatureScript: the pommel feature (all sizes, "Print set" layout, rod fit-test bar, built-in snap-off supports). Also contains the chair mount feature. |
| `featurescript/gbg_pommel_mount_chair.fs` | The chair change only: the shallow blended saddle recess, the hook seat on the front face, M3 holes opened to Ø3.5 and 2 teardrop rod holes. Used on the chair branch. Its constants must match the copy in `gbg_pommel_v07.fs`. |
| `review/` | Before/after pictures of the seat recess (Python re-model, not Onshape) and the script that draws them. |
| `PRINT_GUIDE.md` | Material (PETG), P1S slicer settings, real and estimated weights, hardware, first-prototype order, strength, assembly. |
| `stl/gbg_pommel_v07_rod_fit_test.stl` | Rod fit-test bar. Print this first (12.9 g, 28 min). |
| `stl/gbg_pommel_v07_S23A_todays_chair.stl` | First prototype: short S23A for a chair without the recess (114 g, 3 h 40 min, with slicer supports). **Stale (v0.7.2):** re-export from v0.7.3 for the Ø3.5 holes, the M3×25 head seats and the lower plain-chair saddle. |
| [`docs/pommel/`](../../../docs/pommel/) | Design sketch, body-measurement reference, expert-panel review and background research. |

## CAD (Onshape)
- Pommel: "GbG Pommel add-on (for GbG_chair_forth)", version **v0.7.2 pre-first-print**: https://cad.onshape.com/documents/344bce461e4ccbed87302a1e
  - Part Studio 1 has feature 1 (one size shown on the chair) and feature 2 (print set: all 6 sizes, their supports and the fit test).
  - Assembly 1 shows the pommel on the chair.
- Chair: GbG_chair_forth **Main, version V11** still has the **old 6 mm recess** (the `pommel-mount` branch, merged). The v0.7.3 shallow recess in `gbg_pommel_mount_chair.fs` isn't in Onshape yet. Load it on a new chair branch and set the pommel feature to v0.7.3 before printing a recess-version pommel.

Only the first-prototype STLs are committed. Export the other sizes from the print set after the fit test confirms the hole sizes.

## Known items
- **Chair nut pockets are 5.7 AF.** The chair's own `Nut Pocket` features (chair Part Studio 1, not in these files) are 5.7 mm across flats (figure from the v0.7.3 review; not re-measured here) and 2.4 mm deep, open at the pan bottom. The print-fit policy for M3 nuts (5.5 AF) is 5.8, as in the headrest. A P1S prints holes 0.1–0.3 mm small, so the nuts may need pressing in (pull them in with the screw, or with a soldering iron on low). Not changed here: it belongs to the chair. Change it on the chair branch if the first print is too tight.
- **Screw tip tolerance.** The tip ends flush with the pan bottom (z 0) on paper. ISO 10642 allows ±0.4 mm on a 25 mm length, and the printed countersink may seat the head slightly lower or higher. Check from below after fitting: if a tip sticks out, use a shorter screw or file it flush.
- **Plastic under the heads.** Recess chair: 6.5 mm from the saddle floor to the head top (5.0 mm under the cone). Plain chair: 5.0 mm (3.5 mm under the cone). An M3×25 through a 20 mm pan fixes this: the head can't go higher without losing nut engagement. That plastic is clamped against the seat, so it only takes compression.

## Before this can drop "pre-first-print"
- [ ] Load v0.7.3 into Onshape (pommel doc + a chair branch) and check that every size regenerates without clashing with the real chair.
- [ ] Print the rod fit-test bar, then set `CHAIR_BORE_D` / `RIB_I` from the results.
- [ ] Print S23A on today's chair ("Chair has recess" off) and check the fit, the hook and the screws.
- [ ] Print the chair seat with the v0.7.3 mount and a long size, then check rod fit and pad flex.
- [ ] Load tests: 300 N down, 200 N sideways at the pad, 150 N up. No cracks, less than 3 mm movement.
- [ ] Add 10–15 mm foam padding, then a PT/OT fit on a real child.
- [x] Real slice for the fit test and S23A (see the print guide).
- [ ] Export and commit the STLs for the other sizes (after the fit test), and re-export the S23A today's-chair STL from v0.7.3.
