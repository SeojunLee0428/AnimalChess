using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public enum eBgm   //BGM 사운드 열거
{
    idle,
    login,
    lobby,
    wait,
    play,
    win,
    lose
}

public enum eUi   //UI 사운드 열거
{
    idle,
    button,
    toggle,
    action,
    skill,
    choice,
    select,
    charge
}

public enum ePlayer   //플레이 캐릭터 사운드 열거
{
    idle,
    own,
    enem,
    skillMove
}

public enum eAction   //플레이 행동 사운드 열거
{
    idle,
    attack,
    heal,
    fire,
    skillAttack,
    skillArrow,
    skillHeal,
    skillFire,
    swingAttack,
    swingArrow,
    swingJump,
    swingFire,
    walk1,
    walk2
}

public class Controller   //데이터 및 네트워크 관리 싱글턴 클래스
{
    private static Controller Instance = null;

    public static Controller instance
    {
        get
        {
            if (Instance == null)
            {
                Instance = new Controller();
            }
            return Instance;
        }
    }
    //Data 클래스 객체선언
    public Packet packRecv;   

    public UserData userData;
    public LobbyData lobbyData;

    public PlayData ownData;
    public PlayData enemData;

    public SoundData soundData;
    //패킷 구성 dictionary 
    public Dictionary<string, string> dictSend = new Dictionary<string, string>();
    public Dictionary<string, string> dictRecv = new Dictionary<string, string>();
    //네트워크 연결된 이벤트 처리 delegate
    public delegate void DelegateJoin(Packet dataJoin);   
    public static event DelegateJoin eventJoin;

    public delegate void DelegateLogin(Packet dataLogin);
    public static event DelegateLogin eventLogin;

    public delegate void DelegateLobby(Packet dataLobby);
    public static event DelegateLobby eventLobby;

    public delegate void DelegateUsers(Packet dataUsers);
    public static event DelegateUsers eventUsers;

    public delegate void DelegateAllChat(Packet dataAllChat);
    public static event DelegateAllChat eventAllChat;

    public delegate void DelegateRoom(Packet dataRooms);
    public static event DelegateRoom eventRooms;

    public delegate void DelegatePlay(Packet dataPlay);
    public static event DelegatePlay eventPlay;

    public delegate void DelegateDelay(bool change);
    public static DelegateDelay delay;

    public Controller()   //클래스 초기화
    {
        packRecv = new Packet("recv");

        userData = new UserData();
        lobbyData = new LobbyData();

        ownData = new PlayData("King", "War1", "War2", "War3", "War4", "Arc1", "Arc2", "Mag1", "Mag2", "Pri1", "Pri2", 0, 0, 3, false, false, false);
        enemData = new PlayData("EKing", "EWar1", "EWar2", "EWar3", "EWar4", "EArc1", "EArc2", "EMag1", "EMag2", "EPri1", "EPri2", 0, 0, 3, false, false, false);

        soundData = new SoundData(0.5f, 0.5f, eBgm.idle, eUi.idle, ePlayer.idle, eAction.idle, false, false);

        dictSend = new Dictionary<string, string>();
        dictRecv = new Dictionary<string, string>();
    }

    public void PacketSend(string url, string result)   //보낼 패킷, 연결된 delegate 이벤트 처리
    {
       if(url == "users" || url == "allchat" || url == "rooms/poll" || url == "play/poll/match" || url == "play/poll/ready" || url == "play/poll/battle")
       {
            Packet pollSend = new Packet("send");   //지속 요청 패킷

            pollSend.data = JsonConvert.SerializeObject(dictSend);
            pollSend.url = url;
            pollSend.result = result;

            dictSend.Clear();

            if (url == "users")   //로비 유저 목록 갱신
            {
                eventUsers(pollSend);
            }
            else if(url == "allchat")   //로비 채팅 갱신
            {
                eventAllChat(pollSend);
            }
            else if(url == "rooms/poll")   //로비 방 목록 갱신
            {
                eventRooms(pollSend);
            }
            else if(url == "play/poll/match" || url == "play/poll/ready" || url == "play/poll/battle")   //플레이 갱신
            {
                eventPlay(pollSend);
            }
       }
       else
       {
            Packet packSend = new Packet("send");   //일회 요청 패킷

            packSend.data = JsonConvert.SerializeObject(dictSend);
            packSend.url = url;
            packSend.result = result;

            dictSend.Clear();

            if (url == "join")   //회원가입요청
            {
                eventJoin(packSend);
            }
            else if(url == "login" || url == "logout")   //로그인,로그아웃 요청
            {
                eventLogin(packSend);
            }
            else if(url == "lobby/info" || url == "lobby/rank" || url == "store")   //로비 정보,랭킹,상점 요청
            {
                eventLobby(packSend);
            }
            else if (url == "rooms/create" || url == "rooms/match" || url == "rooms/select")   //방 관련 요청
            {
                eventRooms(packSend);
            }
            else if(url== "play/reset" || url == "play/ready" || url == "play/change" || url == "play/delay" || url == "play/out" || url == "play/result")   //플레이 관련 요청
            {
                eventPlay(packSend);
            }
        }
    }

