// Button option MOCKUPS. The arm surface around the button is taken from the real Onshape mesh (sampled; a cubic fit bridges
// the old button), and each option is a smooth height + colour map on that patch. Look and proportion only, not CAD.
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { computeBoundsTree, acceleratedRaycast } from 'three-mesh-bvh';
THREE.BufferGeometry.prototype.computeBoundsTree = computeBoundsTree;
const V = (x, y, z) => new THREE.Vector3(x, y, z);
const T = V(-0.6083053, 0.79370313, 0).normalize();            // press axis (+t, into the wing)
const O = V(162.11, 63.06, 226.92);                              // current cap centre (measured)
const N = V(0.988, 0.145, 0.061).normalize();                    // arm surface normal there (outboard)
const A = T.clone().sub(N.clone().multiplyScalar(T.dot(N))).normalize();   // along the arm, toward the wing
const U = N.clone().cross(A).normalize();                        // up
const R = 12.5;                                                  // sampled patch radius (mm)
const ss = (e0, e1, x) => { const t = Math.min(1, Math.max(0, (x - e0) / (e1 - e0))); return t * t * t * (t * (6 * t - 15) + 10); };
const at = (a, u, h) => O.clone().add(A.clone().multiplyScalar(a)).add(U.clone().multiplyScalar(u)).add(N.clone().multiplyScalar(h));
// signed distance to a stadium w (along a) x h (along u), centred at (ca, cu)
function sdStadium(a, u, w, h, ca = 0, cu = 0) {
  a -= ca; u -= cu; const r = Math.min(w, h) / 2;
  if (h >= w) { const qy = Math.max(Math.abs(u) - (h / 2 - r), 0); return Math.hypot(a, qy) - r; }
  const qx = Math.max(Math.abs(a) - (w / 2 - r), 0); return Math.hypot(qx, u) - r;
}
const mono = (a, u) => [1, a, u, a * a, a * u, u * u, a * a * a, a * a * u, a * u * u, u * u * u];
function solve(M, b) {
  const n = b.length;
  for (let i = 0; i < n; i++) {
    let p = i; for (let k = i + 1; k < n; k++) if (Math.abs(M[k][i]) > Math.abs(M[p][i])) p = k;
    [M[i], M[p]] = [M[p], M[i]]; [b[i], b[p]] = [b[p], b[i]];
    for (let k = i + 1; k < n; k++) { const f = M[k][i] / M[i][i]; for (let j = i; j < n; j++) M[k][j] -= f * M[i][j]; b[k] -= f * b[i]; }
  }
  const x = Array(n).fill(0); for (let i = n - 1; i >= 0; i--) { let s = b[i]; for (let j = i + 1; j < n; j++) s -= M[i][j] * x[j]; x[i] = s / M[i][i]; }
  return x;
}
function fitSurface(meshes) {
  const rc = new THREE.Raycaster(); let pts = [];
  for (let a = -R - 1; a <= R + 1; a += 1) for (let u = -R - 1; u <= R + 1; u += 1) {
    if (a * a + u * u > (R + 1.5) ** 2) continue;
    rc.set(at(a, u, 15), N.clone().negate());
    const h = rc.intersectObjects(meshes, false)[0]; if (h) { const hh = 15 - h.distance; if (Math.abs(hh) < 7) pts.push([a, u, hh]); }
  }
  let c; const f = (a, u) => mono(a / 10, u / 10).reduce((s, m, i) => s + m * c[i], 0);
  for (let it = 0; it < 4; it++) {
    const M = Array.from({ length: 10 }, () => Array(10).fill(0)), b = Array(10).fill(0);
    for (const [a, u, h] of pts) { const m = mono(a / 10, u / 10); for (let i = 0; i < 10; i++) { b[i] += m[i] * h; for (let j = 0; j < 10; j++) M[i][j] += m[i] * m[j]; } }
    c = solve(M, b);
    pts = pts.filter(([a, u, h]) => Math.abs(h - f(a, u)) < (it < 2 ? 0.6 : 0.25));    // drop rays that fell into the old gap
  }
  return f;
}
// ---------- the options: (a, u) -> { d: height offset from the arm surface, c: 0 arm / 1 cap / 2 gap / 3 arm (reshaped) } ----------
function options(S) {
  const gapD = 2.2, G = 0.3;
  const round = (sd, eb) => (sd > -eb ? -eb + Math.sqrt(Math.max(0, eb * eb - (sd + eb) ** 2)) : 0);   // quarter-round edge break
  const ring = (sd, eb) => { if (sd < 0) return { d: round(sd, eb), c: 1 }; if (sd < G) return { d: -gapD, c: 2 }; const x = sd - G; return { d: x < eb ? round(-x, eb) : 0, c: 0 }; };
  const o = {};
  o.pill = (a, u) => ring(sdStadium(a, u, 12, 9.5), 0.4);
  o.grip = (a, u) => {
    const sd = sdStadium(a, u, 12, 9.5); const r = ring(sd, 0.4);
    if (r.c === 1) for (const k of [-2.2, 0, 2.2]) { const x = Math.abs(a - k); if (x < 0.45) r.d -= 0.4 * Math.cos(x / 0.45 * Math.PI / 2) ** 2 * ss(0, 1.2, -sd); }
    return r;
  };
  // angled landing: floor plane through the centre turned 20 deg toward the push; C2 blend 3 mm wide around a 12 x 15 outline
  const ang = 20 * Math.PI / 180;
  const nL = N.clone().multiplyScalar(Math.cos(ang)).add(A.clone().multiplyScalar(-Math.sin(ang)));
  const pl = (a) => -(nL.dot(A) * a) / nL.dot(N);
  o.landing = (a, u) => {
    const base = S(a, u) - S(0, 0); const sdL = sdStadium(a, u, 12, 15); const w = 1 - ss(-0.5, 2.5, sdL);
    const r = ring(sdStadium(a, u, 8, 11), 0.6); r.d += (pl(a) - base) * w; if (r.c === 0) r.c = sdL < 0 ? 3 : 0; return r;
  };
  o.scoop = (a, u) => {
    const ea = (a + 1.5) / 8, eu = u / 7, r2 = ea * ea + eu * eu; const D = 2.6 + 0.9 * ss(-8, 6, -a);
    const r = ring(sdStadium(a, u, 10, 8, -1, 0), 0.4); r.d += r2 < 1 ? -D * (1 - r2) ** 2 : 0;
    if (r.c === 1) { const q = Math.hypot(a + 1, u); if (q < 0.8) r.d -= 0.35 * (1 - (q / 0.8) ** 2); }
    if (r.c === 0 && r2 < 1) r.c = 3; return r;
  };
  o.dome = (a, u) => {
    const sd = sdStadium(a, u, 11, 8); const r = ring(sd, 0.35);
    if (r.c === 1) { const e = (a / 5.5) ** 2 + (u / 4) ** 2; r.d += 1.0 * Math.max(0, 1 - e) ** 0.8 + 0.25 * ss(0, 1, -sd); }
    return r;
  };
  return o;
}
export function mountMock(el, geo) {
  const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true }); renderer.setPixelRatio(Math.min(devicePixelRatio, 2)); el.appendChild(renderer.domElement);
  const scene = new THREE.Scene(); const cam = new THREE.PerspectiveCamera(22, 1, 1, 3000); cam.up.set(0, 0, 1);
  const ctl = new OrbitControls(cam, renderer.domElement); ctl.enableDamping = true;
  scene.add(new THREE.HemisphereLight(0xffffff, 0x80868c, 1.45)); const dl = new THREE.DirectionalLight(0xffffff, 2.0); cam.add(dl); dl.position.set(60, 140, 120); scene.add(cam);
  const armM = new THREE.MeshStandardMaterial({ color: 0xf1f2f3, roughness: .55 });
  scene.add(new THREE.Mesh(geo.wing, new THREE.MeshStandardMaterial({ color: 0xd8d4cc, roughness: .7, transparent: true, opacity: .55, depthWrite: false })));
  // private copies, so the BVH does not touch the main viewer's geometry
  const carM = new THREE.Mesh(geo.carrier.clone(), armM), btnM = new THREE.Mesh(geo.button.clone(), new THREE.MeshStandardMaterial({ color: 0xff6a2a, roughness: .5 }));
  for (const m of [carM, btnM]) { m.updateMatrixWorld(); m.geometry.computeBoundsTree(); m.raycast = acceleratedRaycast; }
  const Sfit = fitSurface([carM, btnM]);
  // the real surface on a 0.25 mm grid (NaN off the arm); the fit stands in only over the old button (r < 7.5, blended out by r 10)
  const gs = 0.25, gn = Math.round(2 * R / gs) + 1, act = new Float32Array(gn * gn).fill(NaN); const rc0 = new THREE.Raycaster();
  for (let j = 0; j < gn; j++) for (let i = 0; i < gn; i++) {
    const a = -R + i * gs, u = -R + j * gs; if (a * a + u * u > (R + 1) ** 2) continue;
    rc0.set(at(a, u, 15), N.clone().negate());
    const h = rc0.intersectObjects([carM, btnM], false)[0]; if (h) { const hh = 15 - h.distance; if (Math.abs(hh - Sfit(a, u)) < 2.5 || a * a + u * u < 64) act[j * gn + i] = hh; }
  }
  const actual = (a, u) => {
    const x = (a + R) / gs, y = (u + R) / gs, i = Math.min(gn - 2, Math.max(0, Math.floor(x))), j = Math.min(gn - 2, Math.max(0, Math.floor(y))), fx = x - i, fy = y - j;
    const v = (ii, jj) => act[jj * gn + ii]; return (v(i, j) * (1 - fx) + v(i + 1, j) * fx) * (1 - fy) + (v(i, j + 1) * (1 - fx) + v(i + 1, j + 1) * fx) * fy;
  };
  const S = (a, u) => { const w = ss(7.5, 10, Math.hypot(a, u)); return w === 0 ? Sfit(a, u) : Sfit(a, u) * (1 - w) + actual(a, u) * w; };
  // the arm with a hole where the patch goes (near-side triangles only)
  const src = carM.geometry.attributes.position, nrm = carM.geometry.attributes.normal, ix = carM.geometry.index; const keepP = [], keepN = [];
  const vi = k => (ix ? ix.getX(k) : k);
  for (let t = 0; t < (ix ? ix.count : src.count); t += 3) {
    let ca = 0, cu = 0, cn = 0;
    for (let k = 0; k < 3; k++) { const p = V(src.getX(vi(t + k)), src.getY(vi(t + k)), src.getZ(vi(t + k))).sub(O); ca += p.dot(A) / 3; cu += p.dot(U) / 3; cn += p.dot(N) / 3; }
    if ((ca / 9.4) ** 2 + (cu / 10.3) ** 2 < 1 && Math.abs(cn - S(ca, cu)) < 1.5) continue;
    for (let k = 0; k < 3; k++) { const q = vi(t + k); keepP.push(src.getX(q), src.getY(q), src.getZ(q)); keepN.push(nrm.getX(q), nrm.getY(q), nrm.getZ(q)); }
  }
  const holed = new THREE.BufferGeometry(); holed.setAttribute('position', new THREE.Float32BufferAttribute(keepP, 3)); holed.setAttribute('normal', new THREE.Float32BufferAttribute(keepN, 3));
  const holedM = new THREE.Mesh(holed, new THREE.MeshStandardMaterial({ color: 0xf1f2f3, roughness: .55, polygonOffset: true, polygonOffsetFactor: 3, polygonOffsetUnits: 3 }));
  // the patch: a fine grid over an ellipse a bit larger than the hole
  const step = 0.08, n = Math.round(2 * R / step) + 1;
  const pos = new Float32Array(n * n * 3), colA = new Float32Array(n * n * 3); const idx = [];
  const geoP = new THREE.BufferGeometry(); geoP.setAttribute('position', new THREE.BufferAttribute(pos, 3)); geoP.setAttribute('color', new THREE.BufferAttribute(colA, 3));
  for (let j = 0; j < n - 1; j++) for (let i = 0; i < n - 1; i++) {
    const a = -R + i * step, u = -R + j * step;
    if (((a + step / 2) / 10.0) ** 2 + ((u + step / 2) / 11.0) ** 2 > 1) continue;
    if (![[a, u], [a + step, u], [a, u + step], [a + step, u + step]].every(([x, y]) => Number.isFinite(S(x, y)))) continue;
    const k = j * n + i; idx.push(k, k + 1, k + n, k + 1, k + n + 1, k + n);
  }
  geoP.setIndex(idx);
  const patchM = new THREE.Mesh(geoP, new THREE.MeshStandardMaterial({ vertexColors: true, roughness: .55 }));
  const OPT = options(S);
  const COL = [new THREE.Color(0xf1f2f3), new THREE.Color(0xff6a2a), new THREE.Color(0x24272b), new THREE.Color(0xf1f2f3)];
  let capColour = true;
  function build(k) {
    const f = OPT[k];
    for (let j = 0; j < n; j++) for (let i = 0; i < n; i++) {
      const a = -R + i * step, u = -R + j * step; const r = f(a, u); const s0 = S(a, u);
      const p = at(a, u, (Number.isFinite(s0) ? s0 : 0) + r.d + 0.01);
      const q = (j * n + i) * 3; pos[q] = p.x; pos[q + 1] = p.y; pos[q + 2] = p.z;
      const cc = COL[r.c === 1 && !capColour ? 0 : r.c]; colA[q] = cc.r; colA[q + 1] = cc.g; colA[q + 2] = cc.b;
    }
    geoP.attributes.position.needsUpdate = true; geoP.attributes.color.needsUpdate = true; geoP.computeVertexNormals();
  }
  const g = new THREE.Group(); scene.add(g);
  function show(k) {
    g.clear();
    if (k === 'current') { btnM.material.color.set(capColour ? 0xff6a2a : 0xf1f2f3); g.add(carM, btnM); }
    else { build(k); g.add(holedM, patchM); }
  }
  const views = {
    face: () => O.clone().add(N.clone().multiplyScalar(85)).add(U.clone().multiplyScalar(5)),
    parent: () => O.clone().add(V(55, -70, 75)),
    thumb: () => O.clone().add(T.clone().multiplyScalar(-80)).add(N.clone().multiplyScalar(18)).add(V(0, 0, 14)),
    side: () => O.clone().add(U.clone().multiplyScalar(85)).add(N.clone().multiplyScalar(12)).add(A.clone().multiplyScalar(-10))
  };
  function view(k) { cam.position.copy(views[k]()); ctl.target.copy(O); }
  function resize() { const w = el.clientWidth, h = el.clientHeight; renderer.setSize(w, h, false); cam.aspect = w / h; cam.updateProjectionMatrix(); }
  const ro = new ResizeObserver(resize); ro.observe(el); resize(); view('face'); show('current');
  renderer.setAnimationLoop(() => { ctl.update(); renderer.render(scene, cam); });
  return { show, view, setColour(v) { capColour = v; } };
}
