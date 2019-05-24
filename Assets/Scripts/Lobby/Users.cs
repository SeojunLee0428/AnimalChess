using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Users : MonoBehaviour
{
    public Text textUsers;

    float netCycle;
    float textWidth;
    float textHeight;
    
    void Start ()
    {
        Controller.eventUsers += this.UpdateUsers;
        Controller.instance.dictSend["users"] = string.Empty;
        Controller.instance.PacketSend("users", "stay");
    }

    private void Update()
    {
        netCycle += Time.deltaTime;
        
        if (netCycle > 2.0f)   //유저 목록 지속 요청
        {
            Controller.instance.dictSend["users"] = Controller.instance.lobbyData.cntUsers;
            Controller.instance.PacketSend("users", "stay");

            netCycle = 0;
        }
    }

    void UpdateUsers(Packet packet)   //유저 목록 요청 결과 처리
    {
        if (packet.type == "recv" && this != null)
        {
            textUsers.text = string.Empty;

            textWidth = textUsers.rectTransform.rect.width;
            textHeight = textUsers.rectTransform.rect.height;

            for (int i = 0; i < Controller.instance.lobbyData.listUsers.Count; i++)
            {
                textUsers.text += Controller.instance.lobbyData.listUsers[i] + "\n";
                textUsers.rectTransform.sizeDelta = new Vector2(textWidth, textHeight += 25);
            }
        }
    }
}
