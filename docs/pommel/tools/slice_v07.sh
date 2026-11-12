#!/usr/bin/env bash
# Headless slice on Mikey's Bambu P1S, PETG, guide settings: 0.20 mm, 4 walls, gyroid 15%, tree supports on build plate only.
# usage: slice_v07.sh part.stl [support:1|0]
set -euo pipefail
HERE="$(cd "$(dirname "$0")/.." && pwd)"; STL="$(realpath "$1")"; SUP="${2:-1}"
NAME="$(basename "$STL" .stl)"; OUT="$HERE/print/out/$NAME"; mkdir -p "$OUT"
P="$HERE/print/presets"; W="$OUT/presets"; mkdir -p "$W"; cp "$P"/*.json "$W/"
python3 - "$W/process020.json" "$SUP" <<'PY'
import json,sys; f=sys.argv[1]; d=json.load(open(f))
d.update({"enable_support":sys.argv[2],"support_type":"tree(auto)","support_on_build_plate_only":"1","support_threshold_angle":"30",
          "wall_loops":"4","sparse_infill_density":"15%","sparse_infill_pattern":"gyroid","top_shell_layers":"5","bottom_shell_layers":"4"})
json.dump(d,open(f,"w"),indent=1)
PY
cd "$OUT"
timeout 900 flatpak run --command=bambu-studio --filesystem=home com.bambulab.BambuStudio --slice 0 --arrange 1 --orient 0 \
  --load-settings "$W/machine.json;$W/process020.json" --load-filaments "$W/petg.json" --curr-bed-type "Textured PEI Plate" \
  --export-3mf "$NAME.3mf" --outputdir "$OUT" "$STL" >"$OUT/log.txt" 2>&1 || true
python3 - "$OUT/result.json" <<'PY'
import json,sys; r=json.load(open(sys.argv[1])); p=r.get("sliced_plates",[{}])[0]
g=sum(f["total_used_g"] for f in p.get("filaments",[])); t=p.get("main_predication",0)
print(json.dumps({"result":r.get("error_string"),"time_h":round(t/3600,2),"petg_g":round(g,1)}))
PY
