using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rank : MonoBehaviour
{
    public GameObject objNet;

    public Text textFirst;
    public Text textSecond;
    public Text textThird;

    bool isOnce;

    void Start ()
    {
        Controller.eventLobby += this.UpdateLobby;

        isOnce = false;
    }

    private void Update()
    {
        if (objNet.activeSelf == true && !isOnce)   //랭킹 정보 요청
        {
            Controller.instance.PacketSend("lobby/rank", "stay");

            isOnce = true;
        }
    }

    void UpdateLobby(Packet packet)   //순위 정보 갱신
    {
        if (this != null)
        {
            if (packet.type == "recv" && packet.url == "lobby/rank")
            {
                textFirst.text = Controller.instance.lobbyData.arrRanker[0];
                textSecond.text = Controller.instance.lobbyData.arrRanker[1];
                textThird.text = Controller.instance.lobbyData.arrRanker[2];
            }
        }
    }
}