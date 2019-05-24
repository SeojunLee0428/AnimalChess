using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Join : MonoBehaviour
{
    public GameObject panelJoin;
    public GameObject panelLogin;

    public InputField inputId;
    public InputField inputPass;
    public InputField inputNick;

    public Button btnInput;
    public Button btnBack;

    public Text textJoin;

    void Start()
    {
        Controller.eventJoin += this.UpdateJoin;   
    }

    public void OnInputBtn()   //회원정보 입력 확인
    {
        Controller.instance.soundData.clipUi = eUi.button;

        if (inputId.text != "" || inputPass.text != "" || inputNick.text != "")
        {
            Controller.instance.dictSend["id"] = inputId.text;
            Controller.instance.dictSend["passwd"] = inputPass.text;
            Controller.instance.dictSend["nick"] = inputNick.text;
            Controller.instance.PacketSend("join", "stay");

            btnInput.interactable = false;
            btnBack.interactable = false;
        }
        else
        {
            textJoin.text = "빈곳을 모두 입력해 주세요..";
        }
    }

    public void OnBackBtn()   //로그인 창으로 돌아가기
    {
        Controller.instance.soundData.clipUi = eUi.button;

        inputId.text = string.Empty;
        inputPass.text = string.Empty;
        inputNick.text = string.Empty;
        textJoin.text = string.Empty;

        panelLogin.SetActive(true);
        panelJoin.SetActive(false);
    }

    void UpdateJoin(Packet packet)   //회원가입 요청 결과 처리
    {
        if (this != null)
        {
            if (packet.result == "stay")
            {
                textJoin.text = "회원 등록 중입니다..";
            }
            else if (packet.result == "true")
            {
                textJoin.text = "회원등록에 성공 하셨습니다..";

                panelLogin.SetActive(true);

                Controller.instance.dictSend["id"] = inputId.text;
                Controller.instance.dictSend["passwd"] = inputPass.text;
                Controller.instance.PacketSend("login", "stay");
                
                panelJoin.SetActive(false);
            }
            else if (packet.result == "false")
            {
                textJoin.text = "이미 존재하는 회원정보입니다..";
            }
        }

        btnInput.interactable = true;
        btnBack.interactable = true;
    }
}
