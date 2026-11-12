# "Why the pommel is low over the screws": side + top sketches of a small (2 y P5) and big (5 y P95) child on GbG_chair_forth
# with the real v0.7 pommel outlines (S23A, K45A, recess version). Chair coordinates in mm (-y = front, z up, seat top z 20).
import html, math, os

OUT = os.path.join(os.path.dirname(__file__), "..", "body_measurements_reference.html")

BACK_Y, SEAT_Z, EDGE_Y, FACE_Y = 81, 20, -86, -96      # backrest plane, seat top, end of flat pan, pan front face
F, Y0 = 14, -93; HF = Y0 - 6                             # recess floor, hook inner / outer face

KIDS = {
    "small": dict(title="Small child: 2 years, 5th percentile", bkl=250, hipbr=166, thigh=62, H=73, hjc=56, femur=172, wk=46,
                  thighB=70, kneeBack=228, groin=80, nogo=145, size="S23A", gap0=28, gap5=58, gap10=87),
    "big": dict(title="Big child: 5 years, 95th percentile", bkl=382, hipbr=235, thigh=109, H=103, hjc=83, femur=264, wk=75,
                thighB=107, kneeBack=347, groin=125, nogo=170, size="K45A", gap0=29, gap5=75, gap10=120),
}
POM = {  # code: long?, s0, L, H, wR, wF   (from sizeSpec in gbg_pommel_v07.fs)
    "S23A": (False, 0, 75, 60, 58, 70),
    "K45A": (True, 85, 70, 78, 68, 80),
}

def pommel(code):
    lng, s0, L, Hh, wR, wF = POM[code]
    zT = 20 + Hh
    a = 96 + s0
    b = a + L if lng else 96 + L
    top = [(-36, F + 7.5), (-55, F + 8.5), (-75, F + 11), (Y0 + 2, 29), (HF, 30)]
    if lng:
        top += [(HF - 10, 25), (-(a - 22), 21), (-(a - 4), 26), (-(a + 12), zT - 12), (-(a + 28), zT), (-(b - 14), zT - 1), (-b, zT - 22)]
    else:
        top += [(HF - 10, 42), (-124, zT - 10), (-140, zT), (-(b - 13), zT - 2), (-b, zT - 22)]
    under = [(-b, 8), (-(b - 8), 0), (Y0, 0), (Y0, F - 1), (Y0 + 1, F), (-36, F)]
    half = [(15, -36), (27.5, (-36 + Y0) / 2), (40, Y0), (37, HF - 2)]
    if lng:
        half += [(18, HF - 16), (18, -(a - 14)), (wR / 2, -(a + 6)), (wF / 2, -(b - 12))]
    else:
        half += [(max(wR / 2, 30), HF - 14), (wF / 2, -(b - 12))]
    return dict(top=top, under=under, half=half, b=b, a=a, zT=zT, lng=lng, wR=wR, wF=wF, H=Hh)

class V:
    """Maps chair coords to SVG. side: horizontal = -y (front to the right), vertical = z. plan: horizontal = x, vertical = -y (front down)."""
    def __init__(self, kind, u0, u1, v0, v1, s, pad=34):
        self.kind, self.u0, self.u1, self.v0, self.v1, self.s, self.pad = kind, u0, u1, v0, v1, s, pad
        self.w = (u1 - u0) * s + 2 * pad
        self.h = (v1 - v0) * s + 2 * pad
        self.out = []
    def P(self, y_or_x, z_or_y):
        if self.kind == "side":
            u, v = -y_or_x, z_or_y
            return (self.pad + (u - self.u0) * self.s, self.pad + (self.v1 - v) * self.s)
        u, v = y_or_x, -z_or_y
        return (self.pad + (u - self.u0) * self.s, self.pad + (v - self.v0) * self.s)
    def add(self, s):
        self.out.append(s)

def fmt(p):
    return f"{p[0]:.1f},{p[1]:.1f}"

def smooth(v, pts):
    """Catmull-Rom through pts -> cubic Bezier path segments (no M)."""
    P = [v.P(*p) for p in pts]
    d = ""
    for i in range(len(P) - 1):
        p0 = P[i - 1] if i > 0 else P[i]
        p1, p2 = P[i], P[i + 1]
        p3 = P[i + 2] if i + 2 < len(P) else p2
        c1 = (p1[0] + (p2[0] - p0[0]) / 6, p1[1] + (p2[1] - p0[1]) / 6)
        c2 = (p2[0] - (p3[0] - p1[0]) / 6, p2[1] - (p3[1] - p1[1]) / 6)
        d += f" C{fmt(c1)} {fmt(c2)} {fmt(p2)}"
    return d

