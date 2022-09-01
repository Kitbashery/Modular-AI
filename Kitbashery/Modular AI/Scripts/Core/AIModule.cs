using System;
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
    /// A base class for all AI modules.
    /// </summary>
    [Serializable]
    [HelpURL("https://kitbashery.com/docs/modular-ai/ai-module.html")]
    [RequireComponent(typeof(AIAgent))]
    [AddComponentMenu("Kitbashery/AI/Modules/MAI_Module.cs")]
    public abstract class AIModule : MonoBehaviour
    {
        #region Properties:

        public abstract string[] conditions { get; }
        public abstract string[] actions { get; }

#if UNITY_EDITOR
        [HideInInspector]
        public bool foldoutToggled = false;
        [HideInInspector]
        public bool removingModule = false;
#endif

        #endregion

        #region Initialization & Updates:

        private void OnValidate()
        {
            hideFlags = HideFlags.HideInInspector;
        }

        #endregion

        #region Methods:

        /// <summary>
        /// Executes an action based on a string that should match an action's name in this module.
        /// </summary>
        /// <param name="actionName"></param>
        public abstract void executeAction(int actionIndex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditionName"></param>
        public abstract bool checkCondition(int conditionIndex);

        #endregion
    }
}
