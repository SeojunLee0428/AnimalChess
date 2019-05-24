using System.Collections;
using System.Collections.Generic;

public class Packet   // 패킷 구성
{
    public Packet(string type)
    {
        this.type = type;
    }

    public string url;
    public string type;
    public string data;
    public string result;

    public bool isState;
}

public class UserData   // 유저 데이터
{
    public string nick;
    public string session;   //맺어진 세션 문자열
    public string rank;

    public float rate;

    public int win;
    public int lose;
    public int count;
    public int coin;

    public bool isLogin;
}

public class LobbyData   // 로비 데이터
{
    public List<string> listUsers = new List<string>();
    public List<string> listRooms = new List<string>();

    public string[] arrRanker = new string[3];
    public string[,] arrChat = new string[2,51];

    public string cntUsers;
    public string cntRooms;
    public string timeChat;
}

public class PlayData   // 플레이 데이터
{
    public PlayData(string job0, string job1, string job2, string job3, string job4, string job5, string job6, string job7, string job8, string job9, string job10, int playId, int state, int playCnt, bool ready,bool stay,bool skill)
    {
        this.arrNameJob[0] = job0;
        this.arrNameJob[1] = job1;
        this.arrNameJob[2] = job2;
        this.arrNameJob[3] = job3;
        this.arrNameJob[4] = job4;
        this.arrNameJob[5] = job5;
        this.arrNameJob[6] = job6;
        this.arrNameJob[7] = job7;
        this.arrNameJob[8] = job8;
        this.arrNameJob[9] = job9;
        this.arrNameJob[10] = job10;
        this.playId = playId;
        this.playState = state;
        this.playCnt = playCnt;
        this.isReady = ready;
        this.isStay = stay;
        this.isSkill = skill;
    }

    public string[] arrNameJob = new string[11];   //캐릭터 이름 배열
    public string[] arrPosJob = new string[11];   //캐릭터 위치 배열
    public string[] arrHitJob = new string[11];   //공격 위치 배열

    public string playName;

    public int playId;
    public int playState;
    public int playCnt;

    public bool isReady;
    public bool isStay;
    public bool isSkill;
}

public class SoundData   // 사운드 데이터
{
    public SoundData(float volBgm, float volUi ,eBgm clipBgm, eUi clipEffect, ePlayer clipPlayer, eAction clipAction, bool mute, bool isChangeWalk)
    {
        this.volBgm = volBgm;
        this.volUi = volUi;
        this.clipBgm = clipBgm;
        this.clipUi = clipEffect;
        this.clipPlayer = clipPlayer;
        this.clipAction = clipAction;
        this.isMute = mute;
        this.isChangeWalk = isChangeWalk;
    }

    public float volBgm;
    public float volUi;

    public eBgm clipBgm;
    public eUi clipUi;
    public ePlayer clipPlayer;
    public eAction clipAction;

    public bool isMute;
    public bool isChangeWalk;
}