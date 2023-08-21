using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [SerializeField] BossEnemy Boss_Script;

    [SerializeField] Transform Bullet_Parent;

    [SerializeField] GameObject[] Body;

    [SerializeField] CircleCollider2D Hit_Coll;

    [SerializeField] ParticleSystem DestroyEffect;
    bool Hit = false;


    // ���Ϳ��� �Ѿ� ������Ʈ�� ť�� ���� �Ͽ� Ǯ�� ����� ���
    private void OnEnable() { Hit_Coll.enabled = true; Hit = false; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || collision.tag == "Ground")
            StartCoroutine(DestroyControl());
    }

    // �浹 ���� �Ŀ� ������Ʈ�� �÷��� ���� ���� ���������� �̿��Ͽ� õõ�� ������������ ����
    IEnumerator DestroyControl()
    {
        Hit = true;

        Hit_Coll.enabled = false;

        for (int i = 0; i < Body.Length; i++)
        {
            Body[i].SetActive(false);
        }

        DestroyEffect.Play();

        yield return new WaitForSeconds(1.5f);

        // ������Ʈ�� �θ� ���� ��Ű�� �ٽ� ť�� ����
        Boss_Script.Bullet_List.Enqueue(gameObject);
        gameObject.transform.SetParent(Bullet_Parent);

        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        for (int i = 0; i < Body.Length; i++)
        {
            Body[i].SetActive(true);
        }

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!Hit)
            transform.Translate(-transform.right * 10 * Time.deltaTime, Space.World);

        // ť�� ���ԵǱ� �� �� �ʱ�ȭ�� ������Ʈ ����
        if (Map.Instance.Reset) Destroy(gameObject);
    }
}
