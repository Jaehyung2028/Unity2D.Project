using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] Transform PlayerPos;
    [SerializeField] GameObject[] Body;
    [SerializeField] BoxCollider2D Coll;
    [SerializeField] ParticleSystem DestroyEffect;
    bool Hit = false;

    // �÷��̾�� �Ѿ� ������Ʈ�� ť�� ���� �Ͽ� Ǯ�� ����� ���
    private void OnEnable()
    {
        Coll.enabled = true;
        Hit = false;
        gameObject.transform.SetParent(null);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" || collision.tag == "Ground")
        {
            StartCoroutine(DestroyControl());
        }
    }

    // �浹 ���� �Ŀ� ������Ʈ�� �÷��� ���� ���� ���������� �̿��Ͽ� õõ�� ������������ ����
    IEnumerator DestroyControl()
    {
        Hit = true;

        Coll.enabled = false;

        for (int i = 0; i < Body.Length; i++)
        {
            Body[i].SetActive(false);
        }

        DestroyEffect.Play();

        yield return new WaitForSeconds(1.5f);

        // ������Ʈ�� �θ� ���� ��Ű�� �ٽ� ť�� ����
        Player.Instance.Bullet_List.Enqueue(gameObject);
        gameObject.transform.SetParent(PlayerPos);

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
        if(!Hit)
        transform.Translate(-transform.right * 10 * Time.deltaTime, Space.World);

        // ť�� ���ԵǱ� �� �� �ʱ�ȭ�� ������Ʈ ����
        if (Map.Instance.Reset) Destroy(gameObject);
    }
}
