// Side-by-side cutaway, seen from above at the button's height (wing-tip frame: d across the tip, + outboard;
// t into the wing). Left: today's v1.1 latch, cut from the real Onshape meshes. Right: the round-4 draft latch (schematic).
const NS = 'http://www.w3.org/2000/svg';
const X = (d) => d, Y = (t) => -t;                      // plan: outboard to the right, the wing at the top, the chair front at the bottom
const el = (tag, a, parent) => { const e = document.createElementNS(NS, tag); for (const k in a) e.setAttribute(k, a[k]); parent && parent.appendChild(e); return e; };
const segs = (g, list, cls) => { let d = ''; for (const [[a, b], [c, e]] of list) d += `M${X(a)} ${Y(b)}L${X(c)} ${Y(e)}`; el('path', { d, class: cls }, g); };
const poly = (g, pts, cls) => el('polygon', { points: pts.map(([d, t]) => `${X(d)},${Y(t)}`).join(' '), class: cls }, g);
const rect = (g, d0, d1, t0, t1, cls, rx = 0) => el('rect', { x: X(d0), y: Y(t1), width: d1 - d0, height: t1 - t0, rx, class: cls }, g);
const text = (g, d, t, s, cls = 'lb', anchor = 'start') => { const e = el('text', { x: X(d), y: Y(t), class: cls, 'text-anchor': anchor }, g); e.textContent = s; return e; };
function spring(g, d0, d1, t, w = 3, n = 7) { let p = `M${X(d0)} ${Y(t)}`; for (let i = 1; i <= 2 * n; i++) { const d = d0 + (d1 - d0) * i / (2 * n + 1); p += `L${X(d)} ${Y(t + (i % 2 ? w : -w))}`; } p += `L${X(d1)} ${Y(t)}`; return el('path', { d: p, class: 'spr' }, g); }
function arrow(g, d, t, dd, dt, cls) { const L = Math.hypot(dd, dt), ux = dd / L, uy = dt / L; const e = el('g', {}, g);
  el('line', { x1: X(d - dd), y1: Y(t - dt), x2: X(d - ux * 2.5), y2: Y(t - uy * 2.5), class: cls }, e);
  poly(e, [[d, t], [d - ux * 4 - uy * 2, t - uy * 4 + ux * 2], [d - ux * 4 + uy * 2, t - uy * 4 - ux * 2]], cls + ' fillarrow'); return e; }
