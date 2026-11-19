// ROUND 5b mockups (#mocks-5b): four small 3D views on the real left wing, meshes from viewer/mock5b/gen_mock5b.py (packed by pack.py).
//   1 saddle      option 3 for comparison: a mount that grips both faces of the blade (m5b_saddle_L on m5b_wing_sad) vs today's tongue
//   2 carriers    why LOW and HIGH are separate prints: the LOW carrier turned over leaves a 2.3 / 2.8 mm step (m5b_flip_over / _gap)
//   3 drop-in     how the button goes in: spring + button down the opening in the nose top, split sleeve, slide on, click, release
//   4 pad screw   pad onto the foot's dovetail, nut in the groove, M5 x 10 from the child side under the foam, snap stop on the rail
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

const V = (x, y, z) => new THREE.Vector3(x, y, z);
// wing-tip frame (gbg_sidepad_v1.fs) and the lean-free frame S used by the generators: d' = d - 0.0565 (z - 227), t' = t - 0.069 (z - 227)
const P0 = [131.3, 80.7], AXH = [0.79577626, 0.60559074], TH = [-0.6083053, 0.79370313], SHX = (244.25 - 227.8) * 0.8 / 32.9, SHY = (244.25 - 227.8) * 2.7 / 32.9;
const LD = 0.0565, LT = 0.069, C = 227;
const Q = (d, t, z) => V(P0[0] + (d + 2) * AXH[0] + t * TH[0] + SHX, P0[1] + (d + 2) * AXH[1] + t * TH[1] + SHY, z);
const QS = (d, t, z) => Q(d + LD * (z - C), t + LT * (z - C), z);
const O = QS(0, 0, 0), eD = QS(1, 0, 0).sub(O), eT = QS(0, 1, 0).sub(O), eZ = QS(0, 0, 1).sub(O);
const S2W = new THREE.Matrix4().makeBasis(eD, eT, eZ).setPosition(O);
const sv = (d, t, z) => eD.clone().multiplyScalar(d).add(eT.clone().multiplyScalar(t)).add(eZ.clone().multiplyScalar(z));   // S move -> world move
const dirD = V(AXH[0], AXH[1], 0), dirT = V(TH[0], TH[1], 0);
const BT = { tc: -6.3, face: 24.6, thick: 4.6, travel: 5.0 };
const NZ = [207.3, 246.7];

const M = (c, o = {}) => new THREE.MeshStandardMaterial({ color: c, roughness: .58, ...o });
const COL = { chair: 0xd8d4cc, kit: 0xf1f2f3, btn: 0xff6a2a, red: 0xe0302a, feat: 0x7b3fe4, steel: 0x8b939c, dark: 0x4a5058, foot: 0xbcc8d3, pad: 0xa9b6c2, foam: 0x22304d, snap: 0x16a394 };
const ghost = (c, op = .25) => new THREE.MeshStandardMaterial({ color: c, transparent: true, opacity: op, depthWrite: false, roughness: .6 });
const ease = (u) => u <= 0 ? 0 : u >= 1 ? 1 : u * u * (3 - 2 * u);
const lerp = (a, b, u) => a + (b - a) * u;

