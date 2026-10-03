#!/usr/bin/env python3
# -*- coding: utf-8 -*-
r"""
make-card-375x526.py —— 圆/椭圆立绘「外填底色」无缝化 + 精确尺寸输出（最终版）
=================================================================================
实测真相（原始测量 JSON 见 docs/18-先古竖图375x526制作流程.md 第 7 节）
------------------------------------------------------------------
源图 1344x1792 RGBA，alpha 全 255（白底是烤进 RGB 的），外填底色 = (235,235,235)。

1) 这个圆**没有硬边**。沿椭圆法线的径向中位亮度：
       r=0.70 -> 244.97   r=0.90 -> 235.0   r=0.99 -> 235.0
       r=1.00 -> 235.0     r=1.05 -> 235.0  r=1.30 -> 235.0
   边界的平均亮度跳变 = **0.00/255**。720 条射线的「非底色末位置」半径从
   631 散到 903，RANSAC 圆拟合只能给出 cx=4.6, cy=-20.6 的垃圾解。

2) 真正的差异是**纸纹**（7x7 局部标准差的中位）：
       圆内 r=0.90..0.99 -> 4.117      圆外 -> 0.0
   纸纹在边界处戛然而止 —— 这才是肉眼看到的那圈「脏边/色差」。

做法（边界两侧取值数学上相同 => 接缝不可能存在）
------------------------------------------------
    S = 高斯模糊( 人物像素->T(r), 半径 blur )        平滑纸面
    w = 1 - smoothstep(0, fade, depth_inside(disc))  圆外恒为 1
    out = orig + (S - orig) * w * (1 - psoft)
  · 圆外 w=1、圆内贴边 w=1  -> 两侧都等于 S
  · 圆心 w=0 -> 纸纹、笔触、渐晕原样保留
  · 人物/碎片被 psoft 挡住
  · 不生成任何合成噪点

用法
----
  py tools\make-card-375x526.py <src> --out <dir> [--size 375x526] [--top-gap .5]
       [--bottom-gap .5] [--fade 56] [--paper-blur 9] [--fix seam|none] [--save-full]
"""
from __future__ import annotations

import argparse
import json
import os
import sys
from collections import deque

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _card_lib_375x526 import (  # noqa: E402
    character_mask, head_metrics, linear_to_srgb_u8, srgb_to_linear, smoothstep)

LUM_W = np.array([0.2126, 0.7152, 0.0722], np.float32)


# ───────────────────────── 形态学 / 连通域 ─────────────────────────
def dilate(mask, n):
    m = mask.copy()
    for _ in range(n):
        d = m.copy()
        d[1:, :] |= m[:-1, :]
        d[:-1, :] |= m[1:, :]
        d[:, 1:] |= m[:, :-1]
        d[:, :-1] |= m[:, 1:]
        m = d
    return m


def erode(mask, n):
    m = mask.copy()
    for _ in range(n):
        e = m.copy()
        e[1:, :] &= m[:-1, :]
        e[:-1, :] &= m[1:, :]
        e[:, 1:] &= m[:, :-1]
        e[:, :-1] &= m[:, 1:]
        m = e
    return m


def component_containing(mask, sy, sx):
    H, W = mask.shape
    if not mask[sy, sx]:
        ys, xs = np.nonzero(mask)
        if ys.size == 0:
            return np.zeros_like(mask)
        i = int(np.argmin((ys - sy) ** 2 + (xs - sx) ** 2))
        sy, sx = int(ys[i]), int(xs[i])
    out = np.zeros_like(mask)
    out[sy, sx] = True
    q = deque([(sy, sx)])
    while q:
        y, x = q.popleft()
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < H and 0 <= nx < W and mask[ny, nx] and not out[ny, nx]:
                out[ny, nx] = True
                q.append((ny, nx))
    return out


def depth_inside(mask, dmax):
    """到 mask 边界的 4-邻域距离，截断 dmax；mask 外为 0。"""
    H, W = mask.shape
    depth = np.zeros((H, W), np.int32)
    cur = mask.copy()
    for k in range(1, dmax + 1):
        cur = erode(cur, 1)
        gone = mask & ~cur
        if not gone.any():
            break
        depth[gone] = k
        if not cur.any():
            break
    depth[cur] = dmax + 1
    return depth


