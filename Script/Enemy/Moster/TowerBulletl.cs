using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBulletl : MonoBehaviour
{
    [SerializeField] CircleCollider2D Coll;

    [SerializeField] TowerMonster TowerEnemy;

    [SerializeField] Transform Pos;

    [SerializeField] ParticleSystem DestroyEffect;

    Rigidbody2D PlayerPos;
    Vector2 Direction;
    bool Hit = false;

    private void Awake() => PlayerPos = GameObject.Find("Player(Clone)").GetComponent<Rigidbody2D>();

    // ���Ϳ��� �Ѿ� ������Ʈ�� ť�� ���� �Ͽ� Ǯ�� ����� ���
    private void OnEnable()
    {
        Coll.enabled = true;
        Direction = (PlayerPos.transform.position - transform.position).normalized;
        Hit = false;
        gameObject.transform.SetParent(null);
    }

    void Update()
    {
        if (!Hit) transform.Translate(Direction * 5 * Time.deltaTime, Space.World);

        // ť�� ���ԵǱ� �� �� �ʱ�ȭ�� ������Ʈ ����
        if (Map.Instance.Reset) Destroy(gameObject);
    }

    // �浹 ���� �Ŀ� ������Ʈ�� �÷��� ���� ���� ���������� �̿��Ͽ� õõ�� ������������ ����
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || collision.tag == "Ground")
            StartCoroutine(DestroyControl());
    }

    IEnumerator DestroyControl()
    {
        Hit = true;

        Coll.enabled = false;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);

        DestroyEffect.Play();

        yield return new WaitForSeconds(1.5f);

        // ������Ʈ�� �θ� ���� ��Ű�� �ٽ� ť�� ����
        TowerEnemy.Bullet_List.Enqueue(gameObject);
        gameObject.transform.SetParent(Pos);

        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