def poly(v, pts, cls):
    v.add(f'<polygon class="{cls}" points="{" ".join(fmt(v.P(*p)) for p in pts)}"/>')

def line(v, a, b, cls):
    A, B = v.P(*a), v.P(*b)
    v.add(f'<line class="{cls}" x1="{A[0]:.1f}" y1="{A[1]:.1f}" x2="{B[0]:.1f}" y2="{B[1]:.1f}"/>')

def text(v, p, t, cls="lbl", anchor="start", dy=0):
    P = v.P(*p)
    v.add(f'<text class="{cls}" x="{P[0]:.1f}" y="{P[1] + dy:.1f}" text-anchor="{anchor}">{html.escape(t)}</text>')

def dot(v, p, cls="pt", r=3.5):
    P = v.P(*p)
    v.add(f'<circle class="{cls}" cx="{P[0]:.1f}" cy="{P[1]:.1f}" r="{r}"/>')

def dim_h(v, y1, y2, z, label, cls="dim"):
    A, B = v.P(y1, z), v.P(y2, z)
    v.add(f'<line class="{cls}" x1="{A[0]:.1f}" y1="{A[1]:.1f}" x2="{B[0]:.1f}" y2="{B[1]:.1f}" marker-start="url(#ar)" marker-end="url(#ar)"/>')
    v.add(f'<text class="dimt" x="{(A[0] + B[0]) / 2:.1f}" y="{A[1] - 5:.1f}" text-anchor="middle">{html.escape(label)}</text>')

def dim_v(v, y, z1, z2, label, cls="dim", side="right"):
    A, B = v.P(y, z1), v.P(y, z2)
    v.add(f'<line class="{cls}" x1="{A[0]:.1f}" y1="{A[1]:.1f}" x2="{B[0]:.1f}" y2="{B[1]:.1f}" marker-start="url(#ar)" marker-end="url(#ar)"/>')
    dx = 6 if side == "right" else -6
    v.add(f'<text class="dimt" x="{A[0] + dx:.1f}" y="{(A[1] + B[1]) / 2 + 4:.1f}" text-anchor="{"start" if side == "right" else "end"}">{html.escape(label)}</text>')

def svg(v):
    defs = ('<defs><marker id="ar" viewBox="0 0 10 10" refX="5" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse">'
            '<path d="M0,1 L9,5 L0,9 z" class="arh"/></marker>'
            '<pattern id="hatch" width="7" height="7" patternUnits="userSpaceOnUse" patternTransform="rotate(45)"><line x1="0" y1="0" x2="0" y2="7" class="hl"/></pattern></defs>')
    return f'<svg viewBox="0 0 {v.w:.0f} {v.h:.0f}" width="100%" role="img">{defs}{"".join(v.out)}</svg>'

