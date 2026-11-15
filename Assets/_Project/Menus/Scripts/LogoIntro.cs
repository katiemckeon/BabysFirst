using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BabysFirst.Intro
{
    /// <summary>
    /// Plays the studio logo once, frame by frame, then loads the game. Unity cannot play a GIF,
    /// so the frames and their timings are extracted from it ahead of time. Any key or click skips.
    /// </summary>
    public class LogoIntro : MonoBehaviour
    {
        public RawImage screen;
        public Texture2D[] frames;
        public float[] seconds;          // how long each frame stays up
        public string nextScene;
        public float skipAfter = 0.3f;   // ignore input for a moment so a stray click does not skip it

        int index;
        float frameTime, total;

        void Start()
        {
            Show(0);
        }

        void Update()
        {
            total += Time.deltaTime;
            frameTime += Time.deltaTime;
            while (index < frames.Length && frameTime >= seconds[index])
            {
                frameTime -= seconds[index];
                index++;
                if (index < frames.Length) Show(index);
            }
            if (index >= frames.Length || (total > skipAfter && SkipPressed()))
            {
                enabled = false;
                SceneManager.LoadScene(nextScene);
            }
        }

        void Show(int i)
        {
            screen.texture = frames[i];
        }

        static bool SkipPressed()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            return (kb != null && kb.anyKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        }
    }
}
