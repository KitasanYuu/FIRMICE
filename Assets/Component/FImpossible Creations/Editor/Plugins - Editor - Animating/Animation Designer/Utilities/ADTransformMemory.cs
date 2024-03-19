using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    /// <summary>
    /// Class containing information about assigned transform for a modifier to restore
    ///  it when starting work back on the animation design after closing the editor
    /// </summary>
    [System.Serializable]
    public class ADTransformMemory
    {
        public string ID = "";
        public string DisplayName = "";
        public string Tooltip = "";
        [SerializeField] private int nameHash;
        public int NameHash { get { return nameHash; } }

        public void Allocate(AnimationDesignerSave save, string nameID, string displayName, string tooltip)
        {
            ParentSave = save;
            ID = nameID;
            nameHash = ID.GetHashCode();
            DisplayName = displayName;
            Tooltip = tooltip;
        }

        [SerializeField] private AnimationDesignerSave ParentSave;

        public bool _S_IsArmatureElement = false;
        public string _S_ArmatureBoneName = "";
        public string _S_ArmatureParentName = "";
        public string _S_ParentBonePath;
        public int _S_ParentDepth = -1;

        public Vector3 _S_LastLocalPosition = Vector3.zero;
        public Quaternion _S_LastLocalRotation = Quaternion.identity;


        /// <summary> Initialized transform, gathered from scene armature </summary>
        public Transform Transform { get; private set; }
        public Transform Parent { get; private set; }


        // TODO: If not found, it will try to reconstruct transform setup.
        /// <summary>
        /// Initializing 'Transform' variable using remembered data and searching for correct transform reference
        /// </summary>
        public void InitializeReference(AnimationDesignerSave save)
        {
            if (save != null)
            {
                ParentSave = save;
                RefreshBoneReferenceBasingOnArmatureRelation();
            }
        }

        /// <summary>
        /// Set possible to get position/rotation correctly
        /// </summary>
        public bool IsSet
        {
            get
            {
                if (Transform) return true;
                if (Parent) return true;
                return false;
            }
        }

        public Vector3 Position
        {
            get
            {
                if (Transform) return Transform.position;
                if (Parent) return Parent.TransformPoint(_S_LastLocalPosition);
                return Vector3.zero;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                if (Transform) return Transform.rotation;
                if (Parent) return Parent.rotation * (_S_LastLocalRotation);
                return Quaternion.identity;
            }
        }

        public void DefineNewBoneArmatureRelation(Transform t)
        {

            if (t == null)
            {
                _S_ArmatureBoneName = "";
                _S_ArmatureParentName = "";
                _S_ParentBonePath = "";
                _S_IsArmatureElement = false;
                _S_ParentDepth = -1;
                Transform = t;
                Parent = null;
                return;
            }

            if (t.parent != null)
            {
                _S_ArmatureBoneName = t.name;
                _S_ArmatureParentName = t.parent.name;

                if (ParentSave)
                {
                    var ar = ParentSave.Armature;
                    Transform at = ar.GetBoneWithName(t.name);
                    _S_IsArmatureElement = at != null;
                    Transform par_at = ar.GetBoneWithName(t.parent.name);
                    if (!_S_IsArmatureElement) _S_IsArmatureElement = par_at != null;

                    if (_S_IsArmatureElement)
                    {
                        _S_ParentDepth = ADBoneReference.GetDepth(t.parent, ParentSave.SkelRootBone);
                        _S_ParentBonePath = AnimationUtility.CalculateTransformPath(t.parent, ParentSave.SkelRootBone);
                    }
                }

                Transform = t;
            }
            else
            {
                UnityEngine.Debug.Log("[Animation Designer] Bone transform requires parent!");
            }
        }

        public void UpdateMemory()
        {
            if (ParentSave == null) return;
            if (Transform == null) return;
            _S_LastLocalPosition = Transform.localPosition;
            _S_LastLocalRotation = Transform.localRotation;
        }

        void RefreshBoneReferenceBasingOnArmatureRelation()
        {
            if (ParentSave == null) return;
            if (ParentSave.Armature == null) return;
            if (ParentSave.Armature.RootBoneReference == null) return;
            if (ParentSave.Armature.RootBoneReference.TempTransform == null) return;

            if (!string.IsNullOrWhiteSpace(_S_ArmatureParentName))
            {
                if (!string.IsNullOrWhiteSpace(_S_ParentBonePath))
                    Parent = ADBoneReference.GetTransformUsingPath(ParentSave, _S_ParentBonePath);

                if (Parent == null)
                    Parent = ParentSave.Armature.GetBoneWithName(_S_ArmatureParentName);

                if (Parent == null)
                    Parent = ParentSave.SearchForBoneInAllAnimatorChildren(_S_ArmatureParentName);
            }

            if (!string.IsNullOrWhiteSpace(_S_ArmatureBoneName))
            {
                if (Parent) Transform = Parent.Find(_S_ArmatureBoneName);
                else
                {
                    Transform = ParentSave.Armature.GetBoneWithName(_S_ArmatureBoneName);
                    if (!Transform) Transform = ParentSave.SearchForBoneInAllAnimatorChildren(_S_ArmatureBoneName);
                }
            }
        }



            string selectorHelperId = "";
        static GUIContent _gc = null;
        /// <summary> Returns true when changed </summary>
        public bool DrawGUI(bool updateMemory = true, bool foldoutForAllBones = true)
        {
            bool changed = false;
            if (_gc == null) _gc = new GUIContent(ID);

            _gc.text = String.IsNullOrWhiteSpace(DisplayName) ? ID : DisplayName;
            _gc.tooltip = Tooltip;

            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 140;
            Transform preTr = Transform;
            preTr = (Transform)EditorGUILayout.ObjectField(_gc, preTr, typeof(Transform), true);
            EditorGUIUtility.labelWidth = 0;


            if (!Transform) if (IsSet)
                {
                    EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "No transform reference found, but computing position/rotation basing on the skeleton bone parenting"), GUILayout.Width(18), GUILayout.Height(16));
                }


            // Foldout button for searchbox
            if (ParentSave != null)
            {

                if (Searchable.IsSetted)
                    if (selectorHelperId != "")
                        if (selectorHelperId == "mem"+ID)
                        {
                            object g = Searchable.Get();

                            if (g != null) if (g is Transform) preTr = (g as Transform);

                            selectorHelperId = "";
                        }

                GUILayout.Space(6);

                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(18)))
                {
                    selectorHelperId = "mem" + ID;
                    AnimationDesignerWindow.ShowBonesSelector("Choose Your Character Model Bone", foldoutForAllBones ? ParentSave.GetAllAnimatorTransforms : ParentSave.GetAllArmatureBonesList, AnimationDesignerWindow.GetMenuDropdownRect(), true);
                }
            }



            if (preTr != Transform)
            {
                changed = true;
                DefineNewBoneArmatureRelation(preTr);
                if (ParentSave) ParentSave._SetDirty();
            }


            EditorGUILayout.EndHorizontal();


            if (Transform == null)
                if (IsSet)
                {
                    _gc.text += " PARENT:";
                    Parent = (Transform)EditorGUILayout.ObjectField(_gc, Parent, typeof(Transform), true);
                    _S_LastLocalPosition = EditorGUILayout.Vector3Field("Local Offset:", _S_LastLocalPosition);
                    _S_LastLocalRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Local Rotation:", _S_LastLocalRotation.eulerAngles));
                    if ( Parent == null) DefineNewBoneArmatureRelation(null);
                }

            //Parent = (Transform)EditorGUILayout.ObjectField("PARENT", Parent, typeof(Transform), true);
            //_S_ArmatureBoneName = EditorGUILayout.TextField("_S_ArmatureBoneName", _S_ArmatureBoneName);
            //_S_ArmatureParentName = EditorGUILayout.TextField("_S_ParentBoneName", _S_ArmatureParentName);
            //EditorGUILayout.LabelField("IS Set: " + IsSet);

            if ( updateMemory)
            {
                UpdateMemory();
            }

            return changed;
        }


    }
}