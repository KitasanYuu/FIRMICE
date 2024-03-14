using BattleHealth;
using CustomInspector;
using DataManager;
using System.Collections.Generic;
using System.Data.Common;
using TestField;
using UnityEngine;

public class HPVisionManager : MonoBehaviour
{
    public Camera mainCamera; // 主相机
    [SerializeField]private List<HealthBar> healthBars = new List<HealthBar>(); // 存储所有血条的列表
    public LayerMask ignoreLayers; // 在射线检测中要忽略的层
    public float checkRadius = 0.5f; // 胶囊体检测的半径
    public float Offset = 1;

    private DataMaster DM = new DataMaster();
    private ResourceReader RR = new ResourceReader();

    [SerializeField, ReadOnly] private GameObject HealthBarPrefab;


    void Start()
    {
        ResourceInit();
        // 自动收集所有子物体中的HealthBar组件
        healthBars.AddRange(GetComponentsInChildren<HealthBar>());
    }

    void Update()
    {
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
    }

    private void ResourceInit()
    {
        HealthBarPrefab = RR.GetGameObject("HealthBar", "HealthBarPrefab");
    }

    public void ObjectHPRegister(GameObject RegisteObject)
    {
        Canvas ccanvas = GetComponent<Canvas>();
        VirtualHP VHP = RegisteObject.GetComponent<VirtualHP>();

        GameObject hpbar = null;
        Color HPBarColor = Color.white;
        Color LevelBGImageColor = Color.white;
        string name = DM.GetActorName(RegisteObject);
        int camp = DM.GetActorCamp(RegisteObject);
        switch (camp)
        {
            case 0:
                Debug.LogError(RegisteObject.name + "Regist HP Failure,Because Camp Error!");
                VHP.SetRegistResult(false);
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

        hpbar = Instantiate(HealthBarPrefab, gameObject.transform);

        hpbar.name = "Health "+RegisteObject.name;

        HealthBar healthBar = hpbar.GetComponent<HealthBar>();

        healthBar.SetHPColor(HPBarColor);
        healthBar.SetLevelBackGroundColor(LevelBGImageColor);
        healthBar.SetName(name);
        healthBar.SetInitBarParameter(1, 1);

        VHP.SetHealthBar(healthBar);
        healthBar.SetParameter(RegisteObject,VHP,VHP.HPAnchor.transform, mainCamera, ccanvas);
        VHP.SetRegistResult(true);
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
