// Round-5 latch cutaway, seen from above at z 230 (wing-tip frame: d across the tip, + outboard; t into the wing, front down).
// Real sections of the Onshape round-5 parts, from sections.json (options/gen_main.py).
const NS = 'http://www.w3.org/2000/svg';
const X = (d) => d, Y = (t) => -t;
const el = (tag, a, parent) => { const e = document.createElementNS(NS, tag); for (const k in a) e.setAttribute(k, a[k]); parent && parent.appendChild(e); return e; };
const segs = (g, list, cls) => { let d = ''; for (const [[a, b], [c, e]] of list) d += `M${X(a)} ${Y(b)}L${X(c)} ${Y(e)}`; return el('path', { d, class: cls }, g); };
const text = (g, d, t, s, cls = 'lb', anchor = 'start') => { const e = el('text', { x: X(d), y: Y(t), class: cls, 'text-anchor': anchor }, g); e.textContent = s; return e; };
function arrow(g, d, t, dd, dt, cls) { const L = Math.hypot(dd, dt), ux = dd / L, uy = dt / L; const e = el('g', {}, g);
  el('line', { x1: X(d - dd), y1: Y(t - dt), x2: X(d - ux * 2.5), y2: Y(t - uy * 2.5), class: cls }, e);
  el('polygon', { points: [[d, t], [d - ux * 4 - uy * 2, t - uy * 4 + ux * 2], [d - ux * 4 + uy * 2, t - uy * 4 - ux * 2]].map(([a, b2]) => `${X(a)},${Y(b2)}`).join(' '), class: cls + ' fillarrow' }, e); return e; }
export function mountCutaway(root, S) {
  const f = document.createElement('figure'); f.className = 'cfig'; root.appendChild(f);
  const h = document.createElement('figcaption'); h.innerHTML = '<b>Round 5</b>: press straight in; the button <i>is</i> the catch'; f.appendChild(h);
  const s = el('svg', { viewBox: '-26 -40 62 82', role: 'img' }, f);
  segs(s, S.wing, 'wing'); segs(s, S.carrier, 'kit');
  const mv = el('g', {}, s); segs(mv, S.button, 'btn'); segs(s, S.sleeve, 'btn');
  const th = arrow(s, 18.5, -6.3, -9, 0, 'thumb'), fg = arrow(s, -8.5, -6.3, 9, 0, 'finger');
  text(s, -24, 30, 'wing', 'lb'); text(s, -24, -2, 'nose (kit)', 'lb'); text(s, 20, 3, 'button', 'lb'); text(s, -24, -30, 'arm', 'lb');
  const st = text(s, 5, -37, '', 'st', 'middle');
  function set(p) {
    const pb = 5 * p; mv.setAttribute('transform', `translate(${-pb} 0)`); th.setAttribute('transform', `translate(${-pb} 0)`);
    st.textContent = pb > 3.6 ? 'tooth out of the groove: released' : `latched (pressed ${pb.toFixed(1)} of 5 mm)`;
    st.setAttribute('class', pb > 3.6 ? 'st ok' : 'st');
  }
  set(0);
  return { set };
}
