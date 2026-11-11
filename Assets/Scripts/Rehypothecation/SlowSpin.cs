using UnityEngine;

namespace BabysFirst.Rehypothecation
{
    /// <summary>Turns its object slowly around the world up axis. Used to drift the cloud ring.</summary>
    public class SlowSpin : MonoBehaviour
    {
        public float degreesPerSecond = 0.6f;

        void Update()
        {
            transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
        }
    }
}
