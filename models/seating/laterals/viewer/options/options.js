// Mount review: (A) the current front-edge tongue, (B) three-prong plug, (C) child-side socket, on the real chair (left side).
// Geometry: options/geo.json + geodata.json (pack_options.py) from the Onshape build (wing repaired) + gen_opts2.py / gen_opt6.py.
// Each option: 3D viewer with assembled / exploded / kit-off states, a connect-and-release step-through in a cutaway, child
// proxies with measured clearances (kit on and off), and top-down section drawings.
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { toCreasedNormals } from 'three/addons/utils/BufferGeometryUtils.js';

const V = (x, y, z) => new THREE.Vector3(x, y, z);
const P0 = [131.3, 80.7], AXH = [0.79577626, 0.60559074], TH = [-0.6083053, 0.79370313], SHX = (244.25 - 227.8) * 0.8 / 32.9, SHY = (244.25 - 227.8) * 2.7 / 32.9;
const LD = 0.0565, LT = 0.069, C = 227;
const Q = (d, t, z) => V(P0[0] + (d + 2) * AXH[0] + t * TH[0] + SHX, P0[1] + (d + 2) * AXH[1] + t * TH[1] + SHY, z);
const QS = (d, t, z) => Q(d + LD * (z - C), t + LT * (z - C), z);
const O = QS(0, 0, 0), eD = QS(1, 0, 0).sub(O), eT = QS(0, 1, 0).sub(O), eZ = QS(0, 0, 1).sub(O);
const S2W = new THREE.Matrix4().makeBasis(eD, eT, eZ).setPosition(O), W2S = S2W.clone().invert();
const sv = (d, t, z) => eD.clone().multiplyScalar(d).add(eT.clone().multiplyScalar(t)).add(eZ.clone().multiplyScalar(z));
const dirD = V(AXH[0], AXH[1], 0), dirT = V(TH[0], TH[1], 0);
const ease = (u) => u <= 0 ? 0 : u >= 1 ? 1 : u * u * (3 - 2 * u);
const lerp = (a, b, u) => a + (b - a) * u;
const COL = { chair: 0xd8d4cc, kit: 0xe9edf2, btn: 0xff6a2a, pad: 0xa9b6c2, foam: 0x22304d, carbon: 0x1d1f22, feat: 0x7b3fe4, skin: 0xe8b89a };
const M = (c, o = {}) => new THREE.MeshStandardMaterial({ color: c, roughness: .58, ...o });
const SMOOTH = /^(prong_wing|prong_carrier|tall_wing|tall_kit|tallp_wing|tallp_kit)/;

