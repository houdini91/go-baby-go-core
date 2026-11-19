# Side-pad mount options: strength numbers for the owner's comparison (2026-11-19)

Method: the r5b contact model (`strength_r5b/sf_r5b.py`, after the strength reviewer's `loads.py`). PETG allowables: 40 MPa along the layers, 25 MPa bearing. Mount moments at 300 N:
- **lean (pad centre): yaw 19.1 N·m, twist 14.9 N·m**;
- **corner knock: yaw 27.5 N·m, twist 23.1 N·m**.

All numbers below are hand estimates. None of them is a CAD result.

## (3) Tongue as built (r5b) + load test
| | lean 300 | corner 300 | load at SF 1 (lean / corner) |
|---|---|---|---|
| Carrier tongue (root Kt 1.8 / 1.3; nominal in brackets) | 0.95 (1.49) | 0.65 (1.01) | ≈ 285 / 195 N (450 / 300 N) |
| Wing slot wall, peak bearing (local dent) / mean bearing | 0.80 / 1.77 | 0.57 / 1.27 | dents from ≈ 240 / 170 N |
| Tenon (defined weak point): root / nut slot | 3.55 / 2.87 | 1.36 / 1.16 | ≈ 410 / 350 N corner |

A load test (300 N lean, 150 N pull, pad on the long arm) decides whether the model is pessimistic. Expect first damage near the slot mouth below 300 N.

## (2) Thick saddle (clamps both blade faces)
The yaw moment has to go through the jaws as plate bending (a cantilever from the nose). Jaw stress = M_yaw / (h · t² / 6):

| Jaw thickness t | 69 mm tall: SF lean / corner 300 N | 100 mm tall outer jaw: SF lean / corner 300 N |
|---|---|---|
| 3 mm (owner's flush/proud style) | 0.22 / 0.15 | 0.31 / 0.22 |
| 6 mm | 0.87 / 0.60 | 1.26 / 0.87 |
| 8 mm | 1.54 / 1.07 | 2.23 / 1.55 |
| 10 mm | 2.41 / 1.67 | 3.49 / 2.42 |
| **12 mm** | **3.47 / 2.41** | **5.03 / 3.49** |
| **14 mm** | **4.72 / 3.28** | 6.84 / 4.75 |

- **SF ≥ 3 at 300 N (lean and corner):** 14 mm jaws at 69 mm tall, or 12 mm jaws with the outer jaw 100 mm tall. The inner (child-side) jaw is limited by S1 to about 28 mm front-to-back; the outer jaw can reach about 48 mm.
- **Wing:** the jaws bear on large areas (> 1500 mm² per face), so bearing on the wing is well under 5 MPa at 300 N. The wing would not dent before the tenon.
- **Fuse:** the tenon (≈ 350–410 N corner) then fails first, as wanted.
- **900 N at SF 2:** needs about 3x the jaw section modulus (≈ 24 mm jaws). Not realistic.
- **Look:** 12–14 mm proud jaws are bulky. Recessing helps little, because the blade is only 20 mm thick: a 6 mm recess per side leaves 8 mm of blade.

## (1) Tongue + support stay
Idea: a stay from the depth-piece foot to the arm rest, so the pad's load goes straight into the arm rest. The mount then only locates the kit and takes a fraction of the moment.

**Geometry (Part Studio 1, left side):**
- Arm rest: x 60–169, y −96…130, top z ≤ 198. Its top under the foot is lower than 156 (the LOW long setting clears it).
- Depth-piece foot: x 101–128, y 16–49, bottom z ≈ 156 (LOW long) up to ≈ 222 (HIGH long).
- The stay is a post from the foot's underside straight down to the arm rest's top. It is 40–120 mm long depending on the setting (6 pad heights), so it needs either 2–3 lengths or a sliding post with a pin.

**Stay section and SF:**
- A 20 × 30 mm PETG post fixed to the arm rest takes the pad's outward force as bending: 300 N × 40–120 mm = 12–36 N·m on Z = 30 · 20² / 6 = 2000 mm³, giving 6–18 MPa, **SF ≈ 2.2–6.7** (short post best).
- Fixing: 2 × M5 into the arm rest top (threaded inserts or a clamp): an arm-rest change.
- **What it does to the mount:** the post and the arm/mount share the load by stiffness. With a short post (≤ 60 mm), the post is several times stiffer than the 80 mm pad lever on the arm, so the mount moment drops by roughly 70–90 %. Tongue SF at 300 N would then be about 3–8; the tongue then mainly locates.
- **Fuse:** the tenon stays the weak point between the pad and the mount only for loads that bypass the post. Loads through the post go into the arm rest.
- **Costs:** an extra part per setting (or adjustable), an arm-rest modification, and a visible post under the pad (parent side).
- **Not yet checked:** clash with the arm rest top contour along y 16–49, and how a sliding/pinned post behaves under the tilt feet (±8 / ±15°).

## Summary for the decision
| Option | SF lean / corner at 300 N | Wing dents first? | Look / cost |
|---|---|---|---|
| (3) Tongue as is + test | ≈ 1.0–1.5 / 0.65–1.0 | yes (≈ 170–240 N, local) | slim; test may confirm or reject |
| (2) Saddle, 12–14 mm jaws | ≥ 3 / 2.4–3.5 | no | bulky clamp on the blade front |
| (1) Tongue + stay | ≈ 3–8 (with a short post) / similar | no | extra post to the arm rest, arm-rest change, 2–3 post lengths |
