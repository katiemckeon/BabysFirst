using UnityEngine;
using UnityEngine.InputSystem;

namespace BabysFirst.Rehypothecation
{
    /// <summary>
    /// First-person Farmer Brown: WASD or arrows to walk, mouse to look, click to interact with
    /// whatever <see cref="Interactable"/> is under the screen-center reticle.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FarmerController : MonoBehaviour
    {
        public Camera cam;
        public float walkSpeed = 3.4f;
        public float lookSensitivity = 0.12f;
        public float reach = 5f;
        public float pitch = 12f;

        public Interactable Focused { get; private set; }

        CharacterController cc;
        bool controlsEnabled;
        float yaw;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
            ApplyLook();
        }

        public void SetControls(bool on)
        {
            controlsEnabled = on;
            Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !on;
            if (!on) Focused = null;
        }

        void Update()
        {
            if (!controlsEnabled) return;

            var mouse = Mouse.current;
            var kb = Keyboard.current;
            bool click = mouse != null && mouse.leftButton.wasPressedThisFrame;

            // The browser drops pointer lock on Escape; the next click only takes it back.
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (click)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                click = false;
            }
            else if (mouse != null)
            {
                Vector2 d = mouse.delta.ReadValue() * lookSensitivity;
                yaw += d.x;
                pitch = Mathf.Clamp(pitch - d.y, -50f, 70f);
                ApplyLook();
            }

            Vector2 move = Vector2.zero;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1;
            }
            Vector3 dir = transform.right * move.x + transform.forward * move.y;
            cc.SimpleMove(Vector3.ClampMagnitude(dir, 1f) * walkSpeed);

            Focused = FindFocus();
            if (click && Focused != null) Focused.Click();
        }

        void ApplyLook()
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (cam != null) cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        Interactable FindFocus()
        {
            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out var hit, reach, ~0, QueryTriggerInteraction.Collide)) return null;
            var it = hit.collider.GetComponentInParent<Interactable>();
            return it != null && it.available ? it : null;
        }

        /// <summary>Move the farmer without fighting the CharacterController (used by tests and resets).</summary>
        public void Teleport(Vector3 position, float newYaw)
        {
            cc.enabled = false;
            transform.position = position;
            cc.enabled = true;
            yaw = newYaw;
            ApplyLook();
        }
    }
}
