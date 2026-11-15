# Headrest (head support) for GbG_chair_forth — v1.0

> ⚠️ **STATUS: PRE-FIRST-PRINT. Work in progress.**
> Nothing here has been printed, load-tested or fitted on a child yet. All fits, weights and strength figures are CAD/slicer/hand estimates.
> Do not use with a child until it has been printed, tested and checked by a PT/OT.

A printed head support for children aged 2–5 with low head control. It mounts in a socket in the chair's backrest. It has an optional pod for the kit's 20 mm PVC head-switch rod and slots for an optional forehead strap (**only if the child's therapist prescribes it**).

![Headrest v1.0 on the chair (Onshape, branch headrest-v1)](../../../assets/headrest_v10_on_chair_onshape.png)

## How it works
- **Pad (F3).** One curved shell, 5 mm thick, that cups the back of the head. It has wings above the ears and a soft roll under the occiput. The back thickens smoothly where the joint and the button pocket are, so there are no added blocks or visible steps. The head side is clean: there are no slits, just 8 shallow pockets for Ø25 mm hook-and-loop (Velcro) dots that hold a 10 mm EVA foam liner.
- **Mast (M1).** A printed stem with two Ø8 mm carbon rods inside it as a backup spine. At the top, a tapered curved tenon (K2) slides into a channel in the pad and wedges tight. Two M3×30 screws from under the chin lock it; **the shape carries the load, the screws only lock**. The bottom is a 56×26 plug in a 45 mm deep socket in the backrest, held by 2× M5 from behind.
- **Receiver (chair change, R2).** The chair's `top_back` part is reprinted with the socket. Behind the backrest it is a slim lens shape: 16 mm deep, running down to z ≈ 303, and nothing behind the chair below about 2/3 of its height (the chair sits against the car's rear wall).
- **Button pod (W1).** It clicks into an 8 mm triangle pocket on either side of the pad with 1× M3, and holds the 20 mm PVC rod at 12°, 20° or 28° toward the outside. Where the switch finally sits and at what angle is decided by the therapist at fitting. A flush **blank** plugs an unused pocket.
- **Heights.** One pad, five masts: H0/H25/H50/H75/H100. The pad centre sits 431–531 mm above the seat. The relief window in the stem only appears where the stem is long enough (H100).

## Parts and print data (real Bambu Studio slices, P1S, PETG, 0.2 mm)
| Part | File (`stl/`) | Orientation | Walls / infill | Time | PETG |
|---|---|---|---|---|---|
| Pad F3 | `headrest_pad_F3.stl` | upright, as worn | 4 / 20 % gyroid, tree supports | 6.2 h | 176 g |
| Mast H0 / H25 / H50 / H75 / H100 | `headrest_mast_H000…H100.stl` | **on its side** (loads run along the layers) | 6 / 40 % gyroid, tree supports | 4.7 / 5.5 / 6.3 / 7.1 / 7.8 h | 125 / 152 / 176 / 197 / 213 g |
| Button pod, yaw 12/20/28, right + left | `headrest_button_pod_yaw*_{right,left}.stl` | PVC bore vertical, opening up | 4 / 40 %, tree supports | 1.3–1.4 h | 25–30 g |
| Pocket blank, right + left | `headrest_pocket_blank_*.stl` | wedge down | 4 / 40 % | 10 min | 3 g |
| Fit-test bar (**print first**) | `headrest_fit_test.stl` | flat | 3 / 20 % | 1.4 h | 44 g |
| Chair `top_back` with receiver | `chair_top_back_with_headrest_receiver.stl` | chair coordinates: orient like the original top_back | your chair settings | – | – |
| Foam template part | `padding/foam_template_part_2mm.stl` | flat | 2 / 15 % | 1.3 h | 44 g |

One headrest (pad + H50 mast + 1 pod + 1 blank) is about 380 g of PETG and 14 h. See [`PRINT_GUIDE.md`](PRINT_GUIDE.md).

## Strength (hand beam model on the real sections, solid PETG E 1.9 GPa / 40 MPa, carbon EI added; untested)
300 N knock at the pad, 150 N push, 250 N side bash:

| Mast | Knock SF (plastic) | Side SF | Push flex at the pad | If the plastic cracks: carbon rods alone, SF | Socket wall pressure |
|---|---|---|---|---|---|
| H0 | 10.5 | 14.0 | 0.02 mm | 8.4 | 0.9 MPa |
| H25 | 7.4 | 11.0 | 0.14 mm | 5.9 | 1.3 MPa |
| H50 | 5.7 | 8.9 | 0.34 mm | 4.6 | 1.7 MPa |
| H75 | 4.6 | 7.5 | 0.69 mm | 3.7 | 2.1 MPa |
| H100 | 3.9 | 6.4 | 1.49 mm | 3.1 | 2.5 MPa |

These are for a solid section. With 6 walls and 40 % infill, expect roughly 70–80 % of this. That is why the H100 mast must pass the load test below before it is used.