def side_view(k):
    K, pm = KIDS[k], pommel(KIDS[k]["size"])
    kneeY = BACK_Y - K["kneeBack"]
    v = V("side", -BACK_Y - 25, max(-kneeY, pm["b"]) + 60, -105, SEAT_Z + 175, 1.55 if k == "small" else 1.25)
    # chair: pan (with recess), backrest
    seat = [(BACK_Y, 0), (FACE_Y, 0), (FACE_Y, 10), (EDGE_Y - 2, SEAT_Z), (BACK_Y, SEAT_Z)]
    poly(v, seat, "chair")
    poly(v, [(BACK_Y, 0), (BACK_Y + 14, 0), (BACK_Y + 14, SEAT_Z + 165), (BACK_Y, SEAT_Z + 165)], "chair")
    text(v, (BACK_Y + 7, SEAT_Z + 168), "backrest", "lbl", "middle")
    # groin keep-low zone
    gY, nY = BACK_Y - K["groin"], BACK_Y - K["nogo"]
    poly(v, [(BACK_Y - 30, SEAT_Z), (nY, SEAT_Z), (nY, SEAT_Z + 10), (BACK_Y - 30, SEAT_Z + 10)], "nogo")
    line(v, (nY, -4), (nY, SEAT_Z + 30), "nogol")
    text(v, (nY, -12), f"keep-low line: {K['nogo']} mm from backrest", "warn", "middle")
    # pommel (real outline)
    d = "M" + fmt(v.P(*pm["top"][0])) + smooth(v, pm["top"]) + "".join(" L" + fmt(v.P(*p)) for p in pm["under"]) + " Z"
    v.add(f'<path class="pom" d="{d}"/>')
    for by in (-45, -60, -75):
        line(v, (by, 2), (by, F + 9), "bolt")
    if pm["lng"]:
        line(v, (-36, 7), (-(pm["b"] - 15), 7), "rod")
    # near-side leg drawn over the pommel, see-through (the pommel is between the legs)
    T = K["thigh"]
    hipY = BACK_Y - K["hjc"]
    knee_r = K["wk"] * 0.48
    torso = [(BACK_Y - 4, SEAT_Z + 6), (BACK_Y - 4, SEAT_Z + 160), (BACK_Y - K["hjc"] * 1.7, SEAT_Z + 160), (BACK_Y - K["hjc"] * 1.6, SEAT_Z + T + 10)]
    poly(v, torso, "skin2")
    d = "M" + fmt(v.P(BACK_Y - 4, SEAT_Z + T * 0.55))
    d += smooth(v, [(BACK_Y - 4, SEAT_Z + T * 0.55), (BACK_Y - 12, SEAT_Z + 4), (BACK_Y - 40, SEAT_Z)])
    d += " L" + fmt(v.P(kneeY + knee_r * 0.3, SEAT_Z + 2))
    d += smooth(v, [(kneeY + knee_r * 0.3, SEAT_Z + 2), (kneeY - knee_r * 0.9, SEAT_Z + T * 0.42), (kneeY - knee_r * 0.2, SEAT_Z + T * 0.85)])
    d += smooth(v, [(kneeY - knee_r * 0.2, SEAT_Z + T * 0.85), (kneeY + 25, SEAT_Z + T * 0.98), (hipY - 10, SEAT_Z + T + 2), (BACK_Y - K["hjc"] * 1.6, SEAT_Z + T + 10)])
    d += " Z"
    v.add(f'<path class="skin" d="{d}"/>')
    shank = [(kneeY - knee_r * 0.8, SEAT_Z + T * 0.45), (kneeY + knee_r * 0.6, SEAT_Z + T * 0.35), (kneeY + knee_r * 0.2, -100), (kneeY - knee_r * 1.1, -100)]
    poly(v, shank, "skin")
    # points
    dot(v, (hipY, SEAT_Z + T * 0.5), "pt")
    text(v, (hipY, SEAT_Z + T * 0.5), "hip joint", "lbl", "middle", dy=-9)
    dot(v, (kneeY, SEAT_Z + T * 0.45), "pt")
    text(v, (kneeY, SEAT_Z + T * 0.45 + 4), "knee", "lbl", "middle", dy=-12)
    dot(v, (gY, SEAT_Z + 4), "ptw")
    text(v, (gY, SEAT_Z + 4), "groin", "warn", "middle", dy=18)
    # dims
    dim_h(v, BACK_Y, kneeY, -48, f"backrest → knee centre {K['kneeBack']} mm")
    dim_h(v, FACE_Y, kneeY, -70, f"knee sits {FACE_Y - kneeY:.0f} mm past the seat edge")
    dim_h(v, FACE_Y, -pm["b"], -92, f"pommel reaches {pm['b'] + FACE_Y:.0f} mm past the edge", "dimp")
    dim_v(v, -(pm["b"] - 30), SEAT_Z, pm["zT"], f"pad {pm['H']} mm tall", "dimp", "left")
    dim_v(v, BACK_Y - K["kneeBack"] * 0.72, SEAT_Z, SEAT_Z + T, f"thigh {T}", "dim", "left")
    text(v, (-55, -30), "saddle over the 3 screws: 1.5–5 mm above the seat", "lblp", "middle")
    line(v, (-55, -24), (-55, SEAT_Z - 2), "lead")
    return svg(v)

