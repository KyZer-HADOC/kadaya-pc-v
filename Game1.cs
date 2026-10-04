using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kadaya
{
    class Foe { public int Type, F, St; public bool Boss, Dead, Did; public float X, Y, Vx, Vy, Hp, Mh, Sp, S, Tm, Flash, Walk, Mv; }
    class Proj { public string K; public bool Own, Spent; public float X, Y, Vx, Vy, L, R, Dmg; }
    class Clone { public float X, L, Cd, At = -1, T, Mv; public int F; }
    class Part { public float X, Y, Vx, Vy, L, M, Sz, G; public Color C; }
    class Fx { public string K; public float X, Y, R, L, M, A; public int F; public Color C; public bool E; }
    class Pl { public float X, Y, Vx, Vy, Hp, Mh = 100, En, CbT, Atk, AtkT, Inv, Dash, Dcd, Chg, Kcd; public int F, J, Cb; public bool Hit, Dead, Run; }

    public class Game1 : Game
    {
        const int W = 1280, H = 720, G = 600, WW = 2600;
        static readonly Color CY = new Color(95, 243, 255), PU = new Color(138, 92, 255), VI = new Color(192, 107, 255), RD = new Color(255, 45, 85), OR = new Color(255, 179, 71), WH = Color.White;

        GraphicsDeviceManager gdm; SpriteBatch sb; RenderTarget2D rt, small; Texture2D px, circ, glow, tri, cres, vig; SpriteFont font;
        static Random rnd = new Random();
        KeyboardState cur, prev;

        string state = "title";
        float t, shake, hitstop, cam, flash, ult, banT, wDelay = 1, spT; bool ultHit = true; string banS = "";
        int wave, kills;
        Pl P = new Pl();
        List<Foe> en = new List<Foe>(); List<Proj> proj = new List<Proj>(); List<Clone> clones = new List<Clone>();
        List<Part> pt = new List<Part>(); List<Fx> fx = new List<Fx>(); List<int> spawnQ = new List<int>(); // 0 ninja 1 samurai 2 thrower 3 boss
        float[,] stars = new float[140, 4]; float[,] amb = new float[110, 5];

        public Game1()
        {
            gdm = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = W, PreferredBackBufferHeight = H };
            Content.RootDirectory = "Content"; IsMouseVisible = false; Window.AllowUserResizing = true; Window.Title = "KADAYA";
        }

        static float Rf(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        static float Cl(float v, float a, float b) => Math.Max(a, Math.Min(b, v));
        static int Sg(float v) => v >= 0 ? 1 : -1;
        static Color Ad(Color c, float k) => new Color(c.R / 255f * k, c.G / 255f * k, c.B / 255f * k, 0f);

        protected override void LoadContent()
        {
            sb = new SpriteBatch(GraphicsDevice); font = Content.Load<SpriteFont>("Font");
            rt = new RenderTarget2D(GraphicsDevice, W, H, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents); small = new RenderTarget2D(GraphicsDevice, 320, 180);
            px = new Texture2D(GraphicsDevice, 1, 1); px.SetData(new[] { Color.White });
            circ = Make(64, (u, v) => { float r = MathF.Sqrt(u * u + v * v); return Pm(Cl((1 - r) * 32, 0, 1)); });
            glow = Make(128, (u, v) => { float r = MathF.Sqrt(u * u + v * v); return Pm(MathF.Pow(Cl(1 - r, 0, 1), 2.2f)); });
            tri = Make(64, (u, v) => Pm((v > -1 && MathF.Abs(u) <= (v + 1) / 2) ? 1 : 0));
            cres = Make(256, (u, v) => { float ao = Cl((1 - MathF.Sqrt(u * u + v * v)) * 128, 0, 1); float ai = Cl((.9f - MathF.Sqrt((u - .42f) * (u - .42f) + v * v)) * 128, 0, 1); return Pm(ao * (1 - ai)); });
            vig = Make(256, (u, v) => { float r = MathF.Sqrt(u * u + v * v); return new Color(0, 0, 0, Cl(MathF.Pow(Cl((r - .5f) / .9f, 0, 1), 1.6f) * .9f, 0, 1)); });
            for (int i = 0; i < 140; i++) { stars[i, 0] = Rf(0, W); stars[i, 1] = Rf(0, 420); stars[i, 2] = Rf(.5f, 2); stars[i, 3] = Rf(0, 6); }
            for (int i = 0; i < 110; i++) { amb[i, 0] = Rf(0, W); amb[i, 1] = Rf(0, H); amb[i, 2] = Rf(-30, -8); amb[i, 3] = Rf(20, 70); amb[i, 4] = Rf(1.5f, 4); }
            Reset();
        }
        static Color Pm(float a) => new Color(a, a, a, a);
        Texture2D Make(int n, Func<float, float, Color> f)
        {
            var tx = new Texture2D(GraphicsDevice, n, n); var c = new Color[n * n];
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++) c[y * n + x] = f((x + .5f) / n * 2 - 1, (y + .5f) / n * 2 - 1);
            tx.SetData(c); return tx;
        }

        // ---------- draw helpers ----------
        void Box(float x, float y, float w, float h, float rot, Color c) => sb.Draw(px, new Vector2(x, y), null, c, rot, new Vector2(.5f), new Vector2(w, h), SpriteEffects.None, 0);
        void Ln(float x1, float y1, float x2, float y2, float th, Color c) { float dx = x2 - x1, dy = y2 - y1; sb.Draw(px, new Vector2(x1, y1), null, c, MathF.Atan2(dy, dx), new Vector2(0, .5f), new Vector2(MathF.Sqrt(dx * dx + dy * dy), th), SpriteEffects.None, 0); }
        void Circ(float x, float y, float r, Color c) => sb.Draw(circ, new Vector2(x, y), null, c, 0, new Vector2(32), r / 32f, SpriteEffects.None, 0);
        void Glow(float x, float y, float r, Color c) => sb.Draw(glow, new Vector2(x, y), null, c, 0, new Vector2(64), r / 64f, SpriteEffects.None, 0);
        void GlowS(float x, float y, float rx, float ry, Color c) => sb.Draw(glow, new Vector2(x, y), null, c, 0, new Vector2(64), new Vector2(rx / 64f, ry / 64f), SpriteEffects.None, 0);
        void Tri(float x, float y, float w, float h, float rot, Color c) => sb.Draw(tri, new Vector2(x, y), null, c, rot, new Vector2(32), new Vector2(w / 64f, h / 64f), SpriteEffects.None, 0);
        void Cres(float x, float y, float rx, float ry, float rot, bool flip, Color c) => sb.Draw(cres, new Vector2(x, y), null, c, rot, new Vector2(128), new Vector2(rx / 128f, ry / 128f), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        void Txt(string s, float x, float y, float px_, Color c, int al = 0, Color? gl = null)
        {
            float k = px_ / 64f; var sz = font.MeasureString(s) * k; float X = al == 1 ? x - sz.X / 2 : al == 2 ? x - sz.X : x; var gc = Ad(gl ?? c, .35f);
            foreach (var o in new[] { new Vector2(-2, 0), new Vector2(2, 0), new Vector2(0, -2), new Vector2(0, 2), new Vector2(-4, 0), new Vector2(4, 0) }) sb.DrawString(font, s, new Vector2(X, y) + o * (px_ / 40f), gc, 0, Vector2.Zero, k, SpriteEffects.None, 0);
            sb.DrawString(font, s, new Vector2(X, y), c, 0, Vector2.Zero, k, SpriteEffects.None, 0);
        }

        // ---------- audio (synthesised, no asset files) ----------
        Dictionary<string, SoundEffect> sc = new Dictionary<string, SoundEffect>();
        void Sfx(float f, float d, int wave = 0, float v = .25f, float sweep = .3f)
        {
            string key = (int)f + "_" + (int)(d * 100) + "_" + wave + "_" + (int)(sweep * 10);
            if (!sc.TryGetValue(key, out var se))
            {
                int sr = 22050, n = (int)(sr * d); var buf = new byte[n * 2]; double ph = 0;
                for (int i = 0; i < n; i++)
                {
                    double u = (double)i / n, fr = f * Math.Pow(Math.Max(.05, sweep), u); ph += fr / sr;
                    double s = wave == 0 ? (ph % 1) * 2 - 1 : wave == 1 ? ((ph % 1) < .5 ? 1 : -1) : Math.Sin(ph * 6.2832);
                    short sv = (short)(s * Math.Pow(1 - u, 2) * short.MaxValue * .4); buf[2 * i] = (byte)(sv & 255); buf[2 * i + 1] = (byte)(sv >> 8);
                }
                se = new SoundEffect(buf, sr, AudioChannels.Mono); sc[key] = se;
            }
            try { se.Play(v, 0, 0); } catch { }
        }

        // ---------- effects ----------
        void Burst(float X, float Y, int n, Color col, float sp = 300, float sz = 3, float g = 600, float life = .6f)
        { for (int i = 0; i < n && pt.Count < 1800; i++) { float a = Rf(0, 6.283f), s = Rf(.2f, 1) * sp; pt.Add(new Part { X = X, Y = Y, Vx = MathF.Cos(a) * s, Vy = MathF.Sin(a) * s, L = Rf(.4f, 1) * life, M = life, Sz = Rf(.5f, 1) * sz, C = col, G = g }); } }
        void Arc(float X, float Y, int f, float r, Color c, float l = .22f, float a = 0) => fx.Add(new Fx { K = "arc", X = X, Y = Y, F = f, R = r, L = l, M = l, C = c, A = a });
        void Ring(float X, float Y, float r, Color c, float l = .5f) => fx.Add(new Fx { K = "ring", X = X, Y = Y, R = r, L = l, M = l, C = c });

        void Reset()
        {
            P = new Pl { X = WW / 2, Y = G, F = 1, Hp = 100, En = 60 }; en.Clear(); proj.Clear(); clones.Clear(); pt.Clear(); fx.Clear(); spawnQ.Clear();
            wave = 0; kills = 0; wDelay = 1.2f; ult = 0; ultHit = true;
        }

        // ---------- combat ----------
        void HurtP(float d, int dir)
        {
            if (P.Inv > 0 || P.Dash > 0 || P.Dead || ult > 0) return;
            P.Hp -= d; P.Inv = .9f; P.Vx = dir * 380; P.Vy = -300; P.Cb = 0; P.Atk = 0; P.Chg = 0; shake = 16; flash = .35f; hitstop = .06f;
            Burst(P.X, P.Y - 70, 22, RD, 400, 4); Sfx(110, .3f, 1, .3f);
            if (P.Hp <= 0) { P.Dead = true; state = "dead"; Burst(P.X, P.Y - 60, 80, VI, 600, 6, 200, 1.6f); shake = 30; }
        }
        void HitE(Foe e, float d, float kx)
        {
            if (e.Dead) return;
            e.Hp -= d; e.Flash = .1f; e.Vx = kx * (e.Boss ? .25f : 1); if (!e.Boss) e.Vy = -200; if (e.St == 1 && !e.Boss) { e.St = 3; e.Tm = .5f; }
            P.En = Math.Min(100, P.En + d * .3f); hitstop = Math.Max(hitstop, .05f); shake = Math.Max(shake, d > 30 ? 18 : 8);
            Burst(e.X, e.Y - 60 * e.S, 14, WH, 420, 3, 500, .35f); Burst(e.X, e.Y - 60 * e.S, 10, RD, 300, 4, 700, .5f); Sfx(160, .12f, 1, .25f, .4f);
            if (e.Hp <= 0)
            {
                e.Dead = true; kills++; Burst(e.X, e.Y - 60 * e.S, 26, VI, 160, 6, -260, 1.5f); Burst(e.X, e.Y - 60 * e.S, 30, WH, 500, 3, 400, .5f);
                Ring(e.X, e.Y - 60, 90, VI, .6f); shake = Math.Max(shake, e.Boss ? 35 : 14); Sfx(80, .4f, 0, .3f, .2f);
            }
        }
        void SlashHit()
        {
            float r = P.Cb == 2 ? 200 : 150, dm = new[] { 12f, 12f, 26f }[P.Cb];
            Arc(P.X, P.Y - 70, P.F, r * (P.Cb == 2 ? 1.1f : .9f), P.Cb == 2 ? VI : CY, .25f, new[] { -.35f, .35f, 0f }[P.Cb]);
            if (P.Cb == 2) { Ring(P.X, P.Y - 60, 170, VI, .4f); Burst(P.X + P.F * 80, P.Y - 70, 25, VI, 500, 4); }
            Sfx(P.Cb == 2 ? 220 : 380, .15f, 0, .25f, .2f);
            foreach (var e in en) { float dx = (e.X - P.X) * P.F; if (dx > -40 && dx < r + 20 * e.S && Math.Abs(e.Y - P.Y) < 150) HitE(e, dm * (1 + wave * .03f), P.F * (P.Cb == 2 ? 700 : 380)); }
            foreach (var p in proj) if (!p.Own && Math.Abs(p.X - P.X) < r && Math.Abs(p.Y - (P.Y - 70)) < 110) { p.L = 0; P.En = Math.Min(100, P.En + 6); Burst(p.X, p.Y, 14, CY, 300, 3); }
        }
        void Explode(Proj p)
        {
            float R = 210 * p.R / 30; Ring(p.X, p.Y, R, CY, .5f); Ring(p.X, p.Y, R * .6f, WH, .35f); Burst(p.X, p.Y, 70, CY, 800, 5, 100, .8f); Burst(p.X, p.Y, 30, WH, 600, 3, 0, .5f);
            shake = 24; flash = .2f; hitstop = .08f; Sfx(90, .5f, 0, .35f, .15f);
            foreach (var e in en) if (Math.Abs(e.X - p.X) < R + 20 && Math.Abs(e.Y - p.Y) < R) HitE(e, p.Dmg, Sg(e.X - p.X) * 600);
        }

        // ---------- spawning ----------
        Foe Mk(int type)
        {
            bool boss = type == 3; int ty = boss ? 1 : type; float[] hp = { 40, 95, 32 }, sp = { 230, 115, 130 }; int side = rnd.Next(2) == 0 ? -1 : 1;
            float h = hp[ty] * (boss ? 9 : 1) * (1 + wave * .07f);
            return new Foe { Type = ty, Boss = boss, X = Cl(P.X + side * (720 + Rf(0, 260)), 50, WW - 50), Y = G, Hp = h, Mh = h, Sp = sp[ty], S = boss ? 1.9f : 1, F = -side, Tm = Rf(.5f, 1.4f) };
        }
        void StartWave()
        {
            wave++; int n = 3 + wave * 2; spawnQ.Clear(); if (wave % 5 == 0) spawnQ.Add(3);
            int[] pool = { 0, 0, 1, 2 }; for (int i = 0; i < n; i++) spawnQ.Add(pool[rnd.Next(wave < 2 ? 2 : wave < 3 ? 3 : 4)]);
            banS = wave % 5 == 0 ? "WAVE " + wave + " - ONI WARLORD" : "WAVE " + wave; banT = 2.4f; Sfx(70, .8f, 0, .3f, .5f);
        }

        // ---------- update ----------
        bool Dn(Keys k) => cur.IsKeyDown(k);
        bool Pr(Keys k) => cur.IsKeyDown(k) && prev.IsKeyUp(k);

        void UpdE(Foe e, float dt)
        {
            float d = P.X - e.X, ad = Math.Abs(d); e.F = d > 0 ? 1 : -1; e.Flash -= dt; e.Vy += 2300 * dt; e.Y += e.Vy * dt; if (e.Y >= G) { e.Y = G; e.Vy = 0; }
            e.X = Cl(e.X + e.Vx * dt, 30, WW - 30); e.Vx *= MathF.Pow(.02f, dt); e.Tm -= dt; e.Mv = 0; int T = e.Type;
            if (e.St == 0)
            {
                if (T == 2) { int w = ad < 330 ? -1 : ad > 540 ? 1 : 0; e.X += w * e.F * e.Sp * dt; e.Mv = w; if (e.Tm <= 0 && ad < 700) { e.St = 1; e.Tm = .5f; } }
                else
                {
                    float rg = T == 1 ? 125 * e.S : 230;
                    if (ad > (T == 1 ? 100 * e.S : 90) && (ad > rg || e.Tm > 0)) { e.X += e.F * e.Sp * dt; e.Mv = 1; }
                    if (e.Tm <= 0 && ad <= rg) { e.St = 1; e.Tm = T == 1 ? .6f : .35f; }
                }
            }
            else if (e.St == 1 && e.Tm <= 0)
            {
                if (T == 0) { e.St = 2; e.Tm = .24f; e.Did = false; Sfx(260, .2f, 0, .2f, .3f); }
                else if (T == 1)
                {
                    Arc(e.X + e.F * 20, e.Y - 70 * e.S, e.F, 120 * e.S, new Color(255, 64, 32), .22f); Sfx(150, .2f, 0, .25f, .3f);
                    if (ad < 150 * e.S && Math.Abs(P.Y - e.Y) < 130) HitP(e.Boss ? 26 : 16, e.F);
                    if (e.Boss) { proj.Add(new Proj { K = "wave", X = e.X, Y = G - 30, Vx = e.F * 480, L = 2, R = 30, Dmg = 16 }); shake = 14; }
                    e.St = 3; e.Tm = e.Boss ? .8f : .7f;
                }
                else { proj.Add(new Proj { K = "star", X = e.X + e.F * 20, Y = G - 70, Vx = e.F * 560, L = 2.5f, R = 10, Dmg = 9 }); e.St = 3; e.Tm = .6f; Sfx(500, .1f, 2, .15f, .5f); }
            }
            else if (e.St == 2)
            {
                e.X = Cl(e.X + e.F * 820 * dt, 30, WW - 30);
                if (!e.Did && ad < 60 && Math.Abs(P.Y - e.Y) < 120) { e.Did = true; HitP(12, e.F); }
                fx.Add(new Fx { K = "ghost", X = e.X, Y = e.Y, F = e.F, L = .2f, M = .2f, C = RD, E = true });
                if (e.Tm <= 0) { e.St = 3; e.Tm = .55f; }
            }
            else if (e.St == 3 && e.Tm <= 0) { e.St = 0; e.Tm = Rf(.7f, 1.6f); }
            e.Walk += dt * (e.Mv != 0 ? 1 : 0);
        }
        void HitP(float d, int dir) => HurtP(d, dir);

        void Update(float dt)
        {
            if (banT > 0) banT -= dt;
            bool Lf = Dn(Keys.A) || Dn(Keys.Left), Rt = Dn(Keys.D) || Dn(Keys.Right); int dir = (Rt ? 1 : 0) - (Lf ? 1 : 0);
            if (!P.Dead)
            {
                P.Inv -= dt; P.Dcd -= dt; P.Kcd -= dt; P.CbT -= dt; bool gr = P.Y >= G;
                if (P.Dash > 0) { P.Dash -= dt; P.X += P.F * 1150 * dt; P.Vy = 0; fx.Add(new Fx { K = "ghost", X = P.X, Y = P.Y, F = P.F, L = .3f, M = .3f, C = PU }); }
                else
                {
                    float sl = P.Chg > 0 ? .35f : P.Atk > 0 ? .6f : 1; P.Vx += (dir * 410 * sl - P.Vx) * Math.Min(1, dt * (gr ? 14 : 6)); if (dir != 0 && P.Atk <= 0 && P.Chg <= 0) P.F = dir;
                    if ((Pr(Keys.Space) || Pr(Keys.W) || Pr(Keys.Up)) && P.J < 2) { P.Vy = -850; P.J++; Burst(P.X, P.Y, 12, PU, 220, 3, 300, .4f); Sfx(320, .15f, 2, .2f, 2); }
                    P.Vy += 2300 * dt; P.X += P.Vx * dt; P.Y += P.Vy * dt; if (P.Y >= G) { P.Y = G; P.Vy = 0; P.J = 0; }
                }
                P.X = Cl(P.X, 40, WW - 40); P.Run = gr && Math.Abs(P.Vx) > 60;
                if (Pr(Keys.LeftShift) && P.Dcd <= 0) { P.Dash = .17f; P.Dcd = .65f; P.Inv = Math.Max(P.Inv, .3f); Burst(P.X, P.Y - 60, 20, PU, 300, 4); Sfx(180, .22f, 2, .3f, 3); }
                if (P.Atk > 0) { P.AtkT += dt; P.Atk -= dt; if (P.Hit && P.AtkT > .07f) { P.Hit = false; SlashHit(); } }
                if (Pr(Keys.J) && P.Dash <= 0 && P.Chg <= 0 && (P.Atk <= 0 || P.AtkT > .17f)) { P.Cb = P.CbT > 0 ? (P.Cb + 1) % 3 : 0; P.Atk = P.Cb == 2 ? .42f : .28f; P.AtkT = 0; P.Hit = true; P.CbT = .75f; P.Vx = P.F * (P.Cb == 2 ? 450 : 280); }
                if (Pr(Keys.K) && P.Kcd <= 0 && P.Dash <= 0) { P.Kcd = .2f; foreach (var a in new[] { -.12f, 0f, .12f }) proj.Add(new Proj { Own = true, K = "kunai", X = P.X + P.F * 30, Y = P.Y - 72, Vx = P.F * 1150, Vy = a * 1150 * .6f, Dmg = 9, L = 1.1f, R = 8 }); Sfx(700, .1f, 2, .15f, .5f); }
                if (Dn(Keys.L) && P.En >= 25 && P.Atk <= 0 && P.Dash <= 0)
                {
                    P.Chg = Math.Min(1.5f, P.Chg + dt); float a = Rf(0, 6.283f), r = Rf(60, 120);
                    pt.Add(new Part { X = P.X + P.F * 45 + MathF.Cos(a) * r, Y = P.Y - 75 + MathF.Sin(a) * r, Vx = -MathF.Cos(a) * r * 4, Vy = -MathF.Sin(a) * r * 4, L = .25f, M = .25f, Sz = 3, C = CY });
                }
                else if (P.Chg > 0)
                {
                    if (P.Chg > .3f && P.En >= 25) { float sz = .6f + P.Chg * .8f; proj.Add(new Proj { Own = true, K = "ras", X = P.X + P.F * 60, Y = P.Y - 75, Vx = P.F * 560, R = 30 * sz, Dmg = 32 + P.Chg * 22, L = 1.5f }); P.En -= 25; shake = 10; Sfx(200, .4f, 0, .3f, .2f); }
                    P.Chg = 0;
                }
                if (Pr(Keys.E) && P.En >= 40) { P.En -= 40; foreach (var s in new[] { -1, 1 }) { clones.Add(new Clone { X = P.X + s * 90, F = P.F, L = 7, Cd = .2f }); Burst(P.X + s * 90, G - 60, 30, new Color(223, 233, 255), 300, 6, -100, .7f); } Sfx(400, .3f, 2, .25f, 3); }
                if (Pr(Keys.Q) && P.En >= 100 && ult <= 0) { P.En = 0; ult = 1.4f; ultHit = false; P.Inv = 2; Sfx(60, 1.2f, 0, .4f, .5f); }
            }
            if (ult > 0)
            {
                ult -= dt;
                if (!ultHit && ult < 1)
                {
                    ultHit = true; flash = 1; shake = 45; foreach (var e in en) if (Math.Abs(e.X - P.X) < 1100) HitE(e, 95, Sg(e.X - P.X) * 500);
                    for (int i = 0; i < 6; i++) Arc(P.X + (i - 2.5f) * 220, P.Y - 130, i % 2 == 0 ? -1 : 1, 300 + i * 20, VI, .5f, Rf(-.6f, .6f));
                    Ring(P.X, P.Y - 80, 900, VI, .8f); Burst(P.X, P.Y - 80, 150, VI, 1200, 6, 0, 1);
                }
            }
            if (ult <= .9f) foreach (var e in en) UpdE(e, dt);
            en.RemoveAll(e => e.Dead);
            foreach (var k in clones)
            {
                k.L -= dt; k.T += dt; k.Cd -= dt; k.Mv = 0; Foe tg = null; float bd = 9e9f;
                foreach (var e in en) { float d = Math.Abs(e.X - k.X); if (d < bd) { bd = d; tg = e; } }
                if (tg != null) { k.F = Sg(tg.X - k.X); if (bd > 100) { k.X += k.F * 330 * dt; k.Mv = 1; } else if (k.Cd <= 0) { k.Cd = .55f; k.At = 0; Arc(k.X + k.F * 30, G - 70, k.F, 120, new Color(223, 233, 255), .2f); HitE(tg, 10, k.F * 300); } }
                else { k.F = Sg(P.X - k.X); if (Math.Abs(P.X - k.X) > 140) { k.X += k.F * 300 * dt; k.Mv = 1; } }
                if (k.At >= 0) { k.At += dt; if (k.At > .28f) k.At = -1; }
                if (k.L <= 0) Burst(k.X, G - 60, 25, new Color(223, 233, 255), 250, 5, -100, .6f);
            }
            clones.RemoveAll(k => k.L <= 0);
            foreach (var p in proj)
            {
                p.L -= dt; p.X += p.Vx * dt; p.Y += p.Vy * dt;
                if (p.Own)
                {
                    pt.Add(new Part { X = p.X, Y = p.Y, Vx = Rf(-30, 30), Vy = Rf(-30, 30), L = .3f, M = .3f, Sz = p.K == "ras" ? p.R * .5f : 3, C = CY });
                    foreach (var e in en)
                        if (!e.Dead && p.L > 0 && Math.Abs(p.X - e.X) < 26 * e.S + p.R && p.Y > e.Y - 125 * e.S && p.Y < e.Y + 10)
                        { if (p.K == "ras") { Explode(p); p.Spent = true; } else HitE(e, p.Dmg, Sg(p.Vx) * 250); p.L = 0; }
                }
                else
                {
                    if (p.K == "wave") Burst(p.X, G - 5, 2, new Color(255, 64, 32), 100, 5, -300, .4f);
                    if (Math.Abs(p.X - P.X) < p.R + 16 && p.Y > P.Y - 115 && p.Y < P.Y + 10 && p.L > 0) { HurtP(p.Dmg, Sg(p.Vx)); p.L = 0; }
                }
                if (p.X < 0 || p.X > WW) p.L = 0;
            }
            foreach (var p in proj) if (p.L <= 0 && p.K == "ras" && !p.Spent) { p.Spent = true; Explode(p); }
            proj.RemoveAll(p => p.L <= 0);
            if (state == "play")
            {
                if (en.Count == 0 && spawnQ.Count == 0) { wDelay -= dt; if (wDelay <= 0) { if (wave > 0) { P.Hp = Math.Min(P.Mh, P.Hp + 20); P.En = Math.Min(100, P.En + 30); } StartWave(); wDelay = 2.2f; } }
                spT -= dt; if (spawnQ.Count > 0 && spT <= 0) { spT = .8f; int ty = spawnQ[0]; spawnQ.RemoveAt(0); en.Add(Mk(ty)); }
            }
        }
        void UpdFx(float dt)
        {
            foreach (var p in pt) { p.L -= dt; p.Vy += p.G * dt; p.X += p.Vx * dt; p.Y += p.Vy * dt; }
            pt.RemoveAll(p => p.L <= 0); foreach (var f in fx) f.L -= dt; fx.RemoveAll(f => f.L <= 0);
            for (int i = 0; i < 110; i++) { amb[i, 0] += amb[i, 2] * dt + MathF.Sin(t + amb[i, 1]) * .3f; amb[i, 1] += amb[i, 3] * dt; if (amb[i, 1] > H) { amb[i, 1] = -5; amb[i, 0] = Rf(0, W); } if (amb[i, 0] < 0) amb[i, 0] = W; }
            shake = Math.Max(0, shake - 70 * dt); flash = Math.Max(0, flash - dt * 1.5f);
        }

        protected override void Update(GameTime gt)
        {
            prev = cur; cur = Keyboard.GetState(); float dt = Math.Min(.033f, (float)gt.ElapsedGameTime.TotalSeconds); t += dt;
            if (Pr(Keys.Escape)) Exit();
            if (Pr(Keys.F11)) gdm.ToggleFullScreen();
            if ((state == "title" || state == "dead") && Pr(Keys.Enter)) { Reset(); state = "play"; wDelay = 1; }
            if (state == "play") { if (hitstop > 0) { hitstop -= dt; dt *= .1f; } Update(dt); }
            UpdFx(dt); base.Update(gt);
        }

        // ---------- drawing ----------
        void Mnt(float par, float baseY, float amp, Color col, float f)
        {
            float o = cam * par;
            for (int i = 0; i <= W; i += 16) { float u = i + o; float y = baseY - amp * (MathF.Sin(u * .006f * f) * .5f + MathF.Sin(u * .0023f * f + 2) * .35f + MathF.Sin(u * .015f * f) * .15f + .6f); Box(i + 8, (y + H) / 2, 17, H - y, 0, col); }
        }
        void Pagoda(float px_, float baseY, float s, Color col)
        {
            for (int i = 0; i < 4; i++)
            {
                float w = (110 - i * 20) * s, h = 46 * s, y = baseY - i * h;
                Box(px_, y - h * .35f, w - 16 * s, h * .7f, 0, col); Tri(px_, y - h * .75f, w + 56 * s, h * .55f, 0, col);
            }
            Box(px_, baseY - 4 * 46 * s - 17 * s, 4 * s, 34 * s, 0, col);
        }
        void DrawBg()
        {
            for (int i = 0; i < 40; i++) { float u = i / 39f; Color c = u < .5f ? Color.Lerp(new Color(5, 2, 12), new Color(38, 9, 44), u * 2) : Color.Lerp(new Color(38, 9, 44), new Color(138, 26, 52), (u - .5f) * 2); Box(W / 2, i * (G / 40f) + G / 80f, W, G / 40f + 1, 0, c); }
            Box(W / 2, G + (H - G) / 2, W, H - G, 0, Color.Black);
            for (int i = 0; i < 140; i++) { float a = .4f + .6f * MathF.Abs(MathF.Sin(t * 1.5f + stars[i, 3])); Box((stars[i, 0] - cam * .02f + W) % W, stars[i, 1], stars[i, 2], stars[i, 2], 0, Ad(WH, a)); }
            float mx = 880 - cam * .03f, my = 210; Glow(mx, my, 520, Ad(new Color(255, 50, 60), .55f)); Circ(mx, my, 115, new Color(163, 16, 42)); Circ(mx - 10, my - 10, 98, new Color(210, 60, 55)); Circ(mx - 20, my - 20, 70, new Color(255, 125, 105) * .6f);
            Circ(mx - 40, my - 20, 24, new Color(60, 0, 20) * .25f); Circ(mx + 30, my + 40, 30, new Color(60, 0, 20) * .25f); Circ(mx - 10, my + 55, 14, new Color(60, 0, 20) * .25f);
            Mnt(.08f, 430, 150, new Color(29, 10, 42), 1); Mnt(.18f, 500, 120, new Color(21, 7, 32), 1.7f);
            for (int k = 0; k < 3; k++) Pagoda(((k * 640 - cam * .3f) % 1920 + 1920) % 1920 - 260, G - 35, 1.05f, new Color(14, 4, 22));
            Mnt(.4f, 590, 70, new Color(10, 3, 15), 2.6f);
            for (int i = 0; i < 2; i++) GlowS(W / 2 + MathF.Sin(t * .2f + i) * 80, 420 + i * 110, 900, 80, Ad(new Color(180, 60, 120), .25f));
        }
        void DrawGround()
        {
            for (int i = 0; i < 12; i++) Box(cam + W / 2, G + 10 + i * 10, W, 11, 0, Color.Lerp(new Color(26, 10, 28), Color.Black, i / 11f));
            Box(cam + W / 2, G, W, 3, 0, RD); GlowS(cam + W / 2, G, 700, 30, Ad(RD, .6f));
            for (int i = (int)(cam / 16) * 16; i < cam + W + 16; i += 16) { float h = 10 + MathF.Abs(MathF.Sin(i * 12.9f)) * 22; Tri(i + 8, G - h / 2, 16, h, 0, new Color(7, 2, 10)); }
        }
        void Reaper(float X, float Y, int f, float a, float tt, bool run, float at, int cb, bool chg, Color ti, float s = 1)
        {
            Vector2 L(float lx, float ly) => new Vector2(X + f * s * lx, Y + s * ly);
            float b = MathF.Sin(tt * (run ? 15 : 3)) * (run ? 4 : 2); var g0 = L(0, -65);
            Glow(g0.X, g0.Y, 95 * s, Ad(ti, .35f * a));
            for (int i = 0; i < 10; i++)
            {
                float ly = -112 + i * 10.5f, w = 24 + i * 5.2f, cx = -5 - i * (run ? 1.6f : .5f); var c = L(cx, ly + 5); Color col = Color.Lerp(new Color(28, 20, 44), new Color(5, 3, 8), i / 9f) * a;
                Box(c.X, c.Y, w * s, 11.5f * s, 0, col); var e1 = L(cx - w / 2, ly + 5); var e2 = L(cx + w / 2, ly + 5); Box(e1.X, e1.Y, 2 * s, 11.5f * s, 0, Ad(ti, .5f * a)); Box(e2.X, e2.Y, 2 * s, 11.5f * s, 0, Ad(ti, .25f * a));
            }
            for (int i = 0; i < 7; i++) { var h = L(-30 + i * 10, -2 + (i % 2 == 0 ? 4 : -6) + MathF.Sin(tt * 6 + i) * 3 + b * .3f); Tri(h.X, h.Y, 12 * s, 24 * s, MathF.PI, new Color(5, 3, 8) * a); }
            var hd = L(4, -124); Circ(hd.X, hd.Y, 22 * s, new Color(16, 10, 26) * a); var tp = L(-22, -112); Tri(tp.X, tp.Y, 26 * s, 44 * s, f * -1.9f, new Color(16, 10, 26) * a);
            var fc = L(10, -119); Circ(fc.X, fc.Y, 12.5f * s, Color.Black * a);
            Color ec = ti == CY ? new Color(170, 255, 255) : new Color(224, 200, 255); foreach (float ex in new[] { 7f, 15f }) { var e = L(ex, -120); Circ(e.X, e.Y, 3 * s, ec * a); Glow(e.X, e.Y, 15 * s, Ad(ti, .9f * a)); }
            float ang; if (at >= 0) { float p = Cl(at, 0, 1), e = p * p * (3 - 2 * p), a0 = -2.6f + (cb == 1 ? .7f : 0); ang = a0 + (1.5f - a0) * e; } else ang = chg ? -2.3f : -1.1f + MathF.Sin(tt * 3) * .06f;
            float ca = MathF.Cos(ang), sa = MathF.Sin(ang);
            Vector2 Sc(float dx, float dy) => L(16 + dx * ca - dy * sa, -72 + dx * sa + dy * ca);
            var p1 = Sc(-28, 0); var p2 = Sc(98, 0); Ln(p1.X, p1.Y, p2.X, p2.Y, 5 * s, new Color(42, 36, 56) * a);
            var bc = Sc(104, 40); Glow(bc.X, bc.Y, 90 * s, Ad(ti, .35f * a)); Cres(bc.X, bc.Y, 62 * s, 62 * s, f * (ang - .5f), f < 0, Color.Lerp(ti, WH, .6f) * a);
            var hn = Sc(0, 0); Circ(hn.X, hn.Y, 5 * s, new Color(217, 211, 198) * a);
        }
        void DrawFoe(Foe e)
        {
            int T = e.Type; bool sm = T == 1, fl = e.Flash > 0; float s = e.S, w = MathF.Sin(e.Walk * 12) * 10 * e.Mv, bw = sm ? 25 : 18; int f = e.F;
            Color col = fl ? WH : sm ? new Color(43, 18, 22) : T == 2 ? new Color(29, 21, 51) : new Color(15, 18, 32), ac = sm ? OR : T == 2 ? new Color(164, 92, 255) : RD;
            Vector2 L(float lx, float ly) => new Vector2(e.X + f * s * lx, e.Y + s * ly);
            Glow(e.X, e.Y - 60 * s, 80 * s, Ad(ac, fl ? .6f : .22f));
            var a1 = L(-7, -42); var a2 = L(-7 + w, 0); var b1 = L(7, -42); var b2 = L(7 - w, 0); Ln(a1.X, a1.Y, a2.X, a2.Y, 10 * s, col); Ln(b1.X, b1.Y, b2.X, b2.Y, 10 * s, col);
            for (int i = 0; i < 5; i++) { var p = L(-14 - i * 9, -96 + MathF.Sin(t * 9 + i) * 5 + i * 3); var q = L(-14 - (i + 1) * 9, -96 + MathF.Sin(t * 9 + i + 1) * 5 + (i + 1) * 3); Ln(p.X, p.Y, q.X, q.Y, 3.5f * s, Ad(ac, .8f)); }
            var tr = L(0, -66); Box(tr.X, tr.Y, (bw * 2 - 4) * s, 52 * s, 0, col); var tt = L(0, -88); Tri(tt.X, tt.Y, (bw * 2 + 6) * s, 14 * s, MathF.PI, col);
            var e1 = L(-bw + 2, -66); var e2 = L(bw - 2, -66); Box(e1.X, e1.Y, 2 * s, 52 * s, 0, Ad(ac, .5f)); Box(e2.X, e2.Y, 2 * s, 52 * s, 0, Ad(ac, .5f));
            if (sm) { var sp = L(-bw - 2, -90); var sq = L(bw + 2, -90); Box(sp.X, sp.Y, 16 * s, 14 * s, 0, fl ? WH : new Color(90, 30, 38)); Box(sq.X, sq.Y, 16 * s, 14 * s, 0, fl ? WH : new Color(90, 30, 38)); var bl = L(0, -62); Box(bl.X, bl.Y, 32 * s, 3 * s, 0, ac); }
            var hd = L(0, -106); Circ(hd.X, hd.Y, 13 * s, col); var ey = L(6, -108); Box(ey.X, ey.Y, 11 * s, 3.5f * s, 0, ac); Glow(ey.X, ey.Y, 16 * s, Ad(ac, .9f));
            if (sm) { var h1 = L(-14, -132); var h2 = L(14, -132); Tri(h1.X, h1.Y, 8 * s, 22 * s, f * -.5f, ac); Tri(h2.X, h2.Y, 8 * s, 22 * s, f * .5f, ac); var hm = L(0, -120); Box(hm.X, hm.Y, 30 * s, 5 * s, 0, ac); }
            if (T == 2) { var ht = L(0, -126); Tri(ht.X, ht.Y, 56 * s, 28 * s, 0, new Color(42, 31, 64)); }
            float ang = e.St == 1 ? -2f : (e.St == 2 || (e.St == 3 && e.Tm > .3f && sm)) ? 1f : .35f, len = sm ? 85 : 55; var h0 = L(14, -70); var h3 = L(14 + len * MathF.Cos(ang), -70 + len * MathF.Sin(ang));
            Ln(h0.X, h0.Y, h3.X, h3.Y, 3.5f * s, new Color(232, 240, 255)); Glow(h3.X, h3.Y, 20 * s, Ad(WH, .5f));
            if (e.St == 1) { Glow(e.X, e.Y - 150 * s, 50, Ad(RD, .9f)); Txt("!", e.X - 8, e.Y - 190 * s, 46, RD, 0); }
            if (!e.Boss && e.Hp < e.Mh) { Box(e.X, e.Y - 148, 48, 5, 0, Color.Black * .55f); Box(e.X - 24 + 24 * e.Hp / e.Mh, e.Y - 148, 48 * e.Hp / e.Mh, 5, 0, RD); }
        }
        void DrawProj(Proj p)
        {
            if (p.K == "ras") { float r = p.R; Glow(p.X, p.Y, r * 3, Ad(CY, .9f)); Glow(p.X, p.Y, r * 1.6f, Ad(WH, 1)); for (int i = 0; i < 3; i++) Cres(p.X, p.Y, r * (.7f + i * .25f), r * (.7f + i * .25f), t * 14 + i * 2.1f, false, Ad(WH, .8f)); }
            else if (p.K == "wave") { Tri(p.X, p.Y - 40, 60, 90, 0, Ad(new Color(255, 64, 32), .9f)); Glow(p.X, p.Y - 30, 90, Ad(new Color(255, 64, 32), .6f)); }
            else if (p.Own) { float a = MathF.Atan2(p.Vy, p.Vx); Ln(p.X - MathF.Cos(a) * 14, p.Y - MathF.Sin(a) * 14, p.X + MathF.Cos(a) * 16, p.Y + MathF.Sin(a) * 16, 5, new Color(232, 255, 255)); Glow(p.X, p.Y, 28, Ad(CY, .9f)); }
            else { Box(p.X, p.Y, 24, 6, t * 25, new Color(255, 64, 96)); Box(p.X, p.Y, 24, 6, t * 25 + 1.57f, new Color(255, 64, 96)); Glow(p.X, p.Y, 30, Ad(RD, .9f)); }
        }
        void DrawFx()
        {
            foreach (var f in fx)
            {
                float p = 1 - f.L / f.M;
                if (f.K == "arc") { float r = f.R; Cres(f.X, f.Y, r * (.7f + .4f * p) * .95f, r * .95f, f.F * (MathF.PI + f.A), f.F < 0, Ad(f.C, 1 - p)); Cres(f.X, f.Y, r * (.7f + .4f * p) * .8f, r * .8f, f.F * (MathF.PI + f.A), f.F < 0, Ad(WH, (1 - p) * .8f)); }
                else if (f.K == "ring") { float r = f.R * MathF.Sqrt(p), th = 10 * (1 - p) + 1; int n = 36; for (int i = 0; i < n; i++) { float a = i * 6.283f / n; Box(f.X + MathF.Cos(a) * r, f.Y + MathF.Sin(a) * r, th * 2.2f, th, a + 1.57f, Ad(f.C, 1 - p)); } }
                else if (f.K == "ghost") { if (f.E) Box(f.X, f.Y - 50, 28, 100, 0, Ad(f.C, .35f * (1 - p))); else Reaper(f.X, f.Y, f.F, .4f * (1 - p), 0, false, -1, 0, false, f.C); }
            }
            foreach (var p in pt) { float k = Cl(p.L / p.M, 0, 1); Glow(p.X, p.Y, p.Sz * (.6f + k) * 2.2f, Ad(p.C, k)); }
        }
        void Bar(float x, float y, float w, float h, float v, Color c1, Color c2)
        { Box(x + w / 2, y + h / 2, w + 4, h + 4, 0, Color.Black * .6f); float ww = w * Cl(v, 0, 1); for (int i = 0; i < 20; i++) { float u = i / 19f; Box(x + ww * (i + .5f) / 20, y + h / 2, ww / 20 + 1, h, 0, Color.Lerp(c1, c2, u)); } Glow(x + ww, y + h / 2, h * 3, Ad(c2, .4f)); }
        void DrawHud()
        {
            Bar(40, 36, 340, 18, P.Hp / P.Mh, new Color(138, 10, 34), new Color(255, 51, 85)); Bar(40, 62, 260, 10, P.En / 100, new Color(42, 108, 255), CY); Txt("REAPER", 40, 6, 26, WH);
            Txt("WAVE " + wave, W - 40, 14, 40, new Color(255, 207, 90), 2); Txt("SOULS " + kills, W - 40, 58, 28, VI, 2);
            var sk = new[] { ("J", "SLASH", 0), ("K", "KUNAI", 0), ("L", "RASENGAN", 25), ("E", "CLONES", 40), ("Q", "ECLIPSE", 100), ("SHIFT", "STEP", 0) };
            for (int i = 0; i < sk.Length; i++)
            {
                float X = 40 + i * 84; bool ok = P.En >= sk[i].Item3; float a = ok ? 1 : .35f; Color bc = (sk[i].Item1 == "Q" && ok) ? VI : new Color(255, 255, 255, 90);
                Box(X + 37, H - 66, 74, 2, 0, bc * a); Box(X + 37, H - 22, 74, 2, 0, bc * a); Box(X, H - 44, 2, 44, 0, bc * a); Box(X + 74, H - 44, 2, 44, 0, bc * a);
                Txt(sk[i].Item1, X + 37, H - 64, sk[i].Item1.Length > 1 ? 18 : 26, ok ? WH : Color.Gray, 1); Txt(sk[i].Item2, X + 37, H - 38, 14, ok ? CY : Color.DimGray, 1);
            }
            foreach (var e in en) if (e.Boss) { Bar(W / 2 - 250, 90, 500, 12, e.Hp / e.Mh, new Color(160, 16, 32), new Color(255, 96, 48)); Txt("ONI WARLORD", W / 2, 54, 28, OR, 1); }
            if (banT > 0) Txt(banS, W / 2, H / 2 - 190, 72, WH * Cl(banT, 0, 1), 1, RD);
        }

        protected override void Draw(GameTime gt)
        {
            cam += (Cl(P.X - W / 2, 0, WW - W) - cam) * .12f; float sx = Rf(-1, 1) * shake, sy = Rf(-1, 1) * shake;
            GraphicsDevice.SetRenderTarget(rt); GraphicsDevice.Clear(Color.Black);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.CreateTranslation(sx, sy, 0)); DrawBg(); sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.CreateTranslation(-cam + sx, sy, 0));
            DrawGround();
            for (int i = 0; i < 110; i++) { bool pet = i % 5 < 3; Box(cam + amb[i, 0], amb[i, 1], amb[i, 4], amb[i, 4] * (pet ? .6f : 1), 0, pet ? Ad(new Color(255, 158, 200), .7f) : Ad(new Color(255, 138, 48), .8f)); }
            foreach (var k in clones) Reaper(k.X, G, k.F, Cl(k.L, 0, .7f), k.T, k.Mv != 0, k.At >= 0 ? k.At / .28f : -1, 0, false, CY);
            foreach (var e in en) DrawFoe(e);
            if (!P.Dead) Reaper(P.X, P.Y, P.F, P.Dash > 0 ? .35f : (P.Inv > 0 && (int)(t * 30) % 2 == 0 ? .4f : 1), t, P.Run, P.Atk > 0 ? P.AtkT / (P.Cb == 2 ? .42f : .28f) : -1, P.Cb, P.Chg > 0, PU);
            if (P.Chg > 0) { float r = 14 + P.Chg * 26, ox = P.X + P.F * 50, oy = P.Y - 76; Glow(ox, oy, r * 3, Ad(CY, .9f)); Glow(ox, oy, r * 1.6f, Ad(WH, 1)); for (int i = 0; i < 3; i++) Cres(ox, oy, r * (.7f + i * .3f), r * (.7f + i * .3f), t * 16 + i * 2.1f, false, Ad(WH, .8f)); }
            foreach (var p in proj) DrawProj(p); DrawFx();
            if (ult > 0) Reaper(P.X, G + 40, 1, .5f, t, false, Cl((1 - ult / 1.4f) * 1.3f, 0, 1), 0, false, VI, 3.8f);
            sb.End();
            if (ult > 0) { sb.Begin(); sb.Draw(px, new Rectangle(0, 0, W, H), new Color(10, 0, 25) * (.55f * Math.Min(1, ult))); sb.End(); }
            GraphicsDevice.SetRenderTarget(small); sb.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp); sb.Draw(rt, new Rectangle(0, 0, 320, 180), WH); sb.End();
            GraphicsDevice.SetRenderTarget(null); GraphicsDevice.Clear(Color.Black);
            var pp = GraphicsDevice.PresentationParameters; float scl = Math.Min(pp.BackBufferWidth / (float)W, pp.BackBufferHeight / (float)H);
            var m = Matrix.CreateScale(scl) * Matrix.CreateTranslation((pp.BackBufferWidth - W * scl) / 2, (pp.BackBufferHeight - H * scl) / 2, 0);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, m);
            sb.Draw(rt, new Rectangle(0, 0, W, H), WH); sb.Draw(small, new Rectangle(0, 0, W, H), new Color(90, 90, 90, 0));
            bool low = state == "play" && P.Hp < 35; sb.Draw(vig, new Rectangle(0, 0, W, H), WH);
            if (low) Box(W / 2, H / 2, W, H, 0, new Color(120, 0, 20) * (.12f + .1f * MathF.Sin(t * 8)));
            if (flash > 0) Box(W / 2, H / 2, W, H, 0, WH * (flash * .6f));
            if (state == "play") DrawHud();
            if (state == "title")
            {
                Box(W / 2, H / 2, W, H, 0, Color.Black * .45f); Txt("KADAYA", W / 2, 90, 190, new Color(220, 190, 255), 1, PU);
                Txt("THE GRIM REAPER", W / 2, 300, 44, new Color(255, 106, 128), 1, RD); if ((int)(t * 1.6f) % 2 == 0) Txt("PRESS ENTER", W / 2, 380, 40, WH, 1);
                Txt("A/D MOVE   W/SPACE JUMP x2   SHIFT SHADOW STEP   J SLASH COMBO   K KUNAI", W / 2, H - 120, 24, new Color(207, 192, 255), 1, PU);
                Txt("HOLD L SOUL RASENGAN   E SHADOW CLONES   Q DEATH'S ECLIPSE   F11 FULLSCREEN", W / 2, H - 80, 24, new Color(207, 192, 255), 1, PU);
            }
            if (state == "dead")
            {
                Box(W / 2, H / 2, W, H, 0, new Color(20, 0, 10) * .6f); Txt("YOU HAVE BEEN REAPED", W / 2, 220, 90, RD, 1); Txt("WAVE " + wave + "    SOULS " + kills, W / 2, 340, 40, WH, 1);
                if ((int)(t * 1.6f) % 2 == 0) Txt("PRESS ENTER TO RISE AGAIN", W / 2, 420, 34, VI, 1);
            }
            sb.End(); base.Draw(gt);
        }
    }
}
