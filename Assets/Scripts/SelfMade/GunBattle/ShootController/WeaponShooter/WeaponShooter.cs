using UnityEngine;
using AvatarMain;
using playershooting;
using BattleShoot;
using CustomInspector;
using BattleHealth;
using System.Linq.Expressions;
using Unity.VisualScripting;
using System.Collections;

public class WeaponShooter : MonoBehaviour
{
    [ReadOnly, SerializeField] private bool UsingAIControl;
    [ReadOnly, SerializeField] private bool UsingMasterControl;

    [SerializeField] private bool RayMethod = true;
    [SerializeField] private bool InstanceMethod;
    [Space2(20)]

    [SerializeField] private bool Semi;
    [SerializeField] private int TempAmmoTotal;
    [SerializeField] private bool LimitAmmo = true;
    [SerializeField, ShowIf(nameof(LimitAmmo))] private int defaultBulletCount;
    [ReadOnly, SerializeField, ShowIf(nameof(LimitAmmo))] private int CurrentBulletCount;

    [Space2(20)]
    public GameObject Shooter;
    // 子弹预制件或游戏对象
    [SerializeField] private GameObject bulletPrefab;
    //子弹命中后的特效
    [SerializeField] private GameObject VFXHitEffect;
    // 子弹生成位置
    [SerializeField] private Transform spawnBulletPosition;
    // 子弹生成后的速度
    [SerializeField] public float bulletspeed;
    [SerializeField] public LayerMask DestoryLayer;
    [SerializeField] public float BulletDamage;
    //子弹速度与射线检测的比值 射线长度 = 子弹速度*SRR
    private float CSRR = 0.0385f;
    private float LSRR = 0.05f;
    private float SRR;

    float lastShootTime = 0f;
    public float fireRate = 0.5f; // 0.5秒为例，可以根据需要调整射速
    private bool CanFireNow = false;

    private float PreviousBulletSpeed = 0;

    private bool Reloading = false;
    private bool reloadingInProgress = false;
    public float reloadDuration = 2.0f; // 将重新加载的持续时间设为公共属性
    public float FastreloadDuration = 1.0f; // 将重新加载的持续时间设为公共属性

    //获取脚本
    private BasicInput _input;
    private ShootController shootController;
    private TPSShootController tpsShootController;
    private bool Basicinput;
    private bool Tpsshootcontroller;
    private bool shootcontroller;

    [HideInInspector]
    public Vector3 PredictedAimPoint;

    // Start is called before the first frame update
    void Start()
    {
        ParameterInit();
        ComponemetInit();
        SRRSelecting();

    }

    // Update is called once per frame
    void Update()
    {
        ComponentRetake();
        BulletSpeedWatcher();
        FireAction();
        ReloadProgress();
    }

    private void FireAction()
    {
        if (Shooter != null)
        {
            // 开火
            if ((UsingAIControl && shootController.Fire && shootController.isAiming) || (UsingMasterControl && tpsShootController.Fire && tpsShootController.isAiming))
            {
                if (CurrentBulletCount > 0)
                {
                    // 获取当前时间
                    float currentTime = Time.time;

                    if (currentTime - lastShootTime > fireRate || CanFireNow)
                    {
                        Vector3 aimDir = Vector3.zero;

                        if (Tpsshootcontroller)
                            aimDir = (tpsShootController.TmouseWorldPosition - spawnBulletPosition.position).normalized;
                        else if (shootcontroller && RayMethod && !shootController.UsingTrajectoryPredict)
                            aimDir = (shootController.hitpoint - spawnBulletPosition.position).normalized;
                        else if (shootcontroller && InstanceMethod && shootController.UsingTrajectoryPredict)
                        {
                            // 计算射击方向和距离
                            Vector3 shootDirection = shootController.hitpoint - spawnBulletPosition.position;
                            float shootDistance = shootDirection.magnitude;

                            // 计算子弹的飞行时间
                            float bulletTravelTime = shootDistance / bulletspeed;

                            // 预测目标位置，同时考虑 X、Y 和 Z 轴上的移动
                            Vector3 predictedTargetPosition = shootController.hitpoint + bulletTravelTime * shootController.hitpointVelocity;
                            PredictedAimPoint = predictedTargetPosition;

                            // 重新计算 aimDir
                            aimDir = (predictedTargetPosition - spawnBulletPosition.position).normalized;
                        }

                        if (InstanceMethod)
                        {
                            CurrentBulletCount--;
                            // 生成子弹实例
                            GameObject bulletInstance = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                            // 获取子弹脚本并设置速度
                            Bullet bullet = bulletInstance.GetComponent<Bullet>();
                            if (bullet != null)
                            {

                                bullet?.SetDamage(BulletDamage);
                                bullet?.SetRayLength(bulletspeed * SRR);
                                bullet?.SetBulletSpeed(bulletspeed);
                                bullet?.SetBulletHitLayer(DestoryLayer);
                                bullet?.SetFatherObj(Shooter);
                                bullet?.SetVFXHitEffect(VFXHitEffect);
                                //Debug.Log(bulletspeed * SRR);
                                //Debug.LogError("BulletSpwaned");
                            }
                            else
                            {
                                Debug.LogError("BulletTest component not found on instantiated object.");
                            }
                        }
                        else if (RayMethod)
                        {
                            // 在这里执行射线投射
                            Ray shootRay = new Ray(spawnBulletPosition.position, aimDir);
                            if (Physics.Raycast(shootRay, out RaycastHit shootRaycastHit))
                            {
                                // 获取击中点的坐标
                                Vector3 hitPoint = shootRaycastHit.point;
                                GameObject HitObject = shootRaycastHit.collider.gameObject;
                                RayDamegeIn(HitObject);
                                // 生成特效
                                Instantiate(VFXHitEffect, hitPoint, Quaternion.identity);
                                // 在这里处理射击命中的逻辑，例如对击中物体造成伤害或触发其他效果等
                                Debug.Log("射击命中：" + shootRaycastHit.collider.gameObject.name + "，击中坐标：" + hitPoint);
                                // 只在尚未命中过的情况下执行一次
                                CurrentBulletCount--;

                            }
                        }

                        CanFireNow = false;

                        if (Semi)
                        {
                            if (UsingMasterControl)
                            {
                                _input.shoot = false;
                            }
                        }


                        lastShootTime = currentTime;
                    }
                }
                else
                {
                    ReloadProgress(true);
                }
            }
        }
    }