def local_mean(x, k):
    H, W = x.shape
    p = np.pad(x, k // 2, mode="edge")
    cs = np.pad(np.cumsum(np.cumsum(p, 0), 1), ((1, 0), (1, 0)), mode="constant")
    ys = np.arange(H)[:, None]
    xs = np.arange(W)[None, :]
    return ((cs[ys + k, xs + k] - cs[ys, xs + k]
             - cs[ys + k, xs] + cs[ys, xs]) / float(k * k)).astype(np.float32)


def local_std(x, k):
    m = local_mean(x, k)
    m2 = local_mean(x * x, k)
    return np.sqrt(np.maximum(m2 - m * m, 0.0)).astype(np.float32)


# ───────────────────────── 分割 / 色调 ─────────────────────────
def segment_disc(a_u8, fill):
    """「像素 != 底色」的含图心连通域 = 圆盘。

    不用阈值洪泛：近边纸纹本身也正好等于 235，洪泛会漏进圆内约 10% 画布。
    """
    notfill = np.abs(a_u8.astype(np.int32)
                     - fill.reshape(1, 1, 3)).max(axis=2) > 0
    H, W = notfill.shape
    return dilate(component_containing(notfill, H // 2, W // 2), 1)


def radial_tone(lin, ok, r, nb=200, smooth=13):
    """[0,1] 内实测 T(r)（线性光中位色）；空 bin 线性插值。"""
    edges = np.linspace(0.0, 1.0, nb + 1)
    idx = np.clip(np.digitize(r.ravel(), edges) - 1, 0, nb - 1)
    v = lin.reshape(-1, 3)
    g = ok.ravel()
    order = np.argsort(idx, kind="stable")
    bnd = np.searchsorted(idx[order], np.arange(nb + 1))
    out = np.full((nb, 3), np.nan, np.float32)
    cnt = np.zeros(nb, np.int32)
    for b in range(nb):
        sel = order[bnd[b]:bnd[b + 1]]
        if sel.size:
            sel = sel[g[sel]]
        if sel.size >= 60:
            out[b] = np.median(v[sel], axis=0)
            cnt[b] = sel.size
    bad = np.isnan(out[:, 0])
    if bad.any():
        gi = np.arange(nb)
        for c in range(3):
            out[:, c] = np.interp(gi, gi[~bad], out[~bad, c])
    k = max(3, smooth | 1)
    p = k // 2
    sm = np.stack([np.convolve(np.pad(out[:, c], p, mode="edge"),
                              np.ones(k, np.float32) / k, mode="valid")
                   for c in range(3)], axis=1)
    return (edges[:-1] + edges[1:]) / 2.0, sm, cnt, int(bad.sum())


def eval_tone(centers, prof, r):
    """查询 T(r)；r>1 钳到 T(1)，绝不外推。"""
    o = np.empty(r.shape + (3,), np.float32)
    f = np.clip(r.ravel(), 0.0, 1.0)
    for c in range(3):
        o[..., c] = np.interp(f, centers, prof[:, c]).reshape(r.shape)
    return o


def protect_mask(a_u8, blur=12.0, dev_th=25.0, grow=3):
    lum = a_u8 @ LUM_W
    sat = a_u8.max(axis=2).astype(np.int16) - a_u8.min(axis=2).astype(np.int16)
    bg = np.asarray(Image.fromarray(a_u8).filter(
        ImageFilter.GaussianBlur(blur))).astype(np.float32)
    dev = np.abs(a_u8.astype(np.float32) - bg).max(axis=2)
    art = (lum < 140) | (sat > 50) | (dev > dev_th)
    return dilate(art, grow) | art


def paper_field(lin, T, protect, blur):
    """人物像素先填成 T(r)，再高斯模糊 -> 平滑纸面 S（无人物、无噪点）。

    模糊在 **sRGB u8** 域做（Pillow 的 GaussianBlur 是 8-bit 实现的，
    精度够了），取回后再转线性光，**只做一次** sRGB->linear 往返。
    """
    base_srgb = linear_to_srgb_u8(np.where(protect[..., None], T, lin))
    sm = np.asarray(Image.fromarray(base_srgb).filter(
        ImageFilter.GaussianBlur(blur)), np.float32)
    return srgb_to_linear(sm)


# ───────────────────────── 主流程 ─────────────────────────
def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("src")
    ap.add_argument("--out", default=None)
    ap.add_argument("--size", default="375x526")
    ap.add_argument("--fix", default="seam", choices=["seam", "none"])
    ap.add_argument("--top-gap", type=float, default=0.5)
    ap.add_argument("--bottom-gap", type=float, default=0.5)
    ap.add_argument("--head-height", type=float, default=0.0)
    ap.add_argument("--fade", type=int, default=56)
    ap.add_argument("--paper-blur", type=float, default=9.0)
    ap.add_argument("--save-full", action="store_true")
    ap.add_argument("--tag", default="")
    a = ap.parse_args()

    tw, th = (int(v) for v in a.size.lower().split("x"))
    outdir = a.out or os.path.dirname(os.path.abspath(a.src))
    os.makedirs(outdir, exist_ok=True)
    stem = os.path.splitext(os.path.basename(a.src))[0]
    tag = a.tag or f"_{tw}x{th}"

    rep = {"src": a.src, "target": [tw, th], "fix": a.fix,
           "fade_px": a.fade, "paper_blur": a.paper_blur}
    src_im = Image.open(a.src)
    a_u8 = np.asarray(src_im.convert("RGB"))
    H, W = a_u8.shape[:2]
    rep["source_size"] = [W, H]
    rep["source_mode"] = src_im.mode
    if src_im.mode == "RGBA":
        al = np.asarray(src_im)[:, :, 3]
        rep["source_alpha"] = {"min": int(al.min()), "max": int(al.max())}

    frame = np.concatenate([a_u8[0], a_u8[-1], a_u8[:, 0], a_u8[:, -1]], 0)
    fill = np.median(frame.astype(np.float32), axis=0)
    rep["fill_rgb"] = [float(v) for v in fill]

    disc = segment_disc(a_u8, fill)
    rep["disc_frac"] = round(float(disc.mean()), 4)
    ys, xs = np.nonzero(disc)
    cx, cy = (xs.min() + xs.max()) / 2.0, (ys.min() + ys.max()) / 2.0
    ra, rb = (xs.max() - xs.min()) / 2.0, (ys.max() - ys.min()) / 2.0
    rep["ellipse"] = {"cx": round(cx, 1), "cy": round(cy, 1),
                      "ra": round(ra, 1), "rb": round(rb, 1)}
    Y, X = np.mgrid[0:H, 0:W]
    r = np.sqrt(((X - cx) / ra) ** 2 + ((Y - cy) / rb) ** 2).astype(np.float32)

    protect = protect_mask(a_u8)
    rep["protect_frac"] = round(float(protect.mean()), 4)
    rep["protect_frac_outside"] = round(
        float((protect & ~disc).sum()) / max(1, int((~disc).sum())), 4)

    lin = srgb_to_linear(a_u8)
    centers, prof, cnt, ninterp = radial_tone(lin, disc & ~protect, r)
    rep["tone_bins_used"] = int((cnt > 0).sum())
    rep["tone_bins_interpolated"] = ninterp
    T = eval_tone(centers, prof, r)
    Ts = linear_to_srgb_u8(T)
    rep["tone_inside_rgb"] = [int(v) for v in np.median(Ts[disc], axis=0)]
    rep["tone_edge_rgb"] = [int(v) for v in np.median(Ts[disc & (r > 0.98)], axis=0)]
    rep["tone_clamped_outside_rgb"] = [int(v) for v in np.median(Ts[~disc], axis=0)]

    gsd = local_std(a_u8 @ LUM_W, 7)
    band = disc & (r > 0.90) & (r < 0.99)
    rep["grain_inside_rim"] = round(float(np.median(gsd[band])), 3) if band.any() else 0
    rep["grain_outside"] = round(float(np.median(gsd[~disc])), 3)

    fixed = a_u8.copy()
    if a.fix == "seam":
        S = paper_field(lin, T, protect, a.paper_blur)
        Ss = linear_to_srgb_u8(S)
        rep["S_inside_rgb"] = [int(v) for v in np.median(Ss[disc], axis=0)]
        rep["S_outside_rgb"] = [int(v) for v in np.median(Ss[~disc], axis=0)]
        rep["S_outside_min"] = [int(v) for v in Ss[~disc].reshape(-1, 3).min(axis=0)]
        rep["S_outside_max"] = [int(v) for v in Ss[~disc].reshape(-1, 3).max(axis=0)]
        depth = depth_inside(disc, a.fade)
        w = np.where(disc, 1.0 - smoothstep(0, a.fade, depth.astype(np.float32)),
                     1.0).astype(np.float32)
        psoft = np.clip(np.asarray(Image.fromarray((protect * 255).astype(np.uint8))
                                   .filter(ImageFilter.GaussianBlur(2.5))
                                   ).astype(np.float32) / 255.0, 0, 1)
        w = w * (1.0 - psoft)
        rep["w"] = {"outside_min": round(float(w[~disc].min()), 4),
                    "outside_med": round(float(np.median(w[~disc])), 4),
                    "edge_in_med": round(float(np.median(w[disc & (r > 0.97)])), 4),
                    "core_med": round(float(np.median(w[disc & (r < 0.5)])), 4)}
        fixed = linear_to_srgb_u8(lin + (S - lin) * w[..., None])
        rep["fix_applied"] = True
    else:
        rep["fix_applied"] = False

    # ── 构图：头离顶 0.5 头高，裙摆离底 0.5 头高 ──
    comp, crep = character_mask(a_u8)
    rep["character"] = crep
    hm = head_metrics(comp)
    head_h = a.head_height if a.head_height > 0 else hm["head_h"]
    hm["head_h_used"] = round(head_h, 1)
    rep["head"] = hm
    ratio = tw / float(th)
    y0 = max(0.0, hm["head_top"] - a.top_gap * head_h)
    fh = max(8.0, (hm["comp_bottom"] + a.bottom_gap * head_h) - y0)
    fw = fh * ratio
    x0 = min(max(0.0, hm["head_cx"] - fw / 2.0), max(0.0, W - fw))
    y0 = min(y0, max(0.0, H - fh))
    fw, fh = min(fw, float(W)), min(fh, float(H))
    box = (int(round(x0)), int(round(y0)), int(round(x0 + fw)), int(round(y0 + fh)))
    k = th / fh
    rep["crop"] = {"box": list(box), "src_wh": [round(fw, 1), round(fh, 1)],
                   "scale": round(k, 5),
                   "top_gap_head": round((hm["head_top"] - y0) / head_h, 3),
                   "bottom_gap_head": round(((y0 + fh) - hm["comp_bottom"]) / head_h, 3),
                   "head_h_out": round(head_h * k, 1),
                   "top_gap_out_px": round((hm["head_top"] - y0) * k, 1),
                   "bottom_gap_out_px": round(((y0 + fh) - hm["comp_bottom"]) * k, 1)}

    out = Image.fromarray(fixed).crop(box).resize((tw, th), Image.LANCZOS)
    dst = os.path.join(outdir, f"{stem}{tag}.png")
    out.save(dst)
    rep["output"] = dst
    rep["out_size"] = list(out.size)
    if a.save_full:
        fdst = os.path.join(outdir, f"{stem}{tag}_full.png")
        Image.fromarray(fixed).save(fdst)
        rep["output_full"] = fdst

    vis = Image.fromarray(fixed)
    ov = np.zeros((H, W, 3), np.uint8)
    ov[disc] = (0, 210, 0)
    ov[protect] = (255, 0, 0)
    Image.fromarray(ov).resize((W // 3, H // 3), Image.NEAREST).save(
        os.path.join(outdir, f"{stem}{tag}_regions.png"))
    d = ImageDraw.Draw(vis)
    d.ellipse([cx - ra, cy - rb, cx + ra, cy + rb], outline=(255, 0, 255), width=5)
    d.line([0, hm["head_top"], W, hm["head_top"]], fill=(0, 128, 255), width=3)
    d.line([0, hm["comp_bottom"], W, hm["comp_bottom"]], fill=(255, 140, 0), width=3)
    d.rectangle(list(box), outline=(255, 255, 0), width=6)
    vis.resize((W // 3, H // 3), Image.LANCZOS).save(
        os.path.join(outdir, f"{stem}{tag}_debug.png"))

    with open(os.path.join(outdir, f"{stem}{tag}_report.json"), "w",
              encoding="utf-8") as f:
        json.dump(rep, f, ensure_ascii=False, indent=2, default=float)


if __name__ == "__main__":
    main()
