# Side pads (lateral trunk supports) v1.0: pre-first-print

FeatureScript: `featurescript/gbg_sidepad_v1.fs` (two features)

- **GbG Side Pad Wing (chair):** the chair change. Each wing tip gets a vertical slot, comb ribs every 12.5 mm at the back of the slot, a column of 8 Ø6.4 bolt holes (one per height step) in a 12 mm recessed track on the outer face, and a 30° entry ramp.
- **GbG Side Pad v1:** the kit, built in place on the chair for a chosen setting:
  - carrier, the flip pair A / A' (LOW, or HIGH = flipped)
  - depth piece N0 / N12 / N24 / N36
  - pad shell
  - clip ring
  - tilt shim (8° or 15°, either way)
  - snap-button bolt and retainer washer

Design record (research, options, renders, decisions): `~/research/onshape/gobabygo/addons/laterals/options_v05/` (v0.5 to v1.0) and the published pages.

## How it works
- **Height:**
  - the tongue sits on the comb, in 12.5 mm steps;
  - the pad seat adds -6.25 / 0 / +6.25;
  - the carrier gives LOW or HIGH by flipping it.
- **Depth:**
  - depth piece N0, N12, N24 or N36, moving the pad 12 mm per step;
  - foam 17 / 30 / 43 mm.
- **Tilt:** an 8° or 15° shim between the foot and the pad seat. Flip it for the other direction.
- **Lock:** a Ø6 snap-button bolt in the tongue. One spring from home stock (OD ≤ 6, 10–16 mm long) pushes it through the wing hole for that height. Press the button flush and slide the arm out. Slide it back in: the ramp pushes the bolt down until it clicks into the hole.
- **Loads:** the comb ribs carry up/down; the slot walls carry the lean twist; the dovetail carries the joint between carrier and depth piece; the foot sits in the shell's seat with ribs in grooves. Screws only hold parts together.
- **Fasteners:** real screws and nuts only (DIN 7991 countersunk + DIN 934). No printed threads.

| Joint | Fastener |
|---|---|
| Carrier ↔ depth piece (dovetail retainer) | M5x30 + nut. The nut sits 6 mm up inside the part, so the screw tip ends inside it. |
| Foot ↔ pad shell | 2x M5x25 from **outside**, through slots in the foot, into nuts held in the shell under the EVA. Use M5x30 with the 15° shim. |
| Bolt retainer washer | 2x M3x10 countersunk + nuts |

## Status
- Sketch model reviewed: pages v0.5 to v1.0 (2026-11-18).
- FeatureScript functions tested in an Onshape scratch Part Studio: every part builds; the wing change was tested on a stand-in block.
- **Not yet done:**
  - install on an Onshape branch of GbG_chair_forth;
  - check against the real wings in the browser;
  - export the STLs;
  - print the test pieces (latch, dovetail, foot and seat);
  - a pull test on one wing.

Do not print production parts before the test pieces pass.
