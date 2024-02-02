using UnityEngine;
using AvatarMain;
using playershooting;
using BattleShoot;
using CustomInspector;
using UnityEngine.InputSystem.XR.Haptics;

public class WeaponShooter : MonoBehaviour
{
    [ReadOnly, SerializeField] private bool UsingAIControl;
    [ReadOnly, SerializeField] private bool UsingMasterControl;
    [SerializeField] private bool Semi;
    public GameObject Shooter;
    // 子弹预制件或游戏对象
    [SerializeField] private GameObject bulletPrefab;
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

    private float PreviousBulletSpeed;

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
        PreviousBulletSpeed = bulletspeed;
        ComponemetInit();
        SRRSelecting();
    }

    // Update is called once per frame
    void Update()
    {
        ComponentRetake();
        BulletSpeedWatcher();
        FireAction();
    }

    private void FireAction()
    {
        if (Shooter != null)
        {
            // 开火
            if ((UsingAIControl && shootController.Fire && shootController.isAiming) || (UsingMasterControl && tpsShootController.Fire && tpsShootController.isAiming))
            {
                // 获取当前时间
                float currentTime = Time.time;

                if (currentTime - lastShootTime > fireRate)
                {
                    //// 在这里执行射线投射
                    //Ray shootRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    //if (Physics.Raycast(shootRay, out RaycastHit shootRaycastHit))
                    //{
                    //    // 获取击中点的坐标
                    //    Vector3 hitPoint = shootRaycastHit.point;

                    //    // 生成特效
                    //    Instantiate(vfxHitYellow, mouseWorldPosition, Quaternion.identity);

                    //    // 在这里处理射击命中的逻辑，例如对击中物体造成伤害或触发其他效果等
                    //    Debug.Log("射击命中：" + shootRaycastHit.collider.gameObject.name + "，击中坐标：" + hitPoint);
                    //}

                    Vector3 aimDir = Vector3.zero;

                    if (Tpsshootcontroller)
                        aimDir = (tpsShootController.TmouseWorldPosition - spawnBulletPosition.position).normalized;
                    else if (shootcontroller && !shootController.UsingTrajectoryPredict)
                        aimDir = (shootController.hitpoint - spawnBulletPosition.position).normalized;
                    else if (shootcontroller && shootController.UsingTrajectoryPredict)
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

                    // 生成子弹实例
                    GameObject bulletInstance = Instantiate(bulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                    // 获取子弹脚本并设置速度
                    Bullet bullet = bulletInstance.GetComponent<Bullet>();
                    if (bullet != null)
                    {

                        bullet.SetDamage(BulletDamage);
                        bullet.SetRayLength(bulletspeed * SRR);
                        bullet.SetBulletSpeed(bulletspeed);
                        bullet.SetBulletHitLayer(DestoryLayer);
                        bullet.SetFatherObj(Shooter);
                        //Debug.Log(bulletspeed * SRR);
                        //Debug.LogError("BulletSpwaned");
                    }
                    else
                    {
                        Debug.LogError("BulletTest component not found on instantiated object.");
                    }

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
        }
    }

    private void BulletSpeedWatcher()
    {
        if(PreviousBulletSpeed != bulletspeed)
        {
            SRRSelecting();
            PreviousBulletSpeed = bulletspeed;
        }
    }

    private void ComponentRetake()
    {
        if(Shooter == null)
        {
           Shooter = FindFatherObj(true);
            if(Shooter != null)
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

    private void ComponemetInit()
    {
        Basicinput = Shooter.TryGetComponent<BasicInput>(out _input);
        Tpsshootcontroller = Shooter.TryGetComponent<TPSShootController>(out tpsShootController);
        shootcontroller = Shooter.TryGetComponent<ShootController>(out shootController);

        if (Tpsshootcontroller && Basicinput)
        {
            UsingMasterControl = true;
            UsingAIControl = false;
            Debug.Log("BulletSpwanInitSuccess!"+gameObject+ "CurrentUsingTPSMasterControl");
        }

        if (shootcontroller)
        {
            UsingAIControl = true;
            UsingMasterControl = false;
            Debug.Log("BulletSpwanInitSuccess!"+gameObject+ "CurrentUsingAIControl");
        }
    }
}
