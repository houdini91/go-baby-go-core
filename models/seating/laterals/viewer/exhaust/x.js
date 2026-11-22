// Option X (closed exhaust socket, insert-and-rotate-in, push-button release) vs F (tall front-edge tongue). Two panes with
// one shared camera, kit on/off, prongs, cutaway, and a connect + release animation (0-1 connect, 1-2 release).
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { QS, S2W, dirD, dirT, V, COL, M } from './exh.js';
import { pane, kitGroup, setS, planeD, seg, W2S } from './h2h.js';

export function mountX(geo, J) {
  const q = (id) => document.getElementById(id);
  const X = J.X; const TF = -14.27, TP = X.pivot_S[0], ZP = X.pivot_S[1], PHI0 = -X.start_tilt_deg;
  const cam = new THREE.PerspectiveCamera(30, 1, 1, 5000); cam.up.set(0, 0, 1);
  const T0 = QS(8, -4, 250);
  const VIEWS = {
    p34: () => [T0.clone().add(dirD.clone().multiplyScalar(220)).add(dirT.clone().multiplyScalar(-240)).add(V(0, 0, 140)), T0.clone().add(V(-15, -10, -25))],
    lead: () => [T0.clone().add(dirT.clone().multiplyScalar(-360)).add(V(0, 0, 25)), T0],
    parent: () => [T0.clone().add(dirD.clone().multiplyScalar(340)).add(V(0, 0, 20)), T0],
    child: () => [T0.clone().add(dirD.clone().multiplyScalar(-340)).add(dirT.clone().multiplyScalar(-60)).add(V(0, 0, 40)), T0],
    top: () => [T0.clone().add(V(0, 0, 330)).add(dirT.clone().multiplyScalar(-30)), T0],
    low34: () => [T0.clone().add(dirD.clone().multiplyScalar(210)).add(dirT.clone().multiplyScalar(-200)).add(V(0, 0, -110)), T0],
    mouth: () => { const t = QS(5.5, -14, 254); return [t.clone().add(dirT.clone().multiplyScalar(-150)).add(dirD.clone().multiplyScalar(45)).add(V(0, 0, 20)), t]; }
  };
  // ---------------- pane X
  const Xp = pane(q('x-X'), cam, geo, { cut: planeD(5.5) });
  Xp.scene.add(new THREE.Mesh(geo.armrest, M(COL.chair)));
  Xp.add('x_wing', COL.chair, Xp.scene);
  const XK = kitGroup(Xp.scene);
  Xp.add('x_kit', COL.kit, XK, { cap: 0x2a6df4 });
  for (const [k, c] of [['depth', COL.kit], ['pad', COL.pad], ['ring', 0x5e6a77], ['cover', COL.foam]]) Xp.add(k, c, XK);
  const Xb = new THREE.Group(); Xb.matrixAutoUpdate = false; XK.add(Xb); Xp.add('x_btn', COL.btn, Xb);
  const rod = Xp.add('x_rod', 0x8a96a3, XK, { metalness: .6 });
  // orange tell: a ring round the button, visible until the catch has clicked in (hidden when locked)
  const B = X.button_S; const tell = new THREE.Mesh(new THREE.TorusGeometry(7.5, 1.1, 10, 40), Xp.mat(0xff8a3d));
  { const g = new THREE.Group(); g.matrixAutoUpdate = false; g.matrix.copy(S2W); const m = tell; m.position.set(B[0] + 0.3, B[1], B[2]); m.rotation.set(0, Math.PI / 2, 0); m.scale.set(1.5, 1, 1); g.add(m); XK.add(g); }
  // rotation arrow: arc about the pitch axis, on the parent side
  const arc = new THREE.Group(); arc.matrixAutoUpdate = false; arc.matrix.copy(S2W); Xp.scene.add(arc);
  const pts = []; for (let i = 0; i <= 30; i++) { const th = (-122 + 30 * i / 30) * Math.PI / 180; pts.push(V(24, TP + 70 * Math.cos(th), ZP + 70 * Math.sin(th))); }
  arc.add(new THREE.Mesh(new THREE.TubeGeometry(new THREE.CatmullRomCurve3(pts), 40, 1.6, 10), new THREE.MeshBasicMaterial({ color: 0xff3b30 })));
  const head = new THREE.Mesh(new THREE.ConeGeometry(4.5, 10, 20), new THREE.MeshBasicMaterial({ color: 0xff3b30 })); arc.add(head);
  const setArrow = (dir) => { const i = dir > 0 ? pts.length - 1 : 0, j = dir > 0 ? i - 1 : 1; head.position.copy(pts[i]); head.quaternion.setFromUnitVectors(V(0, 1, 0), pts[i].clone().sub(pts[j]).normalize()); };
  const L = (txt, d, t, z, cls = 'mu') => { const l = Xp.label(txt, QS(d, t, z), cls); l.on = false; return l; };
  const xl = [L('ledge (pull-out)', 5.5, -2, 312), L('beak hook', 5.5, 16, 304), L('guide wedges + seat', 5.5, 10, 284), L('steel-spring catch', 5.5, 6, 206),
    L('button rod', 5.5, -14, 222), L('throat', 5.5, -22, 262), L('pitch axis', 5.5, TP - 8, ZP + 16)];
  const xNo = Xp.label('no prongs: the plug rotates as it seats (prongs only fit a plain push-in)', QS(10, -20, 318), 'mu'); xNo.on = false;
  // ---------------- pane F (as the head-to-head)
  const F = pane(q('x-F'), cam, geo, { cut: planeD(9.9) });
  F.scene.add(new THREE.Mesh(geo.armrest, M(COL.chair)));
  const Fw = { off: F.add('h2h_F_wing', COL.chair, F.scene), on: F.add('h2h_FP_wing', COL.chair, F.scene) };
  const FK = kitGroup(F.scene); const FKp = { std: new THREE.Group(), pr: new THREE.Group() }; FK.add(FKp.std, FKp.pr);
  F.add('h2h_F_kit', COL.kit, FKp.std, { cap: 0x2a6df4 }); F.add('h2h_F_rods', COL.carbon, FKp.std); F.add('h2h_FP_kit', COL.kit, FKp.pr, { cap: 0x2a6df4 }); F.add('h2h_FP_rods', COL.carbon, FKp.pr);
  for (const [k, c] of [['depth', COL.kit], ['pad', COL.pad], ['ring', 0x5e6a77], ['cover', COL.foam]]) F.add(k, c, FK);
  const Fb = new THREE.Group(); Fb.matrixAutoUpdate = false; FK.add(Fb); F.add('h2h_F_btn', COL.btn, Fb); F.add('h2h_F_sleeve', 0xff8a3d, FK);
  // ---------------- state + animation
  const st = { kit: true, prong: false, cut: false, u: 0.999, play: false, last: 0 };
  const XT = [[0, .5, 'Hold the arm tilted 15° nose-up and push the plug straight back into the mouth: the throat and the tapered plug centre it.'],
    [.5, .9, 'Keep going: it rotates 15° nose-down (red arrow), one motion; the beak rises behind the ledge and the rear seats on the guide wedges.'],
    [.9, 1, 'CLICK: the steel-spring catch snaps in; the orange tell ring hides. The collar sits in the lip.'],
    [1, 1.1, 'Press the button: the rod pulls the catch out.'], [1.1, 1.35, '…the steel return spring rotates it back 15° and nudges it 3 mm: free.'], [1.35, 2.01, 'Pull it out.']];
  const FT = [[0, .8, 'Push the tongue straight back into the slot.'], [.8, 1, 'Click: the deep catch drops in.'], [1, 1.15, 'Press the button: it cams the catch out.'],
    [1.15, 1.3, '…the ejector nudges it 3 mm forward.'], [1.3, 2.01, 'Pull forward.']];
  const txt = (T, u) => (T.find(([a, b]) => u >= a && u < b) || T[T.length - 1])[2];
  const poseX = (phi, s) => {                                     // rotate about the pitch axis, then slide out s along the plug axis
    const R = new THREE.Matrix4().makeTranslation(0, TP, ZP).multiply(new THREE.Matrix4().makeRotationX(phi * Math.PI / 180)).multiply(new THREE.Matrix4().makeTranslation(0, -TP, -ZP));
    const ax = new THREE.Vector3(0, -1, 0).applyMatrix4(new THREE.Matrix4().makeRotationX(phi * Math.PI / 180)).multiplyScalar(s);
    return new THREE.Matrix4().makeTranslation(ax.x, ax.y, ax.z).multiply(R);
  };
  const apply = () => {
    const u = st.u; let phi, s;
    if (u < 1) { phi = PHI0 * (1 - seg(u, .5, .9)); s = 70 * (1 - seg(u, 0, .5)); }
    else if (u < 1.35) { const k = seg(u, 1.1, 1.35); phi = PHI0 * k; s = 3 * k; }
    else { phi = PHI0; s = 3 + 67 * seg(u, 1.35, 2); }
    setS(XK, poseX(phi, s));
    const xp = u >= 1 && u < 1.2 ? 3 * Math.min(seg(u, 1, 1.05), 1 - seg(u, 1.15, 1.2)) : 0;
    Xb.matrix.copy(S2W).multiply(new THREE.Matrix4().makeTranslation(-xp, 0, 0)).multiply(W2S); Xb.matrixWorldNeedsUpdate = true;
    tell.visible = !(u >= .95 && u < 1.08);
    arc.visible = (u > .5 && u < .92) || (u > 1.1 && u < 1.37); setArrow(u < 1 ? 1 : -1);
    let ft = u < 1 ? -70 * (1 - seg(u, 0, .8)) : u < 1.3 ? -3 * seg(u, 1.15, 1.3) : -3 - 67 * seg(u, 1.3, 2);
    setS(FK, new THREE.Matrix4().makeTranslation(0, ft, 0));
    const fp = u >= 1 && u < 1.42 ? 4 * Math.min(seg(u, 1, 1.08), 1 - seg(u, 1.34, 1.42)) : 0;
    Fb.matrix.copy(S2W).multiply(new THREE.Matrix4().makeTranslation(-fp, 0, 0)).multiply(W2S); Fb.matrixWorldNeedsUpdate = true;
    q('x-Xt').textContent = txt(XT, u); q('x-Ft').textContent = txt(FT, u);
    const hasP = !!Fw.on; if (Fw.off) Fw.off.visible = !(st.prong && hasP); if (Fw.on) Fw.on.visible = st.prong && hasP; FKp.std.visible = !(st.prong && hasP); FKp.pr.visible = st.prong && hasP;
    XK.visible = FK.visible = st.kit; xNo.on = st.prong; for (const l of xl) l.on = st.cut && u > .97 && u < 1.03;
    if (rod) rod.visible = st.cut;
    for (const P of [Xp, F]) { P.clip.length = 0; if (st.cut) P.clip.push(P === F ? planeD(9.9) : planeD(5.5)); for (const m of Object.values(P.parts)) m.userData.cap.visible = st.cut && m.visible; }
    q('x-u').value = u;
  };
  const ctls = [Xp, F].map((P) => { const c = new OrbitControls(cam, P.r.domElement); c.enableDamping = true; return c; });
  ctls.forEach((c, i) => c.addEventListener('change', () => { const o = ctls[1 - i]; if (!o.target.equals(c.target)) o.target.copy(c.target); }));
  const view = (k) => { const [p, t] = VIEWS[k](); cam.position.copy(p); ctls.forEach((c) => { c.target.copy(t); c.update(); }); };
  const resize = () => { cam.aspect = Xp.el.clientWidth / Xp.el.clientHeight; cam.updateProjectionMatrix(); }; new ResizeObserver(resize).observe(Xp.el); resize();
  let vis = true; new IntersectionObserver((es) => { vis = es[es.length - 1].isIntersecting; }, { rootMargin: '200px' }).observe(q('xsec'));
  Xp.r.setAnimationLoop((t) => {
    const dt = st.last ? (t - st.last) / 1000 : 0; st.last = t;
    if (st.play) { st.u = Math.min(2, st.u + dt / 7); if (st.u >= 2) st.play = false; apply(); }
    if (!vis) return; ctls.forEach((c) => c.update()); Xp.render(); F.render();
  });
  const row = q('x-c1'), row2 = q('x-c2');
  const tog = (label, on, fn) => { const b = document.createElement('button'); b.className = 'b' + (on ? ' on' : ''); b.textContent = label; row.appendChild(b); b.onclick = () => { on = !on; b.classList.toggle('on', on); fn(on); }; };
  tog('Kit on', true, (o) => { st.kit = o; apply(); }); tog('Optional Ø8 prongs', false, (o) => { st.prong = o; apply(); }); tog('Cutaway', false, (o) => { st.cut = o; apply(); });
  const pb = document.createElement('button'); pb.className = 'b'; pb.textContent = '▶ Play connect + release'; pb.onclick = () => { st.u = 0; st.play = true; }; row2.appendChild(pb);
  const sl = q('x-u'); sl.oninput = () => { st.u = +sl.value; st.play = false; apply(); }; row2.appendChild(sl);
  for (const [k, label] of [['p34', '3/4'], ['lead', 'Leading edge'], ['parent', 'Parent side'], ['child', 'Child side'], ['top', 'Top'], ['low34', 'Low 3/4'], ['mouth', 'Mouth close-up']]) {
    const b = document.createElement('button'); b.className = 'b'; b.textContent = label; b.onclick = () => view(k); row2.appendChild(b); }
  apply(); view('p34');
  const api = { set: (o) => { Object.assign(st, o); apply(); if (o.view) view(o.view); }, view, Xp, F }; window.__x = api; return api;
}
