using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static FIMSpace.AnimationTools.ADClipSettings_CustomModules;

namespace FIMSpace.AnimationTools
{
    public abstract class ADCustomModuleBase : ScriptableObject
    {
        /// <summary> Title name which will be used for the module selector menu </summary>
        public abstract string ModuleTitleName { get; }


        #region Shortcut Fields

        protected CustomModuleSet relevantSet { get; private set; }

        protected AnimationDesignerSave S { get { return AnimationDesignerWindow.Get.S; } }
        protected ADArmatureSetup Ar { get { return AnimationDesignerWindow.Get.Ar; } }

        public UnityEngine.Object SaveDirectory { get { return AnimationDesignerWindow.Get.ModuleSetupsDirectory; } }

        #endregion


        /// <summary> If you allow to display blending GUI elements like slider / curve </summary>
        public virtual bool SupportBlending { get { return false; } }
        /// <summary> Allow to draw foldout button </summary>
        public virtual bool GUIFoldable { get { return false; } }


        /// <summary> [Base is calling Variables Refresh] When module file starts to work for a first time </summary>
        public virtual void OnInitialize(CustomModuleSet customModuleSet)
        {
            relevantSet = customModuleSet;

            if (customModuleSet == null) return;
            if (customModuleSet.TransformsMemory == null) return;

            for (int i = 0; i < customModuleSet.TransformsMemory.Count; i++)
            {
                customModuleSet.TransformsMemory[i].InitializeReference(S);
            }
        }


        /// <summary> When module file starts to serve other animation clip setup </summary>
        public virtual void OnSetupChange(CustomModuleSet customModuleSet) { customModuleSet.ModuleVariables.Clear(); relevantSet = customModuleSet; }

        /// <summary> Update() method simulation in Animation Designer loop.
        /// Base is checking Initialization! 
        /// It's before Elasticness update. </summary>
        public virtual void OnInheritElasticnessUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set)
        {
            relevantSet = set;
            CheckInitialization(set);
        }

