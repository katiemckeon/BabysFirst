using System.Collections.Generic;
using System.IO;
using BabysFirst.Menu;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BabysFirst.EditorTools
{
    /// <summary>
    /// The one way to make the published web build, used by hand today and by CI later:
    ///   Editor menu:  Baby's First > Build Web
    ///   Command line: unity build . --target WebGL --execute-method BabysFirst.EditorTools.WebBuild.Build
    /// Output goes to Builds/Web (git-ignored), ready to copy to a GitHub Pages branch.
    /// </summary>
    public static class WebBuild
    {
        public const string OutputPath = "Builds/Web";

        const string ShellScenes = "Assets/_Project/Scenes/";
        const string Episodes = "Assets/_Project/Episodes/";

        /// <summary>
        /// The scenes that ship, in order: the intro (what the page opens on), the menu, then every
        /// episode that GameCatalog lists with a scene, at Episodes/&lt;Name&gt;/&lt;Name&gt;.unity.
        /// Adding an episode to the catalog is all it takes to ship it.
        /// </summary>
        public static string[] Scenes()
        {
            var scenes = new List<string> { ShellScenes + "Intro.unity", ShellScenes + GameCatalog.MenuScene + ".unity" };
            foreach (var category in GameCatalog.Categories)
                foreach (var game in category.games)
                    if (!string.IsNullOrEmpty(game.scene))
                        scenes.Add(Episodes + game.scene + "/" + game.scene + ".unity");
            return scenes.ToArray();
        }

        [MenuItem("Baby's First/Build Web")]
        public static void Build()
        {
            var scenes = Scenes();
            foreach (var scene in scenes)
                if (!File.Exists(scene)) throw new BuildFailedException("Missing scene: " + scene);
            if (!Directory.Exists("Assets/ThirdParty") || Directory.GetDirectories("Assets/ThirdParty").Length == 0)
                throw new BuildFailedException("Assets/ThirdParty is empty. The asset packs live in the private assets repo; run 'git submodule update --init'.");

            // GitHub Pages cannot send the Content-Encoding header a compressed Unity build expects,
            // so ship Brotli with Unity's built-in decompression fallback.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            if (s.result != BuildResult.Succeeded)
                throw new BuildFailedException("Web build " + s.result + " with " + s.totalErrors + " error(s).");

            // Tell GitHub Pages to serve the folder as-is (no Jekyll processing).
            File.WriteAllText(Path.Combine(OutputPath, ".nojekyll"), "");
            Debug.Log("Web build OK: " + OutputPath + " (" + (s.totalSize / (1024 * 1024)) + " MB, " + s.totalTime.TotalMinutes.ToString("F1") + " min)");
        }
    }
}
