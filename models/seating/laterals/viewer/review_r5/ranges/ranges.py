"""Round 5 side-pad ranges vs child needs -> viewer/site/ranges5.json (scratch generator)."""
import json, itertools, os
OUT = os.path.join(os.path.dirname(__file__), '..', '..', 'site', 'ranges5.json')
SEAT = 20.0; MOUNT = 227.0; PAD_H = 80.0; SHELL = 4.5; COVER = 3.0   # listed cover: 3 mm SBR neoprene (4000031372319)
# Foam from the listed Super2 EVA sheets (5 / 10 mm) and HD top foam: 5 = 1x5 EVA, 10 = 1x10 EVA, 20 = 2x10 EVA or 5 EVA + 15 HD, 30 = 5 EVA + 25 HD
FOAMS = [5.0, 10.0, 20.0, 30.0]; FOAM_STD = 20.0
# 80/20 set: ONE depth piece. The joint stack (tenon 13 + 2.35 margin + button head 2.75 + floor 6 from arm end x 128.1) caps the pad back at ~x 106,
# so a wider piece needs the arm end moved; width is set by foam thickness instead. D1-D3 dropped (they only go narrower).
DEPTH = {'D0': 104.0}
# heights: (id, z, arm, flip, screw)
H = [('H1', 172, ['L'], 'LOW', ['lower']), ('H2', 194, ['S', 'L'], 'LOW', ['lower', 'upper']),
     ('H3', 216, ['S'], 'LOW', ['upper']), ('H4', 238, ['S'], 'HIGH', ['lower']),
     ('H5', 260, ['S', 'L'], 'HIGH', ['upper', 'lower']), ('H6', 282, ['L'], 'HIGH', ['upper'])]
for _, z, arms, flip, _s in H:   # check: 227 -/+ drop +/- 11
    assert any(abs(MOUNT + (-1 if flip == 'LOW' else 1) * (22 if a == 'S' else 44) + d - z) < 1e-6 for a in arms for d in (-11, 11)), z
# children (Snyder 1977 via NIST AnthroKids; axilla-above-seat derived, chair_interfaces.md)
C = {'2y P5':  dict(ax=220, B=138, D=150, sh=220, hip=171, waist=143, msh=284, cy=34),
     '2y P50': dict(ax=248, B=160, D=160, sh=243, hip=190, waist=160, msh=313, cy=31),
     '5y P50': dict(ax=310, B=171, D=175, sh=268, hip=205, waist=178, msh=375, cy=29),
     '5y P95': dict(ax=350, B=190, D=185, sh=291, hip=230, waist=195, msh=415, cy=27)}
RULE = dict(top_below_min=25, top_below_max=50)
GAP = dict(ok=[-5, 10], warn_loose=[10, 20], warn_firm=[-12, -5])  # mm per side; + = air gap, - = foam compression
TILT = {  # tilt_out.txt §1 (left pad; dx<0 = toward the child)
 'T0':   dict(label='0°', deg=0, cdz=0.0, top_dx=0.0, top_dz=0.0, bot_dx=0.0),
 'IN8':  dict(label='8° top-in', deg=8, cdz=-4.8, top_dx=-5.2, top_dz=-5.2, bot_dx=5.6),
 'OUT8': dict(label='8° top-out', deg=-8, cdz=4.8, top_dx=5.9, top_dz=4.4, bot_dx=-5.6),
 'IN15': dict(label='15° top-in', deg=15, cdz=-8.9, top_dx=-9.2, top_dz=-10.3, bot_dx=10.4),
 'OUT15': dict(label='15° top-out', deg=-15, cdz=8.9, top_dx=11.5, top_dz=7.6, bot_dx=-10.4)}

def contact(xs, foam): return xs - SHELL - foam - COVER

def gap_status(g):
    if GAP['ok'][0] <= g <= GAP['ok'][1]: return 'ok'
    if GAP['warn_loose'][0] < g <= GAP['warn_loose'][1]: return 'loose'
    if GAP['warn_firm'][0] <= g < GAP['warn_firm'][1]: return 'firm'
    return 'too loose' if g > 0 else 'squeeze'