        /// <summary> [base need to be executed on the beginning of the override!] After Update() method simulation in Animation Designer loop </summary>
        public virtual void OnBeforeIKUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set) { relevantSet = set; }

        /// <summary> [base need to be executed on the beginning of the override!] After IK - LateUpdate() method simulation in Animation Designer loop </summary>
        public virtual void OnLateUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set) { relevantSet = set; }

        /// <summary> [base need to be executed on the beginning of the override!] After all Animation Designer LateUpdates() simulation in Animation Designer loop </summary>
        public virtual void OnLastUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set) { relevantSet = set; }

        /// <summary> [base need to be executed on the beginning of the override!] Called when sampling target clip animation </summary>
        public virtual void OnPreUpdateSampling(AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, CustomModuleSet set, ref float animProgress, ref float animationProgressClipTime) { relevantSet = set; }
        /// <summary> [base need to be executed on the beginning of the override!] </summary>
        public virtual void OnPreUpdateSamplingMorph(AnimationDesignerSave s, AnimationClip clip, ADClipSettings_Morphing.MorphingSet morphSet, CustomModuleSet set, ref float clipTime) { relevantSet = set; }

        /// <summary> [base need to be executed on the beginning of the override!] After IK Offsets Update but before applying IK to the bones </summary>
        public virtual void OnInfluenceIKUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set) { relevantSet = set; }


        /// <summary> [base need to be executed on the beginning of the override!] Called when animation clip compleated baking and is finalizing target animation clip </summary>
        public virtual void OnExportFinalizing(AnimationClip originalClip, AnimationClip newClip, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set, List<AnimationEvent> addingEvents) { relevantSet = set; }



        public virtual void OnResetState(CustomModuleSet customModuleSet) { relevantSet = customModuleSet; }


        #region Utils

        private bool _wasInitialized = false;
        void CheckInitialization(CustomModuleSet set)
        {
            if (_wasInitialized) return;

            OnInitialize(set);

            _wasInitialized = true;
        }


        protected float GetEvaluatedBlend(CustomModuleSet set, float animProgress)
        {
            return set.Blend * set.BlendEvaluation.Evaluate(animProgress);
        }


        public ADArmatureLimb GetLimbByID(AnimationDesignerSave s, int id)
        {
            if (id < 0) return null; // No limb with the provided ID

            if (s == null) return null;
            if (s.Limbs == null) return null;

            if (id >= s.Limbs.Count) return null; // No limb with the provided ID

            ADArmatureLimb limb = s.Limbs[id];
            if (limb == null) return null; // No limb with the provided ID

            return limb;
        }


        public ADClipSettings_IK GetIKClipSettings(AnimationDesignerSave s, ADClipSettings_Main main)
        {
            if (s == null) return null;
            if (main == null) return null;
            ADClipSettings_IK ik = s.GetSetupForClip<ADClipSettings_IK>(s.IKSetupsForClips, main.settingsForClip, main.SetIDHash);
            return ik;
        }

        #endregion


        #region Variables Related

        string _selectorHelperID = "";
        protected Transform ArmatureTransformField(ADVariable transformVar, ADVariable transformNameVar, AnimationDesignerSave s, string title, string fieldID = "1")
        {
            Transform preT = transformVar.GetUnityObjRef() as Transform;

            bool searchableWasSet = Searchable.IsSetted;

            if (_selectorHelperID == fieldID)
            {
                var getTr = Searchable.Get<Transform>();
                if (searchableWasSet) if (getTr != preT) { transformVar.SetValue(getTr); if (getTr != null) transformNameVar.SetValue(getTr.name); else transformNameVar.SetValue(""); }
            }

            if (preT == null)
            {
                if (!string.IsNullOrEmpty(transformNameVar.GetStringValue()))
                {
                    preT = s.GetBoneByName(transformNameVar.GetStringValue());
                    transformVar.SetValue(preT);
                }
            }

            EditorGUILayout.BeginHorizontal();

            Transform newT = (Transform)EditorGUILayout.ObjectField(title, preT, typeof(Transform), true);
            if (preT != newT) { transformVar.SetValue(newT); if ( newT != null) transformNameVar.SetValue(newT.name); }

            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for available bone transform to choose"), EditorStyles.label, GUILayout.Width(16), GUILayout.Height(16)))
            {
                _selectorHelperID = fieldID;
                AnimationDesignerWindow.ShowBonesSelector("Choose Your Character Model Bone", s.GetAllArmatureBonesList, AnimationDesignerWindow.GetMenuDropdownRect(), true);
            }

            EditorGUILayout.EndHorizontal();

            return newT;
        }

        protected void ReadTransformForIDVariables(ADVariable transformVar, ADVariable transformNameVar, AnimationDesignerSave s)
        {
            Transform preT = transformVar.GetUnityObjRef() as Transform;

            if (preT == null)
            {
                if (!string.IsNullOrEmpty(transformNameVar.GetStringValue()))
                {
                    preT = s.GetBoneByName(transformNameVar.GetStringValue());
                    transformVar.SetValue(preT);
                }
            }
        }


        protected ADTransformMemory GetMemorizedTransform(string id)
        {
            return GetMemorizedTransform(id, id, "");
        }

        protected ADTransformMemory GetMemorizedTransform(string id, string displayName, string tooltip)
        {
            if (relevantSet == null) return null;

            for (int i = 0; i < relevantSet.TransformsMemory.Count; i++)
            {
                if (relevantSet.TransformsMemory[i].ID == id)
                {
                    var tm = relevantSet.TransformsMemory[i];
                    return tm;
                }
            }

            ADTransformMemory ntm = new ADTransformMemory();
            ntm.Allocate(S, id, displayName, tooltip);
            relevantSet.TransformsMemory.Add(ntm);
            return relevantSet.TransformsMemory[relevantSet.TransformsMemory.Count - 1];
        }


        /// <summary>
        /// Should be called on initialize to keep Animation Clip specific variable values
        /// </summary>
        protected ADVariable AddModuleSetVariable(CustomModuleSet customModuleSet, string varName, object initialValue)
        {
            customModuleSet.ModuleVariables.Add(new ADVariable(varName, initialValue));
            return customModuleSet.ModuleVariables[customModuleSet.ModuleVariables.Count - 1];
        }

        /// <summary>
        /// Getting universal variable reference of the specific animation clip working on
        /// </summary>
        protected ADVariable GetVariable(string id, CustomModuleSet customModuleSet = null, object autoGenerateIfNotFound = null)
        {
            if (customModuleSet == null) customModuleSet = relevantSet;
            if (customModuleSet == null) return null;

            //int hash = name.GetHashCode();

            for (int i = 0; i < customModuleSet.ModuleVariables.Count; i++)
            {
                //if (customModuleSet.ModuleVariables[i].NameHash == hash) return customModuleSet.ModuleVariables[i];
                if (customModuleSet.ModuleVariables[i].ID == id) return customModuleSet.ModuleVariables[i];
            }

            if (autoGenerateIfNotFound != null)
            {
                return AddModuleSetVariable(customModuleSet, id, autoGenerateIfNotFound);
            }

            return null;
        }

        protected ADVariable RequestVariable(string id, object autoGenerateIfNotFound = null)
        {
            if (relevantSet == null) return null;

            //int hash = name.GetHashCode();

            for (int i = 0; i < relevantSet.ModuleVariables.Count; i++)
            {
                //if (customModuleSet.ModuleVariables[i].NameHash == hash) return customModuleSet.ModuleVariables[i];
                if (relevantSet.ModuleVariables[i].ID == id) return relevantSet.ModuleVariables[i];
            }

            if (autoGenerateIfNotFound != null)
            {
                return AddModuleSetVariable(relevantSet, id, autoGenerateIfNotFound);
            }

            return null;
        }


        protected int GetIntVariable(string name)
        {
            var v = GetVariable(name);
            if (v == null) return 1;
            return v.IntV;
        }

        protected float GetFloatVariable(string name)
        {
            var v = GetVariable(name);
            if (v == null) return 1f;
            return v.Float;
        }

        protected Vector3 GetVector3Variable(string name)
        {
            var v = GetVariable(name);
            if (v == null) return Vector3.one;
            return v.GetVector3Value();
        }

        protected Vector3 GetVector2Variable(string name)
        {
            var v = GetVariable(name);
            if (v == null) return Vector2.one;
            return v.GetVector2Value();
        }

        protected AnimationCurve GetCurveVariable(string name)
        {
            var v = GetVariable(name, null, AnimationCurve.EaseInOut(0f, 1f, 1f, 1f));
            if (v == null) return null;
            return v.GetCurve();
        }

        #endregion


        #region Editor GUI Related Code


        private UnityEditor.SerializedObject _baseSO = null;


        public UnityEditor.SerializedObject baseSerializedObject
        {
            get
            {
                if (_baseSO == null || _baseSO.targetObject != this)
                {
                    _baseSO = new UnityEditor.SerializedObject(this);
                }

                return _baseSO;
            }
        }


        /// <summary> Base is checking Initialization! </summary>
        public virtual void InspectorGUI_Header(float animProgress, CustomModuleSet customModuleSet)
        {
            relevantSet = customModuleSet;
            CheckInitialization(customModuleSet);
        }

        /// <summary> [No need for base execution] Called only if GUIFoldable returns true </summary>
        public virtual void InspectorGUI_HeaderFoldown(CustomModuleSet customModuleSet)
        {

        }

        /// <summary> Base tries to display all ModuleVariables by default </summary>
        public virtual void InspectorGUI_ModuleBody(float progress, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, CustomModuleSet set)
        {
            for (int i = 0; i < set.ModuleVariables.Count; i++)
            {
                if (set.ModuleVariables[i].HideFlag) continue;
                set.ModuleVariables[i].DrawGUI();
            }
        }

        /// <summary> [No need for base execution] </summary>
        public virtual void SceneView_DrawSceneHandles(CustomModuleSet customModuleSet, float alphaAnimation = 1f, float progress = 0f)
        {
        }

        #endregion


        #region Global Modules Utilities

        public static List<Type> GetCustomModulesTypes()
        {
            Type baseType = typeof(ADCustomModuleBase);

            List<Type> types = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try { types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray()); }
                catch (ReflectionTypeLoadException) { }
            }

            List<Type> myTypes = new List<Type>();
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type == baseType) continue;
                myTypes.Add(type);
            }

            return myTypes;
        }


        #endregion

    }
}
