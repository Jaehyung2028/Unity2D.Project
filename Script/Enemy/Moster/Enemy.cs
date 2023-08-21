using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.WSA;

// ������ ���̽� ��ũ��Ʈ�̱� ������ �߻�Ŭ������ �ۼ�
public abstract class Enemy : MonoBehaviour
{
    [Header("�̵� �˰���")][SerializeField] NodeControl MoveNode;

    [Space][Header("������Ʈ")][SerializeField] protected Rigidbody2D Rd, Player;
    [SerializeField] protected Animator Ani;
    [SerializeField] protected ParticleSystem DeathEffect, HItEffect;
    [SerializeField] protected Collider2D AttackCollider;
    [SerializeField] protected GameObject AllBody;
    [SerializeField] protected SpriteRenderer[] EnemySprite;

    [Space][Header("���� ��ġ")][SerializeField] protected int Hp, Speed;
    public Vector3Int LeftPos, RightPos;
    [SerializeField] protected float Move_Area, Attack_Area, AttackDelay;

    protected bool Follow_Player = false, Attack_Player = false, IsDeath = false, Delay = false, Lock = false;
    public Dungeon CurDungeon;

    protected enum EnemyStat { Idle, Move, Attack, Skill, Die };
    [Space][Header("���� ����")][SerializeField] protected EnemyStat enemyStat;


    protected abstract void OnDrawGizmos();

    protected virtual void Idle()
    {
        Rd.velocity = Vector3.zero;
        Ani.SetBool("Walk", false);
        Ani.SetBool("Idle", true);
    }

    protected IEnumerator Move()
    {
        Lock = true;

        Stack<Node> MovePos = new Stack<Node>();
        Node Destination;
        Node Point = null;

        float CurTime = 0;

        Ani.SetBool("Walk", true);
        Ani.SetBool("Idle", false);

        // ���� �̵��� ��� �Ű������� �Ѱ��־� �̵� ��ũ��Ʈ�� �ִ� �Լ��� ����
        MoveNode.PathFind(LeftPos, RightPos, Rd.transform.position, Player.transform.position, out MovePos, out Destination);

        // �Ѱ� ���� ��� ��尡 �����ϴ� ��� ����ؼ� ���� �ǰ� �ƴ� ��� �Լ� ����
        if (MovePos.Count != 0)
        {
            if (Destination != null)
            {
                Point = MovePos.Pop();
            }
        }
        else
        {
            Lock = false;
            yield break;
        }

        while (Lock && !IsDeath)
        {
            if (CurTime < 0.5f)
                CurTime += Time.deltaTime;

            // �ڽ��� ��ġ�� ���� �̵��Ͽ��� �ϴ� ��ġ���� �����Ͽ��� ��� ����
            // �ǽð����� �÷��̾ �����Ͽ��� �ϱ� ������ 0.5�ʰ� ���� ������ Ÿ���� �ݿø� ��ġ�� �������� �ٸ� ��� ��Ž�� �ϵ��� ����
            if (Rd.transform.position == new Vector3(Point.x, Point.y, 0))
            {
                if (Vector3Int.RoundToInt(Player.transform.position) != new Vector3Int(Destination.x, Destination.y, 0) && CurTime >= 0.5f)
                {
                    MoveNode.PathFind(LeftPos, RightPos, Rd.transform.position, Player.transform.position, out MovePos, out Destination);

                    CurTime = 0;

                    if (MovePos.Count != 0)
                    {
                        if (Destination != null)
                        {
                            Point = MovePos.Pop();
                        }
                    }
                    else
                    {
                        Lock = false;
                        yield break;
                    }
                }
                else
                {
                    if (MovePos.Count != 0)
                    {
                        if (Destination != null)
                        {
                            Point = MovePos.Pop();
                        }
                    }
                }

            }

            // ��ġ �̵��� �������� �Ѱ� ���� ����� �� ��ġ�� ���ʴ�� �̵��ϵ��� ����
            Rd.transform.position = Vector3.MoveTowards(Rd.transform.position, new Vector3(Point.x, Point.y, 0), Speed * Time.deltaTime);

            // �÷��̾�� ������ �Ÿ� ���� �̿��Ͽ� �÷��̾ �ٶ󺸵��� ����
            if ((Player.transform.position.x - Rd.transform.position.x) < 0 && AllBody.transform.rotation != Quaternion.Euler(0, 0, 0))
                AllBody.transform.rotation = Quaternion.Euler(0, 0, 0);
            else if ((Player.transform.position.x - Rd.transform.position.x) > 0 && AllBody.transform.rotation != Quaternion.Euler(0, 180, 0))
                AllBody.transform.rotation = Quaternion.Euler(0, 180, 0);

            yield return null;

        }
    }

