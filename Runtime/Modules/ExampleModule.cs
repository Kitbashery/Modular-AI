using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kitbashery.AI
{
    [HelpURL("https://kitbashery.com/docs/modular-ai/example-module.html")]
    [DisallowMultipleComponent]
    [AddComponentMenu("Kitbashery/AI/AI Modules/ExampleModule.cs")]
    public class ExampleModule : AIModule
    {
        #region Properties:

        [Header("ExampleModule.cs is a template script for programming reference.")]
        [Header("Example Inpector Header")]
        [Tooltip("Example Inspector Tooltip")]
        public int exampleVariable;

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
                    _conditions = new string[3] { "example condition 1", "example condition 2", "example condition 3" };
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

                    // This is an example of returning a statement.
                    return transform.position == Vector3.zero;

                case 1:

                    // This is an example of how you could return a more complex statement.
                    if (transform.position == Vector3.zero && (transform.position.y > 1 || transform.position.y < 0))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case 2:

                    // This is an example of how to return a boolean value from a method (useful for managing more complex code such as loops).
                    return ConditionExample3();

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
                    _actions = new string[2] { "do something", "do another thing" };
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

                    // Do someting.

                    break;

                case 1:

                    // Do another thing.

                    break;
            }
        }

        #endregion

        #region Initialization & Updates:

        // All standard Monobehaviour updates can go here such as Start() and Update().

        #endregion

        #region Methods:

        public bool ConditionExample3()
        {
            for(int i = 0; i < 10; i++)
            {
                if(i > 5)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}