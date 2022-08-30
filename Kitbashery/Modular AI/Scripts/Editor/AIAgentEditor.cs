﻿using System;
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
    [CustomEditor(typeof(AIAgent))]
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

        private AIBehaviour copied;

        private bool renaming = false;

        private bool needsRefresh = false;

        SerializedProperty behaviourList, preActionExecution, postActionExecution, debugMode, debugLevel, scoreType, scoreThreshold;

        #endregion

        #region Initialization & Updates:

        private void OnEnable()
        {
            behaviourList = serializedObject.FindProperty("behaviours");
            debugMode = serializedObject.FindProperty("debugMode");
            debugLevel = serializedObject.FindProperty("debugLevel");
            preActionExecution = serializedObject.FindProperty("preActionExecution");
            postActionExecution = serializedObject.FindProperty("postActionExecution");
            scoreType = serializedObject.FindProperty("scoreType");
            scoreThreshold = serializedObject.FindProperty("scoreThreshold");

            RefreshModules((AIAgent)target);
        }

        public override void OnInspectorGUI()
        {
            // Get the agent we have selected.
            self = (AIAgent)target;

            serializedObject.Update();

            // Check for script changes or component additions.
            // Note: It is expensive to call GetComponents, but only one agent can be inspected at a time so it's not so bad.
            if (EditorApplication.isCompiling == true || (self.GetComponents<AIModule>().Length > self.modules.Length))
            {
                // Note: Can't just call RefreshModules() here since it might not fully execute while the editor is compiling. (test this)?
                needsRefresh = true;
            }

            // Refresh modules if needed.
            if (needsRefresh == true)
            {
                RefreshModules(self);
                needsRefresh = false;
            }

            if (self.modules.Length == 0 || self.modules == null)
            {
                if (self.behaviours.Count > 0)
                {
                    // Check if there is behaviour logic but all the modules were removed:
                    foreach (AIBehaviour b in self.behaviours)
                    {
                        if (b.actions.Count > 0 || b.conditions.Count > 0)
                        {
                            self.hasBrokenReferences = true;
                        }
                    }
                    DrawBrokenReferenceNotice(self);
                }

                if (self.hasBrokenReferences == false)
                {
                    EditorGUILayout.HelpBox("To begin add a module by clicking the " + @"""" + "Add Component" + @"""" + " button. Look for components under Kitbashery/AI/Modules.", MessageType.Info);
                }
            }
            else
            {
                DrawModules(self);
                EditorGUILayout.Space();

                // Draw behaviours:
                if (self.behaviours.Count > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawPagination(self);
                    DrawBehaviourOptions(self);
                    EditorGUILayout.Space();
                    DrawConditionListGUI(self, pagination - 1);
                    EditorGUILayout.Space();
                    DrawActionListGUI(self, pagination - 1);
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

        public void RefreshModules(AIAgent self)
        {
            selectedModule = 0;
            selectedAction = 0;
            selectedCondition = 0;
            self.modules = self.gameObject.GetComponents<AIModule>();
            moduleNames = new string[self.modules.Length];
            for (int i = 0; i < self.modules.Length; i++)
            {
                moduleNames[i] = self.modules[i].GetType().Name;
            }
        }

        public void DrawModules(AIAgent self)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Modules:", MAI_EditorUtility.lowerLeftBoldLabel);
            //DrawModuleRefreshButton(self);
            //EditorGUILayout.EndHorizontal();
            GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
            for (int m = self.modules.Length - 1; m >= 0; m--)
            {
                if (self.modules[m] == null)
                {
                    RefreshModules(self);
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
                        RefreshModules(self);
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

                    EditorGUILayout.BeginVertical("box");
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

        public void DrawModuleRefreshButton(AIAgent self)
        {
            GUIContent content = EditorGUIUtility.IconContent("Refresh");
            content.tooltip = "Refesh Modules";
            if (GUILayout.Button(content, EditorStyles.helpBox, GUILayout.Width(24), GUILayout.Height(24)))
            {
                RefreshModules(self);
                Debug.Log("|MAI|: Modules refreshed.");
            }
        }

        public void DrawPagination(AIAgent self)
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
                self.behaviours.Add(new AIBehaviour("New Behaviour " + (self.behaviours.Count + 1), new List<BehaviourEvent>(), new List<BehaviourEvent>()));
                behaviourList.serializedObject.ApplyModifiedProperties();
                behaviourList.serializedObject.Update();
                pagination = self.behaviours.Count;
            }
            if (pagination > self.behaviours.Count)
            {
                pagination = self.behaviours.Count;
            }
            if (pagination < 1)
            {
                pagination = 1;
            }
            if (pagination != previousPage)
            {
                addingAction = false;
                addingCondition = false;
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
                GUILayout.Box("", MAI_EditorUtility.horizontalLine);
                EditorGUILayout.LabelField("Conditions:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("A conditional statement that adds a defined score to a behaviour's total score if the statment meets it's target state.", MAI_EditorUtility.wrappedMiniLabel);
                GUILayout.Box("", MAI_EditorUtility.horizontalLine);
                EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Code that executes when a behaviour's score wins over all other behaviour scores.", MAI_EditorUtility.wrappedMiniLabel);
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        public void DrawBehaviourOptions(AIAgent self)
        {
            GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
            if (renaming == true)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                self.behaviours[pagination - 1].name = EditorGUILayout.TextField(self.behaviours[pagination - 1].name);
                if (GUILayout.Button("Apply", EditorStyles.miniButton))
                {
                    renaming = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(self.behaviours[pagination - 1].name, MAI_EditorUtility.centeredLabel, GUILayout.ExpandWidth(true));
                if (self.behaviours.Count > 1)
                {
                    if (GUILayout.Button(new GUIContent("", "Remove Behaviour"), "OL Minus", GUILayout.Width(20)))
                    {
                        self.behaviours.Remove(self.behaviours[pagination - 1]);
                        if (pagination > 1)
                        {
                            pagination--;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Box("", MAI_EditorUtility.horizontalLine);

                EditorGUILayout.BeginHorizontal();
                if (renaming == false && GUILayout.Button("Rename", EditorStyles.miniButtonLeft))
                {
                    renaming = true;
                }
                if (GUILayout.Button("Copy", EditorStyles.miniButtonMid))
                {
                    copied = new AIBehaviour(self.behaviours[pagination - 1].name + " (Copy)", self.behaviours[pagination - 1].conditions, self.behaviours[pagination - 1].actions);
                }
                if (GUILayout.Button("Paste", EditorStyles.miniButtonMid))
                {
                    if (copied != null)
                    {
                        self.behaviours[pagination - 1] = new AIBehaviour(copied.name, copied.conditions, copied.actions);
                    }
                }
                if (GUILayout.Button("Duplicate", EditorStyles.miniButtonRight))
                {
                    if (copied != null)
                    {
                        self.behaviours.Add(new AIBehaviour(self.behaviours[pagination - 1].name + " (Copy)", self.behaviours[pagination - 1].conditions, self.behaviours[pagination - 1].actions));
                        pagination = self.behaviours.Count;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Box("", MAI_EditorUtility.horizontalLine);
            DrawBrokenReferenceNotice(self);
        }

        public void DrawConditionListGUI(AIAgent self, int page)
        {
            showConditions = MAI_EditorUtility.DrawFoldout(showConditions, "Conditions:");
            if (showConditions)
            {
                // Collapse open folouts:
                if (showActions == true || showEvents == true || showBehaviourHelp == true)
                {
                    showActions = false;
                    showEvents = false;
                    showBehaviourHelp = false;
                }

                // Draw conditions:
                EditorGUILayout.BeginVertical("box");
                if (self.behaviours[page].conditions != null && self.behaviours[page].conditions.Count > 0)
                {
                    GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
                    for (int c = self.behaviours[page].conditions.Count - 1; c >= 0; c--)
                    {
                        if (self.behaviours[page].conditions[c].instance != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.BeginVertical(EditorStyles.miniButtonLeft, GUILayout.Width(20));                         
                            self.behaviours[page].conditions[c].score = Mathf.Clamp(EditorGUILayout.IntField(self.behaviours[page].conditions[c].score, MAI_EditorUtility.centeredMiniLabel, GUILayout.Width(20)), 1, 999);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical(EditorStyles.miniButtonMid);
                            GUILayout.Label(self.behaviours[page].conditions[c].name, MAI_EditorUtility.centeredMiniLabel);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical(EditorStyles.miniButtonRight, GUILayout.Width(30));
                            if (GUILayout.Button(self.behaviours[page].conditions[c].state.ToString(), MAI_EditorUtility.centeredMiniLabel))
                            {
                                self.behaviours[page].conditions[c].state = !self.behaviours[page].conditions[c].state;
                            }
                            EditorGUILayout.EndVertical();

                            if (GUILayout.Button("", "OL Minus", GUILayout.Width(25)))
                            {
                                self.behaviours[page].conditions.Remove(self.behaviours[page].conditions[c]);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Missing module for " + self.behaviours[page].conditions[c].name, MessageType.Warning);
                            EditorGUILayout.LabelField("Class name changed? Update it below then attempt repair:");
                            self.behaviours[page].conditions[c].moduleName = EditorGUILayout.TextField(self.behaviours[page].conditions[c].moduleName);
                            EditorGUILayout.LabelField("OR");
                            if (GUILayout.Button("Remove Condition", EditorStyles.miniButton))
                            {
                                self.behaviours[page].conditions.Remove(self.behaviours[page].conditions[c]);
                            }

                            self.hasBrokenReferences = true;
                        }

                        GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
                    }
                }
                DrawConditionCreationGUI(self.behaviours[page]);
                EditorGUILayout.EndVertical();
            }
            else
            {
                addingCondition = false;
            }
        }

        public void DrawConditionCreationGUI(AIBehaviour behaviour)
        {
            if (addingCondition == false)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Add Condition:", MAI_EditorUtility.centeredBoldLabel);
                if (GUILayout.Button("", "OL Plus", GUILayout.Width(20)))
                {
                    addingCondition = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField("New Condition ", MAI_EditorUtility.centeredBoldLabel);
                GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);

                // Begin variable settings.
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Score:", "The influence of the condition toward the AI's motivation of acting on the behaviour."), GUILayout.MaxWidth(70));
                newConditionValue = EditorGUILayout.IntSlider(newConditionValue, 1, 999);
                EditorGUILayout.EndHorizontal();
                GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
                newConditionState = EditorGUILayout.Toggle(new GUIContent("State: [" + newConditionState.ToString() + "]", "The required state for the condition's score to count."), newConditionState);

                GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);

                selectedModule = MAI_EditorUtility.DrawCompactPopup("Module:", selectedModule, moduleNames);
                selectedCondition = MAI_EditorUtility.DrawCompactPopup("Conditions:", selectedCondition, self.modules[selectedModule].conditions);

                GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Condition", EditorStyles.miniButton))
                {
                    if(self.modules[selectedModule].conditions.Length > 0)
                    {
                        behaviour.conditions.Add(new BehaviourEvent(self.modules[selectedModule].conditions[selectedCondition], newConditionValue, self.modules[selectedModule], selectedCondition, newConditionState));
                        //TODO: Mark current scene scene dirty? EditorSceneManager.MarkSceneDirty();
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

        public void DrawActionListGUI(AIAgent self, int page)
        {
            showActions = MAI_EditorUtility.DrawFoldout(showActions, "Actions:");
            if (showActions)
            {
                // Collapse open foldouts:
                if (showConditions == true || showEvents == true || showBehaviourHelp == true)
                {
                    showConditions = false;
                    showEvents = false;
                    showBehaviourHelp = false;
                }

                // Draw Actions:
                EditorGUILayout.BeginVertical("box");
                if (self.behaviours[page].actions != null && self.behaviours[page].actions.Count > 0)
                {
                    GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
                    for (int i = self.behaviours[page].actions.Count - 1; i >= 0; i--)
                    {
                        if (self.behaviours[page].actions[i].instance != null)
                        {
                            EditorGUILayout.BeginHorizontal();

                            EditorGUILayout.LabelField((self.behaviours[page].actions.Count - i).ToString(), MAI_EditorUtility.centeredMiniLabel, GUILayout.Width(20), GUILayout.ExpandWidth(false));
                            if (self.behaviours[page].actions.Count > 1)
                            {
                                int oldIndex = i;
                                BehaviourEvent oldAction = self.behaviours[page].actions[i];
                                if(i == self.behaviours[page].actions.Count - 1)
                                {
                                    if (GUILayout.Button("↓", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
                                    {
                                        // Move action down.
                                        self.behaviours[page].actions.Remove(self.behaviours[page].actions[oldIndex]);
                                        self.behaviours[page].actions.Insert(oldIndex - 1, oldAction);
                                    }
                                }
                                else if(i == 0)
                                {
                                    if (GUILayout.Button("↑", EditorStyles.miniButtonLeft, GUILayout.Width(50)))
                                    {
                                        // Move action up.
                                        self.behaviours[page].actions.Remove(self.behaviours[page].actions[oldIndex]);
                                        self.behaviours[page].actions.Insert(oldIndex + 1, oldAction);
                                    }
                                }
                                else if(i > 0 && i < self.behaviours[page].actions.Count)
                                {
                                    if (GUILayout.Button("↓", EditorStyles.miniButtonLeft, GUILayout.Width(25)))
                                    {
                                        // Move action down.
                                        self.behaviours[page].actions.Remove(self.behaviours[page].actions[oldIndex]);
                                        self.behaviours[page].actions.Insert(oldIndex - 1, oldAction);
                                    }
                                    if (GUILayout.Button("↑", EditorStyles.miniButtonMid, GUILayout.Width(25)))
                                    {
                                        // Move action up.
                                        self.behaviours[page].actions.Remove(self.behaviours[page].actions[oldIndex]);
                                        self.behaviours[page].actions.Insert(oldIndex + 1, oldAction);
                                    }
                                }
                            }
                            if(self.behaviours[page].actions.Count == 1)
                            {
                                EditorGUILayout.BeginHorizontal(EditorStyles.miniButton);
                            }
                            else
                            {
                                EditorGUILayout.BeginHorizontal(EditorStyles.miniButtonRight);
                            }
                            EditorGUILayout.LabelField(self.behaviours[page].actions[i].name, MAI_EditorUtility.upperLeftMiniLabel, GUILayout.MaxWidth(120));
                            EditorGUILayout.EndHorizontal();
                            if (GUILayout.Button("", "OL Minus", GUILayout.Width(25)))
                            {
                                self.behaviours[page].actions.Remove(self.behaviours[page].actions[i]);
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Missing " + Type.GetType(self.behaviours[page].actions[i].moduleName).Name + " for " + self.behaviours[page].actions[i].name, MessageType.Warning);
                            EditorGUILayout.LabelField("Class name changed? Update it below then attempt repair.");
                            self.behaviours[page].actions[i].moduleName = EditorGUILayout.TextField(self.behaviours[page].actions[i].moduleName);
                            EditorGUILayout.LabelField("OR");
                            if(GUILayout.Button("Remove Action", EditorStyles.miniButton))
                            {
                                self.behaviours[page].actions.Remove(self.behaviours[page].actions[i]);
                            }
                            self.hasBrokenReferences = true;
                        }


                        GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
                    }
                }
                DrawActionCreationGUI(self.behaviours[page]);
                EditorGUILayout.EndVertical();
            }
            else
            {
                addingAction = false;
            }
        }

        public void DrawActionCreationGUI(AIBehaviour behaviour)
        {
            if (addingAction == false)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Add Action:", MAI_EditorUtility.centeredBoldLabel);
                if (GUILayout.Button("", "OL Plus", GUILayout.Width(20)))
                {
                    addingAction = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField("New Action ", MAI_EditorUtility.centeredBoldLabel);

                GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
                selectedModule = MAI_EditorUtility.DrawCompactPopup("Module:", selectedModule, moduleNames);
                selectedAction = MAI_EditorUtility.DrawCompactPopup("Actions:", selectedAction, self.modules[selectedModule].actions);

                GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
                if (behaviour.actions.Count > 1)
                {
                    EditorGUILayout.HelpBox("Note: actions are executed in the order they are arranged.", MessageType.None);
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Action", EditorStyles.miniButton))
                {
                    if(self.modules[selectedModule].actions.Length > 0)
                    {
                        behaviour.actions.Insert(0, new BehaviourEvent(self.modules[selectedModule].actions[selectedAction], selectedAction, self.modules[selectedModule]));
                        //TODO: Mark current scene scene dirty? EditorSceneManager.MarkSceneDirty();
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

        public void DrawSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Agent Settings:", EditorStyles.boldLabel);
            GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);

            if (EditorGUILayout.PropertyField(debugMode) == true)
            {
                EditorGUILayout.PropertyField(debugLevel);
            }

            GUILayout.Box("", MAI_EditorUtility.horizontalLine);
            EditorGUILayout.PropertyField(scoreType);
            if (scoreType.enumValueIndex == ((int)ScoreTypes.AllScoresAboveThreshold) || scoreType.enumValueIndex == ((int)ScoreTypes.FirstScoreAboveThreshold))
            {
                EditorGUILayout.PropertyField(scoreThreshold);
            }
            GUILayout.Box("", MAI_EditorUtility.horizontalLine);
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

                EditorGUILayout.BeginHorizontal("box");
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

        private void DrawBrokenReferenceNotice(AIAgent self)
        {
            if (self.hasBrokenReferences == true)
            {
                EditorGUILayout.Space();
                GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
                EditorGUILayout.HelpBox("Broken logic detected! A missing module could impact gameplay!", MessageType.Error);
                if (GUILayout.Button("Attempt Repair"))
                {
                    self.ValidateBehaviours();
                    self.FixBrokenReferences();
                }
                GUILayout.Box("", MAI_EditorUtility.thickHorizontalLine);
            }
        }

        #endregion
    }
}