def plan_view(k):
    K, pm = KIDS[k], pommel(KIDS[k]["size"])
    kneeY = BACK_Y - K["kneeBack"]
    v = V("plan", -150, 150, -BACK_Y - 20, max(-kneeY + K["wk"] * 0.6, pm["b"]) + 30, 1.25 if k == "small" else 1.05)
    # seat (pan) outline
    poly(v, [(-125, BACK_Y), (125, BACK_Y), (125, FACE_Y + 8), (117, FACE_Y), (-117, FACE_Y), (-125, FACE_Y + 8)], "seatp")
    poly(v, [(-125, BACK_Y), (125, BACK_Y), (125, BACK_Y + 14), (-125, BACK_Y + 14)], "chair")
    text(v, (0, BACK_Y + 7), "backrest", "lbl", "middle", dy=4)
    text(v, (118, FACE_Y), "seat edge", "lbl", "end", dy=14)
    # groin keep-low zone
    gY, nY = BACK_Y - K["groin"], BACK_Y - K["nogo"]
    poly(v, [(-22, BACK_Y - 30), (22, BACK_Y - 30), (22, nY), (-22, nY)], "nogo")
    # thighs at ~5 deg natural abduction (translucent, so the pommel shows through)
    for s in (-1, 1):
        hx, hy = s * K["H"] / 2, BACK_Y - K["hjc"]
        ang = math.radians(5)
        kx, ky = hx + s * K["femur"] * math.sin(ang), hy - K["femur"] * math.cos(ang)
        kx, ky = kx, min(ky, kneeY)
        w0, w1 = K["thighB"] / 2, K["wk"] / 2
        nx, ny = math.cos(ang), s * math.sin(ang)
        pts = [(hx - w0, hy + 25), (hx + w0, hy + 25), (kx + w1, ky - K["wk"] * 0.35), (kx - w1, ky - K["wk"] * 0.35)]
        poly(v, pts, "skin")
        dot(v, (kx, ky), "pt")
    v.add(f'<ellipse class="skin2" cx="{v.P(0, BACK_Y - K["hjc"] * 0.6)[0]:.1f}" cy="{v.P(0, BACK_Y - K["hjc"] * 0.6)[1]:.1f}" rx="{K["hipbr"] / 2 * v.s:.1f}" ry="{K["hjc"] * 0.95 * v.s:.1f}"/>')
    # pommel plan (real outline)
    half = pm["half"]
    mir = [(-x, y) for (x, y) in reversed(half)]
    d = "M" + fmt(v.P(*half[0])) + smooth(v, half)
    # front arc through (0, -b)
    E, M2 = v.P(*half[-1]), v.P(-half[-1][0], half[-1][1])
    w = half[-1][0]
    R = (w * w + 144) / 24 * v.s
    d += f" A{R:.1f},{R:.1f} 0 0 1 {fmt(M2)}"
    d += smooth(v, mir) + " Z"
    v.add(f'<path class="pom" d="{d}"/>')
    if pm["lng"]:
        for s in (-1, 1):
            line(v, (s * 12, -36), (s * 12, -(pm["b"] - 15)), "rod")
    for by in (-45, -60, -75):
        dot(v, (0, by), "boltp", 2.6)
    dot(v, (0, gY), "ptw")
    text(v, (26, gY), "groin", "warn", "start", dy=4)
    line(v, (-60, nY), (60, nY), "nogol")
    text(v, (-62, nY), "keep ≤10 mm tall", "warn", "end", dy=4)
    text(v, (0, -pm["b"]), f"{pm['wR']}→{pm['wF']} mm wide", "lblp", "middle", dy=18)
    dim_h(v, -K["H"] / 2, K["H"] / 2, BACK_Y - K["hjc"] + 8, f"hip joints {K['H']} apart")
    return svg(v)