children = {}
for k, c in C.items():
    axz = SEAT + c['ax']
    lo, hi = axz - RULE['top_below_max'] - PAD_H / 2, axz - RULE['top_below_min'] - PAD_H / 2
    fits_h = [h[0] for h in H if lo <= h[1] <= hi]
    half = c['B'] / 2
    w = []
    for d, xs in DEPTH.items():
        for f in FOAMS:
            g = contact(xs, f) - half
            w.append(dict(depth=d, foam=f, gap=round(g, 1), status=gap_status(g)))
    okw = [r for r in w if r['status'] == 'ok']   # best = the thickest foam that still gives an OK gap (most padding), else the closest gap
    best = max(okw, key=lambda r: (r['foam'], -abs(r['gap'] - 2))) if okw else min(w, key=lambda r: abs(r['gap'] - 2))
    asym = hi - min(h[1] for h in H)
    children[k] = dict(c, axilla_z=axz, centre_window=[lo, hi], fits_heights=fits_h, half_chest=half,
                       width_options=w, best_width=best, need_free_width=[c['B'] - 10, c['B'] + 20],
                       max_LR_height_difference=asym)

# which L/R height pairs the 4-carrier kit (S-A, S-B, L-A, L-B) can make at the same time.
# Flip logic (derived, verify in CAD): A = left LOW / right HIGH, B = right LOW / left HIGH.
def carriers(side, h):
    _, z, arms, flip, _s = h
    hand = ('A' if flip == 'LOW' else 'B') if side == 'L' else ('B' if flip == 'LOW' else 'A')
    return {a + '-' + hand for a in arms}
pairs = {}
for hl in H:
    for hr in H:
        ok = any(cl != cr for cl in carriers('L', hl) for cr in carriers('R', hr))
        pairs[hl[0] + '/' + hr[0]] = ok
blocked = [k for k, v in pairs.items() if not v]

