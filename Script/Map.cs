using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using System;
using UnityEngine.UIElements;

// ���� ������Ҹ� �����ڷ� ����
[System.Serializable]
public class Dungeon
{
    public Dungeon(Vector2Int _LeftBenchmark, bool _HiddenRoom = false, bool _BossRoom = false, GameObject _MiniMap_Obj = null, int _Monster = 0)
    {
        LeftBenchmark = _LeftBenchmark;
        HidenRoom = _HiddenRoom;
        BossRoom = _BossRoom;
        MiniMap_Obj = _MiniMap_Obj;
        Monster = _Monster;
    }

    public Vector2Int LeftBenchmark;
    public bool HidenRoom, BossRoom;
    public GameObject MiniMap_Obj;
    public int Monster;
}

public class Map : MonoBehaviour
{
    public static Map Instance;

    public List<Dungeon> _DungeonList = new List<Dungeon>();

    List<Vector2Int> NomarRoom = new List<Vector2Int>();

    HashSet<Vector3Int> GroundPos = new HashSet<Vector3Int>();
    public HashSet<Vector3Int> ObstaclePos = new HashSet<Vector3Int>();
    public List<GameObject> Soul, Scroll, AllMonster, PortalArray;

    [Header("������Ʈ")]
    public GameObject[] MonsterArray;
    public GameObject _Portal, HiddenPortal, BossPortal, SoulObj, KeyObj, MiniMapNormal, MiniMapHidden, MiniMapBoss, BossObj;

    [Space][Header("Ÿ��")]
    public Tilemap GroundTileMap, ObstacleTileMap;
    public RuleTile GroundTile, ObstacleTile;

    [Space][Header("�� ��ġ")]
    public int RoomSize_X, RoomSize_Y, RoomCount, HiddenCount, MaxMonster, RoomCountCheck = 0, DoingMapInstace = 0;

    Vector2Int[] TileDirection;

    public bool Reset = false;

    bool FinishTile = false;

    private void Awake()
    {
        Instance = this;
        Screen.SetResolution(1920, 1080, true);
    }

    // ���� ����ġ �������� ��, ��, ��, �Ʒ� ���� �迭
    void TitleDirectionReset()
    {
        TileDirection = new Vector2Int[]
        {
               new Vector2Int(RoomSize_X + 10, 0),
               new Vector2Int(0, RoomSize_Y + 10),
               new Vector2Int(-RoomSize_X - 10, 0),
               new Vector2Int(0, -RoomSize_Y - 10)
        };
    }

    // ù ���۹� ����
    public void StartTile()
    {
        RoomCountCheck = RoomCount;

        TitleDirectionReset();

        // ī�޶��� ����� ���� ����� ���� ����
        Camera.main.orthographicSize = 3;

        RoomCount -= 1;

        // �ٴڿ� �ش��ϴ� Ÿ���� �����ϰ� �ٴ� HashSet�� ����
        for (int i = 0; i < RoomSize_X; i++)
        {
            for (int j = 0; j < RoomSize_Y; j++)
            {
                GroundTileMap.SetTile(new Vector3Int(i, j, 0), GroundTile);
                GroundPos.Add(new Vector3Int(i, j, 0));
            }
        }

        // ������ �ش� ��Ҹ� ����
        _DungeonList.Add(new Dungeon(Vector2Int.zero));

        Vector2 Center = new Vector3(RoomSize_X / 2 - 0.5f, RoomSize_Y / 2 - 0.5f);

        // ���� ��ġ�� �°� ��Ż�� ����
        // ��Ż�� �� �̵� ��ġ ����
        PortalArray.Add(Instantiate(_Portal, Center + new Vector2(0, RoomSize_Y / 2 - 1.5f), Quaternion.identity));
        PortalArray[0].GetComponent<PorTarDirection>().Direction = "Up";

        PortalArray.Add(Instantiate(_Portal, Center - new Vector2(0, RoomSize_Y / 2 - 1.5f), Quaternion.identity));
        PortalArray[1].GetComponent<PorTarDirection>().Direction = "Down";

        PortalArray.Add(Instantiate(_Portal, Center + new Vector2(RoomSize_X / 2 - 1.5f, 0), Quaternion.identity));
        PortalArray[2].GetComponent<PorTarDirection>().Direction = "Right";

        PortalArray.Add(Instantiate(_Portal, Center - new Vector2(RoomSize_X / 2 - 1.5f, 0), Quaternion.identity));
        PortalArray[3].GetComponent<PorTarDirection>().Direction = "Left";

        // ī�޶��� ��ġ�� ���� �߾ӿ� �� �� �ֵ��� ����
        Camera.main.transform.position = new Vector3(RoomSize_X / 2 - 0.5f, RoomSize_Y / 2 - 0.5f, -10);

        // ���� �� �������� �� 4�������� Ÿ���� ����
        StartCoroutine(StartMap(TileDirection[0], Vector2Int.zero));
        StartCoroutine(StartMap(TileDirection[1], Vector2Int.zero));
        StartCoroutine(StartMap(TileDirection[2], Vector2Int.zero));
        StartCoroutine(StartMap(TileDirection[3], Vector2Int.zero));
    }

