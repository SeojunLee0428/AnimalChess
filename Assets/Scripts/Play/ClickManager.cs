using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClickManager : MonoBehaviour
{
    public GameObject WAlly;
    public GameObject BAlly;
    public GameObject WEnemy;
    public GameObject BEnemy;
    public GameObject coin;
    public GameObject curtain;
    public GameObject objMsgOut;
    public GameObject objMsgCount;
    public GameObject objBg;
    public GameObject objResultBg;
    public GameObject objPlayer1;
    public GameObject objPlayer2;
    public GameObject objTimer;

    public Camera cam1;   //메인 뷰
    public Camera cam2;   //탑 뷰
    public Camera ActionCam;   //액션 뷰

    public Image imgCount1; 
    public Image imgCount2;
    public Image imgCount3;
    public Image winImage;   
    public Image loseImage;
    public Image enemTimer;  
    
    public Text textTimer; 
    public Text textPlayer1;
    public Text textPlayer2;
    
    public Toggle togAction;   //공격 이동 전환
    public Toggle togSkill;   //스킬모드 전환

    public Button btnReady;
    public Button btnCam;
    public Button btnReplay;
    public Button btnOut;

    public Move[] myTactics = new Move[11];

    public bool change = true; 
    public bool once = false;
    public bool myturn;
    public bool move;
    public bool attack;
    public bool end;
    public bool rolltheCoin;
    public bool isStart;
    public bool isOut;

    public int cnt;
    public int result;

    public float spinSpeed;

    GameObject obj = null;
    GameObject lastobj = null;
    GameObject[] objMyUnits = new GameObject[11];
    GameObject[] objEnUnits = new GameObject[11];

    Move[] enTactics = new Move[11];

    Vector3 posFirst;
    Vector3 posSecond;

    bool isMoveOnce = false;
    bool isAttackOnce = false;
    bool isSkillOnce = false;

    float timeleft;
    float netCycle;

    void Awake()
    {
        Controller.eventPlay += this.UpdatePlay;
        Controller.instance.ownData.isStay = true;
        Controller.instance.soundData.clipBgm = eBgm.wait;
        Controller.instance.ownData.playName = Controller.instance.userData.nick;

        posFirst = objPlayer1.transform.position;
        posSecond = new Vector3(objPlayer1.transform.position.x + 15, objPlayer1.transform.position.y - 20, 0);
        textPlayer1.text = Controller.instance.ownData.playName;

        move = false;
        attack = false;
        myturn = false;
        end = false;
        rolltheCoin = false;
        isOut = false;
        isStart = false;

        cnt = 3;
        timeleft = 61;
        result = 0;

        objTimer.SetActive(false);
        togSkill.isOn = false;
        togSkill.gameObject.SetActive(false);

        ResetUI();
    }

    void Update()
    {
        if (rolltheCoin)   //선 잡기 코인 회전
        {
            coin.SetActive(true);
            coin.transform.Rotate(new Vector3(0, 0, coin.transform.rotation.z) + Vector3.forward * Time.deltaTime * spinSpeed);
            spinSpeed -= 5.0f;

            if (spinSpeed <= 0)
            {
                if (myturn)
                {
                    Coin_Firstatk();
                }
                else
                {
                    Coin_Secondatk();
                }

                Controller.instance.soundData.clipBgm = eBgm.play;
            }
        }

        if (result == 1)   //게임 결과 처리
        {
            ResultWin();
            ResetUI();
            isStart = false;
            myturn = false;
            end = true;
            result = 0;
        }
        else if (result == 2)
        {
            ResultLose();
            ResetUI();
            isStart = false;
            myturn = false;
            end = true;
            result = 0;
        }

        netCycle += Time.deltaTime;

        if (netCycle > 1.0f)   //게임 상태 갱신 요청 
        {
            if (Controller.instance.ownData.playId == 0 || Controller.instance.enemData.playId == 0)   //매칭 상태 체크
            {
                Controller.instance.PacketSend("play/poll/match", "stay");
            }
            else if (Controller.instance.ownData.playState == 0 || Controller.instance.enemData.playState == 0)   //준비 상태 체크
            {
                Controller.instance.dictSend["idx"] = Controller.instance.enemData.playId.ToString();
                Controller.instance.PacketSend("play/poll/ready", "stay");
            }
            else if ((Controller.instance.ownData.isReady == true || Controller.instance.enemData.isReady == true) && isStart)   //게임 진행 상태 체크
            {
                Controller.instance.dictSend["idx"] = Controller.instance.enemData.playId.ToString();
                Controller.instance.dictSend["count"] = Controller.instance.enemData.playCnt.ToString();
                Controller.instance.PacketSend("play/poll/battle", "stay");
            }

            netCycle = 0;
        }

        if (!end && isStart)
        {
            if (myturn)
            {
                Check();
                timeleft -= Time.deltaTime;
                textTimer.text = (int)timeleft + "";

                if (timeleft <= 0)   //타이머 만료시 턴 전환
                {
                    cnt = 0;
                }
                else if(timeleft <= 10)   //10초 이하 색상변경
                {
                    textTimer.color = Color.red;
                }

                if (cnt == 3)   //카운트 소진에 따른 처리
                {
                    imgCount1.gameObject.SetActive(true);
                    imgCount2.gameObject.SetActive(true);
                    imgCount3.gameObject.SetActive(true);

                    togSkill.gameObject.SetActive(true);
                }
                else if (cnt == 2)
                {
                    imgCount1.gameObject.SetActive(true);
                    imgCount2.gameObject.SetActive(true);
                    imgCount3.gameObject.SetActive(false);

                    togSkill.gameObject.SetActive(true);
                }
                else if (cnt == 1)
                {
                    imgCount1.gameObject.SetActive(true);
                    imgCount2.gameObject.SetActive(false);
                    imgCount3.gameObject.SetActive(false);

                    togSkill.interactable = false;
                    togSkill.gameObject.SetActive(false);
                }
                else if (cnt == 0 && !move && !attack)   //카운트 모두 소진시 초기화 및 턴전환 
                {
                    imgCount1.gameObject.SetActive(false);
                    imgCount2.gameObject.SetActive(false);
                    imgCount3.gameObject.SetActive(false);

                    togSkill.isOn = false;
                    togSkill.interactable = true;
                    togSkill.gameObject.SetActive(false);

                    for (int i = 0; i < 11; i++)
                    {
                        ResetJob(objMyUnits[i]);
                        ResetJob(objEnUnits[i]);
                    }
                    
                    obj = null;
                    lastobj = null;

                    togAction.isOn = false;
                    myturn = false;

                    Controller.instance.ownData.playCnt = 3;
                    Controller.instance.dictSend["idx"] = Controller.instance.enemData.playId.ToString();
                    Controller.instance.PacketSend("play/change", "stay");
                }

                if (!move && !attack)   //액션,스킬 토글 전환 처리
                {
                    if (togSkill.isOn && !isSkillOnce)
                    {
                        ChangetoSkill();
                        isSkillOnce = true;
                        isAttackOnce = false;
                        isMoveOnce = false;
                    }

                    if (!togSkill.isOn)
                    {
                        if (togAction.isOn && !isMoveOnce)
                        {
                            change = true;
                            ChangetoAttack();
                            isMoveOnce = true;
                            isAttackOnce = false;
                        }

                        if (!togAction.isOn && !isAttackOnce)
                        {
                            change = false;
                            ChangetoMove();
                            isAttackOnce = true;
                            isMoveOnce = false;
                        }

                        isSkillOnce = false;
                    }
                }
            }

            if (!myturn)   //턴에 따른 ui 전환
            {
                objTimer.SetActive(true);
                togAction.gameObject.SetActive(false);
                togSkill.gameObject.SetActive(false);
                textTimer.gameObject.SetActive(false);
                enemTimer.gameObject.SetActive(true);

                objPlayer2.transform.position = posSecond;
                objPlayer1.transform.position = posFirst;
                objPlayer2.GetComponent<RectTransform>().SetSiblingIndex(2);
                objPlayer1.GetComponent<RectTransform>().SetSiblingIndex(1);
            }
            else
            {
                objTimer.SetActive(true);
                togAction.gameObject.SetActive(true);
                togSkill.gameObject.SetActive(true);
                textTimer.gameObject.SetActive(true);
                enemTimer.gameObject.SetActive(false);

                objPlayer1.transform.position = posSecond;
                objPlayer2.transform.position = posFirst;
                objPlayer1.GetComponent<RectTransform>().SetSiblingIndex(2);
                objPlayer2.GetComponent<RectTransform>().SetSiblingIndex(1);
            }
        }
    }

    public void Check()   //캐릭터 클릭 체크
    {
        string strTempName = "";

        if (lastobj != null)   //마지막 선택 오브젝트 체크
        {
            strTempName = lastobj.name;
        }

        if (Input.GetMouseButtonUp(0) && myturn && !end && !togSkill.isOn)
        {
            Ray ray;
            if (!cam2.isActiveAndEnabled)
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
                if (hit.transform.tag != "Tile" && !hit.transform.gameObject.GetComponent<Move>().enemy && hit.transform.name != strTempName && !move && !attack)   //마지막 선택헀던 오브젝트 재선택 방지
                {
                    if (hit.transform.tag == "War" || hit.transform.tag == "Arc" || hit.transform.tag == "Mag" || hit.transform.tag == "Pri" || hit.transform.tag == "King")
                    {
                        Controller.instance.soundData.clipUi = eUi.choice;

                        if (!hit.transform.GetComponent<Move>().isCheckHeal)   //힐 상태 선택 방지
                        {
                            if (obj != null)   //이전 캐릭터 선택 해제
                            {
                                if (!change)   
                                {
                                    obj.GetComponent<Move>().turn = false;
                                }
                                else
                                {
                                    if (obj.transform.tag == "Pri")
                                    {
                                        obj.GetComponent<Heal>().turn = false;
                                    }
                                    else
                                    {
                                        obj.GetComponent<Attack>().turn = false;
                                    }
                                }
                            }

                            obj = hit.transform.gameObject;

                            if (!change)   //현재 캐릭터 선택
                            {
                                obj.GetComponent<Move>().turn = true;
                            }
                            else
                            {
                                if (obj.transform.tag == "Pri")
                                {
                                    obj.GetComponent<Heal>().turn = true;
                                }
                                else
                                {
                                    obj.GetComponent<Attack>().turn = true;
                                }
                            }
                        }
                    }

                    lastobj = obj;

                    isMoveOnce = false;
                    isAttackOnce = false;
                    isSkillOnce = false;
                }
            }
        }
    }
    
    void ChangetoMove()   //이동 상태로 전환 처리
    {
        foreach (GameObject ob in objMyUnits)
        {
            if (ob.transform.tag == "Pri")
            {
                ob.GetComponent<TacticHeal>().RemoveSkillTiles();
                ob.GetComponent<TacticHeal>().turn = false;
                ob.GetComponent<TacticHeal>().RemoveSelectableTiles();
                ob.GetComponent<TacticHeal>().enabled = false;
            }
            else
            {
                if (ob.transform.tag == "King")
                {
                    ob.GetComponent<TacticMove>().RemoveSelectableTiles();
                }

                ob.GetComponent<TacticAttack>().turn = false;
                ob.GetComponent<TacticAttack>().RemoveSelectableTiles();
                ob.GetComponent<TacticAttack>().enabled = false;
            }

            ob.GetComponent<TacticMove>().enabled = true;
            ob.GetComponent<TacticMove>().isOnceRange = false;

            if (lastobj != null)
            {
                if (lastobj.transform.tag == "Pri")
                {
                    lastobj.GetComponent<TacticHeal>().turn = true;
                }
                else
                {
                    lastobj.GetComponent<TacticAttack>().turn = true;
                }
            }
        }
    }

    void ChangetoAttack()   //공격 상태로 전환 처리
    {
        foreach (GameObject ob in objMyUnits)
        {
            if (ob.transform.tag == "Pri")
            {
                ob.GetComponent<TacticHeal>().RemoveSkillTiles();
                ob.GetComponent<TacticHeal>().isOnceRange = false;
            }
            else
            {
                ob.GetComponent<TacticAttack>().RemoveSelectableTiles();
                ob.GetComponent<TacticAttack>().isOnceRange = false;
            }

            ob.GetComponent<TacticMove>().turn = false;
            ob.GetComponent<TacticMove>().RemoveSelectableTiles();
            ob.GetComponent<TacticMove>().enabled = false;

            if (ob.transform.tag == "Pri")
            {
                ob.GetComponent<TacticHeal>().enabled = true;
            }
            else
            {
                ob.GetComponent<TacticAttack>().enabled = true;
            }

            if (lastobj != null)
            {
                lastobj.GetComponent<TacticMove>().turn = true;
            }
        }
    }

    void ChangetoSkill()   //스킬 사용 상태로 전환 처리
    {
        foreach (GameObject ob in objMyUnits)
        {
            if (ob.transform.tag == "King")
            {
                ob.GetComponent<TacticMove>().isOnceRange = false;
                ob.GetComponent<TacticAttack>().turn = false;
                ob.GetComponent<TacticAttack>().RemoveSelectableTiles();
                ob.GetComponent<TacticAttack>().enabled = false;
            }
            else
            {
                if (ob.transform.tag == "Pri")
                {
                    ob.GetComponent<TacticHeal>().RemoveSelectableTiles();
                }
                else
                {
                    ob.GetComponent<TacticAttack>().isOnceRange = false;
                }

                ob.GetComponent<TacticMove>().RemoveSelectableTiles();
                ob.GetComponent<TacticMove>().turn = false;
                ob.GetComponent<TacticMove>().enabled = false;
            }

            if (ob.transform.tag == "Pri")
            {
                ob.GetComponent<TacticHeal>().enabled = true;
            }
            else if (ob.transform.tag == "King")
            {
                ob.GetComponent<TacticMove>().enabled = true;
            }
            else if (ob.transform.tag == "War" || ob.transform.tag == "Arc" || ob.transform.tag == "Mag")
            {
                ob.GetComponent<TacticAttack>().enabled = true;
            }

            if (lastobj != null)
            {
                if (lastobj.transform.tag == "King")
                {
                    lastobj.GetComponent<TacticAttack>().turn = true;
                }
                else
                {
                    lastobj.GetComponent<TacticMove>().turn = true;
                }
            }
        }
    }

    public void OnChangeAction()   //액션 토글 클릭
    {
        Controller.instance.soundData.clipUi = eUi.action;
    }

    public void OnChangeSkill()   //스킬 토글 클릭
    {
        Controller.instance.soundData.clipUi = eUi.skill;
    }

    public void OnChangeCam()   //뷰 전환
    {
        Controller.instance.soundData.clipUi = eUi.button;

        if (cam1.isActiveAndEnabled == true)
        {
            cam1.gameObject.SetActive(false);
            cam2.gameObject.SetActive(true);
            for (int cnt = 0; cnt < 11; cnt++)
            {
                myTactics[cnt].topview = true;
            }
        }
        else
        {
            cam2.gameObject.SetActive(false);
            cam1.gameObject.SetActive(true);
            for (int cnt = 0; cnt < 11; cnt++)
            {
                myTactics[cnt].topview = false;
            }
        }
    }

    public void OnReadyBtn()   //준비완료 서버 전송
    {
        Controller.instance.soundData.clipUi = eUi.button;
        Controller.instance.dictSend["ready"] = "true";
        Controller.instance.PacketSend("play/ready", "stay");

        btnReady.gameObject.SetActive(false);
        btnOut.gameObject.SetActive(false);
    }

    public void OnReGame()   //게임 재시작 서버 전송
    {
        Controller.instance.soundData.clipUi = eUi.button;
        Controller.instance.dictSend["ready"] = "false";
        Controller.instance.PacketSend("play/ready", "stay");
        Controller.instance.ResetPlay();
    }

    public void OnOut()   //게임 나가기 서버 전송
    {
        Controller.instance.soundData.clipUi = eUi.button;
        Controller.instance.PacketSend("play/out", "stay");
        Controller.instance.ResetPlay();

        isOut = true;
    }

    public void OnMsgOut()   //상대방 나감 메세지 확인
    {
        Controller.instance.soundData.clipUi = eUi.button;

        OnReGame();

        curtain.SetActive(false);
        objMsgOut.SetActive(false);
    }

    public void OnMsgCount()   //카운트 모두 소진 메세지 확인
    {
        Controller.instance.soundData.clipUi = eUi.button;

        OnOut();

        curtain.SetActive(false);
        objMsgOut.SetActive(false);
    }

    public void SendOwnData()   //자신 데이터 전송
    {
        Controller.instance.dictSend["count"] = Controller.instance.ownData.playCnt.ToString();
        Controller.instance.dictSend["position"] = string.Join(",", Controller.instance.ownData.arrPosJob);
        Controller.instance.dictSend["attack"] = string.Join(",", Controller.instance.ownData.arrHitJob);
        Controller.instance.PacketSend("play/reset", "stay");
    }

    public void ResetJob(string myColor, string enColor)   //캐릭터 정보 초기화
    {
        Transform temp;

        temp = GameObject.Find(myColor).transform;
        objMyUnits = new GameObject[temp.childCount];

        for (int i = 0; i < temp.childCount; i++)
        {
            objMyUnits[i] = temp.GetChild(i).gameObject;
            myTactics[i] = objMyUnits[i].GetComponent<Move>();

            Controller.instance.ownData.arrPosJob[i] = myTactics[i].myPosition;
            Controller.instance.ownData.arrHitJob[i] = "99";
        }

        temp = GameObject.Find(enColor).transform;
        objEnUnits = new GameObject[temp.childCount];

        for (int i = 0; i < temp.childCount; i++)
        {
            objEnUnits[i] = temp.GetChild(i).gameObject;
            enTactics[i] = objEnUnits[i].GetComponent<Move>();

            Controller.instance.enemData.arrPosJob[i] = enTactics[i].myPosition;
            Controller.instance.enemData.arrHitJob[i] = "99";
        }

        SendOwnData();
    }

    void ResultWin()   //게임 결과 승리처리, 서버전송
    {
        Controller.instance.dictSend["win"] = "true";
        Controller.instance.PacketSend("play/result", "stay");
        Controller.instance.soundData.clipBgm = eBgm.win;

        objResultBg.gameObject.SetActive(true);
        winImage.gameObject.SetActive(true);
        btnOut.gameObject.SetActive(true);
        btnReplay.gameObject.SetActive(true);

        ResetUI();
    }

    void ResultLose()   //게임 결과 패배처리, 서버전송
    {
        Controller.instance.dictSend["win"] = "false";
        Controller.instance.PacketSend("play/result", "stay");
        Controller.instance.soundData.clipBgm = eBgm.lose;

        objResultBg.gameObject.SetActive(true);
        loseImage.gameObject.SetActive(true);
        btnOut.gameObject.SetActive(true);
        btnReplay.gameObject.SetActive(true);

        ResetUI();
    }

    void Coin_Firstatk()   //코인 선공 처리
    {
        coin.transform.eulerAngles = new Vector3(-45, 0, 0);
        StartCoroutine(CoinRoll(2.0f));
    }

    void Coin_Secondatk()   //코인 후공 처리
    {
        coin.transform.eulerAngles = new Vector3(-45, 0, 180);
        StartCoroutine(CoinRoll(2.0f));
    }

    IEnumerator CoinRoll(float k)   //코인 처리후 초기화
    {
        yield return new WaitForSeconds(k);

        rolltheCoin = false;
        isStart = true;

        coin.SetActive(false);
        curtain.SetActive(false);
        btnCam.gameObject.SetActive(true);
    }

    void ResetUI()   //Ui 끄기 
    {
        imgCount1.gameObject.SetActive(false);
        imgCount2.gameObject.SetActive(false);
        imgCount3.gameObject.SetActive(false);

        textTimer.gameObject.SetActive(false);
        togAction.gameObject.SetActive(false);
        togSkill.gameObject.SetActive(false);
        objTimer.SetActive(false);
        btnCam.gameObject.SetActive(false);
    }

    void ResetJob(GameObject job)   //캐릭터 상태 초기화
    {
        job.GetComponent<TacticMove>().turn = false;
        job.GetComponent<TacticMove>().isSkill = false;
        job.GetComponent<TacticMove>().RemoveSelectableTiles();

        if (job.transform.tag == "Pri")
        {
            job.GetComponent<TacticHeal>().turn = false;
            job.GetComponent<TacticHeal>().isSkill = false;
            job.GetComponent<TacticHeal>().RemoveSelectableTiles();
            job.GetComponent<TacticHeal>().RemoveSkillTiles();
        }
        else
        {
            job.GetComponent<TacticAttack>().turn = false;
            job.GetComponent<TacticAttack>().isSkill = false;
            job.GetComponent<TacticAttack>().RemoveSelectableTiles();
        }
    }

    void UpdatePlay(Packet packet)   //플레이 관련 서버 갱신 처리
    {
        if (packet.type == "recv" && this != null)
        {
            if (packet.url == "play/poll/match")   //매칭 갱신 처리
            {
                if (packet.result == "true")
                {
                    textPlayer2.text = Controller.instance.enemData.playName;

                    objPlayer2.SetActive(true);
                    btnReady.gameObject.SetActive(true);
                    curtain.SetActive(true);
                }
                else
                {
                    objPlayer2.SetActive(false);
                    btnReady.gameObject.SetActive(false);
                    curtain.SetActive(false);
                }
            }
            else if (packet.url == "play/poll/ready")   //게임준비 갱신 처리
            {
                if (packet.result == "true")
                {
                    if (Controller.instance.ownData.playState == 1)
                    {
                        myturn = true;

                        WAlly.SetActive(true);
                        BEnemy.SetActive(true);

                        ResetJob(WAlly.name, BEnemy.name);
                    }
                    else if (Controller.instance.ownData.playState == 2)
                    {
                        myturn = false;

                        BAlly.SetActive(true);
                        WEnemy.SetActive(true);

                        ResetJob(BAlly.name, WEnemy.name);
                    }

                    rolltheCoin = true;
                }
                else if (packet.result == "out")   //준비중 상대방 나감 처리
                {
                    OnReGame();
                }
            }
            else if (packet.url == "play/poll/battle" && !move && !attack && !end)   //플레이 갱신 처리
            {
                if (packet.result == "true")   //턴 전환 처리
                {
                    if (Controller.instance.ownData.playState == 1)
                    {
                        cnt = 3;
                        timeleft = 61;
                        textTimer.color = Color.white;

                        myturn = true;
                    }
                    else if (Controller.instance.ownData.playState == 2)
                    {
                        myturn = false;
                    }
                }
                else if (packet.result == "out")   //플레이중 상대방 나감 처리
                {
                    isStart = false;
                    myturn = false;
                    end = true;

                    curtain.SetActive(true);
                    objMsgOut.SetActive(true);
                }
            }
            else if (packet.url == "play/out" && isOut)   //로비로 이동 처리
            {
                SceneManager.LoadScene(1);
            }
            else if (packet.url == "play/ready" && packet.result == "off")   //준비중 재시작
            {
                SceneManager.LoadScene(2);
            }
            else if (packet.url == "play/result")   //플레이 결과 처리
            {
                if (Controller.instance.userData.count == 0)
                {
                    curtain.SetActive(true);
                    objMsgCount.SetActive(true);

                    btnReplay.interactable = false;
                    btnOut.interactable = false;
                }
            }
        }
    }
}