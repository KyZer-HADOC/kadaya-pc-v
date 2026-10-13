using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kadaya
{
    public partial class Game1
    {
        // ---------- primitives ----------
        static float Hs(float i) { float v = MathF.Sin(i * 12.9898f) * 43758.5453f; return v - MathF.Floor(v); }
        void Box(float x, float y, float w, float h, float rot, Color c) => sb.Draw(px, new Vector2(x, y), null, c, rot, new Vector2(.5f), new Vector2(w, h), SpriteEffects.None, 0);
        void Box(Vector2 p, float w, float h, float rot, Color c) => Box(p.X, p.Y, w, h, rot, c);
        void Ln(float x1, float y1, float x2, float y2, float th, Color c) { float dx = x2 - x1, dy = y2 - y1; sb.Draw(px, new Vector2(x1, y1), null, c, MathF.Atan2(dy, dx), new Vector2(0, .5f), new Vector2(MathF.Sqrt(dx * dx + dy * dy), th), SpriteEffects.None, 0); }
        void Ln(Vector2 a, Vector2 b, float th, Color c) => Ln(a.X, a.Y, b.X, b.Y, th, c);
        void Circ(float x, float y, float r, Color c) => sb.Draw(circ, new Vector2(x, y), null, c, 0, new Vector2(32), r / 32f, SpriteEffects.None, 0);
        void Circ(Vector2 p, float r, Color c) => Circ(p.X, p.Y, r, c);
        void CircS(float x, float y, float rx, float ry, Color c) => sb.Draw(circ, new Vector2(x, y), null, c, 0, new Vector2(32), new Vector2(rx / 32f, ry / 32f), SpriteEffects.None, 0);
        void Glow(float x, float y, float r, Color c) => sb.Draw(glow, new Vector2(x, y), null, c, 0, new Vector2(64), r / 64f, SpriteEffects.None, 0);
        void Glow(Vector2 p, float r, Color c) => Glow(p.X, p.Y, r, c);
        void GlowS(float x, float y, float rx, float ry, Color c) => sb.Draw(glow, new Vector2(x, y), null, c, 0, new Vector2(64), new Vector2(rx / 64f, ry / 64f), SpriteEffects.None, 0);
        void Tri(float x, float y, float w, float h, float rot, Color c) => sb.Draw(tri, new Vector2(x, y), null, c, rot, new Vector2(32), new Vector2(w / 64f, h / 64f), SpriteEffects.None, 0);
        void Tri(Vector2 p, float w, float h, float rot, Color c) => Tri(p.X, p.Y, w, h, rot, c);
        void Cres(float x, float y, float rx, float ry, float rot, bool flip, Color c) => sb.Draw(cres, new Vector2(x, y), null, c, rot, new Vector2(128), new Vector2(rx / 128f, ry / 128f), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        void Txt(string s, float x, float y, float size, Color c, int al = 0, Color? gl = null)
        {
            float k = size / 64f; var sz = font.MeasureString(s) * k; float X = al == 1 ? x - sz.X / 2 : al == 2 ? x - sz.X : x; var gc = Ad(gl ?? c, .3f);
            foreach (var o in new[] { new Vector2(-2, 0), new Vector2(2, 0), new Vector2(0, -2), new Vector2(0, 2), new Vector2(-5, 0), new Vector2(5, 0), new Vector2(0, 5) }) sb.DrawString(font, s, new Vector2(X, y) + o * (size / 40f), gc, 0, Vector2.Zero, k, SpriteEffects.None, 0);
            sb.DrawString(font, s, new Vector2(X, y), c, 0, Vector2.Zero, k, SpriteEffects.None, 0);
        }

        // ---------- background (parallax silhouettes, god rays, fog) ----------
        void Mnt(float par, float baseY, float amp, Color col, float f)
        {
            float o = cam * par;
            for (int i = 0; i <= W + 16; i += 16) { float u = i + o; float y = baseY - amp * (MathF.Sin(u * .006f * f) * .5f + MathF.Sin(u * .0023f * f + 2) * .35f + MathF.Sin(u * .015f * f) * .15f + .6f); Box(i + 8, (y + H + 300) / 2, 17, H + 300 - y, 0, col); }
        }
        void Pagoda(float x, float by, float s, Color col)
        {
            for (int i = 0; i < 4; i++) { float w = (110 - i * 20) * s, h = 46 * s, y = by - i * h; Box(x, y - h * .35f, w - 16 * s, h * .7f, 0, col); Tri(x, y - h * .75f, w + 56 * s, h * .55f, 0, col); }
            Box(x, by - 4 * 46 * s - 17 * s, 4 * s, 34 * s, 0, col);
        }
        void Layer(float par, float sp, Color col, int style, float sc, float baseY)
        {
            float off = cam * par; int i0 = (int)MathF.Floor(off / sp) - 1, i1 = (int)MathF.Floor((off + W) / sp) + 2;
            for (int i = i0; i <= i1; i++)
            {
                float h = Hs(i * 1.7f + par * 50), x = i * sp - off + (h - .5f) * sp * .5f;
                if (style == 0) Bamboo(x, baseY, sc, col, h); else if (style == 1) Grave(x, baseY, sc, col, h); else Temple(x, baseY, sc, col, h);
            }
        }
        void Bamboo(float x, float by, float sc, Color col, float h)
        {
            for (int j = 0; j < 3; j++)
            {
                float bx = x + j * 24 * sc, hh = (420 + Hs(h * 10 + j) * 280) * sc, sw = MathF.Sin(t * .8f + bx * .01f) * 4 * sc;
                Box(bx + sw * .5f, by - hh / 2, 9 * sc, hh, 0, col);
                for (float y = 60 * sc; y < hh; y += 80 * sc) Box(bx + sw * (y / hh) * .5f, by - y, 13 * sc, 4 * sc, 0, col);
                for (int l = 0; l < 4; l++) { float ly = by - hh + l * 34 * sc, sd = l % 2 == 0 ? 1 : -1; Tri(bx + sw + sd * 30 * sc, ly, 70 * sc, 12 * sc, sd * 1.25f, col); }
            }
        }
        void Grave(float x, float by, float sc, Color col, float h)
        {
            if (h < .34f) { Box(x, by - 50 * sc, 42 * sc, 100 * sc, 0, col); Circ(x, by - 100 * sc, 21 * sc, col); }
            else if (h < .6f) { Box(x, by - 90 * sc, 14 * sc, 180 * sc, 0, col); Box(x, by - 130 * sc, 80 * sc, 14 * sc, 0, col); }
            else
            {
                Box(x, by - 130 * sc, 16 * sc, 260 * sc, 0, col);
                for (int b = 0; b < 5; b++) { float sd = b % 2 == 0 ? 1 : -1; Ln(x, by - (100 + b * 36) * sc, x + sd * (60 + b * 24) * sc, by - (170 + b * 50) * sc, 6 * sc, col); Ln(x + sd * (60 + b * 24) * sc, by - (170 + b * 50) * sc, x + sd * (90 + b * 24) * sc, by - (160 + b * 60) * sc, 3 * sc, col); }
            }
            Glow(x, by - 80 * sc, 55 * sc, Ad(L.Th.Accent, .1f + .05f * MathF.Sin(t * 2 + x * .01f)));
        }
        void Temple(float x, float by, float sc, Color col, float h)
        {
            if (h < .5f) { Pagoda(x, by, sc * 1.1f, col); Glow(x, by - 80 * sc, 120 * sc, Ad(L.Th.Accent, .25f + .1f * MathF.Sin(t * 7 + x))); }
            else { Box(x - 70 * sc, by - 110 * sc, 14 * sc, 220 * sc, 0, col); Box(x + 70 * sc, by - 110 * sc, 14 * sc, 220 * sc, 0, col); Box(x, by - 215 * sc, 200 * sc, 18 * sc, 0, col); Box(x, by - 180 * sc, 160 * sc, 9 * sc, 0, col); Glow(x, by - 60 * sc, 90 * sc, Ad(L.Th.Accent, .2f)); }
        }
        void DrawBg()
        {
            var th = L.Th;
            for (int i = 0; i < 56; i++) { float u = i / 55f; Color c = u < .6f ? Color.Lerp(th.Sky0, th.Sky1, u / .6f) : Color.Lerp(th.Sky1, th.Sky2, (u - .6f) / .4f); float y0 = -320 + i * ((H + 340) / 56f); Box(W / 2, y0 + (H + 340) / 112f, W, (H + 340) / 56f + 1, 0, c); }
            if (th.Style != 2) for (int i = 0; i < 140; i++) { float a = .3f + .7f * MathF.Abs(MathF.Sin(t * 1.4f + stars[i, 3])); Box((stars[i, 0] - cam * .02f + W * 3) % W, stars[i, 1] * .8f, stars[i, 2], stars[i, 2], 0, Ad(WH, a * .8f)); }
            float mx = W * .72f - cam * .015f, my = 170;
            Glow(mx, my, 640, Ad(th.Moon, .4f)); Glow(mx, my, 300, Ad(th.Moon, .45f));
            for (int k = 0; k < 7; k++) Tri(mx + (k - 3) * 110, my + 330, 150 + (k % 3) * 50, 780, (k - 3) * .07f + MathF.Sin(t * .3f + k) * .02f, Ad(th.Moon, .05f));
            Circ(mx, my, 98, Color.Lerp(th.Moon, Color.Black, .45f)); Circ(mx - 8, my - 8, 88, Color.Lerp(th.Moon, Color.Black, .15f)); Circ(mx - 16, my - 16, 66, Color.Lerp(th.Moon, WH, .35f) * .6f);
            Circ(mx - 35, my - 10, 22, Color.Black * .15f); Circ(mx + 28, my + 36, 28, Color.Black * .15f); Circ(mx - 6, my + 52, 12, Color.Black * .15f);
            Mnt(.05f, 430, 170, th.Far, 1f); Mnt(.12f, 520, 130, Color.Lerp(th.Far, th.Mid, .5f), 1.8f);
            Layer(.28f, 270, Color.Lerp(th.Far, th.Mid, .7f), th.Style, .75f, G - 30);
            for (int i = 0; i < 3; i++) GlowS(W / 2 + MathF.Sin(t * .15f + i * 2) * 140, 380 + i * 90, 950, 70, Ad(th.Fog, .16f));
            Layer(.5f, 340, th.Mid, th.Style, 1.05f, G - 5);
            for (int i = 0; i < 2; i++) GlowS(W / 2 + MathF.Cos(t * .2f + i * 3) * 160, 470 + i * 70, 1000, 60, Ad(th.Fog, .2f));
            Layer(.78f, 430, th.Near, th.Style, 1.45f, G + 12);
        }

        // ---------- world pieces ----------
        void Seg(float a, float b)
        {
            var th = L.Th; float x0 = Math.Max(a, cam - 80), x1 = Math.Min(b, cam + W + 80); if (x1 <= x0) return; float cx = (x0 + x1) / 2, w = x1 - x0;
            for (int i = 0; i < 14; i++) Box(cx, G + 8 + i * 10, w, 11, 0, Color.Lerp(th.Ground, Color.Black, i / 13f));
            Box(cx, G, w, 3, 0, Ad(th.Accent, .6f)); GlowS(cx, G, w / 2 + 10, 26, Ad(th.Accent, .25f));
            for (int i = (int)(x0 / 14) * 14; i < x1; i += 14) { float h = 8 + Hs(i) * 26, sw = MathF.Sin(t * 1.5f + i * .05f) * .15f; Tri(i + 7, G - h / 2 + 2, 14, h, sw, Color.Lerp(th.Ground, Color.Black, .7f)); }
            if (a > 0) { Box(a + 3, G + 80, 6, 160, 0, Color.Black); for (int j = 0; j < 5; j++) Tri(a + 8, G + 14 + j * 28, 18, 26, 1.57f, Color.Black); }
            Box(b - 3, G + 80, 6, 160, 0, Color.Black); for (int j = 0; j < 5; j++) Tri(b - 8, G + 14 + j * 28, 18, 26, -1.57f, Color.Black);
        }
        void DrawGround()
        {
            float x0 = 0; foreach (var g in L.Gaps) { Seg(x0, g.X); x0 = g.Y; if (g.Y > cam - 100 && g.X < cam + W + 100) { float cx = (g.X + g.Y) / 2; Box(cx, G + 330, g.Y - g.X, 700, 0, new Color(2, 1, 4)); GlowS(cx, G + 60, (g.Y - g.X) * .7f, 90, Ad(L.Th.Fog, .22f)); } }
            Seg(x0, L.GateX + 1200);
        }
        void DrawPlats()
        {
            var th = L.Th;
            foreach (var p in L.Plats)
            {
                if (p.X > cam + W + 100 || p.X + p.W < cam - 100) continue; float cx = p.X + p.W / 2;
                Box(cx, p.Y + 15, p.W, 30, 0, new Color(16, 12, 22)); Box(cx, p.Y + 2, p.W, 5, 0, new Color(42, 34, 54)); Box(cx, p.Y, p.W, 2, 0, Ad(th.Accent, .7f)); GlowS(cx, p.Y, p.W * .6f, 18, Ad(th.Accent, .25f));
                for (float x = p.X + 10; x < p.X + p.W; x += 22) { float h = 14 + Hs(x) * 20; Tri(x, p.Y + 30 + h / 2, 14, h, 3.14159f, new Color(10, 7, 14)); }
                for (float x = p.X + 8; x < p.X + p.W; x += 16) { float h = 6 + Hs(x * 2) * 14; Tri(x, p.Y - h / 2, 10, h, 0, new Color(6, 4, 9)); }
                GlowS(cx, p.Y + 46, p.W * .5f, 14, Ad(th.Accent, .14f));
            }
        }
        void DrawSpikes()
        {
            foreach (var s in L.Spikes)
            {
                if (s.X > cam + W + 100 || s.Y < cam - 100) continue; float cx = (s.X + s.Y) / 2;
                GlowS(cx, G - 8, (s.Y - s.X) / 2 + 60, 36, Ad(RD, .3f));
                for (float x = s.X; x < s.Y; x += 20) { Tri(x + 10, G - 22, 18, 44, 0, new Color(24, 20, 30)); Glow(x + 10, G - 42, 14, Ad(RD, .6f)); Tri(x + 10, G - 36, 6, 14, 0, new Color(150, 20, 30)); }
            }
        }
        void DrawChecks()
        {
            for (int i = 0; i < L.Checks.Count; i++)
            {
                float x = L.Checks[i]; if (x < cam - 200 || x > cam + W + 200) continue; bool on = lit[i]; var ac = L.Th.Accent;
                Box(x, G - 14, 56, 28, 0, new Color(24, 20, 30)); Box(x, G - 52, 22, 60, 0, new Color(30, 26, 38)); Box(x, G - 92, 40, 36, 0, new Color(20, 16, 26)); Tri(x, G - 122, 58, 24, 0, new Color(16, 12, 20));
                if (on)
                {
                    float fl = 1 + .15f * MathF.Sin(t * 9 + x); Glow(x, G - 92, 230 * fl, Ad(ac, .5f)); Glow(x, G - 92, 80, Ad(WH, .6f)); Box(x, G - 92, 20, 22, 0, Ad(ac, .95f));
                    for (int k = 0; k < 6; k++) { float yy = (t * 45 + k * 28) % 170; Glow(x + MathF.Sin(t * 2 + k * 1.7f) * 22, G - 110 - yy, 9, Ad(ac, 1 - yy / 170f)); }
                }
                else Box(x, G - 92, 18, 20, 0, new Color(40, 36, 52));
            }
        }
        void DrawGate()
        {
            float x = L.GateX; if (x < cam - 300 || x > cam + W + 300) return; var ac = L.Th.Accent; Color st = new Color(18, 14, 24);
            if (bossDead) { GlowS(x, G - 190, 150, 230, Ad(ac, .8f)); GlowS(x, G - 190, 70, 160, Ad(WH, .7f)); for (int i = 0; i < 4; i++) Cres(x, G - 190, 70 + i * 22, 140 + i * 28, t * (2 + i) + i, false, Ad(ac, .5f)); for (int k = 0; k < 10; k++) { float yy = (t * 70 + k * 40) % 380; Glow(x + MathF.Sin(t * 2 + k) * 60, G - yy, 12, Ad(WH, .8f)); } }
            else { Box(x, G - 190, 176, 380, 0, new Color(24, 0, 10) * .8f); GlowS(x, G - 190, 120, 220, Ad(RD, .35f + .1f * MathF.Sin(t * 3))); Ln(x - 88, G - 380, x + 88, G, 5, Ad(RD, .6f)); Ln(x + 88, G - 380, x - 88, G, 5, Ad(RD, .6f)); }
            Box(x - 110, G - 190, 38, 380, 0, st); Box(x + 110, G - 190, 38, 380, 0, st); Box(x, G - 385, 300, 26, 0, st); Box(x, G - 345, 240, 12, 0, st); Tri(x, G - 412, 330, 36, 0, st);
            Box(x - 110, G - 190, 3, 380, 0, Ad(ac, .5f)); Box(x + 110, G - 190, 3, 380, 0, Ad(ac, .5f)); Glow(x, G - 385, 160, Ad(ac, bossDead ? .5f : .2f));
        }
        void DrawSouls()
        {
            foreach (var s in L.Souls) { if (s.Got || s.X < cam - 80 || s.X > cam + W + 80) continue; float by = s.Y + MathF.Sin(t * 3 + s.X) * 8; Glow(s.X, by, 60, Ad(VI, .8f)); Circ(s.X, by, 7, new Color(240, 220, 255)); Tri(s.X - 5, by + 14, 8, 22, 3.14159f + MathF.Sin(t * 4) * .3f, Ad(VI, .5f)); }
        }

        // ---------- the Grim Reaper ----------
        static float Ease(float p) => p * p * (3 - 2 * p);
        static float ScytheAng(float at, int cb, bool chg, bool air, bool run, float tt)
        {
            if (at >= 0)
            {
                float p = Cl(at, 0, 1), e = p < .22f ? -.18f * (p / .22f) : -.18f + 1.18f * Ease((p - .22f) / .78f);
                if (cb == 0) return -2.5f + 3.5f * e; if (cb == 1) return 1.3f - 4.0f * e; return -3.5f + 5.9f * e;
            }
            if (chg) return -2.7f; if (air) return -1.35f; if (run) return -.8f + MathF.Sin(tt * 15) * .07f; return -1.15f + MathF.Sin(tt * 2.5f) * .05f;
        }
        Vector2[] ScytheGeom(float X, float Y, int f, float s, float ang, float atp, out Vector2 hand, out Vector2 pole0, out Vector2 pole1)
        {
            Vector2 Lc(float lx, float ly) => new Vector2(X + f * s * lx, Y + s * ly);
            float reach = atp >= 0 ? MathF.Sin(Cl(atp, 0, 1) * MathF.PI) * 16 : 0, ca = MathF.Cos(ang), sa = MathF.Sin(ang);
            float hx = 20 + reach, hy = -82 + (atp >= 0 ? -4 : 0);
            hand = Lc(hx, hy); pole0 = Lc(hx - ca * 72, hy - sa * 72); pole1 = Lc(hx + ca * 104, hy + sa * 104);
            float p0x = hx + ca * 104, p0y = hy + sa * 104, nx = -sa, ny = ca, Rb = 72; var pts = new Vector2[13];
            for (int i = 0; i < 13; i++) { float ph = i / 12f * 2.0f, a = Rb * MathF.Sin(ph), b = Rb * (1 - MathF.Cos(ph)) * .9f; pts[i] = Lc(p0x + nx * a - ca * b, p0y + ny * a - sa * b); }
            return pts;
        }
        void Reaper(float X, float Y, int f, float a, float tt, float vx, bool air, float at, int cb, bool chg, Color ti, float s = 1, bool shadow = true)
        {
            Vector2 Lc(float lx, float ly) => new Vector2(X + f * s * lx, Y + s * ly);
            bool run = Math.Abs(vx) > 60 && !air; float flow = Cl(-vx * f * .055f, -12, 40) + (air ? 10 : 0), bob = run ? MathF.Abs(MathF.Sin(tt * 13)) * -4 : MathF.Sin(tt * 2.2f) * -3;
            if (shadow && s == 1) { float fy = FloorBelow(X, Y); if (fy < 9999) CircS(X, fy, 36, 6, Color.Black * (.55f * a * (1 - Cl((fy - Y) / 300, 0, .8f)))); }
            Y += bob * s; Color dk = new Color(5, 3, 8) * a;
            Glow(Lc(0, -65), 105 * s, Ad(ti, .3f * a));
            for (int i = 0; i < 14; i++)
            {
                float u = i / 13f, ly = -112 + u * 112, w = (24 + u * 62) * (air ? 1.1f : 1), cx = -4 + flow * u * u * 1.2f + MathF.Sin(tt * 3.2f + u * 5) * (1 + 5 * u); Color c = Color.Lerp(new Color(30, 22, 46), new Color(4, 2, 7), u) * a;
                Box(Lc(cx, ly), w * s, 9.5f * s, 0, c); Box(Lc(cx - w / 2, ly), 2.2f * s, 9.5f * s, 0, Ad(ti, .45f * a)); Box(Lc(cx + w / 2, ly), 2f * s, 9.5f * s, 0, Ad(ti, .2f * a));
            }
            for (int j = 0; j < 10; j++)
            {
                float x0 = -40 + j * 8.5f, len = 26 + Hs(j * 3.3f) * 36, sway = MathF.Sin(tt * 4 + j * .9f) * (5 + flow * .25f) + flow * .5f; var p = Lc(x0 + flow * .5f, -4);
                Tri(p.X + f * sway * .5f * s, p.Y + len / 2 * s, 11 * s, len * s, f * (3.14159f + sway * .03f), dk); Glow(Lc(x0 + sway + flow * .5f, -4 + len), 14 * s, Ad(ti, .22f * a));
            }
            Circ(Lc(-2, -110), 21 * s, new Color(14, 9, 22) * a); Circ(Lc(5, -126), 23 * s, new Color(16, 10, 26) * a); Tri(Lc(-24, -114), 28 * s, 48 * s, f * -1.95f, new Color(16, 10, 26) * a); Glow(Lc(-8, -138), 34 * s, Ad(ti, .18f * a));
            Circ(Lc(11, -120), 13.5f * s, Color.Black * a); Circ(Lc(14, -116), 8 * s, new Color(34, 30, 44) * (a * .6f));
            Color ec = ti == CY ? new Color(170, 255, 255) : new Color(224, 200, 255);
            foreach (float ex in new[] { 8f, 17f }) { var e = Lc(ex, -122); Circ(e, 3f * s, ec * a); Glow(e, 17 * s, Ad(ti, a)); if (run || at >= 0) Ln(e, Lc(ex - 24 - flow * .6f, -122 + 2), 2.2f * s, Ad(ti, .5f * a)); }
            float ang = ScytheAng(at, cb, chg, air, run, tt); var pts = ScytheGeom(X, Y, f, s, ang, at, out var hand, out var p0, out var p1);
            var sh1 = Lc(10, -100); var sh2 = Lc(-4, -98); var dir = Vector2.Normalize(p1 - p0); var rear = hand - dir * 52 * s;
            var el1 = (sh1 + hand) / 2 + new Vector2(0, 12 * s); var el2 = (sh2 + rear) / 2 + new Vector2(0, 12 * s);
            Ln(sh2, el2, 8 * s, dk); Ln(el2, rear, 7 * s, dk); Circ(rear, 5 * s, new Color(216, 208, 190) * a);
            Ln(p0, p1, 5.5f * s, new Color(46, 38, 60) * a); for (int k = 1; k <= 3; k++) { var q = Vector2.Lerp(p0, p1, k * .22f); Circ(q, 2.4f * s, Ad(ti, .8f * a)); }
            Circ(p1, 6 * s, new Color(90, 80, 110) * a);
            for (int i = 0; i < 12; i++)
            {
                float u = i / 11f, w = (14 * MathF.Pow(1 - u, .8f) + 1.6f) * s;
                Ln(pts[i], pts[i + 1], w, Color.Lerp(new Color(60, 56, 78), new Color(236, 240, 255), u) * a); Ln(pts[i], pts[i + 1], 3.5f * s * (1 - u * .6f), Ad(ti, (.5f + .5f * u) * a));
            }
            Glow(pts[12], 30 * s, Ad(ti, .9f * a)); Glow(pts[6], (at >= 0 ? 60 : 30) * s, Ad(ti, (at >= 0 ? .55f : .25f) * a));
            Ln(sh1, el1, 9 * s, dk); Ln(el1, hand, 8 * s, dk); Circ(hand, 5.5f * s, new Color(216, 208, 190) * a);
        }

        // ---------- enemies ----------
        void DrawFoe(Foe e)
        {
            int T = e.Type, f = e.F; bool sm = T == 1, ar = T == 2, fl = e.Flash > 0; float s = e.S, al = e.Al * (e.Boss && e.Kind == 1 ? .88f : 1);
            Color col, ac, trim;
            if (e.Boss) { if (e.Kind == 0) { col = new Color(66, 16, 20); ac = new Color(255, 70, 30); trim = OR; } else if (e.Kind == 1) { col = new Color(14, 26, 44); ac = CY; trim = new Color(160, 220, 255); } else { col = new Color(46, 12, 6); ac = new Color(255, 140, 30); trim = new Color(255, 210, 90); } }
            else if (sm) { col = new Color(42, 22, 28); ac = OR; trim = new Color(255, 200, 110); } else if (ar) { col = new Color(28, 22, 46); ac = new Color(164, 92, 255); trim = new Color(200, 160, 255); } else { col = new Color(14, 17, 30); ac = RD; trim = new Color(110, 130, 180); }
            if (fl) col = WH; Color cc = col * al, dk = Color.Lerp(col, Color.Black, .5f) * al;
            Vector2 Lc(float lx, float ly) => new Vector2(e.X + f * s * lx, e.Y + s * ly);
            bool air = e.Y < G - 6; float w = MathF.Sin(e.Walk * 12) * (e.Mv != 0 ? 1 : 0), idle = MathF.Sin(e.Ph * 2.4f) * 2;
            CircS(e.X, G, 40 * s, 7 * s, Color.Black * (.5f * al)); Glow(Lc(0, -60), 90 * s, Ad(ac, (fl ? .6f : .2f) * al));
            if (e.Boss) { Glow(Lc(0, -95), (170 + MathF.Sin(t * 3) * 18) * s * .7f, Ad(ac, .25f * al)); if (e.Kind == 2) for (int k = 0; k < 6; k++) { float yy = (t * 90 + k * 40) % 240; Glow(Lc(MathF.Sin(t * 3 + k) * 40, -yy), 10 * s, Ad(ac, 1 - yy / 240f)); } }
            for (int side = -1; side <= 1; side += 2)
            {
                float ph = w * side; var hip = Lc(side * 6, -44); var knee = air ? Lc(side * 10, -30) : Lc(side * 6 + ph * 10, -24 - MathF.Abs(ph) * 4); var foot = air ? Lc(side * 14, -8) : Lc(side * 6 + ph * 20, -Math.Max(0, ph) * 7);
                Ln(hip, knee, 9 * s, cc); Ln(knee, foot, 8 * s, cc); Box(foot.X + f * 4 * s, foot.Y, 16 * s, 5 * s, 0, dk);
            }
            float bw = sm ? 52 : 38;
            for (int i = 0; i < 7; i++) { float u = i / 6f; var p = Lc(idle * .3f, -92 + u * 48); Box(p, (bw - u * (sm ? 12 : 8)) * s, 8.5f * s, 0, Color.Lerp(cc, dk, u * .6f)); Box(Lc(idle * .3f + (bw - u * 8) / 2, -92 + u * 48), 2 * s, 8.5f * s, 0, Ad(ac, .4f * al)); }
            Tri(Lc(0, -34), (bw + 4) * s, 24 * s, 3.14159f, dk); Box(Lc(0, -46), 34 * s, 5 * s, 0, Ad(trim, .8f * al));
            if (sm) { for (int k = 0; k < 4; k++) Box(Lc(0, -88 + k * 11), bw * s, 2.4f * s, 0, Ad(trim, .5f * al)); foreach (int sd in new[] { -1, 1 }) { Box(Lc(sd * (bw / 2 + 3), -90), 18 * s, 14 * s, 0, Color.Lerp(cc, trim * al, .25f)); Box(Lc(sd * (bw / 2 + 3), -83), 18 * s, 2.5f * s, 0, Ad(trim, .8f * al)); } }
            if (ar) { Box(Lc(-15, -80), 9 * s, 36 * s, f * -.3f, dk); for (int k = 0; k < 3; k++) Tri(Lc(-18 - k * 2, -100), 5 * s, 12 * s, f * -.3f, Ad(trim, .5f * al)); }
            for (int k = 0; k < 6; k++) { var p = Lc(-8 - k * 9 - (air ? 4 : 0), -92 + MathF.Sin(t * 9 + k) * 5 + k * 3 + (e.Mv != 0 ? 0 : -k)); var q = Lc(-8 - (k + 1) * 9, -92 + MathF.Sin(t * 9 + k + 1) * 5 + (k + 1) * 3); if (!sm) Ln(p, q, 4f * s, Ad(ac, .75f * al)); }
            var hd = Lc(idle * .3f, -106); Circ(hd, 13 * s, cc);
            if (sm || e.Boss && e.Kind != 1) { Circ(Lc(0, -112), 17 * s, Color.Lerp(cc, trim * al, .2f)); Box(Lc(0, -104), 38 * s, 4 * s, 0, Ad(trim, .8f * al)); foreach (int sd in new[] { -1, 1 }) Tri(Lc(sd * 11, -130), 8 * s, 30 * s, f * sd * .5f, Ad(ac, .9f * al)); Box(Lc(5, -98), 17 * s, 10 * s, 0, new Color(70, 10, 20) * al); }
            else if (ar) Tri(Lc(0, -124), 64 * s, 28 * s, 0, Color.Lerp(cc, trim * al, .2f)); else { Box(Lc(0, -109), 25 * s, 4 * s, 0, Ad(ac, .8f * al)); }
            if (e.Boss) foreach (int sd in new[] { -1, 1 }) Tri(Lc(sd * 13, -138), 12 * s, 46 * s, f * sd * .45f, Ad(ac, .95f * al));
            var ey = Lc(6, -107); Box(ey, 12 * s, 3.5f * s, 0, ac * al); Glow(ey, 20 * s, Ad(ac, al)); if (e.Mv != 0 || e.St == 2) Ln(ey, Lc(-20, -105), 2.5f * s, Ad(ac, .6f * al));
            float wang; Vector2 sh = Lc(8, -92), gp = Lc(16, -72);
            if (T == 0) { float a1 = e.St == 1 ? -2f : e.St == 2 ? t * 22 : .6f; for (int k = 0; k < 2; k++) { var g = Lc(14 + k * 3, -72 + k * 4); float aa = a1 + k * 1.2f; var tp = g + new Vector2(MathF.Cos(f > 0 ? aa : 3.14159f - aa), MathF.Sin(aa)) * 44 * s; Ln(g, tp, 3.5f * s, new Color(232, 240, 255) * al); Glow(tp, 22 * s, Ad(ac, (e.St == 2 ? .8f : .4f) * al)); } Ln(sh, gp, 8 * s, cc); }
            else if (T == 1 || e.Boss)
            {
                wang = e.St == 1 ? -2.1f : (e.St == 3 && e.Tm > .35f && !e.Boss) || e.St == 5 ? 1.1f : .5f; float len = e.Boss ? 125 : 100; var tp = gp + new Vector2(MathF.Cos(f > 0 ? wang : 3.14159f - wang), MathF.Sin(wang)) * len * s;
                Ln(sh, gp, 9 * s, cc); Ln(gp - (tp - gp) * .2f, tp, 4.5f * s, new Color(228, 236, 255) * al); Ln(gp, tp, 2f * s, Ad(ac, .8f * al)); Glow(tp, (e.St == 1 ? 40 : 24) * s, Ad(ac, (e.St == 1 ? 1 : .5f) * al)); Box(gp, 6 * s, 14 * s, 0, Ad(trim, al));
            }
            else
            {
                var bowc = Lc(28, -76); Cres(bowc.X, bowc.Y, 16 * s, 40 * s, 0, f > 0, new Color(70, 50, 36) * al); Ln(Lc(28, -114), Lc(28 - (e.St == 1 ? 14 : 0), -76), 1.6f * s, Ad(trim, .7f * al)); Ln(Lc(28 - (e.St == 1 ? 14 : 0), -76), Lc(28, -38), 1.6f * s, Ad(trim, .7f * al)); Ln(sh, gp, 8 * s, cc);
                if (e.St == 1) { var tp = Lc(80, -76); Ln(Lc(14, -76), tp, 3 * s, new Color(220, 200, 160) * al); Glow(tp, 30 * s, Ad(ac, al)); }
            }
            if (e.St == 1) { Glow(e.X, e.Y - 150 * s, 55, Ad(RD, .9f)); Txt("!", e.X - 8, e.Y - 195 * s, 46, RD, 0); }
            if (!e.Boss && e.Hp < e.Mh && e.Aggro) { Box(e.X, e.Y - 150 * s, 48, 5, 0, Color.Black * .55f); Box(e.X - 24 + 24 * e.Hp / e.Mh, e.Y - 150 * s, 48 * e.Hp / e.Mh, 5, 0, RD); }
        }

        // ---------- projectiles & effects ----------
        void DrawProj(Proj p)
        {
            if (p.K == "pillar")
            {
                if (p.T0 > 0) { float k = 1 - p.T0 / 1.1f; CircS(p.X, G, 46, 8, Ad(OR, .4f + .5f * k)); Ring(p.X, G - 2, 50 * (1 - k * .3f), OR, .05f); }
                else { float k = Cl(-p.T0 / .45f, 0, 1); GlowS(p.X, G - 150, 70 * (1 - k * .4f), 190, Ad(OR, 1 - k)); Box(p.X, G - 150, 50 * (1 - k), 320, 0, Ad(WH, (1 - k) * .9f)); GlowS(p.X, G, 120, 30, Ad(OR, 1 - k)); }
                return;
            }
            if (p.K == "ras") { float r = p.R; Glow(p.X, p.Y, r * 3, Ad(CY, .9f)); Glow(p.X, p.Y, r * 1.6f, Ad(WH, 1)); for (int i = 0; i < 3; i++) Cres(p.X, p.Y, r * (.7f + i * .25f), r * (.7f + i * .25f), t * 14 + i * 2.1f, false, Ad(WH, .8f)); }
            else if (p.K == "wave") { Tri(p.X, p.Y - 40, 64, 100, 0, Ad(new Color(255, 64, 32), .9f)); Glow(p.X, p.Y - 30, 100, Ad(new Color(255, 64, 32), .6f)); Tri(p.X, p.Y - 30, 28, 60, 0, Ad(WH, .6f)); }
            else if (p.K == "orb") { Glow(p.X, p.Y, 60, Ad(VI, .9f)); Circ(p.X, p.Y, 8, new Color(240, 220, 255)); }
            else if (p.K == "arrow") { float a = MathF.Atan2(p.Vy, p.Vx); Ln(p.X - MathF.Cos(a) * 28, p.Y - MathF.Sin(a) * 28, p.X, p.Y, 3, new Color(220, 200, 160)); Glow(p.X, p.Y, 34, Ad(OR, .95f)); Glow(p.X, p.Y, 14, Ad(WH, 1)); }
            else if (p.Own) { float a = MathF.Atan2(p.Vy, p.Vx); Ln(p.X - MathF.Cos(a) * 14, p.Y - MathF.Sin(a) * 14, p.X + MathF.Cos(a) * 16, p.Y + MathF.Sin(a) * 16, 5, new Color(232, 255, 255)); Glow(p.X, p.Y, 30, Ad(CY, .9f)); }
        }
        void DrawFx()
        {
            foreach (var p in pt) if (p.Dk) { float k = Cl(p.L / p.M, 0, 1); Circ(p.X, p.Y, p.Sz, p.C * (k * .55f)); }
            for (int i = 1; i < trail.Count; i++)
            {
                var q = trail[i]; if (q.Brk) continue; var q0 = trail[i - 1]; float k = Cl(q.L / .3f, 0, 1); Color c = Color.Lerp(CY, VI, .35f);
                Ln(q0.B, q.B, 22 * k, Ad(c, .55f * k)); Ln(q0.B, q.B, 8 * k, Ad(WH, .9f * k)); Ln(q0.A, q.A, 12 * k, Ad(c, .3f * k));
            }
            foreach (var f in fx)
            {
                float p = 1 - f.L / f.M;
                if (f.K == "arc") { float r = f.R; Cres(f.X, f.Y, r * (.7f + .4f * p) * .95f, r * .95f, f.F * (MathF.PI + f.A), f.F < 0, Ad(f.C, 1 - p)); Cres(f.X, f.Y, r * (.7f + .4f * p) * .8f, r * .8f, f.F * (MathF.PI + f.A), f.F < 0, Ad(WH, (1 - p) * .8f)); }
                else if (f.K == "ring") { float r = f.R * MathF.Sqrt(p), th = 10 * (1 - p) + 1; for (int i = 0; i < 40; i++) { float a = i * 6.283f / 40; Box(f.X + MathF.Cos(a) * r, f.Y + MathF.Sin(a) * r, th * 2.4f, th, a + 1.57f, Ad(f.C, 1 - p)); } }
                else if (f.K == "ghost") { if (f.E) Box(f.X, f.Y - 50, 28, 100, 0, Ad(f.C, .35f * (1 - p))); else Reaper(f.X, f.Y, f.F, .4f * (1 - p), 0, 0, false, -1, 0, false, f.C, 1, false); }
            }
            foreach (var p in pt) if (!p.Dk) { float k = Cl(p.L / p.M, 0, 1); Glow(p.X, p.Y, p.Sz * (.6f + k) * 2.4f, Ad(p.C, k)); }
        }

        void DrawWorld()
        {
            var th = L.Th;
            DrawGround(); DrawPlats(); DrawSpikes(); DrawChecks(); DrawGate(); DrawSouls();
            for (int i = 0; i < 120; i++) { bool a = i % 4 != 0; float ax = cam + amb[i, 0]; Glow(ax, amb[i, 1] + camY, amb[i, 4] * (a ? 3 : 2), Ad(th.Style == 0 ? new Color(200, 255, 120) : th.Style == 1 ? new Color(150, 220, 255) : new Color(255, 150, 50), a ? .7f : .4f)); }
            foreach (var k in clones) Reaper(k.X, G, k.F, Cl(k.L, 0, .7f), k.T, k.Mv * 300 * k.F, false, k.At >= 0 ? k.At / .28f : -1, 0, false, CY);
            foreach (var e in en) if (e.X > cam - 300 && e.X < cam + W + 300) DrawFoe(e);
            if (!P.Dead)
            {
                float atp = P.Atk > 0 ? P.AtkT / (P.Cb == 2 ? .42f : .28f) : -1; float al = P.Dash > 0 ? .35f : (P.Inv > 0 && (int)(t * 30) % 2 == 0 ? .4f : 1);
                Reaper(P.X, P.Y, P.F, al, t, P.Vx, !P.Ground, atp, P.Cb, P.Chg > 0, PU);
            }
            if (P.Chg > 0) { float r = 14 + P.Chg * 26, ox = P.X + P.F * 50, oy = P.Y - 76; Glow(ox, oy, r * 3, Ad(CY, .9f)); Glow(ox, oy, r * 1.6f, Ad(WH, 1)); for (int i = 0; i < 3; i++) Cres(ox, oy, r * (.7f + i * .3f), r * (.7f + i * .3f), t * 16 + i * 2.1f, false, Ad(WH, .8f)); }
            foreach (var p in proj) DrawProj(p);
            DrawFx();
            if (ult > 0) Reaper(P.X, G + 40, 1, .5f, t, 0, false, Cl((1 - ult / 1.4f) * 1.3f, 0, 1), 0, false, VI, 3.8f, false);
        }

        // ---------- limbo-style foreground silhouettes ----------
        void DrawForeground()
        {
            Color b = new Color(1, 0, 3);
            for (int i = (int)MathF.Floor(cam * 1.35f / 26) - 1; i < (cam * 1.35f + W) / 26 + 2; i++) { float x = i * 26 - cam * 1.35f + (Hs(i) - .5f) * 14, h = 26 + Hs(i * 3.1f) * 70, sw = MathF.Sin(t * 1.4f + i * .7f) * .22f; Tri(x, H + 8 - h / 2, 16, h, sw, b); }
            for (int i = (int)MathF.Floor(cam * 1.75f / 1500) - 1; i < (cam * 1.75f + W) / 1500 + 2; i++) { float x = i * 1500 - cam * 1.75f + Hs(i * 9.1f) * 400, w = 90 + Hs(i * 2.2f) * 70; if (x < -200 || x > W + 200) continue; Box(x, H / 2, w, H + 40, 0, b * .97f); Ln(x, 120, x + 160, 20, 14, b); Ln(x, 220, x - 150, 150, 10, b); }
            for (int i = (int)MathF.Floor(cam * 1.25f / 230) - 1; i < (cam * 1.25f + W) / 230 + 2; i++) { float x = i * 230 - cam * 1.25f, len = 40 + Hs(i * 5.5f) * 150, sw = MathF.Sin(t * .9f + i) * 10; Ln(x, -4, x + sw * .5f, len * .5f, 3.5f, b); Ln(x + sw * .5f, len * .5f, x + sw, len, 2.5f, b); Tri(x + sw, len + 8, 8, 20, 3.14159f, b); }
            for (int i = 0; i < 3; i++) GlowS(W / 2 + MathF.Sin(t * .22f + i * 2.4f) * 200, H - 40 - i * 30, 900, 50, Ad(L.Th.Fog, .12f));
        }

        // ---------- HUD ----------
        void Bar(float x, float y, float w, float h, float v, Color c1, Color c2)
        {
            Box(x + w / 2, y + h / 2, w + 6, h + 6, 0, Color.Black * .65f); float ww = w * Cl(v, 0, 1);
            for (int i = 0; i < 24; i++) { float u = i / 23f; Box(x + ww * (i + .5f) / 24, y + h / 2, ww / 24 + 1, h, 0, Color.Lerp(c1, c2, u)); }
            Glow(x + ww, y + h / 2, h * 3.2f, Ad(c2, .45f)); Box(x + w / 2, y + 2, w, 2, 0, Color.White * .12f);
        }
        void DrawHud()
        {
            Bar(40, 36, 340, 18, P.Hp / P.Mh, new Color(138, 10, 34), new Color(255, 51, 85)); Bar(40, 64, 260, 10, P.En / 100, new Color(42, 108, 255), CY); Txt("THE REAPER", 40, 4, 26, WH);
            Txt("LEVEL " + (lv + 1), W - 40, 10, 34, new Color(255, 207, 90), 2); Txt("SOULS " + kills, W - 40, 50, 24, VI, 2);
            float prog = Cl(P.X / L.GateX, 0, 1); Box(W / 2, 22, 300, 4, 0, Color.White * .15f); Box(W / 2 - 150 + 150 * prog, 22, 300 * prog, 4, 0, Ad(L.Th.Accent, .9f)); Glow(W / 2 - 150 + 300 * prog, 22, 18, Ad(L.Th.Accent, 1));
            var sk = new[] { ("J", "SLASH", 0), ("K", "KUNAI", 0), ("L", "RASENGAN", 25), ("E", "CLONES", 40), ("Q", "ECLIPSE", 100), ("SHIFT", "STEP", 0) };
            for (int i = 0; i < sk.Length; i++)
            {
                float X = 40 + i * 84; bool ok = P.En >= sk[i].Item3; float a = ok ? 1 : .35f; Color bc = (sk[i].Item1 == "Q" && ok) ? VI : new Color(255, 255, 255, 90);
                Box(X + 37, H - 66, 74, 2, 0, bc * a); Box(X + 37, H - 22, 74, 2, 0, bc * a); Box(X, H - 44, 2, 44, 0, bc * a); Box(X + 74, H - 44, 2, 44, 0, bc * a);
                Txt(sk[i].Item1, X + 37, H - 64, sk[i].Item1.Length > 1 ? 18 : 26, ok ? WH : Color.Gray, 1); Txt(sk[i].Item2, X + 37, H - 38, 14, ok ? CY : Color.DimGray, 1);
            }
            foreach (var e in en) if (e.Boss && e.Aggro) { Bar(W / 2 - 260, 96, 520, 12, e.Hp / e.Mh, new Color(160, 16, 32), new Color(255, 96, 48)); Txt(e.Kind == 0 ? "ONI WARLORD" : e.Kind == 1 ? "THE WRAITH LORD" : "FIRE SHOGUN", W / 2, 58, 30, OR, 1); }
            if (banT > 0) Txt(banS, W / 2, H / 2 - 190, 54, WH * Cl(banT, 0, 1), 1, RD);
        }

        protected override void Draw(GameTime gt)
        {
            cam += (Cl(P.X - W * .4f + P.F * 120 + P.Vx * .12f, 0, Math.Max(0, L.GateX + 400 - W)) - cam) * .08f; camY += (Cl(P.Y - 440, -260, 0) - camY) * .06f;
            float sx = Rf(-1, 1) * shake, sy = Rf(-1, 1) * shake;
            GraphicsDevice.SetRenderTarget(rt); GraphicsDevice.Clear(Color.Black);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.CreateTranslation(sx, sy - camY * .6f, 0)); DrawBg(); sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.CreateTranslation(-cam + sx, -camY + sy, 0)); DrawWorld(); sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.CreateTranslation(sx, sy, 0)); DrawForeground(); sb.End();
            if (ult > 0) { sb.Begin(); sb.Draw(px, new Rectangle(0, 0, W, H), new Color(10, 0, 25) * (.55f * Math.Min(1, ult))); sb.End(); }
            GraphicsDevice.SetRenderTarget(small); sb.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp); sb.Draw(rt, new Rectangle(0, 0, 320, 180), WH); sb.End();
            GraphicsDevice.SetRenderTarget(null); GraphicsDevice.Clear(Color.Black);
            var pp = GraphicsDevice.PresentationParameters; float scl = Math.Min(pp.BackBufferWidth / (float)W, pp.BackBufferHeight / (float)H);
            var m = Matrix.CreateScale(scl) * Matrix.CreateTranslation((pp.BackBufferWidth - W * scl) / 2, (pp.BackBufferHeight - H * scl) / 2, 0);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, m);
            sb.Draw(rt, new Rectangle(0, 0, W, H), WH); sb.Draw(small, new Rectangle(0, 0, W, H), new Color(110, 110, 110, 0));
            Box(W / 2, H / 2, W, H, 0, Ad(L.Th.Accent, .035f)); sb.Draw(vig, new Rectangle(0, 0, W, H), WH); sb.Draw(vig, new Rectangle(0, 0, W, H), Color.White * .5f);
            for (int i = 0; i < 70; i++) Box(Rf(0, W), Rf(0, H), 2, 2, 0, Color.White * Rf(.03f, .09f));
            if (state == "play" && P.Hp < 35) Box(W / 2, H / 2, W, H, 0, new Color(120, 0, 20) * (.12f + .1f * MathF.Sin(t * 8)));
            if (flash > 0) Box(W / 2, H / 2, W, H, 0, WH * (flash * .6f));
            Box(W / 2, 14, W, 28, 0, Color.Black * .85f); Box(W / 2, H - 14, W, 28, 0, Color.Black * .0f);
            if (state == "play") DrawHud();
            if (state == "title")
            {
                Box(W / 2, H / 2, W, H, 0, Color.Black * .5f); Txt("KADAYA", W / 2, 80, 190, new Color(225, 195, 255), 1, PU);
                Txt("THE GRIM REAPER", W / 2, 290, 44, new Color(255, 106, 128), 1, RD);
                if ((int)(t * 1.6f) % 2 == 0) Txt(lv > 0 ? "ENTER  -  CONTINUE  (LEVEL " + (lv + 1) + ")" : "ENTER  -  START", W / 2, 370, 38, WH, 1);
                Txt("N  -  NEW GAME", W / 2, 425, 24, new Color(180, 160, 220), 1);
                Txt("A/D MOVE   W/SPACE JUMP x2   SHIFT SHADOW STEP   J SCYTHE COMBO   K KUNAI", W / 2, H - 120, 22, new Color(207, 192, 255), 1, PU);
                Txt("HOLD L SOUL RASENGAN   E SHADOW CLONES   Q DEATH'S ECLIPSE   F11 FULLSCREEN", W / 2, H - 82, 22, new Color(207, 192, 255), 1, PU);
            }
            if (state == "dead") { Box(W / 2, H / 2, W, H, 0, new Color(20, 0, 10) * .65f); Txt("YOU HAVE BEEN REAPED", W / 2, 200, 84, RD, 1); Txt("LEVEL " + (lv + 1) + "     SOULS " + kills, W / 2, 320, 38, WH, 1); if ((int)(t * 1.6f) % 2 == 0) Txt("ENTER  -  RISE AGAIN FROM THE LAST SHRINE", W / 2, 400, 32, VI, 1); }
            if (state == "win") { Box(W / 2, H / 2, W, H, 0, new Color(10, 0, 20) * .6f); Txt("LEVEL " + (lv + 1) + " COMPLETE", W / 2, 190, 86, new Color(255, 220, 120), 1, OR); Txt("SOULS REAPED  " + kills, W / 2, 310, 38, WH, 1); if (winT > 1 && (int)(t * 1.6f) % 2 == 0) Txt(lv >= 2 ? "ENTER  -  FINISH" : "ENTER  -  NEXT LEVEL", W / 2, 390, 34, VI, 1); }
            if (state == "end") { Box(W / 2, H / 2, W, H, 0, Color.Black * .85f); Txt("THE REAPER'S JOURNEY ENDS", W / 2, 200, 70, new Color(225, 195, 255), 1, PU); Txt("THANK YOU FOR PLAYING KADAYA", W / 2, 310, 36, WH, 1); if ((int)(t * 1.6f) % 2 == 0) Txt("ENTER  -  RETURN TO TITLE", W / 2, 390, 30, VI, 1); }
            sb.End(); base.Draw(gt);
        }
    }
}
