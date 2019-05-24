using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Out : MonoBehaviour
{
    public GameObject objOut;

    public Toggle togOut;
    
    void Start()
    {
        Controller.eventLogin += this.UpdateLogin;
    }

    void Update()
    {
        if (togOut.isOn)
        {
            objOut.SetActive(true);
        }
        else
        {
            objOut.SetActive(false);
        }
    }

    public void OnLogoutBtn()   //로그아웃 요청
    {
        Controller.instance.soundData.clipUi = eUi.button;

        if (Controller.instance.ownData.isStay)
        {
            Controller.instance.PacketSend("play/out", "stay");
            Controller.instance.ResetPlay();
        }

        Controller.instance.PacketSend("logout", "logout");
        Controller.instance.ResetLog();
    }

    public void OnExitBtn()   //프로그랭 종료 요청
    {
        Controller.instance.soundData.clipUi = eUi.button;

        if (Controller.instance.ownData.isStay)
        {
            Controller.instance.PacketSend("play/out", "stay");
            Controller.instance.ResetPlay();
        }

        Controller.instance.PacketSend("logout", "logout");
        Controller.instance.ResetLog();

        Application.Quit();
    }

    public void OnChangeToggle()   //나가기 토글 전환
    {
        Controller.instance.soundData.clipUi = eUi.toggle;
    }

    void UpdateLogin(Packet packet)   //로그아웃 요청 결과 처리
    {
        if (this != null && packet.type == "recv" && packet.result == "logout")
        {
            SceneManager.LoadScene(0);
        }
    }
}
