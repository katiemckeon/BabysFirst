using System;
using System.Collections;
using BabysFirst.Menu;
using BabysFirst.Storybook;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BabysFirst.Rehypothecation
{
    [Serializable]
    public class NarrationLine
    {
        public string id;
        [TextArea(2, 4)] public string text;
        public AudioClip clip; // optional; pages are timed from the text until VO exists
    }

    /// <summary>
    /// Runs Baby's First Rehypothecation from title page to end page. The player's actions only
    /// gate the next narrator page; the borrowing chain itself happens entirely in the narration.
    /// </summary>
    public class RehypothecationDirector : MonoBehaviour
    {
        [Header("Narration (order matters)")]
        public NarrationLine[] lines;
        public AudioSource voice;          // optional
        public float secondsPerWord = 0.36f;
        public float minLineSeconds = 2.5f;
        public float lingerSeconds = 2f;   // extra time a page stays up if nobody presses Space

        [Header("Cast")]
        public FarmerController farmer;
        public Donkey donkey;
        public Interactable donkeyClick;
        public Wheelbarrow barrow;
        public PotatoPatch[] patches;
        public StorybookHud hud;

        [Header("Places")]
        public Transform gateHinge;
        public float gateOpenYaw = -100f;
        public Collider gateBlocker;       // keeps the farmer out of the pen until the ending
        public Transform gateOutside;      // just south of the gate
        public Transform penInside;        // just north of the gate
        public Transform feederSpot;       // where Andreas waits
        public Transform feeder;
        public Transform barrowPark;
        public Transform penCenter;

        [Header("Potatoes")]
        public GameObject potatoPrefab;
        public int potatoesPerPatch = 3;
        public float potatoScale = 1.4f;

        public string Beat { get; private set; } = "title";
        public bool Paused { get; private set; }

        // "Begin the chapter again" reloads the scene and skips the title page.
        static bool skipTitleOnce;

        string objVerb, objTarget, objTail;
        bool started, turnPage, donkeyClicked, barrowClicked;
        int picked, page;

        void Awake()
        {
            hud.playButton.onClick.AddListener(Begin);
            hud.replayButton.onClick.AddListener(() => Reload(false));
            hud.carryOnButton.onClick.AddListener(() => SetPaused(false));
            hud.restartButton.onClick.AddListener(() => Reload(true));
            hud.closeBookButton.onClick.AddListener(BackToMenu);
            hud.menuButton.onClick.AddListener(BackToMenu);
            donkeyClick.Clicked += () => donkeyClicked = true;
            barrow.GetComponent<Interactable>().Clicked += () => barrowClicked = true;
            foreach (var p in patches) p.Picked += OnPicked;
        }

        public void Begin() { started = true; }
        public void TurnPage() { turnPage = true; }

        void Reload(bool skipTitle)
        {
            Time.timeScale = 1f;
            skipTitleOnce = skipTitle;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Back to the series' contents page. Falls back to this game's own title page when the
        // menu scene is not in the build (for example, playing this scene alone).
        public void BackToMenu()
        {
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded(GameCatalog.MenuScene)) SceneManager.LoadScene(GameCatalog.MenuScene);
            else Reload(false);
        }

        public void SetPaused(bool on)
        {
            if (Beat == "title" || Beat == "chapter" || Beat == "end") return;
            Paused = on;
            Time.timeScale = on ? 0f : 1f;
            farmer.SetControls(!on);
            hud.ShowPause(on);
        }

        IEnumerator Start()
        {
            farmer.SetControls(false);
            if (skipTitleOnce)
            {
                skipTitleOnce = false;
                hud.ShowTitle(false);
            }
            else
            {
                hud.ShowTitle(true);
                yield return new WaitUntil(() => started);
                hud.ShowTitle(false);
            }

            // Chapter card: Space (or a click) turns the page into the scene.
            Beat = "chapter";
            hud.ShowChapterCard(true);
            turnPage = false;
            yield return null;
            yield return new WaitUntil(() => turnPage);
            hud.ShowChapterCard(false);
            hud.ShowPlayLayer(true);
            farmer.SetControls(true);

            // 1-2. Meet Andreas, hear the problem.
            Beat = "intro";
            yield return Say(0);
            StartCoroutine(BounceGate());
            yield return Say(1);

            // 3. Walk Andreas to Farmer Tucker's pen.
            Beat = "fetch-donkey";
            Objective("Click", "Andreas", null);
            donkeyClick.available = true;
            yield return new WaitUntil(() => donkeyClicked);
            donkeyClick.available = false;
            donkey.Eyes("Eyes_Happy");
            donkey.Follow(farmer.transform);

            Beat = "walk-to-pen";
            Objective("Walk", "Andreas", "to Farmer Tucker's pen");
            yield return new WaitUntil(() => Flat(farmer.transform.position, gateOutside.position) < 3.5f
                                            && Flat(donkey.transform.position, farmer.transform.position) < 5f);
            Objective(null, null, null);
            donkey.WalkPath(gateOutside.position, penInside.position, feederSpot.position);
            yield return new WaitUntil(() => donkey.Arrived);
            donkey.Face(feeder.position);
            donkey.Body("Eat");
            yield return SwingGate(0f);

            // Andreas is now Farmer Tucker's collateral.
            Beat = "collateral";
            yield return Say(2);

            // 4. Take the wheelbarrow.
            Beat = "take-barrow";
            Objective("Take", "the wheelbarrow", null);
            barrow.Interactable.available = true;
            yield return new WaitUntil(() => barrowClicked);
            barrow.AttachTo(farmer.transform);
            Objective(null, null, null);
            yield return Say(3);

            // 5-10. Six patches, one chain link each. Andreas leaves quietly during the third.
            for (int i = 0; i < patches.Length; i++)
            {
                Beat = "patch-" + (i + 1);
                Objective("Pick", "the wiggly potatoes", null);
                patches[i].Ripen();
                int need = i + 1;
                yield return new WaitUntil(() => picked >= need);
                Objective(null, null, null);
                if (i == 2) StartCoroutine(VanishWhenUnseen());
                yield return Say(4 + i);
            }
            Vanish();

            // 11. Return the wheelbarrow.
            Beat = "return-barrow";
            Objective("Return", "the wheelbarrow", "to Farmer Tucker");
            yield return new WaitUntil(() => Flat(farmer.transform.position, barrowPark.position) < 3.5f);
            Objective(null, null, null);
            barrow.ParkAt(barrowPark.position, barrowPark.rotation);

            // 12. Go and get Andreas.
            Beat = "collect-donkey";
            gateBlocker.enabled = false;
            yield return SwingGate(gateOpenYaw);
            Objective("Go and get", "Andreas", null);
            yield return new WaitUntil(() => Flat(farmer.transform.position, penCenter.position) < 2.6f);
            Objective(null, null, null);
            yield return Say(10);

            // 13-14. So collaborative.
            Beat = "finale";
            yield return Say(11);

            Beat = "end";
            farmer.SetControls(false);
            hud.ShowEnd();
            if (voice != null && lines[12].clip != null) voice.PlayOneShot(lines[12].clip);
        }

        void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb != null)
            {
                if (kb.spaceKey.wasPressedThisFrame) turnPage = true;
                if (kb.escapeKey.wasPressedThisFrame) SetPaused(!Paused);
            }
            if (Beat == "chapter" && mouse != null && mouse.leftButton.wasPressedThisFrame) turnPage = true;

            var focus = farmer.Focused;
            if (focus != null) hud.SetPrompt(focus.verb, focus.target, null, true);
            else hud.SetPrompt(objVerb, objTarget, objTail, false);
        }

        void Objective(string verb, string target, string tail)
        {
            objVerb = verb; objTarget = target; objTail = tail;
        }

        void OnPicked(PotatoPatch patch)
        {
            picked++;
            barrow.Receive(potatoPrefab, patch.transform.position + Vector3.up * 0.3f, potatoesPerPatch, potatoScale);
        }

        // One narration page. It turns on Space, or by itself once it has been up long enough to read.
        IEnumerator Say(int index)
        {
            var line = lines[index];
            page++;
            hud.Say(line.text, page);
            float seconds;
            if (voice != null && line.clip != null)
            {
                voice.PlayOneShot(line.clip);
                seconds = line.clip.length + 0.3f;
            }
            else
            {
                int words = line.text.Split(' ').Length;
                seconds = Mathf.Max(minLineSeconds, words * secondsPerWord) + lingerSeconds;
            }
            turnPage = false;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                if (turnPage && t > 0.4f) break;
                turnPage = false;
                yield return null;
            }
            hud.Hush();
            yield return new WaitForSeconds(0.35f);
        }

        IEnumerator BounceGate()
        {
            for (float t = 0f; t < 1.6f; t += Time.deltaTime)
            {
                float wobble = Mathf.Sin(t * 14f) * 6f * (1f - t / 1.6f);
                gateHinge.localRotation = Quaternion.Euler(0f, gateOpenYaw + wobble, 0f);
                yield return null;
            }
            gateHinge.localRotation = Quaternion.Euler(0f, gateOpenYaw, 0f);
        }

        IEnumerator SwingGate(float toYaw)
        {
            Quaternion from = gateHinge.localRotation, to = Quaternion.Euler(0f, toYaw, 0f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.9f)
            {
                gateHinge.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            gateHinge.localRotation = to;
        }

        // Andreas is only ever removed while the farmer has his back to the pen.
        IEnumerator VanishWhenUnseen()
        {
            while (donkey.gameObject.activeSelf)
            {
                var cam = farmer.cam.transform;
                if (Vector3.Angle(cam.forward, penCenter.position - cam.position) > 80f) Vanish();
                yield return null;
            }
        }

        // Nothing is left behind: the pen is simply empty.
        void Vanish()
        {
            donkey.gameObject.SetActive(false);
        }

        static float Flat(Vector3 a, Vector3 b)
        {
            a.y = 0f; b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
