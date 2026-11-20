# Tall tongue (15.8 × 64 in the old mount's envelope): concept check (2026-11-20, no Onshape build)

**Verdict first: about 1.65× today's tongue, still short of SF 3.** At 300 N the tongue gets SF 1.57 (lean) and 1.09 (corner knock) with the root stress concentration, or 1.88 / 1.31 nominal. The yaw moment bends the tongue across its 15.8 mm width (stress goes with width²), and the height only adds linearly. The extra height mostly helps the twist and the bearing on the wing walls. **My earlier hand estimate "nominal SF ≈ 4" (in option6.md and mount_options.md) was optimistic: it left out the torsion and the real contact distribution.** Both files are corrected.
Method: `strength_r5b/tall.py` (the r5b contact model, the same patches and latch, tongue z changed). Results are in `strength_r5b/tall_out.json`.

## Geometry (S frame of the FS: d' + outboard, t' + into the wing, z up). Numbers for the mock agent
| Item | Value |
|---|---|
| Notch (nose band) in the front edge | z **207.0–281.0** (74), t' < 2.0 (as r5b) |
| Nose | the wing's own front section in the notch, 0.3 hairlines: z 207.3–280.7 |
| Tongue | plan as r5b: d' 2.0–17.8 (15.8 wide), back edge t' 25.48, inner diagonal (2.2, 16.57)→(9.0, 25.48); **z 214.3–278.3 (64 tall)**; root R3 inner/bottom, R0.5 outer (or R2 with a 2.0 outer mouth chamfer, see below) |
| Slot | tongue + 0.2 all round: z 214.1–278.5 |
| Mouth chamfer | 3.3 × 45° inner/floor/ceiling; 0.5 outer (the r5b strength fix) |
| Swell (old envelope, extended up) | the cheek section (d' 24.65) carried back to t' 26.5 and faired into the blade at t' 36, as r5b, but over z **202–296**: its top chamfer moves up from z 270.6–278 to **≈ 285–293**. That gives ≥ 6.4 of wall above the slot (z 278.5 → 285). The bottom stays as r5b (cheek chamfer z 202–216) |
| Latch A (same parts, moved up) | button 12 × 24, crown 0.8, centre t' −6.3, **z 252** (nose centre 244 + 8: closer to the bar). Block z 259.5–280, bar on the tongue top z **273.5–278**, tooth in a pocket at the tongue's rear (t' 17–21) z **245.5–277.5** (32 tall, reach 2.4), groove in the wing's outer wall behind the 15 mm shoulder z 245–278, skin 4.0. Drop-in from the nose top (z 280.7, hidden in the notch); barb, split sleeve, 8-coil spring, travel 4.0 as r5b |
| Arm attachment | unchanged at z 227 (inside the nose band), so all arm paths and pad heights stay |
| Bare-wing look | a 74 mm notch; with the blank cap fitted it reads as one hairline outline on the front edge |

## Clearances
| Feature | Clearance |
|---|---|
| S1 (z 202.5–272) | tongue back edge 6.25 (same plan as r5b; the taller band does not change it). Above z 272 S1 has ended |
| S2 | ≥ 11 (t' ≥ 37) |
| Teardrop channel (d' −5.4…1, t' 35–45) | ≥ 9 behind the slot |
| Hole column (world x 75.5; z 204.5 / 244.4 / 284.2) | > 50 mm inboard of the slot and swell |
| z 186 hole (Ø3.3, t' 2.1) | notch floor z 207 → 18.8 mm above the hole's top |
| Swell to S1 | ≥ 3.4 (as r5b) |

## Strength (seated contact model; PETG 40 / 22 / 25 MPa)
| Item | lean 300 | corner 300 | lean 600 | corner 600 | lean 900 | corner 900 |
|---|---|---|---|---|---|---|
| Tongue, root Kt 1.8 / 1.3 (r5b roots) | **1.57** | **1.09** | 0.78 | 0.55 | 0.52 | 0.36 |
| Tongue, nominal | 1.88 | 1.31 | 0.94 | 0.66 | 0.63 | 0.44 |
| Tongue, outer root R2 (outer chamfer 2.0, Kt 1.4) | 1.73 | 1.20 | 0.86 | 0.60 | 0.58 | 0.40 |
| **+ 2 Ø2 corner rods** (Kt 1.8/1.3; rods bonded 16 mm each side across the root, about 4.8 N·m of yaw relief) | 1.72 | 1.17 | 0.82 | 0.57 | 0.54 | 0.37 |
| Wing slot wall, peak bearing (dent onset) | 1.61 | 1.15 | 0.81 | 0.58 | 0.54 | 0.38 |
| Wing slot wall, mean bearing | 3.45 | 2.50 | 1.72 | 1.25 | 1.15 | 0.83 |
| Tooth bearing (2.4 × 32 = 77 mm²) | 2.8 | 2.1 | 1.4 | 1.0 | 0.9 | 0.7 |
| Wing shoulder (15 × 33 shear, 10.9 kN) | 15.9 | 11.7 | 7.9 | 5.8 | 5.3 | 3.9 |
| Arm (r5b 22 × 31) | 1.90 | 1.24 | 0.95 | 0.62 | 0.63 | 0.41 |
| Tenon root / nut slot (the fuse) | 3.55 / 2.87 | 1.36 / 1.16 | 1.77 / 1.43 | 0.57 / 0.58 | 1.10 / 0.96 | 0.35 / 0.39 |

**Ordering in a corner knock** (the load at SF 1):

| Part | Corner-knock load at SF 1 |
|---|---|
| Tongue | ≈ 330 N (≈ 390 nominal) |
| Wing dent onset | ≈ 345 N |
| Tenon nut slot | ≈ 350 N |
| Arm | ≈ 370 N |
| Tenon root | ≈ 410 N |

- **They all sit within about 15 %, so the fuse is not reliably first.** For a true fuse, weaken the tenon's nut-slot section to about 260–280 N corner. That still gives ≈ 650 N in a centred lean, above the tongue's ≈ 470 N.
- **In a centred lean the tongue (≈ 470 N) and the dent (≈ 480 N) still come before the tenon (> 800 N).** A tenon fuse cannot protect the mount in a pure lean, because the mount's lever is always the longer one.

**Cutting the root stress concentration:**
- An outer root R2 (with the outer mouth chamfer at 2.0 instead of 0.5) buys +10 %. It costs 1.5 mm of outer-wall contact length (t' 2.5 → 4.0), which is included above.
- Ø2 corner rods buy about +10 % and take some load off the wall. They are bond-limited at about 400 N per rod.
- **R2 root + rods together: ≈ 1.9 / 1.3 at 300 N.** Still below 3.

## Pad heights and arm variants
- The arm still attaches at z 227, so the six heights (172–282) and the 8 arm variants (S/L × LOW/HIGH × L/R) stay as in r5b.
- The taller nose (207–281) would allow a second arm attachment at z ≈ 260–271 (the HIGH arms drop 0/11 instead of 22/44). That removes no variants, and the drop barely changes the SF (mount_options.md, Option 5): **no reduction in arm variants**.

## Print orientation
- Carrier: on the nose's outer face, as r5b. The tongue (64 tall) and the arm root lie in the layer plane, and the yaw bending is in-plane (40 MPa).
- The tall nose makes the part 74 mm tall in that direction (it fits the P1S). The tooth pocket and bar channel open upward.
- Wing: the slot ceiling (z 278.5, 15.8 wide) must bridge in a z-up print. Use a 45° roof or slicer support, as today's slot.

## Compared with r5b and the other options (300 N, lean / corner, tongue with Kt)
| | r5b tongue (32 tall) | Tall tongue (64) | Tall + R2 root + 2 Ø2 rods | Tongue + 2 Ø8 prongs (Option 4) | Thick saddle 14 mm |
|---|---|---|---|---|---|
| SF | 0.95 / 0.65 | 1.57 / 1.09 | ≈ 1.9 / 1.3 | ≈ 1.6–2.6 / 1.1–1.8 | ≈ 4.7 / 3.3 |
| Wing dents first? | yes (170–240 N) | about with the tongue (≈ 345 N corner) | slightly later | less | no |
| Looks | 40 mm notch | 74 mm notch, taller swell | same | + 2 round holes | bulky clamp |

**Conclusion:** the tall tongue is worth having as the base (about 1.65× r5b, a cleaner single opening, no extra parts). Alone, or with R2 and rods, it does not reach SF 3. SF 3 needs the Ø8 prongs added (O1 in mount_research.md: tall tongue + prongs, untested estimate ≈ 2.5–4), a support stay, or the thick saddle.

## Add-on: tall tongue (R2 outer root, 2 Ø2 corner rods) + 2 Ø8 carbon prongs (ESTIMATE, 2026-11-20)
Method: `strength_r5b/tall_prong.py`, using the same contact model. The prongs remove a share of the yaw and twist from the tongue and the slot walls. The shares are assumed, not modelled, and are conservative: 35 % yaw / 60 % twist, and 50 % / 70 %. The real split depends on the rod grade and the bore fit (mount_research.md O1).
- The prong bearing in the blade bores is taken from mount_research.md: SF ≈ 2.5–3.5 at a 300 N lean.
- It is scaled by the corner/lean yaw ratio (1.44) to get ≈ 1.75–2.4 for the corner knock.

**Prong holes** (S frame, Ø8 solid carbon in Ø8.2 bores with 3 crush ribs, centred at d' 5.5):

| Prong | Centre z | Bore z | Bore t' (depth) | Walls / clearances |
|---|---|---|---|---|
| Lower | **204.0** | 199.9–208.1 | 2 → 20 (**18 deep**) | ≈ 3 to S1 (S1's front edge is at t' ≈ 25 at this d'; estimate); 5.9 to the inner face at z 200; 11.7 above the z 186 hole |
| Upper | **288.7** | 284.6–292.8 | 2 → 32 (**30 deep**) | above S1 (ends z 272); ≈ 3 to the teardrop (t' 35); ≥ 6 to both clean blade faces |

- **The notch and nose must grow to z ≈ 196–297 (101 tall, was 74)**, so that ≥ 4 mm of nose surrounds each prong root.
- The tongue stays at z 214.3–278.3, with 6 mm webs to the bores.
- The bare wing would show a 101 mm notch (or a blank cap) with two Ø8.2 rounded holes in its back face, above and below the slot.

**SF at 300 N** (lean / corner knock):

| Part | Prongs take 35 % yaw / 60 % twist | Prongs take 50 % / 70 % |
|---|---|---|
| Tongue | 3.05 / 2.17 | 3.60 / 2.63 |
| Wing slot wall, peak bearing (dent onset) | 3.23 / 2.18 | 4.35 / 2.90 |
| Tooth bearing | 4.4 / 3.2 | 5.0 / 3.8 |
| Prong bores (from O1) | ≈ 2.5–3.5 / 1.75–2.4 | ≈ 2.5–3.5 / 1.75–2.4 |
| Arm (r5b, unchanged) | 1.90 / 1.24 | 1.90 / 1.24 |
| Tenon nut slot (the fuse) | 2.87 / 1.16 | 2.87 / 1.16 |

**The mount itself reaches ≈ 2.2–3 at a corner knock and ≥ 2.5 in a lean.** The limit then moves to the arm (1.90 / 1.24) and the tenon fuse (1.16 corner, by design). So the system at 300 N is ≈ 1.9 / 1.16, and SF 3 for the whole kit would also need a stronger arm.

## Load at which each variant first gives (lean / corner knock, N; ESTIMATE = 300 × lowest SF)
| Variant | Lean (part) | Corner (part) |
|---|---|---|
| r5b tongue (as built) | ≈ 240 (wing dent) | ≈ 170 (wing dent) |
| Tall tongue | ≈ 470 (tongue) | ≈ 330 (tongue) |
| Tall + R2 root | ≈ 480 (wing dent) | ≈ 345 (wing dent) |
| Tall + 2 Ø2 rods | ≈ 515 (tongue) | ≈ 350 (tenon fuse / tongue, a tie) |
| Tall + R2 + 2 Ø2 rods | ≈ 570 (tongue / arm) | ≈ 350 (tenon fuse) |
| Tall + R2 + rods + 2 Ø8 prongs | ≈ 570 (arm) | ≈ 350 (tenon fuse) |

With the prongs, a corner knock now breaks the tenon fuse first (≈ 350 N) and the wing dents only at ≈ 650 N. A centred lean breaks the arm first (≈ 570 N). That is the intended order for the corner knock, and the arm is a reprintable part.
