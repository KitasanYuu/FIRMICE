using FIMSpace.FEditor;
using FIMSpace.FTools;
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class ADBoneID
    {
        public FElasticTransform MotionMuscle { get; private set; }
        public FMuscle_Quaternion RotationMuscle { get; private set; }
        public FMuscle_Eulers EulerAnglesMuscle { get; private set; }


        public void CheckForElasticnessInitialization(bool reInitialize)
        {
            bool reInitialization = false;
            if (MotionMuscle == null || MotionMuscle.transform == null || reInitialize)
            {
                AnimationDesignerWindow.ForceZeroFramePose();
                MotionMuscle = new FElasticTransform();
                MotionMuscle.Initialize(T);
                reInitialization = true;
            }

            if (RotationMuscle == null || reInitialization)
            {
                if (!reInitialization) AnimationDesignerWindow.ForceZeroFramePose();
                RotationMuscle = new FMuscle_Quaternion();
                RotationMuscle.Initialize(T.rotation);
            }

            if (EulerAnglesMuscle == null || reInitialization)
            {
                if (!reInitialization) AnimationDesignerWindow.ForceZeroFramePose();
                EulerAnglesMuscle = new FMuscle_Eulers();
                EulerAnglesMuscle.Initialize(T.eulerAngles);
            }
        }

        internal void ResetState()
        {
            if (MotionMuscle != null) MotionMuscle.OverrideProceduralPositionHard(pos);
            if (MotionMuscle != null) MotionMuscle.OverrideProceduralRotation(rot);
            if (RotationMuscle != null) RotationMuscle.OverrideProceduralRotation(rot);
        }

        public void CheckForInitialization_SetRelation(ADBoneID child)
        {
            child.MotionMuscle.SetParent(MotionMuscle);
            MotionMuscle.SetChild(child.MotionMuscle);
        }

        public float GetBlending(ADClipSettings_Elasticness.ElasticnessSet elastic, float progr, float boneBlend)
        {
            return elastic.Blend * elastic.BlendEvaluation.Evaluate(progr) * boneBlend * ElasticnessBlend;
        }

        public void UpdateElasticnessParams(ADClipSettings_Elasticness.ElasticnessSet elastic, float dt, float progr, float boneBlend)
        {
            if (elastic.Enabled == false) return;

            float blend = GetBlending(elastic, progr, boneBlend);

            if (blend <= 0f) return;

            #region  Elasticness Params Update

            float motBlend = blend * elastic.OnMoveBlend;

            if (motBlend > 0f)
            {
                MotionMuscle.PositionMuscle.Acceleration = elastic.MoveRapidity * 10000f;
                MotionMuscle.PositionMuscle.AccelerationLimit = elastic.MoveRapidity * 5000f;

                MotionMuscle.RotationRapidness = 1f - elastic.MoveMildRotate;

                MotionMuscle.PositionMuscle.BrakePower = 1f - elastic.MoveSmoothing;
                MotionMuscle.PositionMuscle.Damping = elastic.MoveDamping * 40f;
            }


            float rotBlend = blend * elastic.RotationsBlend;

            if (rotBlend > 0f)
            {
                if (elastic.EulerMode)
                {
                    EulerAnglesMuscle.Acceleration = elastic.RotationsRapidity * 10000f;
                    EulerAnglesMuscle.AccelerationLimit = elastic.RotationsRapidity * 5000f;

                    EulerAnglesMuscle.BrakePower = 1f - elastic.RotationsSwinginess;
                    EulerAnglesMuscle.Damping = elastic.RotationsDamping * 40f;
                }
                else
                {
                    RotationMuscle.Acceleration = elastic.RotationsRapidity * 10000f;
                    RotationMuscle.AccelerationLimit = elastic.RotationsRapidity * 5000f;

                    RotationMuscle.BrakePower = 1f - elastic.RotationsSwinginess;
                    RotationMuscle.Damping = elastic.RotationsDamping * 40f;
                }
            }

            #endregion

        }


        #region Backup

        //public void ElasticnessLateUpdateSimulationFirst(ADClipSettings_Elasticness.ElasticnessSet elastic, float dt, float progr, float boneBlend)
        //{
        //    if (elastic.Enabled == false) return;

        //    float blend = GetBlending(elastic, progr, boneBlend);

        //    if (blend <= 0f) return;

        //    // Motion Based Elasticness
        //    float motBlend = blend * elastic.OnMoveBlend;

        //    if (motBlend > 0f)
        //    {
        //        if (elastic.MotionInfluence < 1f) MotionMuscle.PositionMuscle.MotionInfluence(elastic.TempInfluenceOffset);
        //        MotionMuscle.UpdateElasticPosition(dt);
        //    }

        //}

        //public void ElasticnessLateUpdateSimulationSecond(ADClipSettings_Elasticness.ElasticnessSet elastic, float dt, float progr, float boneBlend)
        //{
        //    if (elastic.Enabled == false) return;

        //    float blend = GetBlending(elastic, progr, boneBlend);

        //    if (blend <= 0f) return;

        //    float motBlend = blend * elastic.OnMoveBlend;

        //    if (motBlend > 0f)
        //    {
        //        MotionMuscle.UpdateElasticRotation(motBlend);
        //    }
        //}

        #endregion


        public void ElasticnessLateUpdateSimulationLast(ADClipSettings_Elasticness.ElasticnessSet elastic, float dt, float progr, float boneBlend)
        {
            if (elastic.Enabled == false) return;

            float blend = GetBlending(elastic, progr, boneBlend);

            if (blend <= 0f) return;

            // Rotation Based Elasticness
            float rotBlend = blend * elastic.RotationsBlend;

            if (rotBlend > 0f)
            {

                if (elastic.EulerMode)
                {
                    #region Euler Update

                    //EulerAnglesMuscle.MotionInfluence(elastic.TempRotInfluenceOffset.eulerAngles);
                    EulerAnglesMuscle.Update(dt, T.eulerAngles);

                    float blendC = rotBlend * boneBlend;

                    Quaternion targetRot = Quaternion.Euler(EulerAnglesMuscle.ProceduralEulerAngles);

                    if (blendC >= 1f) T.rotation = targetRot;
                    else T.rotation = Quaternion.LerpUnclamped(T.rotation, targetRot, blendC);

                    #endregion
                }
                else
                {
                    #region Quaternion Update

                    //if (elastic.TempRotInfluenceOffset != Quaternion.identity) RotationMuscle.MotionInfluence(elastic.TempRotInfluenceOffset);
                    RotationMuscle.UpdateEnsured(dt, T.transform.rotation);

                    float blendC = rotBlend * boneBlend;

                    if (blendC >= 1f) T.rotation = RotationMuscle.ProceduralRotation;
                    else T.rotation = Quaternion.LerpUnclamped(T.rotation, RotationMuscle.ProceduralRotation, blendC);

                    #endregion
                }
            }

        }

        internal void ClearElasticComponentsAndTransformRef()
        {
            MotionMuscle = null;
            RotationMuscle = null;
            EulerAnglesMuscle = null;
            GatheredTransform = null;
        }
    }
}