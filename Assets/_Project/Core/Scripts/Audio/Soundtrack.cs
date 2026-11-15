using UnityEngine;

namespace BabysFirst
{
    /// <summary>
    /// The one background track for the whole series. Started once before the first scene loads,
    /// on an object that survives scene changes, so going to the menu, opening a chapter, finishing
    /// it or pressing Play again never restarts it. It just loops forever.
    /// </summary>
    public static class Soundtrack
    {
        const string ClipPath = "Music/one_sheep_six_banks_loop"; // Assets/_Project/Shared/Audio/Resources/Music/
        const float Volume = 0.5f;

        static AudioSource source;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (source != null) return;

            var clip = Resources.Load<AudioClip>(ClipPath);
            if (clip == null)
            {
                Debug.LogWarning("[Soundtrack] No clip at Resources/" + ClipPath);
                return;
            }

            var go = new GameObject("Soundtrack");
            Object.DontDestroyOnLoad(go);

            source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;          // plain stereo, wherever the camera is
            source.priority = 0;               // never stolen by other sounds
            source.ignoreListenerPause = true; // keeps playing through pause menus
            source.volume = Volume;
            source.Play();
        }
    }
}
