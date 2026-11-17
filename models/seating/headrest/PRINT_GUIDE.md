# GbG headrest v1.1: print guide for the first prototype (Bambu P1S)

> PRE-FIRST-PRINT. The settings below are first guesses, checked only by slicing. Update them after the first prints.

## 1. Files
Everything is in `stl/`, already in print orientation (on z = 0). **The folder mixes versions for now:** the pods and the fit-test bar are v1.2 (Python reference export), while the pad, masts, blanks and chair `top_back` are still v1.1 from Onshape. Print from the official set, re-exported from Onshape after the v1.2 FeatureScript has been inspected there. The Onshape **Headrest v1.0** Part Studio has the same parts in its "print layout" feature; export from there if you change a parameter.

## 2. Material
PETG for every printed part. PLA is OK only for a throwaway fit check: it creeps and cracks under the head load and in a hot car.

## 3. Orientation: chosen by where the support touches
Every surface was sorted into **visible** (finish matters), **hidden** (under the foam or against the pad) and **fit** (sliding or clamping faces). Then each part was tried in every orientation (`generator/headrest_SURF.py` + real slices, `generator/headrest_ORIENT.py`). Rule: no support on fit faces, as little as possible on visible ones, then the fewest grams.

| Part | Orientation | Support touches | Why not the others |
|---|---|---|---|
| Pad | **upside down** (top edge on the bed, brim) | fit 0 mm², visible 818 mm² near the rim, 10 g | upright: 1628 mm² of support inside the joint (tenon channel, column socket), 16 g; on its sides/back: 21–43 g |
| Mast | **on its side** (+x down, nut slots face up) | some on the stem underside (sand it), 10.8 g | strongest: knock and side loads run along the layers. Upright or upside down is cleaner but bends across the layers. |
| Pod | **lying on its pad-side underside** | almost all on the hidden underside (visible 92 mm²) | bore-up put 1202 mm² of support on the visible shell |
| Blank | wedge down | v1.2: under the cover flange only (its underside lies on the pad, hidden) | the flange is the cover for the unused pod-screw holes |

The triangle pocket already suits these orientations: in the upside-down pad it prints point-up (self-supporting), and in the pod the wedge points straight down.

**Teardrops.** Every hole that is horizontal in its print orientation is a teardrop aimed at print-up, and every hex nut trap has a corner up. So holes and nut traps print without support. Don't rotate a part from the orientation in its file without checking this.

## 4. Slicer settings (Bambu Studio, P1S, 0.4 nozzle, 0.20 mm layers, textured PEI)
| Part | Walls | Infill | Top/bottom | Supports | Real slice |
|---|---|---|---|---|---|
| Pad | 4 | 20 % gyroid | 4 / 3 | tree, build plate only; brim on | 5.43 h, 172.0 g |
| Mast H0 / H25 / H50 / H75 / H100 | 6 | 40 % gyroid | 4 / 3 | tree, build plate only | 4.31 / 5.12 / 5.87 / 6.59 / 7.26 h; 120.2 / 147.2 / 170.9 / 191.6 / 207.1 g |
| Pod yaw 12 / 20 / 28 | 4 | 40 % gyroid | 4 / 3 | tree, build plate only | v1.1: 1.67 / 1.61 / 1.53 h; 33.3 / 32.2 / 30.1 g (v1.2 not re-sliced yet; solid volume 36.0 / 33.4 / 30.5 cm³) |
| Blank | 4 | 40 % | 4 / 3 | v1.2: tree, build plate only (under the flange) | v1.1: 0.16 h, 2.6 g (v1.2 with flange not sliced yet) |
| Fit-test bar | 3 | 20 % | 4 / 3 | none | 1.37 h, 43.5 g |
| Template part | 2 | 15 % | 4 / 3 | none | 1.27 h, 44.2 g |
| Rod cutting gauge base / lid (`stl/tools/`) | 3 | 15 % | 4 / 3 | none | 2.16 h, 70.0 g / 0.24 h, 6.7 g |
| Chair `top_back` with receiver | follow the chair's settings, at least 4 walls round the socket; print upright (top edge up) so the M5 teardrops point up | | | | |

