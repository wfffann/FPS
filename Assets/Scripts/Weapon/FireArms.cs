using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Scripts.Weapon
{
    //这是一个抽象类，所有武器都会继承他
    public abstract class FireArms : MonoBehaviour, IWeapon
    {
        //枪械的组件
        public Transform muzzlePoint;
        public Transform casingPoint;

        public ParticleSystem muzzleParticle;
        //public ParticleSystem casingParticle;
        public GameObject casingPrefab;

        public GameObject light;

        //开枪的频率
        public float fireRate;
        protected float lastFireTime;

        //子弹
        public int ammoInMag = 30;
        public int maxAmmoCarried = 60;

        protected int currentAmmo;
        protected int currentMaxAmmoCarried;
        public int GetCurrentAmmoreturn => currentAmmo;
        public int GetCurrentMaxAmmoCarried => currentMaxAmmoCarried;

        public float spreadAngle;

        //特效
        public GameObject bulletPrefab;

        //动画机
        internal Animator gunAnimator;
        protected AnimatorStateInfo gunStateInfo;

        //音效
        public AudioSource firearmsShootingAudioSource;
        public AudioSource firearmsReloadAudioSource;
        public FireArmsAudioData fireArmsAudioData;
        public ImpactAudioData impactAudioData;

        //public AudioClip aimClip;

        public AudioSource casingAudioSource;
        public List<AudioClip> casingSounds = new List<AudioClip>();

        //bool
        public bool isAiming;
        public bool isRealoding;
        public bool isFire;
        public bool isInspecting;
        public bool isRunning;


        protected bool isHoldingTrigger;
        public bool IsHoldingTrigger => isHoldingTrigger;

        //FOV
        public Camera eyeCamera;
        public Camera gunCamera;
        protected float originFOV;

        protected float eyeOriginFOV;
        protected float gunOriginFOV;

        protected Transform gunCameraTransform;
        protected Vector3 originalEyePosition;



        //协程
        private IEnumerator doAimCoroutine;


        //Scope
        public List<ScopeInfo> scopeInfos;
        public ScopeInfo baseIronSight;
        protected ScopeInfo rigoutScopeInfo;

        protected virtual void Awake()
        {
            currentAmmo = ammoInMag;
            currentMaxAmmoCarried = maxAmmoCarried;
            gunAnimator = GetComponent<Animator>();
            originFOV = eyeCamera.fieldOfView;

            eyeOriginFOV = eyeCamera.fieldOfView;
            gunOriginFOV = gunCamera.fieldOfView;

            doAimCoroutine = DoAim();

            gunCameraTransform = gunCamera.transform;
            originalEyePosition = gunCameraTransform.localPosition;

            rigoutScopeInfo = baseIronSight;

            //isRunning = FPCharacterControllerMovement.Instance.isRunning;
        }

        public void DoAttack()
        {
            //使用子类的实现
            Shooting();
        }


        protected abstract void Shooting();
        protected abstract void Reload();
        //protected abstract void Aim();



        //开枪间隔
        protected bool IsAllowShooting()
        {
            // 715 1m
            // 715 / 60 = 11.7
            // 1 / 11.7
            return Time.time - lastFireTime > 1 / fireRate;
        }

        //子弹的散射
        protected Vector3 CalculateSpreadOffset()
        {
            float tmp_SpreadPercent = spreadAngle / eyeCamera.fieldOfView;
            return tmp_SpreadPercent * Random.insideUnitCircle;
        }


        //瞄准时FOV变化的协程
        protected IEnumerator DoAim()
        {
            while (true)
            {
                yield return null;

                //MainCamera
                float tmp_EyeCurrentFOV = 0;
                eyeCamera.fieldOfView = Mathf.SmoothDamp(eyeCamera.fieldOfView,
                    isAiming ? rigoutScopeInfo.eyeFOV : eyeOriginFOV,
                    ref tmp_EyeCurrentFOV, Time.deltaTime * 2);

                //GunCamera
                float tmp_GunCurrentFOV = 0;
                gunCamera.fieldOfView = Mathf.SmoothDamp(gunCamera.fieldOfView,
                    isAiming ? rigoutScopeInfo.gunFOV : gunOriginFOV,
                    ref tmp_GunCurrentFOV, Time.deltaTime * 2);

                //GunCamera.transform.localPosition
                Vector3 tmp_RefPosition = Vector3.zero;
                gunCameraTransform.localPosition = Vector3.SmoothDamp(gunCameraTransform.localPosition,
                    isAiming ? rigoutScopeInfo.gunCameraPosition : originalEyePosition,
                    ref tmp_RefPosition, Time.deltaTime * 2);
            }
        }

        //检测换弹是否完成的协程
        protected IEnumerator CheckReloadAmmoAnimationEnd()
        {
            while (true)
            {
                isRealoding = true;

                yield return null;
                //获取第三层动画机信息
                gunStateInfo = gunAnimator.GetCurrentAnimatorStateInfo(2);
                //寻找对应Tag的全部动画
                if (gunStateInfo.IsTag("ReloadAmmo"))
                {
                    //检测该动画执行的完成度
                    if (gunStateInfo.normalizedTime > 0.9f)
                    {
                        //换弹需求子弹数量
                        int tmp_NeedAmmoCount = ammoInMag - currentAmmo;
                        //剩余子弹数量
                        int tmp_RemainingAmmo = currentMaxAmmoCarried - tmp_NeedAmmoCount;

                        //更新当前子弹信息
                        if (tmp_RemainingAmmo <= 0) currentAmmo += currentMaxAmmoCarried;
                        else currentAmmo = ammoInMag;
                        currentMaxAmmoCarried = tmp_RemainingAmmo <= 0 ? 0 : tmp_RemainingAmmo;

                        isRealoding = false;

                        yield break;
                    }
                }
            }
        }

        //执行瞄准FOV协程和瞄准动画
        internal void Aiming(bool _isAiming)
        {
            //瞄准的音效
            /*firearmsReloadAudioSource.clip = aimClip;
            firearmsReloadAudioSource.Play();*/

            isAiming = _isAiming;
            gunAnimator.SetBool("Aim", isAiming);
            if (doAimCoroutine == null)
            {
                doAimCoroutine = DoAim();
                StartCoroutine(doAimCoroutine);
            }
            else
            {
                StopCoroutine(doAimCoroutine);
                doAimCoroutine = null;
                doAimCoroutine = DoAim();
                StartCoroutine(doAimCoroutine);
            }
        }

        internal void HoldTrigger()
        {
            DoAttack();
            isHoldingTrigger = true;
        }

        internal void ReleaseTrigger()
        {
            isHoldingTrigger = false;
        }

        internal void ReloadAmmo()
        {
            Reload();
        }


        //装备当前瞄具的数值
        internal void SetUpCarriedScope(ScopeInfo _scopeInfo)
        {
            rigoutScopeInfo = _scopeInfo;
        }





        [System.Serializable]
        public class ScopeInfo
        {
            public string ScopeName;
            public GameObject ScopeGameObject;
            public float eyeFOV;
            public float gunFOV;
            public Vector3 gunCameraPosition;
        }




    }
}