data = dict(
  title='Side pad v1.1 / Round 5: placement and padding ranges vs children aged 2–5',
  frame='Chair world mm, z up, seat surface z 20, child centreline x 0, left pad at +x. Front view, looking at the child.',
  chair=dict(seat_z=SEAT, seat_half=92, side_inner_x=134, armrest=dict(x=[134, 165], z=[0, 198]),
             wing=dict(x=[134, 161.5], z=[179, 390]), back=dict(x=[-92, 92], z=[150, 286]), mount_z=MOUNT,
             arm_end_x=128, note='Simplified from the viewer meshes (geodata.json bounding boxes): armrest inner face x ~134, wing inner line x ~134. Mesh slices (raw/ps1_J_D, ps1_RiBH): in the pad band y −27…55 the armrest is x ≥ 135 and only up to z 149.4; its z 197.8 top is the rear block (y ≥ 62), and the wing starts at y 65.5. So the depth piece never meets the armrest at any height; the limit on width is the joint stack (arm end x 128.1 − tenon 13 − margin − pad-screw head − floor 6 ⇒ pad back ≤ ~106).'),
  pad=dict(height=PAD_H, y=[-27, 55], shell=SHELL, cover=COVER, foams=FOAMS, foam_std=FOAM_STD,
           contact_formula='x_contact = pad back x − 4.5 shell − foam − 2 cover (at the pad apex, y 34, where the chest is widest; the front edge wraps 7.5 mm further in)',
           foam_note='One depth piece (D0); the child\'s width is set by the foam. Foam from the listed stock only: 5 = one 5 mm Super2 EVA sheet, 10 = one 10 mm sheet, 20 (standard) = 2 × 10 EVA laminated or 5 EVA + 15 HD top, 30 = 5 EVA + 25 HD top (padding_research build-up B). Cover 3 mm SBR neoprene (listed 4000031372319). Use: 2y P5 30 (or 20), 2y P50 20, ~3–4 y 10, 5y P50 10, 5y P95 5 (firm, thin: the price of not moving the arm end).',
           reference_cushion_check='Viewer mesh D0: cushion min x 65.5, face at apex ≈ 69.5 (30 mm incl. cover, old FS reference); formula now gives 66.5 for 30 mm foam + 3 mm cover.'),
  heights=[dict(id=h[0], z=h[1], top=h[1] + 40, bottom=h[1] - 40, arm=h[2], flip=h[3], screw=h[4],
                serves_axilla=[h[1] + 65, h[1] + 90]) for h in H],
  depths=[dict(id=d, back_x=xs, contact={str(int(f)): contact(xs, f) for f in FOAMS},
               free_width={str(int(f)): 2 * contact(xs, f) for f in FOAMS}) for d, xs in DEPTH.items()],
  tilts=TILT, rule=RULE, gap_rule=GAP,
  gap_rule_note='Manufacturers specify a "snug" fit, not a number (Spex). Working rule used here: −5…+10 mm per side = OK (light contact, room to breathe and for a T-shirt); +10…+20 = loose (child can lean before support); > +20 = no support; −5…−12 = firm preload (foam squashed, OK short term for a weak trunk); < −12 = squeeze (foam bottoms out, pressure on ribs). Not a published standard.',
  children=children,
  asym_pairs=dict(note='Each side is independent mechanically. With the 4-carrier kit (short A+B, long A+B) and the flip rule A = left LOW / right HIGH, B = right LOW / left HIGH (derived from the flip-mirror description in ROUND5_PLAN §8; verify), a few L/R pairs need two carriers of the same hand.',
                  ok=pairs, blocked=blocked),
  sources=[
   dict(id='aci4', t='NSW Agency for Clinical Innovation, Spinal seating module 4: max lateral height = seat-to-axilla − 25 mm (axillary nerve)', u='https://aci.health.nsw.gov.au/networks/spinal-cord-injury/spinal-seating/module-4/the-purpose-of-specific-body-measurements'),
   dict(id='aci7', t='NSW ACI, Spinal seating module 7: key client–wheelchair relationships; 3-point control most effective but may restrict function', u='https://aci.health.nsw.gov.au/networks/spinal-cord-injury/spinal-seating/module-7/recognise-key-relationships-between-the-client-and-the-wheelchair'),
   dict(id='pp_meas', t='Physiopedia, Wheelchair Assessment – Body Measurements: ≥ 30 mm between trunk side pad top and axilla', u='https://www.physio-pedia.com/Wheelchair_Assessment_-_Body_Measurements'),
   dict(id='pp_adapt', t='Physiopedia, Adaptive Seating for Children (SATCo / PPAS; back and trunk supports)', u='https://www.physio-pedia.com/Adaptive_Seating_for_Children'),
   dict(id='lange', t='Presperin Pedersen & Lange, Positioning Chart (6/2021, public domain): lateral trunk supports "may need to be asymmetrically placed, one lower at the apex of lateral convexity"; anterior supports for rotation; pelvis first', u='https://atk.ku.edu/sites/atk/files/documents/2003%20Workshop/positioning-chart-06-2021.pdf'),
   dict(id='satco', t='Santamaria et al. 2016, Effect of segmental trunk support on posture and reaching in children with CP (support matched to the level of trunk control)', u='https://pubmed.ncbi.nlm.nih.gov/27341576/'),
   dict(id='scire', t='SCIRE: postural implications of seating set-up; lateral trunk supports improve frontal-plane alignment (Mao 2006, level 4)', u='https://scireproject.com/evidence/wheeled-mobility-and-seating-equipment/seating-equipment-for-wheelchairs/postural-implications-of-seating-equipment-set-up/'),
   dict(id='medifab', t='Medifab/Spex knowledge base: achieving correction of postural asymmetry with lateral trunk supports; Spex "exact snug fitting"', u='https://medifab.com/knowledge-base/achieving-correction-of-postural-asymmetry-with-lateral-trunk-supports/'),
   dict(id='invacare', t='Invacare clinical poster: lateral trunk supports give 3 or 4 points of control for scoliosis', u='https://www.invacare.eu.com/sites/eu/files/csv_migration/product_docs/sales_docs/DSAL010121_Clinical%20Poster.pdf'),
   dict(id='rch_dmd', t='Royal Children\'s Hospital Melbourne, DMD wheelchair brochure: lateral trunk supports if leaning starts; consider swing-away pads for hoisting; firm back', u='https://www.rch.org.au/uploadedFiles/Main/Content/neurology/DMD_brochure_Wheelchair.pdf'),
   dict(id='nmd_delphi', t='Wheelchair seating criteria in adults with neuromuscular disease (Delphi): scoliosis 60–90 % in SMA, ~90 % in DMD without steroids', u='https://paperity.org/p/310530600/identification-of-wheelchair-seating-criteria-in-adults-with-neuromuscular-diseases-a'),
   dict(id='sb', t='Spina bifida: reduced sensation and pressure risk in wheelchair users with myelomeningocele (PMC3069254); SBH Scotland postural care', u='https://pmc.ncbi.nlm.nih.gov/articles/PMC3069254'),
   dict(id='sbh', t='SBH Scotland: postural care in spina bifida', u='https://www.sbhscotland.org.uk/postural-care/'),
   dict(id='dyst', t='Dynamic seating for dystonia: forces from tone break hardware; evidence weak (PMGUK 2017 review; Healthcare 2020 8:11 protocol)', u='https://www.mdpi.com/2227-9032/8/1/11'),
   dict(id='ds', t='Adaptive seating in Down syndrome and central hypotonia: very-low-quality evidence; fine-motor gains with a fitting chair (MDPI review)', u='https://www.mdpi.com/2673-7272/6/2/28'),
   dict(id='aaos_ds', t='AAOS OrthoInfo, Down syndrome musculoskeletal effects (hypotonia, ligament laxity, atlanto-axial instability)', u='https://orthoinfo.aaos.org/en/diseases--conditions/down-syndrome-musculoskeletal-effects'),
   dict(id='resna', t='RESNA 2020 student design (GoBabyGo seat): laterals adjustable in breadth, depth, height; pelvis stabilised first', u='https://sites.psu.edu/resnasdc/files/2020/11/165-Design_Brief_with_images_Pocwierz.pdf'),
   dict(id='anthro', t='Snyder et al. 1977 UM-HSRI-77-17 & 1975 UM-HSRI-BI-75-5, via NIST AnthroKids (chest breadth at axilla #64, bideltoid #35, seated hip #11; sitting axilla derived)', u='https://math.nist.gov/~SRessler/anthrokids/'),
   dict(id='prod', t='Catalogue pad sizes: Spex Narrow 85×115, Dreamline XS 95×130, x:panda 80–200 × 110–170 (research_products.md §1–2)', u='https://www.etac.com/globalassets/0-united-kingdom/spex/spex_accessories.pdf'),
   dict(id='local', t='Local: ROUND5_PLAN.md §2.6, §3.4–3.6, §8; gbg_sidepad_v1.fs (depthX, SHELL_T, FOAM, shellXi); review_r5/heights, review_r5/tilt/tilt_out.txt; chair_interfaces.md; child_fit.json; padding_research.md; research_products.md', u='')],
)