    public void RecvPacket(string recvData) //받은 패킷, 연결된 delegate 이벤트 처리
    {
        dictRecv = JsonConvert.DeserializeObject<Dictionary<string, string>>(recvData);

        packRecv.url = dictRecv["url"];
        packRecv.result = dictRecv["result"];

        if (packRecv.url == "join")   //회원 가입 처리
        {
            eventJoin(packRecv);
        }
        else if (packRecv.url == "login" || packRecv.url == "logout")   //로그인,로그아웃 처리
        {
            eventLogin(packRecv);
        }
        else if (packRecv.url == "lobby/info" || packRecv.url == "lobby/rank" || packRecv.url == "store")
        {
            if (packRecv.url == "lobby/info")   //사용자 정보 갱신 처리
            {
                userData.nick = dictRecv["nick"];
                userData.coin = int.Parse(dictRecv["coin"]);
                userData.count = int.Parse(dictRecv["count"]);
            }
            else if (packRecv.url == "lobby/rank")   //랭킹 갱신 처리
            {
                userData.win = int.Parse(dictRecv["win"]);
                userData.lose = int.Parse(dictRecv["lose"]);
                userData.rate = float.Parse(dictRecv["rate"]);
                userData.rank = dictRecv["rank"];

                for (int iRank = 0; iRank < 3; iRank++)   //랭커 갱신 처리
                {
                    lobbyData.arrRanker[iRank] = dictRecv["ranks" + iRank.ToString()];
                }
            }
            else if (packRecv.url == "store" && packRecv.result == "true")   //상점 구매처리된 값 갱신 처리
            {
                userData.coin = int.Parse(dictRecv["coin"]);
                userData.count = int.Parse(dictRecv["count"]);
            }

            eventLobby(packRecv);
        }
        else if (packRecv.url == "users" && packRecv.result == "true")   //유저 목록 갱신
        {
            lobbyData.listUsers.Clear();
            lobbyData.cntUsers = dictRecv["users"];

            for (int iUser = 0; iUser < dictRecv.Count - 3; iUser++)   //패킷 기본 구성 3개 제외한 data 리스트화
            {
                lobbyData.listUsers.Add(dictRecv["data" + iUser.ToString()]);
            }

            eventUsers(packRecv);
        }
        else if (packRecv.url == "allchat" && packRecv.result == "true")   //채팅 내역 갱신
        {
            Array.Clear(lobbyData.arrChat, 0, lobbyData.arrChat.Length);

            for (int iChat = 0; iChat < (dictRecv.Count - 2) / 3; iChat++)   //채팅 닉네임,메세지,시간 묶음 분류
            {
                if (iChat < ((dictRecv.Count - 2) / 3) - 1)   //지난 채팅기록
                {
                    lobbyData.arrChat[0, iChat] = dictRecv["nick" + iChat.ToString()] + " : " + dictRecv["message" + iChat.ToString()] + "\n";
                    lobbyData.arrChat[1, iChat] = dictRecv["time" + iChat.ToString()];
                }

                if (iChat == (dictRecv.Count - 2) / 3 - 1)   //새로 입력된 채팅
                {
                    lobbyData.arrChat[0, iChat] = dictRecv["nick" + iChat.ToString()] + " : " + dictRecv["message" + iChat.ToString()];
                    lobbyData.arrChat[1, iChat] = dictRecv["time" + iChat.ToString()];
                    lobbyData.timeChat = dictRecv["time" + iChat.ToString()];
                }
            }
            eventAllChat(packRecv);
        }
        else if (packRecv.url == "rooms/poll" && packRecv.result == "true")   //방 목록 갱신
        {
            lobbyData.listRooms.Clear();
            lobbyData.cntRooms = dictRecv["rooms"];

            for (int iNick = 0; iNick < dictRecv.Count - 3; iNick++)
            {
                lobbyData.listRooms.Add(dictRecv["nick" + iNick.ToString()]);
            }

            eventRooms(packRecv);
        }
        else if (packRecv.url == "rooms/create" || packRecv.url == "rooms/match" || packRecv.url == "rooms/select")   //방 관련 처리
        {
            eventRooms(packRecv);
        }
        else if (packRecv.url == "play/poll/match")   //플레이 매칭 갱신
        {
            if (packRecv.result == "true")
            {
                ownData.playId = int.Parse(dictRecv["own"]);
                enemData.playId = int.Parse(dictRecv["enemy"]);
                enemData.playName = dictRecv["nick"];
            }

            eventPlay(packRecv);
        }
        else if (packRecv.url == "play/poll/ready")   //플레이 준비상태 갱신
        { 
            if (packRecv.result == "true")
            {
                if (dictRecv["ownReady"] == "1")
                {
                    ownData.isReady = true;
                }

                if (dictRecv["enemReady"] == "1")
                {
                    enemData.isReady = true;
                }

                ownData.playState = int.Parse(dictRecv["ownState"]);
                enemData.playState = int.Parse(dictRecv["enemState"]);
            }

            eventPlay(packRecv);
        }
        else if (packRecv.url == "play/poll/battle")   //플레이 정보 갱신
        {
            if (packRecv.result == "out")   //상대방 나감
            {
                eventPlay(packRecv);
            }
            else if ((packRecv.result == "true" && enemData.playCnt != int.Parse(dictRecv["count"])) || enemData.playState != int.Parse(dictRecv["state"]))
            {
                string posJob = dictRecv["position"];
                string hitJob = dictRecv["attack"];

                if (enemData.playState != int.Parse(dictRecv["state"]))   //턴 체인지
                {
                    ownData.playState = enemData.playState;
                    enemData.playState = int.Parse(dictRecv["state"]);
                }

                if (enemData.playCnt - int.Parse(dictRecv["count"]) == 2)   //스킬 사용 판정
                {
                    enemData.isSkill = true;
                }

                enemData.playCnt = int.Parse(dictRecv["count"]);
                enemData.arrPosJob = posJob.Split(',');
                enemData.arrHitJob = hitJob.Split(',');

                for (int iJob = 0; iJob < 11; iJob++)   //상대방 좌표 역전환 
                {
                    if (enemData.arrPosJob[iJob] != "99")
                    {
                        int tempPos = 88 - int.Parse(enemData.arrPosJob[iJob]);

                        if (tempPos < 10)
                        {
                            enemData.arrPosJob[iJob] = "0" + tempPos.ToString();
                        }
                        else
                        {
                            enemData.arrPosJob[iJob] = tempPos.ToString();
                        }
                    }
                }

                eventPlay(packRecv);
            }
        }
        else if(packRecv.url == "play/out" || packRecv.url == "play/ready")   // 나가기,준비 요청 처리
        {
            eventPlay(packRecv);
        }
        else if( packRecv.url == "play/result")   //게임 결과 판정 처리
        {
            userData.count = int.Parse(dictRecv["count"]);
            eventPlay(packRecv);
        }

        dictRecv.Clear();
    }

    public void ResetLog()   //로그인 유저 데이터 초기화
    {
        UserData tempUserData = new UserData();
        LobbyData tempLobbyData = new LobbyData();

        userData = tempUserData;
        lobbyData = tempLobbyData;
    }

    public void ResetPlay()   //플레이 데이터 초기화
    {
        LobbyData tempLobbyData = new LobbyData();
        PlayData tempOwnData = new PlayData("King", "War1", "War2", "War3", "War4", "Arc1", "Arc2", "Mag1", "Mag2", "Pri1", "Pri2", 0, 0, 3, false, false, false);
        PlayData tempEnemData = new PlayData("EKing", "EWar1", "EWar2", "EWar3", "EWar4", "EArc1", "EArc2", "EMag1", "EMag2", "EPri1", "EPri2", 0, 0, 3, false, false, false);

        lobbyData = tempLobbyData;
        ownData = tempOwnData;
        enemData = tempEnemData;
    }
}