export function mountCutaway(root, S) {
  const mk = (title) => { const f = document.createElement('figure'); f.className = 'cfig'; root.appendChild(f); const h = document.createElement('figcaption'); h.innerHTML = title; f.appendChild(h);
    const s = el('svg', { viewBox: '-38 -62 80 112', role: 'img' }, f); return { f, s, cap: h }; };
  // ---------------- TODAY ----------------
  const A = mk('<b>Today (v1.1)</b>: press along the arm, an angled slot turns it 90°');
  segs(A.s, S.wing, 'wing'); segs(A.s, S.carrier, 'kit');
  const rod = el('g', {}, A.s); segs(rod, S.button, 'btn');
  const sh = el('g', {}, A.s); segs(sh, S.shuttle, 'cat');
  const spA = el('g', {}, A.s);
  arrow(A.s, 10.7 - 3.5, -33.6 - 3, -8, -10, 'thumb');
  text(A.s, -34, 44, 'wing tip block', 'lb'); text(A.s, 12, -44, 'pill button', 'lb'); text(A.s, -2, 18, 'push-rod', 'lb');
  text(A.s, 23, 4, 'catch', 'lb'); text(A.s, 24, -2, 'groove', 'lb sm'); text(A.s, -34, -40, 'arm', 'lb');
  const stA = text(A.s, 2, -47.5, '', 'st', 'middle');
  // ---------------- DRAFT ----------------
  const B = mk('<b>Draft</b>: press straight in; the button <i>is</i> the catch');
  const blade = S.blade.filter(([[a, b], [c, e]]) => Math.min(b, e) > 6);
  segs(B.s, blade, 'wing');
  el('path', { d: `M${X(-6)} ${Y(6)} L${X(-6)} ${Y(14)} Q${X(-6)} ${Y(30)} ${X(-3)} ${Y(38)} M${X(23)} ${Y(6)} L${X(23)} ${Y(16)} Q${X(23)} ${Y(30)} ${X(19)} ${Y(42)}`, class: 'swell' }, B.s);
  rect(B.s, 1.2, 17.6, 6, 26.5, 'slot'); rect(B.s, 17.6, 22, 8, 13, 'groove');
  rect(B.s, -6, 23, -14, 6, 'nose', 6);
  rect(B.s, 1.4, 17.4, 6, 26, 'tongue');
  el('path', { d: `M${X(0.5)} ${Y(-14)} L${X(0.5)} ${Y(-46)} M${X(18.5)} ${Y(-14)} L${X(18.5)} ${Y(-46)}`, class: 'armln' }, B.s);
  rect(B.s, 9.6, 23.4, -9, 4, 'pocket', 1.5); rect(B.s, 12.6, 17.6, 4, 13.6, 'pocket');
  const sl = el('g', {}, B.s);
  rect(sl, 10, 23.35, -8.5, 3.5, 'btnfill', 1.5); rect(sl, 13, 21.6, 3.5, 13, 'catfill');
  const spB = el('g', {}, B.s);
  el('line', { x1: X(2.5), y1: Y(-6), x2: X(2.5), y2: Y(1), class: 'seat' }, B.s);
  const thB = arrow(B.s, 25.5, -2.5, -12, 0, 'thumb'); const fgB = arrow(B.s, -8.5, -2.5, 12, 0, 'finger');
  text(B.s, -34, 44, 'wing (no block)', 'lb'); text(B.s, -34, 2, 'nose (kit)', 'lb'); text(B.s, 25, 9, 'groove', 'lb sm');
  text(B.s, 25, -10, 'button', 'lb'); text(B.s, 3, 22, 'tongue', 'lb sm'); text(B.s, -34, -40, 'arm', 'lb');
  text(B.s, -34, 34, 'wing thickened around', 'lb sm'); text(B.s, -34, 30, 'the slot (dashed)', 'lb sm');
  const stB = text(B.s, 2, -47.5, '', 'st', 'middle');
  // ---------------- press ----------------
  function set(p) {                      // p 0..1 of each button's full travel
    const pa = 3.9 * p, sa = Math.min(pa, 3.5) * 4.5 / 3.5;
    rod.setAttribute('transform', `translate(0 ${-pa})`);       // the rod slides along +t (up in the drawing)
    sh.setAttribute('transform', `translate(${-sa} 0)`);
    spA.innerHTML = ''; { let pth = ''; const t0 = 10.1 + pa, t1 = 19.5, n = 6; pth = `M${X(9.5)} ${Y(t0)}`; for (let i = 1; i <= 2 * n; i++) { const t = t0 + (t1 - t0) * i / (2 * n + 1); pth += `L${X(9.5 + (i % 2 ? 2.6 : -2.6))} ${Y(t)}`; } pth += `L${X(9.5)} ${Y(t1)}`; el('path', { d: pth, class: 'spr' }, spA); }
    const tipA = 21.6 - sa; stA.textContent = tipA < 17.6 ? 'catch out of the groove: released' : `latched (catch ${(tipA - 17.6).toFixed(1)} mm in)`;
    stA.setAttribute('class', tipA < 17.6 ? 'st ok' : 'st');
    const pb = 5 * p; sl.setAttribute('transform', `translate(${-pb} 0)`); thB.setAttribute('transform', `translate(${-pb} 0)`);
    spB.innerHTML = ''; spring(spB, 2.5, 10 - pb, -2.5, 2.6, 6);
    const tipB = 21.6 - pb; stB.textContent = tipB < 17.6 ? 'catch out of the groove: released' : `latched (catch ${(tipB - 17.6).toFixed(1)} mm in)`;
    stB.setAttribute('class', tipB < 17.6 ? 'st ok' : 'st');
  }
  set(0);
  return { set };
}
