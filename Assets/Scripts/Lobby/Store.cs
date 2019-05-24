using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Store : MonoBehaviour
{
    public GameObject objStore;
    public GameObject objBuy;
    public GameObject objResult;

    public Toggle togStore;
    public Text textResult;
    public Text textCount;
    public Text textCoin;

    string itemNumber;

    void Start()
    {
        Controller.eventLobby += this.UpdateLobby;

        itemNumber = "";
    }

    void Update()
    {
        if (togStore.isOn)
        {
            objStore.SetActive(true);
        }
        else
        {
            objStore.SetActive(false);
        }
    }

    public void OnStore()   //선택한 아이템 정보 등록
    {
        Controller.instance.soundData.clipUi = eUi.button;

        itemNumber = EventSystem.current.currentSelectedGameObject.gameObject.transform.GetChild(1).GetComponent<Text>().text;

        objBuy.SetActive(true);
    }

    public void OnBuy()   //구매 확정 요청
    {
        Controller.instance.soundData.clipUi = eUi.button;

        Text onBtn = EventSystem.current.currentSelectedGameObject.gameObject.transform.GetChild(0).GetComponent<Text>();

        if (onBtn.text == "Ok")
        {
            Controller.instance.dictSend["item"] = itemNumber;
            Controller.instance.PacketSend("store", "stay");
        }

        objBuy.SetActive(false);
    }

    public void OnResult()   //구매 결과 확인
    {
        Controller.instance.soundData.clipUi = eUi.button;

        objResult.SetActive(false);
    }

    public void OnChangeToggle()   //상점 토글 전환
    {
        Controller.instance.soundData.clipUi = eUi.toggle;
    }

    void UpdateLobby(Packet packet)   //상점 이용 결과 처리
    {
        if (this != null && packet.type == "recv" && packet.url == "store")
        {
            objResult.SetActive(true);

            if (packet.result == "true")
            {
                textCount.text = "";
                textCoin.text = "";

                textResult.text = "구매 성공 하셨습니다..";

                textCount.text = Controller.instance.userData.count.ToString();
                textCoin.text = Controller.instance.userData.coin.ToString();
            }
            else if (packet.result == "false")
            {
                textResult.text = "코인이 부족합니다..";
            }
        }
    }
}
