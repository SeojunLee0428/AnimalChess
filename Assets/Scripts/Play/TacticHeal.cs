using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticHeal : Heal
{
    public Camera cam1;
    public Camera cam2;

    GameObject opponent;
    GameObject skillEffect;

    Move tac;
    ClickManager scriptCM;
    
    void Start()
    {
        Controller.eventPlay += UpdatePlay;

        Init();
        getUnitPosition();

        ani = transform.gameObject.GetComponent<Animations>();
        scriptCM = GameObject.Find("ClickManager").GetComponent<ClickManager>();
        cam1 = scriptCM.cam1;
        cam2 = scriptCM.cam2;
        
        isSkill = false;
    }
    
    void Update()
    {
        if (cm.myturn && !cm.togSkill.isOn)   //스킬 사용 체크
        {
            isSkill = false;
        }

        if (transform.gameObject.GetComponent<Move>().topview == true)   //카메라 뷰 체크
        {
            topview = true;
        }
        else if (transform.gameObject.GetComponent<Move>().topview == false)
        {
            topview = false;
        }

        if (!turn)
        {
            return;
        }
        else if (cm.myturn && !cm.attack)   //스킬 토글에 따라 타일 찾기 전환
        {
            if (!cm.togSkill.isOn)
            {
                FindSelectableTiles();
            }
            else
            {
                FindSkillTiles();
            }

            CheckMouse();
        }
    }

    void CheckMouse()   //힐 선택 체크
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
                if (hit.collider.tag == "War" || hit.collider.tag == "Arc" || hit.collider.tag == "Mag" || hit.collider.tag == "Pri" || hit.collider.tag == "King")
                {
                    opponent = hit.transform.gameObject;
                    tac = opponent.GetComponent<Move>();
                    string posMine = tac.myPosition;
                    Tile t = GameObject.Find(posMine).transform.GetComponent<Tile>();

                    if ((t.isHeal || t.isSkill) && !cm.attack)   //타일 상태 체크
                    {
                        Controller.instance.soundData.clipUi = eUi.select;

                        if (t.isHeal)
                        {
                            transform.LookAt(hit.transform);
                        }
                        else if (t.isSkill)
                        {
                            isSkill = true;
                        }

                        cm.attack = true;

                        SendHeal(t);
                        playAttack();
                    }
                }
            }
        }
    }

    public void playAttack()   //힐 실행
    {
        Controller.instance.soundData.clipPlayer = ePlayer.own;
        ani.Attack1();

        if (isSkill)   // 힐 스킬모드 
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

            foreach (Move tactic in listMyTactic)
            {
                tactic.health += 1;

                if (tactic.health > tactic.maxhealth)
                {
                    tactic.health = tactic.maxhealth;
                }
            }
        }
        else
        {
            cm.objBg.gameObject.SetActive(false);
            cm.ActionCam.gameObject.SetActive(true);
            cm.ActionCam.transform.SetPositionAndRotation(new Vector3(tac.transform.position.x + 3, 4, tac.transform.position.z - 2), cm.ActionCam.transform.rotation);

            tac.health += atk;

            if (tac.health > tac.maxhealth)
            {
                tac.health = tac.maxhealth;
            }
        }

        Controller.instance.soundData.clipAction = eAction.swingFire;

        StartCoroutine(OnHeal());
        StartCoroutine(OnDelay());
    }

    IEnumerator OnHeal()   //힐 받는 처리
    {
        yield return new WaitForSeconds(0.17f);

        if (isSkill)
        {
            Controller.instance.soundData.clipAction = eAction.skillHeal;

            foreach (Move tactic in listMyTactic)
            {
                GameObject objHeal = tactic.transform.GetChild(4).gameObject;
                objHeal.GetComponent<ParticleSystem>().Play();
                tactic.ani.Damage();
            }
        }
        else
        {
            Controller.instance.soundData.clipAction = eAction.heal;

            GameObject hitEffect = GameObject.Find("Heal");
            hitEffect.transform.position = new Vector3(tac.transform.position.x, tac.transform.position.y + 0.5f, tac.transform.position.z - 0.1f);
            hitEffect.GetComponent<ParticleSystem>().Play();
            tac.ani.Damage();
        }
    }

    IEnumerator OnDelay()   //딜레이 후 초기화
    {
        yield return new WaitForSeconds(1.5f);

        RemoveSelectableTiles();
        RemoveSkillTiles();
        
        cm.ActionCam.gameObject.SetActive(false);
        cm.objBg.gameObject.SetActive(true);
        cm.togSkill.isOn = false;
        cm.attack = false;
        healCycle = false;
        isSkill = false;

        Controller.delay(false);
    }

    void SendHeal(Tile clickTile)   //힐 처리 서버 갱신
    {
        for (int i = 0; i < 11; i++)
        {
            if (Controller.instance.ownData.arrNameJob[i] == unit.name)
            {
                Controller.instance.ownData.arrHitJob[i] = clickTile.name;
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

        for (int i = 0; i < 11; i++)
        {
            Controller.instance.ownData.arrHitJob[i] = "99";
        }
    }

    void UpdatePlay(Packet packet)   //상대방 힐사용 처리
    {
        if (packet.type == "recv" && packet.url == "play/poll/battle" && this != null && !healCycle && !cm.attack)   //url,힐싸이클,힐사용중 체크
        {
            for (int iNum = 0; iNum < 11; iNum++)
            {
                if (Controller.instance.enemData.arrNameJob[iNum] == unit.name && Controller.instance.enemData.arrHitJob[iNum] != "99")   //힐 사용 캐릭 찾기
                {
                    Controller.delay(true);
                    healCycle = true;

                    for (int jNum = 0; jNum < 11; jNum++)   //상대방 좌표 계산
                    {
                        int tempPos = 88 - int.Parse(Controller.instance.enemData.arrPosJob[jNum]);

                        if (tempPos < 10 && Controller.instance.enemData.arrHitJob[iNum] == "0" + tempPos.ToString())
                        {
                            opponent = GameObject.Find(Controller.instance.enemData.arrNameJob[jNum]);
                            tac = opponent.GetComponent<Move>();
                        }
                        else if (Controller.instance.enemData.arrHitJob[iNum] == tempPos.ToString())
                        {
                            opponent = GameObject.Find(Controller.instance.enemData.arrNameJob[jNum]);
                            tac = opponent.GetComponent<Move>();
                        }
                    }

                    if (!Controller.instance.enemData.isSkill)   //스킬 사용 체크
                    {
                        FindSelectableTiles();
                        transform.LookAt(opponent.transform);
                    }
                    else
                    {
                        FindSkillTiles();
                        isSkill = true;
                        Controller.instance.enemData.isSkill = false;
                    }

                    playAttack();
                }
            }
        }
    }
}