    #region 子弹的Reload逻辑
    private void ReloadProgress(bool ReloadStart = false)
    {
        if (!Reloading && !reloadingInProgress && (Input.GetKeyDown(KeyCode.R) || ReloadStart))
        {
            if (CurrentBulletCount < defaultBulletCount)
            {
                StartCoroutine(ReloadCoroutine());
            }
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        Reloading = true;
        reloadingInProgress = true;

        int missingBulletCount = defaultBulletCount - CurrentBulletCount;

        if (TempAmmoTotal >= missingBulletCount)
        {
            if(CurrentBulletCount == 0)
            {
                // 模拟延迟后执行重新加载的逻辑
                yield return new WaitForSeconds(reloadDuration);

                TempAmmoTotal -= defaultBulletCount;
                CurrentBulletCount = defaultBulletCount;
            }
            else if(CurrentBulletCount > 0)
            {
                yield return new WaitForSeconds(FastreloadDuration);
                TempAmmoTotal -= missingBulletCount;
                CurrentBulletCount += missingBulletCount;
            }

        }
        else if (TempAmmoTotal > 0 && TempAmmoTotal < missingBulletCount)
        {
            if(CurrentBulletCount == 0)
            {
                // 模拟延迟后执行重新加载的逻辑
                yield return new WaitForSeconds(reloadDuration);

                CurrentBulletCount += TempAmmoTotal;
                TempAmmoTotal = 0;
            }
            else if(CurrentBulletCount > 0)
            {
                yield return new WaitForSeconds(FastreloadDuration);

                CurrentBulletCount += TempAmmoTotal;
                TempAmmoTotal = 0;
            }

        }

        CanFireNow = true;
        Reloading = false;
        reloadingInProgress = false;
    }

#endregion

    private void BulletRefresh(int newvalue)
    {
        CurrentBulletCount = newvalue;
    }

    private void RayDamegeIn(GameObject RayHitTarget)
    {
        VirtualHP virtualhp = RayHitTarget.GetComponent<VirtualHP>();
        if (virtualhp != null)
        {
            virtualhp.AddDamage(BulletDamage, Shooter); // 将伤害值传递给目标脚本的方法
        }
    }

    //用来追踪BulletSpeed有没有在途中变化
    private void BulletSpeedWatcher()
    {
        if (PreviousBulletSpeed != bulletspeed)
        {
            SRRSelecting();
            PreviousBulletSpeed = bulletspeed;
        }
    }

    private void ComponentRetake()
    {
        if (Shooter == null)
        {
            Shooter = FindFatherObj(true);
            if (Shooter != null)
            {
                ComponemetInit();
                SRRSelecting();
            }

        }
    }

    private void SRRSelecting()
    {
        if (bulletspeed < 1000f)
        {
            SRR = CSRR;
        }
        else if (bulletspeed >= 1000 && bulletspeed <= 2000)
        {
            SRR = LSRR;
        }
        else
        {
            SRR = bulletspeed;
        }
    }

    //寻找带有ShootController的Shooter
    public GameObject FindFatherObj(bool Start = false)
    {
        if (Start)
        {
            // 从当前物体开始查找
            GameObject currentObject = gameObject;

            // 在当前物体以及其父级中查找
            while (currentObject != null)
            {
                // 在当前物体上查找TPSShootController脚本
                TPSShootController tpsShootController = currentObject.GetComponent<TPSShootController>();
                if (tpsShootController != null)
                {
                    // 如果找到，则返回当前物体
                    return currentObject;
                }

                // 在当前物体上查找ShootController脚本
                ShootController shootController = currentObject.GetComponent<ShootController>();
                if (shootController != null)
                {
                    // 如果找到，则返回当前物体
                    return currentObject;
                }

                // 继续向上查找
                currentObject = currentObject.transform.parent?.gameObject;
            }
        }
        // 如果未找到任何包含所需脚本的物体，则返回null
        return null;
    }

    private void ParameterInit()
    {
        CurrentBulletCount = defaultBulletCount;
        PreviousBulletSpeed = bulletspeed;
    }

    private void ComponemetInit()
    {
        if (Shooter != null)
        {
            Basicinput = Shooter.TryGetComponent<BasicInput>(out _input);
            Tpsshootcontroller = Shooter.TryGetComponent<TPSShootController>(out tpsShootController);
            shootcontroller = Shooter.TryGetComponent<ShootController>(out shootController);
        }

        if (Tpsshootcontroller && Basicinput)
        {
            UsingMasterControl = true;
            UsingAIControl = false;
            Debug.Log("BulletSpwanInitSuccess!" + "  " + gameObject.name + "  " + "CurrentUsingTPSMasterControl");
        }

        if (shootcontroller)
        {
            UsingAIControl = true;
            UsingMasterControl = false;
            Debug.Log("BulletSpwanInitSuccess!"+ "  " + gameObject.name + "  " + "CurrentUsingAIControl");
        }
    }
}
