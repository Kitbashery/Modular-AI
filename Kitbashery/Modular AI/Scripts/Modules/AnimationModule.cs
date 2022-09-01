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
    [HelpURL("https://kitbashery.com/docs/modular-ai/animation-module.html")]
    [DisallowMultipleComponent]
    [AddComponentMenu("Kitbashery/AI/AI Modules/Animation Module")]
    [RequireComponent(typeof(Animator))]
    public class AnimationModule : AIModule
    {
        #region Properties:

        public UnityEvent animationEvents;

        [Header("References:")]
        [Tooltip("The Animator component.")]
        public Animator anim;

        [Header("Movement State Names:")]
        public string idleState;
        public string walkState;
        public string runState;
        public string jumpState;

        [Header("Combat State Names:")]
        public string[] deathStates = new string[0];
        [HideInInspector]
        public int currentDeathState = -1;
        public string[] attackStates = new string[0];
        [HideInInspector]
        public int currentAttackState = -1;
        public string[] hitReactionStates = new string[0];
        [HideInInspector]
        public int currentReactionState = -1;

        public enum StateOptions { iteratively, randomly }

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
                    _conditions = new string[2] { "are feet stablized", "applying root motion" };
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

                    return anim.stabilizeFeet;

                case 1:

                    return anim.applyRootMotion;

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
                    _actions = new string[13] { "play idle anim", "play walk anim", "play run anim", "play jump anim", "play next / death anim", "play random / death anim", "play next / attack anim", "play random / attack anim", "play next / react anim", "play random / react anim", "toggle stableize feet", "toggle root motion", "invoke anim events" };
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

                    Idle();

                    break;

                case 1:

                    Walk();

                    break;

                case 3:

                    Run();

                    break;

                case 4:

                    Jump();

                    break;

                case 5:

                    Die(StateOptions.iteratively);

                    break;

                case 6:

                    Die(StateOptions.randomly);

                    break;

                case 7:

                    Attack(StateOptions.iteratively);

                    break;

                case 8:

                    Attack(StateOptions.randomly);

                    break;

                case 9:

                    HitReaction(StateOptions.iteratively);

                    break;

                case 10:

                    HitReaction(StateOptions.randomly);

                    break;

                case 11:

                    anim.stabilizeFeet = !anim.stabilizeFeet;

                    break;

                case 12:

                    if(anim.hasRootMotion == true)
                    {
                        anim.applyRootMotion = !anim.applyRootMotion;
                    }

                    break;

                case 13:

                    animationEvents.Invoke();

                    break;
            }
        }

        #endregion

        #region Initialization & Updates:

        public void Start()
        {
            if(anim == null)
            {
                Debug.LogWarning("|MAI|: Forgot to assign Animator to the variable anim on " + gameObject.name + "'s MAI_Animation component; auto assigning.");
                anim = GetComponent<Animator>();
            }
        }

        #endregion

        #region Methods:

        public void Idle()
        {
            anim.Play(idleState);
        }

        public void Walk()
        {
            anim.Play(walkState);
        }

        public void Run()
        {
            anim.Play(runState);
        }

        public void Jump()
        {
            anim.Play(jumpState);
        }

        public void Die(StateOptions option)
        {
            if(option == StateOptions.iteratively)
            {
                currentDeathState++;
                if(currentDeathState > deathStates.Length)
                {
                    currentDeathState = -1;
                }
                anim.Play(deathStates[currentDeathState]);
            }
            else if(option == StateOptions.randomly)
            {
                anim.Play(deathStates[Random.Range(0, deathStates.Length)]);
            }
        }

        public void Attack(StateOptions option)
        {
            if (option == StateOptions.iteratively)
            {
                currentAttackState++;
                if (currentAttackState > attackStates.Length)
                {
                    currentAttackState = -1;
                }
                anim.Play(attackStates[currentAttackState]);
            }
            else if (option == StateOptions.randomly)
            {
                anim.Play(attackStates[Random.Range(0, attackStates.Length)]);
            }
        }

        public void HitReaction(StateOptions option)
        {
            if (option == StateOptions.iteratively)
            {
                currentReactionState++;
                if (currentReactionState > hitReactionStates.Length)
                {
                    currentReactionState = -1;
                }
                anim.Play(hitReactionStates[currentReactionState]);
            }
            else if (option == StateOptions.randomly)
            {
                anim.Play(hitReactionStates[Random.Range(0, hitReactionStates.Length)]);
            }
        }

        #endregion
    }
}