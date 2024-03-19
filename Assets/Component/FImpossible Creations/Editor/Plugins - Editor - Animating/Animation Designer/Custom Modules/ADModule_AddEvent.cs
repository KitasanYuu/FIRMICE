using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{

    public class ADModule_AddEvent : ADCustomModuleBase
    {
        public override string ModuleTitleName { get { return "Utilities/Add Event"; } }
        public override bool GUIFoldable { get { return false; } }
        public override bool SupportBlending { get { return false; } }


        public override void InspectorGUI_ModuleBody(float clipProgress, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            EditorGUILayout.HelpBox("Module for Including Animation Event in the exported animation clip", MessageType.None);

            GUILayout.Space(4);
            var targetName = GetVariable("EventName", set, "My Event");
            targetName.GUISpacing = new Vector2(0, 6); // Spacing
            targetName.HideFlag = false;

            targetName.DrawGUI();
            GUILayout.Space(1);

            var atProgr = GetVariable("At Animation Progress", set, 0.25f);
            atProgr.SetRangeHelperValue(0f, 1f);
            atProgr.DrawGUI();

            GUILayout.Space(4);
            EditorGUILayout.HelpBox("Current Animation Progress = " + System.Math.Round(clipProgress, 2).ToString(), MessageType.None);
        }


        public override void OnExportFinalizing(AnimationClip originalClip, AnimationClip newGeneratedClip, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set, List<AnimationEvent> addingEvents)
        {
            base.OnExportFinalizing(originalClip, newGeneratedClip, s, anim_MainSet, customModules, set, addingEvents);

            var targetName = GetVariable("EventName", set, "My Event").GetStringValue();
            var atProgr = GetFloatVariable("At Animation Progress");

            AnimationEvent evt = new AnimationEvent();
            UnityEngine.Debug.Log("originlen " + originalClip.length + " newlen " + newGeneratedClip.length);
            evt.time = atProgr * newGeneratedClip.length /** anim_MainSet.ClipDurationMultiplier*/;
            evt.functionName = targetName;
            addingEvents.Add(evt);
        }



    }
}