# Side pads: round 5 design document (review page)

The review page used to agree the round 5 design with the owner. Keep it as the reference for what a good design document looks like here: the decision first, then real geometry, cutaways, ranges against requirements, a numbered parts list with sketches, and history kept but clearly marked superseded.

- **Published (private):** https://claude.ai/artifact/KMEgh4mT1TpXPVEQzjZHZK
- **Open locally:** `cd site && python3 -m http.server 4324`, then open http://127.0.0.1:4324/index.html. It needs a local server because the page fetches its data files, so opening it straight from disk won't work.
- **Plan of record:** [`../ROUND5_PLAN.md`](../ROUND5_PLAN.md)

## What's in it (`site/`)
| File | Section |
|---|---|
| `index.html` | the page: round 5 direction, ranges, parts, today's v1.1 internals viewer, round 4 history |
| `draft4.js` | 3D of the round 5 mount on the real chair (kit on / kit off + cap / kit off open; short / long arm) |
| `cutaway.js` + `sections.json` | latch cutaway, today vs round 5 |
| `ranges5.js` + `ranges5.json` | pad placement vs children aged 2–5 and conditions |
| `parts5.js` + `parts5.json` | numbered part list with sketches and exploded assembly |
| `mock.js` | round 4 button-option mockups (history) |
| `geo.json` + `geodata.json` | packed meshes (real Onshape geometry + round 5 wing-side solids) |

## Rebuilding the data
- `pack.py` packs `raw/` (Onshape tessellated faces, not in git, re-fetch from the `sidepads-v1` branch) and `r5mesh/*.stl` into `site/geo*.json`.
- `r5mesh/gen_r5.py` builds the round 5 wing-side solids (manifold3d).
- `review_r4/`, `review_r5/`: the reviewers' reports, scripts and figures behind each decision.