## Fits and tolerances (all in `featurescript/gbg_headrest_v10.fs`, top of file; first guesses for P1S/PETG)
| Fit | Value | Check with |
|---|---|---|
| Ø8 carbon rod in the mast | Ø8.3 bore (ream with an 8 mm drill if tight) | fit-test bar Ø8.2/8.3/8.4 |
| Plug in the chair socket | 0.2 mm per side | first print |
| K2 tenon in the pad channel, column in the pad | 0.25 mm per side, 3° taper (wedges tight) | first print |
| Triangle pocket / pod wedge | +0.15 / −0.05 = 0.2 mm per side, 8 mm deep, 2 click dimples | fit-test bar pocket + wedge |
| M3 nut traps | 5.8 mm across flats (nut 5.5) | fit-test bar 5.6/5.8/6.0 |
| M5 nut slots | 8.3 mm across flats (nut 8.0) | fit-test bar 8.1/8.3/8.5 |
| PVC bore in the pod | Ø20.3 | fit-test rings 20.2/20.3/20.5 |
| Clearance holes | M3 Ø3.4, M5 Ø5.4 | – |
| Pod back to pad surface | 0.3 mm gap | – |

## Hardware
| Qty | Item | Where |
|---|---|---|
| 2 | Ø8 mm pultruded carbon rod, length **85 + H mm** (H0 85, H25 110, H50 135, H75 160, H100 185) | mast spine, socket floor to inside the pad |
| 2 | M5×25 button head (ISO 7380, head Ø9.5 × 2.75 sits flush in the Ø10 counterbore) + 2 M5 nuts (DIN 934) | through the receiver back into nuts slid into the plug (from −x); ×20 only catches 1 mm of the nut |
| 2 | M3×30 countersunk (DIN 7991) + 2 M3 nuts | under the chin, 35° up, into nuts slid in from the tenon side edges |
| 1 per pod/blank | M3×20 countersunk + M3 nut | from the head side (through the Ø9 foam hole) into the pod; nut drops in from outside |
| 2 per pod | kit wood screws | pilots from above into the PVC rod |
| – | 10 mm EVA foam + 8 pairs of Ø25 hook-and-loop dots | see [`padding/`](padding/) and the shopping list in [`docs/headrest`](../../../docs/headrest/) |

## Files
| Path | What |
|---|---|
| `featurescript/gbg_headrest_v10.fs` | Onshape FeatureScript, fully native B-rep. Feature **GbG Headrest v1.0** (height, pod side, pod angle, rods, print layout, self-test) and feature **GbG Headrest Receiver (chair)**, which adds the socket to `top_back`. The pad is built from the same formulas as the Python model (B-spline surfaces + enclose); only the pod's outer hull is section data. |
| `stl/` | Final parts in print orientation (see the table). |
| `padding/` | 1:1 foam template (A4 PDF), the 2 mm template part to trace and punch, and `test_kit/coupon_F3_attach_test.stl` to try dots vs strips vs adhesive EVA cheaply. |
| `generator/` | The reviewed Python model (manifold3d). `headrest_FINAL.py` builds and checks everything; `headrest_FINAL_export.py` writes `stl/`; `headrest_FS_export.py` writes the pod data for the FeatureScript; `headrest_MB_strength.py` gives the strength table. Needs `numpy manifold3d trimesh`. `chair_mesh/chair_top_back.stl` is the chair part it builds the receiver on. |
| [`docs/headrest/`](../../../docs/headrest/) | Design-review pages (pad flow, button pod, mast, base, receiver, padding) and the padding shopping list. |

## CAD (Onshape)
- Chair doc **GbG_chair_forth**, branch **`headrest-v1`**, version **"headrest-v1: v1.0 pre-first-print"** (not merged to Main):
  - Feature Studio **GbG Headrest v1.0** = `featurescript/gbg_headrest_v10.fs`.
  - **Part Studio 1**: last feature **GbG Headrest Receiver (chair) 1**. `top_back` goes from 336.3 to 340.8 cm³.
  - **Headrest v1.0** Part Studio: the headrest on the chair (H50, right pod, yaw 20) and a print layout of the same parts.
  - **Assembly 1**: the chair with the headrest.
- Checked in Onshape: every part is 1 body; there are 0 mm³ clashes between pad, mast, pods and blanks; and the volumes match the Python model (mast H0 134.1 vs 134.3, H50 202.3 vs 202, H100 238.3 vs 238; pod 29.8 vs 30.0; pad 278 vs 276 cm³).

## Before this can drop "pre-first-print"
- [ ] Print the fit-test bar, then set `ROD_HOLE_D`, `NUT_M3_AF`, `NUT_M5_AF`, `TRI_CL`, `PVC_BORE` from the results.
- [ ] Print the chair `top_back` from branch `headrest-v1`, plus an H50 mast, then check the plug in the socket, the bolts and the rods.
- [ ] Print the pad and check the tenon slide-in, the taper wedge and the lock screws.
- [ ] Load tests per mast height: 300 N knock and 250 N side at the pad centre, 150 N push. No cracks, under 3 mm movement. H100 is the critical one.
- [ ] Padding test kit (coupon): dots vs strips vs adhesive EVA, heat-forming, comfort.
- [ ] PT/OT fitting on a real child: height, pod side and angle, foam thickness, and the strap only if prescribed.
- [ ] Then merge the Onshape branch `headrest-v1` into Main.
- Later: S/L pad sizes (same mounting), and reach adjustment (pad further forward) with the rods.
