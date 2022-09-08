using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    /// This module contains actions and condtions to remember <see cref="GameObject"/>s and <see cref="AIAgent"/>s that it has been made aware of in the environment.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/modular-ai/memory-module.html")]
    [AddComponentMenu("Kitbashery/AI/AI Modules/Memory Module")]
    [DisallowMultipleComponent]
    public class MemoryModule : AIModule
    {
        #region Properties:

        public UnityEvent memoryEvents;

        [Header("Environment Memory:")]   
        [NonReorderable]
        public List<GameObject> objects = new List<GameObject>();
        [NonReorderable]
        public List<AIAgent> agents = new List<AIAgent>();
        [NonReorderable]
        public List<GameObject> players = new List<GameObject>();

        [Header("Focused Memory:")]
        public GameObject objectFocus;
        public AIAgent agentFocus;
        public GameObject playerFocus;

        [Header("Search Tags:")]
        [Tooltip("Memory will only focus on a GameObject with this tag. (does not apply to agent, player or random focus actions.")]
        public string focusTag;
        public string agentTag = "Agent";
        public string playerTag = "Player";

        /// <summary>
        /// The current distance to compare against in a distance check.
        /// </summary>
        private float currentDistance;

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
                    _conditions = new string[6] { "has objects in memory", "has agents in memory", "has players in memory", "has a object in focus", "has a agent in focus", "has player focus" };
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

                    return objects.Count > 0;

                case 1:

                    return agents.Count > 0;

                case 2:

                    return players.Count > 0;

                case 3:

                    return objectFocus;

                case 4:

                    return agentFocus;

                case 5:

                    return playerFocus;

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
                    _actions = new string[24] { "clear / object memory", "clear / agent memory", "clear / player memory", "unfocus / object", "unfocus / agent", "unfocus / player", "find agents in environment", "find players in environment", "focus on / nearest / object", "focus on / farthest / object", "focus on / first / object", "focus on / last / object", "focus on / random / object", "focus on / nearest / agent", "focus on / farthest  /agent", "focus on / first / agent", "focus on / last / agent", "focus on / random / agent", "focus on / nearest / player", "focus on / farthest / player", "focus on / first / player", "focus on / last / player", "focus on / random / player", "invoke memory events" };
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

                    objects.Clear();

                    break;

                case 1:

                    agents.Clear();

                    break;

                case 2:

                    players.Clear();

                    break;

                case 3:

                    objectFocus = null;

                    break;

                case 4:

                    agentFocus = null;

                    break;

                case 5:

                    playerFocus = null;

                    break;

                case 6:

                    FindAgentsInEnvironmentMemory();

                    break;

                case 7:

                    FindPlayersInEnvironmentMemory();

                    break;

                case 8:

                    FocusOnGameObject(FocusModes.Nearest);

                    break;

                case 9:

                    FocusOnGameObject(FocusModes.Farthest);

                    break;

                case 10:

                    FocusOnGameObject(FocusModes.First);

                    break;

                case 11:

                    FocusOnGameObject(FocusModes.Last);

                    break;

                case 12:

                    FocusOnGameObject(FocusModes.Random);

                    break;

                case 13:

                    FocusOnAgent(FocusModes.Nearest);

                    break;

                case 14:

                    FocusOnAgent(FocusModes.Farthest);

                    break;

                case 15:

                    FocusOnAgent(FocusModes.First);

                    break;

                case 16:

                    FocusOnAgent(FocusModes.Last);

                    break;

                case 17:

                    FocusOnAgent(FocusModes.Random);

                    break;

                case 18:

                    FocusOnPlayer(FocusModes.Nearest);

                    break;

                case 19:

                    FocusOnPlayer(FocusModes.Farthest);

                    break;

                case 20:

                    FocusOnPlayer(FocusModes.First);

                    break;

                case 21:

                    FocusOnPlayer(FocusModes.Last);

                    break;

                case 22:

                    FocusOnPlayer(FocusModes.Random);

                    break;

                case 23:

                    memoryEvents.Invoke();

                    break;
            }
        }

        #endregion

        #region Initialization & Updates:

        private void Awake()
        {
            if(string.IsNullOrEmpty(playerTag))
            {
                Debug.LogError("|MAI|: playerTag not set in MAI_MemoryModule on " + gameObject.name + " make sure this field has a valid tag!");
            }

            if(string.IsNullOrEmpty(agentTag))
            {
                Debug.LogError("|MAI|: agentTag not set in MAI_MemoryModule on " + gameObject.name + " make sure this field has a valid tag!");
            }
        }

        #endregion

        #region Methods:

        public void FocusOnGameObject(FocusModes focusMode)
        {
            if(objects.Count > 0)
            {
                switch (focusMode)
                {
                    case FocusModes.Nearest:

                        float nearestDistance = Mathf.Infinity;
                        for (int i = objects.Count - 1; i >= 0; i--)
                        {
                            if (objects[i] != null)
                            {
                                if (string.IsNullOrEmpty(focusTag) || objects[i].tag == focusTag)
                                {
                                    currentDistance = Vector3.Distance(transform.position, objects[i].transform.position);
                                    if (currentDistance <= nearestDistance)
                                    {
                                        objectFocus = objects[i];
                                        nearestDistance = currentDistance;
                                    }
                                } 
                            }
                            else
                            {
                                objects.RemoveAt(i);
                            }
                        }

                        break;

                    case FocusModes.Farthest:

                        float farthestDistance = -Mathf.Infinity;
                        for (int i = objects.Count - 1; i >= 0; i--)
                        {
                            if (objects[i] != null)
                            {
                                if(string.IsNullOrEmpty(focusTag) || objects[i].CompareTag(focusTag) == true)
                                {
                                    currentDistance = Vector3.Distance(transform.position, objects[i].transform.position);
                                    if (currentDistance >= farthestDistance)
                                    {
                                        objectFocus = objects[i];
                                        farthestDistance = currentDistance;
                                    }
                                }
                            }
                            else
                            {
                                objects.RemoveAt(i);
                            }
                        }

                        break;

                    case FocusModes.Random:

                        objectFocus = objects[Random.Range(0, objects.Count)];

                        break;

                    case FocusModes.First:

                        if (string.IsNullOrEmpty(focusTag))
                        {
                            objectFocus = objects[0];
                        }
                        else
                        {
                            foreach(GameObject go in objects)
                            {
                                if(go.CompareTag(focusTag) == true)
                                {
                                    objectFocus = go;
                                    break;
                                }
                            }
                        }

                        break;

                    case FocusModes.Last:

                        if(string.IsNullOrEmpty(focusTag))
                        {
                            objectFocus = objects[objects.Count];
                        }
                        else
                        {
                            for(int i = objects.Count - 1; i >= 0; i--)
                            {
                                if(objects[i].CompareTag(focusTag) == true)
                                {
                                    objectFocus = objects[i];
                                    break;
                                }
                            }
                        }

                        break;
                }
            }
        }

        public void FocusOnAgent(FocusModes focusMode)
        {
            if (agents.Count > 0)
            {
                switch (focusMode)
                {
                    case FocusModes.Nearest:

                        float nearestDistance = Mathf.Infinity;
                        for (int i = agents.Count - 1; i >= 0; i--)
                        {
                            if (agents[i] != null)
                            {
                                currentDistance = Vector3.Distance(transform.position, agents[i].transform.position);
                                if (currentDistance <= nearestDistance)
                                {
                                    agentFocus = agents[i];
                                    nearestDistance = currentDistance;
                                }
                            }
                            else
                            {
                                agents.RemoveAt(i);
                            }
                        }

                        break;

                    case FocusModes.Farthest:

                        float farthestDistance = -Mathf.Infinity;
                        for (int i = agents.Count - 1; i >= 0; i--)
                        {
                            if (agents[i] != null)
                            {
                                currentDistance = Vector3.Distance(transform.position, agents[i].transform.position);
                                if (currentDistance >= farthestDistance)
                                {
                                    agentFocus = agents[i];
                                    farthestDistance = currentDistance;
                                }
                            }
                            else
                            {
                                agents.RemoveAt(i);
                            }
                        }

                        break;

                    case FocusModes.Random:

                        agentFocus = agents[Random.Range(0, agents.Count)];

                        break;

                    case FocusModes.First:

                        agentFocus = agents[0];

                        break;

                    case FocusModes.Last:

                        agentFocus = agents[agents.Count];

                        break;
                }
            }
        }

        public void FocusOnPlayer(FocusModes focusMode)
        {
            if (players.Count > 0)
            {
                switch (focusMode)
                {
                    case FocusModes.Nearest:

                        float nearestDistance = Mathf.Infinity;
                        for (int i = players.Count - 1; i >= 0; i--)
                        {
                            if (players[i] != null)
                            {
                                currentDistance = Vector3.Distance(transform.position, players[i].transform.position);
                                if (currentDistance <= nearestDistance)
                                {
                                    playerFocus = players[i];
                                    nearestDistance = currentDistance;
                                }
                            }
                            else
                            {
                                players.RemoveAt(i);
                            }
                        }

                        break;

                    case FocusModes.Farthest:

                        float farthestDistance = -Mathf.Infinity;
                        for (int i = players.Count - 1; i >= 0; i--)
                        {
                            if (players[i] != null)
                            {
                                currentDistance = Vector3.Distance(transform.position, players[i].transform.position);
                                if (currentDistance >= farthestDistance)
                                {
                                    playerFocus = players[i];
                                    farthestDistance = currentDistance;
                                }
                            }
                            else
                            {
                                players.RemoveAt(i);
                            }
                        }

                        break;

                    case FocusModes.Random:

                        playerFocus = players[Random.Range(0, players.Count)];

                        break;

                    case FocusModes.First:

                        playerFocus = players[0];

                        break;

                    case FocusModes.Last:

                        playerFocus = players[players.Count];

                        break;
                }
            }
        }

        public void AddObjectToMemory(GameObject go)
        {
            if (!objects.Contains(go) && go != gameObject)
            {
                objects.Add(go);
            }
        }

        public void AddAgentToMemory(AIAgent agent)
        {
            if (!agents.Contains(agent))
            {
                agents.Add(agent);
            }
        }

        /// <summary>
        /// Finds all <see cref="AIAgent"/>s in <see cref="objects"/> and moves them to <see cref="agents"/>.
        /// </summary>
        public void FindAgentsInEnvironmentMemory()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] != null)
                {
                    AIAgent agent = objects[i].GetComponent<AIAgent>();
                    if (agent != null && !agents.Contains(agent))
                    {
                        agents.Add(agent);
                    }
                    objects.RemoveAt(i);
                }
                else
                {
                    objects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Finds all GameObjects tagged as a player in <see cref="objects"/> and moves them to <see cref="players"/>.
        /// </summary>
        public void FindPlayersInEnvironmentMemory()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] != null)
                {
                    if(objects[i].CompareTag(playerTag) == true && players.Contains(objects[i]))
                    {
                        players.Add(objects[i]);
                    }
                    objects.RemoveAt(i);
                }
                else
                {
                    objects.RemoveAt(i);
                }
            }
        }

        #endregion
    }

    public enum FocusModes { Nearest, Farthest, Random, First, Last }
}