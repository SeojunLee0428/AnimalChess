using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class State : MonoBehaviour
{
    public Text textNick;
    public Text textWL;
    public Text textRate;
    public Text textRank;
    public Text textTear;

    public GameObject objNet;

    bool isOnce;

    void Awake()
    {
        Controller.instance.soundData.clipBgm = eBgm.lobby;
        Controller.eventLobby += this.UpdateLobby;

        textTear.text = string.Empty;
        isOnce = false;
    }

    private void Update()
    {
        if (objNet.activeSelf == true && !isOnce)   //로비 정보 요청
        {
            Controller.instance.PacketSend("lobby/info", "stay");

            isOnce = true;
        }


        if(Controller.instance.userData.rank != null)
        {
            if (Controller.instance.userData.rank == "--")   //랭크별 등급 표시 변경
            {
                textTear.text = "NEW";
                textTear.fontSize = 34;
            }
            else
            {
                if (int.Parse(Controller.instance.userData.rank) <= 3)
                {
                    textTear.text = "S";
                }
                else if (int.Parse(Controller.instance.userData.rank) > 3 && int.Parse(Controller.instance.userData.rank) <= 10)
                {
                    textTear.text = "A";
                }
                else if (int.Parse(Controller.instance.userData.rank) > 10 && int.Parse(Controller.instance.userData.rank) <= 30)
                {
                    textTear.text = "B";
                }
                else if (int.Parse(Controller.instance.userData.rank) > 30 && int.Parse(Controller.instance.userData.rank) <= 100)
                {
                    textTear.text = "C";
                }
                else if (int.Parse(Controller.instance.userData.rank) > 100 && int.Parse(Controller.instance.userData.rank) <= 300)
                {
                    textTear.text = "D";
                }
                else if (int.Parse(Controller.instance.userData.rank) > 300 && int.Parse(Controller.instance.userData.rank) <= 1000)
                {
                    textTear.text = "E";
                }
                else if (int.Parse(Controller.instance.userData.rank) > 1000)
                {
                    textTear.text = "F";
                }

                textTear.fontSize = 68;
            }
        }
    }

    void UpdateLobby(Packet packet)   //유저 정보 요청 결과 처리
    {
        if (this != null) 
        {
            if (packet.type == "recv" && packet.url == "lobby/info")
            {
                textNick.text = textNick.text + Controller.instance.userData.nick;
            }
            else if (packet.type == "recv" && packet.url == "lobby/rank")
            {
                textWL.text = textWL.text + Controller.instance.userData.win + " / " + Controller.instance.userData.lose;
                textRate.text = textRate.text + Controller.instance.userData.rate;
                textRank.text = textRank.text + Controller.instance.userData.rank;
            }
        }
    }
}