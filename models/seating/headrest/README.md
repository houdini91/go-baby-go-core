# Headrest (head support) for GbG_chair_forth — v1.1

> ⚠️ **STATUS: PRE-FIRST-PRINT. Work in progress.**
> Nothing here has been printed, load-tested or fitted on a child yet. All fits, weights and strength figures are CAD/slicer/hand estimates.
> Do not use with a child until it has been printed, tested and checked by a PT/OT.

A printed head support for children aged 2–5 with low head control. It mounts in a socket in the chair's backrest. It has an optional pod for the kit's 20 mm PVC head-switch rod and slots for an optional forehead strap (**only if the child's therapist prescribes it**).

![Headrest v1.1 on the chair (Onshape, branch headrest-v1)](../../../assets/headrest_v10_on_chair_onshape.png)

## How it works
- **Pad (F3).** One curved shell, 5 mm thick, that cups the back of the head. It has wings above the ears and a soft roll under the occiput. The back thickens smoothly where the joint and the button pocket are, with no added blocks. The head side has 8 shallow pockets for Ø25 mm hook-and-loop (Velcro) dots that hold a 10 mm EVA foam liner.
- **Mast (M1).** One smooth printed stem with two Ø8 mm carbon rods inside it as a backup spine. At the top, a tapered curved tenon (K2) slides into a channel in the pad and wedges tight. 2× M3×30 from under the chin lock it; **the shape carries the load, the screws only lock**. The bottom is a 56×26 plug in a 45 mm deep socket in the backrest, locked by 2× M5 from behind.
- **Receiver (chair change, R2).** The chair's `top_back` part is reprinted with the socket. Behind the backrest it is a slim lens, 16 mm deep, that stays inside `top_back` (z ≥ 303; nothing reaches `mid_back` or below 2/3 of the chair's height). The rods stay inside the mast.
- **Button pod (W1).** A smooth pod that sits on the pad side. Its wedge clicks into an 8 mm triangle pocket and carries the load. 2× M3 from **outside**, on flat seats, go into nuts trapped on the pad's head side under the foam, so the pod comes off without removing the foam. The 20 mm PVC rod is clamped by 2× M3×30 through-bolts with nuts (drill the PVC through the pod). Angles 12°, 20° or 28° toward the outside. The final switch position and angle are the therapist's call at fitting. A flush **blank** (1× M3 from outside) plugs an unused pocket.
- **Heights.** One pad, five masts: H0/H25/H50/H75/H100 (pad centre 431–531 mm above the seat). The relief window in the stem appears only where the stem is long enough (H100).

## Parts and print data (real Bambu Studio slices, P1S, PETG, 0.2 mm, tree supports on the plate)
| Part | File (`stl/`) | Print orientation (already in the file) | Walls / infill | Time | PETG |
|---|---|---|---|---|---|
| Pad F3 | `headrest_pad_F3.stl` | **upside down** | 4 / 20 % gyroid | 5.4 h | 172 g |
| Mast H0 / H25 / H50 / H75 / H100 | `headrest_mast_H000…H100.stl` | **on its side** | 6 / 40 % gyroid | 4.3 / 5.1 / 5.9 / 6.6 / 7.3 h | 120 / 147 / 171 / 192 / 207 g |
| Button pod, yaw 12/20/28, right + left | `headrest_button_pod_yaw*_{right,left}.stl` | **lying on its pad-side underside** | 4 / 40 % | 1.5–1.7 h | 30–33 g |
| Pocket blank, right + left | `headrest_pocket_blank_*.stl` | wedge down | 4 / 40 % | 10 min | 3 g |
| Fit-test bar (**print first**) | `headrest_fit_test.stl` (v1.0, still valid) | flat | 3 / 20 % | 1.4 h | 44 g |
| Chair `top_back` with receiver | `chair_top_back_with_headrest_receiver.stl` | chair coordinates: print upright (top edge up) | 4+ walls round the socket | – | – |
| Foam template part | `padding/foam_template_part_2mm.stl` | flat | 2 / 15 % | 1.3 h | 44 g |

One headrest (pad + H50 mast + 1 pod + 1 blank) is about **378 g of PETG and 13 h**. Why these orientations: see [`PRINT_GUIDE.md`](PRINT_GUIDE.md) §3.

