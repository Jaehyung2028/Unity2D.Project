using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// �� ã��� A* �˰����� �̿��Ͽ� ����
// G : �������κ��� �̵��ߴ� �Ÿ�, H : |����|+|����| ��ֹ� �����Ͽ� ��ǥ������ �Ÿ�, F : G + H �� �̿��Ͽ� ���� ���� ���� ã�´�.
[System.Serializable]
public class Node
{
    public Node(bool _isWall, int _x, int _y)
    { isWall = _isWall; x = _x; y = _y; }

    public bool isWall;
    public Node ParentNode;

    public int x, y, G, H;
    public int F { get { return G + H; } }
}


public class NodeControl : MonoBehaviour
{
    public Stack<Node> DestinationNode;

    int sizeX, sizeY;

    Node[,] NodeArray;

    Node StartNode, TargetNode, CurNode;

    List<Node> OpenList, ClosedList;

    public void PathFind(Vector3Int Left, Vector3Int Right, Vector3 Pos, Vector3 TargetPos, out Stack<Node> FinalNode, out Node Destination)
    {
        // Ÿ���� �߾� ������ �������� �̵��ϱ� ���� Ÿ�ϸ��� ��ġ�� -0.5, -0.5 �̵����� ���� ������ ��ǥ�� Ÿ���� �߾ӿ� ��ġ�ϰ� �Ͽ���
        // ���Ϳ� �÷��̾� ��ġ���� Ÿ�� �߾ӱ����� ���߱� ���� �ݿø�
        Pos = Vector3Int.RoundToInt(Pos);
        TargetPos = Vector3Int.RoundToInt(TargetPos);

        // NodeArray�� ���� �ڽ��� �̵��Ҽ� �ִ� ��ǥ ������ �������ְ� �迭�� ũ�⸦ ����
        sizeX = Right.x - Left.x + 1;
        sizeY = Right.y - Left.y + 1;

        NodeArray = new Node[sizeX, sizeY];


        // ���� �����϶� ��ֹ��� �ִ� Ÿ�ϸ��� ��ǥ�� ã�� ������ �νĽ��� ����
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                if (Map.Instance.ObstaclePos.Contains(new Vector3Int(Left.x + i, Left.y + j, 0)))
                    isWall = true;

                NodeArray[i, j] = new Node(isWall, Left.x + i, Left.y + j);
            }
        }

        // ���� ��带 �ڽ��� ��ġ�� �����ϴ� ��ǥ �������� �ʱ�ȭ
        StartNode = NodeArray[(int)Pos.x - Left.x, (int)Pos.y - Left.y];

        // Ÿ���� ��ġ�� �ڽ��� �̵� ������ ���� ��� Ÿ���� ��ġ�� ��忡 �ʱ�ȭ
        // ������ ���ԵǾ� ���� ���� ��� null ���� �������ش�. ���ʹ� null ���� ���ϵ� ��� �̵��ڷ�ƾ�� ����
        if (TargetPos.x >= Left.x && TargetPos.x <= Right.x)
        {
            if (TargetPos.y >= Left.y && TargetPos.y <= Right.y)
            {
                TargetNode = NodeArray[(int)TargetPos.x - Left.x, (int)TargetPos.y - Left.y];
            }
        }
        else
        {
            Destination = null;
            FinalNode = new Stack<Node>();
            return;
        }

        Destination = TargetNode;

        // ������ ����Ʈ���� �ʱ�ȭ
        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        DestinationNode = new Stack<Node>();


        // �̵��Ҽ� �ִ� ���� ����Ʈ�� ���� ��� ����ؼ� ��θ� Ž��
        while (OpenList.Count > 0)
        {
            CurNode = OpenList[0];

            // ù������ ���� ��庸�� �ٸ� �ε��� F�� �۰� F�� ���ٸ� H�� ���� �� ������� �Ͽ� ��������Ʈ�� ����
            for (int i = 1; i < OpenList.Count; i++)
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H) 
                    CurNode = OpenList[i];

            OpenList.Remove(CurNode);
            ClosedList.Add(CurNode);



            // ���� ��尡 Ÿ�� ���� ���� ��� �̵���θ� ���� ����
            // ���������� �̵� �ؾ� �Ǳ� ������ ������ Ȱ��
            if (CurNode == TargetNode)
            {
                Node TargetCurNode = TargetNode;

                while (TargetCurNode != StartNode)
                {
                    DestinationNode.Push(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }

                DestinationNode.Push(StartNode);
            }

            // ��, ��, ��, �Ʒ�, �밢�� �� 8���� �������� ��θ� Ž���Ͽ� ���³�忡 ����
            OpenListAdd(CurNode.x + 1, CurNode.y + 1, Left, Right);
            OpenListAdd(CurNode.x - 1, CurNode.y + 1, Left, Right);
            OpenListAdd(CurNode.x - 1, CurNode.y - 1, Left, Right);
            OpenListAdd(CurNode.x + 1, CurNode.y - 1, Left, Right);
            OpenListAdd(CurNode.x, CurNode.y + 1, Left, Right);
            OpenListAdd(CurNode.x + 1, CurNode.y, Left, Right);
            OpenListAdd(CurNode.x, CurNode.y - 1, Left, Right);
            OpenListAdd(CurNode.x - 1, CurNode.y, Left, Right);
        }

        // �ִ� �̵� ��θ� ���� ��带 �������־� �Ѱ���
        FinalNode = DestinationNode;
    }

    void OpenListAdd(int checkX, int checkY, Vector3Int Left, Vector3Int Right)
    {
        // �̵� ������ ����� �ʰ�, �� �Ǵ� ��������Ʈ�� �ƴ� ��� ����
        if (checkX >= Left.x && checkX <= Right.x && checkY >= Left.y && checkY <= Right.y)
        {
            if (!NodeArray[checkX - Left.x, checkY - Left.y].isWall && !ClosedList.Contains(NodeArray[checkX - Left.x, checkY - Left.y]))
            {
                // �밢�� �̵��� �����̷� �̵� �Ұ�
                if (NodeArray[CurNode.x - Left.x, checkY - Left.y].isWall && NodeArray[checkX - Left.x, CurNode.y - Left.y].isWall) return;

                // �밢�� �̵��� �ڳʸ� ���� ���� ���� �ʰ� �������� ���� ���� ��� �̵� �Ұ�
                if (NodeArray[CurNode.x - Left.x, checkY - Left.y].isWall || NodeArray[checkX - Left.x, CurNode.y - Left.y].isWall) return;


                // ��忡 ���� ��Ų �� ����, �밢���� ���� ��� ����
                Node NeighborNode = NodeArray[checkX - Left.x, checkY - Left.y];
                int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);


                // �̵������ �̿����G���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� G, H, ParentNode�� ���� �� ��������Ʈ�� �߰�
                if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))
                {
                    NeighborNode.G = MoveCost;
                    NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;
                    NeighborNode.ParentNode = CurNode;

                    OpenList.Add(NeighborNode);
                }
            }
        }
    }
}

