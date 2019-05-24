using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject unit;
    public Animations ani;
    public ClickManager cm;

    public bool topview = false;
    public bool turn = false;
    public bool enemy = false;
    public bool moving = false;
    public bool moveCycle = false;
    public bool isSkill = false;
    public bool isCheckHeal = false;
    public bool isOnceRange = false;

    public int health;
    public int maxhealth;
    public int tactics;
    public int move;

    public float moveSpeed;
    public float wait;

    public string myPosition;

    GameObject[] tiles;
    List<Tile> selectableTiles = new List<Tile>();
    Queue<Tile> process = new Queue<Tile>();
    Stack<Tile> path = new Stack<Tile>();
    Tile currentTile;  

    GameObject skillEffect;

    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();

    protected void Init()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile");
        cm = GameObject.Find("ClickManager").GetComponent<ClickManager>();
        wait = 0;
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

                if (isSkill)
                {
                    t.isSkill = true;
                }
                else if(t.current == false)
                {
                    t.isRangeMove = true;
                }

                if (t.distance < move)
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
                }
            }
            isOnceRange = true;
        }
    }

    public void MoveToTile(Tile tile)    //이동 경로 계산
    {
        if (isSkill && transform.tag == "King")   //왕 스킬 사용 시 처리 
        {
            if (transform.parent.tag == "white")
            {
                skillEffect = GameObject.Find("wSkill");
            }
            else if (transform.parent.tag == "black")
            {
                skillEffect = GameObject.Find("bSkill");
            }

            Controller.instance.soundData.clipUi = eUi.charge;

            skillEffect.transform.position = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
            skillEffect.GetComponent<ParticleSystem>().Play();
            transform.GetChild(5).gameObject.GetComponent<ParticleSystem>().Play();
        }

        path.Clear();
        moving = true;

        Tile next = tile;
        while (next != null)   //path 경로 스택 쌓기
        {
            path.Push(next);
            next = next.parent;
        }
    }

    public void Moving(Tile clickTile)   //이동 실행
    {
        foreach (GameObject tile in tiles)  
        {
            Tile rangeTile = tile.GetComponent<Tile>();

            if (rangeTile == clickTile)
            {
                rangeTile.isMove = true;
            }
            else
            {
                rangeTile.isRangeMove = false;
                rangeTile.isMove = false;
            }
        }

        if (path.Count > 0)
        {
            ani = transform.gameObject.GetComponent<Animations>();
            ani.Walk();

            if (isSkill && transform.tag == "King")
            {
                Controller.instance.soundData.clipPlayer = ePlayer.skillMove;
            }

            if (!Controller.instance.soundData.isChangeWalk)
            {
                Controller.instance.soundData.clipAction = eAction.walk1;
            }
            else
            {
                Controller.instance.soundData.clipAction = eAction.walk2;
            }

            Tile t = path.Peek();
            Vector3 target = t.transform.position;

            //목표 타일 위 유닛 위치 ​​계산
            target.y += t.GetComponent<Collider>().bounds.extents.y - 0.5f;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                CalculateHeading(target);
                SetHorizotalVelocity();
                
                transform.forward = heading;
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                //한칸 이동시 경로 제외
                transform.position = target;
                path.Pop();
            }
        }
        else   //이동 후 초기화
        {
            wait += Time.deltaTime;

            ani.Reset();

            if (wait >= 0.5f)
            {
                if (isSkill && transform.tag == "King")
                {
                    transform.GetChild(5).gameObject.GetComponent<ParticleSystem>().Stop();
                }

                GetUnitPosition();
                RemoveSelectableTiles();
                moveCycle = false;
                isSkill = false;
                moving = false;
                cm.move = false;
                cm.togSkill.isOn = false;
                Controller.delay(false);

                wait = 0;
            }
        }
    }

    public void RemoveSelectableTiles() // 선택가능 타일 리스트 제거
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
        
        isOnceRange = false;
        selectableTiles.Clear();
    }
    
    void CalculateHeading(Vector3 target) //방향 계산
    {
        heading = target - transform.position;
        heading.Normalize();
    }

    void SetHorizotalVelocity()  //속도 설정
    {
        velocity = heading * moveSpeed;
    }

    public void GetUnitPosition()   // 유닛 포지션 
    {
        RaycastHit hit;
        if(Physics.Raycast(unit.transform.position, Vector3.down, out hit, 10))
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
