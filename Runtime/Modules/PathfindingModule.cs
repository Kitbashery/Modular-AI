using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    /// Module that defines actions and conditions for a <see cref="AIAgent"/> to pathfind using Unity's built-in <see cref="NavMeshAgent"/>.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/modular-ai/pathfinding-module.html")]
    [DisallowMultipleComponent]
    [AddComponentMenu("Kitbashery/AI/AI Modules/Pathfinding Module")]
    [RequireComponent(typeof(NavMeshAgent))]
    public class PathfindingModule : AIModule
    {
        #region Properties:

        /// <summary>
        /// Toggles debug mode for displaying gizmos when the agent is selected.
        /// </summary>
        [Tooltip("Toggles debug mode for displaying gizmos when the agent is selected.")]
        public bool debugMode = false;

        [Header("References:")]
        public NavMeshAgent agent;

        /// <summary>
        /// The target location for the agent to pathfind to.
        /// </summary>
        [Tooltip("The target location for the agent to pathfind to.")]
        public Transform target;

        public MemoryModule memory;

        /// <summary>
        /// Positions representing a patrol route in the order they should be navigated to.
        /// </summary>
        [Tooltip("Positions representing a patrol route in the order they should be navigated to.")]
        public PatrolRoute patrolRoute;

        /// <summary>
        /// The previous destination we reached.
        /// </summary>
        private Vector3 previousDestination;

        [Header("Flee/Follow:")]
        /// <summary>
        /// How far the agent should flee from a target.
        /// </summary>
        [Tooltip("How far the agent should flee from a target.")]
        public float fleeDistance = 16;
        /// <summary>
        /// How far from a target the agent need to be to follow it.
        /// </summary>
        [Tooltip("How far from a target the agent need to be to follow it.")]
        public float followDistance = 4;

        [Header("Wander:")]
        /// <summary>
        /// How far can this agent wander?
        /// </summary>
        [Tooltip("How far the agent can wander.")]
        public float wanderRange = 4;

        /// <summary>
        /// How long the agent should wait until it wanders again.
        /// </summary>
        [Tooltip("How long the agent should wait until it wanders again.")]
        [Min(0)]
        public float wanderTime = 1.5f;

        private bool isWandering = false;

        [Header("Patrol:")]

        [Tooltip("The time the agent waits before moving to the next waypoint.")]
        public float patrolWaitTime = 0;

        [Tooltip("How many times the agent should patrol though its waypoint route. (0 = forever)")]
        [Min(0)]
        public int timesToPatrol = 0;

        [HideInInspector]
        public int timesPatroled = 0;

        public PatrolTypes patrolType = PatrolTypes.loop;

        /// <summary>
        /// The current point in the patrol route the agent should be moving to.
        /// </summary>
        private int patrolPoint = -1;

        private bool isPatrolling = false;

        private bool reversePatrol = false;

        /// <summary>
        /// Is the patrol currently waiting or switching waypoints.
        /// </summary>
        private bool isPatrolWaiting;

        /// <summary>
        /// How long the agent has waited between wandering or moving to a waypoint.
        /// </summary>
        private float currentWaitTime = 0;

        #endregion

        #region Modular AI Condition Overrides:

        /// <summary>
        /// Condition names used by the editor to set identifiers based on the index of the selected string in this array.
        /// Note: You can encapsulate this with #if UNITY_EDITOR if you don't plan to pass this to UI dropdowns during runtime.
        /// Note: Rearranging the array will break editor visuals for any behaviour logic that already uses these conditions.
        /// </summary>
        private string[] _conditions;
        public override string[] conditions
        {
            get
            {
                if (_conditions == null || _conditions.Length == 0)
                {
                    _conditions = new string[7] { "destination changed", "destination reached", "has pathfinding target", "has patrol route", "is patrolling", "is wandering", "is patrol waiting" };
                }
                return _conditions;
            }
        }

        /// <summary>
        /// Checks a condition based on an index that should match an action's name in this module's <see cref="actions"/> array.
        /// </summary>
        /// <param name="conditionIndex"></param>
        public override bool checkCondition(int conditionIndex)
        {
            switch (conditionIndex)
            {
                case 0:

                    return agent.destination != previousDestination;

                case 1:

                    if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
                    {
                        previousDestination = agent.destination;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 2:

                    return target != null;

                case 3:

                    return (patrolRoute != null && patrolRoute.route.Length > 0);

                case 4:

                    return isPatrolling;

                case 5:

                    return isWandering;

                case 6:

                    return isPatrolWaiting;
            }

            return false;
        }

        #endregion

        #region Modular AI Action Overrides:

        /// <summary>
        /// Action names used by the editor to set identifiers based on the index of the selected string in this array.
        /// Note: You can encapsulate this with #if UNITY_EDITOR if you don't plan to pass this to UI dropdowns during runtime.
        /// Note: Rearranging the array will break editor visuals for any behaviour logic that already uses these actions.
        /// </summary>
        private string[] _actions;
        public override string[] actions
        {
            get
            {
                if (_actions == null || _actions.Length == 0)
                {
                    _actions = new string[12] { "flee", "follow target", "idle", "move to target", "patrol", "stop patrol", "wander", "go to previous location", "path target = focus object", "path target = focus agent", "path target = focus player", "path target = nothing" };
                }
                return _actions;
            }
        }

        /// <summary>
        /// Executes an action based on an index that should match an action's name in this module's <see cref="actions"/> array.
        /// </summary>
        /// <param name="actionIndex"></param>
        public override void executeAction(int actionIndex)
        {
            switch (actionIndex)
            {
                case 0:

                    Flee();

                    break;

                case 1:

                    FollowTarget();

                    break;

                case 2:

                    Idle();

                    break;

                case 3:

                    MoveToTarget();

                    break;

                case 4:

                    Patrol();

                    break;

                case 5:

                    StopPatrol();

                    break;

                case 6:

                    Wander();

                    break;

                case 7:

                    agent.destination = previousDestination;

                    break;

                case 8:

                    if(memory != null)
                    {
                        if(memory.objectFocus != null)
                        {
                            target = memory.objectFocus.transform;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("|MAI|: " + gameObject.name + " Tried to set pathfinding target to focused memory but the variable memory was not assigned in the inspector.");
                    }

                    break;

                case 9:

                    if (memory != null)
                    {
                        if(memory.agentFocus != null)
                        {
                            target = memory.agentFocus.transform;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("|MAI|: " + gameObject.name + " Tried to set pathfinding target to focused memory but the variable memory was not assigned in the inspector.");
                    }

                    break;

                case 10:

                    if (memory != null)
                    {
                        if(memory.playerFocus != null)
                        {
                            target = memory.playerFocus.transform;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("|MAI|: " + gameObject.name + " Tried to set pathfinding target to focused memory but the variable memory was not assigned in the inspector.");
                    }

                    break;

                case 11:

                    target = null;

                    break;
            }
        }

        #endregion

        #region Initialization & Updates:

        // Awake is called when the script instance is being loaded.
        void Awake()
        {
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }
            previousDestination = agent.destination;
        }

        private void OnDrawGizmosSelected()
        {
            if (debugMode == true)
            {
                if(agent != null)
                {
                    // Draw agent's path:
                    for (int i = 0; i < agent.path.corners.Length - 1; i++)
                    {
                        Debug.DrawLine(agent.path.corners[i], agent.path.corners[i + 1], Color.blue);
                    }
                }
            }
        }

        #endregion

        #region Methods:

        public void Flee()
        {
            if(target != null)
            {
                agent.SetDestination(target.forward * fleeDistance);
            }
        }

        public void FollowTarget()
        {
            if (target != null && (followDistance <= Vector3.Distance(transform.position, target.position)))
            {
                agent.SetDestination(target.position + (-target.forward * followDistance));
                agent.transform.LookAt(target.position);
                agent.stoppingDistance = agent.radius * followDistance;
            }
        }

        public void Idle()
        {
            agent.autoBraking = true;
            agent.SetDestination(transform.position);
            previousDestination = agent.destination;
        }

        public void MoveToTarget()
        {
            if (target != null && gameObject.activeSelf == true)
            {
                agent.SetDestination(target.position);
            }
        }

        public void StopPatrol()
        {
            isPatrolling = false;
            timesPatroled = 0;
            agent.SetDestination(transform.position);
        }

        public void Patrol()
        {
            if(patrolRoute != null)
            {
                if (patrolRoute.route.Length > 0)
                {
                    if (timesPatroled < timesToPatrol || timesToPatrol == 0)
                    {
                        isPatrolWaiting = !Wait(patrolWaitTime);
                        if (isPatrolWaiting == false && agent.remainingDistance <= Mathf.Epsilon)
                        {
                            switch (patrolType)
                            {
                                case PatrolTypes.loop:

                                    patrolPoint++;
                                    if (patrolPoint == patrolRoute.route.Length)
                                    {
                                        patrolPoint = 0;
                                        timesPatroled++;
                                    }

                                    break;

                                case PatrolTypes.randomize:

                                    patrolPoint = Random.Range(0, patrolRoute.route.Length);
                                    timesPatroled++;

                                    break;

                                case PatrolTypes.pingPong:

                                    if (timesPatroled == 1 || reversePatrol == true)
                                    {
                                        patrolPoint--;
                                        if (patrolPoint == -1)
                                        {
                                            timesPatroled++;
                                            patrolPoint++;
                                            reversePatrol = false;
                                        }
                                    }
                                    else
                                    {
                                        patrolPoint++;
                                        if (patrolPoint == patrolRoute.route.Length)
                                        {
                                            timesPatroled++;
                                            patrolPoint--;
                                            reversePatrol = true;
                                        }
                                    }

                                    break;
                            }

                            agent.autoBraking = true;
                            previousDestination = transform.position;
                            agent.SetDestination(patrolRoute.route[patrolPoint]);

                            currentWaitTime = patrolWaitTime;
                        }
                    }
                    else
                    {
                        StopPatrol();
                    }
                }
                else
                {
                    Debug.LogWarning("|MAI|: (" + gameObject.name + ") could not begin patrol because the MAI_PathfindingModule component does not have any waypoints.");
                }
            }
        }

        /// <summary>
        /// Waits for the defined time.
        /// </summary>
        /// <param name="time">The time to wait.</param>
        /// <returns>Returns true waiting is over.</returns>
        private bool Wait(float time)
        {
            currentWaitTime -= Time.deltaTime;
            if(currentWaitTime <= 0)
            {
                currentWaitTime = time;
                return true;
            }

            return false;
        }

        public void Wander()
        {
            if (isWandering == false)
            {
                Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRange) + transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, wanderRange, -1) && gameObject.activeSelf == true)
                {
                    currentWaitTime = wanderTime;
                    isWandering = true;
                    agent.SetDestination(hit.position);
                }
            }
            else
            {
                if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
                {
                    previousDestination = agent.destination;
                    isWandering = !Wait(wanderTime);
                }
            }
        }

        #endregion
    }
}