CSS = """
:root{--ink:#1f2328;--mut:#5b636e;--bg:#fbfaf7;--card:#fff;--line:#d9d6cf;--chair:#c9b9a4;--pom:#2f7fc1;--skin:#e7b48f;--warn:#c2410c}
@media (prefers-color-scheme:dark){:root{--ink:#e8e6e1;--mut:#a9a49b;--bg:#16181b;--card:#1e2125;--line:#3a3d42;--chair:#6b5c4a;--pom:#5aa6e6;--skin:#c48b66;--warn:#fb8a4c}}
*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--ink);font:16px/1.55 system-ui,-apple-system,Segoe UI,Roboto,sans-serif}
main{max-width:1180px;margin:0 auto;padding:28px 20px 60px}h1{font-size:28px;margin:0 0 6px}h2{font-size:21px;margin:34px 0 8px}h3{font-size:16px;margin:14px 0 4px;color:var(--mut)}
p{margin:6px 0 10px;max-width:78ch}.lead{color:var(--mut);font-size:17px}
.row{display:grid;grid-template-columns:1.35fr 1fr;gap:16px}@media (max-width:900px){.row{grid-template-columns:1fr}}
.card{background:var(--card);border:1px solid var(--line);border-radius:10px;padding:12px 14px}
.card .cap{font-size:13px;color:var(--mut);margin-top:6px}
table{border-collapse:collapse;width:100%;font-size:14px}th,td{border-bottom:1px solid var(--line);padding:6px 8px;text-align:right}th:first-child,td:first-child{text-align:left}thead th{color:var(--mut);font-weight:600}
ol li,ul li{margin:4px 0;max-width:80ch}
.chair{fill:var(--chair);stroke:#8a7a66;stroke-width:1}.seatp{fill:var(--chair);fill-opacity:.35;stroke:#8a7a66;stroke-width:1}
.pom{fill:var(--pom);fill-opacity:.85;stroke:#1b4f7a;stroke-width:1.2}
.skin{fill:var(--skin);fill-opacity:.55;stroke:#a86b45;stroke-width:1}.skin2{fill:var(--skin);fill-opacity:.3;stroke:#a86b45;stroke-width:.8;stroke-dasharray:3 3}
.nogo{fill:url(#hatch);stroke:var(--warn);stroke-width:1}.hl{stroke:var(--warn);stroke-width:2;opacity:.55}
.nogol{stroke:var(--warn);stroke-width:1.6;stroke-dasharray:6 4}.lead{stroke:var(--mut);stroke-width:.8}
.bolt{stroke:#333;stroke-width:2}.boltp{fill:#222}.rod{stroke:#111;stroke-width:3;opacity:.75}
.pt{fill:var(--ink)}.ptw{fill:var(--warn)}
.lbl{font-size:12px;fill:var(--ink)}.lblp{font-size:12px;fill:var(--pom);font-weight:600}.warn{font-size:12px;fill:var(--warn);font-weight:600}
.dim{stroke:var(--mut);stroke-width:1}.dimp{stroke:var(--pom);stroke-width:1.2}.dimt{font-size:11.5px;fill:var(--mut)}.arh{fill:var(--mut)}
.key{display:flex;gap:16px;flex-wrap:wrap;font-size:13px;color:var(--mut);margin:6px 0 0}.key span:before{content:"";display:inline-block;width:12px;height:12px;border-radius:3px;margin-right:6px;vertical-align:-2px;background:var(--c)}
"""

