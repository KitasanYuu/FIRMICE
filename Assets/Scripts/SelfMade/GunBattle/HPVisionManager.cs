using BattleHealth;
using CustomInspector;
using DataManager;
using System.Collections.Generic;
using System.Data.Common;
using TestField;
using Unity.VisualScripting;
using UnityEngine;

public class HPVisionManager : MonoBehaviour
{
    public Camera mainCamera; // 主相机
    [SerializeField]private List<HealthBar> healthBars = new List<HealthBar>(); // 存储所有血条的列表
    [SerializeField] private List<EliteHealthBar> ehealthBars = new List<EliteHealthBar>();
    public LayerMask ignoreLayers; // 在射线检测中要忽略的层
    public float checkRadius = 0.5f; // 胶囊体检测的半径
    public float Offset = 1;

    private DataMaster DM = new DataMaster();
    private LocalDataSaver LDS = new LocalDataSaver();
    private ResourceReader RR = new ResourceReader();
    private Canvas RenderCanvas;
    private HPVisionManager HVM;

    [SerializeField, ReadOnly] private GameObject HealthBarPrefab;
    [SerializeField, ReadOnly] private GameObject EHealthBarPrefab;


    private void Awake()
    {
        ComponentInit();
    }

    void Start()
    {
        ResourceInit();
        // 自动收集所有子物体中的HealthBar组件
        healthBars.AddRange(GetComponentsInChildren<HealthBar>());
    }

    void Update()
    {
        ComponentInit();
        RayCastDetect();
    }

    private void RayCastDetect()
    {
        // 对healthBars列表进行排序，根据与相机的距离，距离越近的越后面
        healthBars.Sort((a, b) =>
        {
            float distanceA = Vector3.Distance(mainCamera.transform.position, a.HPAnchor.position);
            float distanceB = Vector3.Distance(mainCamera.transform.position, b.HPAnchor.position);
            return distanceB.CompareTo(distanceA);
        });

        foreach (var healthBar in healthBars)
        {
            Transform npcTransform = healthBar.HPAnchor;
            if (npcTransform == null) continue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(npcTransform.position);

            // 检查是否在渲染范围内
            bool isWithinRenderView = screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= Screen.width && screenPos.y >= 0 && screenPos.y <= Screen.height;

            if (!isWithinRenderView)
            {
                healthBar.SetVisibility(false);
                continue;
            }
            else
            {
                healthBar.SetVisibility(true);
            }

            Vector3 start = mainCamera.transform.position;
            Vector3 end = npcTransform.position;
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            Vector3 capsulePointA = start + direction.normalized * checkRadius;
            Vector3 capsulePointB = end - direction.normalized * checkRadius;

            float ScaleFactor = healthBar.ScaleFactor;
            float FinalRadius = checkRadius * ScaleFactor * Offset;
            // 使用胶囊体进行射线检测，判断是否有物体阻挡视线
            bool isBlocked = Physics.CheckCapsule(capsulePointA, capsulePointB, FinalRadius, ~ignoreLayers);

            // 调整healthBar的层级顺序，使得距离越近的血条越先渲染
            healthBar.transform.SetAsLastSibling();

            // 根据是否被阻挡来设置血条的显示状态
            healthBar.SetVisibility(!isBlocked);
        }

        // 现在遍历ehealthBars进行处理
        foreach (var ehealthBar in ehealthBars)
        {
            // 设置elite血条的可见性或其他属性
            ehealthBar.SetVisibility(true);

            // 将elite血条设置为在其父级中的最后一个子对象，确保它在最上层渲染
            ehealthBar.transform.SetAsLastSibling();
        }
    }

    private void ResourceInit()
    {
         HVM = GetComponent<HPVisionManager>();
        EHealthBarPrefab = RR.GetGameObject("HealthBar", "EliteHealthBarPrefab");
        HealthBarPrefab = RR.GetGameObject("HealthBar", "HealthBarPrefab");
    }