async function loadGeo() {
  const man = await (await fetch('geo.json', { cache: 'no-cache' })).json();
  const b64 = (await (await fetch('geodata.json', { cache: 'no-cache' })).json()).b; const bin = Uint8Array.from(atob(b64), (ch) => ch.charCodeAt(0)).buffer;
  const geo = {};
  for (const m of man) {
    const g = new THREE.BufferGeometry(); g.setAttribute('position', new THREE.BufferAttribute(new Float32Array(bin, m.pos[0], m.pos[1] * 3), 3));
    g.setIndex(new THREE.BufferAttribute(new Uint32Array(bin, m.idx[0], m.idx[1]), 1));
    geo[m.name] = toCreasedNormals(g, SMOOTH.test(m.name) ? Math.PI / 3.3 : Math.PI / 7);
  }
  return geo;
}
function makeView(el) {
  const r = new THREE.WebGLRenderer({ antialias: true, alpha: true }); r.setPixelRatio(Math.min(devicePixelRatio, 2)); r.localClippingEnabled = true; el.appendChild(r.domElement);
  const scene = new THREE.Scene(); const cam = new THREE.PerspectiveCamera(30, 1, 1, 5000); cam.up.set(0, 0, 1);
  const ctl = new OrbitControls(cam, r.domElement); ctl.enableDamping = true;
  scene.add(new THREE.HemisphereLight(0xffffff, 0x80868c, 1.5)); const dl = new THREE.DirectionalLight(0xffffff, 1.8); cam.add(dl); dl.position.set(80, 160, 200); scene.add(cam);
  const lab = document.createElement('div'); lab.className = 'lab'; el.appendChild(lab);
  const v = { el, r, scene, cam, ctl, labels: [], views: {}, tick: null };
  v.resize = () => { const w = el.clientWidth, h = el.clientHeight; r.setSize(w, h, false); cam.aspect = w / h; cam.updateProjectionMatrix(); };
  new ResizeObserver(v.resize).observe(el); v.resize();
  let vis = true; new IntersectionObserver((es) => { vis = es[es.length - 1].isIntersecting; }, { rootMargin: '200px' }).observe(el);
  v.label = (text, pos, cls = '') => { const d = document.createElement('div'); d.className = 'l ' + cls; d.innerHTML = text; lab.appendChild(d); const L = { d, pos, on: true }; v.labels.push(L); return L; };
  r.setAnimationLoop((t) => {
    if (!vis) return; v.tick && v.tick(t); ctl.update(); r.render(scene, cam);
    const w = el.clientWidth, h = el.clientHeight;
    for (const L of v.labels) { const p = L.pos.clone().project(cam); if (!L.on || p.z > 1) { L.d.style.display = 'none'; continue; } L.d.style.display = ''; L.d.style.left = ((p.x + 1) / 2 * w) + 'px'; L.d.style.top = ((1 - p.y) / 2 * h) + 'px'; }
  });
  v.view = (k) => { const [p, t] = v.views[k](); cam.position.copy(p); ctl.target.copy(t); ctl.update(); };
  return v;
}
function radio(row, items, on, fn) {
  const bs = items.map(([k, txt]) => { const b = document.createElement('button'); b.className = 'b' + (k === on ? ' on' : ''); b.textContent = txt; b.dataset.k = k; row.appendChild(b); return b; });
  bs.forEach((b) => b.onclick = () => { bs.forEach((x) => x.classList.toggle('on', x === b)); fn(b.dataset.k); });
  return { bs, set: (k) => bs.forEach((x) => x.classList.toggle('on', x.dataset.k === k)) };
}
function toggle(row, txt, on, fn) { const b = document.createElement('button'); b.className = 'b' + (on ? ' on' : ''); b.textContent = txt; row.appendChild(b); b.onclick = () => { on = !on; b.classList.toggle('on', on); fn(on); }; return b; }
function btn(row, txt, fn) { const b = document.createElement('button'); b.className = 'b'; b.textContent = txt; b.onclick = fn; row.appendChild(b); return b; }
const gap = (row) => { const s = document.createElement('span'); s.style.width = '10px'; row.appendChild(s); };
function dashed(scene, a, b, color = 0x2a6df4) {
  const g = new THREE.BufferGeometry().setFromPoints([a, b]); const l = new THREE.Line(g, new THREE.LineDashedMaterial({ color, dashSize: 3, gapSize: 2, depthTest: false })); l.computeLineDistances(); l.renderOrder = 9; scene.add(l); return l;
}

