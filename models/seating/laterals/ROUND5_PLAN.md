# Side pads (lateral trunk supports): Round 5 plan

Date: 2026-11-19 (updated with the owner's latest decisions). Applies to v1.1 (`featurescript/gbg_sidepad_v1.fs`, [README.md](README.md), [REVIEW.md](REVIEW.md)).
Working copies: Onshape branch `sidepads-v1`, git branch `laterals-v1`.
Final mock designs and diagrams will be added to this file later.

**Status tags used on every item**

| Tag | Meaning |
|---|---|
| **AGREED** | The owner has decided. Build it. |
| **RECOMMENDED** | Reviewers propose it, and it is waiting for the owner's OK. |
| **OPEN** | Still to decide together. It may need a mock-up, coupon or check first. |

---

## 1 Purpose & status

This is the official issue of everything agreed for the side-pad kit up to Round 5. **Round 5 delivers production parts:** every part of the arm assembly is fine-tuned and gets a part number (§8). Nothing here has been built in Onshape yet. v1.1 is still the model of record until step 6.5 below.

| # | Area | Where it stands | Status |
|---|---|---|---|
| 1.1 | **One mount only** at tongue z 227: a 40 mm piece of the wing's front edge becomes the kit's nose | Agreed | AGREED |
| 1.2 | **6 pad heights, 22 mm apart:** 2 arm lengths × LOW/HIGH flip × 2 pad-screw positions | Agreed | AGREED |
| 1.3 | **Button form:** 12 × 24 stadium, square push, thumb scoop, orange ring only when not latched | Agreed | AGREED |
| 1.4 | Latch behind the button | **A: the button is the catch** (one sliding part; tooth in a groove in the wing), with the agreed upgrades: bigger tooth sized for the ~900 N corner knock, 6° tapered locking face (no rattle), sharp-edged click entry, orange not-latched ring; steel dowel tooth = coupon option. B (hook-and-drop) and C (steel pin) considered, not chosen | AGREED |
| 1.5 | Latch upgrades (taper, click, flush indicator, dowel coupon) | Apply to whichever latch wins | AGREED |
| 1.6 | Carbon: one Ø6 rod in the arm, 4 × Ø2 tenon pins kept, gauge v2 | Agreed | AGREED |
| 1.7 | Clip ring gets positive retention; cover wrap reviewed | Agreed | AGREED |
| 1.8 | Production parts with part numbers (§8) | Agreed | AGREED |
| 1.9 | Tilt in the depth piece's foot (A/B feet) | In production scope; angle set awaits OK | RECOMMENDED |
| 1.10 | Screw-load fixes (screws only lock) | Audit done (review_r5/tilt §4–5) | RECOMMENDED |

---

## 2 Decisions agreed (with who/why/source)

### 2.1 Agreed by the owner

| # | Decision | Who / why | Source | Status |
|---|---|---|---|---|
| 2.1 | **Button principle:** the face meets the push squarely (no sideways slide). It is big and very easy to press, with a squeeze against a reaction surface opposite. | Owner chose the designer's direction over options 0–5. The v1.1 pill sits ~50° off its push and can jam: self-locks from μ ≈ 0.51, est. 38–99 N. | Review page "The release button…"; `viewer/site/index.html` | AGREED |
| 2.2 | **Button form (the one the owner liked):** 12 × 24 stadium on the nose's outer face at the arm's height; face square to its press; shallow thumb scoop; chair colour with an **orange ring visible only when NOT latched**; flush = latched. | Owner. | `viewer/review_r5/button_design/` (`fig2_favourite_spec.png`) | AGREED |
| 2.3 | **The mount must "complete the chair":** remove the v1.1 wing-tip block (32 × 54 × 149 mm: `TIP_D` −7..25, `TIP_T` −16..38, `TIP_Z` 198..347), the slot rails and the slot blanks. | Owner: the block looked bolted on. All Round 4 reviewers agreed. | `gbg_sidepad_v1.fs` lines 33–35, 608 | AGREED |
| 2.4 | **One mount only**, tongue centre **z 227**. No 3-position slot band, no blanks, no fillers. Only a **40 mm piece of the wing's front edge (z ≈ 207–247)** becomes the kit's **nose**. It keeps the blade's own width and lean (no swell where avoidable), the arm grows out of its front, and the seams are hairlines. | Owner. Rejected options are in 2.18. | Owner decision | AGREED |
| 2.5 | **Kit off:** a small matching **blank cap** closes the notch. The owner still has to confirm cap vs no cap (4.6). | Owner. | Owner decision | AGREED (cap) / OPEN (confirm) |
| 2.6 | **Heights:** 2 arm lengths (pad drop **22 / 44 mm**) × LOW/HIGH flip × 2 pad-screw positions (**±11 mm**) give **6 pad heights 22 mm apart, z 172–282**. They cover armpit z 237–372 (2y P5 → 5y P95), with the pad top 25–50 mm below the armpit. Table in §3.4. | Owner. Check: 227 ∓ 22/44 ± 11 gives the listed heights; pad top = centre + 40; each setting serves armpit centre + 65…+ 90. | Owner decision; rule from `viewer/review_r5/heights/heights.py` (seat z 20; axilla 2y P5 240 … 5y P95 370) | AGREED |
| 2.7 | **Nothing may cover, cut or move the wing's existing slots and holes:** slanted strap slit S1, slit S2, teardrop channel, hole column, hole at z 186. **v1.1 bug:** the tip block covers S1's inner mouth. This must be fixed. S1 runs z 203–272, which overlaps the nose, so keep a 6 mm web to S1. The tongue is then only ~10 deep at its inboard corner and ~31 at its outboard corner. | Owner rule; designer/mechanical measurements. | `review_r4/designer/sections_features.png`, `sections_plan.png`; `review_r5/hookdrop/geometry_ABC.json` ("common") | AGREED |
| 2.8 | **Easy assembly:** it cannot go together wrong and small parts are captive. **Plastic carries the loads; screws only lock.** | Owner principle. | REVIEW.md (v1.0 lessons) | AGREED |
| 2.9 | **Latch upgrades, whichever latch is chosen:** ~6° tapered seating faces for zero rattle; a defined audible/tactile click; flush-when-locked status (2.2); an optional bought steel dowel as the bolt or tooth (as a coupon option). | Owner, from the lock research standard (US 5,678,798 taper: "removes play, so no rattle"; click on close). | `~/research/onshape/gobabygo/addons/laterals/lock_research.md` §1.5, §1.7, §4 | AGREED |
| 2.10 | **One Ø6 carbon rod in the arm** replaces Ø4 + Ø3. Bore Ø6.5 teardrop, peak up. | Owner approved. Strength: arm SF at u 13 (300 N knock) is 1.20 as built and 1.50 with Ø6. | `review_r4/strength/out_alt.txt` | AGREED |
| 2.11 | **Keep the 4 × Ø2 carbon tenon pins.** | Strength: about 2× the tenon-root capacity (e.g. "edge 300 N" at xe−3: 10.95 solid → 24.37 pinned, in that file's units). | `review_r4/strength/out_tenon.txt` | AGREED |
| 2.12 | **All carbon lengths and diameters go into the printed rod cutting gauge v2**, one lane per diameter (v1 is Ø8 only: 85–185 mm). | Owner. | `models/seating/headrest/generator/rod_cutting_gauge.py` | AGREED |
| 2.13 | **High-Tg epoxy** (Tg > 80 °C) for all bonded carbon. **Carbon is the default everywhere**; PVC only for skeleton parts where bending is wanted (the back). | Strength (car heat); owner. | REVIEW.md round 3 | AGREED |
| 2.14 | **History:** the v1.1 README says Ø4 × 62 and Ø3 × 59. The FS bores are 58.0 / 59.5, so the correct cuts were **56.5 / 58.0**. This is superseded by 2.10. | Carbon review. | `gbg_sidepad_v1.fs` line 47 | AGREED |
| 2.15 | **Ø6 audit:** all Ø6 bores already use the Ø6.5 teardrop slip fit; only the pommel v0.7b sketch and README said 6.2. Commit `bef5f12`, branch `rod6-clearance-audit`, docs only. **Owner to-dos:** re-upload the pommel v0.7.3 FS; put the chair-mount FS on a branch; re-export the fit-test and S23A STLs. | Ø6 audit. | `git show bef5f12` (worktree `/home/mikey/go-baby-go-core-rod6`) | AGREED |
| 2.16 | **Clip ring:** today it is only a press fit in the back-face groove. Add **positive retention** (e.g. a snap lip or undercut) so it can't work loose. It stays flush and sew-free. **Review the cover wrap too:** neoprene 2 mm, groove 3.2–8.8 mm in from the edge, 2.5 deep. | Owner. | `gbg_sidepad_v1.fs` line 67 (`GROOVE`, `GROOVE_D`) | AGREED |
| 2.17 | **Production parts in Round 5:** fine-tune every part of the arm assembly, give each one a **part number** (§8), and engrave it on a hidden face where printable. Use the same numbers in the BOM, print guide, file names and review page. | Owner. | Owner decision | AGREED |
| 2.18 | **Process:** Onshape `sidepads-v1` and git `laterals-v1` only. No merge to Main/main without the owner's OK. The Onshape API allocation is personal, so avoid the MCP. Verify in the browser. Reviewers each round: designer, mechanical, strength, ergonomics. | Owner. | Owner decision | AGREED |

### 2.2 Considered and rejected (for the record)

| # | Option | Why rejected | Source | Status |
|---|---|---|---|---|
| 2.19 | 40 mm nose + 4 identical 19.15 mm fillers + flip (7 heights ~19 mm apart) | Superseded by the single mount: too bulky, looks like a socket | `review_r5/heights/heights_out.txt` | AGREED (rejected) |
| 2.20 | "One nose per height" (3 carriers) and "1 carrier + 2 fillers" (Round 4 page) | Same reasons | Review page "Decision for you: heights" | AGREED (rejected) |
| 2.21 | v1.1's 3 tongue positions × flip (6 heights, only 4 useful, with gaps) | Gaps at armpit z 240–244, 270–276.5, 302.5–309, 335–339, 365–370 | `heights_out.txt` ("today v1.1") | AGREED (rejected) |
| 2.22 | Tilt at the arm↔depth joint; separate tilt wedges | The pad moves up to 15.6 mm (D3, 15°); the wedge puts the screw 15° off the normal and adds parts | `review_r5/tilt/tilt_out.txt` §1, §3 | RECOMMENDED (rejected) |

### 2.3 Recommended (awaiting owner OK)

| # | Proposal | Who / why | Source | Status |
|---|---|---|---|---|
| 2.23 | **Tilt lives in the depth piece's foot:** one print per angle, not flippable. Left top-in θ = right top-out θ, so the angles are A/B mirror pairs. Labels go on hidden faces. Angle set 0 / ±8 / ±15 → 6 depth pieces per depth length (2 × P0 + A8 + B8 + A15 + B15). The contact point moves up to ±8.9 mm at 15°, so re-check the armpit gap. The A/B feet are in the production scope (2.17); the angle set itself is awaiting OK. | Tilt review. | `tilt_out.txt` §1–3, §7; `tilt.json`; `tilt_section.svg` | RECOMMENDED |
| 2.24 | **Screw-load fixes.** (a) Pad screw: 812 N tension for 300 N outward at the front edge. Fix it by turning the foot rib into a front-to-back dovetail with a closed front, so plastic is the stop. (b) Tenon M5×12: the only thing holding a 100 N inward pull (hole play 0.25 < seam 0.3). Fix it with a hidden step at the tenon tip (hollow 13.4 × 20.4 → 11 × 18; 4.0 MPa at 300 N). | Mechanical audit. | `tilt_out.txt` §4–5 | RECOMMENDED |
| 2.25 | **Spring** (exact spec follows the latch choice). Round 4: OD 6, wire 0.55, free 16, 9 coils (~4 N rest / 8 N pressed). Hookdrop calc: SS304 d 0.55, OD 6, 6 + 2 coils, free 14 → 0.81 N/mm, 3.3 N installed, 6.5 N pressed, solid 4.4. Either way, the latch holds by its shape, not by the spring. | Mechanical. | Review page; `review_r5/hookdrop/loads_out.txt` | RECOMMENDED |
| 2.26 | **Hidden transport lock:** probably skip. | Mechanical. | `lock_research.md` §1.7 | RECOMMENDED |

---

## 3 Design spec (per part)

Coordinates are chair world mm (z up, seat surface z 20). d/t are the wing-tip axes in the FS (`P0`, `AXH`, `TH`; d + outboard, t − front). Part numbers are from §8.

### 3.1 Wing reprint (WG)

| Item | Spec | Status |
|---|---|---|
| Tip block, rails, slot blanks | Removed | AGREED |
| Notch | Only z ≈ 207–247 (40 mm) at the front edge; the rest of the edge stays original | AGREED |
| Section | The blade's own width and lean, with no swell where avoidable. Strength guardrail: walls ≥ 6 mm and back ≥ 8 mm around the tongue pocket; where the blade can't give that, use a faired local swell (never a box). | AGREED (rule) / OPEN (where needed) |
| Tongue pocket | 6 mm web to S1 (S1 at z 203–272); tongue engagement as deep as S1 allows (~10 inboard / ~31 outboard) | AGREED (rule) / OPEN (final numbers) |
| Existing slits/holes | Untouched. Re-measure every clearance in CAD (only "≥ 3 mm short of S1" and the hookdrop geometry are saved to files). | AGREED |
| Latch features | Latch A: slider pocket in the nose/tongue, tooth groove in the wing's outer wall at the mount, 6° locking face, click entry, hard stop at 5 mm | AGREED (sizes in CAD) |
| Print orientation with the notch | To check (bridged face) | OPEN |

### 3.2 Carrier: nose + arm + tenon (AR)

| Item | Spec | Status |
|---|---|---|
| Nose | 40 tall (z 207–247 at the mount), the blade's width and lean, flush, hairline seams (0.3) | AGREED |
| Arm | 18 × 28 pill, one smooth loft out of the nose's front | AGREED |
| Arm lengths | Two: pad drop **22** (short, S) and **44** (long, L) below/above the mount | AGREED |
| Flip | Rotate 180° about the front–back axis for LOW/HIGH. This makes the carrier a mirror pair A/B per arm length (A = left LOW / right HIGH, B = its mirror), so check it in CAD. | AGREED (flip) / OPEN (A/B check) |
| Old pill pocket and push-rod channel | Removed (the weakest zone, SF ≈ 1.4) | AGREED |
| Carbon | One Ø6 in a Ø6.5 teardrop, from the tongue rear face toward t ≈ −29 (hookdrop geometry); final length into gauge v2 | AGREED (length OPEN) |
| Tenon | 13 × 20 × 13, 4 × Ø2 × 21 carbon pins | AGREED |
| Corner knock (300 N at the pad front-top corner) | Arm hand calc below SF 1 (0.76–0.98 as built, 0.96–1.05 with Ø6, `out_alt.txt` "edge300"). Design for it. | OPEN |

### 3.3 Button and latch (BT, LK, SP, DW)

| Item | Spec | Status |
|---|---|---|
| Button | 12 × 24 stadium, outer face of the nose at arm height, square push, shallow thumb scoop, chair colour, orange ring shows only when unlatched, flush = latched | AGREED |
| Latch mechanism | A: button is the catch (§1.4) | AGREED |
| Upgrades | ~6° taper, click, flush indicator, optional steel dowel coupon. Note: the taper **reverses with the flip**, so it must work both ways up. It back-drives at μ 0.1 (PTFE-lubed) and self-holds at μ ≥ 0.2. | AGREED / OPEN (detail) |
| Spring | See 2.25 | RECOMMENDED |
| Transport lock | Skip | RECOMMENDED |
| Thumb room vs the car's side wall (x > 160) | Check | OPEN |

### 3.4 Heights (no extra parts)

| Setting | Arm | Flip | Pad screw | Pad centre z | Pad top z | Serves armpit z | Child (sitting axilla z: 2y P5 240, 2y P50 268, 5y P50 330, 5y P95 370) | Status |
|---|---|---|---|---|---|---|---|---|
| H1 | Long | LOW | lower | 172 | 212 | 237–262 | 2y P5 | AGREED |
| H2 | Short / Long | LOW | lower / upper | 194 | 234 | 259–284 | 2y P50 | AGREED |
| H3 | Short | LOW | upper | 216 | 256 | 281–306 | ~3 y | AGREED |
| H4 | Short | HIGH | lower | 238 | 278 | 303–328 | ~4 y | AGREED |
| H5 | Short / Long | HIGH | upper / lower | 260 | 300 | 325–350 | 5y P50 | AGREED |
| H6 | Long | HIGH | upper | 282 | 322 | 347–372 | 5y P95 | AGREED |

Mount (tongue centre) z 227. The pad is 80 tall. Settings overlap by 3 mm, so there are no gaps from 237 to 372.

### 3.5 Depth pieces (DP)

| Item | Spec | Status |
|---|---|---|
| Depths | D0–D3: pad back x 104 / 92 / 80 / 68 | AGREED |
| Tilt feet | 0 + A/B pairs at 8° and 15°; engraved labels | RECOMMENDED (angles) |
| D0 at 15° | Tenon tip to tilted floor margin is only 2.4 mm (3.7 at 8°); keep ≥ 2 | RECOMMENDED |
| Tenon-tip step | Hollow 13.4 × 20.4 → 11 × 18 | RECOMMENDED |
| Joint screw | M5×12 countersunk + nut, lock only | AGREED |

### 3.6 Pad shell, clip ring, cover (PD, RG, CV)

| Item | Spec | Status |
|---|---|---|
| Pad shell | Plain curved plate, 80 tall, shell 4.5 | AGREED |
| Two pad-screw positions | ±11 mm about the pad centre (two bosses/nut pockets, both hidden under the foot) | AGREED |
| Foot joint | Rib → front-to-back dovetail with a closed front, valid at both screw positions | RECOMMENDED |
| Pad screw | M5×10 button head + nut, lock only | AGREED |
| Clip ring | Positive retention (snap lip/undercut), flush, sew-free | AGREED (form OPEN) |
| Cover wrap | Neoprene 2 mm; groove 3.2–8.8 in from the edge, 2.5 deep: review the fit and wrap | AGREED (review) |
| Clip ring print method | Flat and bend in, or supported print | OPEN |

### 3.7 Blank cap (CP)

| Item | Spec | Status |
|---|---|---|
| Blank cap | Matches the nose (40 tall, flush, hairlines); closes the notch when the kit is off; captive or stowable | AGREED (pending 4.6) |

### 3.8 Tool: rod cutting gauge v2 (GA)

| Item | Spec | Status |
|---|---|---|
| Lanes | Ø8 (headrest H0–H100), Ø6 (side-pad arm, plus any chair Ø6 rods), Ø2 (tenon pins 21 mm) | AGREED |
| Accuracy | ±0.3 mm | RECOMMENDED |

---

## 4 Open decisions (to finalise together)

### 4.1 Latch behind the button: DECIDED (A, button is the catch, with upgrades; see §1.4). The comparison below is kept for the record.

| # | Candidate | How it works | What carries pull-out | Findings (`review_r5/hookdrop/loads_out.txt`) | Status |
|---|---|---|---|---|---|
| 4.1 | **A: button is the catch** | One sliding part: press straight in (−d), squeeze, pull forward | The pressed part's tooth | The tooth must grow: with the pitch moment counted it sees 688 N (knock) to 901 N (corner knock), root bending SF **0.8** (0.6 at the corner, 0.8 for a 60 °C lean) | OPEN |
| 4.2 | **B: hook-and-drop** | Push back, drop ~8 mm; hooks fall behind lips in the wing; a small button only blocks the lift. Remove: press + lift, then pull forward (two deliberate motions). | Plastic hook faces (two rows, 221–289 N each); the button sees almost nothing (lift SF 18) | Bearing SF 1.7–2.2, root bending ≥ 1.6. Its main conflict was the fillers, which the single mount removes. | OPEN |
| 4.3 | **C: steel spring pin** (lock research Rank 1) | Spring-loaded steel pin through the wing wall into the tongue; pull-knob | Steel pin (shear 24–32 MPa, allowable ~200) in PETG bearing | Bearing SF 1.3 at the corner knock (1.7 at a knock) | OPEN |

### 4.2 Other open items

| # | Item | What decides it | Status |
|---|---|---|---|
| 4.4 | Thumb room vs the car's side wall (x > 160) | CAD/car measure | OPEN |
| 4.5 | Lift-out (current) vs swing-away (what the lock research was written for) | Owner | OPEN |
| 4.6 | Blank cap vs no cap when the kit is off | Owner | OPEN |
| 4.7 | 6° taper detail: it must work in both flip orientations; dry vs PTFE (back-drives at μ 0.1) | Coupon | OPEN |
| 4.8 | Wing section: where a local swell is unavoidable (walls ≥ 6 / back ≥ 8) | Strength + designer in CAD | OPEN |
| 4.9 | Wing print orientation with the notch | Slicer check | OPEN |
| 4.10 | Carrier A/B mirror pair under flip, and whether the button/latch survives the flip | CAD | OPEN |
| 4.11 | Corner-knock case (arm SF < 1 in the hand calc) | Strength redesign + load test | OPEN |
| 4.12 | Inner wing support plate `chair_support_left_wing` (z 268–304). v1.0 review item 9: a print support, removed by the wing feature. Owner to confirm it is gone. Probably moot now that the mount is z 207–247; check the pad at HIGH. | CAD + owner | OPEN |
| 4.13 | Clip ring retention form (snap lip vs undercut) and print method | Designer + coupon | OPEN |
| 4.14 | Ø6 rod final length/position | CAD after 4.1 | OPEN |
| 4.15 | Keep or drop the old pill-button content (options 0–5) on the review page | Owner | OPEN |
| 4.16 | Accept the Recommended items 2.23–2.26 and the numbering scheme (§8) | Owner | OPEN |

---

## 5 Checks & tests

| # | Check / test | Pass criterion | When | Status |
|---|---|---|---|---|
| 5.1 | Clash check over every setting: 2 arms × LOW/HIGH × 2 screw positions × 4 depths × 5 angles = 160 per side, kit vs kit and kit vs the real chair, including S1/S2/teardrop/hole column/z 186, blank cap fitted | 0 interference; kit max x ≤ 165 (arm rest) | Each CAD rebuild | RECOMMENDED |
| 5.2 | Armpit gap per setting, including tilt (±8.9 mm at 15°) | Pad top 25–50 mm below the armpit | With 5.1 | AGREED |
| 5.3 | Load test: 600 N, or 300 N at 60 °C | No crack or creep (tenon root, latch, wing notch) | Before use | AGREED |
| 5.4 | 300 N on the pad's front-top corner; 100 N pull at the front edge | No failure; screws not loaded | Before use | RECOMMENDED |
| 5.5 | Latch coupon: press 8–12 N, insertion ≤ 25 N, pull-out ≥ 600 N, 200 cycles, wobble ≤ 0.5 mm at 60 mm, dry vs PTFE, both flip orientations | All met | Before production parts | RECOMMENDED |
| 5.6 | Latch upgrades on the coupon: zero rattle, audible click, orange ring hidden only when latched, steel dowel vs printed | Clear pass | With 5.5 | AGREED |
| 5.7 | Spring rate on a kitchen scale | Matches 2.25 | Before the coupon | RECOMMENDED |
| 5.8 | Clip ring retention: cover can't be pulled loose by hand; ring stays flush | Pass | Pad coupon | AGREED |
| 5.9 | Rod gauge v2 accuracy | ±0.3 mm | First gauge print | RECOMMENDED |
| 5.10 | Part numbers: engraved and readable on every printed part; match the BOM, file names and review page | 100 % match | Before release | AGREED |
| 5.11 | Real-child fit (PT/OT) | Pad top ≥ 25 mm below the armpit; comfortable; thumb room in the car | After the first set | AGREED |
| 5.12 | Visual quality gate: single smooth lofts, splines, no silent fallbacks, compare with the approved render, wing in view | Owner's look rules | Every round | AGREED |
| 5.13 | Reviewers: designer, mechanical, strength, ergonomics | Findings closed or listed | Every round | AGREED |

---

## 6 Work plan (ordered steps)

| Step | What | Type | Depends on | Status |
|---|---|---|---|---|
| 6.1 | Latch chosen: A (button is the catch) + upgrades; size the tooth for the corner knock in CAD and check the 6° face with the LOW/HIGH flip | CAD | – | AGREED |
| 6.2 | Owner decides the latch, lift-out vs swing-away, blank cap, the Recommended items and the numbering scheme | Owner | 6.1 | OPEN |
| 6.3 | Latch coupon (chosen latch + upgrades, dowel/no-dowel, both flips) → print → tests 5.5–5.7 | CAD + test | 6.2 | AGREED (coupon first) |
| 6.4 | Pad coupon: clip-ring retention + cover wrap → test 5.8 | CAD + test | – | AGREED |
| 6.5 | Wing feature rewrite (WG): remove the block, rails and blanks; 40 mm notch; blade-section pocket; free S1; latch features | CAD (FS, `sidepads-v1`) | 6.3 | AGREED |
| 6.6 | Carrier (AR): nose, S and L arms, A/B, Ø6 bore, button and latch | CAD | 6.5 | AGREED |
| 6.7 | Blank cap (CP) | CAD | 6.5, 4.6 | AGREED |
| 6.8 | Depth pieces (DP) D0–D3 with tilt feet, labels, tenon-tip step; pad shell (PD) with 2 screw positions + dovetail; clip ring (RG) | CAD | 6.2, 6.4 | RECOMMENDED (tilt, dovetail) / AGREED (rest) |
| 6.9 | Engrave part numbers on hidden faces; name the STL/3MF files by part number | CAD + export | 6.5–6.8 | AGREED |
| 6.10 | Extend "check all settings" to §5.1; run it and the quality gate; verify in the browser | CAD + check | 6.9 | RECOMMENDED |
| 6.11 | Reviewer round | Review | 6.10 | AGREED |
| 6.12 | Rod cutting gauge v2 | Tools (python generator) | 6.6 lengths | AGREED |
| 6.13 | README/REVIEW/BOM/print guide/review page updated with part numbers | Docs | 6.11 | AGREED |
| 6.14 | Owner Ø6 to-dos (2.15) | Owner | – | AGREED |
| 6.15 | Print the production set → load tests 5.3–5.4 → child fit 5.11 | Test | 6.11 | AGREED |
| 6.16 | Merge to Main/main only with the owner's OK | Owner | 6.15 | AGREED |

---

## 7 Bill of materials changes

| # | Item | v1.1 | Round 5 | Status |
|---|---|---|---|---|
| 7.1 | Carbon, arm | Ø4 + Ø3 per side (cut 56.5 / 58.0; README wrongly said 62 / 59) | **One Ø6 per carrier** (same listing as the 6/8 mm rods); length → gauge v2 | AGREED (length OPEN) |
| 7.2 | Carbon, tenon pins | Ø2, 4 × 21 per carrier | Unchanged | AGREED |
| 7.3 | Epoxy | High-Tg | Required for all bonds | AGREED |
| 7.4 | Latch spring | OD ≤ 6, free 11–12, solid ≤ 5.2 | OD 6, wire 0.55; free 14–16 per the chosen latch (2.25) | RECOMMENDED |
| 7.5 | Latch parts | Pill + push-rod + shuttle + spring | Button-catch slider + spring (OD 6, wire 0.55, L0 16, 9 coils) + orange ring | AGREED |
| 7.6 | Steel dowel | – | Optional, size set on the coupon | AGREED (coupon option) |
| 7.7 | Steel spring pin kit | – | Only for candidate C (`lock_research.md` §2.2) | OPEN |
| 7.8 | Tenon screw | M5×12 countersunk + M5 nut | Unchanged, lock only | AGREED |
| 7.9 | Pad screw | M5×10 button head + M5 nut | Unchanged, lock only; nut pockets at both ±11 positions | AGREED |
| 7.10 | Printed parts | Carrier, 2 slot blanks, depth piece, pad, clip ring | See §8: wing, 2 carriers (S, L), blank cap, button/latch, depth pieces, pad, clip ring. No blanks, no fillers. | AGREED |
| 7.11 | Lube | – | Dry PTFE, if the coupon allows (taper back-drives at μ 0.1) | OPEN |
| 7.12 | Tool | Gauge v1 (Ø8) | Gauge v2 (Ø8 / Ø6 / Ø2) | AGREED |

All hardware comes from home stock; reuse the chair's sizes and check the cart before listing new buys.

---

## 8 Part list & numbering

**Proposed scheme** (status: RECOMMENDED; awaiting owner OK, 4.16): `GBG-SP-<part>-<variant>-r<rev>`

| Field | Values |
|---|---|
| `GBG-SP` | GoBabyGo side pads |
| `<part>` | 2-letter code (table below) |
| `<variant>` | Fields joined by `-`, in this order: side `L`/`R`, or mirror `A`/`B` for flip-handed parts; arm length `S` (22) / `L` (44); depth `D0`–`D3`; angle `T0`, `T8`, `T15`. Omitted when not used. |
| `r<rev>` | Design revision, `r5` for this round |
| Engraving | Short form without `GBG-SP-`, e.g. `AR-S-A r5`, 0.4 deep on a hidden face. For depth pieces add the use, e.g. `DP-D2-T15A · L+15 / R−15`. |
| Files | `GBG-SP-<part>-<variant>-r5.stl`; the same number in the BOM, print guide and review page |

| Part no. | Part | Variants | Qty per side (fitted) | Qty in a full kit (both sides) | Material | Print orientation | Status |
|---|---|---|---|---|---|---|---|
| GBG-SP-WG-L/R-r5 | Wing reprint with the nose notch | L, R | 1 | 2 | PETG | To check with the notch (4.9) | AGREED (part) / OPEN (orientation) |
| GBG-SP-AR-S-A/B-r5 | Carrier, short arm (drop 22): nose + arm + tenon | A, B (flip mirror pair) | 1 of S/L | 2 (A + B) | PETG + Ø6 carbon + 4 × Ø2 pins | On its outer face, tree support under the arm bend (v1.1 study); recheck | AGREED (part) / OPEN (A/B, orientation) |
| GBG-SP-AR-L-A/B-r5 | Carrier, long arm (drop 44) | A, B | 1 of S/L | 2 | as above | as above | AGREED |
| GBG-SP-CP-L/R-r5 | Blank cap for the notch | L, R | 1 when the kit is off | 2 | PETG | Outer face down | AGREED (pending 4.6) |
| GBG-SP-BT-r5 | Button-catch (latch A, one sliding part): 12 × 24 stadium face, bar, tooth with 6° locking face; travel 5 to a hard stop | One part for A and B (confirm with the flip, 4.7/4.10) | 1 per carrier | 4 | PETG chair colour (± steel dowel tooth) | Standing, 24 along z (fig2); recheck with the tooth | AGREED (form, latch A) / OPEN (tooth size for ~900 N) |
| GBG-SP-TS-r5 | Orange tell sleeve (0.6 wall ring round the button; shows only when not latched) | – | 1 per carrier | 4 | PETG orange | Standing, separate print | AGREED (function) / RECOMMENDED (separate sleeve, new code TS) |
| ~~GBG-SP-LK-…-r5~~ | Merged into BT: with latch A the button is the catch | – | – | – | – | – | RECOMMENDED (merge) |
| GBG-SP-DP-D0…D3-T0-r5 | Depth piece, no tilt | D0–D3 | 1 | 2 per depth used | PETG | Joint end down (v1.1); recheck | AGREED |
| GBG-SP-DP-Dn-T8A/T8B-r5 | Depth piece, 8° foot (A = L top-in / R top-out; B = mirror) | D0–D3 × A/B | 1 | 1 A + 1 B per depth | PETG | as above | RECOMMENDED |
| GBG-SP-DP-Dn-T15A/T15B-r5 | Depth piece, 15° foot | D0–D3 × A/B | 1 | 1 A + 1 B per depth | PETG | as above | RECOMMENDED |
| GBG-SP-PD-L/R-r5 | Pad shell (2 screw positions, dovetail, cover groove) | L, R (or one if symmetric) | 1 | 2 | PETG | Child side down, tree supports on the hidden side | AGREED |
| GBG-SP-RG-L/R-r5 | Clip ring with retention | L, R (or one) | 1 | 2 | PETG | Flat or supported (4.13) | AGREED (form OPEN) |
| GBG-SP-CV-r5 | Cover (no-sew) | – | 1 | 2 | Neoprene 2 mm | Cut pattern | AGREED |
| GBG-SP-FM-r5 | Foam 30 mm, pad outline 82 × 80 (FS reference). README's 17 / 30 / 43 per depth (would be FM-D0…D3) to confirm | – | 1 | 2 | Foam | Cut | OPEN (one thickness vs per depth) |
| GBG-SP-CF6-r5 | Carbon rod Ø6, arm | length per gauge v2 | 1 per carrier | 4 | Carbon pultruded + high-Tg epoxy | Gauge v2 Ø6 lane | AGREED |
| GBG-SP-CF2-r5 | Carbon pin Ø2 × 21 | – | 4 per carrier | 16 | as above | Gauge v2 Ø2 lane | AGREED |
| GBG-SP-SP-r5 | Latch spring OD 6, wire 0.55, L0 16, 9 coils (7.5) | – | 1 per carrier | 4 | SS304 wire | Bought | AGREED (rate check 5.7) |
| GBG-SP-DW-r5 | Steel dowel (optional) | size from the coupon | 1 per carrier | 4 | Steel | Bought | AGREED (coupon option) |
| GBG-SP-HW-M5x12C | Tenon screw M5×12 countersunk + M5 nut | – | 1 + 1 | 2 + 2 | Steel, stock | – | AGREED |
| GBG-SP-HW-M5x10B | Pad screw M5×10 button head (ISO 7380) + M5 nut | – | 1 + 1 | 2 + 2 | Steel, stock | – | AGREED |
| GBG-SP-EP-r5 | High-Tg epoxy (Tg > 80 °C) for CF6 + CF2 | – | as needed | 1 pack | Epoxy | – | AGREED (material) / RECOMMENDED (code) |
| GBG-SP-GA-r5 (tool) | Rod cutting gauge v2 | – | – | 1 | PETG + 2 × M3×30 + nuts | Flat, no supports | AGREED |

Sketches of every part, an exploded assembly with these numbers as balloons, the part-list table and an example child's set (2y P50, H2: short carriers A left + B right, LOW): [viewer/site/parts5.js](viewer/site/parts5.js) + [parts5.json](viewer/site/parts5.json) (test page `viewer/site/parts5_test.html`). Keep the JSON and this table in step.

Notes: a full tilt set is 6 depth pieces per depth length (2 × T0 + T8A + T8B + T15A + T15B). Each side needs both a short and a long carrier to reach all 6 heights; one is fitted at a time.

---

## 9 Sources

| What | Path / link |
|---|---|
| Current model | [featurescript/gbg_sidepad_v1.fs](featurescript/gbg_sidepad_v1.fs), [README.md](README.md), [REVIEW.md](REVIEW.md) |
| Live review page (Round 4 draft, latch cutaway, button options) | [viewer/site/index.html](viewer/site/index.html) (`draft4.js`, `cutaway.js`, `mock.js`), published at https://claude.ai/artifact/KMEgh4mT1TpXPVEQzjZHZK |
| Designer (Round 4) | [viewer/review_r4/designer/](viewer/review_r4/designer/): `c1_keystone_concept.png`, `sections_features.png`, `sections_plan.png`, `work/` |
| Mechanical (Round 4) | [viewer/review_r4/mech/](viewer/review_r4/mech/): `press.py`, `slices.png`, `wing_sides.png` |
| Strength (Round 4) | [viewer/review_r4/strength/](viewer/review_r4/strength/): `out_alt.txt`, `out_tenon.txt`, `out_pocket.txt`, `out_wing.txt` |
| Heights (Round 5, multi-position study, now rejected) | [viewer/review_r5/heights/](viewer/review_r5/heights/) |
| Tilt + screw audit (Round 5) | [viewer/review_r5/tilt/](viewer/review_r5/tilt/): `tilt_out.txt`, `tilt.json`, `tilt_section.svg` |
| Latch A/B/C loads and geometry (Round 5, in progress) | [viewer/review_r5/hookdrop/](viewer/review_r5/hookdrop/): `loads_out.txt`, `geometry_ABC.json`, `hookdrop_B_sketch.png`, `work_wing_slices.png` |
| Button design (Round 5, in progress) | [viewer/review_r5/button_design/](viewer/review_r5/button_design/): `fig1_placements.png`, `fig2_favourite_spec.png`, `fig3_ABC_forms.png`, `fig4_block_above_arm.png` |
| Lock research standard | `~/research/onshape/gobabygo/addons/laterals/lock_research.md` |
| Ø6 audit | branch `rod6-clearance-audit`, worktree `/home/mikey/go-baby-go-core-rod6`, commit `bef5f12` |
| Rod cutting gauge | `models/seating/headrest/generator/rod_cutting_gauge.py` |

### Notes where files disagree or are stale
- **README carbon lengths** (62 / 59) disagree with the FS bores (cuts 56.5 / 58.0). See 2.14.
- **The review page** still shows the Round 4 draft (multi-position band, 12 × 18 button, "one nose per height" choice). All of it is superseded by 2.2, 2.4 and 2.6.
- **The hookdrop calculations** were made for the multi-position band (`geometry_ABC.json`: fillers, 7 cells). Re-run them for the single mount (6.1).
- **Wing clearance numbers** (S2, teardrop, hole column, z 186) are not stored in files. Re-measure them in CAD.
- **Two spring specs** exist (Round 4 page: free 16, 9 coils; hookdrop: free 14, 6 + 2 coils). Choose one with the latch.
