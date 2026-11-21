// Exhaust-tip side mount: (1) look study on the real chair, shapes A/B/C, kit on/off; (2) rough 3D of three lock options.
// Geometry: exhaust/geo.json + geodata.json (pack_exh.py), from gen_exh3d.py on the repaired Onshape wing.
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { toCreasedNormals } from 'three/addons/utils/BufferGeometryUtils.js';

const V = (x, y, z) => new THREE.Vector3(x, y, z);
const P0 = [131.3, 80.7], AXH = [0.79577626, 0.60559074], TH = [-0.6083053, 0.79370313], SHX = (244.25 - 227.8) * 0.8 / 32.9, SHY = (244.25 - 227.8) * 2.7 / 32.9;
const LD = 0.0565, LT = 0.069, C = 227;
const Q = (d, t, z) => V(P0[0] + (d + 2) * AXH[0] + t * TH[0] + SHX, P0[1] + (d + 2) * AXH[1] + t * TH[1] + SHY, z);
const QS = (d, t, z) => Q(d + LD * (z - C), t + LT * (z - C), z);
const O = QS(0, 0, 0), eD = QS(1, 0, 0).sub(O), eT = QS(0, 1, 0).sub(O), eZ = QS(0, 0, 1).sub(O);
const S2W = new THREE.Matrix4().makeBasis(eD, eT, eZ).setPosition(O);
const dirD = V(AXH[0], AXH[1], 0), dirT = V(TH[0], TH[1], 0);
const COL = { chair: 0xd8d4cc, kit: 0xe9edf2, btn: 0xff6a2a, pad: 0xa9b6c2, foam: 0x22304d, carbon: 0x1d1f22, red: 0xe0302a, lock: 0x2a6df4 };
const M = (c, o = {}) => new THREE.MeshStandardMaterial({ color: c, roughness: .55, ...o });

async function loadGeo() {
  const man = await (await fetch('geo.json', { cache: 'no-cache' })).json();
  const b64 = (await (await fetch('geodata.json', { cache: 'no-cache' })).json()).b; const bin = Uint8Array.from(atob(b64), (c) => c.charCodeAt(0)).buffer;
  const geo = {};
  for (const m of man) {
    const g = new THREE.BufferGeometry(); g.setAttribute('position', new THREE.BufferAttribute(new Float32Array(bin, m.pos[0], m.pos[1] * 3), 3));
    g.setIndex(new THREE.BufferAttribute(new Uint32Array(bin, m.idx[0], m.idx[1]), 1)); geo[m.name] = toCreasedNormals(g, /^(exh_|h2h_)/.test(m.name) ? Math.PI / 3.3 : Math.PI / 7);
  }
  return geo;
}
function makeView(el) {
  const r = new THREE.WebGLRenderer({ antialias: true, alpha: true, preserveDrawingBuffer: true }); r.setPixelRatio(Math.min(devicePixelRatio, 2)); el.appendChild(r.domElement);
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
  bs.forEach((b) => b.onclick = () => { bs.forEach((x) => x.classList.toggle('on', x === b)); fn(b.dataset.k); }); return bs;
}
function toggle(row, txt, on, fn) { const b = document.createElement('button'); b.className = 'b' + (on ? ' on' : ''); b.textContent = txt; row.appendChild(b); b.onclick = () => { on = !on; b.classList.toggle('on', on); fn(on); }; return b; }
const mesh = (g, c, o) => new THREE.Mesh(g, M(c, o));
const T0 = QS(10, 4, 232);
const VIEWS = {
  parent: () => [T0.clone().add(dirD.clone().multiplyScalar(300)).add(V(0, 0, 30)), T0],
  front: () => [T0.clone().add(dirT.clone().multiplyScalar(-330)).add(V(0, 0, 20)), T0],
  child: () => [T0.clone().add(dirD.clone().multiplyScalar(-320)).add(dirT.clone().multiplyScalar(-40)).add(V(0, 0, 30)), T0],
  top: () => [T0.clone().add(V(0, 0, 300)).add(dirT.clone().multiplyScalar(-20)), T0],
  low34: () => [T0.clone().add(dirD.clone().multiplyScalar(190)).add(dirT.clone().multiplyScalar(-170)).add(V(0, 0, -90)), T0],
  p34: () => [T0.clone().add(dirD.clone().multiplyScalar(220)).add(dirT.clone().multiplyScalar(-200)).add(V(0, 0, 150)), T0.clone().add(V(-10, -10, -20))],
  close: () => [QS(95, 7, 236).add(dirT.clone().multiplyScalar(-30)), QS(16, 7, 231)]
};

