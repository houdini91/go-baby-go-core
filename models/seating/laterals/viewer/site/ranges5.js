// Side pad Round 5: placement & padding ranges vs children aged 2–5 (review diagram).
// mountRanges(el): fetches ranges5.json and renders (a) a true-scale front view with controls and a fit check,
// (b) range charts for height and width, (c) a conditions table, (d) gaps & recommendations.
// No external libraries. Uses the page vars --bg --panel --ink --mute --line --acc.

const NS = 'http://www.w3.org/2000/svg';
const esc = s => String(s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const f1 = v => (Math.round(v * 10) / 10).toString().replace('-', '−');
const sgn = v => (v > 0 ? '+' : '') + f1(v);

const CSS = `
.r5{--r5ok:light-dark(#0b7a44,#4fd18b);--r5warn:light-dark(#9a5b00,#f0b44c);--r5bad:light-dark(#b42318,#ff7b70);
  --r5chair:light-dark(#d8d4cc,#4d4a45);--r5kit:#f1f2f3;--r5foam:#22304d;--r5child:light-dark(#e7dfd6,#3b3631);--r5childln:light-dark(#9c9187,#8a8076);
  color:var(--ink);font:14px/1.45 "IBM Plex Sans",system-ui,sans-serif}
.r5 *{box-sizing:border-box}
.r5 h2{font-size:19px;margin:22px 0 6px;font-weight:600}.r5 h3{font-size:15px;margin:14px 0 6px;font-weight:600}
.r5 p{margin:4px 0 8px;max-width:95ch}.r5 .m{color:var(--mute)}
.r5 .grid{display:grid;grid-template-columns:minmax(0,1.25fr) minmax(280px,1fr);gap:16px;align-items:start}
@media (max-width:700px){.r5 .grid{grid-template-columns:1fr}}
.r5 .card{background:var(--panel);border:1px solid var(--line);border-radius:10px;padding:12px}
.r5 svg{display:block;width:100%;height:auto}
.r5 .front{max-height:78vh}
.r5 .ctl{margin:0 0 10px}.r5 .ctl b{display:block;font-size:12px;font-weight:600;color:var(--mute);text-transform:uppercase;letter-spacing:.04em;margin:0 0 4px}
.r5 .bs{display:flex;flex-wrap:wrap;gap:4px}
.r5 button{font:inherit;font-size:13px;padding:4px 9px;border-radius:6px;border:1px solid var(--line);background:var(--bg);color:var(--ink);cursor:pointer;min-height:30px}
.r5 button:hover{border-color:var(--mute)}
.r5 button.on{border-color:var(--acc);box-shadow:inset 0 0 0 1px var(--acc);font-weight:600}
.r5 button:focus-visible{outline:2px solid var(--acc);outline-offset:1px}
.r5 .chk{width:100%;border-collapse:collapse;font-size:13px;margin-top:6px}
.r5 .chk td,.r5 .chk th{padding:5px 6px;border-top:1px solid var(--line);text-align:left;vertical-align:top}
.r5 .chk th{font-weight:600;color:var(--mute)}
.r5 .st{font-weight:600;white-space:nowrap}.r5 .st.ok{color:var(--r5ok)}.r5 .st.warn{color:var(--r5warn)}.r5 .st.bad{color:var(--r5bad)}
.r5 .st::before{content:"";display:inline-block;width:.62em;height:.62em;margin-right:.35em;border-radius:50%;background:currentColor;vertical-align:.02em}
.r5 .st.warn::before{border-radius:1px;transform:rotate(45deg) scale(.85)}.r5 .st.bad::before{border-radius:1px}
.r5 .leg{display:flex;flex-wrap:wrap;gap:6px 14px;font-size:12px;color:var(--mute);margin:6px 0 0}
.r5 .leg i{display:inline-block;width:14px;height:10px;margin-right:5px;vertical-align:-1px;border:1px solid var(--ink);border-radius:2px}
.r5 .tbl{width:100%;border-collapse:collapse;font-size:13px}
.r5 .tbl th,.r5 .tbl td{border-top:1px solid var(--line);padding:7px 8px;text-align:left;vertical-align:top}
.r5 .tbl th{font-weight:600;color:var(--mute);font-size:12px;text-transform:uppercase;letter-spacing:.03em}
.r5 .tbl td:first-child{font-weight:600;min-width:12ch}
.r5 .scroll{overflow-x:auto}
@media (max-width:700px){.r5 .tbl thead{display:none}.r5 .tbl tr{display:block;border-top:1px solid var(--line);padding:6px 0}
  .r5 .tbl td{display:block;border:0;padding:3px 4px}.r5 .tbl td[data-h]::before{content:attr(data-h) ": ";color:var(--mute);font-weight:600}}
.r5 sup a{color:var(--mute);text-decoration:none;font-size:10px}.r5 sup a:hover{color:var(--acc)}
.r5 ol.src{font-size:12px;color:var(--mute);padding-left:22px}.r5 ol.src a{color:inherit}
.r5 ul.gaps li{margin:0 0 6px;max-width:95ch}
.r5 .pairs{border-collapse:collapse;font-size:12px}.r5 .pairs td,.r5 .pairs th{border:1px solid var(--line);padding:3px 7px;text-align:center}
.r5 .err{color:var(--r5bad);font-weight:600}
.r5 svg text{fill:var(--ink);font:11px "IBM Plex Sans",system-ui,sans-serif}
.r5 svg .mu{fill:var(--mute)}.r5 svg .sm{font-size:9px}
.r5 svg .chair{fill:var(--r5chair);stroke:none}
.r5 svg .chairln{fill:none;stroke:var(--r5chair);stroke-width:1.5}
.r5 svg .child{fill:var(--r5child);stroke:var(--r5childln);stroke-width:.8}
.r5 svg .arm{fill:var(--r5child);fill-opacity:.45;stroke:var(--r5childln);stroke-width:.6;stroke-dasharray:3 2}
.r5 svg .shell{fill:var(--r5kit);stroke:var(--ink);stroke-width:.7}
.r5 svg .foam{fill:var(--r5foam);stroke:var(--ink);stroke-width:.5}
.r5 svg .ghost{fill:none;stroke:var(--mute);stroke-width:.6;stroke-dasharray:2.5 2}
.r5 svg .ghostw{fill:none;stroke:var(--mute);stroke-width:.8;stroke-dasharray:1.5 1.5}
.r5 svg .axl{stroke:var(--ink);stroke-width:.7;stroke-dasharray:5 3}
.r5 svg .zone{fill:var(--r5ok);fill-opacity:.12;stroke:var(--r5ok);stroke-width:.5;stroke-dasharray:3 2}
.r5 svg .grid{stroke:var(--line);stroke-width:1}
.r5 svg .need{fill:var(--r5child);stroke:var(--ink);stroke-width:1}
.r5 svg .need.sel{stroke:var(--acc);stroke-width:2}
.r5 svg .tick{stroke:var(--ink);stroke-width:2;stroke-linecap:round}
.r5 svg .tick.mu2{stroke:var(--mute);stroke-width:1.5}
.r5 svg .guide{stroke:var(--mute);stroke-width:.8;stroke-dasharray:2 3}
.r5 svg .hit{fill:var(--r5ok)}.r5 svg .miss{fill:none;stroke:var(--mute)}
.r5 svg .acc{stroke:var(--acc)}
`;

export async function mountRanges(el) {
  el.classList.add('r5');
  el.innerHTML = '<p class="m">Loading ranges…</p>';
  try {
    const res = await fetch(new URL('./ranges5.json', import.meta.url));
    if (!res.ok) throw new Error('ranges5.json: HTTP ' + res.status);
    const D = await res.json();
    render(el, D);
  } catch (e) {
    console.error(e);
    el.innerHTML = '<p class="err">Ranges diagram failed: ' + esc(e.message) + '</p>';
  }
}

function render(el, D) {
  const kids = Object.keys(D.children);
  const H = Object.fromEntries(D.heights.map(h => [h.id, h]));
  const DP = Object.fromEntries(D.depths.map(d => [d.id, d]));
  const T = D.tilts;
  const srcN = Object.fromEntries(D.sources.map((s, i) => [s.id, i + 1]));
  const cite = ids => ids.map(id => `<sup><a href="#r5src-${id}" title="${esc(D.sources[srcN[id] - 1].t)}">[${srcN[id]}]</a></sup>`).join('');

  const S = { kid: '2y P50', hL: 'H2', hR: 'H2', dL: 'D0', dR: 'D0', foam: '30', tilt: 'T0' };
  // start on each child's best width + its height
  const autoFit = k => { const c = D.children[k]; S.hL = S.hR = c.fits_heights[0] || 'H1'; S.dL = S.dR = c.best_width.depth; S.foam = String(c.best_width.foam); };
  autoFit(S.kid);

  const btns = (key, items, label) => `<div class="ctl"><b>${label}</b><div class="bs" role="group" aria-label="${esc(label)}">` +
    items.map(([v, t, tip]) => `<button type="button" data-k="${key}" data-v="${esc(v)}"${tip ? ` title="${esc(tip)}"` : ''}>${esc(t)}</button>`).join('') + '</div></div>';
  const hItems = D.heights.map(h => [h.id, `${h.id} · ${h.z}`, `${h.arm.map(a => a === 'S' ? 'short' : 'long').join(' / ')} arm, ${h.flip}, pad screw ${h.screw.join(' / ')}; top z ${h.top}; serves axilla z ${h.serves_axilla[0]}–${h.serves_axilla[1]}`]);
  const dItems = D.depths.map(d => [d.id, `${d.id} · x ${d.back_x}`, `pad back x ${d.back_x}`]);

  el.innerHTML = `<style>${CSS}</style>
  <h2>Side pad ranges vs what children aged 2–5 need</h2>
  <p class="m">${esc(D.frame)} True scale (1 unit = 1 mm). Child centred on x 0. ${esc(D.pad.contact_formula)}.</p>
  <div class="grid">
    <div class="card"><svg class="front" id="r5front" role="img" aria-label="Front view of the chair, child and side pads"></svg>
      <div class="leg"><span><i style="background:var(--r5chair);border-color:var(--r5chair)"></i>chair (simplified)</span><span><i style="background:var(--r5kit)"></i>pad shell / kit</span><span><i style="background:var(--r5foam)"></i>foam + cover</span><span><i style="background:var(--r5child)"></i>child (selected size)</span><span><i style="border-style:dashed;background:none"></i>other heights (left side)</span><span><i style="border-style:dotted;background:none"></i>D0–D3 foam faces</span><span><i style="background:color-mix(in srgb,var(--r5ok) 14%,transparent);border-color:var(--r5ok);border-style:dashed"></i>pad-top zone 25–50 mm below axilla</span></div>
    </div>
    <div class="card">
      ${btns('kid', kids.map(k => [k, k, `seated axilla ${D.children[k].ax} mm above seat, chest breadth ${D.children[k].B}`]), 'Child size')}
      ${btns('hL', hItems, "Height · child's left pad (+x)")}
      ${btns('hR', hItems, "Height · child's right pad (−x)")}
      ${btns('dL', dItems, 'Depth piece · left')}
      ${btns('dR', dItems, 'Depth piece · right')}
      ${btns('foam', D.pad.foams.map(f => [String(f), f + ' mm' + (f === D.pad.foam_std ? ' (std)' : '')]), 'Foam (both pads)')}
      ${btns('tilt', Object.entries(T).map(([k, t]) => [k, t.label]), 'Tilt foot (both pads, mirrored)')}
      <div class="bs"><button type="button" id="r5fit" title="Pick the child's own height and best depth/foam">Best fit for this child</button></div>
      <div id="r5chk" aria-live="polite"></div>
    </div>
  </div>

  <h2>Range charts</h2>
  <p class="m">Height: the bar is the pad-centre window that puts the pad top 25–50 mm below the axilla (pad top = centre + 40). Ticks are the 6 kit heights; a filled dot marks a height inside the window. Width: the bar is the free width between the two foam faces a child needs (chest breadth − 10 … + 20 mm, i.e. 5 mm preload to 10 mm air per side); the black tick is the chest breadth. Kit rows show D0–D3 for each foam thickness.</p>
  <div class="grid"><div class="card"><h3>Pad centre height (z, mm)</h3><svg id="r5hc" role="img" aria-label="Needed pad-centre window per child vs the 6 kit heights"></svg></div>
  <div class="card"><h3>Free width between the pads (mm)</h3><svg id="r5wc" role="img" aria-label="Needed free width per child vs depth pieces D0 to D3"></svg></div></div>

  <h2>Conditions</h2>
  <p class="m">What lateral supports typically have to do, and how the Round 5 kit covers it. Coverage is always written out; the colour only repeats it. Clinical numbers are for typical children; measure each child.</p>
  <div class="card scroll"><table class="tbl"><thead><tr><th>Condition</th><th>Typically needs from lateral supports</th><th>Kit covers it?</th><th>Why</th><th>Suggested setting</th></tr></thead><tbody>
  ${D.conditions.map(c => { const cl = /^No/.test(c.cover) ? 'bad' : /^Yes(?!\s*\/)/.test(c.cover) ? 'ok' : 'warn';
    return `<tr><td>${esc(c.c)}</td><td data-h="Needs">${esc(c.need)}${cite(c.src)}</td><td data-h="Covers"><span class="st ${cl}">${esc(c.cover)}</span></td><td data-h="Why">${esc(c.why)}</td><td data-h="Setting">${esc(c.set)}</td></tr>`; }).join('')}
  </tbody></table></div>

  <h2>Gaps &amp; recommendations</h2>
  <div class="card"><ul class="gaps">${D.gaps.map(g => `<li>${esc(g)}</li>`).join('')}</ul>
  <h3>Left/right height pairs the 4-carrier kit can build at once</h3>
  <p class="m">${esc(D.asym_pairs.note)}</p>
  <div class="scroll"><table class="pairs"><tr><th>L \\ R</th>${D.heights.map(h => `<th>${h.id}</th>`).join('')}</tr>
  ${D.heights.map(a => `<tr><th>${a.id}</th>${D.heights.map(b => { const ok = D.asym_pairs.ok[a.id + '/' + b.id]; return `<td><span class="st ${ok ? 'ok' : 'bad'}">${ok ? 'yes' : 'no'}</span></td>`; }).join('')}</tr>`).join('')}
  </table></div>
  <p class="m">${esc(D.gap_rule_note)}</p><p class="m">${esc(D.pad.foam_note)} ${esc(D.pad.reference_cushion_check)}</p></div>

  <h3>Sources</h3><ol class="src">${D.sources.map(s => `<li id="r5src-${s.id}">${s.u ? `<a href="${esc(s.u)}" target="_blank" rel="noopener">${esc(s.t)}</a>` : esc(s.t)}</li>`).join('')}</ol>`;

  const $ = s => el.querySelector(s);
  el.querySelectorAll('button[data-k]').forEach(b => b.addEventListener('click', () => {
    S[b.dataset.k] = b.dataset.v;
    if (b.dataset.k === 'kid') autoFit(b.dataset.v);
    update();
  }));
  $('#r5fit').addEventListener('click', () => { autoFit(S.kid); update(); });

  // ---------------- geometry helpers ----------------
  const contact = (d, foam) => DP[d].contact[foam];
  const sideCheck = (hId, dId) => {
    const c = D.children[S.kid], t = T[S.tilt], h = H[hId];
    const xc = contact(dId, S.foam), inward = Math.min(0, t.top_dx, t.bot_dx);
    const gap = xc + inward - c.half_chest;
    const top = h.z + 40 + t.top_dz, below = c.axilla_z - top;
    const hs = below < D.rule.top_below_min ? ['bad', 'too high – presses into the axilla'] : below > D.rule.top_below_max ? ['warn', 'too low – little lateral control'] : ['ok', 'ok'];
    const G = D.gap_rule;
    const gs = gap >= G.ok[0] && gap <= G.ok[1] ? ['ok', 'ok (snug)'] : gap > G.warn_loose[0] && gap <= G.warn_loose[1] ? ['warn', 'loose'] :
      gap >= G.warn_firm[0] && gap < G.warn_firm[1] ? ['warn', 'firm – foam squashed ' + f1(-gap) + ' mm'] : gap > 0 ? ['bad', 'too loose – no support'] : ['bad', 'squeeze – foam bottoms out'];
    return { xc, inward, gap, top, below, hs, gs, cz: h.z + t.cdz };
  };

  function update() {
    el.querySelectorAll('button[data-k]').forEach(b => b.classList.toggle('on', S[b.dataset.k] === b.dataset.v));
    drawFront(); drawChecks(); drawHeightChart(); drawWidthChart();
  }

  // ---------------- (a) front view ----------------
  function drawFront() {
    const svg = $('#r5front'), c = D.children[S.kid], ch = D.chair;
    const ZT = 700, X = x => x, Y = z => ZT - z;
    svg.setAttribute('viewBox', `-240 ${ZT - 690} 430 ${700}`);
    const o = [];
    const rect = (x0, x1, z0, z1, cls, extra = '') => `<rect x="${f(Math.min(x0, x1))}" y="${f(Y(Math.max(z0, z1)))}" width="${f(Math.abs(x1 - x0))}" height="${f(Math.abs(z1 - z0))}" class="${cls}" ${extra}/>`;
    const f = v => Math.round(v * 100) / 100;
    // chair: backrest (behind), seat, armrests, wings
    o.push(rect(ch.back.x[0], ch.back.x[1], ch.seat_z, 390, 'chair', 'fill-opacity=".35"'));
    o.push(rect(-ch.side_inner_x, ch.side_inner_x, 0, ch.seat_z, 'chair'));
    for (const s of [1, -1]) {
      o.push(rect(s * ch.armrest.x[0], s * ch.armrest.x[1], 0, ch.armrest.z[1], 'chair'));
      o.push(rect(s * ch.wing.x[0], s * ch.wing.x[1], ch.wing.z[0], ch.wing.z[1], 'chair', 'fill-opacity=".8"'));
    }
    o.push(`<text x="${-ch.wing.x[1]}" y="${Y(ch.wing.z[1]) - 4}" class="mu sm">wing</text><text x="${ch.armrest.x[0] + 3}" y="${Y(60)}" class="mu sm">armrest</text><text x="-88" y="${Y(392)}" class="mu sm">backrest (behind)</text>`);
    // child silhouette
    const az = c.axilla_z, shz = ch.seat_z + c.msh, hb = c.B / 2;
    const P = [[c.hip / 2, ch.seat_z], [c.hip / 2 - 4, ch.seat_z + 40], [c.waist / 2, ch.seat_z + 0.42 * c.ax], [hb, az - 55], [hb, az], [c.sh / 2 - 16, shz - 8], [c.sh / 2 - 30, shz + 4], [30, shz + 14], [26, shz + 34]];
    const side = s => P.map(([x, z]) => `${f(s * x)},${f(Y(z))}`);
    const left = side(1), right = side(-1).reverse();
    o.push(`<polygon class="child" points="${left.join(' ')} ${right.join(' ')}"><title>${esc(S.kid)}: chest breadth ${c.B}, bideltoid ${c.sh}, seated hip ${c.hip} mm</title></polygon>`);
    const hr = c.msh > 340 ? 72 : 67, hz = shz + 34 + 82;
    o.push(`<ellipse class="child" cx="0" cy="${Y(hz)}" rx="${hr}" ry="88"><title>head (schematic)</title></ellipse>`);
    // upper arms (hang in front of the pads)
    for (const s of [1, -1]) {
      const xi = hb + 2, xo = c.sh / 2 + 4, z0 = az - 115;
      o.push(`<path class="arm" d="M${f(s * xi)},${Y(az + 4)} L${f(s * (c.sh / 2 - 14))},${Y(shz - 6)} Q${f(s * xo)},${Y(shz - 10)} ${f(s * xo)},${Y(shz - 40)} L${f(s * (xo - 3))},${Y(z0)} L${f(s * xi)},${Y(z0)} Z"/>`);
    }
    // pad-top zone & axilla line
    o.push(rect(-236, 175, az - D.rule.top_below_max, az - D.rule.top_below_min, 'zone'));
    o.push(`<line class="axl" x1="-236" x2="180" y1="${Y(az)}" y2="${Y(az)}"/><text x="-236" y="${Y(az) - 3}">axilla z ${az}</text><text x="-236" y="${Y(az - 25) + 10}" class="mu sm">pad-top zone</text><text x="-236" y="${Y(az - 25) + 20}" class="mu sm">25–50 below</text>`);
    // ghosts: all heights (left) at the selected left depth; all depth faces at the selected left height
    const xsL = DP[S.dL].back_x, xcL = contact(S.dL, S.foam);
    for (const h of D.heights) {
      if (h.id === S.hL) continue;
      o.push(rect(xcL, xsL, h.z - 40, h.z + 40, 'ghost'));
      o.push(`<text x="171" y="${Y(h.z) + 3}" class="mu sm">${h.id}</text>`);
    }
    o.push(`<text x="171" y="${Y(H[S.hL].z) + 3}" class="sm" font-weight="700">${S.hL}</text>`);
    const hz0 = H[S.hL].z;
    D.depths.forEach((d, i) => {
      const x = d.contact[S.foam];
      o.push(`<line class="ghostw" x1="${x}" x2="${x}" y1="${Y(hz0 + 46)}" y2="${Y(hz0 - 46)}"><title>${d.id}: foam face x ${x} (free width ${d.free_width[S.foam]} with ${S.foam} mm foam)</title></line>`);
      o.push(`<text x="${x}" y="${Y(hz0 + 50 + (i % 2) * 9)}" class="mu sm" text-anchor="middle">${d.id}</text>`);
    });
    // selected pads (solid) both sides
    const t = T[S.tilt];
    const pad = (s, hId, dId) => {
      const zc = H[hId].z, xs = DP[dId].back_x, xc = contact(dId, S.foam), xf = xs - D.pad.shell;
      const g = [];
      const nose = [ch.side_inner_x, ch.wing.x[1], 207, 247];
      // carrier arm (thick stroke) from the nose to the arm end, then the depth piece to the pad back
      const pth = `M${s * 147},${Y(227)} C${s * 140},${Y(227)} ${s * 132},${Y(zc)} ${s * ch.arm_end_x},${Y(zc)} L${s * (xs + 1)},${Y(zc)}`;
      g.push(`<path d="${pth}" fill="none" stroke="var(--ink)" stroke-width="19" stroke-linecap="round"/><path d="${pth}" fill="none" stroke="var(--r5kit)" stroke-width="17.4" stroke-linecap="round"/>`);
      g.push(rect(s * nose[0], s * nose[1], nose[2], nose[3], 'shell'));
      const rot = s * (-t.deg); // + = clockwise on screen; left top-in rotates the top toward −x
      g.push(`<g transform="rotate(${rot} ${s * xs} ${Y(zc)})"><title>${s > 0 ? 'left' : 'right'} pad: ${hId} (centre z ${zc}), ${dId} (back x ${xs}), foam ${S.foam} mm, ${t.label}; foam face x ${f1(xc)}</title>`
        + rect(s * xc, s * xf, zc - 40, zc + 40, 'foam', 'rx="7"') + rect(s * xf, s * xs, zc - 42, zc + 42, 'shell', 'rx="1.5"') + '</g>');
      return g.join('');
    };
    o.push(pad(1, S.hL, S.dL), pad(-1, S.hR, S.dR));
    // dimension: free width at the pad centre
    const cl = sideCheck(S.hL, S.dL), cr = sideCheck(S.hR, S.dR);
    const zd = Math.min(H[S.hL].z, H[S.hR].z) - 52;
    o.push(`<line x1="${-cr.xc - cr.inward}" x2="${cl.xc + cl.inward}" y1="${Y(zd)}" y2="${Y(zd)}" stroke="var(--ink)" stroke-width=".8" marker-start="url(#r5ar)" marker-end="url(#r5ar)"/>
      <text x="0" y="${Y(zd) + 12}" text-anchor="middle" class="sm">free width ${f1(cl.xc + cl.inward + cr.xc + cr.inward)} · chest ${c.B}</text>`);
    // seat line + scale bar
    o.push(`<text x="-236" y="${Y(ch.seat_z) + 3}" class="mu sm">seat z ${ch.seat_z}</text>`);
    o.push(`<line x1="-236" x2="-186" y1="${Y(-6)}" y2="${Y(-6)}" stroke="var(--ink)" stroke-width="1.2"/><text x="-181" y="${Y(-6) + 3}" class="sm">50 mm</text>`);
    o.push(`<text x="0" y="${Y(-6) + 3}" text-anchor="middle" class="mu sm">child's right (−x) · front view · child's left (+x)</text>`);
    svg.innerHTML = `<defs><marker id="r5ar" viewBox="0 0 6 6" refX="3" refY="3" markerWidth="5" markerHeight="5" orient="auto-start-reverse"><path d="M0,0 L6,3 L0,6 z" fill="var(--ink)"/></marker></defs>` + o.join('');
  }

  // ---------------- check readout ----------------
  function drawChecks() {
    const c = D.children[S.kid], t = T[S.tilt];
    const L = sideCheck(S.hL, S.dL), R = sideCheck(S.hR, S.dR);
    const st = ([k, txt]) => `<span class="st ${k}">${esc(txt)}</span>`;
    const row = (lab, a, b) => `<tr><th>${lab}</th><td>${a}</td><td>${b}</td></tr>`;
    $('#r5chk').innerHTML = `<table class="chk"><tr><th></th><th>Left (+x)</th><th>Right (−x)</th></tr>
      ${row('Setting', `${S.hL} · ${S.dL}`, `${S.hR} · ${S.dR}`)}
      ${row('Pad top below armpit', `${f1(L.below)} mm<br>${st(L.hs)}`, `${f1(R.below)} mm<br>${st(R.hs)}`)}
      ${row('Gap to chest', `${sgn(L.gap)} mm<br>${st(L.gs)}`, `${sgn(R.gap)} mm<br>${st(R.gs)}`)}
      ${row('Contact centre z', f1(L.cz), f1(R.cz))}
      ${row('Tilt', `${esc(t.label)}${t.deg ? `: top edge ${sgn(t.top_dz)} mm in z; nearest edge ${f1(-L.inward)} mm closer` : ''}`, 'mirrored')}
      </table>
      <p class="m" style="font-size:12px">${esc(S.kid)}: axilla z ${c.axilla_z}, chest breadth ${c.B} (half ${c.half_chest}). Rule: pad top 25–50 mm below the axilla${cite(['aci4', 'pp_meas'])}. Gap + = air, − = foam squashed; child assumed centred. Free width ${f1(L.xc + L.inward + R.xc + R.inward)} mm.</p>`;
  }

  // ---------------- (b) range charts ----------------
  function axis(o, x0, x1, sx, yTop, yBot, step, lab) {
    for (let v = Math.ceil(x0 / step) * step; v <= x1; v += step) {
      o.push(`<line class="grid" x1="${sx(v)}" x2="${sx(v)}" y1="${yTop}" y2="${yBot}"/><text x="${sx(v)}" y="${yBot + 14}" text-anchor="middle" class="mu">${v}</text>`);
    }
    o.push(`<text x="${sx((x0 + x1) / 2)}" y="${yBot + 30}" text-anchor="middle" class="mu">${lab}</text>`);
  }
  function drawHeightChart() {
    const svg = $('#r5hc'), W = 520, L0 = 60, R0 = 14, rowH = 34, top = 30;
    const z0 = 130, z1 = 320, sx = z => L0 + (z - z0) / (z1 - z0) * (W - L0 - R0);
    const Hh = top + kids.length * rowH + 40;
    svg.setAttribute('viewBox', `0 0 ${W} ${Hh}`);
    const o = []; axis(o, z0, z1, sx, top - 8, top + kids.length * rowH, 20, 'pad centre z (mm) · pad top = centre + 40');
    D.heights.forEach(h => {
      o.push(`<line class="tick" x1="${sx(h.z)}" x2="${sx(h.z)}" y1="${top - 12}" y2="${top + kids.length * rowH}" stroke-opacity=".55" stroke-width="1.2"/><text x="${sx(h.z)}" y="${top - 16}" text-anchor="middle" font-weight="600">${h.id}</text>`);
    });
    kids.forEach((k, i) => {
      const c = D.children[k], y = top + i * rowH + 8, sel = k === S.kid;
      o.push(`<text x="${L0 - 8}" y="${y + 13}" text-anchor="end" ${sel ? 'font-weight="700"' : ''}>${k}</text>`);
      o.push(`<rect class="need${sel ? ' sel' : ''}" x="${sx(c.centre_window[0])}" y="${y + 2}" width="${sx(c.centre_window[1]) - sx(c.centre_window[0])}" height="16" rx="4"><title>${k}: axilla z ${c.axilla_z}; pad centre ${c.centre_window[0]}–${c.centre_window[1]} (top ${c.centre_window[0] + 40}–${c.centre_window[1] + 40}). Fits ${c.fits_heights.join(', ') || 'none'}</title></rect>`);
      D.heights.forEach(h => {
        const hit = c.fits_heights.includes(h.id);
        if (hit) o.push(`<circle class="hit" cx="${sx(h.z)}" cy="${y + 10}" r="5" stroke="var(--panel)" stroke-width="2"><title>${h.id} fits ${k}</title></circle><text x="${sx(h.z) + 8}" y="${y + 14}" class="sm">${h.id} fits</text>`);
      });
    });
    svg.innerHTML = o.join('');
  }
  function drawWidthChart() {
    const svg = $('#r5wc'), W = 460, L0 = 76, R0 = 12, rowH = 32, top = 18;
    const w0 = 30, w1 = 220, sx = w => L0 + (w - w0) / (w1 - w0) * (W - L0 - R0);
    const rows = kids.length + D.pad.foams.length;
    const yEnd = top + rows * rowH + 10, Hh = yEnd + 40;
    svg.setAttribute('viewBox', `0 0 ${W} ${Hh}`);
    const o = []; axis(o, w0, w1, sx, top - 6, yEnd, 20, 'free width between foam faces (mm), child centred');
    // guides from the selected foam's D0–D3 up through the child rows
    D.depths.forEach(d => { const x = sx(d.free_width[S.foam]); o.push(`<line class="guide" x1="${x}" x2="${x}" y1="${top - 4}" y2="${yEnd - 4}"/>`); });
    kids.forEach((k, i) => {
      const c = D.children[k], y = top + i * rowH, sel = k === S.kid;
      o.push(`<text x="${L0 - 8}" y="${y + 13}" text-anchor="end" ${sel ? 'font-weight="700"' : ''}>${k}</text>`);
      o.push(`<rect class="need${sel ? ' sel' : ''}" x="${sx(c.need_free_width[0])}" y="${y + 2}" width="${sx(c.need_free_width[1]) - sx(c.need_free_width[0])}" height="15" rx="4"><title>${k}: chest breadth ${c.B}; needs free width ${c.need_free_width[0]}–${c.need_free_width[1]}. Best kit: ${c.best_width.depth} + ${c.best_width.foam} mm foam, gap ${c.best_width.gap} per side (${c.best_width.status})</title></rect>`);
      o.push(`<line class="tick" x1="${sx(c.B)}" x2="${sx(c.B)}" y1="${y}" y2="${y + 19}"/>`);
    });
    D.pad.foams.forEach((fm, j) => {
      const y = top + (kids.length + j) * rowH + 8, fk = String(fm), sel = fk === S.foam;
      o.push(`<text x="${L0 - 8}" y="${y + 9}" text-anchor="end" class="${sel ? '' : 'mu'}" ${sel ? 'font-weight="700"' : ''}>kit, ${fm} foam</text>`);
      D.depths.forEach((d, i) => {
        const x = sx(d.free_width[fk]);
        o.push(`<line class="tick${sel ? '' : ' mu2'}" x1="${x}" x2="${x}" y1="${y - 2}" y2="${y + 12}"><title>${d.id} with ${fm} mm foam: free width ${d.free_width[fk]} (foam face x ±${d.contact[fk]})</title></line><text x="${x}" y="${y + 23}" text-anchor="middle" class="sm ${sel ? '' : 'mu'}">${d.id}</text>`);
      });
    });
    svg.innerHTML = o.join('');
  }

  update();
}
