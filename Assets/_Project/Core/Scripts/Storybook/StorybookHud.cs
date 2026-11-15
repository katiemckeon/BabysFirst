using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BabysFirst.Storybook
{
    /// <summary>
    /// The Storybook UI for one Baby's First game: every piece of text sits on a paper page laid
    /// over the low-poly scene. Built by the editor-side StorybookKit; scenes only call the
    /// methods here. See the Storybook UI Guide in design/.
    /// </summary>
    public class StorybookHud : MonoBehaviour
    {
        [Header("Over the game")]
        public GameObject playLayer;
        public Image reticleDot;
        public GameObject promptTag;
        public GameObject promptIcon;
        public TMP_Text promptVerb, promptTarget, promptTail;

        [Header("Narration strip")]
        public RectTransform narration;
        public TMP_Text dropCap, body, pageNumber;
        public float narrationPadX = 34f, narrationPadTop = 16f, narrationFooter = 40f, capGap = 6f;

        [Header("Full pages")]
        public GameObject titlePage;
        public Button playButton;
        public GameObject chapterCard;
        public GameObject pauseMenu;
        public Button carryOnButton, restartButton, closeBookButton;
        public GameObject endPage;
        public Button menuButton, replayButton;

        void Awake()
        {
            narration.gameObject.SetActive(false);
            promptTag.SetActive(false);
            chapterCard.SetActive(false);
            pauseMenu.SetActive(false);
            endPage.SetActive(false);
            playLayer.SetActive(false);
        }

        public void ShowTitle(bool on) { titlePage.SetActive(on); }
        public void ShowChapterCard(bool on) { chapterCard.SetActive(on); }
        public void ShowPlayLayer(bool on) { playLayer.SetActive(on); }
        public void ShowPause(bool on)
        {
            pauseMenu.SetActive(on);
            // "Carry on" starts selected, so the keyboard works as well as the mouse.
            if (on && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(carryOnButton.gameObject);
        }

        public void ShowEnd()
        {
            playLayer.SetActive(false);
            narration.gameObject.SetActive(false);
            endPage.SetActive(true);
        }

        /// <summary>Lay one narration page on the bottom strip, opening with a drop cap.</summary>
        public void Say(string text, int page)
        {
            narration.gameObject.SetActive(true);
            dropCap.text = text.Substring(0, 1);
            body.text = text.Substring(1);
            pageNumber.text = "page " + page;

            // The cap hangs at the left; the body is set to its right and the strip grows to fit.
            dropCap.ForceMeshUpdate();
            float capWidth = dropCap.preferredWidth;
            float left = narrationPadX + capWidth + capGap;
            float width = narration.rect.width - left - narrationPadX;
            float bodyHeight = body.GetPreferredValues(body.text, width, 0f).y;
            var rt = body.rectTransform;
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
            rt.offsetMax = new Vector2(-narrationPadX, rt.offsetMax.y);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, bodyHeight);
            float content = Mathf.Max(bodyHeight, dropCap.rectTransform.rect.height);
            narration.sizeDelta = new Vector2(narration.sizeDelta.x, narrationPadTop + content + narrationFooter);
        }

        public void Hush() { narration.gameObject.SetActive(false); }

        /// <summary>The paper tag under the reticle: verb in italic, target in bold, e.g. "Click Andreas".</summary>
        public void SetPrompt(string verb, string target, string tail, bool clickable)
        {
            bool any = !string.IsNullOrEmpty(verb) || !string.IsNullOrEmpty(target);
            promptTag.SetActive(any);
            if (any)
            {
                promptIcon.SetActive(clickable);
                promptVerb.text = verb ?? "";
                promptTarget.text = target ?? "";
                promptTail.text = tail ?? "";
                promptTail.gameObject.SetActive(!string.IsNullOrEmpty(tail));
            }
            reticleDot.color = clickable ? Palette.Marigold : Palette.Paper;
        }
    }
}
