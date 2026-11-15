using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BabysFirst.EditorTools
{
    /// <summary>
    /// Opens every scene of the project and reports broken links: missing scripts and references
    /// that point at an asset which no longer exists. Run it after moving or deleting assets.
    ///   Editor menu:  Baby's First > Check Project
    ///   Command line: Unity -batchmode -quit -projectPath . -executeMethod BabysFirst.EditorTools.ProjectCheck.Run
    /// From the command line it fails (non-zero exit) when anything is broken. Gaps that an asset
    /// pack shipped with (inside a prefab from Assets/ThirdParty) are listed but do not fail it.
    /// </summary>
    public static class ProjectCheck
    {
        [MenuItem("Baby's First/Check Project")]
        public static void Run()
        {
            var problems = new List<string>();
            var packGaps = new List<string>();
            var log = new StringBuilder();

            var shipping = WebBuild.Scenes();
            foreach (var scene in shipping)
                if (!File.Exists(scene)) problems.Add("Shipping scene is missing: " + scene);

            // Every scene we own, shipping or not.
            var all = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project" })
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
            foreach (var path in all)
            {
                int before = problems.Count;
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                        CheckObject(t.gameObject, path, problems, packGaps);
                log.AppendLine((shipping.Contains(path) ? "  ships  " : "  local  ") + path + ": " + (problems.Count - before) + " problem(s)");
            }

            if (Resources.Load<AudioClip>("Music/one_sheep_six_banks_loop") == null)
                problems.Add("Soundtrack clip not found in a Resources/Music folder.");

            var report = new StringBuilder();
            report.AppendLine("[ProjectCheck] " + all.Length + " scene(s), " + problems.Count + " problem(s)");
            report.Append(log);
            foreach (var p in problems) report.AppendLine("  - " + p);
            if (packGaps.Count > 0)
            {
                report.AppendLine(packGaps.Count + " gap(s) inside asset-pack prefabs (not ours to fix):");
                foreach (var p in packGaps) report.AppendLine("  ~ " + p);
            }

            if (problems.Count == 0) { Debug.Log(report.ToString()); return; }
            Debug.LogError(report.ToString());
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }

        static void CheckObject(GameObject go, string scene, List<string> ours, List<string> packGaps)
        {
            var problems = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go).StartsWith("Assets/ThirdParty/") ? packGaps : ours;
            string where = scene + " > " + PathOf(go.transform);
            int missingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missingScripts > 0) problems.Add(where + ": " + missingScripts + " missing script(s)");

            foreach (var component in go.GetComponents<Component>())
            {
                if (component == null) continue;
                // Particle systems that do not draw meshes often carry stale mesh slots from their pack.
                bool unusedMeshSlots = component is ParticleSystemRenderer psr && psr.renderMode != ParticleSystemRenderMode.Mesh;
                var so = new SerializedObject(component);
                var prop = so.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (unusedMeshSlots && prop.propertyPath.StartsWith("m_Mesh")) continue;
                    // A reference that still holds an ID but resolves to nothing is a broken link.
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue == null && !prop.objectReferenceEntityIdValue.Equals(default(EntityId)))
                        problems.Add(where + " [" + component.GetType().Name + "." + prop.propertyPath + "]: missing reference");
                }
            }
        }

        static string PathOf(Transform t)
        {
            var parts = new List<string>();
            for (; t != null; t = t.parent) parts.Add(t.name);
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
