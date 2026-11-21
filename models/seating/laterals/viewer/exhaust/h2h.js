// Front vs side: head to head. Two panes (F front-edge tongue, S side twist-in), one shared camera, kit on/off, prongs,
// cutaway, and a connect + release animation (play button + step slider, 0-1 connect, 1-2 release).
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { QS, S2W, dirD, dirT, V, COL, M } from './exh.js';

const W2S = S2W.clone().invert();
const sDir = (d, t, z) => new THREE.Vector3(d, t, z).applyMatrix3(new THREE.Matrix3().setFromMatrix4(S2W));
const ease = (x) => x <= 0 ? 0 : x >= 1 ? 1 : x * x * (3 - 2 * x);
const seg = (u, a, b) => ease((u - a) / (b - a));
// plane d' = c (S frame) as a world clipping plane keeping d' < c  (or z < c for F)
function planeD(c) { const e = W2S.elements; const n = new THREE.Vector3(e[0], e[4], e[8]); const k = n.length(); return new THREE.Plane(n.clone().multiplyScalar(-1 / k), (c - e[12]) / k); }
const planeZ = (c) => new THREE.Plane(new THREE.Vector3(0, 0, -1), c);

function pane(el, camera, geo, cfg) {
  const r = new THREE.WebGLRenderer({ antialias: true, alpha: true, preserveDrawingBuffer: true }); r.setPixelRatio(Math.min(devicePixelRatio, 2)); r.localClippingEnabled = true; el.appendChild(r.domElement);
  const scene = new THREE.Scene(); scene.add(new THREE.HemisphereLight(0xffffff, 0x80868c, 1.5));
  const lab = document.createElement('div'); lab.className = 'lab'; el.appendChild(lab);
  const p = { el, r, scene, labels: [], clip: [cfg.cut], cutOn: false, parts: {} };
  const mat = (c, o = {}) => M(c, { clippingPlanes: p.clip, ...o });
  const add = (name, c, parent, o = {}) => {
    const g = geo[name]; if (!g) { console.warn('missing', name); return null; }
    const m = new THREE.Mesh(g, mat(c, o)); parent.add(m);
    const cap = new THREE.Mesh(g, mat(cfg.capColor || 0xc0392b, { side: THREE.BackSide })); cap.visible = false; parent.add(cap); m.userData.cap = cap; p.parts[name] = m; return m;
  };
  p.label = (html, pos, cls = '') => { const d = document.createElement('div'); d.className = 'l ' + cls; d.innerHTML = html; lab.appendChild(d); const L = { d, pos, on: true, obj: null }; p.labels.push(L); return L; };
  p.resize = () => { const w = el.clientWidth, h = el.clientHeight; r.setSize(w, h, false); }; new ResizeObserver(p.resize).observe(el); p.resize();
  p.render = () => {
    r.render(scene, camera); const w = el.clientWidth, h = el.clientHeight;
    for (const L of p.labels) { const q = (L.obj ? L.pos.clone().applyMatrix4(L.obj.matrixWorld) : L.pos.clone()).project(camera); if (!L.on || q.z > 1) { L.d.style.display = 'none'; continue; } L.d.style.display = ''; L.d.style.left = ((q.x + 1) / 2 * w) + 'px'; L.d.style.top = ((1 - q.y) / 2 * h) + 'px'; }
  };
  p.add = add; p.mat = mat; return p;
}
// group that moves with the kit: its matrix = S2W * A(S) * W2S
function kitGroup(scene) { const g = new THREE.Group(); g.matrixAutoUpdate = false; scene.add(g); return g; }
function setS(g, A) { g.matrix.copy(S2W).multiply(A).multiply(W2S); g.matrixWorldNeedsUpdate = true; }