## 5. Order for the first prototype
1. **Fit-test bar** (1.4 h). Check the Ø8 rod slide (Ø8.3/8.5/8.7), the nuts in their traps (2.8 / 4.4 deep), the PVC in the rings (Ø20.6/20.8/21.0: it should slide in by hand), and the wedge clicking into the pocket. Edit the constants at the top of `featurescript/gbg_headrest_v10.fs` if needed.
2. **Rod cutting gauge** (base + lid, 2.4 h), so the rods are cut to length, square, and held still.
3. **Chair `top_back` with the receiver** (from the Onshape branch `headrest-v1`).
4. **Mast H50 + pad** (about 11.3 h together). Check the slide-in, the taper wedge, the lock screws and the rods.
5. **Pod + blank.**
6. **Padding coupon** (`padding/test_kit/`, 1.9 h). Use it to pick the foam attachment before cutting foam for the real pad.
7. Load tests (README), then the other heights only as children need them.

## 6. Assembly
1. Cut 2 carbon rods to **95 + H mm** (H0 95, H25 120, H50 145, H75 170, H100 195; v1.3: 10 mm of each rod goes on into the chair's socket floor) in the **rod cutting gauge**: push the rod against the stop, tighten the lid (2× M3×30 + nuts), wrap tape round the rod at the slot for your height, and cut through the slot with a diamond disc or a fine hacksaw. Wear an FFP3 mask, goggles and gloves; cut outside or over a damp cloth; no blower (wipe or vacuum). Seal both ends with a drop of thin CA, then round the edges with sandpaper (the bottom ends stand out of the plug).
2. Slide the 2 M5 nuts into the plug slots (from −x); a drop of CA keeps them in.
3. Slide the 2 M3 nuts into the slots on the tenon side edges.
4. Push the rods into the mast from the plug end until they stop; 9–10 mm stays out of the plug. When the mast goes into the chair, the rod ends find their two seats in the socket floor first (1 mm lead-in), then the plug slides in.
5. Push the plug into the `top_back` socket. Fit 2× **M5×30** countersunk from behind (flush). Snug only: they lock, the socket carries the load.
6. Slide the pad down onto the tenon until the taper wedges. Fit 2× **M3×25** countersunk from under the chin; the heads sit 1 mm down in flat spot-faces.
7. (v1.2b: there are no nut traps in the pad any more; the M3 nuts go into pockets in the pod and the blank, see 8 and 9.)
8. Pod: push the wedge into the pocket until it clicks. Drop an M3 nut into each of the 2 hex pockets on the outside of the pod and push it to the bottom (a strip of tape over the pocket keeps it there). Lift the foam liner (it is held by the Velcro dots) and fit 2× **M3×20** countersunk **from the head side**, through the pad into the nuts; the heads sit flush or just below the shell surface. Put the liner back. Push the PVC rod in to the bottom of the bore (slip fit), turn the switch to its angle, and pull the switch cable to the **pad side** of the pipe, away from the 3 lock holes (top, outward side, below the side), and keep it there while drilling. Wrap a **tape flag on a Ø2.5 bit about 5 mm from its tip** as a depth stop, then drill a pilot through each Ø4 hole into the PVC, using the pod as the jig: only ~5 mm deep from the flat seat, i.e. just through the pipe wall (stop when the flag touches the seat; never drill across the pipe). Fit 3× **ST3.5 × 6.5 pan-head** self-tappers. Snug only: overtightening strips the PVC. They pin the pipe against **turning and sliding**; they are not meant to hold it against a hard **pull-out**, so don't use the rod as a handle.
9. Blank: push it in until its cover flange lies on the pad's back (it hides the two unused pod-screw holes). Drop an M3 nut down its hex pocket from the outside, then lift the liner and fit 1× **M3×12** from the head side into it.
10. Foam: cut 10 mm EVA from the template (no screw holes are needed any more), heat-form only the roll, stick the loop dots on the foam and the hook dots in the shell pockets.

## 7. Checks after printing
- The tenon slides in without force for the last 10 mm and then wedges. No rattle after the screws are in.
- The plug goes fully into the socket, and both M5 reach their nuts.
- The rods slide; ream with an 8 mm drill if tight.
- The 20 mm PVC slides into the pod by hand; the 3 lock screws stop it sliding and turning.
- No sharp edges on the head side or the wings; sand the support marks on the mast underside and the pad rim. The foam covers the whole head side.
- Weigh each part and write the weight next to the slice figure.
