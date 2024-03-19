using FIMSpace.FTools;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class ADArmatureLimb
    {

        bool _playError = true;

        public void ResetLimbComponentsState()
        {
            for (int i = 0; i < Bones.Count; i++)
            {
                if (Bones[i].T == null) continue;
                Bones[i].ResetState();
            }

        }


        #region Elasticness

        internal void CheckLimbElasticnessComponentsInitialization(AnimationDesignerSave save, bool reInitialize)
        {
            if (_playError)
            {
                RefresTransformReferences(save.Armature);
            }

            _playError = false;

            for (int i = 0; i < Bones.Count; i++)
            {
                if (Bones[i].T == null) { _playError = true; return; }
                Bones[i].CheckForElasticnessInitialization(reInitialize);
            }

            for (int i = 0; i < Bones.Count - 1; i++)
            {
                Bones[i].CheckForInitialization_SetRelation(Bones[i + 1]);
            }

            if (Bones.Count >= 2) Bones[Bones.Count - 1].MotionMuscle.SetParent(Bones[Bones.Count - 2].MotionMuscle);
        }


        internal void ElasticnessPreLateUpdate(ADClipSettings_Elasticness elastic)
        {
            if (_playError) return;

            for (int i = 0; i < Bones.Count; i++)
            {
                if (Bones[i].MotionMuscle == null) { _playError = true; break; }
                Bones[i].MotionMuscle.CaptureSourceAnimation();
            }
        }


        internal void ElasticnessComponentsLateUpdate(ADClipSettings_Elasticness elastic, ADClipSettings_Main main, float dt, float progr)
        {
            if (_playError) return;

            float mainMul = 1f;
            if (main != null) mainMul = main.ElasticnessEvaluation.Evaluate(progr);

            ADClipSettings_Elasticness.ElasticnessSet set = null;

            for (int l = 0; l < elastic.LimbsSets.Count; l++)
            {
                if (elastic.LimbsSets[l].Index == Index)
                {
                    set = elastic.LimbsSets[l];
                    break;
                }
            }

            if (set != null && set.Enabled)
            {

                #region Backup

                //for (int i = 0; i < Bones.Count; i++)
                //{
                //    Bones[i].UpdateElasticnessParams(set, dt, progr, AnimationBlend);
                //    Bones[i].LateUpdateSimulationFirst(set, dt, progr, AnimationBlend);
                //    Bones[i].LateUpdateSimulationSecond(set, dt, progr, AnimationBlend);
                //    Bones[i].LateUpdateSimulationLast(set, dt, progr, AnimationBlend);
                //}

                #endregion

                for (int i = 0; i < Bones.Count; i++)
                {
                    Bones[i].UpdateElasticnessParams(set, dt, progr, AnimationBlend * mainMul);
                }

                if (set.MotionInfluence < 1f) set.TempInfluenceOffset = main.MotionInfluenceOffset * (1f - set.MotionInfluence);
                else set.TempInfluenceOffset = Vector3.zero;

                //if (set.RotationInfluence < 1f) set.TempRotInfluenceOffset = Quaternion.LerpUnclamped(main.MotionRotationInfluenceOffset, Quaternion.identity, set.RotationInfluence);
                //else set.TempRotInfluenceOffset = Quaternion.identity;

                if (Bones.Count > 0)
                {
                    float moveBlend = Bones[0].GetBlending(set, progr, set.OnMoveBlend) * AnimationBlend * mainMul;

                    if (moveBlend > 0f)
                    {
                        #region Movement Based Elasticness

                        FElasticTransform bone = Bones[0].MotionMuscle;
                        while (bone != null)
                        {
                            if (set.MotionInfluence < 1f) bone.PositionMuscle.MotionInfluence(set.TempInfluenceOffset);
                            bone.UpdateElasticPosition(dt);
                            bone = bone.GetElasticChild();
                        }

                        bone = Bones[0].MotionMuscle;
                        int ind = 0;
                        while (bone != null)
                        {
                            bone.UpdateElasticRotation(moveBlend * Bones[ind].ElasticnessBlend);
                            //UnityEngine.Debug.DrawLine(bone.sourceAnimationPosition, bone.ProceduralPosition, Color.green, 0.11f);

                            if (set.MoveStretch > 0f)
                                bone.transform.position = Vector3.LerpUnclamped(bone.transform.position, bone.BlendVector(bone.ProceduralPosition, (set.MoveStretch * 1f)), set.MoveStretch * 1f);

                            bone = bone.GetElasticChild();
                            ind += 1;
                        }

                        #endregion
                    }
                }

                for (int i = 0; i < Bones.Count; i++)
                {
                    Bones[i].ElasticnessLateUpdateSimulationLast(set, dt, progr, AnimationBlend * mainMul);
                }

            }
        }


        #endregion


        #region IK is in ADArmatureLimb.IK.cs file

        #endregion


        #region Blending


        internal void CheckComponentsBlendingInitialization(bool reInitialize)
        {
            //_playError = false;

            if (reInitialize)
                for (int i = 0; i < Bones.Count; i++)
                {
                    Bones[i].SaveInitialCoords();
                    //if (Bones[i].T == null) { _playError = true; return; }
                    //Bones[i].CheckForInitialization();
                }

        }

        internal void ComponentBlendingPreLateUpdateCalibrate(ADClipSettings_Morphing blend)
        {
            if (_playError) return;

            if (Calibrate)
            {
                for (int i = 0; i < Bones.Count; i++)
                {
                    Bones[i].T.localRotation = Bones[i].InitLocalRot;
                    Bones[i].T.localPosition = Bones[i].InitLocalPos;
                }
            }

            //for (int i = 0; i < Bones.Count; i++)
            //{
            //    Bones[i].MotionMuscle.CaptureSourceAnimation();
            //}
        }

        internal void ComponentsBlendingLateUpdate(ADClipSettings_Morphing blend, float dt, float progr)
        {
            if (_playError) return;

            //ADClipSettings_Elasticness.ElasticnessSet set = null;

            //for (int l = 0; l < elastic.LimbsSets.Count; l++)
            //{
            //    if (elastic.LimbsSets[l].Index == Index)
            //    {
            //        set = elastic.LimbsSets[l];
            //        break;
            //    }
            //}

        }

        #endregion


    }
}