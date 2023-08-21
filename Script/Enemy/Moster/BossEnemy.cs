using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    [Space]
    [Header("���� HP �̹���")]
    [SerializeField] GameObject[] Hp_obj;

    Stack<GameObject> HP_Image = new Stack<GameObject>();

    [Space]
    [Header("�Ѿ� �߻� ��ġ")]
    [SerializeField] GameObject[] BulletPos;

    [Space]
    [Header("�Ѿ� ������Ʈ")]
    [SerializeField] GameObject[] Bullet;

    public Queue<GameObject> Bullet_List = new Queue<GameObject>();

    float CurTime = 0;
    bool IsSkill = false;


    protected override void Awake()
    {
        base.Awake();

        // ���̵��� ���� ���� ���� HP�̹��� ���� �� ���ÿ� ����
        for (int i = 0; i < Hp; i++)
        { Hp_obj[i].SetActive(true); HP_Image.Push(Hp_obj[i]); }

        // ���� �Ѿ��� ť�� ����
        for (int i = 0; i < Bullet.Length; i++)
            Bullet_List.Enqueue(Bullet[i]);
    }

    protected override void Hit()
    {
        // ���� ������ ��� HP�� �������� ����
        if (enemyStat != EnemyStat.Die)
        {
            HItEffect.Play();

            Destroy(HP_Image.Pop());

            if (HP_Image.Count == 0)
                enemyStat = EnemyStat.Die;
        }
    }

    protected override IEnumerator AttackControl()
    {
        Delay = true;

        Ani.SetTrigger("Attack");
        yield return new WaitForSeconds(AttackDelay);

        Delay = false;
    }

    IEnumerator SkillControl()
    {
        IsSkill = true;

        // ������ ������ ������ �̿��Ͽ� ����
        int Pattern = UnityEngine.Random.Range(0, 2);

        switch (Pattern)
        {
            case 0:

                // �Ѿ� �߻� ������ �Ѿ��� �θ� ���� ��Ű�鼭 �̸� ������ ��ġ���� �߻� �ǵ��� ����
                GameObject _Bullet;

                for (int i = 0; i < 4; i++)
                {
                    _Bullet = Bullet_List.Dequeue();
                    _Bullet.transform.SetParent(null);
                    _Bullet.transform.position = BulletPos[i].transform.position;
                    _Bullet.transform.rotation = BulletPos[i].transform.rotation;
                    _Bullet.SetActive(true);
                }

                yield return new WaitForSeconds(1f);

                for (int i = 4; i < BulletPos.Length; i++)
                {
                    _Bullet = Bullet_List.Dequeue();
                    _Bullet.transform.SetParent(null);
                    _Bullet.transform.position = BulletPos[i].transform.position;
                    _Bullet.transform.rotation = BulletPos[i].transform.rotation;
                    _Bullet.SetActive(true);
                }

                break;


            case 1:

                // ���� ����� �÷��̾��� ��ġ�� �޾ƿ� �����ϵ��� ����
                float _time = 0;

                Vector3 PlayerPos = Player.transform.position;

                AttackCollider.enabled = true;


                while (_time < 5)
                {
                    _time += Time.deltaTime;
                    Rd.transform.position = Vector3.MoveTowards(Rd.transform.position, PlayerPos, Speed * 3 * Time.deltaTime);

                    yield return null;
                }

                AttackCollider.enabled = false;

                break;

            default:
                break;
        }

        yield return new WaitForSeconds(AttackDelay);

        IsSkill = false;
        CurTime = 0;
    }

    protected override IEnumerator Die()
    {
        IsDeath = true;

        DeathEffect.Play();

        Color[] color = new Color[EnemySprite.Length];
        float CurTime = 0;

        for (int i = 0; i < EnemySprite.Length; i++)
            color[i] = EnemySprite[i].color;

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

        StartCoroutine(ButtonManager.instance.AlarmString("Game Clear"));

        yield return new WaitForSeconds(2f);

        Destroy(Player.transform.gameObject);

        // ���� ���� óġ�� ��� �ʰ� ������Ʈ�� �����ϴ� �Լ� ����
        Map.Instance.TileReset();
    }

    // Phisics�� �̿��Ͽ� �Ϲ� ���� ���� ����
    protected override void PlayerCheck()
    {
        Attack_Player = Physics2D.BoxCast(Rd.transform.position, new Vector2(Attack_Area, Attack_Area / 2), 0, Vector2.zero, 0, LayerMask.GetMask("Player"));

        if (CurTime < 3)
            CurTime += Time.deltaTime;
    }

    protected override void EnemyControl()
    {
        // �÷��̾ �ڽ��� �� ������ ��ġ�� ��� ����
        if (Player.transform.position.x >= LeftPos.x && Player.transform.position.x <= RightPos.x)
        {
            if (Player.transform.position.y >= LeftPos.y && Player.transform.position.y <= RightPos.y)
            {
                // ������ ���¸� ���������� ����
                if (enemyStat != EnemyStat.Die)
                {
                    PlayerCheck();

                    if (Attack_Player)
                    {
                        enemyStat = EnemyStat.Attack;
                        Lock = false;
                    }
                    else
                    {
                        if (CurTime >= 3)
                        {
                            enemyStat = EnemyStat.Skill;
                            Lock = false;
                        }
                        else
                            enemyStat = EnemyStat.Move;
                    }
                }

                if (!IsDeath)
                {
                    switch (enemyStat)
                    {
                        case EnemyStat.Idle:

                            Idle();

                            break;

                        case EnemyStat.Move:

                            if (!Lock)
                                StartCoroutine(Move());

                            break;

                        case EnemyStat.Attack:

                            Attack();

                            break;

                        case EnemyStat.Skill:

                            if (!IsSkill)
                                StartCoroutine(SkillControl());

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

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Rd.transform.position, Attack_Area);
    }
}