export function mountH2H(geo, J) {
  const q = (id) => document.getElementById(id);
  const cam = new THREE.PerspectiveCamera(30, 1, 1, 5000); cam.up.set(0, 0, 1);
  const T0 = QS(12, 4, 244);
  const VIEWS = {
    p34: () => [T0.clone().add(dirD.clone().multiplyScalar(230)).add(dirT.clone().multiplyScalar(-230)).add(V(0, 0, 150)), T0.clone().add(V(-15, -10, -25))],
    parent: () => [T0.clone().add(dirD.clone().multiplyScalar(340)).add(V(0, 0, 20)), T0],
    front: () => [T0.clone().add(dirT.clone().multiplyScalar(-360)).add(V(0, 0, 25)), T0],
    top: () => [T0.clone().add(V(0, 0, 330)).add(dirT.clone().multiplyScalar(-30)), T0],
    child: () => [T0.clone().add(dirD.clone().multiplyScalar(-340)).add(dirT.clone().multiplyScalar(-60)).add(V(0, 0, 40)), T0],
    low34: () => [T0.clone().add(dirD.clone().multiplyScalar(210)).add(dirT.clone().multiplyScalar(-200)).add(V(0, 0, -110)), T0],
    close: () => [QS(120, -40, 290), QS(10, 4, 250)]
  };
  const AX = J.S.axis_S;                                          // [face d', t', z] of the S plug axis
  // ---------------- pane F
  const F = pane(q('h2h-F'), cam, geo, { cut: planeZ(262) });
  F.scene.add(new THREE.Mesh(geo.armrest, M(COL.chair)));
  const Fw = { off: F.add('h2h_F_wing', COL.chair, F.scene), on: F.add('h2h_FP_wing', COL.chair, F.scene) };
  const FK = kitGroup(F.scene); const FKp = {};
  FKp.std = new THREE.Group(); FKp.pr = new THREE.Group(); FK.add(FKp.std, FKp.pr);
  F.add('h2h_F_kit', COL.kit, FKp.std); F.add('h2h_FP_kit', COL.kit, FKp.pr); F.add('h2h_FP_rods', COL.carbon, FKp.pr); F.add('h2h_F_rods', COL.carbon, FKp.std);
  for (const [k, c] of [['depth', COL.kit], ['pad', COL.pad], ['ring', 0x5e6a77], ['cover', COL.foam]]) F.add(k, c, FK);
  const Fb = new THREE.Group(); Fb.matrixAutoUpdate = false; FK.add(Fb); F.add('h2h_F_btn', COL.btn, Fb); F.add('h2h_F_sleeve', 0xff8a3d, FK);
  const Fej = new THREE.Mesh(new THREE.CylinderGeometry(2.2, 2.2, 6, 20), F.mat(0x8a96a3, { metalness: .6 })); { const p = QS(10, 27.5, 254); Fej.position.copy(p); Fej.quaternion.setFromUnitVectors(V(0, 1, 0), dirT); F.scene.add(Fej); Fej.visible = false; }
  // ---------------- pane S
  const S = pane(q('h2h-S'), cam, geo, { cut: planeD(11.5) });
  S.scene.add(new THREE.Mesh(geo.armrest, M(COL.chair)));
  S.add('h2h_S_wing', COL.chair, S.scene);
  const SK = kitGroup(S.scene);
  S.add('h2h_S_kit', COL.kit, SK); const Sb = new THREE.Group(); Sb.matrixAutoUpdate = false; SK.add(Sb); S.add('h2h_S_btn', COL.btn, Sb);
  for (const [k, c] of [['depth', COL.kit], ['pad', COL.pad], ['ring', 0x5e6a77], ['cover', COL.foam]]) S.add(k, c, SK);
  // turn arrow (S frame arc about the plug axis, at d' face + 9, radius 40, 60-110 deg), shown during the turn
  const arc = new THREE.Group(); arc.matrixAutoUpdate = false; arc.matrix.copy(S2W); S.scene.add(arc);
  const pts = []; for (let i = 0; i <= 30; i++) { const th = (55 + 55 * i / 30) * Math.PI / 180; pts.push(V(AX[0] + 9, AX[1] + 40 * Math.cos(th), AX[2] + 40 * Math.sin(th))); }
  const tube = new THREE.Mesh(new THREE.TubeGeometry(new THREE.CatmullRomCurve3(pts), 40, 1.6, 10), new THREE.MeshBasicMaterial({ color: 0xff3b30 })); arc.add(tube);
  const head = new THREE.Mesh(new THREE.ConeGeometry(4.5, 10, 20), new THREE.MeshBasicMaterial({ color: 0xff3b30 })); arc.add(head);
  const setArrow = (dir) => { const i = dir > 0 ? pts.length - 1 : 0, j = dir > 0 ? i - 1 : 1; head.position.copy(pts[i]); head.quaternion.setFromUnitVectors(V(0, 1, 0), pts[i].clone().sub(pts[j]).normalize()); };
  const sNo = S.label('no prong option: the plug turns as it seats', QS(10, 10, 300), 'mu'); sNo.on = false;
  const fLab = F.label('', QS(12, -30, 318), 'big'), sLab = S.label('', QS(12, -30, 318), 'big');
  const fEj = F.label('ejector spring (nudge 3 mm)', QS(10, 27.5, 254), 'mu'); fEj.on = false;
  const sTbd = S.label('return spring + helix', QS(AX[0] - 8, AX[1], AX[2] - 10), 'mu'); sTbd.on = false;
  // ---------------- state + animation
  const st = { kit: true, prong: false, cut: false, u: 0.999, play: false, last: 0 };
  const FT = [[0, .8, 'Line the tongue up with the flared slot and push it back: the tapered tongue and the rounded mouth centre it.'], [.8, 1, 'Click: the deep catch drops into its groove; the orange tell sleeve goes flush.'],
    [1, 1.15, 'Press the button: it cams the catch out of the groove.'], [1.15, 1.3, '…and the ejector spring nudges the tongue 3 mm forward: unlatched, still hanging.'], [1.3, 2.01, 'Pull forward.']];
  const STX = [[0, .6, 'Hold the pad turned 30° and push the plug straight in: the lugs go through the ends of the mouth.'], [.6, .92, 'Keep pushing: the helical ramp turns it 30° home in one motion (red arrow).'], [.92, 1, 'Click: the button drops into its notch; the collar lines up with the mouth.'],
    [1, 1.12, 'Press the button.'], [1.12, 1.42, '…the return spring turns it back 30° and the helix pops the plug out 4 mm.'], [1.42, 2.01, 'Pull it out.']];
  const txt = (T, u) => (T.find(([a, b]) => u >= a && u < b) || T[T.length - 1])[2];
  const apply = () => {
    const u = st.u;
    // F: approach along -t' 70, eject 3 at 1.15-1.3, pull 1.3-2
    let ft = u < 1 ? -70 * (1 - seg(u, 0, .8)) : u < 1.3 ? -3 * seg(u, 1.15, 1.3) : -3 - 67 * seg(u, 1.3, 2);
    setS(FK, new THREE.Matrix4().makeTranslation(0, ft, 0));
    const fpress = u >= 1 && u < 1.42 ? 4 * Math.min(seg(u, 1, 1.08), 1 - seg(u, 1.34, 1.42)) : 0;
    Fb.matrix.copy(S2W).multiply(new THREE.Matrix4().makeTranslation(-fpress, 0, 0)).multiply(W2S); Fb.matrixWorldNeedsUpdate = true;
    // S: start at +60 d', -30 deg; in 0-.6; turn .6-.92; press 1-1.12; turn back + pop 1.12-1.42; out 1.42-2
    let sd, ang;
    if (u < 1) { sd = 60 * (1 - seg(u, 0, .6)); ang = -30 * (1 - seg(u, .6, .92)); }
    else if (u < 1.42) { const k = seg(u, 1.12, 1.42); sd = 4 * k; ang = -30 * k; }
    else { sd = 4 + 56 * seg(u, 1.42, 2); ang = -30; }
    const A = new THREE.Matrix4().makeTranslation(sd, 0, 0).multiply(new THREE.Matrix4().makeTranslation(0, AX[1], AX[2])).multiply(new THREE.Matrix4().makeRotationX(ang * Math.PI / 180)).multiply(new THREE.Matrix4().makeTranslation(0, -AX[1], -AX[2]));
    setS(SK, A);
    const spress = u >= 1 && u < 1.2 ? 4 * Math.min(seg(u, 1, 1.06), 1 - seg(u, 1.14, 1.2)) : 0;
    Sb.matrix.copy(S2W).multiply(new THREE.Matrix4().makeTranslation(-spress, 0, 0)).multiply(W2S); Sb.matrixWorldNeedsUpdate = true;
    const turning = (u > .6 && u < .93) || (u > 1.1 && u < 1.43); arc.visible = turning; setArrow(u < 1 ? 1 : -1);
    fLab.d.textContent = 'F · ' + txt(FT, u); sLab.d.textContent = 'S · ' + txt(STX, u);
    q('h2h-Ft').textContent = txt(FT, u); q('h2h-St').textContent = txt(STX, u);
    fLab.on = sLab.on = false; fEj.on = st.cut && u > 1 && u < 1.4; sTbd.on = st.cut && u > 1 && u < 1.45; Fej.visible = st.cut;
    // visibility
    const hasP = !!Fw.on; if (Fw.off) Fw.off.visible = !(st.prong && hasP); if (Fw.on) Fw.on.visible = st.prong; FKp.std.visible = !(st.prong && hasP); FKp.pr.visible = st.prong && hasP;
    FK.visible = st.kit; SK.visible = st.kit; sNo.on = st.prong;
    for (const P of [F, S]) { P.clip.length = 0; if (st.cut) P.clip.push(P === F ? planeZ(262) : planeD(11.5)); for (const m of Object.values(P.parts)) { m.userData.cap.visible = st.cut && m.visible; } }
    q('h2h-u').value = u;
  };
  // controls: one camera, two orbit controllers kept on the same target
  const ctls = [F, S].map((P) => { const c = new OrbitControls(cam, P.r.domElement); c.enableDamping = true; return c; });
  ctls.forEach((c, i) => c.addEventListener('change', () => { const o = ctls[1 - i]; if (!o.target.equals(c.target)) o.target.copy(c.target); }));
  const view = (k) => { const [p, t] = VIEWS[k](); cam.position.copy(p); ctls.forEach((c) => { c.target.copy(t); c.update(); }); };
  const resize = () => { const w = F.el.clientWidth, h = F.el.clientHeight; cam.aspect = w / h; cam.updateProjectionMatrix(); };
  new ResizeObserver(resize).observe(F.el); resize();
  let vis = true; new IntersectionObserver((es) => { vis = es[es.length - 1].isIntersecting; }, { rootMargin: '200px' }).observe(q('h2h'));
  F.r.setAnimationLoop((t) => {
    const dt = st.last ? (t - st.last) / 1000 : 0; st.last = t;
    if (st.play) { st.u = Math.min(2, st.u + dt / 6); if (st.u >= 2) st.play = false; apply(); }
    if (!vis) return; ctls.forEach((c) => c.update()); F.render(); S.render();
  });
  // UI
  const row = q('h2h-c1'), row2 = q('h2h-c2');
  const tog = (txt, on, fn) => { const b = document.createElement('button'); b.className = 'b' + (on ? ' on' : ''); b.textContent = txt; row.appendChild(b); b.onclick = () => { on = !on; b.classList.toggle('on', on); fn(on); }; return b; };
  tog('Kit on', true, (o) => { st.kit = o; apply(); }); tog('Optional Ø8 prongs', false, (o) => { st.prong = o; apply(); }); tog('Cutaway', false, (o) => { st.cut = o; apply(); });
  const pb = document.createElement('button'); pb.className = 'b'; pb.textContent = '▶ Play connect + release'; pb.onclick = () => { st.u = 0; st.play = true; st.kit = true; }; row2.appendChild(pb);
  const sl = q('h2h-u'); sl.oninput = () => { st.u = +sl.value; st.play = false; apply(); }; row2.appendChild(sl);
  for (const [k, txt] of [['p34', '3/4'], ['parent', 'Parent side'], ['front', 'Front'], ['child', 'Child side'], ['top', 'Top'], ['low34', 'Low 3/4'], ['close', 'Close-up']]) { const b = document.createElement('button'); b.className = 'b'; b.textContent = txt; b.onclick = () => view(k); row2.appendChild(b); }
  apply(); view('p34');
  const api = { set: (o) => { Object.assign(st, o); apply(); if (o.view) view(o.view); }, view, F, S };
  window.__h2h = api; return api;
}
