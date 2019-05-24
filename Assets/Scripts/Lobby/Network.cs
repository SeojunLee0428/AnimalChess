
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class Network : MonoBehaviour
{
    Queue<Packet> queSendPacket;
    Queue<string> queRecvPacket;

    bool isDelay;   //받은 패킷 처리 delay 상태

    string ipNumber;   //ip, port Url 주소 정보

    public void Start()
    {
        ipNumber = "http://121.174.248.13:8910/";

        queSendPacket = new Queue<Packet>();
        queRecvPacket = new Queue<string>();
        
        isDelay = false;

        Controller.eventJoin += this.UpdateQueue;
        Controller.eventLogin += this.UpdateQueue;
        Controller.eventLobby += this.UpdateQueue;
        Controller.eventUsers += this.UpdateQueue;
        Controller.eventAllChat += this.UpdateQueue;
        Controller.eventRooms += this.UpdateQueue;
        Controller.eventPlay += this.UpdateQueue;
        Controller.delay += this.UpdateDelay;
    }

    public void Update()
    {
        if (queSendPacket.Count != 0)
        {
            StartCoroutine(PostMessage());
        }

        if(queRecvPacket.Count != 0 && !isDelay)
        {
            Controller.instance.RecvPacket(queRecvPacket.Dequeue());
        }
    }

    IEnumerator PostMessage()   //보낼 패킷 받은 패킷 처리
    {
        WWWForm form = new WWWForm();
        Packet packet = queSendPacket.Dequeue();

        string strUrl = packet.url;
        string strData = packet.data;

        byte[] sendData = UTF8Encoding.UTF8.GetBytes(strData);

        Hashtable header = new Hashtable();
        header.Add("Content-Type", "application/json");

        if (Controller.instance.userData.isLogin == true)
        {
            header.Add("Cookie", Controller.instance.userData.session);
        }

        WWW www = new WWW(ipNumber + strUrl, sendData, header);

        yield return www;

        if (www.error == null)
        {
            if (www.responseHeaders.ContainsKey("SET-COOKIE") && strUrl == "login")   //로그인 세션 맺기
            {
                char[] splitter = { ';' };
                string[] response = www.responseHeaders["SET-COOKIE"].Split(splitter);

                foreach (string session in response)
                {
                    if (string.IsNullOrEmpty(session))
                    {
                        continue;
                    }

                    if (session.Contains("session"))
                    {
                        Controller.instance.userData.session = session;
                        Controller.instance.userData.isLogin = true;

                        break;
                    }
                }
            }

            queRecvPacket.Enqueue(www.text);
        }
        else
        {
            Debug.Log("error : " + www.error);
        }
    }

    void UpdateQueue(Packet packet)   //이벤트 발생시 보낼 패킷 축적
    {
        if(packet.type == "send")
        {
            queSendPacket.Enqueue(packet);
        }
    }

    void UpdateDelay(bool delay)   //delay 요청
    {
        isDelay = delay;
    }

    private void OnApplicationQuit()   //프로그램 종료 시 즉각 처리 요청 
    {
        WWWForm form = new WWWForm();
        Hashtable header = new Hashtable();

        header.Add("Content-Type", "application/json");

        if (Controller.instance.userData.isLogin == true)
        {
            header.Add("Cookie", Controller.instance.userData.session);
        }

        if (Controller.instance.ownData.isStay)
        {
            string strData = "stay";
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(strData);

            WWW www = new WWW(ipNumber + "play/out", sendData, header);
        }

        string strData2 = "logout";
        byte[] sendData2 = UTF8Encoding.UTF8.GetBytes(strData2);

        WWW www2 = new WWW(ipNumber + "logout", sendData2, header);
    }
}