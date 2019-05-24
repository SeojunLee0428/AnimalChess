using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Info : MonoBehaviour
{
    public Text textCount;
    public Text textCoin;

    void Start()
    {
        Controller.eventLobby += this.UpdateLobby;
    }

    void Update()
    {
        textCount.text = Controller.instance.userData.count.ToString();
        textCoin.text = Controller.instance.userData.coin.ToString();
    }

    void UpdateLobby(Packet packet)   //카운트,코인 변화 갱신 
    {
        if (this != null && packet.type == "recv" && packet.result == "lobby/info")
        {
            textCount.text = Controller.instance.userData.count.ToString();
            textCoin.text = Controller.instance.userData.coin.ToString();
        }
    }
}
