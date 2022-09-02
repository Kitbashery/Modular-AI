using System;
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
    /// Represents an action or condition that will be executed or evaluated by <see cref="AIAgent"/>.
    /// </summary>
    [Serializable]
    public class BehaviourEvent : IComparable<BehaviourEvent>
    {
        #region Properties:

        /// <summary>
        /// The required module component instance that this event links to during runtime.
        /// </summary>
        //[SerializeReference]
        public AIModule instance;

        /// <summary>
        /// The assembly qualified name the required module instance.
        /// </summary>
        public string moduleName;

        /// <summary>
        /// ID is the index of either an action or condition.
        /// </summary>
        public int id;

        /// <summary>
        /// The name of the event.
        /// </summary>
        public string name;

        /// <summary>
        /// Does this event represent a condition?
        /// </summary>
        public bool isCondition;

        /// <summary>
        /// The score value of the event if the event represents a condition.
        /// </summary>
        public int score;

        /// <summary>
        /// The required state of the event if the event represents a condition.
        /// </summary>
        public bool state;

        #endregion

        /// <summary>
        /// Constructs an event as a condition.
        /// </summary>
        public BehaviourEvent(string eventName, int eventID, AIModule module, int conditionScore, bool conditionState)
        {
            instance = module;
            if(module != null)
            {
                moduleName = instance.GetType().AssemblyQualifiedName;
            }
            else
            {
                Debug.LogError("|Modular AI|: Tried to construct an event but the parameter module was null. This event wont track module dependencies. Remove the event and try again.");
            }

            id = eventID;
            name = eventName;
            isCondition = true;
            score = conditionScore;
            state = conditionState;
        }

        /// <summary>
        /// Constructs an event as an action.
        /// </summary>
        public BehaviourEvent(string eventName, int eventID, AIModule module)
        {
            instance = module;
            if (module != null)
            {
                moduleName = instance.GetType().AssemblyQualifiedName;
            }
            else
            {
                Debug.LogError("|Modular AI|: Tried to construct an event but the parameter module was null. This event wont track module dependencies. Remove the event and try again.");
            }

            id = eventID;
            name = eventName;
            isCondition = false;
            score = 0;
            state = false;
        }

        // Required by IComparable.
        public int CompareTo(BehaviourEvent other)
        {
            if (other == null)
            {
                return 1;
            }

            return 0;
        }
    }
}