## Strength (hand beam model on the real sections, solid PETG E 1.9 GPa / 40 MPa, carbon EI added; untested)
300 N knock at the pad, 150 N push, 250 N side bash:

| Mast | Knock SF (plastic) | Side SF | Push flex at the pad | Carbon rods alone, SF | Socket wall pressure |
|---|---|---|---|---|---|
| H0 | 10.5 | 14.5 | 0.02 mm | 8.4 | 0.9 MPa |
| H25 | 7.4 | 11.0 | 0.14 mm | 5.9 | 1.3 MPa |
| H50 | 5.7 | 8.9 | 0.34 mm | 4.6 | 1.7 MPa |
| H75 | 4.6 | 7.5 | 0.69 mm | 3.7 | 2.1 MPa |
| H100 | 3.9 | 6.4 | 1.49 mm | 3.1 | 2.5 MPa |

These are for a solid section. With 6 walls and 40 % infill expect roughly 70–80 % of this, which is why H100 must pass the load test before use.

## Fits and tolerances (top of `featurescript/gbg_headrest_v10.fs`; first guesses for P1S/PETG)
| Fit | Value | Check with |
|---|---|---|
| Ø8 rod in the mast | Ø8.3 teardrop bore, open at the plug end (ream with an 8 mm drill if tight) | fit-test bar Ø8.2/8.3/8.4 |
| Plug in the chair socket | 0.2 mm per side | first print |
| K2 tenon in the pad channel, column in the pad | 0.25 mm per side, 3° taper (wedges tight) | first print |
| Triangle pocket / pod wedge | +0.15 / −0.05 = 0.2 mm per side, 8 mm deep, 2 click dimples | fit-test bar pocket + wedge |
| M3 nut traps | 5.8 mm across flats, 3 mm deep, corner up (nut 5.5 × 2.4) | fit-test bar 5.6/5.8/6.0 |
| M5 nut slots | 8.3 mm across flats (nut 8.0) | fit-test bar 8.1/8.3/8.5 |
| PVC bore in the pod | Ø20.3 teardrop | fit-test rings 20.2/20.3/20.5 |
| Clearance holes | M3 Ø3.4, M5 Ø5.4, all teardrops when horizontal in print | – |
| Countersinks (DIN 7991) | M3 Ø6.5, M5 Ø10.6 | – |
| Pod underside to pad surface | 0.3 mm gap | – |

## Hardware (3 screw types, all DIN 7991 countersunk like the pommel; every screw ends in a hex nut)
| Qty (1 pod + 1 blank) | Item | Where |
|---|---|---|
| 2 | Ø8 mm rod, **standard length per mast** (table below) | mast spine. Bought, not printed. |
| 2 + 2 nuts | **M5×30** DIN 7991 + M5 nut (DIN 934) | from behind through the receiver (flush) into nuts slid into the plug |
| 4 + 4 nuts | **M3×30** DIN 7991 + M3 nut | 2 under the chin into the tenon nuts; 2 through the pod collar and the PVC (PVC clamp) |
| 3 + 3 nuts | **M3×16** DIN 7991 + M3 nut | 2 pod → pad, 1 blank → pad; nuts trapped on the pad's head side |
| – | 10 mm EVA foam + 8 pairs of Ø25 hook-and-loop dots | see [`padding/`](padding/) and [`docs/headrest`](../../../docs/headrest/) |

With a pod on both sides: 6× M3×30, 4× M3×16.

### Rod lengths (normalized to standard Ø8 sizes)
The rod bores are open at the plug end, so a rod drops onto the floor of the chair's socket and sits **bottom-anchored**: the part that matters (the socket exit) is always reinforced, and any gap is at the low-stress top. So a rod may be shorter than its bore. Lengths below are common standard Ø8 sizes (dowel pins, linear shafts, ejector pins, M8 screws), each the longest that fits, and each above the minimum the strength model needs.

| Mast | Bore | **Rod** | Minimum (model) | Off-the-shelf no-cut alternatives (not tested) |
|---|---|---|---|---|
| H0 | 86 | **80 mm** | 60 | hardened dowel pin 8×80 |
| H25 | 111 | **100 mm** | 80 | ejector pin / linear shaft 8×100 |
| H50 | 136 | **125 mm** | 100 | ejector pin 8×125 |
| H75 | 161 | **150 mm** | 100 | linear shaft 8×150 |
| H100 | 186 | **160 mm** | 150 (must pass the stem window) | ejector pin 8×160 |

