using FIMSpace.FTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FIMSpace.AnimationTools.ADClipSettings_CustomModules;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_StrafeCreator : ADCustomModuleBase
    {
        public override string ModuleTitleName { get { return "Inverse Kinematics (IK)/Strafe Creator"; } }
        public override bool GUIFoldable { get { return true; } }
        CustomModuleSet cMod = null;


        float Yaw { get { return GetFloatVariable("DirOff"); } }
        float Pitch { get { return GetFloatVariable("PitchOff"); } }
        float Roll { get { return GetFloatVariable("ROff"); } }
        Vector3 LocalMul { get { return GetVector3Variable("MLoc"); } }
        Vector3 LocalBlend { get { return GetVector3Variable("AxisBlend"); } }
        Vector2 FootAngles { get { return GetVector2Variable("FootAngl"); } }
        Vector3 PositionOffset { get { return GetVector3Variable("IKOff"); } }


        public void OnRefreshVariables(CustomModuleSet customModuleSet)
        {
            cMod = customModuleSet;

            if (customModuleSet.ModuleIDHelper != "AStrafe" || customModuleSet.ModuleVariables.Count != 7)
            {
                customModuleSet.ModuleVariables.Clear();

                var nVar = _DirOffset;
                nVar = _PitchOffset; nVar.HideFlag = true;
                nVar = _RollOffset; nVar.HideFlag = true;

                nVar = _MulLocIK;
                nVar = _BlendAxis;
                nVar = _FootAngles;
                nVar = _IKLocalOff;

                customModuleSet.Foldown = false;
                customModuleSet.ModuleIDHelper = "AStrafe";
            }

            if (customModuleSet.ModuleVariables.Count > 4)
            {
                customModuleSet.ModuleVariables[1].HideFlag = !customModuleSet.Foldown;
                customModuleSet.ModuleVariables[2].HideFlag = true;
                customModuleSet.ModuleVariables[4].HideFlag = !customModuleSet.Foldown;
            }
        }

        public override void OnInfluenceIKUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set)
        {
            Transform animator = s.LatestAnimator;
            if (animator == null) return;

            PrepareReferences(s, anim_MainSet);
            OnRefreshVariables(set);
            
            Quaternion offset = Quaternion.AngleAxis(Yaw, Vector3.up);
            offset *= Quaternion.AngleAxis(Pitch, Vector3.right);
            offset *= Quaternion.AngleAxis(Roll, Vector3.forward);
            Matrix4x4 newRot = Matrix4x4.TRS(animator.position, offset * animator.rotation, animator.lossyScale); ;

            for (int i = 0; i < legs.Count; i++)
            {
                var leg = legs[i];

                FIK_IKProcessor legIK = leg.LastUsedProcessor as FIK_IKProcessor;
                if (legIK == null)
                {
                    //UnityEngine.Debug.Log("nulllik On " + legs[i].GetName);
                    continue;
                }

                Vector3 worldIK = legIK.IKTargetPosition;
                Vector3 localIK = animator.InverseTransformPoint(worldIK);
                Vector3 pelvisLocal = animator.InverseTransformPoint(anim_MainSet.Pelvis.position);

                localIK = Vector3.Scale(LocalMul, localIK);

                // Use rotation offset matrix
                Vector3 targetIK = newRot.MultiplyPoint(localIK);

                // Again Use Animator transform
                Vector3 nlocalIK = animator.InverseTransformPoint(targetIK);

                nlocalIK += PositionOffset;

                nlocalIK.x = Mathf.LerpUnclamped(localIK.x, nlocalIK.x, LocalBlend.x);
                nlocalIK.y = Mathf.LerpUnclamped(localIK.y, nlocalIK.y, LocalBlend.y);
                nlocalIK.z = Mathf.LerpUnclamped(localIK.z, nlocalIK.z, LocalBlend.z);

                legIK.IKTargetPosition = animator.TransformPoint(nlocalIK);

                if (FootAngles != Vector2.zero)
                {
                    offset = Quaternion.AngleAxis(FootAngles.y, Vector3.up);
                    offset *= Quaternion.AngleAxis(FootAngles.x, Vector3.right);
                    legIK.IKTargetRotation = offset * legIK.IKTargetRotation;
                }
            }
        }

        #region Editor GUI Related Code

        [HideInInspector] public bool _InitialInfoClicked = false;

        public override void InspectorGUI_Header(float animProgress, ADClipSettings_CustomModules.CustomModuleSet customModuleSet)
        {
            cMod = customModuleSet;
            OnRefreshVariables(customModuleSet);
            base.InspectorGUI_Header(animProgress, customModuleSet);
        }

        List<ADClipSettings_IK.IKSet> legs = new List<ADClipSettings_IK.IKSet>();

        void PrepareReferences(AnimationDesignerSave s, ADClipSettings_Main _anim_MainSet)
        {
            var ikSet = s.GetSetupForClip<ADClipSettings_IK>(s.IKSetupsForClips, _anim_MainSet.settingsForClip, _anim_MainSet.SetIDHash);
            if (ikSet == null) return;

            if (ikSet.LimbIKSetups.Count == 0)
            {
                return;
            }

            if (legs == null) legs = new List<ADClipSettings_IK.IKSet>();
            if (legs.Count > 0) if (legs[0] != ikSet.LimbIKSetups[0]) legs.Clear();
            if (legs.Count <= 0 || legs[0] == null || legs[0].LastUsedProcessor == null)
            {
                legs.Clear();
                for (int i = 0; i < ikSet.LimbIKSetups.Count; i++)
                {
                    if (ikSet.LimbIKSetups[i].IKType == ADClipSettings_IK.IKSet.EIKType.FootIK)
                    {
                        legs.Add(ikSet.LimbIKSetups[i]);
                    }
                }
            }
        }

        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, CustomModuleSet set)
        {
            #region Prepare Resources

            var ikSet = s.GetSetupForClip<ADClipSettings_IK>(s.IKSetupsForClips, _anim_MainSet.settingsForClip, AnimationDesignerWindow._toSet_SetSwitchToHash);

            if (ikSet == null)
            {
                EditorGUILayout.HelpBox("Can't Find IK Set!", MessageType.Warning);
                return;
            }

            #endregion

            #region Legs IK Search

            if (ikSet.LimbIKSetups.Count == 0)
            {
                EditorGUILayout.HelpBox("Can't Find Limb IK Setups!", MessageType.Warning);
                return;
            }

            PrepareReferences(s, _anim_MainSet);

            if (legs.Count <= 0)
            {
                EditorGUILayout.HelpBox("Can't Find Limb IK Legs!", MessageType.Warning);
                return;
            }

            if (_anim_MainSet.TurnOnIK == false)
            {
                EditorGUILayout.HelpBox("Go to IK Tab and enable using IK", MessageType.Warning);
                return;
            }

            if (legs[0].Enabled == false )
            {
                EditorGUILayout.HelpBox("Some legs IK seems to be disabled! Strafe creator will work only with enabled foot IKs!", MessageType.Info);
                if (GUILayout.Button("Enable Foot IKs"))
                {
                    for (int i = 0; i < legs.Count; i++) legs[i].Enabled = true;
                    S._SetDirty();
                }
            }
            else
            {
                if (!_InitialInfoClicked)
                {
                    EditorGUILayout.HelpBox("Strafe Creator can help you creating 8-direction movement out of single walk/run animation!", MessageType.None);
                    var r = GUILayoutUtility.GetLastRect();

                    if (GUI.Button(r, GUIContent.none, EditorStyles.label))
                    {
                        _InitialInfoClicked = true;
                    }

                    GUILayout.Space(6);
                }
            }

            #endregion

            base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);

            var nVar = _DirOffset;
            nVar = _PitchOffset; nVar.HideFlag = true;
            nVar = _RollOffset; nVar.HideFlag = true;
            
            nVar = _MulLocIK;
            nVar = _BlendAxis;
            nVar = _FootAngles;
            nVar = _IKLocalOff;
        }

        ADVariable _DirOffset { get { return GVar("DirOff", "Direction Angle", 0f, -90f, 90f, "Angle offset for foot IK motion"); } }
        ADVariable _PitchOffset { get { return GVar("PitchOff", "Pitch Offset", 0f, -90f, 90f, "Angle offset for foot IK motion to make motion move up/down the hill"); } }
        ADVariable _RollOffset { get { return GVar("ROff", "Roll Offset", 0f, -90f, 90f, "Angle offset for foot IK motion to make motion move left/right the slope-wall"); } }
        ADVariable _MulLocIK { get { return GVar("MLoc", "Multiply Local IK", Vector3.one); } }
        ADVariable _BlendAxis { get { return GVar("AxisBlend", "Blend Selective Axis", Vector3.one); } }
        ADVariable _FootAngles { get { return GVar("FootAngl", "Foot Angles Offset", Vector2.zero); } }
        ADVariable _IKLocalOff { get { return GVar("IKOff", "IK Points Offset", Vector3.zero); } }


        ADVariable GVar(string id, string displayName, object defValue, string tooltip = "")
        {
            var v = GetVariable(id, null, defValue);
            v.DisplayName = displayName;
            v.Tooltip = tooltip;
            return v;
        }

        ADVariable GVar(string id, string displayName, object defValue, float min, float max, string tooltip = "")
        {
            var v = GVar(id, displayName, defValue, tooltip);
            v.SetRangeHelperValue(new Vector2(min, max));
            return v;
        }

        #endregion



    }
}
