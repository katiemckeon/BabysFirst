namespace BabysFirst.Menu
{
    /// <summary>
    /// Every Baby's First game, grouped by category. This is the one list the main menu is built
    /// from: add a game here (with its scene name once it is playable) and re-run the menu builder.
    /// </summary>
    public static class GameCatalog
    {
        public const string MenuScene = "Menu";

        public class Game
        {
            public string title;
            public string scene; // null until the game exists; the menu then shows it as "soon"
            public Game(string title, string scene = null) { this.title = title; this.scene = scene; }
        }

        public class Category
        {
            public string name;
            public Game[] games;
            public Category(string name, params Game[] games) { this.name = name; this.games = games; }
        }

        public static readonly Category[] Categories =
        {
            new Category("Finance",
                new Game("Rehypothecation", "Rehypothecation"),
                new Game("Dark Pool")),
            new Category("Tech",
                new Game("Exploit Kit")),
        };
    }
}