// ------------------------------------------------------------------ child proxies (ranges5.json: half chest, chest depth, chest centre y, shoulder z)
const CHILD = { '2y P5': { hc: 69, D: 150, cy: 34, sh: 284, r: 21, L: 140 }, '5y P95': { hc: 95, D: 185, cy: 27, sh: 415, r: 30, L: 205 } };
function childProxy(key) {
  const c = CHILD[key], g = new THREE.Group(), skin = new THREE.MeshStandardMaterial({ color: COL.skin, roughness: .7, transparent: true, opacity: .5, depthWrite: false });
  const torso = new THREE.Mesh(new THREE.CylinderGeometry(c.hc, c.hc * .94, c.sh - 50, 48, 1), skin); torso.rotation.x = Math.PI / 2; torso.scale.set(1, 1, (c.D / 2) / c.hc);
  torso.position.set(0, c.cy, 20 + (c.sh - 50) / 2); g.add(torso);
  const arm = new THREE.Mesh(new THREE.CapsuleGeometry(c.r, c.L, 8, 24), skin); arm.rotation.x = Math.PI / 2;
  const ax = c.hc + c.r - 6, az = c.sh - c.r - c.L / 2; arm.position.set(ax, c.cy, az); g.add(arm);
  g.userData = { a: V(ax, c.cy, az + c.L / 2), b: V(ax, c.cy, az - c.L / 2), r: c.r, c };
  return g;
}
function segDist(p, a, b) { const ab = b.clone().sub(a), t = Math.max(0, Math.min(1, p.clone().sub(a).dot(ab) / ab.lengthSq())); return p.distanceTo(a.clone().add(ab.multiplyScalar(t))); }
function torsoDist(p, c) { const dx = p.x / c.hc, dy = (p.y - c.cy) / (c.D / 2); const rr = Math.hypot(dx, dy); const zc = Math.max(0, Math.max(20 - p.z, p.z - (c.sh - 30))); return Math.hypot(Math.max(0, (rr - 1) * c.hc), zc); }
function clearance(geoms, child, offset) {
  let best = 1e9, bp = null; const p = V(0, 0, 0), u = child.userData;
  for (const g of geoms) {
    const P = g.attributes.position;
    for (let i = 0; i < P.count; i += 2) {
      p.fromBufferAttribute(P, i); if (offset) p.add(offset);
      if (p.z < 120 || p.z > 330) continue;
      const d = Math.min(segDist(p, u.a, u.b) - u.r, torsoDist(p, u.c)); if (d < best) { best = d; bp = p.clone(); }
    }
  }
  return [best, bp];
}

// ------------------------------------------------------------------ the three options
const OPT = {
  A: { title: 'current tongue', wing: 'wing_cad', kit: ['car_cad'], btnAxis: () => dirD.clone().negate(), approach: () => sv(0, -70, 0), press: 4.0,
    labels: [['tongue', [10, 14, 230]], ['push button', [26, -6, 240]], ['nose', [8, -14, 250]], ['slot in the wing', [10, 8, 252], 'w']],
    guides: [[[10, 25.5 - 70, 230], [10, 1, 230]]] },
  B: { title: 'three-prong plug', wing: 'prong_wing', kit: ['prong_carrier'], rods: 'prong_rods', btnAxis: () => dirD.clone().negate(), approach: () => sv(0, -70, 0), press: 4.0,
    labels: [['Ø8 carbon prong', [6, 12, 254.4]], ['Ø8 carbon prong', [6, 12, 206.4]], ['tongue', [10, 14, 232]], ['push button', [26, -6, 240]], ['69 mm nose', [8, -14, 264]], ['holes in the wing', [6, 4, 268], 'w'], ['slot', [10, 4, 222], 'w']],
    guides: [[[5.92, 19.4 - 70, 206.35], [5.92, 2, 206.35]], [[7.46, 19.6 - 70, 254.35], [7.46, 2, 254.35]], [[10, 25.5 - 70, 230], [10, 1, 230]]] },
  C: null, D: null                                     // filled in from tall.json / tallp.json (gen_tall.py C / D)
};