    // ���� ��ġ�� �޾ƿ� ���� ����
    IEnumerator StartMap(Vector2Int Room_Direction, Vector2Int Pos)
    {
        while (RoomCount > 0)
        {
            // ���� ����Ʈ�� �����ҷ��� �ϴ� ��ġ�� ���� ���� ��� �ٽ� ���� ��ġ���� �ٸ� �������� ���� �������� Ȯ��
            // ������ ���Ե� ��ġ������ Ȯ���ϱ� ���� ���ٽ��� �̿��Ͽ� ��ġ���� Ȯ��
            if (_DungeonList.FindIndex(item => item.LeftBenchmark.Equals(Room_Direction)) != -1)
            {
                for (int i = 0; i < TileDirection.Length; i++)
                {
                    // �ٸ� ���⿡ ���� ������ ��� ��ġ�� �ٽ� �����Ͽ� While�� �ݺ�
                    if (_DungeonList.FindIndex(item => item.LeftBenchmark.Equals(Pos + TileDirection[i])) == -1)
                    {
                        Room_Direction = Pos + TileDirection[i];
                        break;
                    }
                    // ������ �迭���� Ȯ���� �������� ��ǥ�� ���� ��� DoingMapInstance ���� ���Ͽ� 4���� ��� ���������� ��ǥ�� ����
                    // �����Ǿ�� �ϴ� ���� ������ ���� ���� ������ �ٸ� ��� ���� �ٽ� �ʱ�ȭ
                    else if (i == 3 && _DungeonList.FindIndex(item => item.LeftBenchmark.Equals(Pos + TileDirection[i])) != -1)
                    {
                        DoingMapInstace += 1;

                        if (DoingMapInstace == 4 && RoomCount != RoomCountCheck)
                            TileReset();

                        yield break;
                    }
                }
            }

            // ��������Ʈ�� �ش� ��ǥ�� �������� ���� ��� Ÿ���� �����ϰ� ���� ���� ���� ��ֹ� ����
            if (_DungeonList.FindIndex(item => item.LeftBenchmark.Equals(Room_Direction)) == -1)
            {

                for (int i = 0; i < RoomSize_X; i++)
                {
                    for (int j = 0; j < RoomSize_Y; j++)
                    {
                        GroundTileMap.SetTile(new Vector3Int(Room_Direction.x + i, Room_Direction.y + j, 0), GroundTile);
                        GroundPos.Add(new Vector3Int(Room_Direction.x + i, Room_Direction.y + j, 0));

                        if (i > 3 && j > 3 && i < RoomSize_X - 3 && j < RoomSize_Y - 3 && RoomCount != 1)
                        {
                            if (UnityEngine.Random.Range(0, 20) == 0)
                            {
                                ObstacleTileMap.SetTile(new Vector3Int(Room_Direction.x + i, Room_Direction.y + j, 0), ObstacleTile);
                                ObstaclePos.Add(new Vector3Int(Room_Direction.x + i, Room_Direction.y + j, 0));
                            }
                        }
                    }
                }

                // ������ ������ ���� ��� ���������� ����
                bool Boss = RoomCount == 1 ? true : false;

                RoomCount -= 1;

                int Direction = UnityEngine.Random.Range(0, 4);

                GameObject Monster = null;
                int Monster_Setting = UnityEngine.Random.Range(2, MaxMonster + 1);
                int MonsterCount = 0;

                Enemy CurMonster;

                // ���ο� ������ ��ǥ�� ���� �� �ݺ�
                Pos = Room_Direction;
                Room_Direction = Room_Direction + TileDirection[Direction];

                // �ش� �濡 ���� ����
                // ������ ���� ������Ʈ�� ����Ʈ���� ����
                // �濡 ������ �� ���� �� ���� ����Ʈ�� ����
                if (!Boss)
                {
                    for (int i = 0; i < Monster_Setting; i++)
                    {

                        MonsterCount++;

                        int _X = UnityEngine.Random.Range(Pos.x + 4, Pos.x + RoomSize_X - 4);
                        int _Y = UnityEngine.Random.Range(Pos.y + 4, Pos.y + RoomSize_Y - 4);

                        Monster = Instantiate(MonsterArray[UnityEngine.Random.Range(0, MonsterArray.Length)], new Vector3(_X, _Y, 0), Quaternion.identity);
                        AllMonster.Add(Monster);

                        CurMonster = Monster.transform.GetChild(0).GetComponent<Enemy>();

                        CurMonster.LeftPos = new Vector3Int(Pos.x, Pos.y, 0);
                        CurMonster.RightPos = new Vector3Int(Pos.x + RoomSize_X, Pos.y + RoomSize_Y, 0);
                    }
                }
                else
                {
                    MonsterCount++;

                    Monster = Instantiate(BossObj, new Vector3(RoomSize_X / 2 - 0.5f + Pos.x, RoomSize_Y / 2 - 0.5f + Pos.y, 0), Quaternion.identity);
                    AllMonster.Add(Monster);

                    CurMonster = Monster.transform.GetChild(0).GetComponent<Enemy>();

                    CurMonster.LeftPos = new Vector3Int(Pos.x, Pos.y, 0);
                    CurMonster.RightPos = new Vector3Int(Pos.x + RoomSize_X, Pos.y + RoomSize_Y, 0);
                }

                // ��������Ʈ�� �߰�
                _DungeonList.Add(new Dungeon(Pos, false, Boss, null, MonsterCount));

                yield return null;

            }
        }
        // ��� �� ������ �Ϸ�� ��� ����
        if (!FinishTile)
        {
            FinishTile = true;

            HidenRoomKeyInstace();

            ButtonManager.instance.Success = true;
        }
    }

