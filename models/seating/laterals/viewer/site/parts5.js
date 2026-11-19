// Round 5 part list + sketches for the GoBabyGo side-pad kit.
// Usage: import { mountParts } from './parts5.js'; mountParts(document.getElementById('parts5'));
// Data: ./parts5.json (next to this module). No external libraries. Every sketch is schematic but in proportion;
// sizes are mm, taken from ROUND5_PLAN.md and gbg_sidepad_v1.fs (v1.1).

const COL = { chair: '#d8d4cc', kit: '#f1f2f3', btn: '#ff6a2a', cf: '#26282b', st: '#8b939c', soft: '#22304d', on: '#2b2f33' };

const esc = (s) => String(s ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const n = (v) => Math.round(v * 10) / 10;

// ---------------- SVG helpers (px in the sketch's own viewBox) ----------------
const svg = (w, h, body, label) =>
  `<svg viewBox="0 0 ${w} ${h}" style="min-width:${Math.round(w * 0.85)}px" role="img" aria-label="${esc(label)}">${body}</svg>`;
const txt = (x, y, s, a = 'start', c = 't') => `<text x="${n(x)}" y="${n(y)}" text-anchor="${a}" class="${c}">${esc(s)}</text>`;
const bold = (x, y, s, a = 'start') => `<text x="${n(x)}" y="${n(y)}" text-anchor="${a}" class="t b">${esc(s)}</text>`;
const mute = (x, y, s, a = 'start') => txt(x, y, s, a, 'dt');
const ln = (x1, y1, x2, y2, c = 'dl') => `<line x1="${n(x1)}" y1="${n(y1)}" x2="${n(x2)}" y2="${n(y2)}" class="${c}"/>`;
const rect = (x, y, w, h, c, rx = 0) => `<rect x="${n(x)}" y="${n(y)}" width="${n(w)}" height="${n(h)}"${rx ? ` rx="${n(rx)}"` : ''} class="${c}"/>`;
const path = (d, c) => `<path d="${d}" class="${c}"/>`;
const circ = (x, y, r, c) => `<circle cx="${n(x)}" cy="${n(y)}" r="${n(r)}" class="${c}"/>`;
const poly = (pts, c) => path(pts.map((p, i) => (i ? 'L' : 'M') + n(p[0]) + ' ' + n(p[1])).join(' ') + ' Z', c);
function ah(x, y, dx, dy) {
  const m = Math.hypot(dx, dy) || 1, ux = dx / m, uy = dy / m, L = 6, W = 2.6, bx = x - ux * L, by = y - uy * L;
  return `<path class="ah" d="M${n(x)} ${n(y)} L${n(bx - uy * W)} ${n(by + ux * W)} L${n(bx + uy * W)} ${n(by - ux * W)} Z"/>`;
}
const dimH = (x1, x2, y, label, below = false) =>
  ln(x1, y, x2, y) + ah(x1, y, -1, 0) + ah(x2, y, 1, 0) + (label ? txt((x1 + x2) / 2, below ? y + 15 : y - 5, label, 'middle', 'dt') : '');
const dimV = (x, y1, y2, label, side = 'r') =>
  ln(x, y1, x, y2) + ah(x, y1, 0, -1) + ah(x, y2, 0, 1) + (label ? txt(side === 'r' ? x + 5 : x - 5, (y1 + y2) / 2 + 4, label, side === 'r' ? 'start' : 'end', 'dt') : '');
const lead = (px, py, tx, ty, label, a = 'start') =>
  circ(px, py, 1.8, 'dot') + ln(px, py, tx, ty, 'ld') + txt(a === 'start' ? tx + 3 : tx - 3, ty + 4, label, a);
function zig(x1, x2, yc, amp, segs, c) {
  let d = `M${n(x1)} ${n(yc - amp)} L${n(x1)} ${n(yc + amp)}`;
  for (let i = 1; i <= segs; i++) d += ` L${n(x1 + ((x2 - x1) * i) / segs)} ${n(i % 2 ? yc - amp : yc + amp)}`;
  d += ` L${n(x2)} ${n(yc - amp)}`;
  return path(d, c);
}
function nut(x, yc, sc) { // side view of an M5 nut (m 4, across corners ~9.2)
  const w = 4 * sc, h = 9.2 * sc;
  return rect(x, yc - h / 2, w, h, 'steel ol', 1.5) + ln(x, yc - h / 4, x + w, yc - h / 4, 'onl') + ln(x, yc + h / 4, x + w, yc + h / 4, 'onl');
}

// ---------------- sketches ----------------
const SK = {};

SK.wing = () => {
  const X = (h) => 40 + h * 1.6, Z = (z) => 34 + (300 - z) * 1.6;
  let b = mute(160, 18, 'outer side view, front left', 'middle');
  b += path(`M${X(0)} ${Z(295)} L${X(70)} ${Z(295)} L${X(70)} ${Z(185)} L${X(0)} ${Z(185)} L${X(0)} ${Z(207)} L${X(20)} ${Z(207)} L${X(20)} ${Z(247)} L${X(0)} ${Z(247)} Z`, 'chair ol');
  b += rect(X(0), Z(247), 20 * 1.6, 40 * 1.6, 'hid2');
  b += rect(X(20), Z(241), 31 * 1.6, 28 * 1.6, 'hid');
  b += rect(X(23.8), Z(234.8), 5.4 * 1.6, 15.6 * 1.6, 'acc-d');
  b += rect(X(57), Z(272), 4 * 1.6, 69 * 1.6, 'cut ol');
  b += ln(X(-2), Z(227), X(0), Z(227), 'ax') + txt(36, Z(227) + 4, 'z 227', 'end', 'dt');
  b += lead(X(61), Z(265), 164, 80, 'S1 slit: untouched');
  b += lead(X(10), Z(244), 164, 104, 'notch 40, z 207–247');
  b += lead(X(45), Z(238), 164, 128, 'tongue pocket 10–31');
  b += lead(X(54), Z(225), 164, 152, '6 web to S1');
  b += lead(X(26.5), Z(221), 164, 176, 'catch groove, 6° face');
  b += mute(16, 236, 'walls ≥ 6, back ≥ 8 around the pocket');
  b += mute(16, 253, 'tip block, rails and slot blanks removed');
  return svg(320, 262, b, 'Wing front edge with the 40 mm notch');
};

function carrierPanel(ox, drop, title, heights) {
  const s = 2.2, X = (h) => ox + 40 + (h + 55) * s, Z = (z) => 44 + (255 - z) * s, c = 227 - drop;
  let b = bold(ox + 14, 22, title);
  // arm (one loft, side silhouette), then the nose over it
  b += path(`M${n(X(0))} ${n(Z(241))} C${n(X(-22))} ${n(Z(241))} ${n(X(-22))} ${n(Z(c + 14))} ${n(X(-44))} ${n(Z(c + 14))} A${n(9 * s)} ${n(14 * s)} 0 0 0 ${n(X(-44))} ${n(Z(c - 14))} C${n(X(-22))} ${n(Z(c - 14))} ${n(X(-22))} ${n(Z(213))} ${n(X(0))} ${n(Z(213))} Z`, 'kit ol');
  b += path(`M${n(X(20))} ${n(Z(247))} L${n(X(6))} ${n(Z(247))} Q${n(X(0))} ${n(Z(247))} ${n(X(0))} ${n(Z(241))} L${n(X(0))} ${n(Z(213))} Q${n(X(0))} ${n(Z(207))} ${n(X(6))} ${n(Z(207))} L${n(X(20))} ${n(Z(207))} Z`, 'kit ol');
  b += rect(X(20), Z(241), 31 * s, 28 * s, 'hid2');
  b += txt(X(35.5), Z(231), 'tongue', 'middle', 'dt') + txt(X(35.5), Z(224), '≤31 deep', 'middle', 'dt');
  // carbon rod, tenon end-on with its 4 pins, button
  b += `<line x1="${n(X(40))}" y1="${n(Z(217))}" x2="${n(X(-15))}" y2="${n(Z(217))}" class="rod" stroke-width="${n(6 * s)}"/>`;
  b += rect(X(-44) - 6.5 * s, Z(c) - 10 * s, 13 * s, 20 * s, 'hid', 2.5 * s);
  for (const [dy, dz] of [[3.5, 7.4], [-3.5, 7.4], [3.5, -7.4], [-3.5, -7.4]]) b += circ(X(-44 + dy), Z(c + dz), s, 'cf');
  b += rect(X(5.5), Z(239), 12 * s, 24 * s, 'btn ol', 6 * s);
  // mount line + dims
  b += ln(X(-55), Z(227), X(0), Z(227), 'ax') + txt(X(-55), Z(227) - 4, 'z 227', 'start', 'dt');
  b += dimV(X(-56.5), Z(227), Z(c), String(drop), 'l');
  b += ln(X(0), Z(247), X(0), Z(247) - 12, 'ext') + ln(X(20), Z(247), X(20), Z(247) - 12, 'ext') + dimH(X(0), X(20), Z(247) - 8, '20');
  b += ln(X(20), Z(247), X(57), Z(247), 'ext') + ln(X(20), Z(207), X(57), Z(207), 'ext') + dimV(X(55), Z(247), Z(207), '40');
  // labels
  b += lead(X(-8), Z(217), X(-25), Z(250), 'Ø6 carbon ≈55', 'end');
  b += lead(X(11.5), Z(216), X(4), Z(199), 'button 12 × 24');
  const zb = (199 + c) / 2;
  b += lead(X(-22), Z(zb) + 3, X(-18), Z(174), 'arm 18 × 28 pill');
  b += lead(X(-44), Z(c - 10), X(-44), 246, '') + txt(ox + 14, 260, 'tenon 13 × 20 × 13 · 4 × Ø2 pins');
  b += txt(ox + 14, 283, `pad centre z ${heights.join(' · ')}`, 'start', 'dt');
  return b;
}
SK.carrier = (p) => {
  const hS = (p.heights && p.heights.S) || [194, 216, 238, 260], hL = (p.heights && p.heights.L) || [172, 194, 260, 282];
  let b = carrierPanel(0, 22, 'S · short arm · drop 22', hS) + carrierPanel(320, 44, 'L · long arm · drop 44', hL);
  b += ln(320, 8, 320, 288, 'sep');
  b += txt(320, 304, 'Outer side, front left, LOW shown · A = left LOW / right HIGH, B = mirror', 'middle', 'dt');
  return svg(640, 312, b, 'Carrier, short and long arm side by side');
};

SK.button = () => {
  let b = mute(60, 22, 'face', 'middle') + mute(205, 22, 'plan section, front up', 'middle');
  b += rect(36, 52, 48, 96, 'btn ol', 24);
  b += `<ellipse cx="60" cy="100" rx="15" ry="34" class="onl-f"/>`;
  b += dimH(36, 84, 166, '12', true) + dimV(22, 52, 148, '24', 'l');
  const X = (d) => 140 + d * 4.2, Y = (t) => 48 + (t + 10) * 4.2;
  b += rect(X(-6), Y(-10), 29.6 * 4.2, 16 * 4.2, 'kit ol');
  b += rect(X(1.4), Y(6), 16 * 4.2, 11 * 4.2, 'kit ol');
  b += poly([[17.6, 6], [23.6, 6], [23.6, 17], [17.6, 17], [17.6, 15.2], [21.4, 15.2], [21.4, 10.2], [17.6, 9.8]].map(([d, t]) => [X(d), Y(t)]), 'chair ol');
  b += poly([[23.6, -8.5], [23.6, 3.5], [16, 3.5], [16, 10], [21.1, 10.5], [21.1, 15], [12, 15], [12, 0.5], [10, 0.5], [10, -5.5], [17.6, -5.5], [17.6, -8.5]].map(([d, t]) => [X(d), Y(t)]), 'btn ol');
  b += zig(X(2.5), X(10), Y(-2.5), 3 * 4.2, 9, 'spr') + ln(X(2.5), Y(-6.5), X(2.5), Y(1.5), 'onl');
  b += ln(X(30.5), Y(-2.5), X(24.6), Y(-2.5), 'arr') + ah(X(24.4), Y(-2.5), -1, 0) + txt(245, 64, 'press 5');
  b += lead(X(21.1), Y(12.5), 264, 140, 'tooth');
  b += lead(X(23.6), Y(16), 264, 160, 'wing');
  b += lead(X(6), Y(-2.5) + 12, X(-6) + 2, 178, 'spring OD 6');
  b += mute(10, 206, 'travel 5 to a hard stop · tooth 3.5 engaged, 6° face');
  b += mute(10, 223, 'tooth to be upsized for the ~900 N corner knock');
  return svg(320, 232, b, 'Button-catch: face and plan section');
};

SK.sleeve = () => {
  let b = '';
  for (const [x0, lat] of [[50, true], [190, false]]) {
    b += rect(x0, 30, 60, 120, 'kit ol', 6);
    if (lat) b += rect(x0 + 12, 54, 36, 72, 'kit olk', 18);
    else b += rect(x0 + 9, 51, 42, 78, 'ring', 21) + rect(x0 + 12, 54, 36, 72, 'chair olk', 18);
    b += bold(x0 + 30, 172, lat ? 'LATCHED' : 'NOT LATCHED', 'middle');
    b += mute(x0 + 30, 189, lat ? 'flush, quiet' : 'sunk, orange ring', 'middle');
  }
  b += lead(241, 92, 262, 92, '0.6 wall');
  b += mute(10, 206, 'button shown in its real chair colour here');
  return svg(320, 214, b, 'Orange tell sleeve: latched vs not latched');
};

SK.spring = () => {
  let b = zig(60, 204, 70, 27, 18, 'spr-b');
  b += ln(60, 112, 60, 102, 'ext') + ln(204, 112, 204, 102, 'ext') + dimH(60, 204, 112, 'L0 16', true);
  b += dimV(222, 43, 97, 'OD 6');
  b += mute(20, 160, 'wire 0.55 · 9 coils · SS304');
  b += mute(20, 177, '~4 N at rest · ~8 N pressed');
  return svg(320, 188, b, 'Latch spring');
};

SK.dowel = () => {
  let b = rect(90, 58, 140, 26, 'steel ol', 5);
  b += ln(98, 58, 98, 84, 'onl') + ln(222, 58, 222, 84, 'onl');
  b += txt(160, 116, 'Ø and length from the coupon', 'middle');
  b += mute(160, 134, 'schematic · pressed in as the tooth', 'middle');
  return svg(320, 150, b, 'Optional steel dowel');
};

SK.rod6 = () => {
  let b = mute(20, 34, '1 per carrier, bonded with high-Tg epoxy');
  b += rect(30, 60, 198, 21.6, 'cf', 3);
  b += dimV(20, 60, 81.6, 'Ø6', 'l');
  b += ln(30, 88, 30, 110, 'ext') + ln(228, 88, 228, 110, 'ext') + dimH(30, 228, 105, '≈ 55, final in CAD', true);
  const cx = 275, cy = 74, r = 19.5, k = r * Math.SQRT1_2;
  b += `<path class="bore" d="M${n(cx - k)} ${n(cy - k)} L${cx} ${n(cy - r * Math.SQRT2)} L${n(cx + k)} ${n(cy - k)} A${r} ${r} 0 1 1 ${n(cx - k)} ${n(cy - k)} Z"/>`;
  b += circ(cx, cy, 18, 'cf');
  b += txt(cx, 116, 'Ø6.5 bore', 'middle', 'dt') + txt(cx, 132, 'peak up', 'middle', 'dt');
  return svg(320, 142, b, 'Carbon rod Ø6 and its teardrop bore');
};

SK.pins = () => {
  let b = rect(40, 26, 65, 100, 'kit ol', 12.5);
  for (const [dy, dz] of [[3.5, 7.4], [-3.5, 7.4], [3.5, -7.4], [-3.5, -7.4]]) b += circ(72.5 + dy * 5, 76 - dz * 5, 5, 'cf');
  b += dimH(40, 105, 140, '13', true) + dimV(30, 26, 126, '20', 'l');
  b += rect(160, 71, 105, 10, 'cf', 2) + ln(225, 60, 225, 92, 'hid2');
  b += txt(192, 54, '13 tenon', 'middle', 'dt') + txt(245, 54, '8 arm', 'middle', 'dt');
  b += dimH(160, 265, 110, 'Ø2 × 21', true);
  b += mute(10, 176, 'tenon end view: pins at (±3.5, ±7.4)');
  return svg(320, 184, b, 'Carbon tenon pins');
};

function depthShape(x0, yc, L, s, flip = false) {
  // foot face at x0 (child side), sleeve face at x0 + L*s; flip draws it mirrored (sleeve on the left)
  const u = (v) => (flip ? x0 - v * s : x0 + v * s), z = (v) => yc - v * s;
  const fl = Math.min(18, L - 2);
  let b = path(`M${n(u(L))} ${n(z(14))} L${n(u(fl))} ${n(z(14))} C${n(u(fl / 2))} ${n(z(14))} ${n(u(3))} ${n(z(20))} ${n(u(0))} ${n(z(20))} L${n(u(0))} ${n(z(-20))} C${n(u(3))} ${n(z(-20))} ${n(u(fl / 2))} ${n(z(-14))} ${n(u(fl))} ${n(z(-14))} L${n(u(L))} ${n(z(-14))} Z`, 'kit ol');
  b += poly([[u(0), z(4.5)], [u(-2.5), z(6)], [u(-2.5), z(-6)], [u(0), z(-4.5)]], 'kit ol');
  b += path(`M${n(u(L))} ${n(z(10.2))} L${n(u(L - 13))} ${n(z(10.2))} L${n(u(L - 13))} ${n(z(9))} L${n(u(6))} ${n(z(9))} L${n(u(6))} ${n(z(-9))} L${n(u(L - 13))} ${n(z(-9))} L${n(u(L - 13))} ${n(z(-10.2))} L${n(u(L))} ${n(z(-10.2))}`, 'hid');
  return b;
}
SK.depth = () => {
  const s = 2.6, Ls = [24.1, 36.1, 48.1, 60.1], xs = [104, 92, 80, 68];
  let b = '', x = 50;
  Ls.forEach((L, i) => {
    b += depthShape(x, 120, L, s);
    b += bold(x + (L * s) / 2, 52, `D${i} · ${L}`, 'middle') + mute(x + (L * s) / 2, 196, `pad back x ${xs[i]}`, 'middle');
    if (i === 0) b += dimV(38, 68, 172, '40', 'l');
    if (i === 3) b += dimV(x + L * s + 10, 120 - 14 * s, 120 + 14 * s, '28');
    x += L * s + 40;
  });
  b += mute(20, 224, 'Front view, child side left · dashed: hollow 13.4 × 20.4, hidden step to 11 × 18');
  b += mute(20, 242, 'Left: 27 × 40 foot, front-to-back dovetail, closed front · right: 18 × 28 sleeve');
  return svg(640, 252, b, 'Depth pieces D0 to D3, straight foot');
};

SK.tilt = () => {
  const s = 1.25, L = 36.1, yc = 110;
  let b = mute(160, 20, 'front view, D1 shown, child side left', 'middle');
  const rot = (px, py, cx, cy, a) => [cx + (px - cx) * Math.cos(a) - (py - cy) * Math.sin(a), cy + (px - cx) * Math.sin(a) + (py - cy) * Math.cos(a)];
  [[40, 0, 'T0', '0°'], [145, 8, 'T8A', '8° top-in L'], [250, 15, 'T15A', '15° top-in L']].forEach(([xp, deg, code, sub]) => {
    const a = (-deg * Math.PI) / 180;
    const top = rot(xp, yc - 20 * s, xp, yc, a), bot = rot(xp, yc + 20 * s, xp, yc, a);
    b += path(`M${n(xp + L * s)} ${n(yc - 14 * s)} L${n(xp + 18 * s)} ${n(yc - 14 * s)} C${n(xp + 9 * s)} ${n(yc - 14 * s)} ${n(top[0] + 3 * s)} ${n(top[1])} ${n(top[0])} ${n(top[1])} L${n(bot[0])} ${n(bot[1])} C${n(bot[0] + 3 * s)} ${n(bot[1])} ${n(xp + 9 * s)} ${n(yc + 14 * s)} ${n(xp + 18 * s)} ${n(yc + 14 * s)} L${n(xp + L * s)} ${n(yc + 14 * s)} Z`, 'kit ol');
    const pad = [[xp - 4.5 * s, yc - 40 * s], [xp, yc - 40 * s], [xp, yc + 40 * s], [xp - 4.5 * s, yc + 40 * s]].map(([px, py]) => rot(px, py, xp, yc, a));
    b += poly(pad, 'kit ol');
    b += ln(xp, yc - 46 * s, xp, yc + 46 * s, 'ax');
    b += bold(xp + 15, 186, code, 'middle') + mute(xp + 15, 202, sub, 'middle');
  });
  b += mute(10, 226, 'B = mirror (left top-out / right top-in)');
  b += mute(10, 243, 'contact moves ∓4.8 at 8°, ∓8.9 at 15°');
  b += mute(10, 260, 'D0 at 15°: floor margin 2.4 (keep ≥ 2)');
  return svg(320, 268, b, 'Depth piece tilted feet');
};

SK.pad = () => {
  const s = 1.5, x0 = 40, y0 = 36;
  let b = mute(x0, 24, '← front · back face view');
  b += rect(x0, y0, 82 * s, 80 * s, 'kit ol', 18);
  b += rect(x0 + 3.2 * s, y0 + 3.2 * s, (82 - 6.4) * s, (80 - 6.4) * s, 'hid', 14);
  b += rect(x0 + 8.8 * s, y0 + 8.8 * s, (82 - 17.6) * s, (80 - 17.6) * s, 'hid', 8);
  b += rect(x0 + 46 * s, y0 + 20 * s, 27 * s, 40 * s, 'hid');
  for (const dz of [-11, 11]) b += circ(x0 + 63 * s, y0 + (40 + dz) * s, 7 * s, 'onl-f');
  b += lead(x0 + 30 * s, y0 + 6 * s, 172, 46, 'cover groove') + txt(175, 66, '3.2–8.8 in, 2.5 deep', 'start', 'dt');
  b += lead(x0 + 73 * s, y0 + 22 * s, 172, 92, 'dovetail socket') + txt(175, 108, 'foot 27 × 40', 'start', 'dt');
  b += lead(x0 + 63 * s, y0 + 51 * s + 7 * s, 172, 136, 'screw bosses ± 11');
  b += dimH(x0, x0 + 82 * s, 166, '82', true) + dimV(30, y0, y0 + 80 * s, '80', 'l');
  // plan: the front edge curves toward the child (superellipse wrap from the FS)
  const off = (u) => { const y = u - 27; const ex = 85 * Math.pow(Math.max(1 - Math.pow(Math.abs((y - 34) / 88), 3), 0), 1 / 3); return 0.7 * (85 - ex); };
  const pts = []; for (let u = 0; u <= 82; u += 4.1) pts.push([x0 + u * s, 200 + off(u) * s]);
  const back = pts.map((p, i) => (i ? 'L' : 'M') + n(p[0]) + ' ' + n(p[1])).join(' ');
  const front = pts.slice().reverse().map((p) => 'L' + n(p[0]) + ' ' + n(p[1] + 4.5 * s)).join(' ');
  b += path(back + ' ' + front + ' Z', 'kit ol');
  b += lead(x0 + 70 * s, 200 + off(70) * s + 4.5 * s, 172, 222, 'shell 4.5');
  b += mute(x0, 248, 'plan: front edge curves ~7.5 toward the child');
  return svg(320, 256, b, 'Pad shell: back face and plan');
};

SK.ring = () => {
  let b = mute(20, 26, 'ring loop') + mute(150, 26, 'section at the pad edge');
  b += rect(20, 40, 98, 95, 'ring-k', 14) + rect(20, 40, 98, 95, 'ring-k2', 14);
  b += txt(69, 155, '1.6 × 2.5 loop', 'middle', 'dt') + txt(69, 171, '≈ 70 × 68', 'middle', 'dt');
  // shell section (back face up) with the groove, cover U and ring
  b += path('M170 60 L196 60 L196 85 L252 85 L252 60 L300 60 L300 105 L170 105 Q164 105 164 99 L164 66 Q164 60 170 60 Z', 'kit ol');
  b += path('M300 115 L162 115 Q154 115 154 107 L154 58 Q154 50 162 50 L206 50 L206 85', 'cov');
  b += rect(232, 60, 20, 25, 'soft') + rect(216, 81, 16, 4, 'soft');
  b += rect(216, 60, 16, 21, 'kit ol') + poly([[216, 68], [211, 74], [216, 74]], 'kit ol') + poly([[232, 68], [237, 74], [232, 74]], 'kit ol');
  b += txt(150, 146, 'groove 5.6 × 2.5 in the back', 'start', 'dt');
  b += txt(150, 162, 'ring 1.6 + 2 × 2 neoprene', 'start', 'dt');
  b += txt(150, 178, 'snap lip / undercut: OPEN', 'start', 'dt');
  b += mute(150, 194, 'schematic');
  return svg(320, 202, b, 'Clip ring and its section');
};

SK.cover = () => {
  let b = rect(30, 30, 136, 132, 'soft ol', 30);
  b += rect(65, 64, 65.6, 64, 'lt-d', 10);
  b += txt(180, 56, 'neoprene 2') + txt(180, 72, 'no sewing', 'start', 'dt');
  b += txt(180, 106, '≈ 170 × 165 blank') + txt(180, 140, 'dashed: pad 82 × 80', 'start', 'dt') + txt(180, 156, 'trim on the coupon', 'start', 'dt');
  b += mute(15, 188, 'wraps foam + shell edge into the back groove');
  return svg(320, 198, b, 'Cover cut pattern');
};

SK.foam = () => {
  const s = 1.3;
  let b = rect(30, 36, 82 * s, 80 * s, 'soft ol', 13);
  b += dimH(30, 30 + 82 * s, 156, '82', true) + dimV(22, 36, 36 + 80 * s, '80', 'l');
  b += path(`M180 36 L${n(180 + 30 * s - 13)} 36 Q${n(180 + 30 * s)} 36 ${n(180 + 30 * s)} 49 L${n(180 + 30 * s)} ${n(36 + 80 * s - 13)} Q${n(180 + 30 * s)} ${n(36 + 80 * s)} ${n(180 + 30 * s - 13)} ${n(36 + 80 * s)} L180 ${n(36 + 80 * s)} Z`, 'soft ol');
  b += dimH(180, 180 + 30 * s, 156, '30', true);
  b += txt(232, 84, 'child', 'start', 'dt') + txt(232, 100, 'side →', 'start', 'dt');
  b += mute(10, 186, 'cut to the pad outline, rim ~R10');
  return svg(320, 194, b, 'Foam cushion');
};

SK.csk = () => {
  const s = 7, yc = 70;
  let b = path(`M40 ${yc - 35} L${40 + 2.8 * s} ${yc - 17.5} L${40 + 12 * s} ${yc - 17.5} L${40 + 12 * s} ${yc + 17.5} L${40 + 2.8 * s} ${yc + 17.5} L40 ${yc + 35} Z`, 'steel ol');
  for (let x = 70; x < 120; x += 7) b += ln(x, yc - 17.5, x + 3, yc + 17.5, 'onl');
  b += nut(180, yc, s);
  b += ln(40, 34, 40, 14, 'ext') + ln(124, 52, 124, 14, 'ext') + dimH(40, 124, 18, '12');
  b += dimV(28, yc - 35, yc + 35, 'Ø10', 'l');
  b += txt(40, 128, 'M5 × 12 countersunk') + txt(180, 128, 'M5 nut, pocket 5.1');
  b += mute(10, 150, 'lock only: the tenon step takes the load');
  return svg(320, 158, b, 'Tenon screw and nut');
};

SK.button_head = () => {
  const s = 7, yc = 70, o = 12;
  let b = path(`M${o + 40 + 2.75 * s} ${yc - 33.25} A${2.75 * s} 33.25 0 0 0 ${o + 40 + 2.75 * s} ${yc + 33.25} Z`, 'steel ol');
  b += rect(o + 40 + 2.75 * s, yc - 17.5, 10 * s, 35, 'steel ol');
  for (let x = 84; x < 137; x += 7) b += ln(x, yc - 17.5, x + 3, yc + 17.5, 'onl');
  b += nut(190, yc, s);
  b += ln(71.3, 34, 71.3, 14, 'ext') + ln(141.3, 52, 141.3, 14, 'ext') + dimH(71.3, 141.3, 18, '10');
  b += dimV(40, yc - 33.25, yc + 33.25, 'Ø9.5', 'l');
  b += txt(30, 128, 'M5 × 10 ISO 7380') + txt(190, 128, 'M5 nut in the boss');
  b += mute(10, 150, 'lock only: the dovetail stop takes the load');
  return svg(320, 158, b, 'Pad screw and nut');
};

SK.cap = () => {
  const s = 2.4, X = (h) => 40 + h * s, Z = (z) => 20 + (265 - z) * s;
  let b = rect(X(0), Z(265), 45 * s, 75 * s, 'chair ol');
  b += rect(X(0), Z(247), 20 * s, 40 * s, 'kit ol');
  b += rect(X(20), Z(239), 14 * s, 24 * s, 'hid');
  b += dimV(X(0) - 12, Z(247), Z(207), '40', 'l');
  b += ln(X(0), Z(210), X(0), Z(210) + 34, 'ext') + ln(X(20), Z(210), X(20), Z(210) + 34, 'ext') + dimH(X(0), X(20), Z(210) + 30, '20', true);
  b += lead(X(10), Z(245), 160, 66, 'flush, hairlines 0.3');
  b += lead(X(30), Z(227), 160, 120, 'short tongue');
  b += txt(163, 140, 'retention: OPEN', 'start', 'dt');
  b += mute(10, 232, 'optional: owner to confirm cap vs no cap (4.6)');
  return svg(320, 240, b, 'Blank cap in the wing notch');
};

SK.epoxy = () => {
  let b = rect(50, 40, 140, 16, 'kit ol', 3) + rect(50, 62, 140, 16, 'kit ol', 3);
  b += rect(30, 44, 20, 8, 'steel ol') + rect(30, 66, 20, 8, 'steel ol') + poly([[190, 46], [212, 56], [212, 62], [190, 72]], 'kit ol');
  b += txt(222, 64, 'Tg > 80 °C');
  b += mute(30, 104, 'bonds CF6 and the 4 × CF2 pins');
  return svg(320, 114, b, 'High-Tg epoxy');
};

SK.gauge = () => {
  let b = rect(20, 30, 280, 86, 'kit ol', 6) + rect(20, 30, 14, 86, 'chair ol', 3);
  for (let x = 44; x < 300; x += 12) b += ln(x, 30, x, x % 60 === 44 % 60 ? 40 : 36, 'onl');
  b += rect(34, 49, 266, 14, 'groove') + rect(34, 74, 266, 10, 'groove') + rect(34, 96, 266, 5, 'groove');
  b += txt(292, 47, 'Ø8', 'end', 'on-t') + txt(292, 72, 'Ø6', 'end', 'on-t') + txt(292, 94, 'Ø2', 'end', 'on-t');
  b += mute(20, 138, 'schematic · stop at the left · ±0.3');
  return svg(320, 146, b, 'Rod cutting gauge v2');
};

// ---------------- exploded assembly ----------------
function balloon(cx, cy, label, toX, toY) {
  const w = Math.max(32, label.length * 8.6 + 18);
  return (toX != null ? ln(cx, cy, toX, toY, 'ld') + circ(toX, toY, 1.8, 'dot') : '') + rect(cx - w / 2, cy - 10, w, 20, 'bal', 10) + txt(cx, cy + 4.5, label, 'middle', 't b');
}
function exploded(parts) {
  let b = '';
  // axis
  b += ln(100, 250, 724, 250, 'ax');
  // WG wing with the notch and pocket
  b += rect(40, 60, 59, 280, 'chair ol') + rect(40, 166, 59, 80, 'cut hid2') + rect(51.8, 176, 32.8, 60, 'hid2');
  b += balloon(70, 36, 'WG', 70, 60);
  // carrier: nose, arm, tenon, pins, rod, button
  b += path('M269 178 C320 178 320 222 370 222 L370 278 C320 278 320 234 269 234 Z', 'kit ol');
  b += rect(210, 166, 59, 80, 'kit ol', 4);
  b += rect(370, 230, 26, 40, 'kit ol', 5);
  b += `<line x1="354" y1="235.2" x2="396" y2="235.2" class="rod" stroke-width="4"/><line x1="354" y1="264.8" x2="396" y2="264.8" class="rod" stroke-width="4"/>`;
  b += circ(238.2, 226, 6, 'cf');
  b += rect(210, 182, 6, 48, 'btn ol', 2);
  b += balloon(320, 140, 'AR-S / AR-L', 320, 196);
  b += balloon(238, 300, 'CF6', 238, 232);
  b += balloon(383, 300, 'CF2', 383, 265);
  // latch parts, exploded up-left
  b += rect(104, 92, 12, 48, 'btn ol', 3) + rect(116, 110, 16, 12, 'btn ol');
  b += rect(150, 89, 6, 54, 'ring-o', 3);
  b += zig(190, 222, 116, 6, 9, 'spr');
  b += rect(106, 150, 14, 6, 'steel ol', 2);
  b += `<path d="M160 146 Q190 175 208 204" class="hid2"/>`;
  b += balloon(110, 70, 'BT', 110, 92) + balloon(153, 70, 'TS', 153, 89) + balloon(206, 70, 'SP', 206, 110) + balloon(152, 168, 'DW opt', 121, 153);
  // tenon screw (from the back face) above the sleeve
  b += path('M440 140 L456 140 L452 146 L452 172 L444 172 L444 146 Z', 'steel ol') + rect(441, 178, 14, 8, 'steel ol', 1);
  b += ln(448, 188, 448, 220, 'hid2');
  b += balloon(440, 112, 'HW-M5x12C', 440, 140);
  // depth piece D1, sleeve left / foot right
  b += depthShape(502.2, 250, 36.1, 2, true);
  b += balloon(466, 330, 'DP', 466, 290);
  // pad screw
  b += path('M522 152 A6 9 0 0 0 522 170 Z', 'steel ol') + rect(522, 157, 22, 8, 'steel ol');
  b += `<path d="M546 161 Q566 200 576 246" class="hid2"/>`;
  b += balloon(545, 112, 'HW-M5x10B', 532, 152);
  // ring, shell (2 bosses), foam, cover
  b += rect(556, 204, 5, 136, 'kit ol', 2);
  b += rect(580, 192, 9, 160, 'kit ol', 2) + rect(574, 244, 6, 12, 'kit ol') + rect(574, 288, 6, 12, 'kit ol');
  b += path('M610 192 L650 192 Q670 192 670 212 L670 332 Q670 352 650 352 L610 352 Z', 'soft ol');
  b += `<path d="M652 180 L688 180 Q702 180 702 194 L702 350 Q702 364 688 364 L652 364" class="cov-thin"/>`;
  b += balloon(546, 392, 'RG', 558, 340) + balloon(592, 392, 'PD', 585, 352) + balloon(640, 392, 'FM', 640, 352) + balloon(700, 392, 'CV', 702, 364);
  // kit-off inset: blank cap
  b += rect(40, 362, 59, 44, 'chair ol') + rect(40, 372, 59, 24, 'kit ol');
  b += balloon(126, 384, 'CP') + txt(150, 380, 'blank cap when the kit is off') + txt(150, 397, '(optional, owner to confirm)', 'start', 'dt');
  // legend
  b += bold(760, 40, 'Balloons = GBG-SP-<code>-…-r5');
  const seen = new Set();
  let y = 62;
  for (const p of parts) {
    const k = p.id === 'AR' ? 'AR-S / AR-L' : p.id === 'DPT' ? 'DP (tilt)' : p.id === 'HW12' ? 'HW-M5x12C' : p.id === 'HW10' ? 'HW-M5x10B' : p.id === 'DW' ? 'DW opt' : p.code;
    if (seen.has(k)) continue;
    seen.add(k);
    const name = (p.short || p.name).length > 30 ? (p.short || p.name).slice(0, 29) + '…' : p.short || p.name;
    b += txt(760, y, k, 'start', 't b') + txt(860, y, name, 'start', 'dt');
    y += 17;
  }
  return svg(1080, 420, b, 'Exploded assembly with part balloons');
}

// ---------------- page parts ----------------
const STATUS = ['AGREED', 'RECOMMENDED', 'OPEN'];
const chip = (s) => {
  const k = STATUS.includes(s) ? s : 'OPEN';
  return `<span class="chip st-${k}"><i></i>${esc(s)}</span>`;
};
const qtyStr = (q) => (q ? `per side ${q.side ?? '–'} · per pair ${q.pair ?? '–'} · full kit ${q.kit ?? '–'}` : '–');

function card(p) {
  let sk;
  try {
    sk = (SK[p.sketch] || (() => svg(320, 60, txt(160, 34, 'no sketch', 'middle', 'dt'), 'no sketch')))(p);
  } catch (e) {
    sk = `<p class="p5-err">Sketch failed: ${esc(e.message)}</p>`;
  }
  const nums = (p.numbers || []).map((x) => `<code>${esc(x)}</code>`).join(' ');
  const dims = (p.dims || []).map((d) => `<li>${esc(d)}</li>`).join('');
  return `<article class="card${p.wide ? ' wide' : ''}" id="p5-${esc(p.id)}">
  <div class="sk">${sk}</div>
  <div class="hd">${chip(p.status)}<span class="grp">${esc(p.group || '')}</span></div>
  <h3>${esc(p.name)}</h3>
  <div class="nums">${nums}</div>
  <dl>
    <dt>Variants</dt><dd>${esc((p.variants || []).join(' · ') || '–')}</dd>
    <dt>Qty</dt><dd>${esc(qtyStr(p.qty))}${p.qty && p.qty.note ? `<br><small>${esc(p.qty.note)}</small>` : ''}</dd>
    <dt>Material</dt><dd>${esc(p.material || '–')}</dd>
    <dt>Print</dt><dd>${esc(p.print || '–')}</dd>
    <dt>Does</dt><dd>${esc(p.function || '')}</dd>
  </dl>
  ${dims ? `<details><summary>Key dimensions</summary><ul>${dims}</ul></details>` : ''}
  ${p.status_note ? `<p class="sn">${esc(p.status_note)}</p>` : ''}
</article>`;
}

function table(parts) {
  const rows = parts.map((p) => `<tr>
    <td data-l="Part no."><span>${(p.numbers || []).map((x) => `<code>${esc(x)}</code>`).join('<br>')}</span></td>
    <td data-l="Part"><span>${esc(p.name)}</span></td>
    <td data-l="Variants"><span>${esc((p.variants || []).join(' · '))}</span></td>
    <td data-l="Per side"><span>${esc(p.qty?.side ?? '–')}</span></td>
    <td data-l="Per pair"><span>${esc(p.qty?.pair ?? '–')}</span></td>
    <td data-l="Full kit"><span>${esc(p.qty?.kit ?? '–')}</span></td>
    <td data-l="Material"><span>${esc(p.material || '')}</span></td>
    <td data-l="Print"><span>${esc(p.print || '')}</span></td>
    <td data-l="Status"><span>${chip(p.status)}</span></td></tr>`).join('');
  return `<div class="tw"><table class="pl"><thead><tr><th>Part no.</th><th>Part</th><th>Variants</th><th>Per side</th><th>Per pair</th><th>Full kit</th><th>Material</th><th>Print</th><th>Status</th></tr></thead><tbody>${rows}</tbody></table></div>`;
}

function childSet(cs) {
  if (!cs || !Array.isArray(cs.items)) return '';
  const rows = cs.items.map((i) => `<tr><td data-l="Part no."><span><code>${esc(i.number)}</code></span></td><td data-l="Qty"><span>${esc(i.qty)}</span></td><td data-l="Note"><span>${esc(i.note || '')}</span></td></tr>`).join('');
  return `<h3 class="h">${esc(cs.title || 'Example set')}</h3>
  <p class="m">${esc(cs.setting || '')}</p>
  <div class="tw"><table class="pl cs"><thead><tr><th>Part no.</th><th>Qty</th><th>Note</th></tr></thead><tbody>${rows}</tbody></table></div>
  ${cs.growth ? `<p class="m">${esc(cs.growth)}</p>` : ''}`;
}

const CSS = `
.p5{--p5-ok:#2e9e5b;--p5-rec:#c98a00;color:var(--ink);max-width:1200px}
.p5 .lead{color:var(--mute);margin:0 0 14px;max-width:85ch}
.p5 .grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(300px,1fr));gap:14px}
.p5 .card{background:var(--panel);border:1px solid var(--line);border-radius:12px;padding:12px 14px;min-width:0;display:flex;flex-direction:column;gap:6px}
.p5 .card.wide{grid-column:span 2}
@media (max-width:700px){.p5 .card.wide{grid-column:auto}}
.p5 .sk{overflow-x:auto;border-bottom:1px solid var(--line);padding-bottom:6px}
.p5 .sk svg,.p5 .ex svg{display:block;width:100%;height:auto;font-size:13px;color:var(--ink)}
.p5 h3{margin:2px 0 0;font-size:16px;line-height:1.3}
.p5 .hd{display:flex;gap:8px;align-items:center;justify-content:space-between}
.p5 .grp{color:var(--mute);font-size:13px}
.p5 .nums{display:flex;flex-wrap:wrap;gap:4px}
.p5 code{font-size:12.5px;background:color-mix(in srgb,var(--line) 55%,transparent);border-radius:5px;padding:1px 5px;white-space:nowrap}
.p5 dl{display:grid;grid-template-columns:72px 1fr;gap:3px 8px;margin:2px 0;font-size:14px}
.p5 dt{color:var(--mute)}.p5 dd{margin:0}
.p5 dd small,.p5 .sn,.p5 .m{color:var(--mute);font-size:13px}
.p5 .sn{margin:0;border-top:1px dashed var(--line);padding-top:6px}
.p5 details{font-size:14px}.p5 summary{cursor:pointer;color:var(--mute)}.p5 details ul{margin:4px 0 0;padding-left:18px}
.p5 .chip{display:inline-flex;align-items:center;gap:6px;font-size:12px;font-weight:600;letter-spacing:.03em;border:1px solid currentColor;border-radius:999px;padding:1px 9px;white-space:nowrap}
.p5 .chip i{width:8px;height:8px;border-radius:50%;background:currentColor}
.p5 .st-AGREED{color:var(--p5-ok)}.p5 .st-RECOMMENDED{color:var(--p5-rec)}.p5 .st-OPEN{color:var(--acc)}
.p5 h2.h{font-size:20px;margin:28px 0 8px}.p5 h3.h{font-size:17px;margin:22px 0 4px}
.p5 .ex{overflow-x:auto;background:var(--panel);border:1px solid var(--line);border-radius:12px;padding:10px}
.p5 .tw{overflow-x:auto}
.p5 table.pl{border-collapse:collapse;width:100%;font-size:14px}
.p5 table.pl th,.p5 table.pl td{border-bottom:1px solid var(--line);padding:6px 8px;text-align:left;vertical-align:top}
.p5 table.pl th{color:var(--mute);font-weight:600;font-size:13px}
@media (max-width:760px){
 .p5 table.pl thead{display:none}
 .p5 table.pl,.p5 table.pl tbody,.p5 table.pl tr,.p5 table.pl td{display:block;width:100%;box-sizing:border-box}
 .p5 table.pl tr{border:1px solid var(--line);border-radius:10px;margin:0 0 10px;padding:6px 8px;background:var(--panel)}
 .p5 table.pl td{border:0;padding:3px 0;display:grid;grid-template-columns:86px 1fr;gap:8px}
 .p5 table.pl td>span{min-width:0;overflow-wrap:anywhere}
 .p5 table.pl td code{white-space:normal;overflow-wrap:anywhere}
 .p5 table.pl td::before{content:attr(data-l);color:var(--mute);font-size:13px}
}
.p5 .open li{margin:2px 0}
.p5 .p5-err{color:var(--acc);font-weight:600}
.p5 svg .t{fill:currentColor}.p5 svg .b{font-weight:600}
.p5 svg .dt{fill:var(--mute)}
.p5 svg .on-t{fill:${COL.on}}
.p5 svg .dl,.p5 svg .ext{stroke:var(--mute);stroke-width:1;fill:none}
.p5 svg .ext{stroke-width:.7;stroke-dasharray:2 2}
.p5 svg .ah,.p5 svg .dot{fill:var(--mute)}
.p5 svg .ld{stroke:var(--mute);stroke-width:.9;fill:none}
.p5 svg .ol{stroke:var(--ink);stroke-width:1.4;stroke-linejoin:round}
.p5 svg .kit{fill:${COL.kit}}.p5 svg .chair{fill:${COL.chair}}.p5 svg .btn{fill:${COL.btn}}
.p5 svg .steel{fill:${COL.st}}.p5 svg .soft{fill:${COL.soft}}.p5 svg .cf{fill:${COL.cf}}
.p5 svg .rod{stroke:${COL.cf};stroke-linecap:round;opacity:.85}
.p5 svg .hid{fill:none;stroke:${COL.on};stroke-width:1;stroke-dasharray:4 3}
.p5 svg .hid2{fill:none;stroke:var(--ink);stroke-width:1;stroke-dasharray:4 3}
.p5 svg .cut{fill:var(--panel)}
.p5 svg .acc-d{fill:none;stroke:var(--acc);stroke-width:1.4;stroke-dasharray:3 2}
.p5 svg .ax{stroke:var(--mute);stroke-width:1;stroke-dasharray:8 3 2 3;fill:none}
.p5 svg .arr{stroke:var(--ink);stroke-width:1.6;fill:none}
.p5 svg .olk{stroke:#2b2f33;stroke-width:1.4}
.p5 svg .sep{stroke:var(--line);stroke-width:1}
.p5 svg .onl{stroke:${COL.on};stroke-width:1;fill:none;opacity:.7}
.p5 svg .onl-f{fill:none;stroke:${COL.on};stroke-width:1;opacity:.6}
.p5 svg .spr{fill:none;stroke:${COL.st};stroke-width:2;stroke-linejoin:round}
.p5 svg .spr-b{fill:none;stroke:${COL.st};stroke-width:4;stroke-linejoin:round}
.p5 svg .ring{fill:none;stroke:${COL.btn};stroke-width:3.5}
.p5 svg .ring-o{fill:none;stroke:${COL.btn};stroke-width:2.5}
.p5 svg .ring-k{fill:none;stroke:var(--ink);stroke-width:5}
.p5 svg .ring-k2{fill:none;stroke:${COL.kit};stroke-width:2.6}
.p5 svg .cov{fill:none;stroke:${COL.soft};stroke-width:20;stroke-linejoin:round;opacity:.9}
.p5 svg .cov-thin{fill:none;stroke:${COL.soft};stroke-width:4;stroke-linejoin:round}
.p5 svg .lt-d{fill:none;stroke:${COL.kit};stroke-width:1.2;stroke-dasharray:4 3}
.p5 svg .bore{fill:none;stroke:var(--ink);stroke-width:1.4}
.p5 svg .groove{fill:${COL.chair};stroke:${COL.on};stroke-width:.8}
.p5 svg .bal{fill:var(--panel);stroke:var(--ink);stroke-width:1.2}
`;

function injectStyle() {
  if (typeof document === 'undefined' || document.getElementById('p5-style')) return;
  const st = document.createElement('style');
  st.id = 'p5-style';
  st.textContent = CSS;
  document.head.appendChild(st);
}

export async function mountParts(el, url) {
  try {
    if (!el) throw new Error('no element to mount into');
    injectStyle();
    el.innerHTML = '<div class="p5"><p class="lead">Loading the part list…</p></div>';
    const res = await fetch(url || new URL('./parts5.json', import.meta.url));
    if (!res.ok) throw new Error(`parts5.json: HTTP ${res.status}`);
    const data = await res.json();
    const parts = Array.isArray(data && data.parts) ? data.parts : null;
    if (!parts || !parts.length) throw new Error('parts5.json has no parts');
    const sc = data.scheme || {};
    let h = '<div class="p5">';
    h += `<p class="lead">Every part of the Round 5 side-pad kit with a sketch (mm, schematic but in proportion). Part numbers follow <code>${esc(sc.pattern || 'GBG-SP-<code>-<variant>-r5')}</code> ${chip(sc.status || 'RECOMMENDED')} ${esc(sc.status_note || '')}</p>`;
    h += `<div class="grid">${parts.map(card).join('')}</div>`;
    h += '<h2 class="h">Exploded assembly</h2>';
    let ex;
    try { ex = exploded(parts); } catch (e) { ex = `<p class="p5-err">Exploded view failed: ${esc(e.message)}</p>`; }
    h += `<div class="ex">${ex}</div><p class="m">Front view, schematic (heights in proportion, gaps opened up). Order left → right: wing notch, carrier with the button parts, tenon into the depth piece, pad shell with the clip ring, foam, cover. Dashed lines show where a part goes.</p>`;
    h += '<h2 class="h">Part list</h2>' + table(parts);
    h += childSet(data.child_set);
    if (Array.isArray(data.open) && data.open.length) h += `<h3 class="h">Open for the owner</h3><ul class="open">${data.open.map((o) => `<li>${esc(o)}</li>`).join('')}</ul>`;
    h += '</div>';
    el.innerHTML = h;
    return data;
  } catch (e) {
    console.error('parts5:', e);
    if (el) el.innerHTML = `<div class="p5"><p class="p5-err">Part list could not be shown: ${esc(e && e.message ? e.message : e)}</p></div>`;
    return null;
  }
}