function need(geo, keys) {
  const miss = keys.filter((k) => !geo[k]);
  if (miss.length) throw new Error('missing meshes ' + miss.join(', ') + ' (run python3 pack.py in viewer/)');
}
// one small viewer: renders only while on screen; optional split screen (two renders side by side); HTML labels pinned to 3D points
function makeView(el) {
  const r = new THREE.WebGLRenderer({ antialias: true, alpha: true }); r.setPixelRatio(Math.min(devicePixelRatio, 2)); r.localClippingEnabled = true;
  el.appendChild(r.domElement);
  const scene = new THREE.Scene(); const cam = new THREE.PerspectiveCamera(30, 1, 1, 4000); cam.up.set(0, 0, 1);
  const ctl = new OrbitControls(cam, r.domElement); ctl.enableDamping = true;
  scene.add(new THREE.HemisphereLight(0xffffff, 0x80868c, 1.5)); const dl = new THREE.DirectionalLight(0xffffff, 1.8); cam.add(dl); dl.position.set(80, 160, 200); scene.add(cam);
  const lab = document.createElement('div'); lab.className = 'm5lab'; el.appendChild(lab);
  const v = { el, r, scene, cam, ctl, labels: [], split: null, tick: null, views: {} };
  v.resize = () => { const w = el.clientWidth, h = el.clientHeight; r.setSize(w, h, false); cam.aspect = (v.split ? w / 2 : w) / h; cam.updateProjectionMatrix(); };
  new ResizeObserver(v.resize).observe(el); v.resize();
  let vis = true; new IntersectionObserver((es) => { vis = es[es.length - 1].isIntersecting; }, { rootMargin: '200px' }).observe(el);
  v.label = (text, pos, half = null, cls = '') => { const d = document.createElement('div'); d.className = 'm5l ' + cls; d.innerHTML = text; lab.appendChild(d); const L = { d, pos, half, on: true }; v.labels.push(L); return L; };
  const place = () => {
    const w = el.clientWidth, h = el.clientHeight;
    for (const L of v.labels) {
      if (!L.on || (L.half !== null && !v.split)) { L.d.style.display = 'none'; continue; }
      const p = L.pos.clone().project(cam); if (p.z > 1) { L.d.style.display = 'none'; continue; }
      const ww = v.split ? w / 2 : w, x0 = v.split && L.half === 1 ? w / 2 : 0;
      L.d.style.display = ''; L.d.style.left = (x0 + (p.x + 1) / 2 * ww) + 'px'; L.d.style.top = ((1 - p.y) / 2 * h) + 'px';
    }
  };
  r.setAnimationLoop((t) => {
    if (!vis) return;
    v.tick && v.tick(t); ctl.update();
    if (v.split) {
      const w = el.clientWidth, h = el.clientHeight; r.setScissorTest(true);
      for (let i = 0; i < 2; i++) { r.setViewport(i * w / 2, 0, w / 2, h); r.setScissor(i * w / 2, 0, w / 2, h); v.split[i](); r.render(scene, cam); }
      r.setScissorTest(false);
    } else { r.setViewport(0, 0, el.clientWidth, el.clientHeight); r.render(scene, cam); }
    place();
  });
  v.zoom = 1.35;
  v.view = (k) => { const [p, t] = v.views[k](); cam.position.copy(p.sub(t).multiplyScalar(v.zoom).add(t)); ctl.target.copy(t); ctl.update(); };
  return v;
}
// buttons: a row of radio buttons (one on) or a toggle
function radio(row, items, on, fn) {
  const bs = items.map(([k, txt]) => { const b = document.createElement('button'); b.className = 'b' + (k === on ? ' on' : ''); b.textContent = txt; b.dataset.k = k; row.appendChild(b); return b; });
  bs.forEach((b) => b.onclick = () => { bs.forEach((x) => x.classList.toggle('on', x === b)); fn(b.dataset.k); });
  return bs;
}
function toggle(row, txt, on, fn) {
  const b = document.createElement('button'); b.className = 'b' + (on ? ' on' : ''); b.textContent = txt; row.appendChild(b);
  b.onclick = () => { on = !on; b.classList.toggle('on', on); fn(on); }; return b;
}
const spacer = (row) => { const s = document.createElement('span'); s.style.width = '10px'; row.appendChild(s); };
// mesh with a solid "cut" fill: back faces drawn flat and darker, so a clipped part reads as solid
function cutMesh(g, color, planes, o = {}, fill = null) {
  const grp = new THREE.Group(), ci = planes.length > 1;
  grp.add(new THREE.Mesh(g, M(color, { clippingPlanes: planes, clipIntersection: ci, polygonOffset: true, polygonOffsetFactor: 1, polygonOffsetUnits: 1, ...o })));
  grp.add(new THREE.Mesh(g, new THREE.MeshBasicMaterial({ color: fill ?? new THREE.Color(color).multiplyScalar(.55), clippingPlanes: planes, clipIntersection: ci, side: THREE.BackSide })));
  return grp;
}
function edges(g, color, op = .55) { return new THREE.LineSegments(new THREE.EdgesGeometry(g, 35), new THREE.LineBasicMaterial({ color, transparent: true, opacity: op, depthWrite: false })); }

