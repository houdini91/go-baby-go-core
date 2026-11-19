// ROUND 5 (real solids, not lofts): the wing-side parts built by viewer/r5mesh/gen_r5.py with manifold3d and packed by pack.py.
//   wing_r5      original wing - 40 mm nose notch (z 207-247) - tongue slot (14.1 wide) - catch groove
//   nosearm_L/S  carrier: the nose (the wing's own front edge, 0.3 hairline seams) + tongue + arm (pad drop 44 / 22) + tenon
//   button_r5    latch A slider (12 x 24 stadium cap, flush, the catch tooth on its bar); sleeve_r5 = the orange tell sleeve
//   cap_r5       blank cap for the notch when the kit is off
// Left side, LOW (pad below the mount). Depth piece / pad / clip ring / cushion are today's v1.1 meshes, moved up to the new pad height.
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
const V = (x, y, z) => new THREE.Vector3(x, y, z);
// wing-tip frame (gbg_sidepad_v1.fs): Q(d, t, z); d across the tip (+ outboard), t into the wing (- = front)
const P0 = [131.3, 80.7], AXH = [0.79577626, 0.60559074], TH = [-0.6083053, 0.79370313], SHX = (244.25 - 227.8) * 0.8 / 32.9, SHY = (244.25 - 227.8) * 2.7 / 32.9;
const Q = (d, t, z) => V(P0[0] + (d + 2) * AXH[0] + t * TH[0] + SHX, P0[1] + (d + 2) * AXH[1] + t * TH[1] + SHY, z);
// the blade leans: d' = d - 0.0565 (z - 227), t' = t - 0.069 (z - 227); the generator works in (d', t', z)
const QS = (d, t, z) => Q(d + 0.0565 * (z - 227), t + 0.069 * (z - 227), z);
const dirD = V(AXH[0], AXH[1], 0), dirT = V(TH[0], TH[1], 0);
const C = 227;                                   // tongue centre (the one mount)
const PAD_DZ = { L: 47.5 - 44, S: 47.5 - 22 };   // v1.1 pad meshes sit at drop 47.5: move them up to drop 44 / 22
const BTN = { d: 24.6, t: -6.3 };                // button face centre (S frame) on the nose's outer face
export function mountDraft(el, geo) {
  const need = ['wing_r5', 'nosearm_L', 'nosearm_S', 'button_r5', 'sleeve_r5', 'cap_r5'];
  const miss = need.filter((k) => !geo[k]);
  if (miss.length) throw new Error('missing Round 5 meshes ' + miss.join(', ') + ' (run python3 pack.py in viewer/)');
  const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true }); renderer.setPixelRatio(Math.min(devicePixelRatio, 2)); el.appendChild(renderer.domElement);
  const scene = new THREE.Scene(); const cam = new THREE.PerspectiveCamera(30, 1, 1, 4000); cam.up.set(0, 0, 1);
  const ctl = new OrbitControls(cam, renderer.domElement); ctl.enableDamping = true;
  scene.add(new THREE.HemisphereLight(0xffffff, 0x80868c, 1.5)); const dl = new THREE.DirectionalLight(0xffffff, 1.8); cam.add(dl); dl.position.set(80, 160, 200); scene.add(cam);
  const M = (c, o = {}) => new THREE.MeshStandardMaterial({ color: c, roughness: .58, ...o });
  const chairM = M(0xd8d4cc), kitM = M(0xf1f2f3), hlM = M(0xb9d3f0), btnM = M(0xff6a2a, { roughness: .45 }), sleeveM = M(0xff6a2a, { roughness: .45 }), cushM = M(0x22304d);
  // ---------- NOW (v1.1) ----------
  const now = new THREE.Group();
  for (const k of ['wing', 'armrest']) now.add(new THREE.Mesh(geo[k], chairM));
  for (const k of ['carrier', 'blank1', 'blank2', 'depth', 'pad', 'ring']) now.add(new THREE.Mesh(geo[k], kitM));
  now.add(new THREE.Mesh(geo.button, btnM)); now.add(new THREE.Mesh(geo.cushion, cushM));
  // ---------- ROUND 5 ----------
  const dr = new THREE.Group();
  dr.add(new THREE.Mesh(geo.wing_r5, chairM));
  dr.add(new THREE.Mesh(geo.armrest, chairM));
  const kitG = new THREE.Group(); dr.add(kitG);          // kit on
  const noseL = new THREE.Mesh(geo.nosearm_L, chairM), noseS = new THREE.Mesh(geo.nosearm_S, chairM);
  kitG.add(noseL, noseS, new THREE.Mesh(geo.button_r5, btnM), new THREE.Mesh(geo.sleeve_r5, sleeveM));
  const padG = new THREE.Group(); kitG.add(padG);
  for (const k of ['depth', 'pad', 'ring']) padG.add(new THREE.Mesh(geo[k], kitM));
  padG.add(new THREE.Mesh(geo.cushion, cushM));
  const capMesh = new THREE.Mesh(geo.cap_r5, chairM); dr.add(capMesh);    // kit off + blank cap
  // pinch arrows: thumb on the button (in, -d), fingers on the nose's inner face (out, +d), then pull the arm forward (-t)
  const arrows = new THREE.Group(); dr.add(arrows);
  const pb = QS(BTN.d, BTN.t, C);
  arrows.add(new THREE.ArrowHelper(dirD.clone().negate(), pb.clone().add(dirD.clone().multiplyScalar(40)), 36, 0xff6a2a, 9, 6));
  arrows.add(new THREE.ArrowHelper(dirD.clone(), QS(-8, BTN.t, C).add(dirD.clone().multiplyScalar(-40)), 36, 0x2a6df4, 9, 6));
  arrows.add(new THREE.ArrowHelper(dirT.clone().negate(), QS(9, -16, C + 26), 30, 0x444b55, 7, 5));
  let highlight = false, kitState = 'on', arm = 'L';
  function apply() {
    const on = kitState === 'on';
    kitG.visible = on; capMesh.visible = kitState === 'off-cap'; arrows.visible = arrows.userData.want !== false && on;
    noseL.visible = arm === 'L'; noseS.visible = arm === 'S';
    padG.position.set(0, 0, PAD_DZ[arm]);
    noseL.material = noseS.material = capMesh.material = highlight ? hlM : chairM;
  }
  scene.add(dr); scene.add(now); now.visible = false;
  const views = {
    outer: () => [Q(10, -6, C + 10).add(dirD.clone().multiplyScalar(170)).add(dirT.clone().multiplyScalar(-70)).add(V(0, 0, 40)), Q(10, -6, C + 10)],
    front: () => [Q(10, -6, C).add(dirT.clone().multiplyScalar(-230)).add(V(0, 0, 60)), Q(10, -6, C + 10)],
    parent: () => [Q(10, -6, C).add(V(150, -230, 230)), Q(10, -6, C)],
    whole: () => [V(110, 40, 230).add(V(330, -400, 220)), V(110, 40, 245)],
    child: () => [V(100, 30, 225).add(V(-260, -120, 70)), V(100, 30, 225)]
  };
  function view(k) { const [p, t] = views[k](); cam.position.copy(p); ctl.target.copy(t); }
  function resize() { const w = el.clientWidth, h = el.clientHeight; renderer.setSize(w, h, false); cam.aspect = w / h; cam.updateProjectionMatrix(); }
  const ro = new ResizeObserver(resize); ro.observe(el); resize(); view('parent');
  renderer.setAnimationLoop(() => { ctl.update(); renderer.render(scene, cam); });
  // arm length toggle (index.html has no button for it: add one next to "Highlight kit parts")
  const hlBtn = document.getElementById('dhl');
  if (hlBtn && !document.getElementById('darmL')) {
    const mk = (id, txt, a) => { const b = document.createElement('button'); b.className = 'b' + (a === arm ? ' on' : ''); b.id = id; b.textContent = txt; b.dataset.arm = a; return b; };
    const bl = mk('darmL', 'Long arm (drop 44)', 'L'), bs = mk('darmS', 'Short arm (drop 22)', 'S');
    const sp = document.createElement('span'); sp.style.width = '12px';
    hlBtn.after(sp, bl, bs);
    for (const b of [bl, bs]) b.onclick = () => { arm = b.dataset.arm; bl.classList.toggle('on', arm === 'L'); bs.classList.toggle('on', arm === 'S'); apply(); };
  }
  apply();
  return {
    view,
    setMode() { apply(); },                       // only the single 40 mm mount exists now
    highlight(v) { highlight = v; apply(); },
    kit(v) { kitState = v; apply(); },
    arm(v) { arm = v === 'S' ? 'S' : 'L'; apply(); },
    show(which) { dr.visible = which === 'draft'; now.visible = which === 'now'; },
    arrows(v) { arrows.userData.want = v; apply(); }
  };
}
