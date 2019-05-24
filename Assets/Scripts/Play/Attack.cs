using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public Animations ani;
    public GameObject unit;
    public ClickManager cm;
    public GameObject tomb;

    public List<Tile> listMagicTiles = new List<Tile>();
    public List<Move> listMagicTactic = new List<Move>();

    public bool topview = false;
    public bool turn = false;
    public bool enemy = false;
    public bool battle = false;
    public bool isSkill = false;
    public bool isHitCycle = false;
    public bool isOnceRange = false;

    public int atk;
    public int tactics;
    public int range;
    
    public string myPosition;

    GameObject[] tiles;
    List<Tile> selectableTiles = new List<Tile>();
    Queue<Tile> process = new Queue<Tile>();
    Queue<Tile> processatk = new Queue<Tile>();
    Tile currentTile;

    protected void Init()
    {
        cm = GameObject.Find("ClickManager").GetComponent<ClickManager>();
        tiles = GameObject.FindGameObjectsWithTag("Tile");
    }

    public void GetCurrentTile()   //반환 받은 타일 current 체크
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.current = true;
    }

    public Tile GetTargetTile(GameObject target)   //현재 위치 타일 반환 
    {
        RaycastHit hit;
        Tile tile = null;

        if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 1))
        {
            tile = hit.collider.GetComponent<Tile>();
        }

        return tile;
    }

    public void ComputeAdjacencyLists()   //인접 타일 계산
    {
        foreach (GameObject tile in tiles)
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors();
        }
    }

    public void FindSelectableTiles()   //선택 가능 타일영역 찾기
    {
        if (!isOnceRange)
        {
            ComputeAdjacencyLists();
            GetCurrentTile();

            process.Enqueue(currentTile);
            currentTile.visited = true;

            while (process.Count > 0)
            {
                Tile t = process.Dequeue();
                selectableTiles.Add(t);

                if (isSkill)
                {
                    t.isSkill = true;
                }
                else
                {
                    t.isRangeAtk = true;
                }

                if (t.distance < range)   //범위 내의 타일 process 추가
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        if (!tile.visited)
                        {
                            tile.parent = t;
                            tile.visited = true;
                            tile.distance = 1 + t.distance;
                            process.Enqueue(tile);
                        }
                    }

                    foreach (Tile tile in t.adjacencyListAtk)
                    {
                        if (!tile.visited)
                        {
                            tile.parent = t;
                            tile.visited = true;
                            tile.distance = 1 + t.distance;
                            processatk.Enqueue(tile);
                        }
                    }

                    foreach (Tile tile in processatk)   //공격 가능 타일 체크
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1))
                        {
                            if (hit.transform.GetComponent<Move>().enemy)
                            {
                                tile.isAttack = true;
                            }
                        }
                    }
                }
            }

            isOnceRange = true;
        }
    }
    
    public void FindMagicTiles(Tile tCenter)   //매지션 스킬 타일 체크 호출
    {
        listMagicTiles.Add(tCenter);

        Check(tCenter, Vector3.forward);
        Check(tCenter, -Vector3.forward);
        Check(tCenter, Vector3.right);
        Check(tCenter, -Vector3.right);
    }

    public void Check(Tile tCenter, Vector3 direction)   //매지션 스킬 타일 체크
    {
        Vector3 halfExtents = new Vector3(0.25f, 0.25f, 0.25f);
        Collider[] colliders = Physics.OverlapBox(tCenter.transform.position + direction, halfExtents);

        foreach (Collider item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();
            if (tile != null && tile.walkable)
            {
                RaycastHit hit;
                if (Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1))
                {
                    if (cm.myturn && hit.transform.GetComponent<Move>().enemy)
                    {
                        listMagicTactic.Add(hit.transform.GetComponent<Move>());
                        listMagicTiles.Add(tile);
                    }
                    else if (!cm.myturn && !hit.transform.GetComponent<Move>().enemy)
                    {
                        listMagicTactic.Add(hit.transform.GetComponent<Move>());
                        listMagicTiles.Add(tile);
                    }
                }
            }
        }
    }

    public void RemoveSelectableTiles()   //선택 타일 초기화
    {
        if (currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }

        foreach (Tile tile in selectableTiles)
        {
            tile.Reset();
        }

        foreach (Tile tile in processatk)
        {
            tile.Reset();
        }

        foreach(Tile tile in listMagicTiles)
        {
            tile.Reset();
        }

        isOnceRange = false;

        listMagicTactic.Clear();
        listMagicTiles.Clear();
        processatk.Clear();
        selectableTiles.Clear();
    }

    public void GetUnitPosition()   //유닛 포지션 
    {
        RaycastHit hit;
        if (Physics.Raycast(unit.transform.position, Vector3.down, out hit, 10))
        {
            Tile myTile = hit.transform.GetComponent<Tile>();
            myPosition = myTile.name;
            SetPlayData();
        }
    }

    protected void SetPlayData()   //포지션 갱신
    {
        for (int cnt = 0; cnt < 11; cnt++)
        {
            if (Controller.instance.ownData.arrNameJob[cnt] == unit.name)
            {
                Controller.instance.ownData.arrPosJob[cnt] = myPosition;
            }
        }
    }
}