// ===================================================================== 1. saddle
function saddleView(el, row, geo) {
  need(geo, ['m5b_wing_sad', 'm5b_saddle_L', 'm5b_orig_block', 'm5b_feat_slits', 'm5b_feat_tear', 'm5b_feat_holes', 'wing_r5', 'nosearm_L', 'button_r5', 'sleeve_r5', 'carrier', 'blank1', 'blank2']);
  const v = makeView(el);
  const chairM = M(COL.chair), chairX = M(COL.chair, { transparent: true, opacity: .38, depthWrite: false });
  const hcut = new THREE.Plane(V(0, 0, -1), 230);              // across the mount at z 230 (keep below): tongue, jaws and button in section
  const sad = new THREE.Group(), cur = new THREE.Group();
  const wingS = cutMesh(geo.m5b_wing_sad, COL.chair, [hcut]), wingC = cutMesh(geo.wing_r5, COL.chair, [hcut]);
  sad.add(wingS, cutMesh(geo.m5b_saddle_L, COL.kit, [hcut], {}, 0x5d82b5), cutMesh(geo.button_r5, COL.btn, [hcut]), cutMesh(geo.sleeve_r5, COL.btn, [hcut]));
  cur.add(wingC, cutMesh(geo.nosearm_L, COL.kit, [hcut], {}, 0x5d82b5), cutMesh(geo.button_r5, COL.btn, [hcut]), cutMesh(geo.sleeve_r5, COL.btn, [hcut]));
  let cutOn = false;
  const setCut = () => { for (const g of [sad, cur]) g.traverse((m) => { if (!m.isMesh) return; m.material.clippingPlanes = cutOn ? [hcut] : []; m.material.needsUpdate = true; if (m.material.side === THREE.BackSide) m.visible = cutOn; }); };
  const arm = new THREE.Mesh(geo.armrest, chairM);
  const gh = new THREE.Group();               // the chair's original mount block (from the original wing) + today's v1.1 carrier and blanks
  gh.add(new THREE.Mesh(geo.m5b_orig_block, ghost(0xff9a3c, .35)), edges(geo.m5b_orig_block, 0xd0661a, .9));
  for (const k of ['carrier', 'blank1', 'blank2']) gh.add(new THREE.Mesh(geo[k], ghost(0x2a6df4, .14)), edges(geo[k], 0x2a6df4, .35));
  const feat = new THREE.Group(); const fM = M(COL.feat, { emissive: 0x3a1a80, roughness: .4 });
  for (const k of ['m5b_feat_slits', 'm5b_feat_tear', 'm5b_feat_holes']) feat.add(new THREE.Mesh(geo[k], fM));
  v.scene.add(sad, cur, arm, gh, feat);
  const T = Q(10, 8, 230);
  const labs = [v.label('strap slit S1', QS(3, 30, 262)), v.label('strap slit S2', QS(-6, 47, 266)), v.label('teardrop channel', QS(-2, 39, 300)),
    v.label('hole column', V(75.5, 135, 300)), v.label('hole at z 186', V(141.3, 88.5, 186.6))];
  const gl = [v.label('original chair mount', QS(24, 0, 272), null, 'og'), v.label('today (v1.1) block + blanks', QS(19, -18, 320), null, 'bl')];
  let mode = 'saddle', showG = false, showF = false;
  const apply = () => {
    sad.visible = mode === 'saddle'; cur.visible = mode === 'tongue'; gh.visible = showG; feat.visible = showF;
    for (const w of [wingS, wingC]) { w.children[0].material.transparent = showF; w.children[0].material.opacity = showF ? .38 : 1; w.children[0].material.depthWrite = !showF; w.children[0].material.needsUpdate = true; }
    arm.material = showF ? chairX : chairM;
    labs.forEach((L) => L.on = showF); gl.forEach((L) => L.on = showG);
  };
  v.views = {
    outer: () => [T.clone().add(dirD.clone().multiplyScalar(260)).add(dirT.clone().multiplyScalar(-60)).add(V(0, 0, 40)), T],
    child: () => [T.clone().add(dirD.clone().multiplyScalar(-250)).add(dirT.clone().multiplyScalar(-90)).add(V(0, 0, 60)), T],
    top: () => [T.clone().add(V(0, 0, 110)).add(dirT.clone().multiplyScalar(-12)), T],
    parent: () => [T.clone().add(dirD.clone().multiplyScalar(170)).add(dirT.clone().multiplyScalar(-150)).add(V(0, 0, 120)), T]
  };
  radio(row, [['saddle', 'Saddle (option 3)'], ['tongue', 'Bigger tongue (fixes)']], mode, (k) => { mode = k; apply(); }); spacer(row);
  toggle(row, 'Cut across (z 230)', cutOn, (o) => { cutOn = o; setCut(); if (o) v.view('top'); });
  toggle(row, 'Original mount (ghost)', showG, (o) => { showG = o; apply(); });
  toggle(row, 'Slits and holes', showF, (o) => { showF = o; apply(); }); spacer(row);
  radio(row, [['parent', 'Parent\'s view'], ['outer', 'Outer side'], ['child', 'Child side'], ['top', 'From above']], 'parent', (k) => v.view(k));
  setCut(); apply(); v.view('parent');
  return { v, set: (o) => { if (o.mode) mode = o.mode; if ('ghost' in o) showG = o.ghost; if ('feat' in o) showF = o.feat; if ('cut' in o) { cutOn = o.cut; setCut(); } apply(); } };
}

