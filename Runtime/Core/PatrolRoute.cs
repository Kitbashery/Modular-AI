using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 MIT License

Copyright(c) 2022 Kitbashery

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.



Need support or additional features? Please visit https://kitbashery.com/
*/

namespace Kitbashery.AI
{
    /// <summary>
    /// Defines a patrol route relative to its parent <see cref="Transform"/>.
    /// </summary>
    [AddComponentMenu("Kitbashery/AI/Patrol Route")]
    public class PatrolRoute : MonoBehaviour
    {
        /// <summary>
        /// Waypoints that define a patrol route in the order they should be navigated to.
        /// Note: waypoints are fixed positions in world space.
        /// </summary>
        public Vector3[] waypoints;

        /// <summary>
        /// The patrol route relative to it's transform (waypoints in local space).
        /// Note: route move with the transform.
        /// </summary>
        [HideInInspector]
        public Vector3[] route;

        private Vector3 previousPosition = Vector3.negativeInfinity;

        /// <summary>
        /// Hits chached when randomizing waypoints.
        /// </summary>
        [HideInInspector]
        public RaycastHit[] hits;

        private void OnValidate()
        {
            RefreshPatrolRoute();
        }

        private void OnDrawGizmos()
        {
            RefreshPatrolRoute();

            if (route.Length > 1)
            {
                for (int i = 0; i <= route.Length - 1; i++)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(route[i], 0.1f);
                    Debug.DrawLine(route[i], route[i] + Vector3.forward, Color.blue);
                    Debug.DrawLine(route[i], route[i] + Vector3.right, Color.red);
                    if (i == route.Length - 1)
                    {
                        Debug.DrawLine(route[i], route[0], Color.cyan);
                    }
                    else
                    {
                        Debug.DrawLine(route[i], route[i + 1], Color.cyan);
                    }
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(route[i] + (Vector3.up * 0.2f), "<color=#000000ff>" + i.ToString() + "</color>", new GUIStyle(GUI.skin.label) { richText = true });
#endif
                    if (route[i].y > 0)
                    {
                        Debug.DrawLine(route[i], route[i] + (route[i].y * Vector3.down), Color.green);
                    }
                    else
                    {
                        Debug.DrawLine(route[i], route[i] + (route[i].y * Vector3.up), Color.green);
                    }
                }
            }
        }


        private void Update()
        {
            //Update the patrol route if it has moved.
            if(transform.position != previousPosition)
            {
                RefreshPatrolRoute();
                previousPosition = transform.position;
            }
        }
        
        /// <summary>
        /// Refreshes the patrol route based on the current waypoint and transform position.
        /// </summary>
        public void RefreshPatrolRoute()
        {
            if(route.Length != waypoints.Length)
            {
                route = waypoints;
            }

            for (int i = 0; i <= route.Length - 1; i++)
            {
                route[i] = waypoints[i] + transform.position;
            }
        }

        public void RandomizeWaypoints( float radius, float maxDistance, LayerMask mask, QueryTriggerInteraction triggerInteraction)
        {
            if (Physics.SphereCastNonAlloc(transform.position, radius, Vector3.zero, hits, maxDistance, mask, triggerInteraction) > 0)
            {
                for (int i = 0; i <= waypoints.Length - 1; i++)
                {
                    if(hits.Length <= i)
                    {
                        waypoints[i] = hits[i].point;
                    }
                }
                RefreshPatrolRoute();
            }
        }
    }
    public enum PatrolTypes { loop, pingPong, randomize }
}