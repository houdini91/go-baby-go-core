import headrest_MB_base as B, headrest_M_mast as MM, headrest_M_strength as S
def run(b, H, window=True):
    m = B.mast(b, 0, H, window)
    if b == "B3": r = S.analyse("M1", H, m_in=m, z_fix=B.HOLES_HI[0][1], carbon=(MM.TOP + 2.5, 1e9))
    else: r = S.analyse("M1", H, m_in=m, carbon=(MM.SOCK_Z0 + 1, 1e9))
    zc = MM.frame(0, H)[0]
    if b == "B3":   # tongue pivots on its bottom edge (z 284) against the back face; the top bolt row takes the pull
        Mtop = S.P_KNOCK * (zc - B.HOLES_HI[0][1]); r["bolt_N"] = Mtop / (B.HOLES_HI[0][1] - 284.0) / 3
    else:           # plug levers in the 45 mm socket: wall bearing ~ 6M/(b L^2)
        Mx = S.P_KNOCK * (zc - MM.TOP); r["bolt_N"] = 6 * Mx / (56.0 * 45.0 ** 2)     # here: socket wall pressure, MPa
    return r
if __name__ == "__main__":
    for b in ("B1", "B2", "B3"):
        for H in (50, 100):
            for w in (False, True):
                r = run(b, H, w)
                print("%s H%d win %d | plastic SF %.1f (%.1f MPa at z %.0f, plug %.1f) side SF %.1f | flex %.2f / side %.2f mm | carbon %.0f MPa | backup SF %.1f | %s %.0f | %.0f cm3" % (
                    b, H, w, r["SF"], r["sig_p"], r["z_crit"], r["plug"], r["SF_side"], r["defl"], r["defl_side"], r["sig_c"], r["backup_SF"], "bolt N" if b == "B3" else "socket MPa", r["bolt_N"], r["vol"]), flush=True)
