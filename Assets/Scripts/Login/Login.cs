using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    public GameObject panelLogin;
    public GameObject panelJoin;

    public InputField inputId;
    public InputField inputPass;

    public Button btnJoin;
    public Button btnLogin;

    public Text textLogin;

    void Awake()
    {
        Controller.eventLogin += this.UpdateLogin;
        Controller.instance.soundData.clipBgm = eBgm.login;
    }

    public void OnJoinBtn()   //회원가입 창 이동
    {
        Controller.instance.soundData.clipUi = eUi.button;

        inputId.text = string.Empty;
        inputPass.text = string.Empty;
        textLogin.text = string.Empty;

        panelJoin.SetActive(true);
        panelLogin.SetActive(false);
    }

    public void OnLoginBtn()   //로그인 정보 입력 확인
    {
        Controller.instance.soundData.clipUi = eUi.button;

        if (inputId.text != string.Empty || inputPass.text != string.Empty)
        {
            Controller.instance.dictSend["id"] = inputId.text;
            Controller.instance.dictSend["passwd"] = inputPass.text;
            Controller.instance.PacketSend("login", "stay");

            btnJoin.interactable = false;
            btnLogin.interactable = false;
        }
        else
        {
            textLogin.text = "빈곳을 모두 입력해 주세요..";
        }
    }

    public void UpdateLogin(Packet packet)   //로그인 요청 결과 처리
    {
        inputId.text = string.Empty;
        inputPass.text = string.Empty;

        btnJoin.interactable = true;
        btnLogin.interactable = true;

        if (this != null)
        { 
            if (packet.result == "stay")
            {
                textLogin.text = "로그인 처리중 입니다..";
            }
            else if (packet.result == "true")
            {
                textLogin.text = "로그인에 성공 하셨습니다..";

                SceneManager.LoadScene(1);
            }
            else if (packet.result == "false")
            {
                textLogin.text = "로그인에 실패 하셨습니다..";
            }
            else if (packet.result == "logon")
            {
                textLogin.text = "이미 접속중인 아이디 입니다..";
            }
        }
    }
}