// ------------------------------------------------------------------ 1. look study
function lookView(el, rows, st, geo) {
  const v = makeView(el); v.views = VIEWS;
  v.scene.add(mesh(geo.armrest, COL.chair));
  const W = {}, K = {};
  for (const s of ['A', 'B', 'C']) {
    W[s] = mesh(geo['exh_wing_' + s], COL.chair); v.scene.add(W[s]);
    K[s] = new THREE.Group(); K[s].add(mesh(geo['exh_kit_' + s], COL.chair), mesh(geo['exh_btn_' + s], COL.btn, { roughness: .45 }));
    for (const [k, c] of [['depth', COL.kit], ['pad', COL.pad], ['ring', 0x5e6a77], ['cover', COL.foam]]) K[s].add(mesh(geo[k], c));
    v.scene.add(K[s]);
  }
  const tbd = v.label('lock inside: TBD (not modelled)', QS(26, 24, 198), 'mu');
  let shape = 'B', kit = false;
  const TXT = { A: 'A · oval mouth', B: 'B · stadium (pill) mouth', C: 'C · curved rectangle (superellipse) mouth' };
  const apply = () => { for (const s of ['A', 'B', 'C']) { W[s].visible = s === shape; K[s].visible = s === shape && kit; } tbd.on = kit; st.textContent = TXT[shape] + (kit ? ', kit on: the plug fills the pocket, the trim collar seats on the lip; the orange push button sits in its centre.' : ', kit off: the tip stands ~6 mm proud and flows into the front edge.'); };
  radio(rows[0], [['A', 'A · Oval'], ['B', 'B · Stadium'], ['C', 'C · Curved rectangle']], shape, (k) => { shape = k; apply(); });
  const sp = document.createElement('span'); sp.style.width = '10px'; rows[0].appendChild(sp);
  radio(rows[0], [['off', 'Kit off'], ['on', 'Kit on']], 'off', (k) => { kit = k === 'on'; apply(); });
  radio(rows[1], [['parent', 'Parent side'], ['front', 'Front'], ['child', 'Child side'], ['top', 'Top'], ['low34', 'Low 3/4'], ['p34', '3/4'], ['close', 'Close-up of the mouth']], 'p34', (k) => v.view(k));
  apply(); v.view('p34');
  return { v, set: (o) => { if (o.shape) shape = o.shape; if ('kit' in o) kit = o.kit; apply(); if (o.view) v.view(o.view); } };
}

