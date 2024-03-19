using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {

        #region GUI Related

        [SerializeField] int _sel_ikLimb = -1;
        public static ADClipSettings_IK IKettingsCopyFrom = null;

        void DrawIKTab()
        {
            if (isReady == false) { EditorGUILayout.HelpBox("First prepare Armature", MessageType.Info); return; }

            if (TargetClip == null)
            {
                EditorGUILayout.HelpBox("Animation Clip is required", MessageType.Info);
                return;
            }

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            _anim_MainSet.TurnOnIK = EditorGUILayout.Toggle(_anim_MainSet.TurnOnIK, GUILayout.Width(24));

            if (_anim_MainSet.TurnOnIK == false) GUI.enabled = false;

            DrawTargetClipField("IK Configuration For:", true);

            ADClipSettings_IK setup = S.GetSetupForClip(S.IKSetupsForClips, TargetClip, _toSet_SetSwitchToHash);

            if (IKettingsCopyFrom != null)
            {
                //UnityEngine.Debug.Log("copy from " + IKettingsCopyFrom.LimbIKSetups.Count + " vs " + setup.LimbIKSetups.Count);

                if (IKettingsCopyFrom.LimbIKSetups.Count == setup.LimbIKSetups.Count)
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied values to all limbs of the animation clip"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                    {
                        ADClipSettings_IK.PasteValuesTo(IKettingsCopyFrom, setup);
                        DisplaySave._SetDirty();
                        IKettingsCopyFrom = null;
                    }
            }

            GUILayout.Space(2);

            GUILayout.Space(5);
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy elasticness parameters values from all limbs below to paste them into other animation clip limbs settings"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
            {
                IKettingsCopyFrom = setup;
                DisplaySave._SetDirty();
            }

            EditorGUILayout.EndHorizontal();


            _LimbsRefresh();

            FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 8, 0.975f);
            GUILayout.Space(6);

            if (_anim_MainSet.TurnOnIK == true)
            {

                #region Refresh Coloring

                for (int i = 0; i < S.Limbs.Count; i++)
                {
                    var set = setup.GetIKSettingsForLimb(S.Limbs[i], S);

                    if (set.Enabled == false)
                        S.Limbs[i].GizmosBlend = 0.0f;
                    else
                        S.Limbs[i].GizmosBlend = set.Blend;
                }

                #endregion


                var limbsList = S.GetLimbsExecutionList(setup.LimbIKSetups);
                StartUndoCheckFor(this, ": IK Limbs");
                DrawSelectorGUI(limbsList, ref _sel_ikLimb, 18, position.width - 22, -1);
                EndUndoCheckFor(this);

                if (_sel_ikLimb > -1)
                    if (Limbs.ContainsIndex(_sel_ikLimb, true))
                    {
                        var selectedLimb = Limbs[_sel_ikLimb];
                        var settings = setup.GetIKSettingsForLimb(selectedLimb, S);

                        GUILayout.Space(3);
                        EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                        StartUndoCheck(": IK");

                        DrawNullTweakGUI(selectedLimb, _sel_ikLimb);

                        settings.DrawTopGUI(selectedLimb.GetName, animationProgress);
                        settings.DrawParamsGUI(animationProgress, selectedLimb, _anim_MainSet, S, setup);
                        //settings.DrawIKHipsParameters(selectedLimb, _anim_MainSet, S);

                        EndUndoCheck();

                        EditorGUILayout.EndVertical();

                    }
                    else
                    {
                        GUILayout.Space(8);
                        EditorGUILayout.HelpBox("No Limb Selected for IK Settings", MessageType.Info);
                        GUILayout.Space(3);
                    }


                FGUI_Inspector.DrawUILine(0.5f, 0.4f, 1, 20, 0.975f);
                ADClipSettings_IK.IKSet.DrawIKHipsParameters(animationProgress, _anim_MainSet, S);
                FGUI_Inspector.DrawUILine(0.5f, 0.4f, 1, 4, 0.975f);
                GUILayout.Space(5);

            }
            else
            {
                EditorGUILayout.HelpBox("All IK is turned Off!", MessageType.None);
                GUILayout.Space(5);
            }

            EditorGUILayout.EndVertical();

            IK_DrawTooltipField();

            EditorGUILayout.BeginVertical(); // To Avoid error for ending vertical

        }

        #endregion


        #region Gizmos Related

        void _Gizmos_IKCategory()
        {
            if (_sel_ikLimb != -1)
            {
                if (Limbs.ContainsIndex(_sel_ikLimb, true) == false) return;

                var limb = Limbs[_sel_ikLimb];
                if (limb == null) return;

                ADClipSettings_IK.IKSet ikSet = null;
                if (_anim_ikSet != null) ikSet = _anim_ikSet.GetIKSettingsForLimb(limb, S);
                bool isGroundingSet = false;

                float visMul = 1f;
                if (ikSet != null)
                {
                    isGroundingSet = ikSet.IsLegGroundingMode;
                    if (isGroundingSet) visMul = 0.4f;
                }

                Handles.color = new Color(0.8f, 0.8f, 0.1f, 0.3f * visMul + timeSin01 * 0.3f);
                limb.DrawGizmos(0.65f * visMul + timeSin01 * 0.2f);

                if (limb.Bones != null) if (limb.Bones[0].T != null)
                    {
                        float len = Vector3.Distance(limb.Bones[0].pos, limb.Bones[limb.Bones.Count - 1].pos);
                        Handles.DrawDottedLine(limb.Bones[0].pos, limb.Bones[limb.Bones.Count - 1].pos, 2f);

                        if (isGroundingSet)
                        {
                            #region IK Leg Grounding Gizmos

                            if (ikSet.LegGrounding == ADClipSettings_IK.IKSet.ELegGroundingView.Analyze)
                            {
                                if (ikSet.WasAnalyzed)
                                {
                                    if (limb.Bones[0].T)
                                    {
                                        ikSet.RefreshGizmosBuffers();


                                        Transform animT = latestAnimator.transform;
                                        Vector3 guidePos = animT.position + animT.up * ikSet.FootDataAnalyze.LowestFootCoords.y;
                                        Handles.color = new Color(1f, 1f, 1f, 0.075f);
                                        Handles.DrawDottedLine(guidePos + animT.forward, guidePos - animT.forward, 2f);
                                        Handles.color = new Color(.7f, .37f, 0.15f, 0.5f);
                                        guidePos.y = animT.position.y;
                                        Handles.DrawDottedLine(guidePos + animT.forward, guidePos - animT.forward, 2f);


                                        Handles.color = new Color(1f, 1f, 1f, 1f);
                                        ikSet.DrawHellFootDetectionMask(_anim_MainSet, limb, ikSet.HeelGroundedTreshold, ikSet.ToesGroundedTreshold, 0.5f + timeSin01 * 0.2f, 0.7f - timeSin01 * 0.2f);

                                        if (ikSet.FootDataAnalyze._latestHeelTesh != ikSet.HeelGroundedTreshold ||
                                            ikSet.FootDataAnalyze._latestToesTesh != ikSet.ToesGroundedTreshold)
                                        {
                                            Handles.color = new Color(1f, 1f, 0.4f, 0.75f);
                                            ikSet.DrawHellFootDetectionMask(_anim_MainSet, limb, ikSet.FootDataAnalyze._latestHeelTesh, ikSet.FootDataAnalyze._latestToesTesh, 0.1f, 0.1f);
                                        }


                                        ikSet.DrawFootGroundingAnalyzeGizmos(animationProgress, limb);

                                        #region Display ghosting of other IK grounding setups

                                        for (int i = 0; i < _anim_ikSet.LimbIKSetups.Count; i++)
                                        {
                                            if (i == _sel_ikLimb) continue;
                                            if (Limbs.ContainsIndex(i, true) == false) continue;
                                            var oLimb = Limbs[i];
                                            var oIk = _anim_ikSet.GetIKSettingsForLimb(oLimb, S);
                                            if (oIk.IsLegGroundingMode == false) continue;
                                            oIk.DrawFootGroundingAnalyzeGizmos(animationProgress, oLimb, 0.375f);
                                        }

                                        #endregion


                                    }
                                    else
                                    {
                                        UnityEngine.Debug.Log("[Animation Designer] No bones in limb!");
                                    }
                                }
                            }
                            else if (ikSet.LegGrounding == ADClipSettings_IK.IKSet.ELegGroundingView.Processing)
                            {


                                ikSet.DrawFootGroundingAnalyzeGizmos(animationProgress, limb, 0.425f);

                                #region Display ghosting of other IK grounding setups

                                for (int i = 0; i < _anim_ikSet.LimbIKSetups.Count; i++)
                                {
                                    if (i == _sel_ikLimb) continue;
                                    if (Limbs.ContainsIndex(i, true) == false) continue;
                                    var oLimb = Limbs[i];
                                    var oIk = _anim_ikSet.GetIKSettingsForLimb(oLimb, S);
                                    if (oIk.IsLegGroundingMode == false) continue;
                                    oIk.DrawFootGroundingAnalyzeGizmos(animationProgress, oLimb, 0.375f);
                                }

                                #endregion

                                if (ikSet.IKHintOffset != Vector3.zero)
                                {
                                    Handles.DrawDottedLine(limb._LatestHintPos, limb.Bones[1].pos, 4f);
                                    Handles.RectangleHandleCap(0, limb._LatestHintPos, Quaternion.identity, len * 0.14f, EventType.Repaint);
                                }

                            }

                            #endregion
                        }
                        else
                        {

                            #region Regular IK Gizmos Controls

                            if (_anim_ikSet != null)
                            {

                                Handles.SphereHandleCap(0, limb._LatestIKPos, Quaternion.identity, len * 0.065f, EventType.Repaint);
                                Handles.CircleHandleCap(0, limb._LatestIKPos, limb._LatestIKRot, len * 0.14f, EventType.Repaint);


                                if (ikSet.IKType != ADClipSettings_IK.IKSet.EIKType.ChainIK)
                                {
                                    Handles.DrawDottedLine(limb._LatestHintPos, limb.Bones[1].pos, 4f);
                                    Handles.RectangleHandleCap(0, limb._LatestHintPos, Quaternion.identity, len * 0.14f, EventType.Repaint);
                                }


                                bool positionForStillPoints = false;
                                if (ikSet.UseMultiStillPoints)
                                {
                                    if (ADClipSettings_IK.IKSet._SelectedStillPoint != null) positionForStillPoints = true;
                                }

                                if (positionForStillPoints)
                                {
                                    var p = ADClipSettings_IK.IKSet._SelectedStillPoint;
                                    if (p != null)
                                    {
                                        Vector3 prePos;
                                        if (ikSet.IKStillWorldPos)
                                        { if (latestAnimator.parent == null) prePos = p.pos; else prePos = latestAnimator.parent.TransformPoint(p.pos); }
                                        else
                                            prePos = latestAnimator.transform.TransformPoint(p.pos);

                                        float scale = 0.24f;
                                        if (currentMecanim) if (currentMecanim.isHuman) scale = Mathf.Clamp(currentMecanim.humanScale * 0.14f, 0.175f, 1f);

                                        Vector3 newPos = FEditor_TransformHandles.PositionHandle(prePos, Quaternion.identity, scale);

                                        if (Vector3.Distance(prePos, newPos) > 0.001f)
                                        {
                                            if (ikSet.IKStillWorldPos)
                                            { if (latestAnimator.parent == null) p.pos = (newPos); else p.pos = latestAnimator.parent.InverseTransformPoint(newPos); }
                                            else
                                                p.pos = latestAnimator.transform.InverseTransformPoint(newPos);
                                        }
                                    }
                                }
                                else
                                {
                                    if (ikSet.LatelyAdjusted == ADClipSettings_IK.IKSet.EWasAdjusted.Offset)
                                    {
                                        if (playPreview == false)
                                            if (ikSet.IKPositionOffset != Vector3.zero)
                                                if (ikSet.IKPosOffMul > 0.001f)
                                                    if (ikSet.LastUsedProcessor != null)
                                                    {
                                                        Vector3 prePos = Vector3.zero;
                                                        prePos = limb.LastBone.pos;

                                                        float scale = 0.24f;

                                                        Handles.color = new Color(0.2f, 1f, 0.4f, 1f);
                                                        Vector3 newPos = FEditor_TransformHandles.PositionHandle(prePos, Quaternion.identity, scale, false, true, false);
                                                        Handles.color = Color.white;

                                                        if ((prePos - newPos).sqrMagnitude > 0.0001f)
                                                        {
                                                            //root.TransformVector(IKPositionOffset) * IKPosOffMul * IKPosOffEvaluate.Evaluate(progr)
                                                            ikSet.IKPositionOffset += Ar.LatestAnimator.InverseTransformVector(newPos - prePos);
                                                        }
                                                    }
                                    }
                                    else
                                    {
                                        if (ikSet.IKPosStillMul > 0f)
                                        {
                                            Vector3 prePos;
                                            if (ikSet.IKStillWorldPos)
                                            { if (latestAnimator.parent == null) prePos = ikSet.IKStillPosition; else prePos = latestAnimator.parent.TransformPoint(ikSet.IKStillPosition); }
                                            else
                                                prePos = latestAnimator.transform.TransformPoint(ikSet.IKStillPosition);

                                            float scale = 0.24f;
                                            if (currentMecanim) if (currentMecanim.isHuman) scale = Mathf.Clamp(currentMecanim.humanScale * 0.14f, 0.175f, 1f);

                                            Vector3 newPos = FEditor_TransformHandles.PositionHandle(prePos, Quaternion.identity, scale);

                                            if (Vector3.Distance(prePos, newPos) > 0.001f)
                                            {
                                                if (ikSet.IKStillWorldPos)
                                                { if (latestAnimator.parent == null) ikSet.IKStillPosition = (newPos); else ikSet.IKStillPosition = latestAnimator.parent.InverseTransformPoint(newPos); }
                                                else
                                                    ikSet.IKStillPosition = latestAnimator.transform.InverseTransformPoint(newPos);
                                            }
                                        }
                                    }

                                }
                            }

                            #endregion

                        }

                    }
            }
        }

        #endregion


        #region Update Loop Related

        void _Update_IKCategory()
        {
            IK_UpdateTooltips();
        }

        #endregion


        #region Tip Field


        float _tip_ik_alpha = 0f;
        string _tip_ik = "";
        float _tip_ik_elapsed = -4f;
        int _tip_ik_index = 0;

        void IK_DrawTooltipField()
        {
            if (_tip_ik_alpha > 0f)
            {
                GUI.color = new Color(1f, 1f, 1f, _tip_ik_alpha);
                EditorGUILayout.LabelField(_tip_ik, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30));
                GUI.color = preGuiC;
            }
            else
                EditorGUILayout.LabelField(" ", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30));
        }


        void IK_UpdateTooltips()
        {
            _tip_ik_elapsed += dt;

            if (_tip_ik == "") Tooltip_CheckIKText();

            if (_tip_ik_elapsed > 0f)
            {
                if (_tip_ik_elapsed < 8f)
                {
                    _tip_ik_alpha = Mathf.Lerp(_tip_ik_alpha, 1f, dt * 3f);
                }
                else
                {
                    _tip_ik_alpha = Mathf.Lerp(_tip_ik_alpha, -0.05f, dt * 6f);
                }

                if (_tip_ik_elapsed > 16f)
                {
                    _tip_ik_elapsed = -4f;
                    _tip_ik_alpha = 0f;
                    Tooltip_CheckIKText();
                }
            }
        }

        void Tooltip_CheckIKText()
        {
            if (_tip_ik_index == 0) _tip_ik = "Some IK algorithms Requires correct T-Pose to work";
            else if (_tip_ik_index == 1) _tip_ik = "You can use ik to soothe arm-walk animations.\nTry setting still position and blend ik to about 40%";
            else if (_tip_ik_index == 2) _tip_ik = "Click RIGHT Mouse Button on the Category Button for Focus Mode";
            else if (_tip_ik_index == 3) _tip_ik = "When tweaking Foot Grounding IK - try pausing animation preview\nand use red slider to check animation ik state manually";

            _tip_ik_index += 1;
            if (_tip_ik_index == 4) _tip_ik_index = 0;
        }

        #endregion

    }

}