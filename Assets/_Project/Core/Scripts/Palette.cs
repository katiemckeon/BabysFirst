using UnityEngine;

namespace BabysFirst
{
    /// <summary>
    /// The art bible, as code. Every color in the project reads from here: materials, lights,
    /// fog, UI, text. Never use Color.gray / Color.white literals elsewhere.
    ///
    /// Style: Chunky low-poly toddler toy-box. Oversized rounded shapes, flat candy colors,
    ///        soft warm nursery light, and one bleak grown-up accent for the "ledger" parts.
    /// Mood:  Afternoon playroom light through a big window; dusty lavender shadow bounce;
    ///        the ticker-tape accent reads cool and corporate.
    /// </summary>
    public static class Palette
    {
        public static readonly Color Floor              = Hex("#F2D9B8"); // play-mat beige
        public static readonly Color Walls              = Hex("#BFE3E0"); // nursery mint
        public static readonly Color CharacterPrimary   = Hex("#F5A3B4"); // piglet pink
        public static readonly Color CharacterSecondary = Hex("#FFF4E0"); // cream wool
        public static readonly Color Props              = Hex("#F7C948"); // wooden-block yellow
        public static readonly Color Shadow             = Hex("#7B6F8E"); // dusty lavender
        public static readonly Color Accent             = Hex("#2F3A4A"); // ledger slate (the jaded grown-up tone)

        // Light story
        public static readonly Color KeyLight    = Hex("#FFE8C8"); // warm afternoon sun
        public static readonly Color AmbientSky  = Hex("#CFE9F2"); // cool sky bounce
        public static readonly Color AmbientMid  = Hex("#D9CFC4"); // neutral mid
        public static readonly Color AmbientGround = Hex("#8C7A66"); // dark warm floor bounce
        public static readonly Color SkyTint     = Hex("#A9D8EE"); // skybox tint
        public static readonly Color SkyZenith   = Hex("#4FA3E0"); // picture-book blue overhead, for outdoor scenes
        public static readonly Color WindowGlow  = Hex("#FFF6DD"); // the window as light-story element

        // Storybook UI (design/Baby's First — Storybook UI Guide.md). All text sits on paper.
        public static readonly Color Paper       = Hex("#F7EFDC"); // every panel, tag and page
        public static readonly Color Ink         = Hex("#263445"); // main text, page borders, icon lines
        public static readonly Color Marigold    = Hex("#EDBE3F"); // buttons, selection, icon highlights
        public static readonly Color HenRed      = Hex("#A63D24"); // drop caps, chapter labels, ornaments
        public static readonly Color HenRedLight = Hex("#C8573A"); // lit facet of the diamond ornament
        public static readonly Color Cloth       = Hex("#BDE0DE"); // behind the title page and chapter card
        public static readonly Color ClothShadow = Hex("#8FBFBD"); // flat shadow a page casts on cloth
        public static readonly Color FadedInk    = Hex("#5A5648"); // captions, page numbers, hints
        public static readonly Color BluePencil  = Hex("#35527F"); // Patrick Hand notes only
        public static readonly Color GoldEdge    = Hex("#B88E1F"); // edge under a marigold button
        public static readonly Color PaperLight  = Hex("#FFF9EA"); // unselected choice
        public static readonly Color TanEdge     = Hex("#D9CBA8"); // edge under light paper, plate shadow

        static Color Hex(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out var c) ? c : Color.magenta;
        }
    }
}
