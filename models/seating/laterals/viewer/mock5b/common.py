# Shared setup for the round-5b mockups: runs ../r5mesh/gen_r5.py in a scratch dir (so it writes nothing here) and
# hands back its namespace (S-frame helpers, the original wing W, nose, tongue, latch cuts, build_arm ...).
import os, sys, runpy, tempfile, contextlib, io
HERE = os.path.dirname(os.path.abspath(__file__))
R5 = os.path.join(HERE, '..', 'r5mesh')
def load_r5():
    sys.path.insert(0, R5)
    cwd = os.getcwd(); tmp = tempfile.mkdtemp(prefix='gen_r5_'); os.makedirs(os.path.join(tmp, 'work'))
    os.chdir(tmp)
    try:
        with contextlib.redirect_stdout(io.StringIO()):
            g = runpy.run_path(os.path.join(R5, 'gen_r5.py'))
    finally:
        os.chdir(cwd)
    return g
