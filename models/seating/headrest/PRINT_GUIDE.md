# GbG headrest v1.0: print guide for the first prototype (Bambu P1S)

> PRE-FIRST-PRINT. The settings below are first guesses, checked only by slicing. Update them after the first prints.

## 1. Files
Everything is in `stl/`, already in print orientation (on z = 0). The Onshape **Headrest v1.0** Part Studio has the same parts in its "print layout" feature; export from there if you change a parameter.

## 2. Material
PETG for every printed part. PLA is OK only for a throwaway fit check: it creeps and cracks under the head load and in a hot car.

## 3. Slicer settings (Bambu Studio, P1S, 0.4 nozzle, 0.20 mm layers, textured PEI)
| Part | Orientation | Walls | Infill | Top/bottom | Supports | Why |
|---|---|---|---|---|---|---|
| Pad | upright, as worn | 4 | 20 % gyroid | 4 / 3 | tree, build plate only | thin curved shell: walls do the work |
| Mast | **on its side** (+x down; the nut slots face up) | 6 | 40 % gyroid | 4 / 3 | tree, build plate only | knock and side bending run along the layers, not across them. Printed upright it would split between layers. |
| Button pod | PVC bore vertical, opening up | 4 | 40 % gyroid | 4 / 3 | tree, build plate only | round, accurate bore for the PVC |
| Blank | wedge down | 4 | 40 % | 4 / 3 | none needed | |
| Fit-test bar | flat | 3 | 20 % | 4 / 3 | none | |
| Template part | flat | 2 | 15 % | 4 / 3 | none | |
| Chair `top_back` with receiver | like the original `top_back` | follow the chair's settings, but at least 4 walls round the socket | | | | |

Real slices with these settings (`generator/slice_S.sh` / `slice_E.sh`):

| Part | Time | PETG |
|---|---|---|
| Pad F3 | 6.18 h | 175.9 g |
| Mast H0 / H25 / H50 / H75 / H100 | 4.72 / 5.54 / 6.33 / 7.10 / 7.81 h | 125.2 / 152.1 / 175.9 / 197.0 / 212.9 g |
| Pod yaw 12 / 20 / 28 | 1.44 / 1.36 / 1.32 h | 29.8 / 27.2 / 25.2 g |
| Blank | 0.16 h | 2.7 g |
| Fit-test bar | 1.37 h | 43.5 g |
| Template part | 1.27 h | 43.9 g |

Orientation was chosen by slicing the alternatives. Mast H100: side 7.6 h / 192 g, on its back 8.3 h / 187 g, upright 8.2 h / 208 g, all at 4 walls 40 %. Pod: bore up 1.36 h, bore down 1.43 h, back down 1.50 h, back up 1.24 h; back up was rejected because supports would sit on the visible outside.

## 4. Order for the first prototype
1. **Fit-test bar** (1.4 h). Check the Ø8 rod slide, the nuts dropping into their traps, the PVC in the rings, and the wedge clicking into the pocket. Edit the constants at the top of `featurescript/gbg_headrest_v10.fs` (and `generator/`) if needed.
2. **Chair `top_back` with the receiver** (from the Onshape branch `headrest-v1`).
3. **Mast H50 + pad** (about 12.5 h together). Check the slide-in, the taper wedge, the lock screws and the rods.
4. **Pod + blank.**
5. **Padding coupon** (`padding/test_kit/`, 1.9 h). Use it to pick the foam attachment before cutting foam for the real pad.
6. Load tests (README), then the other heights only as children need them.

## 5. Assembly
1. Cut 2 carbon rods to **85 + H mm** and deburr the ends.
2. Slide the 2 M5 nuts into the plug slots (from −x); a drop of CA keeps them from falling out.
3. Slide the 2 M3 nuts into the slots on the tenon side edges.
4. Push the rods into the mast from the plug end until they stop, so they reach into the pad's sockets.
5. Push the plug into the `top_back` socket. Fit 2× M5×25 from behind and tighten them snug only (they lock; the socket carries the load).
6. Slide the pad down onto the tenon until the taper wedges. Fit 2× M3×30 countersunk from under the chin.
7. Pod: drop the M3 nut into its channel from outside. Push the wedge into the pocket until it clicks, then fit M3×20 countersunk from the head side through the Ø9 hole in the foam. Push in the PVC rod and fit the 2 kit wood screws from above. The blank fits the same way.
8. Foam: cut 10 mm EVA from the template, punch the Ø9 driver holes and the strap slots, heat-form only the roll, then stick the loop dots on the foam and the hook dots in the shell pockets.

## 6. Checks after printing
- The tenon slides in without force for the last 10 mm and then wedges. No rattle after the screws are in.
- The plug goes fully into the socket, and both M5 reach their nuts.
- The rods slide; ream with an 8 mm drill if tight.
- There are no sharp edges on the head side or the wings (sand any support marks). The foam covers the whole head side.
- Weigh each part and write the weight down next to the slice estimate.