    private void ComponentInit()
    {
        if(RenderCanvas == null)
        {
            RenderCanvas = GetComponentInParent<Canvas>();
        }

        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }
    }

    public void ObjectHPRegister(GameObject RegisteObject)
    {
        VirtualHP VHP = RegisteObject.GetComponent<VirtualHP>();

        GameObject hpbar = null;



        int ActorSP = LDS.GetActorSP(RegisteObject);

        if(ActorSP == 0)
        {
            hpbar = Instantiate(HealthBarPrefab, gameObject.transform);
            VHP.SetRegistResult(true);
            hpbar.name = "N-" + RegisteObject.name;

            HealthBar healthBar = hpbar.GetComponent<HealthBar>();

            healthBar.SetParameter(RegisteObject, VHP, VHP.HPAnchor.transform, mainCamera, RenderCanvas);
            VHP.SetHealthBar(healthBar);
            
        }
        
        if(ActorSP == 1)
        {
            hpbar = Instantiate(EHealthBarPrefab, gameObject.transform);
            VHP.SetRegistResult(true);
            hpbar.name = "E-" + RegisteObject.name;

            EliteHealthBar ehealthBar = hpbar.GetComponent<EliteHealthBar>();

            ehealthBar.SetParameter(RegisteObject,VHP,HVM);

            VHP.SetEHealthBar(ehealthBar);
        }
    }

    public void ParameterSetProgress(GameObject RequestObject,GameObject RegisterObject)
    {
        EliteHealthBar EHealthBar = null;
        HealthBar healthBar = null;

        int ActorSP = LDS.GetActorSP(RegisterObject);

        if (ActorSP == 1)
        {
            EHealthBar = RequestObject.GetComponent<EliteHealthBar>();
        }
        else if(ActorSP == 0)
        {
            healthBar = RequestObject.GetComponent<HealthBar>();
        }
            
        string name = LDS.GetActorName(RegisterObject);

        Color HPBarColor = Color.white;
        Color LevelBGImageColor = Color.white;

        int camp = LDS.GetActorCamp(RegisterObject);
        switch (camp)
        {
            case 0:
                Debug.LogError(RegisterObject.name + "Regist HP Failure,Because Camp Error!");
                return;
            case 1:
                HPBarColor = RR.GetColor("AllyColor");
                LevelBGImageColor = RR.GetColor("AllyBGImageColor");
                break;
            case 2:
                HPBarColor = RR.GetColor("EnemyColor");
                LevelBGImageColor = RR.GetColor("EnemyBGImageColor");
                break;
            case 3:
                HPBarColor = RR.GetColor("NeutralColor");
                LevelBGImageColor = RR.GetColor("NeutralBGImageColor");
                break;
        }

        if (ActorSP == 0)
        {
            healthBar.SetHPColor(HPBarColor);
            healthBar.SetLevelBackGroundColor(LevelBGImageColor);
            healthBar.SetName(name);
            healthBar.SetInitBarParameter(1, 1);
        }
        else if(ActorSP == 1)
        {
            string elitetitle = LDS.GetActorSPTitle(RegisterObject);

            EHealthBar.SetEName(name);
            EHealthBar.SetEliteTitle(elitetitle);
            //EHealthBar.SetHPColor(HPBarColor);
            //EHealthBar.SetNameColor(HPBarColor);
            //EHealthBar.SetTitleColor(HPBarColor);
            EHealthBar.SetInitBarParameter(1, 1);
        }
    }

    public void ObjectHPUnRegister(HealthBar healthBart)
    {
        if(healthBars.Contains(healthBart))
            healthBars.Remove(healthBart);
    }

    public void RegisterHealthBar(HealthBar healthBar)
    {
        if (!healthBars.Contains(healthBar))
        {
            healthBars.Add(healthBar);
        }
    }

    public void UnregisterHealthBar(HealthBar healthBar)
    {
        if (healthBars.Contains(healthBar))
        {
            healthBars.Remove(healthBar);
        }
    }

    public void RegisterEliteHealthBar(EliteHealthBar EhealthBar)
    {
        if (!ehealthBars.Contains(EhealthBar))
        {
            ehealthBars.Add(EhealthBar);
        }
    }

    public void UnregisterEliteHealthBar(EliteHealthBar EhealthBar)
    {
        if (ehealthBars.Contains(EhealthBar))
        {
            ehealthBars.Remove(EhealthBar);
        }
    }

    #region 绘制Gizmos
#if UNITY_EDITOR
    // 用于在Unity编辑器中绘制Gizmos的方法
    void OnDrawGizmos()
    {
        if (mainCamera == null || healthBars == null) return;

        foreach (var healthBar in healthBars)
        {
            if (healthBar == null || healthBar.HPAnchor == null) continue;

            Vector3 start = mainCamera.transform.position;
            Vector3 end = healthBar.HPAnchor.position;
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            float ScaleFactor = healthBar.ScaleFactor;
            float FinalRadius = checkRadius * ScaleFactor * Offset;
            Vector3 capsulePointA = start + direction.normalized * FinalRadius;
            Vector3 capsulePointB = end - direction.normalized * FinalRadius;

            // 设置Gizmo颜色为红色，表示射线检测路径
            Gizmos.color = Color.red;

            // 绘制胶囊体表示的射线路径
            Gizmos.DrawLine(start, end);

            // 绘制胶囊体的两个端点以可视化检测范围
            Gizmos.DrawWireSphere(capsulePointA, FinalRadius);
            Gizmos.DrawWireSphere(capsulePointB, FinalRadius);
        }
    }
#endif
    #endregion
}
