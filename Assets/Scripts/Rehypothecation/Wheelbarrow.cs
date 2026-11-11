using System.Collections;
using UnityEngine;

namespace BabysFirst.Rehypothecation
{
    /// <summary>Farmer Tucker's wheelbarrow: parked, or pushed in front of the farmer, collecting potatoes.</summary>
    [RequireComponent(typeof(Interactable))]
    public class Wheelbarrow : MonoBehaviour
    {
        public Transform pile;                 // potatoes land here, in the tray
        public Vector3 carryLocalPosition = new Vector3(0f, 0f, 1.9f);
        public Vector3 carryLocalEuler = new Vector3(0f, -90f, 0f);
        public float pileRadius = 0.42f;

        public Interactable Interactable { get; private set; }
        public bool Carried { get; private set; }

        int landed;

        void Awake()
        {
            Interactable = GetComponent<Interactable>();
        }

        public void AttachTo(Transform carrier)
        {
            Carried = true;
            Interactable.available = false;
            SetColliders(false);
            transform.SetParent(carrier, false);
            transform.localPosition = carryLocalPosition;
            transform.localRotation = Quaternion.Euler(carryLocalEuler);
        }

        public void ParkAt(Vector3 position, Quaternion rotation)
        {
            Carried = false;
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(position, rotation);
            SetColliders(true);
        }

        void SetColliders(bool on)
        {
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = on;
        }

        /// <summary>Throw <paramref name="count"/> potatoes from a patch into the tray.</summary>
        public void Receive(GameObject potatoPrefab, Vector3 from, int count, float potatoScale)
        {
            for (int i = 0; i < count; i++) StartCoroutine(Fly(potatoPrefab, from, i * 0.12f, potatoScale));
        }

        IEnumerator Fly(GameObject prefab, Vector3 from, float delay, float scale)
        {
            yield return new WaitForSeconds(delay);
            var potato = Instantiate(prefab, from, Random.rotation);
            potato.transform.localScale = Vector3.one * scale;
            foreach (var rb in potato.GetComponentsInChildren<Rigidbody>()) Destroy(rb);
            foreach (var c in potato.GetComponentsInChildren<Collider>()) Destroy(c);

            int slot = landed++;
            float angle = slot * 2.4f; // golden-angle spiral keeps the heap even
            float r = pileRadius * Mathf.Sqrt((slot % 9 + 1) / 9f);
            Vector3 local = new Vector3(Mathf.Cos(angle) * r, 0.14f * (slot / 9), Mathf.Sin(angle) * r);

            const float duration = 0.6f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                Vector3 to = pile.TransformPoint(local);
                potato.transform.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 1.6f;
                potato.transform.Rotate(360f * Time.deltaTime, 200f * Time.deltaTime, 0f);
                yield return null;
            }
            potato.transform.SetParent(pile, true);
            potato.transform.position = pile.TransformPoint(local);
        }
    }
}