// ===================================================================== 2. LOW / HIGH carriers
function carriersView(el, row, geo) {
  need(geo, ['wing_r5', 'nosearm_L', 'm5b_carrier_H_L', 'm5b_flip_over', 'm5b_flip_gap', 'button_r5', 'sleeve_r5']);
  const v = makeView(el);
  const chairM = M(COL.chair, { polygonOffset: true, polygonOffsetFactor: 2, polygonOffsetUnits: 2 }), kitM = M(COL.kit, { polygonOffset: true, polygonOffsetFactor: 1, polygonOffsetUnits: 1 }), btnM = M(COL.btn, { roughness: .45 });
  v.scene.add(new THREE.Mesh(geo.wing_r5, chairM), new THREE.Mesh(geo.armrest, chairM));
  const low = new THREE.Group(); low.add(new THREE.Mesh(geo.nosearm_L, kitM), new THREE.Mesh(geo.button_r5, btnM), new THREE.Mesh(geo.sleeve_r5, btnM));
  // the LOW part turned over for HIGH: in the chair that is a mirror about z 227 (the right-hand LOW turned over = this; see gen_mock5b.py)
  const flip = new THREE.Group(); flip.add(new THREE.Mesh(geo.nosearm_L, kitM), new THREE.Mesh(geo.button_r5, btnM), new THREE.Mesh(geo.sleeve_r5, btnM));
  flip.scale.set(1, 1, -1); flip.position.set(0, 0, 2 * C);
  const red = new THREE.Group(); red.add(new THREE.Mesh(geo.m5b_flip_over, M(COL.red, { emissive: 0x500000 })), new THREE.Mesh(geo.m5b_flip_gap, ghost(COL.red, .55)));
  const high = new THREE.Group(); high.add(new THREE.Mesh(geo.m5b_carrier_H_L, kitM));
  const bH = new THREE.Group(); bH.add(new THREE.Mesh(geo.button_r5, btnM), new THREE.Mesh(geo.sleeve_r5, btnM)); bH.scale.set(1, 1, -1); bH.position.set(0, 0, 2 * C); high.add(bH);
  v.scene.add(low, flip, red, high);
  // measurement: the nose's front-outer corner, where it should be vs where the turned-over part puts it, top and bottom
  const mk = new THREE.Group(); v.scene.add(mk);
  const lm = new THREE.LineBasicMaterial({ color: COL.red, depthTest: false });
  for (const [z, zo] of [[NZ[1], NZ[0]], [NZ[0], NZ[1]]]) {
    const good = QS(BT.face, -14.27, z), bad = QS(BT.face, -14.27, zo).setZ(2 * C - zo);
    const l = new THREE.Line(new THREE.BufferGeometry().setFromPoints([good, bad]), lm); l.renderOrder = 9; mk.add(l);
    for (const p of [good, bad]) { const s = new THREE.Mesh(new THREE.SphereGeometry(.9, 12, 8), new THREE.MeshBasicMaterial({ color: COL.red, depthTest: false })); s.position.copy(p); s.renderOrder = 9; mk.add(s); }
  }
  const dd = (2 * LD * 20).toFixed(1), dt = (2 * LT * 20).toFixed(1);   // at the notch ends, 20 mm from the centre
  const L1 = v.label(`top: ${dd} mm sideways, ${dt} mm front-back`, QS(BT.face + 2, -15, NZ[1] + 4), 0, 'red');
  const L2 = v.label(`bottom: ${dd} mm sideways, ${dt} mm front-back`, QS(BT.face + 2, -15, NZ[0] - 4), 0, 'red');
  const H0 = v.label('LOW carrier turned over for HIGH: does not fit', QS(10, -14, 290), 0, 'hd'), H1 = v.label('made as a HIGH carrier: fits', QS(10, -14, 290), 1, 'hd ok');
  const T = QS(12, -8, 232);
  v.views = {
    front: () => [T.clone().add(dirT.clone().multiplyScalar(-190)).add(dirD.clone().multiplyScalar(120)).add(V(0, 0, 25)), T],
    outer: () => [T.clone().add(dirD.clone().multiplyScalar(220)).add(dirT.clone().multiplyScalar(-30)).add(V(0, 0, 10)), T],
    parent: () => [T.clone().add(V(150, -230, 160)), T]
  };
  let mode = 'high';
  const vis = (lo, fl, hi) => { low.visible = lo; flip.visible = red.visible = mk.visible = fl; high.visible = hi; };
  const apply = () => {
    if (mode === 'low') { v.split = null; vis(true, false, false); }
    else if (mode === 'high') v.split = [() => vis(false, true, false), () => vis(false, false, true)];
    else { v.split = null; vis(false, true, false); }
    [L1, L2].forEach((L) => { L.half = mode === 'high' ? 0 : null; L.on = mode !== 'low'; });
    v.resize();
  };
  radio(row, [['low', 'LOW carrier (fits)'], ['high', 'HIGH: turned over vs made right'], ['flip', 'Turned over, close up']], mode, (k) => { mode = k; apply(); if (k === 'flip') v.view('front'); }); spacer(row);
  radio(row, [['front', 'Front-outer'], ['outer', 'Outer side'], ['parent', 'Parent\'s view']], 'front', (k) => v.view(k));
  apply(); v.view('front');
  return { v, set: (o) => { if (o.mode) { mode = o.mode; apply(); } } };
}

