using System;
using UnityEngine;

namespace BabysFirst.Rehypothecation
{
    /// <summary>Something Farmer Brown can look at and click. Needs a collider on itself or a child.</summary>
    public class Interactable : MonoBehaviour
    {
        // Shown on the prompt tag as "verb target", e.g. "Click Andreas".
        public string verb = "Click";
        public string target;
        public bool available;

        public event Action Clicked;

        public void Click()
        {
            if (available) Clicked?.Invoke();
        }
    }
}
