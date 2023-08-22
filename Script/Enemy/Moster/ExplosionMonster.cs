using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionMonster : Enemy
{
    [SerializeField] CircleCollider2D ExplosionAttack;
    [SerializeField] ParticleSystem ExplosionEffect;

    protected override void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Rd.transform.position, Move_Area);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Rd.transform.position, new Vector3(Attack_Area, Attack_Area, Attack_Area));
    }

    // Phisics�� �̿��Ͽ� �Ϲ� ���� ���� �� ���� ���� ����
    protected override void PlayerCheck()
    {
        Follow_Player = Physics2D.CircleCast(Rd.transform.position, Move_Area, Vector2.zero, 0, LayerMask.GetMask("Player"));
        Attack_Player = Physics2D.BoxCast(Rd.transform.position, new Vector2(Attack_Area, Attack_Area), 0, Vector2.zero, 0, LayerMask.GetMask("Player"));
    }

    // ������ ������ �°� �����ϴ� ��� ������
    protected override IEnumerator Die()
    {
        IsDeath = true;

        CurDungeon.Monster--;

        if (CurDungeon.Monster <= 0)
        {
            ButtonManager.instance.Alarm("Clear.");

            for (int i = 0; i < Map.Instance.PortalArray.Count; i++)
            {
                Map.Instance.PortalArray[i].GetComponent<PorTarDirection>().CanMove();
            }
        }

        gameObject.tag = "Untagged";

        for (int i = 0; i < EnemySprite.Length; i++)
            EnemySprite[i].color = Color.red;

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < EnemySprite.Length; i++)
            EnemySprite[i].color = Color.clear;

        ExplosionAttack.enabled = true;
        ExplosionEffect.Play();

        yield return new WaitForSeconds(0.1f);
        ExplosionAttack.enabled = false;


        yield return new WaitForSeconds(2f);

        Destroy(AllBody.transform.parent.gameObject);
    }
}
