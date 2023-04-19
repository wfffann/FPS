using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Weapon
{
    public class Bullet : MonoBehaviour
    {
        //���
        //private Rigidbody bulletRigidbody;
        private Transform bulletTransform;
        public List<GameObject> ImpactPrefab;
        public ImpactAudioData impactAudioData;

        public CharacterStats characterStats;

        //�ӵ�����
        public float bulletSpeed;
        private Vector3 prevPosition;


        private void Start()
        {
            //bulletRigidbody = GetComponent<Rigidbody>()
            characterStats = GetComponent<CharacterStats>();
            bulletTransform = transform;
            prevPosition = bulletTransform.position;
        }

        //����Rigidbody
        /*private void FixedUpdate()
        {
            //bulletRigidbody.velocity = bulletTransform.forward * bulletSpeed * Time.fixedTime;
        }*/

        private void Update()
        {
            //������һ֡�ӵ���λ��
            prevPosition = bulletTransform.position;

            //�ӵ�λ�õĸ���
            bulletTransform.Translate(0, 0, bulletSpeed * Time.deltaTime);

            //���ӵ���һ֡����һ֡�������߼���ڼ���ײ����Ϣ 
            if (Physics.Raycast(prevPosition,
                (bulletTransform.position - prevPosition).normalized,
                out RaycastHit tmp_Hit,
                (bulletTransform.position - prevPosition).magnitude))
            {
                //������е��ǽ�ʬ
                if (tmp_Hit.collider.gameObject.CompareTag("Zombie"))
                {
                    //������ǽ�ʬ��characterStats
                    BulletTakeDamage(characterStats, tmp_Hit);
                    Debug.Log("������Zombie��");
                }
                //���������Player
                else if (tmp_Hit.collider.gameObject.CompareTag("Player"))
                {
                    BulletTakeDamage(characterStats, tmp_Hit);
                    Debug.Log("Enemy������Player��");
                }

                //Ѱ��BulletImpact������Ч
                foreach (var tmp_Prefab in ImpactPrefab)
                {
                    if (tmp_Prefab.gameObject.tag == tmp_Hit.collider.gameObject.tag)
                    {
                        Instantiate(tmp_Prefab, tmp_Hit.point, Quaternion.LookRotation(tmp_Hit.normal, Vector3.up));
                    }
                }

                //Ѱ��Tag����ײ��Ч
                var tmp_TagsWithAudio = impactAudioData.impactTagsWithAudios.Find((_audioData) => { return _audioData.Tag.Equals(tmp_Hit.collider.tag); });

                //���ӽ�׳��
                if (tmp_TagsWithAudio != null)
                {
                    int tmp_Length = tmp_TagsWithAudio.impactAudioClips.Count;
                    AudioClip tmp_AudioClip = tmp_TagsWithAudio.impactAudioClips[Random.Range(0, tmp_Length)];

                    //����ײ��������Ч
                    AudioSource.PlayClipAtPoint(tmp_AudioClip, tmp_Hit.point);
                }



                //�������������ٶ���
                //Destroy(tmp_Impact, 3);
                Destroy(transform.gameObject);
            }
        }

        //�ӵ�����˺�
        private void BulletTakeDamage(CharacterStats _characterStat, RaycastHit _Hit)
        {
            /*var tmp_Damage = Random.Range(_characterStat.attackData.minDamage, _characterStat.attackData.maxDemage);
            _Hit.collider.transform.gameObject.GetComponent<CharacterStats>().characterData.currentHealth -= tmp_Damage;*/
            var tmp_Target = _Hit.collider.gameObject.GetComponent<CharacterStats>();
            tmp_Target.TakeDamage(_characterStat, tmp_Target);

        }

    }
}