    protected virtual void Attack() { if (!Delay) StartCoroutine(AttackControl()); }

    protected virtual IEnumerator AttackControl()
    {
        Delay = true;

        while (enemyStat == EnemyStat.Attack)
        {
            Ani.SetTrigger("Attack");
            yield return new WaitForSeconds(AttackDelay);
        }

        Delay = false;
    }

    public IEnumerator AttackCollider_Control()
    {
        AttackCollider.enabled = true;
        yield return new WaitForSeconds(0.1f);
        AttackCollider.enabled = false;
    }

    abstract protected IEnumerator Die();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "PlayerAttack")
            Hit();
    }

    protected virtual void Hit()
    {
        if (enemyStat != EnemyStat.Die)
        {

            HItEffect.Play();

            Hp--;

            if (Hp <= 0)
            {
                enemyStat = EnemyStat.Die;
            }
        }
    }

    // Phisics�� �̿��Ͽ� �Ϲ� ���� ���� �� ���� ���� ����
    protected virtual void PlayerCheck()
    {
        Follow_Player = Physics2D.CircleCast(Rd.transform.position, Move_Area, Vector2.zero, 0, LayerMask.GetMask("Player"));
        Attack_Player = Physics2D.BoxCast(Rd.transform.position, new Vector2(Attack_Area, Attack_Area / 2), 0, Vector2.zero, 0, LayerMask.GetMask("Player"));
    }

    protected virtual void EnemyControl()
    {
        // �÷��̾ �ڽ��� �� ������ ��ġ�� ��� ����
        if (Player.transform.position.x >= LeftPos.x && Player.transform.position.x <= RightPos.x &&
            Player.transform.position.y >= LeftPos.y && Player.transform.position.y <= RightPos.y)
        {
            // ������ ���¸� ���������� ����
            if (enemyStat != EnemyStat.Die)
            {
                PlayerCheck();

                if (Follow_Player)
                {
                    if (!Attack_Player)
                        enemyStat = EnemyStat.Move;
                    else
                    {
                        enemyStat = EnemyStat.Attack;
                        Lock = false;
                    }
                }
                else
                {
                    enemyStat = EnemyStat.Idle;
                    Lock = false;
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

                    case EnemyStat.Die:

                        StartCoroutine(Die());

                        break;

                    default:
                        break;
                }
            }

        }
        else
        {
            if (Lock)
                Lock = false;
        }
    }

    public void CurPosDungeon()
    {
        for (int i = 0; i < Map.Instance._DungeonList.Count; i++)
        {
            if (Map.Instance._DungeonList[i].LeftBenchmark == new Vector2Int(LeftPos.x, LeftPos.y))
                CurDungeon = Map.Instance._DungeonList[i];
        }
    }

    protected virtual void Awake()
    {
        if (GameObject.Find("Player(Clone)") != null)
            Player = GameObject.Find("Player(Clone)").GetComponent<Rigidbody2D>();

        Hp = Hp * ButtonManager.instance.HpLevel;
    }

    private void FixedUpdate() => EnemyControl();
}
