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

## Option 5: more slot positions on the wing, so each pad height gets a shorter arm
Method: the same contact model, with the pad moved to each offset (`sf_r5b.py` + a drop sweep). The tongue is the r5b single tongue (15.8 × 32.1). SF is given with the root stress concentration, then nominal in brackets.

### Where the moment comes from (300 N lean at the pad centre)
Offsets of the pad centre from the mount (tongue reference, world):
- **forward (y): −80 mm**. Fixed: the pad band is y −27…55, the wing starts at y 65.5, and its front edge is vertical (it only moves back 0.05 mm per mm of height).
- **inboard (x): −51 mm**.
- **drop (z): 0 / ±11 / ±22 / ±33 / ±44 / ±55** (arm drop 22/44 ± pad screw 11).

| Offset | Moment it drives (lateral force) | At 300 N |
|---|---|---|
| forward y 80 | **yaw** (about z): the dominant one | **23.9 N·m, for every height** |
| drop z | twist about the tongue axis (t ≈ y) | 0 (drop 0) … 16.5 N·m (drop 55) |
| inboard x 51 | nothing for a lateral force. Yaw under fore/aft knocks, pitch under vertical loads | – |

### Does a shorter arm fix it? Mostly not
| Drop (pad centre to slot) | Tongue SF, lean 300 N | Tongue SF, corner 300 N |
|---|---|---|
| 55 (today's worst: L arm, outer screw) | 0.95 (1.49) | 0.65 (1.01) |
| 33 | 1.01 (1.68) | 0.68 (1.10) |
| 11 (slot at pad height, screw offset) | 1.07 (1.86) | 0.71 (1.20) |
| 0 | 1.10 (1.94) | 0.73 (1.24) |

- Placing the slots at pad height removes the twist but not the yaw. The best a single tongue reaches is **SF ≈ 1.1 (1.9 nominal) at a 300 N lean**.
- **SF 3 is not reachable by shortening the reach alone.**
- Adding the two Ø8 prongs (mount_research.md O1): if they take 50–70 % of the yaw, the tongue reaches ≈ 2.0–3.3 (lean) and 1.3–2.2 (corner) at drop 0. SF 3 at a corner knock still needs ~70 % prong share, the high-grade rod and 8 mm of bore bearing.

### Room on the real wing for more slots
- **Below the current notch (z < 207):** no room. The wing ends at z ≈ 178, and its inner face bevels away below z 200 (at z 192 the blade is only d 2.3…15.6, at z 180 d 12…16). Every slot also needs ≥ 3 mm clear of the z 186.6 hole. So no slot can serve the low heights 172/194 at drop 0.
- **Current band z 207–247:** OK, as built. S1 web 6.25, walls ≥ 5.8.
- **Upper band z ≈ 251–291** (a second slot centred ≈ 271):
  - The outer wall needs the cheek envelope (d' 24.6), which today ends at z 277. So the swell must be extended up to ≈ 291 (the owner allowed "more if clear").
  - S1 runs to z 272, so the slot keeps the same plan shape and the same 6.25 web.
  - The teardrop is ≥ 9 behind, S2 ≥ 11. The hole column (world x 75.5) is > 50 mm inboard of the slots.
  - Depth 24.5 is available.
- **Without the swell (clean 20 mm blade):** 5.8 mm walls leave only a 7.5 mm wide tongue. Yaw capacity scales with width², so it is about 0.25 of today's tongue. Not viable.
- **Top edge:** too far from every pad height.
- **Inner (child-side) face near the front.** A shallow, wide socket in the inner face. **CORRECTED 2026-11-20 (see option6.md):** the yaw bends the plug across its ≤ 10 mm thickness, not its 28 mm length, so plug SF ≈ 0.8–1.0 at 300 N lean. The figures below are kept only to show the original estimate:
  - Socket: t −14…+14 (28 long), 32 tall, 14 deep, leaving a 6 mm outer wall. S1 starts at t ≈ 17, so clearance is ≥ 3.
  - The yaw is then a bearing couple on the socket's front and back walls (arm = depth 14): ≈ 1.7 kN on 450 mm², ≈ 3.8 MPa, **bearing SF ≈ 6.5**.
  - The plug's own section (28 × 32) is ~5× stiffer in yaw than today's tongue: **plug SF ≈ 5–8** (hand estimate).
  - The pocket floor (6 mm plate, supported all round) takes the lean in compression.
  - A 300 N lean pushes the plug in. Only an inward knock pulls it out, and the latch takes that.
  - Cost: the carrier sits on the child-side face (3–6 mm proud plus the arm root) at shoulder height. Needs a fit check against the child's upper arm.

### Layout menu
| Layout | Slots (z centre) | Pad heights served (pad centre z) | Gaps | Arm variants | Tongue SF, lean / corner 300 N | Look on the bare wing |
|---|---|---|---|---|---|---|
| **1 slot (today)** | 227 (front edge) | all 6: 172–282 via drops 22/44, LOW/HIGH, screw ±11 | none | 2 lengths × LOW/HIGH × L/R = 8 | 0.95 / 0.65 (worst, drop 55); 1.1 / 0.73 at the best height | one notch (or one exhaust-tip slot) at mid-height |
| **2 slots** | 227 + 271 (front edge; swell to z 291) | 227: 216/238 (drop 0, screw ±11) and 172/194 (drop 44/22 LOW). 271: 260/282 (drop 0) | none | 0-drop + S + L LOW at 227, 0-drop at 271: 3 shapes × L/R = 6 (one arm shape with no drop serves 4 heights) | 0.95–1.1 / 0.65–0.73 (the low heights stay at drop 22–44) | two equal exhaust-tip slots 44 mm apart (centre to centre), stacked on the front edge: reads as a deliberate double vent |
| **3 slots** | 227 + 271 + (none below: no room) | as 2 slots | low heights still need a long LOW drop | as 2 slots | as 2 slots | a 3rd slot can only go higher (z ≈ 315, swell to 335); it serves no pad height (282 max), so no benefit |
| **1 slot + 2 Ø8 prongs** | 227 (+ bores at ≈ 203 and ≈ 251) | all 6 (today's arms) | none | 8 (as today) | ≈ 1.6–2.6 / 1.1–1.8 at drop 55; ≈ 2.0–3.3 / 1.3–2.2 at drop 0 (prong grade and bearing decide) | one slot plus two Ø8 round holes above and below it (small, rounded): a "three-dot" pattern |
| Inner-face socket (alternative) | 227 (child-side face, t −14…14) | all 6 via drops | none | 8 | **plug ≈ 0.83 / 0.57 (corrected, option6.md)**; wing bearing fine | one shallow rounded pocket on the child-side face; the parent side and the front edge stay intact |
| Stacked simultaneous tongues (side note) | 2 × 25.8 tall inside z 208–277 | as today | – | 8 | ≈ 2.3 / 1.6 (3.8 / 2.6 nominal) | double slot |

### Recommendation (corrected 2026-11-20)
- The inner-face socket is NOT the strong option: its plug is capped at ~10 mm by the blade (option6.md).
- No slot placement reaches SF 3, because the 80 mm forward offset sets the yaw.
- The strength has to come from the connector section. The candidates are the tongue plus Ø8 prongs (Option 4), or a tall 15.8 × 64 tongue in the old mount's envelope.