function optionView(el, rows, st, geo, key, cfg) {
  const v = makeView(el);
  v.scene.add(new THREE.Mesh(geo.armrest, M(COL.chair)));
  const CUT = []; const cutP = new THREE.Plane(V(0, 0, -1), cfg.cutZ || 230);
  const cm = (g, c, fill, o = {}) => { const grp = new THREE.Group(); grp.add(new THREE.Mesh(g, M(c, { clippingPlanes: CUT, ...o }))); const f = new THREE.Mesh(g, new THREE.MeshBasicMaterial({ color: fill, side: THREE.BackSide, clippingPlanes: CUT })); f.visible = false; f.userData.fill = true; grp.add(f); return grp; };
  const wing = cm(geo[cfg.wing], COL.chair, 0x8b867c); v.scene.add(wing);
  const kit = new THREE.Group(); v.scene.add(kit);                       // moves as one when connecting
  for (const k of cfg.kit) kit.add(cm(geo[k], COL.kit, 0x5d82b5));
  if (cfg.rods) kit.add(cm(geo[cfg.rods], COL.carbon, 0x000000, { roughness: .35, metalness: .2 }));
  const button = new THREE.Group(); button.add(cm(geo[cfg.btn || 'btn_cad'], COL.btn, 0xb8471a, { roughness: .45 })); kit.add(button);
  const sleeve = cm(geo[cfg.sleeve || 'sleeve_cad'], 0xff8f4a, 0xb8471a, { roughness: .45, emissive: 0x401000 }); kit.add(sleeve);
  const pad = new THREE.Group(); for (const [k, c] of [['depth', COL.kit], ['pad', COL.pad], ['ring', 0x5e6a77], ['cover', COL.foam]]) pad.add(cm(geo[(cfg.padPre || '') + k] || geo[k], c, 0x333333)); kit.add(pad);
  // labels (S-frame anchors) and the exploded view's dashed guide lines
  const labs = cfg.labels.map(([t, p, cls]) => ({ L: v.label(t, QS(...p), 'big ' + (cls || '')), p: QS(...p), wing: cls === 'w' }));
  const guides = (cfg.guides || []).map(([a, b]) => { const l = dashed(v.scene, QS(...a), QS(...b)); l.visible = false; return { l, a: QS(...a) }; });
  // children
  const kids = {}; for (const k in CHILD) { kids[k] = childProxy(k); kids[k].visible = false; v.scene.add(kids[k]); }
  const kidLab = v.label('', V(0, 0, 0), 'grn big');
  // state
  let mode = 'assembled', step = 0, u = 1, playing = false, last = 0, kidOn = null, cutOn = false;
  const AX = cfg.btnAxis(), APP = cfg.approach();
  const STEPS = [
    ['Ready: the carrier with its pad, held in front of the wing.', (u) => ({ k: 1, b: 0 })],
    ['1 · Press the push button in (4 mm, to a hard stop). The orange ring shows: not latched.', (u) => ({ k: 1, b: ease(u) })],
    ['2 · Keep it pressed and slide the carrier on until it stops.', (u) => ({ k: 1 - ease(u), b: 1 })],
    ['3 · Let go: the button springs out and its tooth clicks into the wing. Flush, no orange = latched.', (u) => ({ k: 0, b: 1 - ease(u), click: u > .6 })],
    ['4 · To take it off: press the button again.', (u) => ({ k: 0, b: ease(u) })],
    ['5 · …and pull the carrier off. Let go of the button.', (u) => ({ k: ease(u), b: u > .97 ? 0 : 1 })]
  ];
  const clickL = v.label('click', QS(28, -6, 252), 'grn big');
  const pose = (s) => {
    kit.position.copy(APP.clone().multiplyScalar(s.k)); button.position.copy(AX.clone().multiplyScalar(cfg.press * s.b));
    clickL.on = !!s.click;
  };
  const applyMode = () => {
    const off = mode === 'off', exp = mode === 'exploded';
    kit.visible = !off; pad.visible = mode !== 'exploded' && !(mode === 'steps' && false);
    if (mode === 'exploded') pose({ k: 1, b: 0 }); else if (mode !== 'steps') pose({ k: 0, b: 0 });
    labs.forEach(({ L, p, wing: w }) => { L.on = (exp && !w) || ((exp || off) && w) || (mode === 'assembled' && !w && /button/.test(L.d.textContent)); L.pos = w ? p : p.clone().add(exp ? APP : V(0, 0, 0)); });
    guides.forEach(({ l }) => l.visible = exp);
    for (const k in kids) kids[k].visible = k === kidOn;
    if (kidOn) {
      const kidG = kids[kidOn]; const geoms = [geo[cfg.wing]].concat(off ? [] : cfg.kit.map((k) => geo[k]));
      const [d, p] = clearance(geoms, kidG, null); kidLab.on = true; kidLab.pos = p || V(0, 0, 0);
      kidLab.d.textContent = `${kidOn}, kit ${off ? 'off' : 'on'}: ${d > 0 ? Math.round(d) + ' mm clear' : 'TOUCHES (' + Math.round(-d) + ' mm overlap)'}`;
      kidLab.d.className = 'l big ' + (d > 15 ? 'grn' : d > 0 ? 'amb' : 'red');
    } else kidLab.on = false;
    if (mode !== 'steps') st.innerHTML = MODE_TXT[mode];
  };
  const MODE_TXT = { assembled: 'Assembled: the mount as fitted on the chair.', exploded: 'Exploded: the carrier pulled forward; dashed lines show where each part goes in.', off: 'Kit off: the bare wing.', steps: '' };
  const showStep = () => { pose(STEPS[step][1](u)); st.innerHTML = `<b>Connect and release</b> · ${STEPS[step][0]}`; };
  v.tick = (t) => { const dt = last ? (t - last) / 1000 : 0; last = t; if (!playing) return; u += dt / 2.0; if (u >= 1) { if (step < STEPS.length - 1) { if (u >= 1.3) { step++; u = 0; } } else { u = 1; playing = false; pb.textContent = 'Play'; } } showStep(); };
  const setCut = (o) => { cutOn = o; CUT.length = 0; if (o) CUT.push(cutP); v.scene.traverse((m) => { if (m.isMesh && m.material.clippingPlanes) { m.material.needsUpdate = true; if (m.userData.fill) m.visible = o; } }); };
  // views
  const T = QS(8, 0, cfg.viewZ || 228);
  v.views = {
    parent: () => [T.clone().add(dirD.clone().multiplyScalar(240)).add(dirT.clone().multiplyScalar(-240)).add(V(0, 0, 170)), T.clone().add(V(-8, -10, -20))],
    side: () => [T.clone().add(dirD.clone().multiplyScalar(380)).add(V(0, 0, 10)), T],
    front: () => [T.clone().add(dirT.clone().multiplyScalar(-380)).add(V(0, 0, 25)), T],
    back: () => [T.clone().add(dirT.clone().multiplyScalar(420)).add(dirD.clone().multiplyScalar(50)).add(V(0, 0, 50)), T],
    child: () => [T.clone().add(V(-330, -150, 80)), T.clone().add(V(-10, -20, -20))],
    top: () => [T.clone().add(V(0, 0, 230)).add(dirT.clone().multiplyScalar(-30)), T],
    close: () => cfg.close ? cfg.close() : [QS(34, -100, 262), QS(8, 2, 232)],
    exploded: () => [T.clone().add(dirD.clone().multiplyScalar(200)).add(dirT.clone().multiplyScalar(-260)).add(V(0, 0, 90)), T.clone().add(APP.clone().multiplyScalar(.5))]
  };
  const r1 = rows[0], r2 = rows[1], r3 = rows[2];
  const mr = radio(r1, [['assembled', 'Assembled'], ['exploded', 'Exploded (labelled)'], ['off', 'Kit off: bare wing']], mode, (k) => { mode = k; playing = false; applyMode(); if (k === 'exploded') v.view('exploded'); });
  gap(r1); toggle(r1, 'Cutaway', false, (o) => { setCut(o); v.view(cfg.cutView || 'top'); });
  btn(r2, '▶ Connect / release:', () => {}).disabled = true;
  btn(r2, '◀', () => { mode = 'steps'; mr.set(''); playing = false; step = Math.max(0, step - 1); u = 1; showStep(); });
  btn(r2, '▶', () => { mode = 'steps'; mr.set(''); playing = false; step = Math.min(STEPS.length - 1, step + 1); u = 1; showStep(); });
  const pb = btn(r2, 'Play', () => { mode = 'steps'; mr.set(''); applyMode(); if (playing) { playing = false; pb.textContent = 'Play'; return; } if (step === STEPS.length - 1 && u >= 1) step = 0; u = 0; last = 0; playing = true; pb.textContent = 'Pause'; });
  gap(r2); radio(r2, [['none', 'No child'], ['2y P5', 'Child 2 y (small)'], ['5y P95', 'Child 5 y (large)']], 'none', (k) => { kidOn = k === 'none' ? null : k; applyMode(); });
  radio(r3, [['parent', 'Parent\'s view'], ['side', 'Side'], ['front', 'Front'], ['back', 'Back'], ['child', 'Child\'s side'], ['close', 'Close-up']], 'parent', (k) => v.view(k));
  applyMode(); v.view('parent');
  return { v, set: (o) => { if (o.mode) { mode = o.mode; mr.set(o.mode); } if ('kid' in o) kidOn = o.kid; if ('cut' in o) setCut(o.cut); if ('step' in o) { mode = 'steps'; step = o.step; u = o.u ?? 1; showStep(); } else applyMode(); if (o.view) v.view(o.view); },
    clearances: () => { const out = {}; for (const k in CHILD) for (const off of [false, true]) { const g = [geo[cfg.wing]].concat(off ? [] : cfg.kit.map((x) => geo[x])); out[`${k} kit ${off ? 'off' : 'on'}`] = Math.round(clearance(g, kids[k], null)[0]); } return out; } };
}