    private void HidenRoomKeyInstace()
    {
        // ���̵��� ���� ������ ����
        if (HiddenCount > 0)
        {
            int ScrollCount = HiddenCount;

            // ù��° ��� �̾��� �� 5���� ��� ���� ���� ������ ������ ���� ����Ʈ�� ����
            for (int i = 5; i < _DungeonList.Count; i++)
            {
                if (_DungeonList[i].BossRoom == false)
                    NomarRoom.Add(_DungeonList[i].LeftBenchmark);
            }

            // ������� ���� ��ŭ �濡 ��ũ���� ������ �� ����Ʈ���� ����
            while (NomarRoom.Count > HiddenCount)
            {
                int s = UnityEngine.Random.Range(0, NomarRoom.Count);

                if (ScrollCount > 0)
                {
                    Scroll.Add(Instantiate(KeyObj, new Vector3(NomarRoom[s].x, NomarRoom[s].y) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2), Quaternion.identity));
                    ScrollCount--;
                }

                NomarRoom.RemoveAt(s);
            }

            // ���ŵǰ� ���� �濡�� �ҿ� ������Ʈ�� ����
            // ��������Ʈ�� ����� ������ True�� ����
            for (int i = 0; i < NomarRoom.Count; i++)
            {
                _DungeonList[_DungeonList.FindIndex(item => item.LeftBenchmark.Equals(NomarRoom[i]))].HidenRoom = true;
                Soul.Add(Instantiate(SoulObj, new Vector3(NomarRoom[i].x, NomarRoom[i].y) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2), Quaternion.identity));
            }

