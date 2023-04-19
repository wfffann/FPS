using System.Collections;
using UnityEngine;

namespace Scripts.Weapon
{
    public class AssualtRifle : FireArms
    {
        private IEnumerator reloadAmmoCheckerCoroutine;


        private FPMouseLook mouseLook;


        //private bool isRunning;

        /*public bool isFiring { get; private set; }
        public void HoldTrigger()
        {
            isFire = true;
        }

        public void releaseTrigger()
        {
            isFire = false;
        }*/

        protected override void Awake()
        {
            base.Awake();
            reloadAmmoCheckerCoroutine = CheckReloadAmmoAnimationEnd();

            mouseLook = FindObjectOfType<FPMouseLook>();



        }

        public void Update()
        {
            isRunning = FPCharacterControllerMovement.Instance.isRunning;
            /*isInspecting = WeaponManager.Instance.isInspecting;*/
        }

        protected override void Shooting()
        {
            if (currentAmmo <= 0) return;

            //TODO：奔跑的时候不能开枪
            if (!IsAllowShooting() || isRealoding || isRunning) return;

            //枪焰特效
            muzzleParticle.Play();

            currentAmmo -= 1;
            gunAnimator.Play("Fire", isAiming ? 1 : 0, 0);

            //音效
            firearmsShootingAudioSource.clip = fireArmsAudioData.shootingAudioClip;
            firearmsShootingAudioSource.Play();

            //实例化子弹
            CreateBullet();

            //光
            StartCoroutine(Light());


            //弹壳特效
            //casingParticle.Play();
            CreatCasing();


            //后坐力
            mouseLook.FirngForTest();


            //更新计时器
            lastFireTime = Time.time;


            //弹壳音效
            int tmp_Index = Random.Range(0, casingSounds.Count);
            var tmp_casingClip = casingSounds[tmp_Index];
            casingAudioSource.clip = tmp_casingClip;
            casingAudioSource.PlayDelayed(1f);

        }

        //换弹
        protected override void Reload()
        {
            //更新动画层的权重
            gunAnimator.SetLayerWeight(2, 1);
            //根据当前的子弹数量判断执行的动画
            gunAnimator.SetTrigger(currentAmmo > 0 ? "ReloadLeft" : "ReloadOutOf");

            //音效
            firearmsReloadAudioSource.clip = currentAmmo > 0 ? fireArmsAudioData.reloadLeft : fireArmsAudioData.reloadOutOf;
            firearmsReloadAudioSource.Play();

            if (reloadAmmoCheckerCoroutine == null)
            {
                reloadAmmoCheckerCoroutine = CheckReloadAmmoAnimationEnd();
                StartCoroutine(reloadAmmoCheckerCoroutine);
            }
            else
            {
                StartCoroutine(reloadAmmoCheckerCoroutine);
                reloadAmmoCheckerCoroutine = null;
                reloadAmmoCheckerCoroutine = CheckReloadAmmoAnimationEnd();
                StartCoroutine(reloadAmmoCheckerCoroutine);
            }
        }

        //实例化子弹
        protected void CreateBullet()
        {
            GameObject tmp_Bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            //tmp_Bullet.AddComponent<Rigidbody>();

            //子弹的散射（圆周内
            tmp_Bullet.transform.eulerAngles += CalculateSpreadOffset();

            //获取impactAudioData
            tmp_Bullet.GetComponent<Bullet>().impactAudioData = impactAudioData;
            tmp_Bullet.GetComponent<Bullet>().bulletSpeed = 100;

            //销魂对象
            if (tmp_Bullet != null)
            {
                Destroy(tmp_Bullet, 5);
            }
        }

        //创建弹壳的实例以及协程音效
        protected void CreatCasing()
        {
            Instantiate(casingPrefab, casingPoint.position, casingPoint.rotation);
        }

        //协程延时灯光
        private IEnumerator Light()
        {
            light.transform.gameObject.SetActive(true);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

            light.transform.gameObject.SetActive(false);

        }

    }
}


