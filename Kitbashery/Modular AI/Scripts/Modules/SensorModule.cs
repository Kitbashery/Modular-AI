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
    /// This module defines actions and conditions for a <see cref="MAI_Agent"/> to be able to detect <see cref="GameObject"/>s in the environment.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/modular-ai/sensor-module.html")]
    [AddComponentMenu("Kitbashery/AI/AI Modules/Sensor Module")]
    [DisallowMultipleComponent]
    public class SensorModule : AIModule
    {
        #region Properties:

        /// <summary>
        /// Toggles debug mode for displaying gizmos when the agent is selected.
        /// </summary>
        [Tooltip("Toggles debug mode for displaying gizmos when the agent is selected.")]
        public bool debugMode = false;

        [Header("References:")]
        /// <summary>
        /// Where to start the ray from when scanning via raycasts.
        /// </summary>
        [Tooltip("Where to start the ray from when scanning via raycasts.")]
        public Transform eyes;

        public MemoryModule memory;

        [Header("Scan Settings:")]
        /// <summary>
        /// Layers to scan for objects on.
        /// </summary>
        [Tooltip("Layers to scan for objects on.")]
        public LayerMask layerMask = 1;

        /// <summary>
        /// Should trigger colliders be ignored?
        /// </summary>
        [Tooltip("Should trigger colliders be ignored? See QueryTriggerInteraction in the Unity Manual.")]
        public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        public enum SensorTypes { sphere, box, ray, _2D_circle, _2D_box, _2D_line}
        public SensorTypes sensorType = SensorTypes.sphere;

        public enum ScanTypes { environment, players, agents, environmentFiltered }

        [Space]
        [Tooltip("The scan range or bounds of the AI's sensor; A minium of twice the NavMeshAgent's height is recommended.")]
        public float scanRange = 4;

        /// <summary>
        /// The scan range the agent started at.
        /// </summary>
        private float originalScanRange = 4;

        /// <summary>
        /// Determines the delay between scans.
        /// </summary>
        [Tooltip("Determines the delay between scans.")]
        public float scanInterval = 0;
        private float timeSinceLastScan = 0;
        private float originalScanInterval = 0;

        /// <summary>
        /// Scans of the environment will only add GameObjects with these tags to memory.
        /// </summary>
        [Tooltip("Filtered environment scans will only add GameObjects with these tags to memory.")]
        public List<string> searchFilterTags = new List<string>();

        /// <summary>
        /// Determines if any existing memory should be cleared before each new scan. (may increase performance, but be careful that clearing memory doesn't impact gameplay).
        /// </summary>
        [Tooltip("Determines if any existing memory should be cleared before each new scan. (may increase performance).")]
        public bool clearOldMemory = false;

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
                    _conditions = new string[1] { "scan range is reduced" };
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
            switch(conditionIndex)
            {
                case 0:

                    return scanRange != originalScanRange;
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
                    _actions = new string[13] { "scan scene without filter", "scan scene with filter", "scan for agents", "scan for players", "clear search filter", "shrink scan range by half", "shrink scan range by 1/3rd", "shrink scan range by 1/4th", "reset scan range", "2x scan interval", "3x scan interval", "4x scan interval", "reset scan interval" };
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

                    Scan(ScanTypes.environment);

                    break;

                case 1:

                    Scan(ScanTypes.environmentFiltered);

                    break;

                case 2:

                    Scan(ScanTypes.agents);

                    break;

                case 3:

                    Scan(ScanTypes.players);

                    break;

                case 4:

                    searchFilterTags.Clear();

                    break;

                case 5:

                    scanRange = originalScanRange / 2;

                    break;

                case 6:

                    scanRange = originalScanRange / 3;

                    break;

                case 7:

                    scanRange = originalScanRange / 4;

                    break;

                case 8:

                    scanRange = originalScanRange;

                    break;

                case 9:

                    scanInterval *= 2;

                    break;

                case 10:

                    scanInterval *= 3;

                    break;

                case 11:

                    scanInterval *= 4;

                    break;

                case 12:

                    scanInterval = originalScanInterval;

                    break;
            }
        }

        #endregion

        #endregion

        #region Initialization & Updates:

        // Start is called before the first frame update.
        void Start()
        {
            // Note: MAI_MemoryModule is not a required component since that would add an unused component to the agent if the users intent is to
            // have a seperate agent store memories for all other agents. (if that is the case make sure clearMemoryOnScan is off for all scanning agents).
            // Not having a MAI_MemoryModule assigned will cause this script to needlessly impact performance and potentially break gameplay; throw error:
            if (memory == null)
            {
                Debug.LogErrorFormat(gameObject, "|Modular AI|: SensorModule does not have a MemoryModule assigned, this agent will not remember what it scans!");
            }
            else if(memory.transform != transform && clearOldMemory == true)
            {
                Debug.LogWarningFormat(gameObject, "|Modular AI|: SensorModule has clearOldMemory set to 'True' but the variable 'memory' is assigned to a diffrent AIAgent. This could lead to unintended gameplay concequences.");
            }

            if(eyes == null)
            {
                eyes = transform;
            }

            originalScanRange = scanRange;
            originalScanInterval = scanInterval;
        }

        // Update is called every frame, if the MonoBehaviour is enabled.
        private void Update()
        {
            timeSinceLastScan += Time.deltaTime;
        }

        private void OnDrawGizmosSelected()
        {
            if (debugMode == true)
            {
                Gizmos.color = Color.green;
                switch (sensorType)
                {
                    case SensorTypes.box:

                        Gizmos.DrawWireCube(gameObject.transform.position, Vector3.one * scanRange);

                        break;

                    case SensorTypes.ray:

                        if(eyes == null)
                        {
                            Gizmos.DrawLine(transform.position, transform.position + (Vector3.forward * scanRange));
                        }
                        else
                        {
                            Gizmos.DrawLine(eyes.position, eyes.position + (Vector3.forward * scanRange));
                        }

                        break;

                    case SensorTypes.sphere:

                        Gizmos.DrawWireSphere(gameObject.transform.position, scanRange);

                        break;

                    case SensorTypes._2D_box:

                        Gizmos.DrawWireCube(gameObject.transform.position, Vector3.one * scanRange);

                        break;

                    case SensorTypes._2D_circle:

#if UNITY_EDITOR
                        UnityEditor.Handles.color = Color.green;
                        UnityEditor.Handles.DrawWireDisc(gameObject.transform.position, Vector3.back, scanRange);
#endif

                        break;

                    case SensorTypes._2D_line:

                        if (eyes == null)
                        {
                            Gizmos.DrawLine(transform.position, transform.position + (Vector3.right * scanRange));
                        }
                        else
                        {
                            Gizmos.DrawLine(eyes.position, eyes.position + (Vector3.right * scanRange));
                        }

                        break;
                }
            }
        }

        #endregion

        #region Methods:

        /// <summary>
        /// Scans for <see cref="GameObject"/>s in the scene.
        /// </summary>
        public void Scan(ScanTypes scanType)
        {
            if(timeSinceLastScan >= scanInterval)
            {
                if (clearOldMemory == true)
                {
                    if ((scanType == ScanTypes.environment || scanType == ScanTypes.environmentFiltered) && memory.objects.Count > 0)
                    {
                        memory.objects.Clear();
                    }
                    else if (scanType == ScanTypes.agents && memory.agents.Count > 0)
                    {
                        memory.agents.Clear();
                    }
                    else if (scanType == ScanTypes.players && memory.players.Count > 0)
                    {
                        memory.players.Clear();
                    }
                }

                switch (sensorType)
                {
                    case SensorTypes.box:

                        foreach (Collider col in Physics.OverlapBox(gameObject.transform.position, Vector3.one * scanRange, Quaternion.identity, layerMask, triggerInteraction))
                        {
                            FilterScanResults(scanType, col.gameObject);
                        }

                        break;

                    case SensorTypes.ray:

                        RaycastHit[] hits = Physics.RaycastAll(eyes.position, Vector3.forward, scanRange, layerMask, triggerInteraction);
                        foreach (RaycastHit hit in hits)
                        {
                            FilterScanResults(scanType, hit.collider.gameObject);
                        }

                        break;

                    case SensorTypes.sphere:

                        foreach (Collider col in Physics.OverlapSphere(gameObject.transform.position, scanRange, layerMask, triggerInteraction))
                        {
                            FilterScanResults(scanType, col.gameObject);
                        }

                        break;

                    case SensorTypes._2D_box:

                        foreach (Collider2D col in Physics2D.OverlapBoxAll(gameObject.transform.position, Vector2.one * scanRange, 0f, layerMask))
                        {
                            FilterScanResults(scanType, col.gameObject);
                        }

                        break;

                    case SensorTypes._2D_circle:

                        foreach (Collider2D col in Physics2D.OverlapCircleAll(gameObject.transform.position, scanRange, layerMask))
                        {
                            FilterScanResults(scanType, col.gameObject);
                        }

                        break;

                    case SensorTypes._2D_line:

                        RaycastHit2D[] hits2d = Physics2D.RaycastAll(eyes.position, Vector2.right, scanRange, layerMask);
                        foreach (RaycastHit2D hit in hits2d)
                        {
                            FilterScanResults(scanType, hit.collider.gameObject);
                        }

                        break;
                }

                timeSinceLastScan = 0;
            }
        }

        private void FilterScanResults(ScanTypes scanType, GameObject go)
        {
            switch(scanType)
            {
                case ScanTypes.environment:

                    if(!memory.objects.Contains(go))
                    {
                        memory.AddObjectToMemory(go);
                    }

                    break;

                case ScanTypes.players:

                    if (go.CompareTag(memory.playerTag) == true && !memory.players.Contains(go))
                    {
                        memory.players.Add(go);
                    }

                    break;

                case ScanTypes.agents:

                    if (go != gameObject && go.CompareTag(memory.agentTag) == true)
                    {
                        if(go.TryGetComponent(out AIAgent agent) && !memory.agents.Contains(agent))
                        {
                            memory.agents.Add(agent);
                        }
                    }

                    break;

                case ScanTypes.environmentFiltered:

                    if(searchFilterTags.Count > 0)
                    {
                        if(searchFilterTags.Contains(go.tag) && !memory.objects.Contains(go))
                        {
                            memory.objects.Add(go);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat(gameObject, "|Modular AI|: SensorModule tried to do a filtered scan of the environment but the search filter was empty!");
                    }

                    break;
            }
        }

        #endregion
    }
}