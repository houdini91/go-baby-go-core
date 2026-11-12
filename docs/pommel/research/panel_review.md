# Pommel v0.5: expert panel review (2026-11-11)
Three independent reviews, run as AI agents: a paediatric seating PT/OT, an anthropometrics researcher and a mechanical/FDM engineer. Every number below is an estimate that needs validating with a real PT and real children. Derivation: NIST AnthroKids 1977 + Carman 2023 (see background_research.md).

## Verdict on v0.5: functionally wrong place
- With the pelvis at the backrest (chair y≈+81), the knees are 77/103/128/149 mm (P50, ages 2–5) past the pan front (y −86). P5 2 y: 58 mm; P95 5 y: 177 mm.
- v0.5 (y −86…−20/−28) presses on the proximal/mid thigh. Its rear end is ~0–20 mm from the groin, which sits ~80–125 mm from the backrest, so a child who slides forward lands on it.
- Leg room is NOT the issue: 103–112 mm per leg at the pan vs thighs 80–93 mm wide.
- 40–58 mm wide on the proximal thigh = legs about straight ahead; anti-scissor only.

## Consensus spec (v0.6 direction)
| | 2–3 y align | 2–3 y abduct | 4–5 y align | 4–5 y abduct |
|---|---|---|---|---|
| Contact zone (past pan front) | ~30–110 | same | ~60–150 | same |
| Face length | 60–75 | | 60–75 | |
| Width rear of face (distal thigh) | 45–55 | 80–85 | 50–65 | 95–100 |
| Width front (knee) | 70 | 100–110 | 80 | 120–130 |
| Height above seat plane (top) | 60–75 | | 70–90 | |
| Fore-aft adjust | ±30 mm | | | |
Shape: a V-wedge, wider at the front. Each face is angled at the target abduction (5° or 10°/side) so the whole face contacts. Shallow concave faces, not saddles. Edges R10 or more. Optional split L/R pads for asymmetry. Abduction sizes only after a PT checks hip ROM and the hip X-ray (≈50% hip subluxation in spastic quadriplegia). Windswept: off-centre pommel plus a lateral knee block, set up by the PT.
Groin: nothing taller than about 10 mm on the pan closer than 145 (2–3 y) / 170 (4–5 y) mm from the backrest. In practice: no pommel body on the pan.
Padding: 10–15 mm medium-firm closed-cell foam with a wipeable cover (3–6 mm is too thin).
Removal: one-handed, tool-free, tethered.
Never: anti-slide, groin contact, a hard post at bump/stop, past the passive ROM, red marks lasting >20–30 min, finger traps.

## Disagreement: width numbers
PT: 70/80 align, 110/130 abduct. Researcher: 40–50 align, 65–115 abduct at the distal thigh; 65–70 / 100–150 at the condyles. Both are right for their contact point, so the wedge covers both (rear = distal-thigh number, front = knee number). The researcher's data is more grounded (Snyder 1977 + Carman 2023 CT). Some of the PT's inputs were estimates.

## Structure (engineer)
Loads: 200 N one-sided lateral at the knee; 300 N down / 150 N up at transfers; 57 Nm at the pan edge.
- A printed cantilever on 3× M3: fails at the joint (end-bolt shear ~1.4 kN, SF ~1.2; nut crush SF ~1; layer peel).
- 20 mm PVC: SF 0.33. Pan channel: 5 mm tall, unusable. Through-pan pipe would mean editing the chair, so no.
- Recommended: 20×2 mm steel tube (fits the kit's 20 mm PVC fittings, bore Ø20.4–20.5), ~340 mm, under the pan.
  - Front saddle on the existing 3× M3 (M3×50).
  - Rear saddle on the centre M6 at (0, 34) (UNVERIFIED: check the hole exists and whether the car mount uses it).
  - Split printed knee block on the tube.
  - Ball-lock pin A sets the block position and gives a 3 s slide-off; pin B releases the tube.
  - Tube SF 2.9; printed parts SF ≥ 4.5; tip deflection ~0.7 mm.
- Simpler option for the PT: a foam knee spacer strapped between the knees, tethered (squeeze is self-reacting).
Tests: ~26 mm clearance under the pan; 300 N down / 200 N lateral (<3 mm, no cracking); 24 h creep at 100 N; pin retention on a bumpy drive; capped tube end at shin height; PT fit on a child.

## Owner decision (2026-11-11): PVC only, no steel
v0.6 is built around that: TWO 20 mm PVC pipes (x = ±22) in a printed carrier under the pan, clamped by the car-mount M6 bolts.
- Sideways knee push: the two pipes plus the block form a frame, so the pipes see mostly push/pull.
- Downward push: the block's foot rests on the car seat surface (same level as the carrier bottom), so it doesn't bend the PVC.
- Upward 150 N at ~120 mm: ~9 Nm per pipe, about 24 MPa vs ~50 MPa for PVC (SF ~2). Hand estimate, untested.
Open: the car seat surface in front of the chair must really be at the carrier-bottom level (otherwise a PVC leg is needed). The chair rises 30 mm, and the M6 car bolts must be 30 mm longer.
