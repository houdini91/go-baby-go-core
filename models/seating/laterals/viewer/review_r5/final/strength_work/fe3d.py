# Minimal 3D voxel FE (8-node hexes, isotropic), CG with Jacobi; stresses at voxel centres.
import numpy as np, scipy.sparse as sp, scipy.sparse.linalg as spl, time
def ke_h8(E, nu, h):
    lam = E * nu / ((1 + nu) * (1 - 2 * nu)); mu = E / (2 * (1 + nu))
    D = np.zeros((6, 6)); D[:3, :3] = lam; D[np.arange(3), np.arange(3)] += 2 * mu; D[3:, 3:] = np.eye(3) * mu
    cn = np.array([[0,0,0],[1,0,0],[1,1,0],[0,1,0],[0,0,1],[1,0,1],[1,1,1],[0,1,1]], float)
    g = 1 / np.sqrt(3); K = np.zeros((24, 24))
    def B_at(xi):
        s = 2 * cn - 1
        dN = np.array([s[:, 0] * (1 + s[:, 1] * xi[1]) * (1 + s[:, 2] * xi[2]),
                       s[:, 1] * (1 + s[:, 0] * xi[0]) * (1 + s[:, 2] * xi[2]),
                       s[:, 2] * (1 + s[:, 0] * xi[0]) * (1 + s[:, 1] * xi[1])]) / 8 * (2 / h)
        B = np.zeros((6, 24))
        B[0, 0::3] = dN[0]; B[1, 1::3] = dN[1]; B[2, 2::3] = dN[2]
        B[3, 0::3] = dN[1]; B[3, 1::3] = dN[0]; B[4, 1::3] = dN[2]; B[4, 2::3] = dN[1]; B[5, 0::3] = dN[2]; B[5, 2::3] = dN[0]
        return B
    for a in (-g, g):
        for b in (-g, g):
            for c in (-g, g):
                B = B_at((a, b, c)); K += B.T @ D @ B * (h / 2) ** 3
    return K, D, B_at((0, 0, 0)), cn.astype(int)
def solve(mask, origin, h, E, nu, fixed_fn, loads, tol=1e-7, maxiter=20000):
    """mask[i,j,k] voxel solid; fixed_fn(X,Y,Z)->bool array per node; loads: (x,y,z,fx,fy,fz) to nearest node."""
    t0 = time.time()
    I, J, Kk = np.nonzero(mask); K0, D, Bc, cn = ke_h8(E, nu, h)
    nshape = np.array(mask.shape) + 1
    flat = lambda a, b, c: (a * nshape[1] + b) * nshape[2] + c
    en = np.stack([flat(I + o[0], J + o[1], Kk + o[2]) for o in cn], 1)
    used, inv = np.unique(en, return_inverse=True); en = inv.reshape(en.shape)
    nn = len(used); N = 3 * nn
    dofs = np.empty((len(I), 24), int)
    for a in range(3): dofs[:, a::3] = 3 * en + a
    rows = np.repeat(dofs, 24, 1).ravel(); cols = np.tile(dofs, (1, 24)).ravel(); vals = np.tile(K0.ravel(), len(I))
    Kg = sp.csr_matrix((vals, (rows, cols)), shape=(N, N))
    ci = np.stack(np.unravel_index(used, nshape), 1)
    P = origin + ci * h
    F = np.zeros(N)
    for (x, y, z, fx, fy, fz) in loads:
        k = np.argmin(((P - [x, y, z]) ** 2).sum(1)); F[3 * k:3 * k + 3] += (fx, fy, fz)
    fx_ = fixed_fn(P[:, 0], P[:, 1], P[:, 2]); fixd = np.repeat(fx_, 3)
    free = ~fixd; Kf = Kg[free][:, free]
    Md = 1 / Kf.diagonal()
    u = np.zeros(N)
    sol, info = spl.cg(Kf, F[free], rtol=tol, maxiter=maxiter, M=sp.diags(Md))
    u[free] = sol
    S = (D @ (Bc @ u[dofs].T)).T
    out = np.full(mask.shape + (6,), np.nan); out[I, J, Kk] = S
    return out, info, time.time() - t0, len(I)
