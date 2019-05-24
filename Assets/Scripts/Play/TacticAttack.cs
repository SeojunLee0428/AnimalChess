using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticAttack : Attack
{
    public GameObject objArrow;
    public GameObject RHand;
    public GameObject warEffect;

    public Camera cam1;
    public Camera cam2;

    GameObject opponent;
    GameObject skillEffect;
    GameObject hitEffect;

    Vector3 ThisPos;

    Move tac;
    ClickManager scriptCM;
    
    void Start()
    {
        Controller.eventPlay += UpdatePlay;

        Init();
        GetUnitPosition();

        ani = transform.gameObject.GetComponent<Animations>();
        tomb = GameObject.Find("99");
        scriptCM = GameObject.Find("ClickManager").GetComponent<ClickManager>();

        cam1 = scriptCM.cam1;
        cam2 = scriptCM.cam2;

        if (transform.tag == "Arc")
        {
            objArrow = GameObject.Find("Arrow");
        }
    }
    
    void Update()
    {
        if (cm.myturn && !transform.GetComponent<Move>().enemy && !cm.togSkill.isOn)   //스킬 모드 체크
        {
            isSkill = false;
            if (transform.tag == "Arc")
            {
                range = 3;
            }
        }
        else if (cm.myturn && !transform.GetComponent<Move>().enemy && cm.togSkill.isOn)
        {
            isSkill = true;
            if (transform.tag == "Arc")
            {
                range = 5;
            }
        }

        if (transform.gameObject.GetComponent<Move>().topview == true)   //카메라 뷰 체크
        {
            topview = true;
        }
        else if (transform.gameObject.GetComponent<Move>().topview == false)
        {
            topview = false;
        }

        if (enemy || !turn)
        {
            return;
        }
        else if (cm.myturn && !cm.attack)
        {
            FindSelectableTiles();
            CheckMouse();
        }
    }

    void CheckMouse()   //공격 선택 체크
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
                    string posTarget = tac.myPosition;
                    Tile tile = GameObject.Find(posTarget).transform.GetComponent<Tile>();

                    if (transform.tag == "Arc")   //아처 일때 화살 준비
                    {
                        ThisPos.x = this.transform.position.x;
                        ThisPos.y = this.transform.position.y + 0.5f;
                        ThisPos.z = this.transform.position.z;

                        objArrow.GetComponent<Arrow>().GetCArrow().RenderEnFalse();
                        objArrow.GetComponent<Arrow>().CharPos = this.transform.position;
                        objArrow.GetComponent<Arrow>().posTarget = tac.gameObject;
                        objArrow.GetComponent<Arrow>().Rhand = RHand;
                        objArrow.transform.position = RHand.transform.position;

                        if (isSkill)
                        {
                            objArrow.GetComponent<Arrow>().speed = 9;
                        }
                        else
                        {
                            objArrow.GetComponent<Arrow>().speed = 6;
                        }
                    }
                    
                    if (tile.isAttack && !cm.attack)   //타일 상태 체크
                    {
                        Controller.instance.soundData.clipUi = eUi.select;

                        if (isSkill && transform.tag == "Mag")   //매지션 스킬 타일 영역 호출
                        {
                            FindMagicTiles(tile);
                        }

                        transform.LookAt(hit.transform);

                        cm.attack = true;

                        SendHit(tile);
                        PlayAttack();
                    }
                }
            }
        }
    }

    public void PlayAttack()   //공격 실행
    {
        Controller.instance.soundData.clipPlayer = ePlayer.own;

        if (isSkill && (transform.tag == "War" || transform.tag == "Arc" || transform.tag == "Mag"))   //공격 스킬 처리
        {
            ParentTag(1);

            Controller.instance.soundData.clipUi = eUi.charge;

            skillEffect.transform.position = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
            skillEffect.GetComponent<ParticleSystem>().Play();

            if (transform.tag == "War")
            {
                warEffect.GetComponent<ParticleSystem>().Play();

                cm.ActionCam.gameObject.SetActive(true);
                cm.ActionCam.transform.SetPositionAndRotation(new Vector3(tac.transform.position.x + 3, 4, tac.transform.position.z - 2), cm.ActionCam.transform.rotation);
                cm.objBg.gameObject.SetActive(false);

                tac.health -= 5;
            }
            else if (transform.tag == "Arc")
            {
                ParentTag(2);

                tac.health -= atk;
            }
            else if (transform.tag == "Mag")
            {
                tac.health -= atk;

                foreach (Move tac in listMagicTactic)
                {
                    tac.health -= atk;
                }
            }
        }
        else
        {
            cm.ActionCam.gameObject.SetActive(true);
            cm.ActionCam.transform.SetPositionAndRotation(new Vector3(tac.transform.position.x + 3, 4, tac.transform.position.z - 2), cm.ActionCam.transform.rotation);
            cm.objBg.gameObject.SetActive(false);
            tac.health -= atk;
        }
        // 애니메이션 사운드 처리
        if (transform.tag == "King")
        {
            Controller.instance.soundData.clipAction = eAction.swingJump;
            ani.Jump();
        }
        else if (transform.tag == "Arc")
        {
            Controller.instance.soundData.clipAction = eAction.swingArrow;
            ani.Attack3();

            objArrow.GetComponent<Arrow>().Shoot();
            this.gameObject.GetComponentInChildren<BowString>().Stretch();
        }
        else if(transform.tag == "War")
        {
            Controller.instance.soundData.clipAction = eAction.swingAttack;
            ani.Attack2();
        }
        else
        {
            Controller.instance.soundData.clipAction = eAction.swingFire;
            ani.Attack2();
        }

        StartCoroutine(PlayDamage());

        if (isSkill && transform.tag == "Mag")
        {
            foreach (Move tacDeath in listMagicTactic)
            {
                Death(tacDeath);
            }
        }

        Death(tac);

        StartCoroutine(OnDelay());
    }

    IEnumerator PlayDamage()   //공격에 따른 데미지 처리
    {
        if (transform.tag == "Mag")   //매지션 데미지 지연
        {
            yield return new WaitForSeconds(0.3f);

            if (isSkill)
            {
                Controller.instance.soundData.clipAction = eAction.skillFire;
                hitEffect = GameObject.Find("MagicSkill");
            }
            else
            {
                Controller.instance.soundData.clipAction = eAction.fire;
                hitEffect = GameObject.Find("MagicHit");
            }
        }
        else if (transform.tag == "King")   //왕 데미지 지연
        {
            yield return new WaitForSeconds(0.5f);

            Controller.instance.soundData.clipAction = eAction.attack;
            hitEffect = GameObject.Find("Hit");
        }
        else   //워리어, 아쳐 데미지 지연
        {
            yield return new WaitForSeconds(0.17f);

            if(isSkill && transform.tag == "War")
            {
                Controller.instance.soundData.clipAction = eAction.skillAttack;
                warEffect.GetComponent<ParticleSystem>().Stop();
            }
            else if (isSkill && transform.tag == "Arc")
            {
                Controller.instance.soundData.clipAction = eAction.skillArrow;

                ParentTag(3);
            }
            
            if(!isSkill)
            {
                Controller.instance.soundData.clipAction = eAction.attack;
                hitEffect = GameObject.Find("Hit");
            }
            else
            {
                ParentTag(4);
            }
        }
        
        hitEffect.transform.position = new Vector3(tac.transform.position.x, tac.transform.position.y + 0.2f, tac.transform.position.z);

        if (isSkill && transform.tag == "Mag")   //매지션 스킬 범위공격 처리
        {
            for (int i = 0; i < 5; i++)
            {
                hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
            }

            Damage(tac);

            foreach (Move tacMagic in listMagicTactic)
            {
                Damage(tacMagic);
            }
        }
        else
        {
            hitEffect.GetComponent<ParticleSystem>().Play();

            Damage(tac);
        }
        
        Controller.instance.soundData.clipPlayer = ePlayer.enem;
    }

    IEnumerator RemoveCharacter(Move tac)   //캐릭터 죽음 처리
    {
        yield return new WaitForSeconds(1.0f);
        
        tac.gameObject.transform.SetPositionAndRotation(tomb.transform.position, tomb.transform.rotation);
        tac.myPosition = tomb.name;
        
        if (tac.gameObject.transform.tag == "King")   //왕 죽을시 게임 승패
        {
            if (tac.enemy)
            {
                cm.result = 1;
            }
            else
            {
                cm.result = 2;
            }
        }
    }

    IEnumerator OnDelay()   //딜레이 후 초기화
    {
        yield return new WaitForSeconds(1.5f);

        hitEffect.transform.position = new Vector3(10, 10, 10);
        Tile t = GameObject.Find(myPosition).transform.GetComponent<Tile>();

        RemoveSelectableTiles();
        
        cm.ActionCam.gameObject.SetActive(false);
        cm.objBg.gameObject.SetActive(true);
        cm.togSkill.isOn = false;
        cm.attack = false;
        t.isAttack = false;
        t.isSkill = false;
        isHitCycle = false;
        isSkill = false;

        Controller.delay(false);
    }

    void SendHit(Tile clickTile)   //공격 처리 서버 갱신
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

    void Death(Move tac)   //죽음 서버 갱신 처리
    {
        if (!cm.myturn && tac.health <= 0)
        {
            for (int i = 0; i < 11; i++)
            {
                if (Controller.instance.ownData.arrNameJob[i] == tac.unit.name)
                {
                    Controller.instance.ownData.arrPosJob[i] = tomb.name;
                    cm.SendOwnData();
                }
            }
        }
    }

    void Damage(Move tac)   //데미지 처리
    {
        if (tac.health > 0)
        {
            tac.ani.Damage();
        }
        else
        {
            tac.ani.Die();
            StartCoroutine(RemoveCharacter(tac));
        }
    }

    void ParentTag(int num)   //부모 태그에 따른 이펙트 처리
    {
        if (transform.parent.tag == "white")
        {
            switch(num)
            {
                case 1:
                    skillEffect = GameObject.Find("wSkill");
                    break;
                case 2:
                    objArrow.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
                    break;
                case 3:
                    objArrow.transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
                    break;
                case 4:
                    hitEffect = GameObject.Find("wSkillHit");
                    break;
            }
        }
        else if (transform.parent.tag == "black")
        {
            switch (num)
            {
                case 1:
                    skillEffect = GameObject.Find("bSkill");
                    break;
                case 2:
                    objArrow.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
                    break;
                case 3:
                    objArrow.transform.GetChild(2).GetComponent<ParticleSystem>().Stop();
                    break;
                case 4:
                    hitEffect = GameObject.Find("bSkillHit");
                    break;
            }
        }
    }

    void UpdatePlay(Packet packet)   //상대방 공격 처리
    {
        if (packet.type == "recv" && packet.url == "play/poll/battle" && this != null && !isHitCycle && !cm.attack)   //url,공격싸이클,공격중 체크
        {
            for (int iNum = 0; iNum < 11; iNum++)
            {
                if (Controller.instance.enemData.arrNameJob[iNum] == unit.name && Controller.instance.enemData.arrHitJob[iNum] != "99")   //공격 캐릭 찾기
                {
                    Controller.delay(true);
                    isHitCycle = true;

                    for (int jNum = 0; jNum < 11; jNum++)   //상대방 좌표 계산
                    {
                        int tempPos = 88 - int.Parse(Controller.instance.ownData.arrPosJob[jNum]);

                        if (tempPos < 10 && Controller.instance.enemData.arrHitJob[iNum] == "0" + tempPos.ToString())
                        {
                            opponent = GameObject.Find(Controller.instance.ownData.arrNameJob[jNum]);
                            tac = opponent.GetComponent<Move>();
                        }
                        else if (Controller.instance.enemData.arrHitJob[iNum] == tempPos.ToString())
                        {
                            opponent = GameObject.Find(Controller.instance.ownData.arrNameJob[jNum]);
                            tac = opponent.GetComponent<Move>();
                        }
                    }

                    if (this.tag == "Arc")   //아쳐일 경우 화살 준비
                    {
                        ThisPos.x = this.transform.position.x;
                        ThisPos.y = this.transform.position.y + 0.5f;
                        ThisPos.z = this.transform.position.z;

                        objArrow.GetComponent<Arrow>().GetCArrow().RenderEnFalse();
                        objArrow.GetComponent<Arrow>().CharPos = this.transform.position;
                        objArrow.GetComponent<Arrow>().posTarget = tac.gameObject;
                        objArrow.GetComponent<Arrow>().Rhand = RHand;
                        objArrow.transform.position = RHand.transform.position;
                    }
                    
                    if (Controller.instance.enemData.isSkill)   //스킬 사용 체크
                    {
                        isSkill = true;
                        Controller.instance.enemData.isSkill = false;
                    }

                    FindSelectableTiles();
                    transform.LookAt(opponent.transform);

                    if (isSkill && transform.tag == "Mag")   //매지션 공격 계산
                    {
                        Tile tile;

                        int tempPos = 88 - int.Parse(Controller.instance.enemData.arrHitJob[iNum]);

                        if (tempPos < 10)
                        {
                            tile = GameObject.Find("0" + tempPos.ToString()).transform.GetComponent<Tile>();
                        }
                        else
                        {
                            tile = GameObject.Find(tempPos.ToString()).transform.GetComponent<Tile>();
                        }
                            
                        FindMagicTiles(tile);
                    }

                    PlayAttack();
                }
            }
        }
    }
}