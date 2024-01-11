using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CameraTools;
using RenderTools;

namespace AvatarMain
{
    public class DistanceDither : MonoBehaviour
    {
        public GameObject MainTarget;
        public GameObject CloneTarget;
        public GameObject Camera;

        public float StartDitherDistance;
        public float CompleteDitherDistance;

        private AllinOneRenderTool allinoneTarget;
        private AllinOneRenderTool allinoneClone;
        private DistanceToScreen distanceToScreen;

        private float distancetocamera;
        private float DitherF;
        private float DitherCount;
        private float PreviousDitherCount;
        private float DitherChangeRate = 10f;
        private bool MainRenderClose;
        private bool DitherChange;

        // Start is called before the first frame update
        void Start()
        {
            ComponentInit();
            ParameterInit();
        }

        // Update is called once per frame
        void Update()
        {
            DistanceCalcu();
            RenderSwitch();
            SetDither();
        }

        private void ComponentInit()
        {
            allinoneTarget = MainTarget.GetComponent<AllinOneRenderTool>();
            allinoneClone = CloneTarget.GetComponent<AllinOneRenderTool>();
            distanceToScreen = Camera.GetComponent<DistanceToScreen>();
        }

        private void ParameterInit()
        {
            allinoneClone._ENABLEDITHER = true;
        }

        private void DistanceCalcu()
        {
            distancetocamera = distanceToScreen.DistanceToTarget;
            PreviousDitherCount = DitherCount;

            if (distancetocamera > StartDitherDistance)
            {
                MainRenderClose = false;
                DitherChange = false;
            }
            else if (distancetocamera < StartDitherDistance && distancetocamera > CompleteDitherDistance)
            {
                MainRenderClose = true;
                DitherChange = true;
                DitherCount = (distancetocamera - CompleteDitherDistance) / StartDitherDistance;
                //Debug.Log(DitherCount);
            }
            else if(distancetocamera < CompleteDitherDistance)
            {
                MainRenderClose = true;
                DitherChange = true;
                DitherCount = 0;
            }
            //Debug.LogWarning(MainRenderClose);

            DitherF = Mathf.Lerp(PreviousDitherCount,DitherCount,Time.deltaTime * DitherChangeRate);
        }

        private void RenderSwitch()
        {
            if(MainRenderClose)
            {
                allinoneTarget.SetRenderersEnabled(false);
                allinoneClone.SetRenderersEnabled(true);
            }
            else
            {
                allinoneTarget.SetRenderersEnabled(true);
                allinoneClone.SetRenderersEnabled(false);
            }
        }

        private void SetDither()
        {
            if(DitherChange)
            {
                //Debug.LogError(DitherF);
                allinoneClone._Dither = DitherF;
                allinoneClone.UpdateMaterialProperties();
            }
        }
    }
}
