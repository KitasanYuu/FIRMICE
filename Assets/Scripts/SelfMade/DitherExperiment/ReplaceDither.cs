using CameraTools;
using RenderTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceDither : MonoBehaviour
{
    public GameObject Camera;
    public GameObject TargetObject;

    public float StartDitherDistance;
    public float CompleteDitherDistance;

    private RendererCollector RC;
    private AllinOneRenderTool allinone;
    private DistanceToScreen distanceToScreen;

    private float distancetocamera;
    private float DitherF;
    private float DitherCount;
    private float PreviousDitherCount;
    private float DitherChangeRate = 10f;
    private bool MainRenderClose;
    private bool DitherChange;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        ComponentInit();
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
        RC = GetComponent<RendererCollector>();
        allinone = TargetObject.GetComponent<AllinOneRenderTool>();
        distanceToScreen = Camera.GetComponent<DistanceToScreen>();
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
            DitherCount = (distancetocamera - CompleteDitherDistance) / (StartDitherDistance - CompleteDitherDistance);
            //Debug.Log(DitherCount);
        }
        else if (distancetocamera < CompleteDitherDistance)
        {
            MainRenderClose = true;
            DitherChange = true;
            DitherCount = 0;
        }
        //Debug.LogWarning(MainRenderClose);

        DitherF = Mathf.Lerp(PreviousDitherCount, DitherCount, Time.deltaTime * DitherChangeRate);
    }

    private void RenderSwitch()
    {
        RC.SetRendererStatue(MainRenderClose, allinone);
    }

    private void SetDither()
    {
        if (DitherChange)
        {
            if(allinone.isRefreshing == false)
            {
                //Debug.LogError(DitherF);
                allinone._Dither = DitherF;
                allinone.UpdateMaterialProperties();
            }
        }
    }
}
