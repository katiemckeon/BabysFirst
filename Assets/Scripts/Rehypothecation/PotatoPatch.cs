using System;
using UnityEngine;

namespace BabysFirst.Rehypothecation
{
    /// <summary>
    /// One Wild Harvest potato patch, driven by the director instead of the pack's demo
    /// PatchController: it sits unripe, ripens when told to, and is picked with one click.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class PotatoPatch : MonoBehaviour
    {
        public GameObject[] stages;      // the pack's five growth meshes, in order
        public Transform plants;         // parent of the stages, pulsed while ripe
        public GameObject leafBurst;     // the pack's leaf-drop particle object
        public int unripeStage = 1;
        public int ripeStage = 3;
        public int pickedStage = 4;

        public bool Ripe { get; private set; }
        public event Action<PotatoPatch> Picked;

        Interactable interactable;

        void Awake()
        {
            interactable = GetComponent<Interactable>();
            interactable.available = false;
            interactable.Clicked += Pick;
            Show(unripeStage);
        }

        public void Ripen()
        {
            Ripe = true;
            Show(ripeStage);
            interactable.available = true;
        }

        void Pick()
        {
            if (!Ripe) return;
            Ripe = false;
            interactable.available = false;
            plants.localScale = Vector3.one;
            Show(pickedStage);
            if (leafBurst != null)
            {
                leafBurst.SetActive(false);
                leafBurst.SetActive(true);
            }
            Picked?.Invoke(this);
        }

        void Update()
        {
            if (!Ripe) return;
            float s = 1f + 0.06f * Mathf.Sin(Time.time * 5f);
            plants.localScale = new Vector3(1f, s, 1f);
        }

        void Show(int index)
        {
            for (int i = 0; i < stages.Length; i++) stages[i].SetActive(i == index);
        }
    }
}