Carbon (the default): cut to these lengths. One 8×500 rod gives a pair for any one height. A strip of tape round the rod end stops it rattling. Steel alternatives are heavier (about 0.4 g/mm) and **not yet checked**: linear shafts are only skin-hardened (weaker than carbon), dowel/ejector pins must be hardened, and M8 12.9 socket screws (head in a pocket) need a model change first.

## Files
| Path | What |
|---|---|
| `featurescript/gbg_headrest_v10.fs` | Onshape FeatureScript, fully native B-rep (v1.1). Feature **GbG Headrest v1.1** (height, pod side, pod angle, rods, print layout, review gallery) and feature **GbG Headrest Receiver (chair)**. The pad and the mast stem are built from the same formulas as the Python model (B-spline surfaces); only the pod hull is section data. |
| `stl/` | Final parts exported from **Onshape's geometry**, in print orientation (decimated to ≤ 0.02 mm deviation). |
| `padding/` | 1:1 foam template (A4 PDF), the 2 mm template part to trace and mark, and `test_kit/coupon_F3_attach_test.stl`. |
| `review/` | v1.1 shape-review evidence: pod renders from Onshape, support-by-surface-class study. |
| `generator/` | The Python reference model (manifold3d): `headrest_FINAL.py` (all parts + checks), `headrest_POD11.py` (pod + `write_fsdata()` for the FeatureScript), `headrest_MB_strength.py`, `headrest_SURF.py` / `headrest_ORIENT.py` (orientation study), `headrest_REVIEW.py` (Onshape vs reference), `headrest_STL_FROM_ONSHAPE.py`. Needs `numpy manifold3d trimesh scipy`. |
| [`docs/headrest/`](../../../docs/headrest/) | Design-review pages and the padding shopping list. |

## CAD (Onshape)
- Chair doc **GbG_chair_forth**, **Main, version V12 "Main + headrest v1.1 (pre-first-print)"** (branch `headrest-v1` merged 2026-11-16):
  - Feature Studio **GbG Headrest v1.0** (the tab name) = `featurescript/gbg_headrest_v10.fs`.
  - **Part Studio 1**: last feature **GbG Headrest Receiver (chair) 1** (the socket in `top_back`).
  - **Headrest v1.0** Part Studio: the headrest on the chair (H50, right pod, 20°) and the print layout.
  - **Headrest review gallery**: every height, pod angle and side.
  - **Assembly 1**: the chair with the headrest.

## v1.1 review (what was checked, all on Onshape's own geometry)
- Every variant (5 masts, 6 pods, 2 blanks, pad) is exactly one valid closed solid.
- Shape matches the Python reference; the only differences are intended (teardrops, hex traps, screw seats).
- Functional probes: every hole, slot, bore and nut trap is open end to end on every mast, pod and the pad.
- No clash between pad, pod and blank (0.001 mm³).
- Every horizontal hole is a teardrop aimed at its part's print-up; nut traps are corner-up.
- Print orientation chosen by where support touches: none on fit surfaces, as little as possible on visible ones.

## Before this can drop "pre-first-print"
- [ ] Print the fit-test bar, then set `ROD_HOLE_D`, `NUT_M3_AF`, `NUT_M5_AF`, `TRI_CL`, `PVC_BORE` from the results.
- [ ] Print the chair `top_back` from branch `headrest-v1`, plus an H50 mast, then check the plug in the socket, the bolts and the rods.
- [ ] Print the pad and check the tenon slide-in, the taper wedge and the lock screws.
- [ ] Load tests per mast height: 300 N knock and 250 N side at the pad centre, 150 N push. No cracks, under 3 mm movement. H100 is the critical one.
- [ ] Padding test kit (coupon): dots vs strips vs adhesive EVA, heat-forming, comfort.
- [ ] PT/OT fitting on a real child: height, pod side and angle, foam thickness, and the strap only if prescribed.
- Later: S/L pad sizes (same mounting), and reach adjustment (pad further forward) with the rods.