// ------------------------------------------------------------------ 2. lock options (rough 3D): parts built from primitives in the S frame
function stadiumShape(w, h, r) { const s = new THREE.Shape(); const hw = w / 2, hh = h / 2; s.moveTo(-hw + r, -hh); s.lineTo(hw - r, -hh); s.absarc(hw - r, -hh + r, r, -Math.PI / 2, 0); s.lineTo(hw, hh - r); s.absarc(hw - r, hh - r, r, 0, Math.PI / 2); s.lineTo(-hw + r, hh); s.absarc(-hw + r, hh - r, r, Math.PI / 2, Math.PI); s.lineTo(-hw, -hh + r); s.absarc(-hw + r, -hh + r, r, Math.PI, 1.5 * Math.PI); return s; }
// a solid extruded along local +z from z0 to z1 (local x = t, y = z, z = d), placed with a local->S matrix then S->world
function prism(shape, len, mat, bevel = 1.2) { const g = new THREE.ExtrudeGeometry(shape, { depth: len, bevelEnabled: true, bevelThickness: bevel, bevelSize: bevel, bevelSegments: 4, curveSegments: 32 }); return new THREE.Mesh(g, mat); }
const L2S = new THREE.Matrix4().makeBasis(V(0, 1, 0), V(0, 0, 1), V(1, 0, 0));   // local (x=t, y=z, z=d) -> S (d, t, z)
function lockView(el, rows, st, geo) {
  const v = makeView(el); const LT0 = QS(16, 7, 229);
  v.views = { ...VIEWS,
    p34: () => [LT0.clone().add(dirD.clone().multiplyScalar(120)).add(dirT.clone().multiplyScalar(-120)).add(V(0, 0, 70)), LT0],
    parent: () => [LT0.clone().add(dirD.clone().multiplyScalar(190)).add(V(0, 0, 10)), LT0],
    top: () => [LT0.clone().add(V(0, 0, 190)).add(dirT.clone().multiplyScalar(-10)), LT0],
    front: () => [LT0.clone().add(dirT.clone().multiplyScalar(-190)).add(V(0, 0, 10)), LT0] };
  v.scene.add(mesh(geo.armrest, COL.chair));
  const wingM = M(COL.chair, { transparent: true, opacity: .35, depthWrite: false });
  const wing = new THREE.Mesh(geo.exh_wing_B, wingM); v.scene.add(wing);
  const root = new THREE.Group(); root.matrixAutoUpdate = false; root.matrix.copy(S2W); v.scene.add(root);   // everything below is in S coordinates
  const S = (o) => { const g = new THREE.Group(); g.add(o); return g; };
  const kitM = M(COL.kit), lockM = M(COL.lock, { emissive: 0x0a2a6a }), wingPartM = M(0xc9a27a, { emissive: 0x3a2000 }), carbonM = M(COL.carbon, { roughness: .35 });
  const L = {};                                  // per lock: { plug group (moves), wing features (fixed), motion(u) }
  const plugShape = stadiumShape(29.5, 49.5, 14.75);
  function plugBody() {                          // plug (inside the pocket) + collar + button, local origin at the mouth centre on the outer face (d' 21.6)
    const g = new THREE.Group();
    const p = prism(plugShape, 15.0, kitM, 0.8); p.position.set(0, 0, -15.0); g.add(p);
    const c = prism(stadiumShape(38, 58, 19), 2.4, kitM, 0.6); c.position.set(0, 0, 0.2); g.add(c);
    const b = prism(stadiumShape(12, 24, 6), 1.6, M(COL.btn, { roughness: .45 }), 0.5); b.position.set(0, 0, 3.0); g.add(b);
    return g;
  }
  const place = (g, d, t, z, rx = 0, ry = 0, rz = 0) => { const o = new THREE.Group(); o.add(g); o.matrixAutoUpdate = false; const m = new THREE.Matrix4().makeTranslation(d, t, z).multiply(L2S).multiply(new THREE.Matrix4().makeRotationFromEuler(new THREE.Euler(rx, ry, rz))); o.matrix.copy(m); return o; };
  const D0 = 22.1, TC = 7, ZC = 231;
  function prongs() { const g = new THREE.Group(); for (const dy of [-12, 12]) { const c = new THREE.Mesh(new THREE.CylinderGeometry(4, 4, 26, 32), carbonM); c.rotation.x = Math.PI / 2; c.position.set(0, dy, -16); g.add(c); } return g; }
  // 1 hook-and-drop: a hook under the plug's foot, a ledge in the pocket floor; push in (-d) with the plug 8 up, then drop 8
  { const plug = plugBody(); const hook = prism(stadiumShape(22, 4, 2), 10, lockM, 0.5); hook.position.set(0, -24.5, -13); plug.add(hook); const pr = prongs(); plug.add(pr);
    const ledge = prism(stadiumShape(26, 5, 2.5), 8, wingPartM, 0.5); ledge.position.set(0, -27.5, -15);
    const mv = new THREE.Group(); mv.add(plug); const fix = new THREE.Group(); fix.add(ledge);
    L.hook = { mv: place(mv, D0, TC, ZC), fix: place(fix, D0, TC, ZC), prongs: pr, motion: (u) => { const a = Math.min(u / .6, 1), b = Math.max(0, (u - .6) / .4); mv.position.set(0, 8 * (1 - b), 50 * (1 - a)); },
      steps: ['push in sideways (plug 8 mm high)', 'drop 8 mm onto the hook: click'], btn: 'the push button blocks the lift' }; }
  // 2 quarter-turn: two lugs on the plug run in L-slots: push in, turn 25 deg about the plug axis
  { const plug = plugBody(); const pr = prongs(); plug.add(pr);
    for (const s of [-1, 1]) { const lug = prism(stadiumShape(8, 6, 3), 8, lockM, 0.4); lug.position.set(s * 15.5, 0, -12); plug.add(lug); }
    const fix = new THREE.Group(); for (const s of [-1, 1]) { const sl = prism(stadiumShape(4, 22, 2), 9, wingPartM, 0.4); sl.position.set(s * 19, s * -8, -13); fix.add(sl); }
    const mv = new THREE.Group(); mv.add(plug);
    L.turn = { mv: place(mv, D0, TC, ZC), fix: place(fix, D0, TC, ZC), prongs: pr, motion: (u) => { const a = Math.min(u / .6, 1), b = Math.max(0, (u - .6) / .4); mv.position.set(0, 0, 50 * (1 - a)); mv.rotation.set(0, 0, -0.44 * b); },
      steps: ['push in sideways', 'turn 25 degrees: the lugs lock behind the ledges, click'], btn: 'the push button blocks the turn back' }; }
  // 3 slanted plug: the plug's axis tilted 20 deg down-inboard; push in on the slant
  { const plug = plugBody(); const pr = prongs(); plug.add(pr); const mv = new THREE.Group(); mv.add(plug);
    const tilt = new THREE.Group(); tilt.add(mv); tilt.rotation.set(0.35, 0, 0);
    L.slant = { mv: place(tilt, D0, TC, ZC), fix: new THREE.Group(), prongs: pr, motion: (u) => { mv.position.set(0, 0, 50 * (1 - Math.min(u, 1))); },
      steps: ['push in along the slant, click'], btn: 'the push button blocks the pull back' }; }
  for (const k in L) { root.add(L[k].mv, L[k].fix); }
  let lock = 'hook', u = 1, playing = false, last = 0, prong = false;
  const lab = v.label('', QS(30, 7, 268), 'big');
  const apply = () => {
    for (const k in L) { L[k].mv.visible = L[k].fix.visible = k === lock; L[k].prongs.visible = prong; L[k].motion(k === lock ? u : 1); }
    lab.d.textContent = 'rough 3D · ' + L[lock].btn;
    const s = L[lock].steps; st.innerHTML = `<b>Connect:</b> ${s.join(' → ')}. <b>Release:</b> press the button, reverse the motion.`;
    slider.value = u;
  };
  v.tick = (t) => { const dt = last ? (t - last) / 1000 : 0; last = t; if (!playing) return; u = Math.min(1, u + dt / 2.5); if (u >= 1) playing = false; apply(); };
  radio(rows[0], [['hook', '1 · Hook-and-drop'], ['turn', '2 · Quarter-turn'], ['slant', '3 · Slanted plug']], lock, (k) => { lock = k; u = 1; apply(); });
  const sp = document.createElement('span'); sp.style.width = '10px'; rows[0].appendChild(sp);
  toggle(rows[0], 'Optional Ø8 carbon prongs', false, (o) => { prong = o; apply(); });
  const pb = document.createElement('button'); pb.className = 'b'; pb.textContent = '▶ Play connect'; pb.onclick = () => { u = 0; last = 0; playing = true; }; rows[1].appendChild(pb);
  const slider = document.createElement('input'); slider.type = 'range'; slider.min = 0; slider.max = 1; slider.step = 0.01; slider.value = 1; slider.style.maxWidth = '260px';
  slider.oninput = () => { u = +slider.value; playing = false; apply(); }; rows[1].appendChild(slider);
  radio(rows[2], [['p34', '3/4'], ['parent', 'Parent side'], ['top', 'Top'], ['front', 'Front'], ['close', 'Close-up']], 'p34', (k) => v.view(k));
  apply(); v.view('p34');
  return { v, set: (o) => { if (o.lock) lock = o.lock; if ('u' in o) u = o.u; if ('prong' in o) prong = o.prong; apply(); if (o.view) v.view(o.view); } };
}

export { loadGeo, makeView, radio, toggle, mesh, QS, S2W, dirD, dirT, V, COL, M, VIEWS };
export async function mountExh(geo) {
  geo = geo || await loadGeo(); const q = (id) => document.getElementById(id); const out = {};
  try { out.look = lookView(q('look-v'), [q('look-c1'), q('look-c2')], q('look-st'), geo); q('look-v').querySelector('.load')?.remove(); } catch (e) { console.error(e); q('look-v').querySelector('.load').textContent = 'Failed: ' + e.message; }
  try { out.lock = lockView(q('lock-v'), [q('lock-c1'), q('lock-c2'), q('lock-c3')], q('lock-st'), geo); q('lock-v').querySelector('.load')?.remove(); } catch (e) { console.error(e); q('lock-v').querySelector('.load').textContent = 'Failed: ' + e.message; }
  window.__exh = out; return out;
}