conditions = [
 dict(c='Typical development (ride-on use)', need='Light containment on turns and bumps; symmetric pads; top 25–50 mm below the axilla; snug or a few mm of air.', src=['aci4', 'pp_meas', 'medifab'],
      cover='Partly', why='Height: yes, every size has a setting (H1–H6). Width: yes with D0 and the foam by size: 2y P5 30 mm (−2.5) or 20 (+7.5), 2y P50 20 (−3.5), ~3–4 y 10 (≈ +2…+5), 5y P50 10 (+1), 5y P95 5 (−3.5). The 5y P95 gets only 5 mm EVA + 3 mm neoprene: firm.',
      set='H by age (2y P5 H1, 2y P50 H2, ~3 y H3, ~4 y H4, 5y P50 H5, 5y P95 H6); D0; foam 30/20 (2y), 10 (3–5y P50), 5 (5y P95); tilt 0.'),
 dict(c='CP, low-tone (hypotonic) trunk', need='Pads closer (snug, 0–5 mm) and set higher for poorer trunk control (support at the segment where control is lost); firm base; pelvic belt and often an anterior chest support first. Child slumps, so the axilla drops: keep ≥ 25 mm even when slumped.', src=['satco', 'lange', 'pp_adapt', 'resna'],
      cover='Partly', why='Height: yes, upper end of the 25 mm window available for every size. Firm build-up B / A available. Width: snug only for the smaller children (see typical); children with low tone are often lighter and narrower, which helps. No anterior support or chest strap points in the kit.',
      set='Highest setting that still gives ≥ 25 mm when slumped; D0 with the foam one step thicker than the snug one if it still fits (30 small, 20 2y P50, 10 bigger); T0 or 8° top-in if the chest narrows upward.'),
 dict(c='CP, spastic', need='Support without forcing: a little room (5–15 mm) and soft-faced pads so tone is not triggered; strong hardware (tone forces); asymmetric settings for windswept/rotation.', src=['lange', 'dyst', 'medifab'],
      cover='Partly', why='Independent L/R heights: yes. Softer means thicker foam, which narrows the gap: with one depth piece "room + soft" works only for the 2y sizes (D0 + 30/20); bigger children get thin firm EVA. Hardware is sized for a 900 N corner knock.',
      set='D0, foam 30, one height step lower than the snug setting if the child fixes against the pad; T0.'),
 dict(c='CP, dystonic / athetoid', need='Room for movement, padding against impact, compliant (dynamic) mounts so forces dissipate and hardware does not break.', src=['dyst', 'lange'],
      cover='No', why='Rigid carbon-reinforced arm, no compliance; pad only 82 mm long, small padded area for impacts. Latch strength helps against knock-out.',
      set='Use only with a seating clinician; D0, foam 20–30 (small children only), generous gap; consider a dynamic back instead.'),
 dict(c='Windswept hips / asymmetric trunk (CP GMFCS IV–V)', need='Different L/R heights (and depths) and a pelvic/hip guide as part of the system; pelvis positioned first.', src=['lange', 'medifab', 'invacare'],
      cover='Partly', why='Each side is independent (separate carrier, depth piece, pad), so asymmetric heights/depths work. With one carrier of each hand per length a few L/R pairs need a second carrier (see pair matrix). Any asymmetric depth also narrows the total free width. No hip guide.',
      set='Higher pad on the side the trunk falls toward; depth by side; add a hip guide on the seat.'),
 dict(c='Scoliosis (flexible, reducible): CP, SMA, DMD, spina bifida', need='Three-point control: convex-side pad LOWER at the curve apex, concave-side pad HIGHER just under the axilla, third point at the pelvis on the concave side; angle pads to push along the ribs. Note: the brief had high/low the other way round – the cited guidance (Lange chart) puts the LOWER pad at the apex of the convexity.', src=['lange', 'invacare', 'medifab', 'scire', 'aci7'],
      cover='Partly', why='L/R heights differ independently (yes) and ±8/±15° tilt can follow the rib angle. But the lowest pad (H1, centre z 172) is the same as the highest allowed pad for a 2y P5, so small children get ~0–30 mm of L/R difference (2y P5 3 mm, 2y P50 31, 5y P50 93, 5y P95 133). No pelvic third point. Pad 80 tall is fine.',
      set='Concave side: highest setting (top 25–30 mm below axilla). Convex side: setting nearest the apex. 8° top-in on the convex pad if it slips up the rib cage.'),
 dict(c='Spina bifida (myelomeningocele)', need='Lean control for low thoracic/lumbar lesions; reduced sensation, so wide, soft, pressure-spreading pads and daily skin checks; kyphosis/scoliosis common; contoured back.', src=['sb', 'sbh', 'scire'],
      cover='Partly', why='Heights and asymmetry as above. Pad area is small (80 × 82 mm vs 85–130 × 115–150 catalogue), so contact pressure is higher; bigger children only get thin EVA (10 / 5 mm), so skin checks matter more. Neoprene cover is wipe-clean.',
      set='Light contact only (gap 0–10 mm); never "firm"; check skin after each ride.'),
 dict(c='Duchenne muscular dystrophy (2–5 y usually still walking)', need='Usually no laterals at 2–5; later laterals once leaning starts, a firm back, and swing-away pads for hoisting.', src=['rch_dmd', 'nmd_delphi'],
      cover='Yes (age 2–5)', why='Light symmetric support is enough at this age; height range fits. Lift-out, not swing-away: fine for a child lifted in vertically, not for hoist transfers later.',
      set='As typical development.'),
 dict(c='SMA (type 1 / 2 sitters)', need='Very weak trunk: higher, firmer, snug pads; three-point control with asymmetric heights; often a TLSO/anterior support; scoliosis in 60–90 %. Children are often narrow and light.', src=['nmd_delphi', 'invacare', 'lange'],
      cover='Partly', why='Highest settings and firm build-up available; narrow children simply take thicker foam on D0 (30 mm = free width 133). Same small-child asymmetry limit and no pelvic/anterior support. Snug pads make vertical lift-out harder: lift-out helps.',
      set='Highest allowed setting both sides (or asymmetric per curve); D0; the thickest foam that is still snug; T0.'),
 dict(c='Down syndrome (hypotonia, ligament laxity)', need='Light lateral containment that still invites active trunk control (avoid over-support); shorter trunk; pelvis and feet supported; no pressure into lax joints; head/neck care (atlanto-axial).', src=['ds', 'aaos_ds', 'satco'],
      cover='Yes / Partly', why='Symmetric light support fits for 2y sizes; 5y sizes fail on width like typical children. Height steps fit the shorter trunk (axilla lower ⇒ lower setting).',
      set='One setting lower than the highest allowed; 5–10 mm air gap; foam 30; T0.'),
]
data['conditions'] = conditions
data['gaps'] = [
 'Width (80/20 fix): ONE depth piece D0 (pad back x 104) and the foam picks the width: free width 133 / 153 / 173 / 183 mm with 30 / 20 / 10 / 5 mm foam (3 mm cover) against chest breadths 138 (2y P5) … 190 (5y P95). D1–D3 are dropped (they only went narrower). A wider piece is blocked by the joint stack, not by the armrest: arm end x 128.1 − tenon 13 − 2.35 margin − 2.75 button head − 6 floor ⇒ pad back ≤ ~106. Moving the arm end outward means a new arm path (car limit x 165 is already reached by the arm bend, and a later arm end (YEND) brings the arm over the armrest rear block at y ≥ 62).',
 'The 5y P95 gets only 5 mm EVA + 3 mm neoprene (gap −3.5); 10 mm EVA gives −8.5 (firm). If that is not enough: a wide depth piece with an open sleeve and the pad screw in the other ±11 boss (off the tenon axis) reaches pad back ≈ x 111 → 5y P95 with 10 mm (−1.5), 5y P50 with 20 (−2). It is a new part (taller 54 mm foot).',
 'Step size: depth steps 12 mm (24 mm of free width), foam steps 13 mm. Clinicians fit "snug" to a few mm: add a 6 mm half-step (a shim or a reversible foot) or rely on foam choice + compression.',
 'Asymmetry for small children: the lowest pad centre (172) is too high for a convex-side apex pad on 2-year-olds. A longer drop (66 mm → centres 150/128) as a third arm would give 2y P5/P50 ≥ 40 mm of L/R difference.',
 'Carrier hands: with one A and one B per arm length, some L/R height pairs cannot be built at the same time (pair matrix). Offer a spare carrier or make the carrier ambidextrous.',
 'Transfers: snug pads catch the armpits on a vertical lift; the lift-out is OK for parents but slow. Swing-away (OPEN 4.5) would serve SMA/DMD/hoisted children better.',
 'Pad size: 80 × 82 mm is at the small end of paediatric catalogues (80–130 tall, 110–150 long). For pressure (spina bifida, scoliosis correction) a 110 mm long option would spread load ~35 % more.',
 'Third point / anterior control: no hip guide or chest-strap points. Three-point scoliosis control and low-tone forward flexion need them – add strap slots in the depth piece or a separate hip-guide kit.',
 'Tilt shifts the pad edge 5–10 mm toward the child (top edge for top-in, bottom edge for top-out) and the top edge up to +7.6 mm (15° top-out): re-check the armpit and gap after every tilt change.',
 'Measure the child: these are typical-child 1970s US norms; children with CP/SMA are often smaller. Seat cushions raise the axilla.',
]
json.dump(data, open(OUT, 'w'), indent=1, ensure_ascii=False)
for k, v in children.items():
    print(k, 'axz', v['axilla_z'], 'window', v['centre_window'], 'fits', v['fits_heights'], 'best', v['best_width'], 'asym', v['max_LR_height_difference'])
print('blocked pairs', blocked)
for d in data['depths']: print(d)