// ------------------------------------------------------------------ top-down section drawings (S frame; front of the chair at the bottom)
function drawing(root, S, key) {
  const NS = 'http://www.w3.org/2000/svg';
  const mk = (tag, a, p) => { const e = document.createElementNS(NS, tag); for (const k in a) e.setAttribute(k, a[k]); p && p.appendChild(e); return e; };
  const pl = (polys) => polys.map((p) => 'M' + p.map(([d, t]) => `${d.toFixed(2)} ${(-t).toFixed(2)}`).join('L') + 'Z').join('');
  const sec = S[key];
  const s = mk('svg', { viewBox: '-46 -48 112 112', role: 'img' }, root);
  const defs = mk('defs', {}, s);
  for (const [id, c] of [['b', '#2a6df4'], ['r', '#e0302a']]) { const m = mk('marker', { id: id + key, viewBox: '0 0 10 10', refX: 9, refY: 5, markerWidth: 4, markerHeight: 4, orient: 'auto-start-reverse' }, defs); mk('path', { d: 'M0 0L10 5L0 10z', fill: c }, m); }
  const txt = (x, y, t, cls = 'tl') => { const e = mk('text', { x, y, class: cls }, s); e.textContent = t; return e; };
  mk('path', { d: pl(sec.wing), class: 'sw', 'fill-rule': 'evenodd' }, s); mk('path', { d: pl(sec.kit), class: 'sk', 'fill-rule': 'evenodd' }, s);
  if (sec.rods) mk('path', { d: pl(sec.rods), class: 'srod' }, s);
  mk('path', { d: pl(S.s1), class: 'ss1' }, s); txt(13, -30, 'strap slit', 'tl s1');
  mk('line', { x1: 9, y1: 14, x2: -14, y2: 48, class: 'arm' }, s);
  mk('path', { d: 'M-44 56 l14 0', class: 'fb', 'marker-end': `url(#b${key})` }, s); txt(-44, 52, 'child leans (80 mm in front)', 'tl');
  for (const [d, t, dir, lab] of sec.forces) { mk('path', { d: `M${d - 10 * dir} ${-t} L${d} ${-t}`, class: 'fr', 'marker-end': `url(#r${key})` }, s); if (lab) txt(dir < 0 ? d + 2 : d - 2, -t - 2, lab, 'tl r' + (dir < 0 ? '' : ' end')); }
  const [t0, t1, lt] = sec.lever; mk('line', { x1: 60, y1: -t0, x2: 60, y2: -t1, class: 'dim' }, s);
  for (const t of [t0, t1]) mk('line', { x1: 57, y1: -t, x2: 63, y2: -t, class: 'dim' }, s);
  txt(58, -(t0 + t1) / 2 + 1.5, lt, 'tl r b end');
  for (const [x, y, t, cls] of sec.notes || []) txt(x, y, t, 'tl ' + (cls || ''));
  txt(-44, 62, 'front of the chair ↓    outer side →', 'tl mu');
}

