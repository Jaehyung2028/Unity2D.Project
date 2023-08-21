using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerMonster : Enemy
{
    [SerializeField] GameObject[] Bullet;
    public Queue<GameObject> Bullet_List = new Queue<GameObject>();

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < Bullet.Length; i++)
            Bullet_List.Enqueue(Bullet[i]);
    }

    protected override void Idle() => Ani.SetBool("Idle", true);

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Rd.transform.position, Attack_Area);
    }

    // ���ݽ� �÷��̾ ������ ������
    protected override void Attack()
    {
        if (Player.transform.position.x - Rd.transform.position.x > 0)
            AllBody.transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (Player.transform.position.x - Rd.transform.position.x < 0)
            AllBody.transform.rotation = Quaternion.Euler(0, 0, 0);

        base.Attack();
    }

    protected override IEnumerator AttackControl()
    {
        Delay = true;

        while (enemyStat == EnemyStat.Attack)
        {
            yield return new WaitForSeconds(AttackDelay);

            if (!IsDeath)
                Ani.SetTrigger("Attack");
        }

        Delay = false;
    }

    protected override IEnumerator Die()
    {
        IsDeath = true;

        CurDungeon.Monster--;

        if (CurDungeon.Monster == 0)
        {
            for (int i = 0; i < Map.Instance.PortalArray.Count; i++)
            {
                Map.Instance.PortalArray[i].GetComponent<PorTarDirection>().CanMove();
            }
        }

        Ani.SetBool("Idle", true);

        gameObject.tag = "Untagged";

        DeathEffect.Play();

        Color[] color = new Color[EnemySprite.Length];
        float CurTime = 0;

        for (int i = 0; i < EnemySprite.Length; i++)
        {
            color[i] = EnemySprite[i].color;
        }

        while (color[0].a > 0)
        {
            CurTime += 0.4f * Time.deltaTime;

            for (int i = 0; i < color.Length; i++)
            {
                color[i].a = Mathf.Lerp(1, 0, CurTime);
                EnemySprite[i].color = color[i];
            }

            yield return null;
        }

        Map.Instance.AllMonster.Remove(AllBody.transform.parent.gameObject);
        Destroy(AllBody.transform.parent.gameObject);
    }

    // ���� �ִϸ��̼ǿ� �̺�Ʈ�� �߰��Ͽ� �����Ǿ� �ִ� �Ѿ� ������Ʈ�� Ȱ��ȭ
    public void BulletInstance() => Bullet_List.Dequeue().SetActive(true);

    protected override void PlayerCheck() => enemyStat = Delay ? EnemyStat.Idle : EnemyStat.Attack;


    // Ÿ�� ������ ��� ���� ������ ��ġ�� ��� ���� �ϱ� ������ Phisics�� ������� �ʾ� �̵��� ����
    protected override void EnemyControl()
    {
        if (Player.transform.position.x >= LeftPos.x && Player.transform.position.x <= RightPos.x)
        {
            if (Player.transform.position.y >= LeftPos.y && Player.transform.position.y <= RightPos.y)
            {
                if (enemyStat != EnemyStat.Die)
                    PlayerCheck();

                if (!IsDeath)
                {
                    switch (enemyStat)
                    {
                        case EnemyStat.Idle:

                            Idle();

                            break;

                        case EnemyStat.Attack:

                            Attack();

                            break;

                        case EnemyStat.Die:


                            StartCoroutine(Die());

                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}
