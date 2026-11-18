#!/usr/bin/env python3
# Builds the review-test Feature Studio source: kit library from ../gbg_sidepad_v1.fs (features stripped, comments and
# indentation removed) + harness.fs.  Usage: gen_test.py [defaults-override-python-repl...]  -> prints the path written.
import re, sys, os
H = os.path.dirname(os.path.abspath(__file__))
s = open(os.path.join(H, '..', 'gbg_sidepad_v1.fs')).read()
s = s[:s.index('annotation { "Feature Type Name" : "GbG Side Pad v1"')]
s = re.sub(r'(?m)^\s*//.*\n', '', s); s = re.sub(r'\s+//[^\n"]*$', '', s, flags=re.M)
s = re.sub(r'annotation \{ "Name" : "[^"]*" \} ', '', s)
s = re.sub(r'(?m)^[ \t]+', '', s); s = re.sub(r' {2,}', ' ', s)
h = open(os.path.join(H, 'harness.fs')).read()
for a in sys.argv[1:]:
    k, v = a.split('=', 1); h = re.sub(r'"%s" : [^,}]+' % k, '"%s" : %s' % (k, v), h.split('}, {')[0] + '}, {' + h.split('}, {')[1]) if False else h.replace(h[h.rindex('}, {'):], re.sub(r'"%s" : [A-Za-z0-9_.]+' % k, '"%s" : %s' % (k, v), h[h.rindex('}, {'):]))
out = os.path.join(H, 'review_test.fs'); open(out, 'w').write(s + '\n' + h); print(out, len(s + h))
