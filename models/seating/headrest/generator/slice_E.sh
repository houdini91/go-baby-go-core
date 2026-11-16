#!/usr/bin/env bash
# Headless slice (Bambu P1S, PETG, no supports): slice_E.sh part.stl walls infill% layerpreset(020|028)
set -euo pipefail
HERE="${GBG_PRINT_DIR:?set GBG_PRINT_DIR to a folder holding presets/ (your P1S machine, process020/028 and petg JSONs)}"; STL="$(realpath "$1")"; WL="$2"; INF="$3"; LP="${4:-028}"
NAME="$(basename "$STL" .stl)"; OUT="$HERE/out/$NAME"; mkdir -p "$OUT"
W="$OUT/presets"; mkdir -p "$W"; cp "$HERE/presets/"*.json "$W/"
python3 - "$W/process$LP.json" "$WL" "$INF" <<'PY'
import json,sys; f=sys.argv[1]; d=json.load(open(f))
d.update({"enable_support":"0","wall_loops":sys.argv[2],"sparse_infill_density":sys.argv[3]+"%","sparse_infill_pattern":"gyroid","top_shell_layers":"4","bottom_shell_layers":"3"})
json.dump(d,open(f,"w"),indent=1)
PY
cd "$OUT"
timeout 900 flatpak run --command=bambu-studio --filesystem=home com.bambulab.BambuStudio --slice 0 --arrange 1 --orient 0 \
  --load-settings "$W/machine.json;$W/process$LP.json" --load-filaments "$W/petg.json" --curr-bed-type "Textured PEI Plate" \
  --export-3mf "$NAME.3mf" --outputdir "$OUT" "$STL" >"$OUT/log.txt" 2>&1 || true
python3 - "$OUT/result.json" "$NAME" <<'PY'
import json,sys; r=json.load(open(sys.argv[1])); p=r.get("sliced_plates",[{}])[0]
g=sum(f["total_used_g"] for f in p.get("filaments",[])); t=p.get("main_predication",0)
print(json.dumps({"part":sys.argv[2],"result":r.get("error_string"),"time_h":round(t/3600,2),"petg_g":round(g,1)}))
PY
