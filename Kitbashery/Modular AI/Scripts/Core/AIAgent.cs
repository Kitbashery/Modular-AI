using System;
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
    /// Contains a list of behaviours that can be evaluated.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/modular-ai/ai-agent.html")]
    [Serializable]
    [DisallowMultipleComponent]
    [AddComponentMenu("Kitbashery/AI/AI Agent")]
    public class AIAgent : MonoBehaviour
    {
        #region Properties:

        public AIModule[] modules;

        private int componentCount = 0;

        /// <summary>
        /// Has the amount of modules changed?
        /// </summary>
        public bool modulesChanged = false;

        [SerializeField]
        public List<AIBehaviour> behaviours = new List<AIBehaviour>();

        /// <summary>
        /// Events to be invoked before any behaviour actions are executed when <see cref="ExecuteWinningBehaviourActions"/> is called.
        /// </summary>
        [SerializeField]
        public UnityEvent preActionExecution;

        /// <summary>
        /// Events to be invoked after any behaviour actions are executed when <see cref="ExecuteWinningBehaviourActions"/> is called.
        /// </summary>
        [SerializeField]
        public UnityEvent postActionExecution;

        [Tooltip("Toggles debug information in the console while in playmode.")]
        public bool debugMode = false;
        [Tooltip("How much information to log to the console while in debug mode.")]
        public DebugLevels debugLevel = DebugLevels.BehavioursOnly;

        [SerializeField]
        [Tooltip("The condition a behaviour's score needs to meet for its actions to execute.")]
        public ScoreTypes scoreType = ScoreTypes.HighestScoreWins;

        [SerializeField]
        [Tooltip("The score a behaviour will need to beat in order for its actions to be executed.")]
        public int scoreThreshold = 0;

        /// <summary>
        /// List of <see cref="AIBehaviour"/>s that meet the condition of <see cref="scoreType"/>.
        /// </summary>
        private List<AIBehaviour> winningBehaviours = new List<AIBehaviour>();

        /// <summary>
        /// The current score a behaviour needs to beat to win.
        /// </summary>
        private int scoreToBeat = 0;

        /// <summary>
        /// Has a module that this agent depends on been removed?
        /// </summary>
        [HideInInspector]
        public bool hasBrokenReferences = false;

        /// <summary>
        /// <see cref="BehaviourEvent"/>s that are missing their module instance references.
        /// </summary>
        [HideInInspector]
        private List<BehaviourEvent> brokenEvents = new List<BehaviourEvent>();

        #endregion

        #region Initialization & Updates:

        private void OnValidate()
        {
            CheckForModuleChanges();
        }

        private void Awake()
        {
            if (AIManager.Instance == null)
            {
                AIManager.Instance = new GameObject("AI Manager").AddComponent<AIManager>();
                Debug.LogWarning("AIManager instance not found, creating one...");
            }

            //Debug messages impact performance in builds, make sure this is off by default. If you're in the editor just switch it on when you need it.
            debugMode = false;
        }

        void OnEnable()
        {
            if (AIManager.Instance != null)
            {
                AIManager.Instance.Register(this);
            }

            ResetBehaviourEvaluation();
        }

        void OnDisable()
        {
            if (AIManager.Instance != null)
            {
                AIManager.Instance.Unregister(this);
            }
        }

        #endregion

        #region Methods:

        public void UpdateAI()
        {
            ResetBehaviourEvaluation();
            EvaluateBehaviours();
            ExecuteWinningBehaviourActions();
        }

        /// <summary>
        /// Resets the initial score to beat and clears previously winning behaviours.
        /// Note: Call this if you change <see cref="scoreType"/> during runtime.
        /// </summary>
        public void ResetBehaviourEvaluation()
        {
            if (scoreType == ScoreTypes.HighestScoreWins)
            {
                scoreToBeat = int.MinValue;
            }
            else if (scoreType == ScoreTypes.LowestScoreWins)
            {
                scoreToBeat = int.MaxValue;
            }

            winningBehaviours.Clear();
        }

        /// <summary>
        /// Evaluates behaviour logic to determine which behaviour's have their actions executed. 
        /// Note: <see cref="ResetBehaviourEvaluation"/> should be called before running this.
        /// </summary>
        private void EvaluateBehaviours()
        {
            for (int b = behaviours.Count - 1; b >= 0; b--)
            {
                int score = 0;
                for (int c = behaviours[b].conditions.Count - 1; c >= 0; c--)
                {
                    if (behaviours[b].conditions[c].instance != null)
                    {
                        if (behaviours[b].conditions[c].instance.checkCondition(behaviours[b].conditions[c].id) == behaviours[b].conditions[c].state)
                        {
                            score += behaviours[b].conditions[c].score;

                            if (debugMode == true && (debugLevel == DebugLevels.All || debugLevel == DebugLevels.ConditionsOnly))
                            {
                                Debug.LogFormat(gameObject, "|Modular AI|: Condition in behaviour: '{0}'  met its state and scored '{1}' total score is now {2}. ", behaviours[b].name, behaviours[b].conditions[c].score, score);
                            }

                            if (scoreType == ScoreTypes.FirstScoreWins)
                            {
                                if (score > 0)
                                {
                                    if (debugMode == true && (debugLevel == DebugLevels.All || debugLevel == DebugLevels.BehavioursOnly))
                                    {
                                        Debug.LogFormat(gameObject, "|Modular AI|: First behaviour to score above the threshold is: '{0}' This behaviour will win. | Score = {1}", behaviours[b].name, score);
                                    }
                                    winningBehaviours.Add(behaviours[b]);
                                    break;
                                }
                            }
                            else if (scoreType == ScoreTypes.FirstScoreAboveThreshold)
                            {
                                if (score > scoreThreshold)
                                {
                                    if (debugMode == true && (debugLevel == DebugLevels.All || debugLevel == DebugLevels.BehavioursOnly))
                                    {
                                        Debug.LogFormat(gameObject, "|Modular AI|: First behaviour to score above the threshold is: '{0}' This behaviour will win. | Score = {1}", behaviours[b].name, score);
                                    }
                                    winningBehaviours.Add(behaviours[b]);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat(gameObject, "|Modular AI|: Tried to check " + behaviours[b].conditions[c] + @"" + behaviours[b].conditions[c].name + @"" + " but a module was missing; ignoring condition.");
                    }
                }

                switch (scoreType)
                {
                    case ScoreTypes.AllScoresAboveThreshold:

                        if (score > scoreThreshold)
                        {
                            if (debugMode == true && (debugLevel == DebugLevels.All || debugLevel == DebugLevels.BehavioursOnly))
                            {
                                Debug.LogFormat(gameObject, "|Modular AI|: Behaviour '{0}' scored above the threshold and will have it's actions executed. Score = {1}", behaviours[b].name, score);
                            }
                            winningBehaviours.Add(behaviours[b]);
                        }

                        break;

                    case ScoreTypes.HighestScoreWins:

                        if (score > scoreToBeat)
                        {
                            if (debugMode == true && (debugLevel == DebugLevels.All || debugLevel == DebugLevels.BehavioursOnly))
                            {
                                Debug.LogFormat(gameObject, "|Modular AI|: Behaviour '{0}' scored above the current highest score and is now the new winner. Score = {1}", behaviours[b].name, score);
                            }
                            scoreToBeat = score;
                            winningBehaviours.Clear();
                            winningBehaviours.Add(behaviours[b]);
                        }

                        break;

                    case ScoreTypes.LowestScoreWins:

                        if (score < scoreToBeat)
                        {
                            if (debugMode == true && (debugLevel == DebugLevels.All || debugLevel == DebugLevels.BehavioursOnly))
                            {
                                Debug.LogFormat(gameObject, "|Modular AI|: Behaviour '{0}' scored below the current lowest score and is now the new winner. Score = {1}", behaviours[b].name, score);
                            }
                            scoreToBeat = score;
                            winningBehaviours.Clear();
                            winningBehaviours.Add(behaviours[b]);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Invokes pre-action events, then executes all actions in the order they were added in all behaviours in the order they won, then invokes post action events.
        /// Note: <see cref="EvaluateBehaviours"/> should be called before running this.
        /// </summary>
        private void ExecuteWinningBehaviourActions()
        {
            if (winningBehaviours.Count > 0)
            {
                preActionExecution.Invoke();
                foreach (AIBehaviour b in winningBehaviours)
                {
                    ExecuteBehaviourActions(b);
                }
                postActionExecution.Invoke();
            }
        }

        public void ExecuteBehaviourActions(AIBehaviour behaviour)
        {
            foreach (BehaviourEvent e in behaviour.actions)
            {
                e.instance.executeAction(e.id);
            }
        }

        public void AddNewEvent(int behaviourIndex, BehaviourEvent behaviourEvent)
        {
            if(behaviourEvent.isCondition == true)
            {
                behaviours[behaviourIndex].conditions.Add(behaviourEvent);
            }
            else
            {
                behaviours[behaviourIndex].actions.Add(behaviourEvent);
            }
        }

        /// <summary>
        /// Makes sure all components required by the behaviour logic has a component instance.
        /// </summary>
        public void ValidateBehaviours()
        {
            Dictionary<string, AIModule> currentModules = new Dictionary<string, AIModule>();
            modules = GetComponents<AIModule>();
            foreach (AIModule module in modules)
            {
                currentModules.Add(module.GetType().AssemblyQualifiedName, module);
            }

            if (currentModules.Count > 0)
            {
                foreach (AIBehaviour behaviour in behaviours)
                {
                    foreach (BehaviourEvent condition in behaviour.conditions)
                    {
                        if (condition.instance == null)
                        {
                            if (currentModules.ContainsKey(condition.moduleName) == false)
                            {
                                brokenEvents.Add(condition);
                                hasBrokenReferences = true;
                            }
                            else
                            {
                                condition.instance = currentModules[condition.moduleName];
                            }
                        }
                    }

                    foreach (BehaviourEvent action in behaviour.actions)
                    {
                        if (action.instance == null)
                        {
                            if (currentModules.ContainsKey(action.moduleName) == false)
                            {
                                brokenEvents.Add(action);
                                hasBrokenReferences = true;
                            }
                            else
                            {
                                action.instance = currentModules[action.moduleName];
                            }
                        }
                    }
                }
            }
        }

        public void FixBrokenReferences()
        {
            if (brokenEvents.Count > 0)
            {
                Dictionary<string, AIModule> modulesAdded = new Dictionary<string, AIModule>();
                foreach (BehaviourEvent behaviourEvent in brokenEvents)
                {
                    if (modulesAdded.ContainsKey(behaviourEvent.moduleName) == false)
                    {
                        modulesAdded.Add(behaviourEvent.moduleName, (AIModule)gameObject.AddComponent(Type.GetType(behaviourEvent.moduleName)));
                    }

                    if (modulesAdded[behaviourEvent.moduleName] != null)
                    {
                        behaviourEvent.instance = modulesAdded[behaviourEvent.moduleName];
                    }
                    else
                    {
                        Debug.LogWarningFormat(gameObject, "|Modular AI|: Behaviour event ({0})'s referenced a class that has changed names or no longer exists; it will not function.", behaviourEvent.name);
                    }

                }
                brokenEvents.Clear();
                hasBrokenReferences = false;
            }
        }

        public void CheckForModuleChanges()
        {
            int components = GetComponents<Component>().Length;
            if (components != componentCount)
            {
                ValidateBehaviours();
                componentCount = components;
                modulesChanged = true;
            }
        }

        #endregion
    }

    public enum DebugLevels { All, BehavioursOnly, ConditionsOnly }
    public enum ScoreTypes { AllScoresAboveThreshold, FirstScoreAboveThreshold, FirstScoreWins, HighestScoreWins, LowestScoreWins }
}
