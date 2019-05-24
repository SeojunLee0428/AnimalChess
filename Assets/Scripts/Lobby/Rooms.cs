using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Rooms : MonoBehaviour
{
    public RectTransform contentRoom;

    public Button btnSelect;
    public Button btnCreate;
    public Button btnMatch;

    List<Button> listBtn;

    float netCycle;

    void Start()
    {
        Controller.eventRooms += this.UpdateRooms;
        listBtn = new List<Button>();
    }

    private void Update()
    {
        netCycle += Time.deltaTime;

        if (netCycle > 2.0f)   //방 목록 지속 요청
        {
            Controller.instance.dictSend["rooms"] = Controller.instance.lobbyData.cntRooms;
            Controller.instance.PacketSend("rooms/poll", "stay");

            netCycle = 0;
        }
        
        if(Controller.instance.userData.count == 0)   //카운트 전부 소진시 게임 불가
        {
            for (int iBtn = 0; iBtn < listBtn.Count; iBtn++)
            {
                listBtn[iBtn].interactable = false;
            }

            btnCreate.interactable = false;
            btnMatch.interactable = false;
        }
        else
        {
            for (int iBtn = 0; iBtn < listBtn.Count; iBtn++)
            {
                listBtn[iBtn].interactable = true;
            }

            btnCreate.interactable = true;
            
            if (listBtn.Count == 0)   //방목록 없을시 자동매칭 비활성화
            {
                btnMatch.interactable = false;
            }
            else
            {
                btnMatch.interactable = true;
            }
        }
    }

    public void OnCreateBtn()   //방 생성 요청
    {
        Controller.instance.soundData.clipUi = eUi.button;

        Controller.instance.PacketSend("rooms/create", "stay");
    }
     
    public void OnMatchBtn()   //자동 매칭 요청
    {
        Controller.instance.soundData.clipUi = eUi.button;

        Controller.instance.PacketSend("rooms/match", "stay");
    }

    public void OnSelectBtn()   //선택한 방 입장 요청
    {
        Controller.instance.soundData.clipUi = eUi.button;

        Text btnText = EventSystem.current.currentSelectedGameObject.gameObject.transform.GetChild(0).GetComponent<Text>();

        Controller.instance.dictSend["select"] = btnText.text;
        Controller.instance.PacketSend("rooms/select", "stay");
    }

    void UpdateRooms(Packet packet)   //방 관련 요청 처리
    {
        if (this != null)
        {
            if (packet.type == "recv" && packet.url == "rooms/poll")
            {
                for (int iBtn = 0; iBtn < listBtn.Count; iBtn++)   //기존 방목록 제거
                {
                    Destroy(listBtn[iBtn].gameObject);
                }

                listBtn.Clear();

                for (int iRoom = 0; iRoom < Controller.instance.lobbyData.listRooms.Count; iRoom++)   //방 목록 갱신
                {
                    Button newRooms = Instantiate(btnSelect);
                    Text newText = newRooms.transform.GetChild(0).GetComponent<Text>();

                    newText.text = Controller.instance.lobbyData.listRooms[iRoom];
                    newRooms.transform.SetParent(contentRoom);
                    newRooms.transform.localScale = Vector3.one;
                    newRooms.onClick.AddListener(OnSelectBtn);

                    listBtn.Add(newRooms);
                }
            }
            else if (packet.type == "recv")   //방 입장
            {
                SceneManager.LoadScene("PlayScene");
            }
        }
    }
}
