using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool walkable = true;
    public bool visited = false; 
    public bool current = false; 
    public bool isMove = false;  
    public bool isAttack = false;
    public bool isHeal = false;
    public bool isRangeMove = false;
    public bool isRangeAtk = false;                    
    public bool isRangeHeal = false;
    public bool isSkill = false;

    public int distance = 0;

    public Tile parent = null;
    public ClickManager cm;

    public List<Tile> adjacencyList = new List<Tile>();    // 인접타일 리스트
    public List<Tile> adjacencyListAtk = new List<Tile>();

    Color color;   //타일 기본 색상

    void Start () {
        color = GetComponent<Renderer>().material.color;
        cm = GameObject.Find("ClickManager").GetComponent<ClickManager>();
    }
	
	void Update () {
        if (cm.myturn)   //상태별 타일 색상 변경
        {
            if (current)
            {
                GetComponent<Renderer>().material.color = color;
            }
            else if (isMove)
            {
                GetComponent<Renderer>().material.color = new Color(0f / 255f, 0f / 255f, 200f / 255f);
            }
            else if (isRangeMove)
            {
                GetComponent<Renderer>().material.color = new Color(0f / 255f, 100f / 255f, 255f / 255f);
            }
            else if (isAttack)
            {
                GetComponent<Renderer>().material.color = new Color(200f / 255f, 0f / 255f, 0f / 255f);
            }
            else if (isRangeAtk)
            {
                GetComponent<Renderer>().material.color = new Color(255f / 255f, 100f / 255f, 0f / 255f);
            }
            else if (isHeal)
            {
                GetComponent<Renderer>().material.color = new Color(0f / 255f, 200f / 255f, 0f / 255f);
            }
            else if (isRangeHeal)
            {
                GetComponent<Renderer>().material.color = new Color(100f / 255f, 255f / 255f, 0f / 255f);
            }
            else if (isSkill)
            {
                GetComponent<Renderer>().material.color = new Color(255f / 255f, 200f / 255f, 0f / 255f);
            }
            else
            {
                GetComponent<Renderer>().material.color = color;
            }
        }
        else 
        {
            Reset();
            GetComponent<Renderer>().material.color = color;
        }
    }

    public void Reset()   //타일 초기화
    {
        adjacencyList.Clear();
        adjacencyListAtk.Clear();

        parent = null;
        current = false;
        visited = false;
        isMove = false;
        isAttack = false;
        isHeal = false;
        isRangeMove = false;
        isRangeAtk = false;
        isRangeHeal = false;
        isSkill = false;

        distance = 0;
    }

    public void FindNeighbors()   //인접 타일 체크 호출
    {
        Reset();

        CheckTile(Vector3.forward);
        CheckTile(-Vector3.forward);
        CheckTile(Vector3.right);
        CheckTile(-Vector3.right);
    }

    public void CheckTile(Vector3 direction)   //인접 타일 체크 및 리스트 추가
    {
        Vector3 halfExtents = new Vector3(0.25f, 0.25f, 0.25f);
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents);

        foreach (Collider item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();

            if (tile != null && tile.walkable)
            {
                RaycastHit hit;
                if (!Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1) || tile == isMove)
                {
                    adjacencyList.Add(tile);
                }

                adjacencyListAtk.Add(tile);
            }
        }
    }
}