def page():
    rows = []
    for k in ("small", "big"):
        K = KIDS[k]; pm = pommel(K["size"])
        rows.append(f"""<h2>{K['title']}, with pommel {K['size']}</h2>
<div class="row"><div class="card">{side_view(k)}<div class="cap">Side view, real {K['size']} outline from the CAD. Seat recess version. Edges in the part are rounded 10 mm (not drawn).</div></div>
<div class="card">{plan_view(k)}<div class="cap">Top view. Thighs at about 5° natural abduction, drawn see-through so the pommel shows. Black dots on the pommel = the 3 seat screws.</div></div></div>""")
    S, B = KIDS["small"], KIDS["big"]
    tbl = f"""<table><thead><tr><th>Measurement (seated)</th><th>Small (2 y, P5)</th><th>Big (5 y, P95)</th></tr></thead><tbody>
<tr><td>Buttock to back of knee... buttock–knee length</td><td>{S['bkl']}</td><td>{B['bkl']}</td></tr>
<tr><td>Backrest to knee centre</td><td>{S['kneeBack']}</td><td>{B['kneeBack']}</td></tr>
<tr><td>Knee centre past the seat front</td><td>{S['kneeBack'] - (BACK_Y - FACE_Y)}</td><td>{B['kneeBack'] - (BACK_Y - FACE_Y)}</td></tr>
<tr><td>Groin, from the backrest (estimate)</td><td>~{S['groin']}</td><td>~{B['groin']}</td></tr>
<tr><td>"Keep low" line, from the backrest</td><td>{S['nogo']}</td><td>{B['nogo']}</td></tr>
<tr><td>Thigh thickness (thigh clearance)</td><td>{S['thigh']}</td><td>{B['thigh']}</td></tr>
<tr><td>Hip breadth, seated</td><td>{S['hipbr']}</td><td>{B['hipbr']}</td></tr>
<tr><td>Hip joints, centre to centre</td><td>{S['H']}</td><td>{B['H']}</td></tr>
<tr><td>Knee width</td><td>{S['wk']}</td><td>{B['wk']}</td></tr>
<tr><td>Gap between the knees: legs straight / 5° / 10° apart</td><td>{S['gap0']} / {S['gap5']} / {S['gap10']}</td><td>{B['gap0']} / {B['gap5']} / {B['gap10']}</td></tr>
<tr><td>Pommel used, width at thigh → knee</td><td>{S['size']}: 58 → 70</td><td>{B['size']}: 68 → 80</td></tr>
</tbody></table>"""
    tbl = tbl.replace("Buttock to back of knee... buttock–knee length", "Buttock–knee length (back of buttock to front of knee)")
    return f"""<!DOCTYPE html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>GbG pommel: why it is low over the screws</title><style>{CSS}</style></head><body><main>
<h1>Why the pommel is low over the 3 screws</h1>
<p class="lead">The tall part of the pommel sits where the <b>knees</b> are, not where the thighs meet the body. The part over the screws is only a flat anchor plate. These sketches put a small and a big child on the chair with the real CAD outlines so you can see where everything lands.</p>
<div class="key"><span style="--c:var(--pom)">pommel (CAD outline)</span><span style="--c:var(--chair)">chair seat / backrest</span><span style="--c:var(--skin)">child (approximate)</span><span style="--c:var(--warn)">groin zone: keep low</span></div>
{''.join(rows)}
<h2>Why not make it tall over the screws too?</h2>
<ol>
<li><b>That spot is the groin.</b> Seated against the backrest, the groin is about 80 mm (small child) to 125 mm (big child) from the backrest, which is right above the saddle and its screws. Seating guidance is clear that a pommel ("medial knee support") must sit clear of the groin, because the groin is not built to carry load. A tall block there presses on the genitals and pubic bone, causes pain and pressure marks, and gets dirty.</li>
<li><b>It works at the knees, not at the hips.</b> The thighs start together at the groin and spread toward the knees, so the gap between them is almost nothing near the body and largest at the knees (58–75 mm at a natural 5°). A spacer works best where the gap is, at the far end of the thigh: a small force there holds the knees apart. A block near the groin would only squeeze soft tissue.</li>
<li><b>It must not be used to stop sliding.</b> If the child slides forward, a tall post near the groin would catch them by the crotch. Sliding is the job of the pelvic belt, not the pommel. Keeping the area low makes that mistake impossible.</li>
<li><b>Getting in and out.</b> Parents lift the child in and out over the front of the seat. A post at the groin is in the way and is a knock point during transfers and on bumps.</li>
</ol>
<p>So the design rule from the panel was: nothing taller than about <b>10 mm</b> closer to the backrest than <b>145 mm</b> (ages 2–3) or <b>170 mm</b> (ages 4–5). The saddle follows it: it is 1.5 mm above the seat at the rear screw, about 5 mm at the front screw and 10 mm at the seat edge. On the long sizes the neck also stays low (1–6 mm above the seat line) under the thighs, and only rises into the pad where the knees are.</p>
<h2>The measurements</h2>
<div class="card">{tbl}<div class="cap">Sources: NIST AnthroKids 1977 (typically developing children; children with disabilities are often smaller), Carman 2023 knee CT, and the AI panel's estimates for the groin and the keep-low line. All are estimates to confirm with a PT on a real child.</div></div>
<h2>What this means for fitting</h2>
<ul>
<li><b>Small child:</b> the knees are only about 50 mm past the seat edge, so the short S pommel's pad (0–75 mm past the edge) sits right at the knees.</li>
<li><b>Big child:</b> the knees are about 170 mm past the edge. The long K45 pad covers 85–155 mm, so it holds the inside of the lower thigh just behind the knee. For a tall child, use the reach adjust (+15 or +30 mm) to bring the pad closer to the knee.</li>
<li>If a child needs more control higher up the thigh (strong adductor tone), the safe direction is a <b>longer pad toward the seat edge</b>, still stopping before the keep-low line, rather than a taller saddle. That is a PT decision.</li>
</ul>
</main></body></html>"""

if __name__ == "__main__":
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    open(OUT, "w").write(page())
    print(os.path.abspath(OUT))
