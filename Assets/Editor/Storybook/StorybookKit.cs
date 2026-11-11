using BabysFirst.Storybook;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace BabysFirst.EditorTools
{
    /// <summary>The words a game puts on the shared Storybook screens.</summary>
    public class StorybookSpec
    {
        public string series = "Baby's First";
        public string title;
        public string taglinePlain, taglineItalic;
        public Sprite plate;
        public string plateCaption;

        public string chapterNumeral = "1", chapterNumeralCaption;
        public string chapterLabel, chapterTitle, chapterSummary;

        public string endLabel = "The End", endHeading, endBody, endBodyItalic, replayLabel = "Play again";
    }

    /// <summary>
    /// Builds the Storybook UI described in the Storybook UI Guide in design/ onto a
    /// 1280 x 720 canvas: title page, chapter card, narration strip, prompt tag, counter, pause
    /// menu and end page. Sizes below are the guide's design pixels.
    /// </summary>
    public static class StorybookKit
    {
        const string FontFolder = "Assets/Fonts";
        public static TMP_FontAsset Black, ExtraBold, Bold, Lora, LoraItalic, LoraBold, Hand;
        static Material numeralOutline;

        // ---------------------------------------------------------------- fonts

        public static string EnsureFonts()
        {
            Black = Font("Fraunces-Black");
            ExtraBold = Font("Fraunces-ExtraBold");
            Bold = Font("Fraunces-Bold");
            Lora = Font("Lora-Regular");
            LoraItalic = Font("Lora-Italic");
            LoraBold = Font("Lora-Bold");
            Hand = Font("PatrickHand-Regular");

            // The chapter numeral is marigold with an ink outline: a material variant of Fraunces Black.
            const string path = FontFolder + "/Fraunces-Black SDF Outline.mat";
            numeralOutline = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (numeralOutline == null)
            {
                numeralOutline = new Material(Black.material);
                AssetDatabase.CreateAsset(numeralOutline, path);
            }
            numeralOutline.SetTexture(ShaderUtilities.ID_MainTex, Black.atlasTexture);
            numeralOutline.EnableKeyword("OUTLINE_ON");
            numeralOutline.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.09f);
            numeralOutline.SetColor(ShaderUtilities.ID_OutlineColor, Palette.Ink);
            EditorUtility.SetDirty(numeralOutline);
            AssetDatabase.SaveAssets();
            return "Storybook fonts ready";
        }

        static TMP_FontAsset Font(string name)
        {
            string sdf = FontFolder + "/" + name + " SDF.asset";
            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdf);
            if (fa != null) return fa;
            var ttf = AssetDatabase.LoadAssetAtPath<Font>(FontFolder + "/" + name + ".ttf");
            if (ttf == null) throw new System.Exception("Missing font file: " + FontFolder + "/" + name + ".ttf");
            fa = TMP_FontAsset.CreateFontAsset(ttf, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            AssetDatabase.CreateAsset(fa, sdf);
            fa.material.name = name + " SDF Material";
            fa.atlasTexture.name = name + " SDF Atlas";
            AssetDatabase.AddObjectToAsset(fa.material, fa);
            AssetDatabase.AddObjectToAsset(fa.atlasTexture, fa);
            EditorUtility.SetDirty(fa);
            return fa;
        }

        // ---------------------------------------------------------------- the whole HUD

        public static StorybookHud Build(StorybookSpec spec)
        {
            EnsureFonts();
            StorybookSprites.EnsureAll();

            if (Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvasGo = new GameObject("Storybook", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); // the guide's canvas, so its pixel sizes apply as written
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // the whole 1280 x 720 sheet always fits
            var hud = canvasGo.AddComponent<StorybookHud>();
            var root = canvasGo.transform;

            BuildPlayLayer(hud, root, spec);
            BuildNarration(hud, root);
            BuildEndPage(hud, root, spec);
            BuildPauseMenu(hud, root);
            BuildChapterCard(hud, root, spec);
            BuildTitlePage(hud, root, spec); // last sibling draws on top
            return hud;
        }

        // ---------------------------------------------------------------- 1. title page

        static void BuildTitlePage(StorybookHud hud, Transform root, StorybookSpec s)
        {
            var cloth = Fill("TitlePage", root);
            Paint(cloth, null, Palette.Cloth, true);
            hud.titlePage = cloth.gameObject;
            var page = Sheet(cloth);
            BorderedPage(page, 36, 36, 1208, 648, Palette.ClothShadow);

            Text("Series", page, 0, 77, 1280, 46, LoraItalic, 38, Palette.Ink, TextAlignmentOptions.Center, s.series);
            var title = Text("Title", page, 0, 121, 1280, 112, Black, 104, Palette.Ink, TextAlignmentOptions.Center, s.title);
            title.characterSpacing = -1f;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            Ornament(page, 640, 250, 150);

            // Plate I: an eight-sided picture of the scene, with a flat tan shadow.
            Paint(Box("PlateShadow", page, 178, 327, 420, 236), StorybookSprites.Cut34, Palette.TanEdge);
            Paint(Box("PlateEdge", page, 178, 321, 420, 236), StorybookSprites.Cut34, Palette.Ink);
            var mask = Box("PlateMask", page, 182, 325, 412, 228);
            Paint(mask, StorybookSprites.Cut34, Palette.Paper);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var picture = Fill("Picture", mask).gameObject.AddComponent<Image>();
            picture.sprite = s.plate;
            picture.raycastTarget = false;
            Text("PlateCaption", page, 178, 573, 420, 22, LoraItalic, 17, Palette.FadedInk, TextAlignmentOptions.Center, s.plateCaption);

            // Right column: the pitch, Play, and how to play.
            Text("Tagline", page, 662, 318, 460, 38, Lora, 27, Palette.Ink, TextAlignmentOptions.Left, s.taglinePlain);
            Text("TaglineItalic", page, 662, 356, 460, 38, LoraItalic, 27, Palette.Ink, TextAlignmentOptions.Left, s.taglineItalic);
            hud.playButton = CutButton("Play", page, 662, 420, 212, 72, "Play");

            // Walk: four outlined keys, W filled marigold.
            var keys = Box("WalkKeys", page, 662, 518, 68, 44);
            Key(keys, "W", 23, 0, true); Key(keys, "A", 1, 22, false); Key(keys, "S", 23, 22, false); Key(keys, "D", 45, 22, false);
            Text("WalkLabel", page, 632, 570, 128, 22, LoraItalic, 17, Palette.Ink, TextAlignmentOptions.Center, "Walk");
            // Look: mouse with an arrow each side.
            Paint(Box("LookIcon", page, 770, 528, 71, 34), StorybookSprites.MouseLook, Tint);
            Text("LookLabel", page, 741, 570, 128, 22, LoraItalic, 17, Palette.Ink, TextAlignmentOptions.Center, "Look");
            // Click: mouse with the left button filled marigold.
            Paint(Box("ClickIcon", page, 939, 528, 24, 34), StorybookSprites.MouseClick, Tint);
            var click = Row("ClickLabel", page, 871, 570, 160, 22, 5, TextAnchor.MiddleCenter);
            InlineText("Verb", click, LoraItalic, 17, Palette.Ink, "Click");
            InlineText("Rest", click, Lora, 17, Palette.Ink, "to do things");
        }

        static void Key(Transform parent, string letter, float x, float y, bool filled)
        {
            var key = Box("Key" + letter, parent, x, y, 22, 22);
            Paint(key, filled ? StorybookSprites.KeyFilled : StorybookSprites.KeyOutline, Tint);
            var t = Text("Letter", key, 0, 0, 22, 22, LoraBold, 11, Palette.Ink, TextAlignmentOptions.Center, letter);
            t.rectTransform.anchoredPosition = new Vector2(0, 0.5f);
        }

        // ---------------------------------------------------------------- 2. chapter card

        static void BuildChapterCard(StorybookHud hud, Transform root, StorybookSpec s)
        {
            var cloth = Fill("ChapterCard", root);
            Paint(cloth, null, Palette.Cloth, true);
            hud.chapterCard = cloth.gameObject;
            var page = Sheet(cloth);
            BorderedPage(page, 36, 36, 1208, 648, Palette.ClothShadow);
            Paint(Box("Gutter", page, 640, 84, 1, 552), null, Palette.Ink); // the fold of the open book

            var numeral = Text("Numeral", page, 138, 110, 400, 380, Black, 330, Palette.Marigold, TextAlignmentOptions.Center, s.chapterNumeral);
            numeral.fontSharedMaterial = numeralOutline;
            Text("NumeralCaption", page, 88, 498, 500, 40, LoraItalic, 30, Palette.Ink, TextAlignmentOptions.Center, s.chapterNumeralCaption);

            RunningHead("Label", page, 672, 214, 540, 20, 15, Palette.HenRed, TextAlignmentOptions.Center, s.chapterLabel);
            Text("Heading", page, 672, 240, 540, 78, ExtraBold, 62, Palette.Ink, TextAlignmentOptions.Center, s.chapterTitle);
            Ornament(page, 942, 340, 110);
            var summary = Text("Summary", page, 732, 366, 420, 120, Lora, 25, Palette.Ink, TextAlignmentOptions.Top, s.chapterSummary);
            summary.lineSpacing = 16f;
            Text("Hint", page, 672, 572, 540, 24, LoraItalic, 17, Palette.FadedInk, TextAlignmentOptions.Center, "Press Space to turn the page");
        }

        // ---------------------------------------------------------------- 3. pause menu

        static void BuildPauseMenu(StorybookHud hud, Transform root)
        {
            var dim = Fill("PauseMenu", root);
            Paint(dim, null, Alpha(Palette.Ink, 0.6f), true);
            hud.pauseMenu = dim.gameObject;
            var layer = Sheet(dim);
            var panel = PlainPage(Box("Contents", layer, 370, 140, 540, 420));
            RunningHead("Label", panel, 0, 34, 540, 20, 14, Palette.HenRed, TextAlignmentOptions.Center, "The story is resting");
            Text("Heading", panel, 0, 56, 540, 72, ExtraBold, 58, Palette.Ink, TextAlignmentOptions.Center, "Contents");
            Ornament(panel, 270, 146, 90);
            hud.carryOnButton = ContentsRow(panel, 176, "Carry on", "Esc");
            hud.restartButton = ContentsRow(panel, 240, "Begin the chapter again", "p. 1");
            hud.closeBookButton = ContentsRow(panel, 304, "Close the book", "The End");
        }

        // One line of the table of contents: title, dotted leader, right-hand note. Marigold when chosen.
        static Button ContentsRow(Transform panel, float y, string label, string rightNote)
        {
            var row = Row("Row " + label, panel, 30, y, 480, 52, 12, TextAnchor.MiddleLeft);
            var group = row.GetComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(16, 16, 0, 0);
            var bg = Paint(row, StorybookSprites.Round6, Tint, true);
            var button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            var colors = button.colors;
            colors.normalColor = Palette.Paper;
            colors.highlightedColor = Palette.Marigold;
            colors.selectedColor = Palette.Marigold;
            colors.pressedColor = Palette.GoldEdge;
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            InlineText("Label", row, Bold, 24, Palette.Ink, label);
            var leader = new GameObject("Leader", typeof(RectTransform)).GetComponent<RectTransform>();
            leader.SetParent(row, false);
            leader.sizeDelta = new Vector2(0, 4);
            var dots = Paint(leader, StorybookSprites.LeaderDot, Palette.FadedInk);
            dots.type = Image.Type.Tiled;
            var le = leader.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1; le.preferredHeight = 4; le.minWidth = 20;
            InlineText("Note", row, LoraItalic, 18, Palette.FadedInk, rightNote);
            return button;
        }

        // ---------------------------------------------------------------- 4. narration strip

        static void BuildNarration(StorybookHud hud, Transform root)
        {
            var strip = new GameObject("Narration", typeof(RectTransform)).GetComponent<RectTransform>();
            strip.SetParent(root, false);
            strip.anchorMin = new Vector2(0, 0); strip.anchorMax = new Vector2(1, 0); strip.pivot = new Vector2(0.5f, 0);
            strip.offsetMin = new Vector2(110, 30); strip.offsetMax = new Vector2(-110, 190); // inset 110 px each side
            PlainPage(strip);
            hud.narration = strip;

            hud.dropCap = Text("DropCap", strip, hud.narrationPadX, hud.narrationPadTop - 8, 120, 92, Black, 84, Palette.HenRed, TextAlignmentOptions.TopLeft, "T");
            hud.dropCap.textWrappingMode = TextWrappingModes.NoWrap;

            var body = new GameObject("Body", typeof(RectTransform)).GetComponent<RectTransform>();
            body.SetParent(strip, false);
            body.anchorMin = new Vector2(0, 1); body.anchorMax = new Vector2(1, 1); body.pivot = new Vector2(0, 1);
            body.offsetMin = new Vector2(100, -100); body.offsetMax = new Vector2(-hud.narrationPadX, -hud.narrationPadTop);
            hud.body = Type(body, Lora, 30, Palette.Ink, TextAlignmentOptions.TopLeft, "");
            hud.body.lineSpacing = 18.5f; // 30 px type on a 44 px line

            var pageNo = new GameObject("PageNumber", typeof(RectTransform)).GetComponent<RectTransform>();
            pageNo.SetParent(strip, false);
            pageNo.anchorMin = pageNo.anchorMax = pageNo.pivot = new Vector2(0, 0);
            pageNo.anchoredPosition = new Vector2(hud.narrationPadX, 10); pageNo.sizeDelta = new Vector2(200, 20);
            hud.pageNumber = Type(pageNo, LoraItalic, 15, Palette.FadedInk, TextAlignmentOptions.Left, "page 1");

            var turn = new GameObject("TurnThePage", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            turn.SetParent(strip, false);
            turn.anchorMin = turn.anchorMax = turn.pivot = new Vector2(1, 0);
            turn.anchoredPosition = new Vector2(-hud.narrationPadX, 10); turn.sizeDelta = new Vector2(300, 20);
            var g = turn.GetComponent<HorizontalLayoutGroup>();
            g.childAlignment = TextAnchor.MiddleRight; g.spacing = 7;
            g.childControlWidth = g.childControlHeight = true; g.childForceExpandWidth = g.childForceExpandHeight = false;
            var space = InlineText("Key", turn, LoraBold, 13, Palette.FadedInk, "SPACE");
            space.characterSpacing = 20f;
            InlineText("Hint", turn, LoraItalic, 15, Palette.FadedInk, "turn the page");
            var arrow = new GameObject("Arrow", typeof(RectTransform)).GetComponent<RectTransform>();
            arrow.SetParent(turn, false);
            Paint(arrow, StorybookSprites.Arrow, Palette.HenRed);
            var ale = arrow.gameObject.AddComponent<LayoutElement>();
            ale.preferredWidth = 7; ale.preferredHeight = 9;
        }

        // ---------------------------------------------------------------- 5. prompt, reticle, counter, chapter tag

        static void BuildPlayLayer(StorybookHud hud, Transform root, StorybookSpec s)
        {
            var layer = Fill("PlayLayer", root);
            hud.playLayer = layer.gameObject;

            // Reticle: a 16 px paper dot with an ink ring.
            var ring = Centered("Reticle", layer, 0, 0, 16, 16);
            Paint(ring, StorybookSprites.Circle, Palette.Ink);
            var dot = Centered("Dot", ring, 0, 0, 11, 11);
            hud.reticleDot = Paint(dot, StorybookSprites.Circle, Palette.Paper);

            // Prompt tag: [mouse icon] Click Andreas. Sizes itself to its words.
            var tag = new GameObject("PromptTag", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            tag.SetParent(layer, false);
            tag.anchorMin = tag.anchorMax = new Vector2(0.5f, 0.5f); tag.pivot = new Vector2(0.5f, 1);
            tag.anchoredPosition = new Vector2(0, -34);
            var g = tag.GetComponent<HorizontalLayoutGroup>();
            g.padding = new RectOffset(16, 18, 7, 9); g.spacing = 8; g.childAlignment = TextAnchor.MiddleCenter;
            g.childControlWidth = g.childControlHeight = true; g.childForceExpandWidth = g.childForceExpandHeight = false;
            var fit = tag.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            PlainPage(tag);
            hud.promptTag = tag.gameObject;
            var icon = new GameObject("MouseIcon", typeof(RectTransform)).GetComponent<RectTransform>();
            icon.SetParent(tag, false);
            Paint(icon, StorybookSprites.MouseClick, Tint);
            var ile = icon.gameObject.AddComponent<LayoutElement>();
            ile.preferredWidth = 17; ile.preferredHeight = 24;
            hud.promptIcon = icon.gameObject;
            hud.promptVerb = InlineText("Verb", tag, LoraItalic, 24, Palette.Ink, "Click");
            hud.promptTarget = InlineText("Target", tag, ExtraBold, 24, Palette.Ink, "Andreas");
            hud.promptTail = InlineText("Tail", tag, Lora, 24, Palette.Ink, "");

            // Counter, top right: 1 donkey (ink) beside N promises (hen red), pencilled note beneath.
            var counter = new GameObject("Counter", typeof(RectTransform)).GetComponent<RectTransform>();
            counter.SetParent(layer, false);
            counter.anchorMin = counter.anchorMax = counter.pivot = new Vector2(1, 1);
            counter.anchoredPosition = new Vector2(-32, -28); counter.sizeDelta = new Vector2(264, 152);
            PlainPage(counter);
            hud.counter = counter.gameObject;
            hud.donkeyNumber = Text("DonkeyNumber", counter, 16, 4, 110, 70, Black, 60, Palette.Ink, TextAlignmentOptions.Center, "1");
            hud.donkeyLabel = RunningHead("DonkeyLabel", counter, 16, 74, 110, 18, 13, Palette.Ink, TextAlignmentOptions.Center, "donkey");
            hud.promiseNumber = Text("PromiseNumber", counter, 138, 4, 110, 70, Black, 60, Palette.HenRed, TextAlignmentOptions.Center, "3");
            hud.promiseLabel = RunningHead("PromiseLabel", counter, 138, 74, 110, 18, 13, Palette.HenRed, TextAlignmentOptions.Center, "promises");
            hud.note = Text("PencilNote", counter, 12, 104, 240, 30, Hand, 23, Palette.BluePencil, TextAlignmentOptions.Center, "that is too many!");
            hud.note.rectTransform.localRotation = Quaternion.Euler(0, 0, 3f);

            // Chapter tag, top left.
            var chapter = PlainPage(Box("ChapterTag", layer, 32, 28, 10, 34));
            var cg = chapter.gameObject.AddComponent<HorizontalLayoutGroup>();
            cg.padding = new RectOffset(16, 16, 0, 0); cg.childAlignment = TextAnchor.MiddleCenter;
            cg.childControlWidth = cg.childControlHeight = true; cg.childForceExpandWidth = cg.childForceExpandHeight = false;
            chapter.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var tagText = InlineText("Text", chapter, LoraBold, 13, Palette.Ink, (s.chapterLabel + "  ·  " + s.chapterTitle).ToUpperInvariant());
            tagText.characterSpacing = 20f;
        }

        // ---------------------------------------------------------------- end page

        static void BuildEndPage(StorybookHud hud, Transform root, StorybookSpec s)
        {
            var dim = Fill("EndPage", root);
            Paint(dim, null, Alpha(Palette.Ink, 0.6f), true);
            hud.endPage = dim.gameObject;
            var layer = Sheet(dim);
            BorderedPage(layer, 210, 106, 860, 508, Alpha(Palette.Ink, 0.55f));
            RunningHead("Label", layer, 210, 152, 860, 20, 15, Palette.HenRed, TextAlignmentOptions.Center, s.endLabel);
            Text("Heading", layer, 210, 178, 860, 76, ExtraBold, 58, Palette.Ink, TextAlignmentOptions.Center, s.endHeading);
            Ornament(layer, 640, 274, 130);
            var body = Text("Body", layer, 290, 304, 700, 84, Lora, 26, Palette.Ink, TextAlignmentOptions.Top, s.endBody);
            body.lineSpacing = 18f;
            Text("BodyItalic", layer, 290, 400, 700, 40, LoraItalic, 26, Palette.Ink, TextAlignmentOptions.Center, s.endBodyItalic);
            hud.replayButton = FlatButton("Replay", layer, 530, 482, 220, 58, s.replayLabel);
        }

        // ---------------------------------------------------------------- panels and buttons

        /// <summary>Plain page: paper, 6 px corners, flat shadow 8 px straight down. Add content after calling.</summary>
        public static RectTransform PlainPage(RectTransform container)
        {
            var shadow = Fill("Shadow", container, 0, 8, 0, -8);
            Paint(shadow, StorybookSprites.Round6, Alpha(Palette.Ink, 0.4f));
            shadow.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var paper = Fill("Paper", container);
            Paint(paper, StorybookSprites.Round6, Palette.Paper, true);
            paper.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            return container;
        }

        /// <summary>Bordered page: 4 px ink edge, 1 px rule 10 px inside, corners cut at 45 degrees (18 px), flat shadow.</summary>
        public static RectTransform BorderedPage(Transform parent, float x, float y, float w, float h, Color shadow)
        {
            var page = Box("Page", parent, x, y, w, h);
            Paint(Fill("Shadow", page, 0, 8, 0, -8), StorybookSprites.Cut18, shadow);
            Paint(Fill("Edge", page), StorybookSprites.Cut18, Palette.Ink, true);
            Paint(Fill("Paper", page, 4, 4, 4, 4), StorybookSprites.Cut18, Palette.Paper);
            var rule = Fill("Rule", page, 14, 14, 14, 14);
            Line(rule, new Vector2(0, 1), new Vector2(1, 1), true);
            Line(rule, new Vector2(0, 0), new Vector2(1, 0), true);
            Line(rule, new Vector2(0, 0), new Vector2(0, 1), false);
            Line(rule, new Vector2(1, 0), new Vector2(1, 1), false);
            return page;
        }

        static void Line(Transform parent, Vector2 aMin, Vector2 aMax, bool horizontal)
        {
            var rt = new GameObject("Line", typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.sizeDelta = horizontal ? new Vector2(0, 1) : new Vector2(1, 0);
            Paint(rt, null, Palette.Ink);
        }

        /// <summary>The title page's Play button: marigold, cut corners, 3 px ink outline, 6 px ink shadow.</summary>
        public static Button CutButton(string name, Transform parent, float x, float y, float w, float h, string label)
        {
            var rt = Box(name, parent, x, y, w, h);
            Paint(Fill("Shadow", rt, 0, 6, 0, -6), StorybookSprites.Cut9, Palette.Ink);
            var edge = Paint(Fill("Edge", rt), StorybookSprites.Cut9, Palette.Ink, true);
            Paint(Fill("Face", rt, 3, 3, 3, 3), StorybookSprites.Cut9, Palette.Marigold);
            var text = Type(Fill("Label", rt, 0, 0, 0, 2), ExtraBold, 34, Palette.Ink, TextAlignmentOptions.Center, label);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = edge;
            b.transition = Selectable.Transition.None;
            return b;
        }

        /// <summary>A button over the game: flat marigold, 6 px corners, 4 px darker gold edge underneath.</summary>
        public static Button FlatButton(string name, Transform parent, float x, float y, float w, float h, string label)
        {
            var rt = Box(name, parent, x, y, w, h);
            Paint(Fill("Edge", rt, 0, 4, 0, -4), StorybookSprites.Round6, Palette.GoldEdge);
            var face = Paint(Fill("Face", rt), StorybookSprites.Round6, Palette.Marigold, true);
            Type(Fill("Label", rt, 0, 0, 0, 2), Bold, 28, Palette.Ink, TextAlignmentOptions.Center, label);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = face;
            b.transition = Selectable.Transition.None;
            return b;
        }

        /// <summary>A ruled diamond ornament centred on (cx, cy).</summary>
        public static void Ornament(Transform parent, float cx, float cy, float ruleWidth)
        {
            Paint(Box("RuleL", parent, cx - 9 - 14 - ruleWidth, cy, ruleWidth, 1), null, Palette.Ink);
            Paint(Box("Diamond", parent, cx - 9, cy - 9, 18, 18), StorybookSprites.Diamond, Tint);
            Paint(Box("RuleR", parent, cx + 9 + 14, cy, ruleWidth, 1), null, Palette.Ink);
        }

        // ---------------------------------------------------------------- primitives

        // Sprites that carry their own colors are drawn untinted.
        static readonly Color Tint = new Color(1f, 1f, 1f, 1f);

        static Color Alpha(Color c, float a) { c.a = a; return c; }

        /// <summary>A rect placed in design pixels from its parent's top-left corner.</summary>
        public static RectTransform Box(string name, Transform parent, float x, float y, float w, float h)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        // Full-screen pages lay their content on a 1280 x 720 sheet kept centred at any aspect ratio.
        static RectTransform Sheet(Transform parent)
        {
            return Centered("Sheet", parent, 0, 0, 1280, 720);
        }

        static RectTransform Centered(string name, Transform parent, float x, float y, float w, float h)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        /// <summary>A rect stretched over its parent, inset by left, top, right, bottom design pixels.</summary>
        public static RectTransform Fill(string name, Transform parent, float l = 0, float t = 0, float r = 0, float b = 0)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        static Image Paint(RectTransform rt, Sprite sprite, Color color, bool raycast = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        static TextMeshProUGUI Type(RectTransform rt, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, string text)
        {
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.text = text ?? "";
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            return t;
        }

        static TextMeshProUGUI Text(string name, Transform parent, float x, float y, float w, float h, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, string text)
        {
            return Type(Box(name, parent, x, y, w, h), font, size, color, align, text);
        }

        // Labels and running heads: Lora Bold capitals, tracked 0.2 em.
        static TextMeshProUGUI RunningHead(string name, Transform parent, float x, float y, float w, float h, float size, Color color, TextAlignmentOptions align, string text)
        {
            var t = Text(name, parent, x, y, w, h, LoraBold, size, color, align, (text ?? "").ToUpperInvariant());
            t.characterSpacing = 20f;
            return t;
        }

        // A horizontal run of differently-set words, e.g. italic verb + bold target.
        static RectTransform Row(string name, Transform parent, float x, float y, float w, float h, float spacing, TextAnchor align)
        {
            var rt = Box(name, parent, x, y, w, h);
            var g = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            g.spacing = spacing; g.childAlignment = align;
            g.childControlWidth = g.childControlHeight = true;
            g.childForceExpandWidth = g.childForceExpandHeight = false;
            return rt;
        }

        static TextMeshProUGUI InlineText(string name, Transform parent, TMP_FontAsset font, float size, Color color, string text)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var t = Type(rt, font, size, color, TextAlignmentOptions.Left, text);
            t.textWrappingMode = TextWrappingModes.NoWrap;
            return t;
        }
    }
}