// ===================================================================== 3. button drop-in (animated, cut open)
function dropView(el, row, stat, geo) {
  need(geo, ['wing_r5', 'm5b_carrier_drop_L', 'button_r5', 'm5b_sleeve_split']);
  const v = makeView(el);
  // cut: keep t' >= the button's centre line (we look from the front at the cut face), moving with the carrier
  const W2S = S2W.clone().invert(), e = W2S.elements, nT = V(e[1], e[5], e[9]), nl = nT.length();
  const cut = new THREE.Plane(nT.clone().divideScalar(nl), 0), cut2 = new THREE.Plane(nT.clone().divideScalar(-nl), 0), cutOn = [cut, cut2];   // remove only the slab t' -13.3..tc (the front half of the nose); the arm stays
  const wing = new THREE.Group(); wing.add(new THREE.Mesh(geo.wing_r5, M(COL.chair)), new THREE.Mesh(geo.armrest, M(COL.chair)));
  const car = cutMesh(geo.m5b_carrier_drop_L, COL.kit, cutOn, {}, 0x5d82b5);
  const btn = new THREE.Mesh(geo.button_r5, M(COL.btn, { roughness: .45 }));
  const slv = cutMesh(geo.m5b_sleeve_split, 0xff8f4a, cutOn, { roughness: .45 });
  // spring OD 6, wire 0.55, 8 coils, built along +x on a unit length, placed in S (d' from 8 to the button's back face)
  const pts = []; const N = 8, R = 2.725; for (let i = 0; i <= N * 32; i++) { const a = i / 32 * Math.PI * 2; pts.push(V(i / (N * 32), R * Math.cos(a), R * Math.sin(a))); }
  const spr = new THREE.Mesh(new THREE.TubeGeometry(new THREE.CatmullRomCurve3(pts), N * 32, .275, 6), M(COL.steel, { metalness: .6, roughness: .35 }));
  spr.matrixAutoUpdate = false;
  [car, btn, slv].forEach((o) => { o.matrixAutoUpdate = false; });
  v.scene.add(wing, car, btn, slv, spr);
  const A = -70;                      // carrier parked 70 mm in front of the wing while it is assembled
  // state: car = carrier t' offset; b = button [d, z] relative to the carrier; sp = spring z offset (riding with the button when equal); sl = sleeve d offset
  const S0 = { car: A, b: [-BT.travel, 45], sp: 75, sl: 30, wing: false };
  const steps = [
    ['The parts: carrier (cut open), spring, button, split orange sleeve.', () => S0],
    ['1 · The spring goes onto the pin behind the button.', (u) => ({ ...S0, sp: lerp(75, 45, ease(u)) })],
    ['2 · Press the button and drop it, with its spring, down the opening in the top of the nose (the wing hides this opening once the carrier is on).', (u) => ({ ...S0, sp: lerp(45, 0, ease(u)), b: [-BT.travel, lerp(45, 0, ease(u))] })],
    ['3 · Keep it pressed and snap the split orange sleeve in under the rim. The sleeve now holds the button in.', (u) => ({ ...S0, sp: 0, b: [-BT.travel, 0], sl: lerp(30, 0, ease(u)) })],
    ['4 · Let go: the spring pushes the button flush. It cannot come out, by the top or the face.', (u) => ({ ...S0, sp: 0, b: [lerp(-BT.travel, 0, ease(u)), 0], sl: 0 })],
    ['5 · Slide the carrier onto the wing. The tooth rides up the lead-in (button goes in), then clicks into its groove: flush, no orange = latched.', (u) => {
      const k = ease(Math.min(u / .8, 1)), pr = u < .55 ? 0 : u < .8 ? -3.6 * ease((u - .55) / .25) : -3.6 * (1 - ease((u - .8) / .2));
      return { ...S0, car: lerp(A, 0, k), sp: 0, b: [pr, 0], sl: 0, wing: true }; }],
    ['6 · To take it off: press the button in (5 mm, to a hard stop) and pull the arm forward.', (u) => {
      const pr = -BT.travel * ease(Math.min(u / .3, 1)), k = ease(Math.max(0, (u - .35) / .65));
      return { ...S0, car: lerp(0, A, k), sp: 0, b: [u > .98 ? 0 : pr, 0], sl: 0, wing: true }; }]
  ];
  const tmp = new THREE.Matrix4();
  const pose = (s) => {
    const co = sv(0, s.car, 0);
    car.matrix.makeTranslation(co.x, co.y, co.z);
    const bo = co.clone().add(sv(s.b[0], 0, s.b[1])); btn.matrix.makeTranslation(bo.x, bo.y, bo.z);
    const so = co.clone().add(sv(s.sl, 0, 0)); slv.matrix.makeTranslation(so.x, so.y, so.z);
    const riding = Math.abs(s.sp - s.b[1]) < 1e-6;
    const dBack = BT.face - BT.thick + (riding ? s.b[0] : -BT.travel);   // the spring's front end sits on the button's back face
    const len = dBack - 8.0;
    spr.matrix.copy(S2W).multiply(tmp.makeTranslation(0, s.car, 0)).multiply(tmp.makeTranslation(8.0, BT.tc - 2.2, C + s.sp)).multiply(tmp.makeScale(len, 1, 1));
    wing.visible = s.wing;
    cut.constant = (e[13] - (BT.tc + s.car)) / nl;                          // keep t'(p) - car >= tc
    cut2.constant = -(e[13] - (-13.3 + s.car)) / nl;                       // or t'(p) - car <= -13.3
  };
  let cur = 0, u = 1, playing = false, last = 0;
  const show = () => { pose(steps[cur][1](u)); stat.textContent = steps[cur][0]; };
  v.tick = (t) => {
    const dt = last ? (t - last) / 1000 : 0; last = t;
    if (playing) { u += dt / 2.6; if (u >= 1) { if (cur < steps.length - 1) { if (u >= 1.35) { cur++; u = 0; } } else { u = 1; playing = false; pb.textContent = 'Play'; } } show(); }
  };
  const T = QS(14, -6, 236);
  v.views = {
    cut: () => [T.clone().add(dirT.clone().multiplyScalar(-200)).add(dirD.clone().multiplyScalar(170)).add(V(0, 0, 150)), T.clone().add(sv(0, A * 0.75, 0))],
    top: () => [T.clone().add(V(0, 0, 260)).add(dirT.clone().multiplyScalar(-120)).add(dirD.clone().multiplyScalar(40)), T.clone().add(sv(0, A * 0.75, 0))],
    outer: () => [T.clone().add(dirD.clone().multiplyScalar(240)).add(dirT.clone().multiplyScalar(-90)).add(V(0, 0, 50)), T.clone().add(sv(0, A * 0.75, 0))]
  };
  const go = (i) => { cur = Math.max(0, Math.min(steps.length - 1, i)); u = 1; playing = false; pb.textContent = 'Play'; show(); };
  const bp = document.createElement('button'); bp.className = 'b'; bp.textContent = '◀ Prev'; bp.onclick = () => go(cur - 1); row.appendChild(bp);
  const bn = document.createElement('button'); bn.className = 'b'; bn.textContent = 'Next ▶'; bn.onclick = () => go(cur + 1); row.appendChild(bn);
  const pb = document.createElement('button'); pb.className = 'b'; pb.textContent = 'Play';
  pb.onclick = () => { if (playing) { playing = false; pb.textContent = 'Play'; return; } if (cur === steps.length - 1 && u >= 1) cur = 0; u = 0; last = 0; playing = true; pb.textContent = 'Pause'; };
  row.appendChild(pb); spacer(row);
  toggle(row, 'Cut open', true, (o) => { const p = o ? cutOn : []; for (const g of [car, slv]) g.children.forEach((m) => { m.material.clippingPlanes = p; m.material.needsUpdate = true; m.visible = o || m.material.side !== THREE.BackSide; }); }); spacer(row);
  radio(row, [['cut', 'From the front (cut)'], ['top', 'From above'], ['outer', 'Outer side']], 'cut', (k) => v.view(k));
  v.zoom = 0.75; go(0); v.view('cut');
  return { v, go, at: (i, uu) => { cur = i; u = uu; playing = false; show(); } };
}

