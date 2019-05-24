using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour
{
    public Animations ani;
    public GameObject unit;
    public ClickManager cm;
    public List<Move> listMyTactic = new List<Move>();

    public bool topview = false;
    public bool turn = false;
    public bool enemy = false;
    public bool healCycle = false;
    public bool heal = false;
    public bool isSkill;
    public bool isOnceSkill = false;
    public bool isOnceRange = false;

    public int atk;
    public int tactics;
    public int range;

    public string myPosition;

    List<Tile> selectableTiles = new List<Tile>();
    List<Tile> listSkillTiles = new List<Tile>();

    Queue<Tile> process = new Queue<Tile>();
    Queue<Tile> processatk = new Queue<Tile>();

    GameObject[] tiles;
    Tile currentTile;

    protected void Init()
    {
        cm = GameObject.Find("ClickManager").GetComponent<ClickManager>();
        tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject temp in tiles)
        {
            listSkillTiles.Add(temp.GetComponent<Tile>());
        }
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

    public void FindSelectableTiles()   //선택 가능한 타일 찾기
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
                t.isRangeHeal = true;

                if (t.distance < range)
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

                    foreach (Tile tile in processatk)   //힐 가능 타일 체크
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1))
                        {
                            if (!(hit.transform.GetComponent<Move>().enemy))
                            {
                                tile.isHeal = true;
                                hit.transform.GetComponent<Move>().isCheckHeal = true;
                            }
                        }
                    }
                }
            }
            isOnceRange = true;
        }
    }

    public void FindSkillTiles()   //스킬 사용시 전체 타일 체크
    {
        if (!isOnceSkill)
        {
            foreach (Tile tile in listSkillTiles)
            {
                RaycastHit hit;
                if (Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1))
                {
                    if (!hit.transform.GetComponent<Move>().enemy && cm.myturn)
                    {
                        listMyTactic.Add(hit.transform.GetComponent<Move>());
                        tile.isSkill = true;
                        hit.transform.GetComponent<Move>().isCheckHeal = true;
                    }
                    else if(hit.transform.GetComponent<Move>().enemy && !cm.myturn)
                    {
                        listMyTactic.Add(hit.transform.GetComponent<Move>());
                        tile.isSkill = true;
                        hit.transform.GetComponent<Move>().isCheckHeal = true;
                    }
                }
            }
            isOnceSkill = true;
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

        if (cm != null)
        {
            for (int i = 0; i < 11; i++)
            {
                cm.myTactics[i].isCheckHeal = false;
            }
        }

        isOnceRange = false;
        processatk.Clear();
        selectableTiles.Clear();
    }

    public void RemoveSkillTiles()   //스킬 타일 초기화
    {
        if (cm != null)
        {
            for (int i = 0; i < 11; i++)
            {
                cm.myTactics[i].isCheckHeal = false;
            }
        }

        isOnceSkill = false;
        listMyTactic.Clear();
    }

    public void getUnitPosition()   // 유닛 포지션 
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