// "load at which it first gives" (ESTIMATES, 300 N x lowest SF): ../review_r5/cad/tall_tongue.md (corrected table) and, for B,
// mount_options.md (B's safety factors x 300 N). Each row: name, [lean], lean part, [corner], corner part, wing/kit, wing-dent ticks.
const FIRST = [
  ['A · current tongue (as built)', [240], 'wing dents', [170], 'wing dents', 'w', null],
  ['B · current tongue + 2 Ø8 prongs', [480, 780], 'tongue', [330, 540], 'tongue', 'k', null],
  ['Tall tongue (plain)', [471], 'tongue', [327], 'tongue', 'k', [[483], [345]]],
  ['Tall + R2 root', [483], 'wing dents', [345], 'wing dents', 'w', null],
  ['Tall + 2 Ø2 rods', [483], 'wing dents', [345], 'wing dents', 'w', null],
  ['C · tall + R2 + 2 Ø2 rods', [483], 'wing dents', [345], 'wing dents (ties with the fuse, 350)', 'w', null],
  ['D · C + 2 Ø8 prongs', [570], 'arm', [350], 'tenon fuse', 'k', [[735, 750], [525]]]
];
function loadBars(el) {
  const W = 1100, x0 = 230, x1 = 700, X = (n) => x0 + n * (x1 - x0) / 800, rowH = 30, top = 44, lab = 720;
  const H = top + FIRST.length * rowH * 2 + 40;
  let o = `<svg viewBox="0 0 ${W} ${H}" role="img" class="bars">`;
  o += `<rect x="${X(50)}" y="${top - 8}" width="${X(150) - X(50)}" height="${FIRST.length * rowH * 2 + 4}" class="band"/>`;
  o += `<text x="${X(100)}" y="${top - 14}" class="ct b" text-anchor="middle">normal use 50–150 N</text>`;
  o += `<line x1="${X(300)}" x2="${X(300)}" y1="${top - 8}" y2="${top - 4 + FIRST.length * rowH * 2}" class="l300"/><text x="${X(300)}" y="${top - 14}" class="ct b r" text-anchor="middle">300 N target</text>`;
  for (const n of [0, 100, 200, 300, 400, 500, 600, 700, 800]) o += `<text x="${X(n)}" y="${top + 16 + FIRST.length * rowH * 2}" class="ct mu" text-anchor="middle">${n} N</text>`;
  o += `<text x="${lab}" y="${top - 14}" class="ct b">first gives (estimate)</text>`;
  FIRST.forEach(([name, lean, lp, corner, cp, wk, ticks], i) => {
    const y = top + i * rowH * 2;
    if (i) o += `<line x1="0" x2="${W}" y1="${y - 4}" y2="${y - 4}" class="sep"/>`;
    o += `<text x="4" y="${y + rowH * .75}" class="ct b">${name}</text>`;
    [[lean, lp, 'lean'], [corner, cp, 'corner']].forEach(([v, part, kind], j) => {
      const yy = y + j * rowH; const cls = wk === 'w' ? 'bw' : 'bk'; const h = rowH * .62;
      o += `<rect x="${X(0)}" y="${yy}" width="${X(v[0]) - X(0)}" height="${h}" class="${cls}"/>`;
      if (v[1]) o += `<rect x="${X(v[0])}" y="${yy}" width="${X(v[1]) - X(v[0])}" height="${h}" class="${cls} rng"/>`;
      if (ticks) { const tv = ticks[j]; for (const t of tv) o += `<line x1="${X(t)}" x2="${X(t)}" y1="${yy - 2}" y2="${yy + h + 2}" class="tick"/>`; }
      o += `<text x="${lab}" y="${yy + h * .8}" class="ct"><tspan class="b">${kind}</tspan> ≈ ${v.join('–')} N · ${part}${ticks ? ` · wing dents ≈ ${ticks[j].join('–')} N` : ''}</text>`;
    });
  });
  el.innerHTML = o + '</svg><p class="mu"><span class="swatch bw"></span> the wing (part of the chair) gives first &nbsp; <span class="swatch bk"></span> a reprintable kit part gives first (tongue, arm or the tenon fuse) &nbsp; <span class="swatch tk"></span> where the wing would start to dent. Estimates: 300 N × the lowest safety factor, from the CAD agent\'s files (B: its safety factors × 300 N). With the prongs (D) the kit still gives at about the same load, but it is always a cheap reprintable part, and the wing holds to ≈ 735–750 N lean / 525 N corner.</p>';
}
export async function mountReview() {
  const J = (f) => fetch(f, { cache: 'no-cache' }).then((r) => r.ok ? r.json() : null);
  const [geo, S, SP, O6, O7] = await Promise.all([loadGeo(), J('sections.json'), J('prong_sections2.json'), J('tall.json'), J('tallp.json')]);
  const SECT = {
    A: { wing: S.tongue.wing, kit: S.tongue.kit, forces: [[19.5, 5, -1, 'outer wall'], [0.5, 13, 1, 'inner wall']], lever: [5, 13, '≈ 8 mm'],
      notes: [[22, -2, 'tongue', 'tl b'], [24, 28, 'wing', 'tl mu']] },
    B: { wing: SP.tongue.wing, kit: SP.tongue.kit, rods: SP.rod.rods, forces: [[19.5, 5, -1, 'tongue'], [0.5, 13, 1, ''], [11, 3.5, -1, 'prong'], [1, 18, 1, 'prong']], lever: [3.5, 18, '≈ 14 mm'],
      notes: [[20, 22, 'black: Ø8 prong (24 mm', 'tl'], [20, 26.5, 'above and below)', 'tl']] }
  };
  for (const [k, O] of [['C', O6], ['D', O7]]) if (O) { OPT[k] = O.cfg; OPT[k].btnAxis = () => sv(...O.cfg.btnAxisS).normalize(); OPT[k].approach = () => sv(...O.cfg.approachS); OPT[k].close = () => [QS(...O.cfg.closeCam[0]), QS(...O.cfg.closeCam[1])]; SECT[k] = O.section; }
  S.A = SECT.A; S.B = SECT.B; if (SECT.C) S.C = SECT.C; if (SECT.D) S.D = SECT.D;
  const out = {};
  for (const key of ['A', 'B', 'C', 'D']) {
    const q = (s) => document.getElementById(key + '-' + s);
    if (!q('v')) continue;
    if (!OPT[key]) { q('v').querySelector('.load').textContent = 'Geometry missing (run gen_tall.py).'; continue; }
    try { out[key] = optionView(q('v'), [q('c1'), q('c2'), q('c3')], q('st'), geo, key, OPT[key]); q('v').querySelector('.load')?.remove(); }
    catch (e) { console.error(e); q('v').querySelector('.load').textContent = 'Failed: ' + e.message; }
    try { if (q('d') && S[key]) drawing(q('d'), S, key); } catch (e) { console.error(e); }
  }
  try { const e = document.getElementById('bars'); if (e) loadBars(e); } catch (e) { console.error(e); }
  window.__opt = out; return out;
}
