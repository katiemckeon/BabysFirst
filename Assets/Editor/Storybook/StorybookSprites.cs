using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BabysFirst.EditorTools
{
    /// <summary>
    /// Draws the Storybook UI's flat shapes and icons into small PNG sprites, so the guide's cut
    /// corners, 6 px rounds and 2 px ink-outline icons exist as real 9-slice sprites in uGUI.
    /// Shapes are white (tinted per use from Palette); icons bake their Ink and Marigold.
    /// One canvas unit (a design pixel at 1280 x 720) is <see cref="Scale"/> texture pixels.
    /// </summary>
    public static class StorybookSprites
    {
        public const string Folder = "Assets/UI/Storybook";
        public const int Scale = 4;
        const int Samples = 4;

        public static Sprite Round6, Cut9, Cut18, Cut34, Circle, Diamond, Arrow, LeaderDot;
        public static Sprite KeyOutline, KeyFilled, MouseClick, MouseLook;

        public static void EnsureAll()
        {
            if (!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets", "UI");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/UI", "Storybook");
            Color white = new Color(1f, 1f, 1f, 1f); // tint base; each use is colored from Palette
            Color ink = Palette.Ink, gold = Palette.Marigold;

            Round6 = Make("Round6", 16, 16, 7, p => p.Fill(v => RoundRect(v, 0, 0, 16, 16, 6), white));
            Cut9 = Make("Cut9", 22, 22, 10, p => p.Fill(v => Octagon(v, 22, 22, 9), white));
            Cut18 = Make("Cut18", 40, 40, 19, p => p.Fill(v => Octagon(v, 40, 40, 18), white));
            Cut34 = Make("Cut34", 72, 72, 35, p => p.Fill(v => Octagon(v, 72, 72, 34), white));
            Circle = Make("Circle", 16, 16, 0, p => p.Fill(v => (v - new Vector2(8, 8)).magnitude <= 8f, white));
            Arrow = Make("Arrow", 8, 10, 0, p => p.Fill(v => Poly(v, new Vector2(0, 0), new Vector2(8, 5), new Vector2(0, 10)), white));
            LeaderDot = Make("LeaderDot", 8, 4, 0, p => p.Fill(v => (v - new Vector2(4, 2)).magnitude <= 1.1f, white));
            Diamond = Make("Diamond", 18, 18, 0, p =>
            {
                p.Fill(v => Poly(v, new Vector2(9, 0), new Vector2(18, 9), new Vector2(9, 18), new Vector2(0, 9)), Palette.HenRed);
                p.Fill(v => Poly(v, new Vector2(9, 0), new Vector2(18, 9), new Vector2(9, 9)), Palette.HenRedLight);
            });

            KeyOutline = Make("KeyOutline", 20, 20, 0, p => p.Fill(v => RoundRectStroke(v, 1, 1, 18, 18, 4), ink));
            KeyFilled = Make("KeyFilled", 20, 20, 0, p =>
            {
                p.Fill(v => RoundRect(v, 1, 1, 18, 18, 4), gold);
                p.Fill(v => RoundRectStroke(v, 1, 1, 18, 18, 4), ink);
            });
            MouseClick = Make("MouseClick", 18, 26, 0, p =>
            {
                p.Fill(v => RoundRect(v, 1, 1, 16, 24, 8) && v.x < 9 && v.y < 11, gold);
                MouseBody(p, 1, ink);
            });
            MouseLook = Make("MouseLook", 54, 26, 0, p =>
            {
                MouseBody(p, 19, ink);
                Line(p, ink, 12, 13, 3, 13); Line(p, ink, 7, 9, 3, 13); Line(p, ink, 3, 13, 7, 17);
                Line(p, ink, 42, 13, 51, 13); Line(p, ink, 47, 9, 51, 13); Line(p, ink, 51, 13, 47, 17);
            });
            AssetDatabase.SaveAssets();
        }

        static void MouseBody(Painter p, float x, Color ink)
        {
            p.Fill(v => RoundRectStroke(v, x, 1, 16, 24, 8), ink);
            Line(p, ink, x + 8, 1, x + 8, 11);
            Line(p, ink, x, 11, x + 16, 11);
        }

        static void Line(Painter p, Color c, float x1, float y1, float x2, float y2)
        {
            Vector2 a = new Vector2(x1, y1), b = new Vector2(x2, y2);
            p.Fill(v => SegmentDistance(v, a, b) <= 1f, c);
        }

        // ---------------------------------------------------------------- shapes (design units, y down)

        static bool RoundRect(Vector2 v, float x, float y, float w, float h, float r)
        {
            if (r < 0f) r = 0f;
            float cx = Mathf.Clamp(v.x, x + r, x + w - r), cy = Mathf.Clamp(v.y, y + r, y + h - r);
            if (v.x < x || v.x > x + w || v.y < y || v.y > y + h) return false;
            return (v - new Vector2(cx, cy)).sqrMagnitude <= r * r + 0.0001f;
        }

        // A 2-unit stroke centred on the rectangle's edge, as SVG draws it.
        static bool RoundRectStroke(Vector2 v, float x, float y, float w, float h, float r)
        {
            return RoundRect(v, x - 1, y - 1, w + 2, h + 2, r + 1) && !RoundRect(v, x + 1, y + 1, w - 2, h - 2, r - 1);
        }

        static bool Octagon(Vector2 v, float w, float h, float cut)
        {
            if (v.x < 0 || v.y < 0 || v.x > w || v.y > h) return false;
            float dx = Mathf.Min(v.x, w - v.x), dy = Mathf.Min(v.y, h - v.y);
            return dx + dy >= cut;
        }

        static bool Poly(Vector2 v, params Vector2[] pts)
        {
            bool pos = false, neg = false;
            for (int i = 0; i < pts.Length; i++)
            {
                Vector2 a = pts[i], b = pts[(i + 1) % pts.Length];
                float cross = (b.x - a.x) * (v.y - a.y) - (b.y - a.y) * (v.x - a.x);
                if (cross > 0) pos = true; else if (cross < 0) neg = true;
            }
            return !(pos && neg);
        }

        static float SegmentDistance(Vector2 v, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(v - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.0001f));
            return (v - (a + ab * t)).magnitude;
        }

        // ---------------------------------------------------------------- raster + import

        class Painter
        {
            public readonly int W, H;
            public readonly Color[] Px;
            public Painter(int w, int h) { W = w; H = h; Px = new Color[w * h]; }

            // Supersampled coverage, alpha-blended over what is already there.
            public void Fill(Func<Vector2, bool> inside, Color c)
            {
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        int hit = 0;
                        for (int sy = 0; sy < Samples; sy++)
                            for (int sx = 0; sx < Samples; sx++)
                                if (inside(new Vector2((x + (sx + 0.5f) / Samples) / Scale, (y + (sy + 0.5f) / Samples) / Scale))) hit++;
                        if (hit == 0) continue;
                        float a = c.a * hit / (Samples * Samples);
                        int i = (H - 1 - y) * W + x; // textures are y-up
                        Color d = Px[i];
                        float outA = a + d.a * (1f - a);
                        Color rgb = outA > 0f ? (c * a + d * d.a * (1f - a)) / outA : c;
                        Px[i] = new Color(rgb.r, rgb.g, rgb.b, outA);
                    }
            }
        }

        static Sprite Make(string name, int unitsW, int unitsH, int borderUnits, Action<Painter> draw)
        {
            string path = Folder + "/" + name + ".png";
            var p = new Painter(unitsW * Scale, unitsH * Scale);
            draw(p);
            // Keep the color of transparent pixels equal to their neighbours' so edges do not fringe dark.
            for (int i = 0; i < p.Px.Length; i++)
                if (p.Px[i].a <= 0f) p.Px[i] = new Color(1f, 1f, 1f, 0f);
            var tex = new Texture2D(p.W, p.H, TextureFormat.RGBA32, false);
            tex.SetPixels(p.Px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100 * Scale;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.wrapMode = TextureWrapMode.Clamp;
            int b = borderUnits * Scale;
            imp.spriteBorder = new Vector4(b, b, b, b);
            var settings = new TextureImporterSettings();
            imp.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // required for sliced and tiled images
            imp.SetTextureSettings(settings);
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
