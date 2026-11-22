// X, inside, the best version: one large pane with every internal part (gen_xi.py), exploded view, section planes,
// see-through wing, and the connect / release animation driving the internals (cam bosses, hook, catch, rod, button, springs).
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { QS, S2W, dirD, dirT, V, COL, M } from './exh.js';
import { pane, seg, W2S } from './h2h.js';

export function mountXI(geo, J) {
  const q = (id) => document.getElementById(id);
  const X = J.XI; const TP = X.pivot_S[0], ZP = X.pivot_S[1], PHI0 = -X.start_tilt_deg, SC = X.cam_push_mm;
  const cam = new THREE.PerspectiveCamera(30, 1, 1, 5000); cam.up.set(0, 0, 1);
  const P = pane(q('xi-v'), cam, geo, { cut: new THREE.Plane(new THREE.Vector3(0, 0, -1), 1e6) });
  P.scene.add(new THREE.Mesh(geo.armrest, M(COL.chair)));
  const wing = P.add('xi_wing', COL.chair, P.scene);
  const grp = (parent) => { const g = new THREE.Group(); g.matrixAutoUpdate = false; (parent || P.scene).add(g); return g; };
  const KIT = grp();                                         // moves with the arm (pose); children get extra local offsets
  const G = {};
  G.kit = grp(KIT); P.add('xi_kit', COL.kit, G.kit, { cap: 0x2a6df4 });
  for (const [k, c] of [['depth', COL.kit], ['pad', COL.pad], ['ring', 0x5e6a77], ['cover', COL.foam]]) P.add(k, c, G.kit);
  const parts = [['catch', 'xi_catch', 0x2ea043], ['cspring', 'xi_catch_spring', 0x9aa4ad], ['rod', 'xi_rod', 0x6b7785], ['button', 'xi_button', COL.btn], ['tell', 'xi_tell', 0xff8a3d], ['rspring', 'xi_ret_spring', 0x9aa4ad]];
  for (const [k, n, c] of parts) { G[k] = grp(KIT); P.add(n, c, G[k], { metalness: k.includes('spring') || k === 'rod' ? .6 : 0, cap: 0x2a6df4 }); }
  // rotation arrow about the pitch axis (parent side)
  const arc = new THREE.Group(); arc.matrixAutoUpdate = false; arc.matrix.copy(S2W); P.scene.add(arc);
  const pts = []; for (let i = 0; i <= 30; i++) { const th = (-122 + 30 * i / 30) * Math.PI / 180; pts.push(V(26, TP + 70 * Math.cos(th), ZP + 70 * Math.sin(th))); }
  arc.add(new THREE.Mesh(new THREE.TubeGeometry(new THREE.CatmullRomCurve3(pts), 40, 1.6, 10), new THREE.MeshBasicMaterial({ color: 0xff3b30 })));
  const head = new THREE.Mesh(new THREE.ConeGeometry(4.5, 10, 20), new THREE.MeshBasicMaterial({ color: 0xff3b30 })); arc.add(head);
  const setArrow = (dir) => { const i = dir > 0 ? pts.length - 1 : 0, j = dir > 0 ? i - 1 : 1; head.position.copy(pts[i]); head.quaternion.setFromUnitVectors(V(0, 1, 0), pts[i].clone().sub(pts[j]).normalize()); };
  // labels (S frame, seated pose; they follow the kit)
  const LB = (txt, d, t, z, parent) => { const l = P.label(txt, V(d, t, z), 'mu'); l.obj = parent || null; l.local = true; return l; };
  const B = X.button_S, R = X.rod, RS = X.ret_spring;
  const labs = {
    assembled: [['ledge (pull-out)', 6, 4, 304], ['beak hook', 6, 15, 300], ['guide wedges + seat', 6, 8, 286], ['cam boss in groove', -1, 2, 250], ['catch + notch', 6, 6, 208],
      ['rod', 6, -4, 214], ['button + tell', B[0], B[1], B[2] + 10], ['return spring', 6, 6, 262], ['pitch axis', 6, TP - 6, ZP + 10]],
    exploded: [['catch (steel-spring)', 6, 5, 186], ['catch spring', 6, 5, 202], ['rod Ø4', 6, -30, 214], ['button', B[0] + 30, B[1], B[2] + 10], ['tell ring', B[0] + 18, B[1], B[2] - 12], ['return spring', 6, 40, 262], ['plug + arm (real r5cad arm)', 6, -70, 300]]
  };
  const labObjs = {};
  for (const k in labs) labObjs[k] = labs[k].map(([t, d, tt, z]) => { const l = P.label(t, new THREE.Vector3(), 'mu'); l.S = [d, tt, z]; l.on = false; return l; });
  const EXP = { kit: [0, -70, 0], catch: [0, 0, -28], cspring: [0, 0, -14], rod: [0, -26, 0], button: [30, 0, 0], tell: [18, 0, 0], rspring: [0, 34, 0] };
  const st = { kit: true, mode: 'assembled', wing: 'solid', sec: 'none', u: 0.999, play: false, last: 0 };
  const T = [[0, .5, 'Tilt the arm 15° nose-up and push the plug in: the mouth and throat centre it; the catch rides up on the floor (tell ring shows).'],
    [.5, .9, 'The cam bosses enter the plug\'s grooves: the last 12 mm of push turns it 15° nose-down (red arrow); the beak rises behind the ledge, the rear seats on the wedges, the return spring loads.'],
    [.9, 1, 'CLICK: the catch drops 3 mm into its notch (sharp drop-off, hard stop); the button comes out flush and hides the orange tell.'],
    [1, 1.1, 'Press the button: its 45° cam pushes the rod back, the rod\'s 45° tip lifts the catch.'],
    [1.1, 1.35, '…the preloaded return spring turns it back 15° along the grooves and nudges it 3 mm: free.'], [1.35, 2.01, 'Pull it out.']];
  const txt = (u) => (T.find(([a, b]) => u >= a && u < b) || T[T.length - 1])[2];
  const pose = (phi, s) => {
    const Rm = new THREE.Matrix4().makeTranslation(0, TP, ZP).multiply(new THREE.Matrix4().makeRotationX(phi * Math.PI / 180)).multiply(new THREE.Matrix4().makeTranslation(0, -TP, -ZP));
    const ax = new THREE.Vector3(0, -1, 0).applyMatrix4(new THREE.Matrix4().makeRotationX(phi * Math.PI / 180)).multiplyScalar(s);
    return new THREE.Matrix4().makeTranslation(ax.x, ax.y, ax.z).multiply(Rm);
  };
  const setG = (g, A) => { g.matrix.copy(A); g.matrixWorldNeedsUpdate = true; };
  const planes = {
    none: null,
    horiz: () => new THREE.Plane(new THREE.Vector3(0, 0, -1), 254),
    hook: () => planeS(0, true, 6.0),          // keep d' < 6 (mid-plug): hook, ledge, catch, rod, return spring
    rails: () => planeS(0, true, -0.25),       // keep d' < -0.25: through the inner wall's cam boss and groove
    catch: () => planeS(1, false, 5.0)         // keep t' > 5: across the catch, seen from the front
  };
  function planeS(axis, keepLess, c) {         // S coordinate s(p) = row . p + e12 (from W2S); keep s < c or s > c
    const e = W2S.elements; const n = new THREE.Vector3(e[axis], e[axis + 4], e[axis + 8]); const k = n.length();
    return keepLess ? new THREE.Plane(n.clone().multiplyScalar(-1 / k), (c - e[axis + 12]) / k) : new THREE.Plane(n.clone().multiplyScalar(1 / k), (e[axis + 12] - c) / k);
  }
  const apply = () => {
    const u = st.u; let phi, s, c;                           // c: catch lift (0 locked .. 3 up)
    if (u < 1) { phi = PHI0 * (1 - seg(u, .5, .9)); s = SC + (70 - SC) * (1 - seg(u, 0, .5)) - SC * seg(u, .5, .9); c = u < .25 ? 3 * seg(u, .1, .25) : u < .93 ? 3 : 3 * (1 - seg(u, .93, .96)); if (u < .1) c = 0; }
    else if (u < 1.35) { const k = seg(u, 1.1, 1.35); phi = PHI0 * k; s = 3 * k; c = 3 * seg(u, 1, 1.06); }
    else { phi = PHI0; s = 3 + 67 * seg(u, 1.35, 2); c = 3 * (1 - seg(u, 1.9, 2)); }
    const A = pose(phi, s);
    const ex = st.mode === 'exploded';
    KIT.matrix.copy(S2W).multiply(ex ? new THREE.Matrix4() : A).multiply(W2S); KIT.matrixWorldNeedsUpdate = true;
    const off = (k, extra) => { const e = ex ? EXP[k] : [0, 0, 0]; const v = extra || [0, 0, 0]; return new THREE.Matrix4().makeTranslation(e[0] + v[0], e[1] + v[1], e[2] + v[2]); };
    const local = (M4) => S2W.clone().multiply(M4).multiply(W2S);
    setG(G.kit, local(off('kit')));
    setG(G.catch, local(off('catch', [0, 0, ex ? 0 : c])));
    // catch spring squashes as the catch rises (scale z about its base under the catch's top)
    setG(G.cspring, local(off('cspring').multiply(new THREE.Matrix4().makeTranslation(0, 0, 214 + 9.0)).multiply(new THREE.Matrix4().makeScale(1, 1, Math.max(.35, 1 - (ex ? 0 : c) / 4.5 * .6))).multiply(new THREE.Matrix4().makeTranslation(0, 0, -(214 + 9.0)))));
    setG(G.rod, local(off('rod', [0, ex ? 0 : c, 0])));
    setG(G.button, local(off('button', [ex ? 0 : -c, 0, 0])));
    setG(G.tell, local(off('tell'))); G.tell.visible = ex || c > .5;
    // return spring: seated length when home, free (x1.35) once it has pushed the plug out
    const seated = u >= .88 && u < 1.12; const free = ex || !seated; const ks = free ? 1.35 : 1;
    setG(G.rspring, local(off('rspring').multiply(new THREE.Matrix4().makeTranslation(0, RS[1], 0)).multiply(new THREE.Matrix4().makeScale(1, ks, 1)).multiply(new THREE.Matrix4().makeTranslation(0, -RS[1], 0))));
    arc.visible = !ex && ((u > .5 && u < .92) || (u > 1.1 && u < 1.37)); setArrow(u < 1 ? 1 : -1);
    KIT.visible = st.kit;
    wing.visible = st.wing !== 'hidden'; wing.material.transparent = st.wing === 'ghost'; wing.material.opacity = st.wing === 'ghost' ? .22 : 1; wing.material.depthWrite = st.wing !== 'ghost'; wing.material.needsUpdate = true;
    P.clip.length = 0; const pl = planes[st.sec]; if (pl) P.clip.push(pl());
    for (const m of Object.values(P.parts)) m.userData.cap.visible = !!pl && m.visible && !(m === wing && st.wing === 'ghost');
    for (const k in labObjs) for (const l of labObjs[k]) {
      l.on = (k === 'exploded' ? ex : (!ex && (st.sec !== 'none' || st.wing !== 'solid') && u > .97 && u < 1.03));
      l.pos = new THREE.Vector3(...l.S).applyMatrix4(S2W);
    }
    q('xi-t').textContent = ex ? 'Exploded: the plug + real arm, the catch and its spring, the rod, the button, the tell ring and the return spring.' : txt(u);
    q('xi-u').value = u;
  };
  const ctl = new OrbitControls(cam, P.r.domElement); ctl.enableDamping = true;
  const T0 = QS(8, -10, 248);
  const VIEWS = {
    p34: () => [T0.clone().add(dirD.clone().multiplyScalar(270)).add(dirT.clone().multiplyScalar(-290)).add(V(0, 0, 160)), T0.clone().add(V(0, 0, -15))],
    parent: () => [T0.clone().add(dirD.clone().multiplyScalar(390)).add(V(0, 0, 15)), T0.clone().add(V(0, 0, -10))],
    top: () => [T0.clone().add(V(0, 0, 380)).add(dirT.clone().multiplyScalar(-30)), T0],
    lead: () => [T0.clone().add(dirT.clone().multiplyScalar(-400)).add(V(0, 0, 25)), T0],
    child: () => [T0.clone().add(dirD.clone().multiplyScalar(-400)).add(dirT.clone().multiplyScalar(-60)).add(V(0, 0, 25)), T0],
    low34: () => [T0.clone().add(dirD.clone().multiplyScalar(250)).add(dirT.clone().multiplyScalar(-240)).add(V(0, 0, -150)), T0],
    close: () => [T0.clone().add(dirD.clone().multiplyScalar(150)).add(dirT.clone().multiplyScalar(-90)).add(V(0, 0, 40)), T0.clone().add(V(0, 0, -5))]
  };
  const view = (k) => { const [p, t] = VIEWS[k](); cam.position.copy(p); ctl.target.copy(t); ctl.update(); };
  new ResizeObserver(() => { cam.aspect = P.el.clientWidth / P.el.clientHeight; cam.updateProjectionMatrix(); }).observe(P.el);
  cam.aspect = P.el.clientWidth / P.el.clientHeight; cam.updateProjectionMatrix();
  let vis = true; new IntersectionObserver((es) => { vis = es[es.length - 1].isIntersecting; }, { rootMargin: '200px' }).observe(q('xisec'));
  P.r.setAnimationLoop((t) => {
    const dt = st.last ? (t - st.last) / 1000 : 0; st.last = t;
    if (st.play) { st.u = Math.min(2, st.u + dt / 8); if (st.u >= 2) st.play = false; apply(); }
    if (!vis) return; ctl.update(); P.render();
  });
  const mk = (row, items, on, fn) => { const bs = items.map(([k, label]) => { const b = document.createElement('button'); b.className = 'b' + (k === on ? ' on' : ''); b.textContent = label; b.onclick = () => { bs.forEach((x) => x.classList.toggle('on', x === b)); fn(k); }; row.appendChild(b); return b; }); };
  mk(q('xi-c1'), [['on', 'Kit on'], ['off', 'Kit off']], 'on', (k) => { st.kit = k === 'on'; apply(); });
  mk(q('xi-c1'), [['assembled', 'Assembled'], ['exploded', 'Exploded']], 'assembled', (k) => { st.mode = k; apply(); });
  mk(q('xi-c1'), [['solid', 'Wing solid'], ['ghost', 'Wing see-through'], ['hidden', 'Wing hidden']], 'solid', (k) => { st.wing = k; apply(); });
  mk(q('xi-c2'), [['none', 'No section'], ['horiz', 'Section: level, mid-plug'], ['hook', 'Section: vertical, hook + catch'], ['rails', 'Section: through the cam bosses'], ['catch', 'Section: across the catch']], 'none', (k) => { st.sec = k; apply(); });
  const row3 = q('xi-c3');
  const pb = document.createElement('button'); pb.className = 'b'; pb.textContent = '▶ Play connect + release'; pb.onclick = () => { st.mode = 'assembled'; st.u = 0; st.play = true; }; row3.appendChild(pb);
  const sl = q('xi-u'); sl.oninput = () => { st.u = +sl.value; st.play = false; apply(); }; row3.appendChild(sl);
  for (const [k, label] of [['p34', '3/4'], ['close', 'Close-up'], ['parent', 'Parent side'], ['lead', 'Leading edge'], ['child', 'Child side'], ['top', 'Top'], ['low34', 'Low 3/4']]) { const b = document.createElement('button'); b.className = 'b'; b.textContent = label; b.onclick = () => view(k); row3.appendChild(b); }
  apply(); view('p34');
  const api = { set: (o) => { Object.assign(st, o); apply(); if (o.view) view(o.view); }, view, P }; window.__xi = api; return api;
}