            // �濡 �������� �ʾ����� �̴ϸʿ� ǥ�õǴ� �� ���� ������ �´� ���� �̹����� ���� ��ġ�� �°� ����
            // �濡 �����Ͽ��� ��� �̹����� �����Ͽ� ������ ��� �������� ���� ���� ����
            for (int i = 1; i < _DungeonList.Count; i++)
            {
                GameObject MiniMap = null;

                if (_DungeonList[i].BossRoom == false)
                {
                    MiniMap = _DungeonList[i].HidenRoom ?
                        Instantiate(MiniMapHidden, (new Vector3(_DungeonList[i].LeftBenchmark.x, _DungeonList[i].LeftBenchmark.y, 0) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2, -5)), Quaternion.identity) :
                        Instantiate(MiniMapNormal, (new Vector3(_DungeonList[i].LeftBenchmark.x, _DungeonList[i].LeftBenchmark.y, 0) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2, -5)), Quaternion.identity);
                }
                else
                {
                    MiniMap =
                        Instantiate(MiniMapBoss, (new Vector3(_DungeonList[i].LeftBenchmark.x, _DungeonList[i].LeftBenchmark.y, 0) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2, -5)), Quaternion.identity);
                }

                // �̹����� ����� ���� ����� �°� �ٲپ� �̴ϸʿ��� ���� ���� �� �ֵ��� ����
                // �ش� ��ǥ�� ���� ��������Ʈ�� ������ҿ� �̹����� �����Ͽ� ����
                MiniMap.transform.localScale = new Vector3(RoomSize_X, RoomSize_Y, 0);
                _DungeonList[i].MiniMap_Obj = MiniMap;
            }

            for (int i = 0; i < AllMonster.Count; i++)
            {
                AllMonster[i].transform.GetChild(0).GetComponent<Enemy>().CurPosDungeon();
            }
        }
    }

    private void OBJDestroy()
    {
        for (int i = 0; i < PortalArray.Count; i++)
            Destroy(PortalArray[i]);

        for (int i = 0; i < Soul.Count; i++)
            Destroy(Soul[i]);

        for (int i = 0; i < AllMonster.Count; i++)
            Destroy(AllMonster[i]);

        for (int i = 0; i < Scroll.Count; i++)
            Destroy(Scroll[i]);

        for (int i = 0; i < _DungeonList.Count; i++)
        {
            if (_DungeonList[i].MiniMap_Obj != null)
                Destroy(_DungeonList[i].MiniMap_Obj);
        }
    }

    // ������ ������Ʈ ���� �� ����Ʈ ����
    public void TileReset()
    {

        Reset = true;

        GameObject.Find("Canvas").transform.Find("MiniMap").gameObject.SetActive(false);

        OBJDestroy();

        GroundTileMap.ClearAllTiles();
        ObstacleTileMap.ClearAllTiles();

        PortalArray.Clear();

        Soul.Clear();

        Scroll.Clear();

        AllMonster.Clear();

        NomarRoom.Clear();

        _DungeonList.Clear();

        GroundPos.Clear();
        ObstaclePos.Clear();

        RoomCountCheck = 0;

        HiddenCount = 0;

        FinishTile = false;

        StartCoroutine(ButtonManager.instance.ImageFadeIn());

        TitleDirectionReset();

    }
}
