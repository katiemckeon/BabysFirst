using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BabysFirst.Menu
{
    /// <summary>The contents page of the series: each playable row loads its game's scene.</summary>
    public class MainMenu : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public Button button;
            public string scene;
        }

        public Entry[] entries;

        void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            foreach (var e in entries)
            {
                string scene = e.scene;
                e.button.onClick.AddListener(() => SceneManager.LoadScene(scene));
            }
            // First game starts selected, so arrow keys and Enter work as well as the mouse.
            if (entries.Length > 0 && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(entries[0].button.gameObject);
        }
    }
}
