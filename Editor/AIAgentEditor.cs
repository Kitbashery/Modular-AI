using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    [CustomEditor(typeof(AIAgent)), CanEditMultipleObjects]
    public class AIAgentEditor : Editor
    {
        #region Properties:

        /// <summary>
        /// The <see cref="AIAgent"/> being inspected.
        /// </summary>
        public AIAgent self;

        private int selectedModule = 0;

        private int selectedCondition = 0;

        private int selectedAction = 0;

        private int newConditionValue = 1;

        private bool newConditionState = true;

        private bool showConditions, showActions, showEvents, showBehaviourHelp;

        private bool addingCondition = false;

        private bool addingAction = false;

        private string[] moduleNames = new string[0];

        private int previousPage = 0;

        private int pagination = 1;

        private bool renaming = false;

        SerializedProperty behaviours, preActionExecution, postActionExecution, debugMode, debugLevel, scoreType, scoreThreshold, useCompetingBehaviours;

        const string nameStr = "name", actionsStr = "actions", conditionsStr = "conditions", scoreStr = "score", boxStr = "box";

        #endregion

        #region Initialization & Updates:

        private void OnEnable()
        {
            behaviours = serializedObject.FindProperty("behaviours");
            debugMode = serializedObject.FindProperty("debugMode");
            debugLevel = serializedObject.FindProperty("debugLevel");
            preActionExecution = serializedObject.FindProperty("preActionExecution");
            postActionExecution = serializedObject.FindProperty("postActionExecution");
            scoreType = serializedObject.FindProperty("scoreType");
            scoreThreshold = serializedObject.FindProperty("scoreThreshold");
            useCompetingBehaviours = serializedObject.FindProperty("useCompetingBehaviours");

            RefreshModules();
        }

        public override void OnInspectorGUI()
        {
            // Get the agent we have selected.
            self = (AIAgent)target;

            serializedObject.Update();

            // Refresh modules if needed.
            self.CheckForModuleChanges();
            if (EditorApplication.isCompiling == true || self.modulesChanged == true)
            {
                RefreshModules();
            }

            if (self.modules.Length == 0 || self.modules == null)
            {
                if (self.behaviours.Count > 0)
                {
                    // Check if there is behaviour logic but all the modules were removed:
                    foreach (AIBehaviour b in self.behaviours)
                    {
                        if (b.actions.Count >= 1 || b.conditions.Count >= 1)
                        {
                            self.hasBrokenReferences = true;
                        }
                    }
                    DrawBrokenReferenceNotice();
                }

                if (self.hasBrokenReferences == false)
                {
                    EditorGUILayout.HelpBox("To begin add a module by clicking the " + @"""" + "Add Component" + @"""" + " button. Look for components under Kitbashery/AI/Modules.", MessageType.Info);
                }
            }
            else
            {
                DrawModules();
                EditorGUILayout.Space();

                // Draw behaviours:
                if (self.behaviours.Count > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawPagination();
                    DrawBehaviourOptions(behaviours.GetArrayElementAtIndex(pagination - 1));
                    EditorGUILayout.Space();
                    DrawBehaviourList(behaviours, pagination - 1);
                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    self.behaviours.Add(new AIBehaviour("New Behaviour", new List<BehaviourEvent>(), new List<BehaviourEvent>()));
                    pagination = self.behaviours.Count;
                }

                EditorGUILayout.Space();
                DrawSettings();
            }

            if (serializedObject.hasModifiedProperties == true)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Methods:

        private void RefreshModules()
        {
            addingCondition = false;
            addingAction = false;
            selectedModule = 0;
            selectedAction = 0;
            selectedCondition = 0;
            if(self == null)
            {
                self = (AIAgent)target;
            }
            self.modules = self.gameObject.GetComponents<AIModule>();
            moduleNames = new string[self.modules.Length];
            for (int i = 0; i < self.modules.Length; i++)
            {
                moduleNames[i] = self.modules[i].GetType().Name;
            }
            self.modulesChanged = false;
        }

        private void DrawModules()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Modules:", MAI_EditorUtility.lowerLeftBoldLabel);
            DrawModuleRefreshButton();
            EditorGUILayout.EndHorizontal();
            GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);
            for (int m = self.modules.Length - 1; m >= 0; m--)
            {
                if (self.modules[m] == null)
                {
                    RefreshModules();
                }
                self.modules[m].hideFlags = HideFlags.None;
                EditorGUILayout.BeginHorizontal();
                self.modules[m].foldoutToggled = MAI_EditorUtility.DrawFoldout(self.modules[m].foldoutToggled, self.modules[m].GetType().Name);
                MAI_EditorUtility.DrawComponentOptions(self.modules[m]);
                if (GUILayout.Button("  X", MAI_EditorUtility.centeredBoldHelpBox, GUILayout.Width(24), GUILayout.Height(24)))
                {
                    EditorApplication.Beep();
                    self.modules[m].removingModule = true;
                }
                EditorGUILayout.EndHorizontal();

                if (self.modules[m].removingModule == true)
                {
                    EditorGUILayout.HelpBox("WARNING! Removing a module that is used in any behaviours will break defined logic!", MessageType.Warning);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("!! Remove Module !!", EditorStyles.miniButton))
                    {
                        DestroyImmediate(self.modules[m]);
                        RefreshModules();
                        break;
                    }
                    if (GUILayout.Button("CANCEL", EditorStyles.miniButton))
                    {
                        self.modules[m].removingModule = false;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (self.modules[m].foldoutToggled)
                {
                    // Collapse other open module foldouts:
                    foreach (AIModule module in self.modules)
                    {
                        if (module != self.modules[m] && module.foldoutToggled == true)
                        {
                            module.foldoutToggled = false;
                        }
                    }

                    // Draw module inspector:

                    EditorGUILayout.BeginVertical(boxStr);
                    // Note: Might be able to hide the script ref by iterating through each property with .Next() and comparing
                    // if(m_Script" == property.propertyPath) and ignoring it.
                    EditorGUI.indentLevel++;
                    CreateEditor(self.modules[m]).OnInspectorGUI();
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();
                }
                self.modules[m].hideFlags = HideFlags.HideInInspector;
                if (m < self.modules.Length)
                {
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawModuleRefreshButton()
        {
            /*GUIContent content = EditorGUIUtility.IconContent("Refresh");
            content.tooltip = "Refesh Modules";
            if (GUILayout.Button(content, EditorStyles.helpBox, GUILayout.Width(24), GUILayout.Height(24)))
            {
                RefreshModules();
                Debug.Log("|MAI|: Modules refreshed.");
            }*/
        }

        private void DrawPagination()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Behaviours:", MAI_EditorUtility.clippingBoldLabel, GUILayout.Width(65));
            EditorGUILayout.Space();
            previousPage = pagination;
            if (GUILayout.Button(new GUIContent("←", "Back"), EditorStyles.miniButton, GUILayout.Width(23)))
            {
                pagination--;
            }
            pagination = EditorGUILayout.IntField(pagination, GUILayout.Width(25));
            EditorGUILayout.LabelField("/ " + self.behaviours.Count, EditorStyles.whiteLabel, GUILayout.Width(22));
            if (GUILayout.Button(new GUIContent("→", "Next"), EditorStyles.miniButtonLeft, GUILayout.Width(23)))
            {
                pagination++;
            }
            if (GUILayout.Button(new GUIContent("+", "Add Behaviour"), EditorStyles.miniButtonRight, GUILayout.Width(22)))
            {
                behaviours.InsertArrayElementAtIndex(behaviours.arraySize);
                behaviours.GetArrayElementAtIndex(behaviours.arraySize - 1).FindPropertyRelative(nameStr).stringValue = "New Behaviour " + (behaviours.arraySize);
                behaviours.GetArrayElementAtIndex(behaviours.arraySize - 1).FindPropertyRelative(actionsStr).ClearArray();
                behaviours.GetArrayElementAtIndex(behaviours.arraySize - 1).FindPropertyRelative(conditionsStr).ClearArray();
                behaviours.GetArrayElementAtIndex(behaviours.arraySize - 1).FindPropertyRelative("threshold").intValue = 0;
                pagination = behaviours.arraySize;
            }
            /*
             * Duplicate behaviour button (commented out since elements weren't moving as expected).
             * (If re-enabled be sure to change the add behaviour button's style to miniButtonMid).
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_winbtn_win_restore", "Duplicate Behaviour"), EditorStyles.miniButtonRight, GUILayout.Width(20)))
            {
                int originalIndex = pagination - 1;
                //Move the current behaviour to the end:
                behaviours.GetArrayElementAtIndex(originalIndex).MoveArrayElement(originalIndex, behaviours.arraySize);
                //Insert new at the end (will copy what is currently at the end).
                behaviours.InsertArrayElementAtIndex(behaviours.arraySize);
                //Move the original back to where it was in the array.
                behaviours.GetArrayElementAtIndex(behaviours.arraySize - 1).MoveArrayElement(behaviours.arraySize - 1, originalIndex + 1);
                //Change the name of the duplicated behaviour (it should be at the end).
                behaviours.GetArrayElementAtIndex(behaviours.arraySize - 1).FindPropertyRelative(nameStr).stringValue = behaviours.GetArrayElementAtIndex(behaviours.arraySize -1).FindPropertyRelative(nameStr).stringValue + " (copy)";
                pagination = behaviours.arraySize;
            }
            */
            if (pagination > behaviours.arraySize)
            {
                pagination = behaviours.arraySize;
            }
            if (pagination < 1)
            {
                pagination = 1;
            }
            if (pagination != previousPage)
            {
                addingAction = false;
                addingCondition = false;
                renaming = false;
            }
            EditorGUILayout.Space();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow"), GUIStyle.none, GUILayout.Width(20))) { showBehaviourHelp = !showBehaviourHelp; }
            EditorGUILayout.EndHorizontal();

            if (showBehaviourHelp == true)
            {
                showActions = false;
                showConditions = false;
                showEvents = false;
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow@2x"), GUIStyle.none);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Behaviours:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Sets of logic that compete to have the best score as determined by the score type.", MAI_EditorUtility.wrappedMiniLabel);
                GUILayout.Box(string.Empty, MAI_EditorUtility.horizontalLine);
                EditorGUILayout.LabelField("Conditions:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("A conditional statement that adds a defined score to a behaviour's total score if the statment meets it's target state.", MAI_EditorUtility.wrappedMiniLabel);
                GUILayout.Box(string.Empty, MAI_EditorUtility.horizontalLine);
                EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Code that executes when a behaviour's score wins over all other behaviour scores.", MAI_EditorUtility.wrappedMiniLabel);
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawBehaviourOptions(SerializedProperty behaviour)
        {
            GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);
            if (renaming == true)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(behaviour.FindPropertyRelative(nameStr), MAI_EditorUtility.blankLabel);
                if (GUILayout.Button("Apply", EditorStyles.miniButton))
                {
                    renaming = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                if (renaming == false && GUILayout.Button(new GUIContent(behaviour.FindPropertyRelative(nameStr).stringValue, "Click to rename."), MAI_EditorUtility.centeredLabel, GUILayout.ExpandWidth(true)))
                {
                    renaming = true;
                }
                if (behaviours.arraySize > 1)
                {
                    if (GUILayout.Button(new GUIContent(string.Empty, "Remove Behaviour"), "OL Minus", GUILayout.Width(20)))
                    {
                        behaviours.DeleteArrayElementAtIndex(pagination - 1);
                        if (pagination > 1)
                        {
                            pagination--;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Box(string.Empty, MAI_EditorUtility.horizontalLine);
            DrawBrokenReferenceNotice();
        }

        private void DrawBehaviourList(SerializedProperty list, int page)
        {
            DrawEventList(list.GetArrayElementAtIndex(page).FindPropertyRelative(conditionsStr), ref showConditions, "Conditions", true);
            EditorGUILayout.Space();
            DrawEventList(list.GetArrayElementAtIndex(page).FindPropertyRelative(actionsStr), ref showActions, "Actions", false);
            EditorGUILayout.Space();
            if(useCompetingBehaviours.boolValue == false)
            {
                list.GetArrayElementAtIndex(page).FindPropertyRelative("threshold").intValue = EditorGUILayout.IntField(new GUIContent("Score Threshold", "The value conditions need to meet or exceed for the behaviour's actions to execute."), list.GetArrayElementAtIndex(page).FindPropertyRelative("threshold").intValue);
            }
        }

        private void DrawEventList(SerializedProperty list, ref bool fold, string label, bool isCondition)
        {
            fold = MAI_EditorUtility.DrawFoldout(fold, label);
            if (fold == true)
            {
                // Collapse open foldouts:
                if (showActions == true || showConditions == true)
                {
                    if(isCondition == true)
                    {
                        showActions = false;
                    }
                    else
                    {
                        showConditions = false;
                    }
                    showEvents = false;
                }

                if (list.FindPropertyRelative("Array.size").hasMultipleDifferentValues == false)
                {
                    EditorGUILayout.BeginVertical(boxStr);
                    if (list.arraySize > 0)
                    {
                        for (int i = list.arraySize - 1; i >= 0; i--)
                        {
                            SerializedProperty element = list.GetArrayElementAtIndex(i);
                            GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);
                            if (element.FindPropertyRelative("instance").objectReferenceValue != null)
                            {
                                EditorGUILayout.BeginHorizontal();

                                if (element.FindPropertyRelative("isCondition").boolValue == true)
                                {
                                    //Draw condition:
                                    EditorGUILayout.BeginVertical(EditorStyles.miniButtonLeft, GUILayout.Width(20));
                                    element.FindPropertyRelative(scoreStr).intValue = Mathf.Clamp(EditorGUILayout.IntField(element.FindPropertyRelative(scoreStr).intValue, MAI_EditorUtility.centeredMiniLabel, GUILayout.Width(20)), 1, 999);
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.BeginVertical(EditorStyles.miniButtonMid);
                                    GUILayout.Label(element.FindPropertyRelative(nameStr).stringValue, MAI_EditorUtility.centeredMiniLabel);
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.BeginVertical(EditorStyles.miniButtonRight, GUILayout.Width(30));
                                    SerializedProperty state = element.FindPropertyRelative("state");
                                    if (GUILayout.Button(state.boolValue.ToString(), MAI_EditorUtility.centeredMiniLabel))
                                    {
                                        state.boolValue = !state.boolValue;
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                                else
                                {
                                    //Draw action:
                                    EditorGUILayout.BeginHorizontal();

                                    EditorGUILayout.LabelField((list.arraySize - i).ToString(), MAI_EditorUtility.centeredMiniLabel, GUILayout.Width(20), GUILayout.ExpandWidth(false));
                                    if (list.arraySize > 1)
                                    {
                                        int oldIndex = i;
                                        if (i == list.arraySize - 1)
                                        {
                                            if (GUILayout.Button("↓", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
                                            {
                                                // Move action down.
                                                list.MoveArrayElement(i, oldIndex - 1);
                                            }
                                        }
                                        else if (i == 0)
                                        {
                                            if (GUILayout.Button("↑", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
                                            {
                                                // Move action up.
                                                list.MoveArrayElement(i, oldIndex + 1);
                                            }
                                        }
                                        else if (i > 0 && i < list.arraySize)
                                        {
                                            if (GUILayout.Button("↓", EditorStyles.miniButtonLeft, GUILayout.Width(25)))
                                            {
                                                // Move action down.
                                                list.MoveArrayElement(i, oldIndex - 1);
                                            }
                                            if (GUILayout.Button("↑", EditorStyles.miniButtonMid, GUILayout.Width(25)))
                                            {
                                                // Move action up.
                                                list.MoveArrayElement(i, oldIndex + 1);
                                            }
                                        }
                                    }
                                    if (list.arraySize == 1)
                                    {
                                        EditorGUILayout.BeginHorizontal(EditorStyles.miniButton);
                                    }
                                    else
                                    {
                                        EditorGUILayout.BeginHorizontal(EditorStyles.miniButtonRight);
                                    }
                                    EditorGUILayout.LabelField(element.FindPropertyRelative(nameStr).stringValue, MAI_EditorUtility.upperLeftMiniLabel, GUILayout.MaxWidth(120));
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.EndHorizontal();
                                }

                                if (GUILayout.Button(string.Empty, "OL Minus", GUILayout.Width(25)))
                                {
                                    list.DeleteArrayElementAtIndex(i);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Missing module for " + element.FindPropertyRelative(nameStr).stringValue, MessageType.Warning);
                                EditorGUILayout.LabelField("Class name changed? Update it below then attempt repair:");
                                EditorGUILayout.PropertyField(element.FindPropertyRelative("moduleName"), MAI_EditorUtility.blankLabel);
                                EditorGUILayout.LabelField("OR");
                                if (GUILayout.Button("Remove Condition", EditorStyles.miniButton))
                                {
                                    list.DeleteArrayElementAtIndex(i);
                                }

                                self.hasBrokenReferences = true;
                            }
                        }
                    }

                    EditorGUILayout.Space();
                    DrawEventCreationGUI(list, isCondition);

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    if(isCondition == true)
                    {
                        EditorGUILayout.HelpBox("Multi-object editing Not supported. Make sure the selected AI Agents have an equal amount of conditions on the behaviour.", MessageType.None);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Multi-object editing Not supported. Make sure the selected AI Agents have an equal amount of actions on the behaviour.", MessageType.None);
                    }

                }
            }
        }

        private void DrawEventCreationGUI(SerializedProperty list, bool isCondition)
        {
            if (list != null)
            {
                if (self.modules.Length > 0 && moduleNames.Length == 0)
                {
                    RefreshModules();
                }

                if (isCondition == true)
                {
                    if (addingCondition == false)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("Add Condition:", MAI_EditorUtility.centeredBoldLabel);
                        if (GUILayout.Button(string.Empty, "OL Plus", GUILayout.Width(20)))
                        {
                            addingCondition = true;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.LabelField("New Condition ", MAI_EditorUtility.centeredBoldLabel);
                        GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);

                        // Begin variable settings.
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(new GUIContent("Score:", "The influence of the condition toward the AI's motivation of acting on the behaviour."), GUILayout.MaxWidth(70));
                        newConditionValue = EditorGUILayout.IntSlider(newConditionValue, 1, 999);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);
                        newConditionState = EditorGUILayout.Toggle(new GUIContent("State: [" + newConditionState.ToString() + "]", "The required state for the condition's score to count."), newConditionState);

                        GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);

                        selectedModule = MAI_EditorUtility.DrawCompactPopup("Module:", selectedModule, moduleNames);
                        selectedCondition = MAI_EditorUtility.DrawCompactPopup("Conditions:", selectedCondition, self.modules[selectedModule].conditions);

                        GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Add Condition", EditorStyles.miniButton))
                        {
                            if (self.modules[selectedModule].conditions.Length > 0)
                            {
                                list.InsertArrayElementAtIndex(0);
                                list.GetArrayElementAtIndex(0).FindPropertyRelative(nameStr).stringValue = self.modules[selectedModule].conditions[selectedCondition];
                                list.GetArrayElementAtIndex(0).FindPropertyRelative("id").intValue = selectedCondition;
                                list.GetArrayElementAtIndex(0).FindPropertyRelative("instance").objectReferenceValue = self.modules[selectedModule];
                                list.GetArrayElementAtIndex(0).FindPropertyRelative("moduleName").stringValue = self.modules[selectedModule].GetType().AssemblyQualifiedName;
                                list.GetArrayElementAtIndex(0).FindPropertyRelative(scoreStr).intValue = newConditionValue;
                                list.GetArrayElementAtIndex(0).FindPropertyRelative("isCondition").boolValue = true;
                                list.GetArrayElementAtIndex(0).FindPropertyRelative("state").boolValue = newConditionState;
                                addingCondition = false;
                            }
                            else
                            {
                                EditorApplication.Beep();
                            }
                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                        {
                            addingCondition = false;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    if (addingAction == false)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("Add Action:", MAI_EditorUtility.centeredBoldLabel);
                        if (GUILayout.Button(string.Empty, "OL Plus", GUILayout.Width(20)))
                        {
                            addingAction = true;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.LabelField("New Action ", MAI_EditorUtility.centeredBoldLabel);

                        GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);
                        selectedModule = MAI_EditorUtility.DrawCompactPopup("Module:", selectedModule, moduleNames);
                        selectedAction = MAI_EditorUtility.DrawCompactPopup("Actions:", selectedAction, self.modules[selectedModule].actions);


                        GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);
                        if (list.arraySize > 1)
                        {
                            EditorGUILayout.HelpBox("Note: actions are executed in the order they are arranged.", MessageType.None);
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Add Action", EditorStyles.miniButton))
                        {
                            if (self.modules[selectedModule].actions.Length > 0)
                            {
                                list.InsertArrayElementAtIndex(0);
                                list.GetArrayElementAtIndex(0).FindPropertyRelative(nameStr).stringValue = self.modules[selectedModule].actions[selectedAction];
                                list.GetArrayElementAtIndex(0).FindPropertyRelative("id").intValue = selectedAction;
                                list.GetArrayElementAtIndex(0).FindPropertyRelative("instance").objectReferenceValue = self.modules[selectedModule];
                                list.GetArrayElementAtIndex(0).FindPropertyRelative("moduleName").stringValue = self.modules[selectedModule].GetType().AssemblyQualifiedName;
                                addingAction = false;
                            }
                            else
                            {
                                EditorApplication.Beep();
                            }
                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                        {
                            addingAction = false;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                    }
                }
            }
            else
            {
                if (isCondition == true)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Add Condition:", MAI_EditorUtility.centeredBoldLabel);
                    if (GUILayout.Button(string.Empty, "OL Plus", GUILayout.Width(20)))
                    {
                        addingCondition = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }

            }

        }

        private void DrawSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Agent Settings:", EditorStyles.boldLabel);
            GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);

            if (EditorGUILayout.PropertyField(debugMode) == true)
            {
                EditorGUILayout.PropertyField(debugLevel);
            }

            EditorGUILayout.PropertyField(useCompetingBehaviours);
            if (useCompetingBehaviours.boolValue == true)
            {
                GUILayout.Box(string.Empty, MAI_EditorUtility.horizontalLine);
                EditorGUILayout.PropertyField(scoreType);
                if (scoreType.enumValueIndex == ((int)ScoreTypes.AllScoresAboveThreshold) || scoreType.enumValueIndex == ((int)ScoreTypes.FirstScoreAboveThreshold))
                {
                    EditorGUILayout.PropertyField(scoreThreshold);
                }
                GUILayout.Box(string.Empty, MAI_EditorUtility.horizontalLine);
            }

            EditorGUILayout.Space();
            showEvents = MAI_EditorUtility.DrawFoldout(showEvents, "Events:");
            if (showEvents)
            {
                // Collapse open foldouts:
                if (showActions == true || showConditions == true)
                {
                    showActions = false;
                    showConditions = false;
                }

                EditorGUILayout.BeginHorizontal(boxStr);
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Events to invoke prior to the execution of behaviour actions.", MessageType.None);
                EditorGUILayout.PropertyField(preActionExecution);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Events to invoke after the execution of behaviour actions.", MessageType.None);
                EditorGUILayout.PropertyField(postActionExecution);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
        }

        private void DrawBrokenReferenceNotice()
        {
            if (self.hasBrokenReferences == true)
            {
                EditorGUILayout.Space();
                GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);
                EditorGUILayout.HelpBox("Broken logic detected! A missing module could impact gameplay!", MessageType.Error);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Attempt Repair"))
                {
                    self.ValidateBehaviours();
                    self.FixBrokenReferences();
                    self.hasBrokenReferences = false;
                }
                if (GUILayout.Button("Dismiss"))
                {
                    self.hasBrokenReferences = false;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Box(string.Empty, MAI_EditorUtility.thickHorizontalLine);
            }
        }
        #endregion
    }
}
