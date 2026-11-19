// Mount options document: one interactive 3D view and one top-down section drawing per option, on the real chair (left side).
// Geometry: options/geo.json + geodata.json (gen_options.py): the r5cad build (real Onshape parts), the stays, the thick saddle on
// the clean wing, protected wing features. Sections: options/sections.json.
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
const toS = (p) => p.clone().applyMatrix4(W2S);
const sv = (d, t, z) => eD.clone().multiplyScalar(d).add(eT.clone().multiplyScalar(t)).add(eZ.clone().multiplyScalar(z));
const ease = (u) => u <= 0 ? 0 : u >= 1 ? 1 : u * u * (3 - 2 * u);
const dirD = V(AXH[0], AXH[1], 0), dirT = V(TH[0], TH[1], 0);
let SADDLE_BTN_D = 33.0;
const COL = { chair: 0xd8d4cc, kit: 0xeef0f2, btn: 0xff6a2a, stay: 0xe9ecef, pad: 0xa9b6c2, foam: 0x22304d, feat: 0x7b3fe4, red: 0xe0302a };
const M = (c, o = {}) => new THREE.MeshStandardMaterial({ color: c, roughness: .58, ...o });

async function loadGeo() {
  const man = await (await fetch('geo.json')).json();
  const b64 = (await (await fetch('geodata.json')).json()).b; const bin = Uint8Array.from(atob(b64), (ch) => ch.charCodeAt(0)).buffer;
  const geo = {};
  for (const m of man) {
    const g = new THREE.BufferGeometry(); g.setAttribute('position', new THREE.BufferAttribute(new Float32Array(bin, m.pos[0], m.pos[1] * 3), 3));
    g.setIndex(new THREE.BufferAttribute(new Uint32Array(bin, m.idx[0], m.idx[1]), 1));
    // rounded level-set rims: smooth shading up to ~55 deg between facets; CAD parts keep their crisp 25 deg creases
    geo[m.name] = toCreasedNormals(g, /^(prong_wing|prong_carrier|saddle2|socket|slots2_wing)/.test(m.name) ? Math.PI / 3.3 : Math.PI / 7);
  }
  return geo;
}
function makeView(el) {
  const r = new THREE.WebGLRenderer({ antialias: true, alpha: true }); r.setPixelRatio(Math.min(devicePixelRatio, 2)); el.appendChild(r.domElement);
  const scene = new THREE.Scene(); const cam = new THREE.PerspectiveCamera(30, 1, 1, 5000); cam.up.set(0, 0, 1);
  const ctl = new OrbitControls(cam, r.domElement); ctl.enableDamping = true;
  scene.add(new THREE.HemisphereLight(0xffffff, 0x80868c, 1.5)); const dl = new THREE.DirectionalLight(0xffffff, 1.8); cam.add(dl); dl.position.set(80, 160, 200); scene.add(cam);
  const lab = document.createElement('div'); lab.className = 'lab'; el.appendChild(lab);
  const v = { el, r, scene, cam, ctl, labels: [], views: {} };
  v.resize = () => { const w = el.clientWidth, h = el.clientHeight; r.setSize(w, h, false); cam.aspect = w / h; cam.updateProjectionMatrix(); };
  new ResizeObserver(v.resize).observe(el); v.resize();
  let vis = true; new IntersectionObserver((es) => { vis = es[es.length - 1].isIntersecting; }, { rootMargin: '200px' }).observe(el);
  v.label = (text, pos, cls = '') => { const d = document.createElement('div'); d.className = 'l ' + cls; d.innerHTML = text; lab.appendChild(d); const L = { d, pos, on: true }; v.labels.push(L); return L; };
  r.setAnimationLoop(() => {
    if (!vis) return; v.tick && v.tick(performance.now()); ctl.update(); r.render(scene, cam);
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
const gap = (row) => { const s = document.createElement('span'); s.style.width = '10px'; row.appendChild(s); };
const mesh = (g, c, o) => new THREE.Mesh(g, M(c, o));

// one option view. kind: 'stay' | 'saddle' | 'asis'
const CHILD = {                                      // proxies from ../site/ranges5.json (half chest, chest depth D, chest centre y, shoulder z)
  '2y P5': { hc: 69, D: 150, cy: 34, sh: 284, r: 21, L: 140 }, '5y P95': { hc: 95, D: 185, cy: 27, sh: 415, r: 30, L: 205 }
};
function childProxy(key) {
  const c = CHILD[key], g = new THREE.Group(), skin = new THREE.MeshStandardMaterial({ color: 0xe8b89a, roughness: .7, transparent: true, opacity: .55, depthWrite: false });
  const torso = new THREE.Mesh(new THREE.CylinderGeometry(c.hc, c.hc * .92, c.sh - 40, 48, 1), skin); torso.rotation.x = Math.PI / 2; torso.scale.set(1, 1, (c.D / 2) / c.hc);
  torso.position.set(0, c.cy, 20 + (c.sh - 40) / 2); g.add(torso);
  const arm = new THREE.Mesh(new THREE.CapsuleGeometry(c.r, c.L, 8, 24), skin); arm.rotation.x = Math.PI / 2;
  const ax = c.hc + c.r - 6, az = c.sh - c.r - c.L / 2; arm.position.set(ax, c.cy, az); g.add(arm);
  g.userData = { a: V(ax, c.cy, az + c.L / 2), b: V(ax, c.cy, az - c.L / 2), r: c.r };
  return g;
}
function segDist(p, a, b) { const ab = b.clone().sub(a), t = Math.max(0, Math.min(1, p.clone().sub(a).dot(ab) / ab.lengthSq())); return p.distanceTo(a.clone().add(ab.multiplyScalar(t))); }

function optionView(el, row, row2, st, geo, kind) {
  const v = makeView(el); v.r.localClippingEnabled = true;
  v.scene.add(mesh(geo.armrest, COL.chair));
  const CUT = [];
  const cutPlane = new THREE.Plane(V(0, 0, -1), 230);
  const nD = V(0, 0, 0); { const e = W2S.elements; nD.set(e[0], e[4], e[8]).normalize(); }
  const fillW = 0x8b867c, fillK = 0x5d82b5;
  const cm = (g, c, fill, o = {}) => { const grp = new THREE.Group(); grp.add(new THREE.Mesh(g, M(c, { clippingPlanes: CUT, ...o }))); const f = new THREE.Mesh(g, new THREE.MeshBasicMaterial({ color: fill, side: THREE.BackSide, clippingPlanes: CUT })); f.visible = false; f.userData.fill = true; grp.add(f); return grp; };
  const wingFor = { stay: 'wing_cad', asis: 'wing_cad', saddle: 'saddle2_wing', prong: 'prong_wing', socket: 'socket_wing', slots: 'wing_cad' };
  const wings = {}; const wingM = [];
  const addWing = (k) => { if (wings[k]) return wings[k]; const w = cm(geo[k], COL.chair, fillW); wingM.push(w.children[0].material); v.scene.add(w); wings[k] = w; return w; };
  const feat = new THREE.Group(); for (const k of ['feat_slits', 'feat_tear', 'feat_holes']) feat.add(mesh(geo[k], COL.feat, { emissive: 0x3a1a80 })); feat.visible = false; v.scene.add(feat);
  const padSet = (g, pre = '') => { g.add(cm(geo[pre + 'depth'], COL.kit, fillK), cm(geo[pre + 'pad'], COL.pad, 0x5e6a77), mesh(geo[pre + 'ring'], 0x5e6a77), cm(geo[pre + 'cover'], COL.foam, 0x111a2d)); };
  const latch = (g) => g.add(cm(geo.btn_cad, COL.btn, 0xb8471a), cm(geo.sleeve_cad, COL.btn, 0xb8471a));
  // kit groups per configuration
  const K = {};
  const kit = (name) => { const g = new THREE.Group(); v.scene.add(g); K[name] = g; return g; };
  if (kind === 'stay' || kind === 'asis') {
    const a = kit('short'); a.add(cm(geo.car_cad, COL.chair, fillK)); latch(a); padSet(a);
    if (kind === 'stay') {
      a.add(cm(geo.stay_short, COL.stay, fillK));
      const b = kit('long'); b.add(cm(geo.hi_carrier, COL.chair, fillK), cm(geo.hi_button, COL.btn, 0xb8471a), cm(geo.hi_sleeve, COL.btn, 0xb8471a), cm(geo.stay_long, COL.stay, fillK)); padSet(b, 'hi_');
    }
  } else if (kind === 'saddle') {
    const a = kit('short'); a.add(cm(geo.saddle2_kit, COL.chair, fillK)); padSet(a);
    // (button and latch omitted on the saddle mock; it would sit on the outer jaw)
  } else if (kind === 'prong') {
    const a = kit('short'); a.add(cm(geo.prong_carrier, COL.chair, fillK), mesh(geo.prong_rods, 0x1d1f22, { roughness: .35, metalness: .2 })); latch(a); padSet(a);
  } else if (kind === 'socket') {
    const a = kit('short'); a.add(cm(geo.socket_kit, COL.chair, fillK)); padSet(a);
  } else if (kind === 'slots') {
    const l = kit('lower'); l.add(cm(geo.car_cad, COL.chair, fillK)); latch(l); padSet(l);
    const u = kit('upper'); u.add(cm(geo.slots2_car_up, COL.chair, fillK)); const lu = new THREE.Group(); latch(lu); padSet(lu); lu.position.z = 44; u.add(lu);
    const p = kit('prong'); p.add(cm(geo.prong_carrier, COL.chair, fillK), mesh(geo.prong_rods, 0x1d1f22, { roughness: .35 })); latch(p); padSet(p);
  }
  // option 3: where it would give way first
  const weak = new THREE.Group(); v.scene.add(weak); const wl = [];
  if (kind === 'asis') {
    const WK = [[QS(20.5, 5, 218), 7, '1 · slot mouth dents (≈ 170–240 N)'], [QS(10, 2.5, 230), 8, '2 · tongue root (≈ 195–285 N)'], [V(122, 38, 183), 8, '3 · tenon, the planned weak point (≈ 350–410 N corner)']];
    for (const [p, r, t] of WK) { const m = new THREE.Mesh(new THREE.SphereGeometry(r, 24, 16), new THREE.MeshBasicMaterial({ color: COL.red, transparent: true, opacity: .45, depthTest: false })); m.position.copy(p); m.renderOrder = 5; weak.add(m); wl.push(v.label(t, p, 'red')); }
  }
  const stl = kind === 'stay' ? v.label('support stay 20 × 30 → 2 × M5 into the arm rest top', V(140, 36, 152), 'grn') : null;
  // children (option 6)
  const kids = {}; let kidOn = null; const kidLab = kind === 'socket' ? v.label('', V(0, 0, 0), 'grn') : null;
  if (kind === 'socket') for (const k in CHILD) { kids[k] = childProxy(k); kids[k].visible = false; v.scene.add(kids[k]); }
  const kidGap = (k) => {
    const g = kids[k].userData, P = geo.socket_kit.attributes.position, p = V(0, 0, 0); let best = 1e9, bp = null;
    for (let i = 0; i < P.count; i += 3) { p.fromBufferAttribute(P, i); const d = segDist(p, g.a, g.b) - g.r; if (d < best) { best = d; bp = p.clone(); } }
    return [best, bp];
  };
  let state = 'on', cfg = kind === 'slots' ? 'lower' : 'short', layout = '1', slide = 1, sliding = false, last = 0, cutOn = false;
  const curWing = () => kind === 'slots' ? (layout === '2' ? 'slots2_wing' : layout === '1p' ? 'prong_wing' : 'wing_cad') : wingFor[kind];
  const curKit = () => kind === 'slots' ? (layout === '1p' ? 'prong' : layout === '2' ? cfg : 'lower') : cfg;
  const setCut = (o) => {
    cutOn = o; CUT.length = 0;
    if (o) {
      if (kind === 'prong' || (kind === 'slots' && layout === '1p')) { cutPlane.normal.copy(nD).negate(); cutPlane.constant = -cutPlane.normal.dot(QS(5.5, 0, 230)); }
      else if (kind === 'socket') { cutPlane.normal.set(0, 0, -1); cutPlane.constant = 227; }
      else { cutPlane.normal.set(0, 0, -1); cutPlane.constant = 230; }
      CUT.push(cutPlane);
    }
    v.scene.traverse((m) => { if (m.isMesh && m.material.clippingPlanes) { m.material.needsUpdate = true; if (m.userData.fill) m.visible = o; } });
  };
  const apply = () => {
    const on = state === 'on';
    for (const k in wings) wings[k].visible = false; addWing(curWing()).visible = true;
    for (const k in K) K[k].visible = on && k === curKit();
    const off = sv(0, -80 * (1 - ease(slide)), 0); for (const k in K) K[k].position.copy(kind === 'socket' ? sv(-70 * (1 - ease(slide)), 0, 0) : off);
    wl.forEach((L) => L.on = on); weak.visible = on;
    if (stl) { stl.on = on; stl.pos = cfg === 'long' ? V(140, 36, 190) : V(140, 36, 152); }
    for (const k in kids) kids[k].visible = k === kidOn;
    if (kidLab) { if (kidOn && on) { const [gap, p] = kidGap(kidOn); kidLab.on = true; kidLab.pos = p; kidLab.d.textContent = `${kidOn}: upper arm ${gap > 0 ? gap.toFixed(0) + ' mm clear of' : 'touches'} the mount`; } else kidLab.on = false; }
    st.innerHTML = (TXT[kind][kind === 'slots' ? layout + (layout === '2' ? cfg : '') : on ? 'on' : 'off'] || '') + (kind === 'slots' && !on ? ' <b>Kit off.</b>' : '') + (kind === 'stay' && on ? (cfg === 'short' ? ' <b>Short setting</b> (long arm LOW).' : ' <b>Long setting</b> (short arm HIGH): about 75 mm of drop.') : '');
  };
  const TXT = {
    stay: { on: 'Kit on: the tongue mount as built, plus a slim post from the foot of the depth piece down to the arm rest. The post takes most of the pad\'s twist straight into the arm rest.',
      off: 'Kit off: the bare wing with its 40 mm notch and slot (as built), and two M5 inserts on the arm rest top. The blank cap closes the notch.' },
    saddle: { on: 'Kit on: a clip over the wing\'s edge, jaws 12 mm thick with tapered, rounded edges; the outer jaw is 100 mm tall, the child-side jaw 69 mm tall and 28 mm long (strap slit S1).',
      off: 'Kit off: the wing with its front edge intact; only a small rounded catch notch (R ≥ 2) on the outer face.' },
    asis: { on: 'Kit on: the round-5 build as it is (red: where it would give way first, weakest first).', off: 'Kit off: the bare wing with its 40 mm notch and slot; the blank cap closes it.' },
    prong: { on: 'Kit on: everything is hidden. The 69 mm nose holds the solid tongue and two Ø8 carbon rods (black), 24 mm above and below it; four Ø2 rods run through the tongue\'s corners into the arm.',
      off: 'Kit off: a 69 mm notch with fully rounded edges (R ≈ 3), the slot, and two round holes with flared, radiused mouths: a deliberate three-opening pattern. A blank cap with the same three prongs closes it.' },
    socket: { on: 'Kit on: the plug sits in a shallow pocket in the child-side face; its rim stands 4 mm proud and feathers into the wing. The arm leaves the plug straight toward the pad. Front edge and parent side untouched.',
      off: 'Kit off: one shallow rounded pocket (25 × 32 × 14) in the child-side face, ≥ 3 mm from strap slit S1. The front edge and the parent side are intact.' },
    slots: { '1': 'Layout 1 slot (today): one slot at z 227 serves all six pad heights with the long and short arms.',
      '2lower': 'Layout 2 slots, lower slot (z 227): pad heights 172–238. The upper slot sits 44 mm higher in the extended swell.',
      '2upper': 'Layout 2 slots, upper slot (z 271), shown with the same carrier as the lower slot. A 0-drop arm (not modelled) puts the pad level with the slot, at 260 / 282.',
      '1p': 'Layout 1 slot + 2 Ø8 prongs: the same as option 4.' }
  };
  const T = QS(8, 0, kind === 'slots' ? 240 : 225);
  v.views = {
    parent: () => [T.clone().add(dirD.clone().multiplyScalar(260)).add(dirT.clone().multiplyScalar(-260)).add(V(0, 0, 200)), T.clone().add(V(-8, -10, -25))],
    side: () => [T.clone().add(dirD.clone().multiplyScalar(420)).add(V(0, 0, 10)), T],
    front: () => [T.clone().add(dirT.clone().multiplyScalar(-420)).add(V(0, 0, 30)), T],
    back: () => [T.clone().add(dirT.clone().multiplyScalar(460)).add(dirD.clone().multiplyScalar(60)).add(V(0, 0, 60)), T],
    child: () => [T.clone().add(V(-330, -120, 90)), T.clone().add(V(-10, -20, -30))],
    close: () => kind === 'socket' ? [QS(-95, 5, 240), QS(-5, 1, 227)] : kind === 'saddle' ? [QS(70, -20, 245), QS(16, -1, 227)] : [QS(30, -95, 262), QS(8, 2, 232)],
    top: () => [T.clone().add(V(0, 0, 260)).add(dirT.clone().multiplyScalar(-40)), T]
  };
  if (kind === 'slots') {
    radio(row, [['1', '1 slot (today)'], ['2', '2 slots: z 227 + 271 ★'], ['1p', '1 slot + 2 prongs']], layout, (k) => { layout = k; setCut(cutOn); apply(); }); gap(row);
    radio(row, [['lower', 'Lower slot'], ['upper', 'Upper slot']], cfg, (k) => { cfg = k; apply(); }); gap(row);
  }
  const stB = radio(row, [['on', 'Kit on'], ['off', 'Kit off: bare wing']], state, (k) => { state = k; slide = 1; apply(); });
  gap(row); const sb = document.createElement('button'); sb.className = 'b'; sb.textContent = 'Slide it on ▶'; row.appendChild(sb);
  sb.onclick = () => { state = 'on'; stB.forEach((b) => b.classList.toggle('on', b.dataset.k === 'on')); slide = 0; last = 0; sliding = true; apply(); };
  v.tick = (t) => { if (!sliding) return; const dt = last ? (t - last) / 1000 : 0; last = t; slide = Math.min(1, slide + dt / 2.4); if (slide >= 1) sliding = false; apply(); };
  if (kind === 'stay') { gap(row); radio(row, [['short', 'Short setting'], ['long', 'Long setting']], cfg, (k) => { cfg = k; apply(); }); }
  if (kind === 'socket') { gap(row); radio(row, [['none', 'No child'], ['2y P5', 'Child 2 y (small)'], ['5y P95', 'Child 5 y (large)']], 'none', (k) => { kidOn = k === 'none' ? null : k; apply(); }); }
  gap(row); toggle(row, kind === 'prong' ? 'Cut open through the rods' : kind === 'socket' ? 'Cut across the socket' : 'Cut across (z 230)', false, (o) => { setCut(o); v.view(kind === 'prong' ? 'side' : 'top'); });
  toggle(row, 'Slits and holes', false, (o) => { feat.visible = o; wingM.forEach((m) => { m.transparent = o; m.opacity = o ? .4 : 1; m.depthWrite = !o; m.needsUpdate = true; }); });
  radio(row2, [['parent', 'Parent\'s view'], ['side', 'Side'], ['front', 'Front'], ['back', 'Back'], ['child', 'Child\'s side'], ['close', 'Close-up']], 'parent', (k) => v.view(k));
  apply(); v.view('parent');
  return { v, set: (o) => { if (o.state) state = o.state; if (o.cfg) cfg = o.cfg; if (o.layout) layout = o.layout; if ('kid' in o) kidOn = o.kid; if ('slide' in o) slide = o.slide; if ('cut' in o) setCut(o.cut); apply(); if (o.view) v.view(o.view); } };
}

// top-down section drawing (S frame: d' across, t' front-to-back; front of the chair at the bottom)
function drawing(root, S, kind) {
  const NSs = 'http://www.w3.org/2000/svg';
  const mk = (tag, a, p) => { const e = document.createElementNS(NSs, tag); for (const k in a) e.setAttribute(k, a[k]); p && p.appendChild(e); return e; };
  const X = (d) => d, Y = (t) => -t;
  const pl = (lines) => lines.map((p) => 'M' + p.map(([d, t]) => `${d.toFixed(2)} ${(-t).toFixed(2)}`).join('L') + 'Z').join('');
  const sec = kind === 'saddle' ? S.saddle : kind === 'prong' ? S.prong.tongue : kind === 'socket' ? S.socket : S.tongue;
  const box = kind === 'stay' ? '-120 -50 175 150' : '-40 -50 95 115';
  const s = mk('svg', { viewBox: box, role: 'img' }, root);
  const defs = mk('defs', {}, s);
  for (const [id, c] of [['ah-b', '#2a6df4'], ['ah-r', '#e0302a'], ['ah-g', '#0b7a44']]) { const m = mk('marker', { id: id + kind, viewBox: '0 0 10 10', refX: 9, refY: 5, markerWidth: 5, markerHeight: 5, orient: 'auto-start-reverse' }, defs); mk('path', { d: 'M0 0L10 5L0 10z', fill: c }, m); }
  const txt = (x, y, t, cls = 'tl') => { const e = mk('text', { x, y, class: cls }, s); e.textContent = t; return e; };
  mk('path', { d: pl(sec.wing), class: 'sw' }, s); mk('path', { d: pl(sec.kit), class: 'sk' }, s);
  mk('path', { d: pl(S.s1), class: 'ss1' }, s);
  if (kind === 'prong') {
    const pz = (polys) => polys.map((p) => 'M' + p.map(([d, t]) => `${d.toFixed(2)} ${(-t).toFixed(2)}`).join('L') + 'Z').join('');
    mk('path', { d: pz(S.prong.rod.rods), class: 'srod' }, s);
    const rb = S.prong.rod.rods.flat(); const dmin = Math.min(...rb.map((p) => p[0])), dmax = Math.max(...rb.map((p) => p[0])), tmax = Math.max(...rb.map((p) => p[1]));
    txt(dmax + 2, -(tmax - 3), 'Ø8 carbon rods (above and below the tongue)', 'tl');
    for (const [d, t, dir] of [[dmax + 0.5, 4, -1], [dmin - 0.5, tmax - 3, 1]]) mk('path', { d: `M${d - 9 * dir} ${-t} L${d} ${-t}`, class: 'fr', 'marker-end': `url(#ah-r${kind})` }, s);
    mk('line', { x1: 50, y1: -4, x2: 50, y2: -(tmax - 3), class: 'dim' }, s);
    txt(49, -(4 + tmax - 3) / 2, `rods bear at both ends: lever ≈ ${Math.round(tmax - 7)} mm`, 'tl r end');
  }
  txt(14, Y(30), 'strap slit S1', 'tl s1');
  txt(-38, 60, 'front of the chair ↓   outer side →', 'tl mu');
  if (kind === 'stay') {
    // pad and stay positions in the S frame (z of the mount)
    const pad = toS(V(99, 14, 194)), foot = toS(V(116, 34, 194)), ar = toS(V(147, 35, 194));
    mk('rect', { x: pad.x - 3, y: -pad.y - 41, width: 6, height: 82, rx: 3, class: 'sp' }, s); txt(pad.x - 30, -pad.y - 44, 'pad (child side)');
    mk('line', { x1: X(9), y1: Y(-14), x2: foot.x, y2: -foot.y, class: 'arm' }, s); txt(foot.x - 22, -foot.y + 24, 'arm to the pad');
    mk('rect', { x: foot.x - 10, y: -foot.y - 15, width: 20, height: 30, rx: 7, class: 'sst' }, s);
    mk('rect', { x: ar.x - 10, y: -ar.y - 15, width: 20, height: 30, rx: 7, class: 'sst' }, s);
    txt(ar.x + 12, -ar.y + 2, 'stay: foot ↓ arm rest', 'tl g');
    mk('path', { d: `M${pad.x - 22} ${-pad.y} l16 0`, class: 'fb', 'marker-end': `url(#ah-b${kind})` }, s); txt(pad.x - 52, -pad.y - 5, 'child leans');
    mk('path', { d: `M${foot.x} ${-foot.y + 18} Q${(foot.x + ar.x) / 2} ${-foot.y + 34} ${ar.x} ${-ar.y + 18}`, class: 'fg', 'marker-end': `url(#ah-g${kind})` }, s);
    txt((foot.x + ar.x) / 2 - 18, -foot.y + 42, 'most of the load goes this way', 'tl g');
  } else {
    mk('line', { x1: X(9), y1: Y(-14), x2: X(-14), y2: Y(-48), class: 'arm' }, s);
    if (kind === 'socket') {
      mk('path', { d: 'M-38 48 l12 0', class: 'fb', 'marker-end': `url(#ah-b${kind})` }, s); txt(-38, 44, 'child leans, 80 mm forward');
      for (const [d, t, dir] of [[-14, -11.5, 1], [-1, 14, -1]]) mk('path', { d: `M${d} ${-(t - 9 * dir)} L${d} ${-t}`, class: 'fr', 'marker-end': `url(#ah-r${kind})` }, s);
      mk('line', { x1: -18, y1: 30, x2: -4, y2: 30, class: 'dim' }, s); txt(-30, 36, 'pocket walls bear front and back; lever ≈ 14 mm deep, 450 mm²', 'tl r');
      txt(-38, Y(18), 'child side ←', 'tl mu');
    } else if (kind !== 'prong') {
    mk('path', { d: 'M-38 48 l12 0', class: 'fb', 'marker-end': `url(#ah-b${kind})` }, s); txt(-38, 44, 'child leans, 80 mm forward');
    const F = kind === 'saddle' ? [[29, -9, -1], [-18, 10, 1]] : [[26, 4, -1], [0.5, 13, 1]];
    for (const [d, t, dir] of F) mk('path', { d: `M${d - 9 * dir} ${-t} L${d} ${-t}`, class: 'fr', 'marker-end': `url(#ah-r${kind})` }, s);
    const lv = kind === 'saddle' ? [-9, 10, 'lever ≈ 19 mm, jaws 12 mm'] : [4, 13, 'lever ≈ 8 mm'];
    mk('line', { x1: 50, y1: -lv[0], x2: 50, y2: -lv[1], class: 'dim' }, s);
    txt(49, -(lv[0] + lv[1]) / 2, lv[2], 'tl r end');
    if (kind === 'asis') { txt(28, Y(-2), '1 slot mouth', 'tl r'); txt(28, Y(-6), '2 tongue root', 'tl r'); }
    } else { mk('path', { d: 'M-38 48 l12 0', class: 'fb', 'marker-end': `url(#ah-b${kind})` }, s); txt(-38, 44, 'child leans, 80 mm forward'); }
  }
}

function coverage(el) {
  const H = [[172, '2 y small'], [194, '2 y'], [216, '3–4 y'], [238, '4 y'], [260, '5 y'], [282, '5 y large']];
  const rows = [['1 slot', H.map(() => ['227', 'r'])], ['2 slots ★', [['227', 'r'], ['227', 'r'], ['227 ↔', 'r'], ['227 ↔', 'r'], ['271 ↔', 'r'], ['271 ↔', 'r']]], ['1 slot + prongs', H.map(() => ['227 + rods', 'w'])]];
  const cw = 70, x0 = 92; let o = `<svg viewBox="0 0 ${x0 + cw * 6 + 4} ${26 + rows.length * 22}" role="img">`;
  H.forEach(([z, a], i) => { o += `<text x="${x0 + cw * i + cw / 2}" y="10" class="ct" text-anchor="middle">pad ${z}</text><text x="${x0 + cw * i + cw / 2}" y="20" class="ct mu" text-anchor="middle">${a}</text>`; });
  rows.forEach(([n, cells], r) => {
    const y = 26 + r * 22; o += `<text x="2" y="${y + 14}" class="ct">${n}</text>`;
    cells.forEach(([lab, c], i) => { o += `<rect x="${x0 + cw * i + 1}" y="${y}" width="${cw - 2}" height="19" rx="3" class="cc ${c}"/><text x="${x0 + cw * i + cw / 2}" y="${y + 13}" class="ct" text-anchor="middle">${lab}</text>`; });
  });
  el.innerHTML = o + '</svg><span class="mu">Which slot serves each pad height (↔ = level with the slot, a 0-drop arm). Red: safety ≈ 1 at a 300 N lean; amber: ≈ 1.6–3.3 with the prongs.</span>';
}
export async function mountOptions() {
  const J = (f) => fetch(f, { cache: 'no-cache' }).then((r) => r.json());
  const [geo, S, SP, SS, SK, R2] = await Promise.all([loadGeo(), J('sections.json'), J('prong_sections2.json'), J('saddle2_sections.json'), J('socket_sections.json'), J('opts2.json')]);
  S.prong = { tongue: SP.tongue, rod: SP.rod }; S.saddle = SS; S.socket = SK; SADDLE_BTN_D = R2['2'].catch_outer_d + 12.0 - 0.5;
  const out = {};
  for (const kind of ['stay', 'saddle', 'asis', 'prong', 'slots', 'socket']) {
    const q = (s) => document.getElementById(kind + '-' + s);
    try { out[kind] = optionView(q('v'), q('c1'), q('c2'), q('st'), geo, kind); q('v').querySelector('.load')?.remove(); }
    catch (e) { console.error(e); q('v').querySelector('.load').textContent = 'Failed: ' + e.message; }
    try { drawing(q('d'), S, kind); } catch (e) { console.error(e); q('d').textContent = 'Drawing failed: ' + e.message; }
  }
  try { coverage(document.getElementById('slots-cov')); } catch (e) { console.error(e); }
  window.__opt = out; return out;
}
