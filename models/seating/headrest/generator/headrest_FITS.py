# P1S / PETG FDM TOLERANCE BLOCK (2026-11-16) -- mirrors the "print fits" block at the top of
# featurescript/gbg_headrest_v10.fs.  FDM holes on a P1S print ~0.1-0.3 mm undersize on the diameter (horizontal ones
# worse, teardrops included), so every hole carries that allowance.  Policy:
#   clearance hole = nominal + 0.4..0.5 on the diameter | nut trap = nut AF + 0.3, depth = nut thickness + 0.4 |
#   sliding fit >= 0.2 per side | PVC / rod bores = slip fits (something else locks them).
# Confirm with the fit-test bar (headrest_FINAL_export.fit_test), then edit here AND in the .fs.
PVC_BORE = 20.8     # 20 mm PVC, OD 20.0 +0.2/-0 (ISO 1452): slip fit even on a +0.2 pipe; 3 wood screws lock it (was 20.3 = press fit)
ROD_HOLE_D = 8.5    # Ø8 carbon rod, often 8.0-8.1 as bought (was 8.3)
M3_CLEAR = 3.5      # M3 clearance, +0.5 (was 3.4)
M5_CLEAR = 5.5      # M5 clearance, +0.5 (was 5.4)
NUT_M3_AF = 5.8     # M3 nut AF 5.5 + 0.3 (unchanged)
NUT_M5_AF = 8.3     # M5 nut AF 8.0 + 0.3 (unchanged)
NUT_M3_H, NUT_M5_H = 2.4, 4.0                   # DIN 934 nut thickness
NUT_M3_TRAP = NUT_M3_H + 0.4                    # 2.8 trap depth (was 3.0 pad / 2.7 tenon slot)
NUT_M5_TRAP = NUT_M5_H + 0.4                    # 4.4 slot thickness (was 4.3)
WOOD_SCREW_HOLE = 4.0  # pod -> PVC wood screws (ST3.5): clearance in the pod, the screw cuts only the PVC
