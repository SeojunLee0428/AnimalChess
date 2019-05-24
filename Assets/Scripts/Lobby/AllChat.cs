using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllChat : MonoBehaviour
{
    public InputField inputChat;
    public Button btnChat;
    public Text textChat;

    float netCycle;

    void Start()
    {
        Controller.eventAllChat += this.UpdateAllChat;
    }

    private void Update()
    {
        netCycle += Time.deltaTime;

        if (netCycle > 1.0f)   //채팅 내역 주기적 요청
        {
            Controller.instance.dictSend["state"] = "poll";
            Controller.instance.dictSend["time"] = Controller.instance.lobbyData.timeChat;
            Controller.instance.dictSend["input"] = string.Empty;
            Controller.instance.PacketSend("allchat", "stay");

            netCycle = 0;
        }
    }

    public void OnChatBtn()   //채팅 입력 확인, 등록 요청
    {
        Controller.instance.soundData.clipUi = eUi.button;

        if (inputChat.text != string.Empty)
        {
            Controller.instance.dictSend["state"] = "push";
            Controller.instance.dictSend["time"] = Controller.instance.lobbyData.timeChat;
            Controller.instance.dictSend["input"] = inputChat.text;
            Controller.instance.PacketSend("allchat", "stay");

            inputChat.text = string.Empty;
        }
    }

    void UpdateAllChat(Packet packet)   //채팅 내역 요청 결과 갱신
    {
        if (packet.type == "recv" && this != null)
        {
            textChat.text = string.Empty;
            int count = 0;

            while(true)
            {
                if (Controller.instance.lobbyData.arrChat[0, count] != null)
                {
                    textChat.text += Controller.instance.lobbyData.arrChat[0, count];
                    count++;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
