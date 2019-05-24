using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticMove : Move
{
    public Camera cam1;
    public Camera cam2;

    ClickManager scriptCM;
    Tile tileClick;

    void Awake()
    {
        Controller.eventPlay += UpdatePlay;

        Init();
        GetUnitPosition(); 
        scriptCM = GameObject.Find("ClickManager").GetComponent<ClickManager>();
        ani = transform.gameObject.GetComponent<Animations>();

        cam1 = scriptCM.cam1;
        cam2 = scriptCM.cam2;
    }

    void Update()
    {
        if (cm.myturn && !cm.togSkill.isOn && transform.tag == "King")   //왕 스킬 모드 전환
        {
            isSkill = false;
            move = 1;
        }
        else if (cm.myturn && cm.togSkill.isOn && transform.tag == "King")
        {
            isSkill = true;
            move = 4;
        }

        if (moveCycle)
        {
            Moving(tileClick);
        }

        if (enemy || !turn)
        {
            return;
        }

        if (cm.myturn)
        {
            if (!moving && !cm.move)
            {
                FindSelectableTiles();  
                CheckMouse();  
            }
            else
            {
                Moving(tileClick); 
            }
        }
    }

    void CheckMouse()   // 마우스 클릭 체크 함수
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray;
            if (!topview)
            {
                ray = cam1.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                ray = cam2.ScreenPointToRay(Input.mousePosition);
            }

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Tile")  //타일 클릭 체크
                {
                    tileClick = hit.collider.GetComponent<Tile>();

                    if ((tileClick.isRangeMove || tileClick.isSkill) && !cm.move)   //타일 상태 체크
                    {
                        Controller.instance.soundData.clipUi = eUi.select;

                        cm.move = true;

                        SendMove(tileClick);
                        MoveToTile(tileClick);
                    }
                }
            }
        }
    }

    void SendMove(Tile clickTile)   //이동 처리 서버 갱신
    {
        for (int i = 0; i < 11; i++)
        {
            if (Controller.instance.ownData.arrNameJob[i] == unit.name)
            {
                Controller.instance.ownData.arrPosJob[i] = clickTile.name;
            }
        }

        if (isSkill)
        {
            cm.cnt -= 2;
            Controller.instance.ownData.playCnt -= 2;
        }
        else
        {
            cm.cnt -= 1;
            Controller.instance.ownData.playCnt -= 1;
        }

        cm.SendOwnData();
    }

    void UpdatePlay(Packet packet)   //상대방 이동 처리
    {
        if (packet.type == "recv" && packet.url == "play/poll/battle" && this != null && !moveCycle && !cm.move)   //url,공격싸이클,공격중 체크
        {
            for (int i = 0; i < 11; i++)
            {
                if (Controller.instance.enemData.arrNameJob[i] == unit.name && Controller.instance.enemData.arrPosJob[i] != myPosition && Controller.instance.enemData.arrPosJob[i] != "99")   //이동 캐릭 찾기
                {
                    Controller.delay(true);
                    moveCycle = true;
                    tileClick = GameObject.Find(Controller.instance.enemData.arrPosJob[i]).transform.GetComponent<Tile>();   //이동 타일 입력

                    if (Controller.instance.enemData.isSkill)   //스킬 사용 체크
                    {
                        isSkill = true;
                        Controller.instance.enemData.isSkill = false;
                    }

                    FindSelectableTiles();
                    MoveToTile(tileClick);
                }
            }
        }
    }
}
