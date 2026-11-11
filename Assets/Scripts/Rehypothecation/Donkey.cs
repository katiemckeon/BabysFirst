using System.Collections.Generic;
using UnityEngine;

namespace BabysFirst.Rehypothecation
{
    /// <summary>Andreas. Follows a target or walks a list of waypoints; no physics, no pathfinding.</summary>
    public class Donkey : MonoBehaviour
    {
        public Animator animator;
        public float speed = 2.8f;
        public float turnSpeed = 360f;
        public float followDistance = 2.2f;

        public bool Arrived { get; private set; } = true;

        Transform followTarget;
        readonly Queue<Vector3> waypoints = new Queue<Vector3>();
        string bodyState;

        void Start()
        {
            Body("Idle_A");
            Eyes("Eyes_Blink");
        }

        public void Follow(Transform target)
        {
            followTarget = target;
            waypoints.Clear();
            Arrived = false;
        }

        public void WalkPath(params Vector3[] points)
        {
            followTarget = null;
            waypoints.Clear();
            foreach (var p in points) waypoints.Enqueue(p);
            Arrived = points.Length == 0;
        }

        public void Body(string state)
        {
            if (bodyState == state) return;
            bodyState = state;
            animator.CrossFadeInFixedTime(state, 0.2f, 0);
        }

        public void Eyes(string state)
        {
            animator.Play(state, 1);
        }

        public void Face(Vector3 point)
        {
            Vector3 d = point - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d);
        }

        void Update()
        {
            if (followTarget != null)
            {
                bool moving = StepToward(followTarget.position, followDistance);
                Body(moving ? "Walk" : "Idle_A");
            }
            else if (waypoints.Count > 0)
            {
                if (!StepToward(waypoints.Peek(), 0.1f)) waypoints.Dequeue();
                if (waypoints.Count == 0)
                {
                    Arrived = true;
                    Body("Idle_A");
                }
                else Body("Walk");
            }
        }

        // Returns false once within stopDistance.
        bool StepToward(Vector3 target, float stopDistance)
        {
            Vector3 d = target - transform.position;
            d.y = 0f;
            float dist = d.magnitude;
            // The slack matters: each step stops exactly stopDistance short, so without it a
            // rounding error can leave Andreas a hair outside and never "arrive".
            if (dist <= stopDistance + 0.02f) return false;
            var want = Quaternion.LookRotation(d);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnSpeed * Time.deltaTime);
            transform.position += d / dist * Mathf.Min(speed * Time.deltaTime, dist - stopDistance);
            return true;
        }
    }
}
