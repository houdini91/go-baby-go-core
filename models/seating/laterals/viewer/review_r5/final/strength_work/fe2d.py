# Minimal 2D plane-stress pixel FE (bilinear quads), unit thickness (results per mm of z: loads in N/mm).
import numpy as np, scipy.sparse as sp, scipy.sparse.linalg as spl
def ke_q4(E, nu, h):
    D = E / (1 - nu ** 2) * np.array([[1, nu, 0], [nu, 1, 0], [0, 0, (1 - nu) / 2]])
    g = 1 / np.sqrt(3); K = np.zeros((8, 8)); xy = np.array([[0, 0], [1, 0], [1, 1], [0, 1]], float)
    Bc = None
    for xi in (-g, g):
        for et in (-g, g):
            dN = 0.25 * np.array([[-(1 - et), (1 - et), (1 + et), -(1 + et)], [-(1 - xi), -(1 + xi), (1 + xi), (1 - xi)]])
            J = dN @ (xy * h); dNx = np.linalg.solve(J, dN)
            B = np.zeros((3, 8)); B[0, 0::2] = dNx[0]; B[1, 1::2] = dNx[1]; B[2, 0::2] = dNx[1]; B[2, 1::2] = dNx[0]
            K += B.T @ D @ B * np.linalg.det(J)
    dN = 0.25 * np.array([[-1, 1, 1, -1], [-1, -1, 1, 1]]); J = dN @ (xy * h); dNx = np.linalg.solve(J, dN)
    Bc = np.zeros((3, 8)); Bc[0, 0::2] = dNx[0]; Bc[1, 1::2] = dNx[1]; Bc[2, 0::2] = dNx[1]; Bc[2, 1::2] = dNx[0]
    return K, D, Bc
def solve(mask, x0, y0, h, E, nu, fixed_fn, loads):
    """mask[i, j]: pixel (x0 + (i+.5)h, y0 + (j+.5)h) solid. fixed_fn(x, y) -> bool for nodes. loads: list of (x, y, fx, fy)
    applied to the nearest solid node. Returns sx, sy, txy per pixel (nan outside), u."""
    nx, ny = mask.shape; nid = -np.ones((nx + 1, ny + 1), int)
    I, Jj = np.nonzero(mask)
    corners = [(0, 0), (1, 0), (1, 1), (0, 1)]
    for a, b in corners: nid[I + a, Jj + b] = 0
    used = np.argwhere(nid == 0); nid[used[:, 0], used[:, 1]] = np.arange(len(used))
    K0, D, Bc = ke_q4(E, nu, h)
    en = np.stack([nid[I + a, Jj + b] for a, b in corners], 1)
    dofs = np.empty((len(I), 8), int); dofs[:, 0::2] = 2 * en; dofs[:, 1::2] = 2 * en + 1
    rows = np.repeat(dofs, 8, 1).ravel(); cols = np.tile(dofs, (1, 8)).ravel(); vals = np.tile(K0.ravel(), len(I))
    N = 2 * len(used); K = sp.csr_matrix((vals, (rows, cols)), shape=(N, N))
    F = np.zeros(N); X = x0 + used[:, 0] * h; Y = y0 + used[:, 1] * h
    for (x, y, fx, fy) in loads:
        k = np.argmin((X - x) ** 2 + (Y - y) ** 2); F[2 * k] += fx; F[2 * k + 1] += fy
    fx_ = np.array([fixed_fn(x, y) for x, y in zip(X, Y)])
    fixd = np.zeros(N, bool); fixd[0::2] = fx_; fixd[1::2] = fx_
    free = ~fixd; u = np.zeros(N)
    u[free] = spl.spsolve(K[free][:, free].tocsc(), F[free])
    S = (D @ (Bc @ u[dofs].T)).T
    out = np.full((nx, ny, 3), np.nan); out[I, Jj] = S
    reac = (K @ u - F)[fixd]
    return out, u, X, Y, reac.reshape(-1)