// ===================================================================== 4. pad screw from the child side
function padView(el, row, stat, geo) {
  need(geo, ['m5b_foot_r5', 'm5b_pad_r5', 'm5b_snap_r5', 'm5b_nut_r5', 'm5b_screw_r5', 'm5b_foam_r5', 'nosearm_L', 'wing_r5']);
  const v = makeView(el);
  const cut = new THREE.Plane(V(0, 1, 0), -32.5), cutOn = [cut];      // keep y >= 32.5: the screw's own plane
  v.scene.add(new THREE.Mesh(geo.nosearm_L, M(COL.kit)));          // wing and arm rest left out: they hide the pad from most angles
  const foot = cutMesh(geo.m5b_foot_r5, COL.foot, cutOn), pad = cutMesh(geo.m5b_pad_r5, COL.pad, cutOn);
  const snap = new THREE.Mesh(geo.m5b_snap_r5, M(COL.snap, { emissive: 0x0a4a42 })); pad.add(snap);
  const nut = cutMesh(geo.m5b_nut_r5, COL.steel, cutOn, { metalness: .5 }), screw = cutMesh(geo.m5b_screw_r5, COL.dark, cutOn, { metalness: .5 });
  const foamM = M(COL.foam, { clippingPlanes: cutOn, side: THREE.DoubleSide }), foamX = M(COL.foam, { clippingPlanes: cutOn, transparent: true, opacity: .3, depthWrite: false, side: THREE.DoubleSide });
  const foam = new THREE.Mesh(geo.m5b_foam_r5, foamM);
  v.scene.add(foot, pad, nut, screw, foam);
  const ZR = 194, PS = V(0, 32.5, ZR);
  const lb = { nut: v.label('nut, hidden in the foot', V(111, 32.5, ZR + 14)), screw: v.label('M5 × 10 button head', V(100, 32.5, ZR - 9)), snap: v.label('snap stop', V(108, 45, ZR - 4), null, 'grn'), foam: v.label('foam covers the head', V(88, 32.5, ZR + 22)) };
  const S0 = { nut: -45, pad: 75, scr: -50, rot: 0, foam: -60, foamOn: false };
  const steps = [
    ['The parts: depth piece (its foot has two dovetail grooves), pad shell with one rail, nut, screw, foam.', () => S0],
    ['1 · The nut drops through the groove mouth into its pocket in the foot (then it is hidden).', (u) => ({ ...S0, nut: lerp(-45, 0, ease(u)) })],
    ['2 · The pad\'s rail slides into the groove from the back until it stops. The green snap clicks into its notch: plastic holds the pad.', (u) => ({ ...S0, nut: 0, pad: lerp(75, 0, ease(u)) })],
    ['3 · The M5 × 10 button-head screw goes in from the child side and pulls the rail tight. It only locks; it does not carry the load.', (u) => ({ ...S0, nut: 0, pad: 0, scr: lerp(-50, 0, ease(u)), rot: lerp(-8 * Math.PI, 0, ease(u)) })],
    ['4 · Foam (10 mm) and cover go on: the screw head is under the foam, nothing hard faces the child.', (u) => ({ ...S0, nut: 0, pad: 0, scr: 0, foam: lerp(-60, 0, ease(u)), foamOn: true })]
  ];
  let seeThrough = false;
  const pose = (s) => {
    nut.position.set(s.nut, 0, 0); pad.position.set(0, s.pad, 0);
    screw.position.set(s.scr, 0, 0); screw.position.add(PS); screw.rotation.set(s.rot, 0, 0); screw.position.sub(PS.clone().applyEuler(screw.rotation));
    foam.position.set(s.foam, 0, 0); foam.visible = s.foamOn; foam.material = seeThrough ? foamX : foamM;
    lb.nut.on = s.nut === 0; lb.snap.on = s.pad === 0; lb.screw.on = s.scr === 0; lb.foam.on = s.foamOn && s.foam === 0;
    snap.material.emissive.setHex(s.pad === 0 ? 0x1fbfa9 : 0x0a4a42);
  };
  let cur = 0, u = 1, playing = false, last = 0;
  const show = () => { pose(steps[cur][1](u)); stat.textContent = steps[cur][0]; };
  v.tick = (t) => {
    const dt = last ? (t - last) / 1000 : 0; last = t;
    if (playing) { u += dt / 2.4; if (u >= 1) { if (cur < steps.length - 1) { if (u >= 1.35) { cur++; u = 0; } } else { u = 1; playing = false; pb.textContent = 'Play'; } } show(); }
  };
  const T = V(104, 34, ZR);
  v.views = {
    cut: () => [T.clone().add(V(-120, -230, 70)), T],
    child: () => [T.clone().add(V(-260, -40, 40)), T],
    top: () => [T.clone().add(V(-60, -40, 280)), T]
  };
  const go = (i) => { cur = Math.max(0, Math.min(steps.length - 1, i)); u = 1; playing = false; pb.textContent = 'Play'; show(); };
  const bp = document.createElement('button'); bp.className = 'b'; bp.textContent = '◀ Prev'; bp.onclick = () => go(cur - 1); row.appendChild(bp);
  const bn = document.createElement('button'); bn.className = 'b'; bn.textContent = 'Next ▶'; bn.onclick = () => go(cur + 1); row.appendChild(bn);
  const pb = document.createElement('button'); pb.className = 'b'; pb.textContent = 'Play';
  pb.onclick = () => { if (playing) { playing = false; pb.textContent = 'Play'; return; } if (cur === steps.length - 1 && u >= 1) cur = 0; u = 0; last = 0; playing = true; pb.textContent = 'Pause'; };
  row.appendChild(pb); spacer(row);
  toggle(row, 'Cut open', true, (o) => {
    const p = o ? cutOn : [];
    for (const g of [foot, pad, nut, screw]) g.traverse((m) => { if (m.isMesh && m !== snap) { m.material.clippingPlanes = p; m.material.needsUpdate = true; if (m.material.side === THREE.BackSide) m.visible = o; } });
    for (const m of [foamM, foamX]) { m.clippingPlanes = p; m.needsUpdate = true; }
  });
  toggle(row, 'See-through foam', false, (o) => { seeThrough = o; show(); }); spacer(row);
  radio(row, [['cut', 'Cut, from the front'], ['child', 'Child side'], ['top', 'From above']], 'cut', (k) => v.view(k));
  v.zoom = 0.6; go(0); v.view('cut');
  return { v, go, foam: (o) => { seeThrough = o; show(); } };
}

export function mountMocks5b(geo) {
  const q = (id) => document.getElementById(id);
  const out = {};
  const run = (k, fn) => { try { out[k] = fn(); q('m5b-' + k)?.querySelector('.m5load')?.remove(); } catch (e) { console.error(e); const l = q('m5b-' + k)?.querySelector('.m5load'); if (l) l.textContent = 'Mockup failed: ' + e.message; } };
  run('saddle', () => saddleView(q('m5b-saddle'), q('m5b-saddle-ctl'), geo));
  run('carriers', () => carriersView(q('m5b-carriers'), q('m5b-carriers-ctl'), geo));
  run('drop', () => dropView(q('m5b-drop'), q('m5b-drop-ctl'), q('m5b-drop-st'), geo));
  run('pad', () => padView(q('m5b-pad'), q('m5b-pad-ctl'), q('m5b-pad-st'), geo));
  return out;
}
