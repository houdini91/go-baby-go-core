# GbG headrest v1.1: print guide for the first prototype (Bambu P1S)

> PRE-FIRST-PRINT. The settings below are first guesses, checked only by slicing. Update them after the first prints.

## 1. Files
Everything is in `stl/`, exported from Onshape and already in print orientation (on z = 0). The Onshape **Headrest v1.0** Part Studio has the same parts in its "print layout" feature; export from there if you change a parameter.

## 2. Material
PETG for every printed part. PLA is OK only for a throwaway fit check: it creeps and cracks under the head load and in a hot car.

## 3. Orientation: chosen by where the support touches
Every surface was sorted into **visible** (finish matters), **hidden** (under the foam or against the pad) and **fit** (sliding or clamping faces). Then each part was tried in every orientation (`generator/headrest_SURF.py` + real slices, `generator/headrest_ORIENT.py`). Rule: no support on fit faces, as little as possible on visible ones, then the fewest grams.

| Part | Orientation | Support touches | Why not the others |
|---|---|---|---|
| Pad | **upside down** (top edge on the bed, brim) | fit 0 mm², visible 818 mm² near the rim, 10 g | upright: 1628 mm² of support inside the joint (tenon channel, column socket), 16 g; on its sides/back: 21–43 g |
| Mast | **on its side** (+x down, nut slots face up) | some on the stem underside (sand it), 10.8 g | strongest: knock and side loads run along the layers. Upright or upside down is cleaner but bends across the layers. |
| Pod | **lying on its pad-side underside** | almost all on the hidden underside (visible 92 mm²) | bore-up put 1202 mm² of support on the visible shell |
| Blank | wedge down | none | – |

The triangle pocket already suits these orientations: in the upside-down pad it prints point-up (self-supporting), and in the pod the wedge points straight down.

**Teardrops.** Every hole that is horizontal in its print orientation is a teardrop aimed at print-up, and every hex nut trap has a corner up. So holes and nut traps print without support. Don't rotate a part from the orientation in its file without checking this.

## 4. Slicer settings (Bambu Studio, P1S, 0.4 nozzle, 0.20 mm layers, textured PEI)
| Part | Walls | Infill | Top/bottom | Supports | Real slice |
|---|---|---|---|---|---|
| Pad | 4 | 20 % gyroid | 4 / 3 | tree, build plate only; brim on | 5.43 h, 172.0 g |
| Mast H0 / H25 / H50 / H75 / H100 | 6 | 40 % gyroid | 4 / 3 | tree, build plate only | 4.31 / 5.12 / 5.87 / 6.59 / 7.26 h; 120.2 / 147.2 / 170.9 / 191.6 / 207.1 g |
| Pod yaw 12 / 20 / 28 | 4 | 40 % gyroid | 4 / 3 | tree, build plate only | 1.67 / 1.61 / 1.53 h; 33.3 / 32.2 / 30.1 g |
| Blank | 4 | 40 % | 4 / 3 | none | 0.16 h, 2.6 g |
| Fit-test bar | 3 | 20 % | 4 / 3 | none | 1.37 h, 43.5 g |
| Template part | 2 | 15 % | 4 / 3 | none | 1.27 h, 44.2 g |
| Chair `top_back` with receiver | follow the chair's settings, at least 4 walls round the socket; print upright (top edge up) so the M5 teardrops point up | | | | |

## 5. Order for the first prototype
1. **Fit-test bar** (1.4 h). Check the Ø8 rod slide, the nuts in their traps, the PVC in the rings, and the wedge clicking into the pocket. Edit the constants at the top of `featurescript/gbg_headrest_v10.fs` if needed.
2. **Chair `top_back` with the receiver** (from the Onshape branch `headrest-v1`).
3. **Mast H50 + pad** (about 11.3 h together). Check the slide-in, the taper wedge, the lock screws and the rods.
4. **Pod + blank.**
5. **Padding coupon** (`padding/test_kit/`, 1.9 h). Use it to pick the foam attachment before cutting foam for the real pad.
6. Load tests (README), then the other heights only as children need them.

## 6. Assembly
1. Cut 2 carbon rods to the **standard length for the mast** (H0 80, H25 100, H50 125, H75 150, H100 160 mm; README "Rod lengths") and deburr the ends. Wrap tape round the cut, wear a mask, cut outside.
2. Slide the 2 M5 nuts into the plug slots (from −x); a drop of CA keeps them in.
3. Slide the 2 M3 nuts into the slots on the tenon side edges.
4. Push the rods into the mast from the plug end. They may be shorter than the bore: once the plug is in the socket they rest on the socket floor (bottom-anchored), which is correct.
5. Push the plug into the `top_back` socket. Fit 2× **M5×30** countersunk from behind (flush). Snug only: they lock, the socket carries the load.
6. Slide the pad down onto the tenon until the taper wedges. Fit 2× **M3×30** countersunk from under the chin.
7. Press 3 M3 nuts into the traps on the pad's head side, at each pocket you use (2 per pod, 1 per blank).
8. Pod: push the wedge into the pocket until it clicks, then fit 2× **M3×16** countersunk from outside on the flat seats. Push in the PVC rod, drill it Ø3.2 through the 2 clamp holes using the pod as the jig, then fit 2× **M3×30** with nuts underneath.
9. Blank: push it in, then fit 1× **M3×16** from outside.
10. Foam: cut 10 mm EVA from the template (no screw holes are needed any more), heat-form only the roll, stick the loop dots on the foam and the hook dots in the shell pockets.

## 7. Checks after printing
- The tenon slides in without force for the last 10 mm and then wedges. No rattle after the screws are in.
- The plug goes fully into the socket, and both M5 reach their nuts.
- The rods slide; ream with an 8 mm drill if tight.
- No sharp edges on the head side or the wings; sand the support marks on the mast underside and the pad rim. The foam covers the whole head side.
- Weigh each part and write the weight next to the slice figure.
