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

            //TODO�����ܵ�ʱ���ܿ�ǹ
            if (!IsAllowShooting() || isRealoding || isRunning) return;

            //ǹ����Ч
            muzzleParticle.Play();

            currentAmmo -= 1;
            gunAnimator.Play("Fire", isAiming ? 1 : 0, 0);

            //��Ч
            firearmsShootingAudioSource.clip = fireArmsAudioData.shootingAudioClip;
            firearmsShootingAudioSource.Play();

            //ʵ�����ӵ�
            CreateBullet();

            //��
            StartCoroutine(Light());


            //������Ч
            //casingParticle.Play();
            CreatCasing();


            //������
            mouseLook.FirngForTest();


            //���¼�ʱ��
            lastFireTime = Time.time;


            //������Ч
            int tmp_Index = Random.Range(0, casingSounds.Count);
            var tmp_casingClip = casingSounds[tmp_Index];
            casingAudioSource.clip = tmp_casingClip;
            casingAudioSource.PlayDelayed(1f);

        }

        //����
        protected override void Reload()
        {
            //���¶������Ȩ��
            gunAnimator.SetLayerWeight(2, 1);
            //���ݵ�ǰ���ӵ������ж�ִ�еĶ���
            gunAnimator.SetTrigger(currentAmmo > 0 ? "ReloadLeft" : "ReloadOutOf");

            //��Ч
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

        //ʵ�����ӵ�
        protected void CreateBullet()
        {
            GameObject tmp_Bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            //tmp_Bullet.AddComponent<Rigidbody>();

            //�ӵ���ɢ�䣨Բ����
            tmp_Bullet.transform.eulerAngles += CalculateSpreadOffset();

            //��ȡimpactAudioData
            tmp_Bullet.GetComponent<Bullet>().impactAudioData = impactAudioData;
            tmp_Bullet.GetComponent<Bullet>().bulletSpeed = 100;

            //�������
            if (tmp_Bullet != null)
            {
                Destroy(tmp_Bullet, 5);
            }
        }

        //�������ǵ�ʵ���Լ�Э����Ч
        protected void CreatCasing()
        {
            Instantiate(casingPrefab, casingPoint.position, casingPoint.rotation);
        }

        //Э����ʱ�ƹ�
        private IEnumerator Light()
        {
            light.transform.gameObject.SetActive(true);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

            light.transform.gameObject.SetActive(false);

        }

    }
}


