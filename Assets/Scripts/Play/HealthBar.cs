using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Sprite[] sprArrEmblem = new Sprite[5];
    public ClickManager scriptCM;

    GameObject objJob;
    GameObject objEmblem;
    GameObject objSelect;

    Camera Cam1;
    Camera Cam2;
    Camera CamAction;

    Slider sliderHpBar;
    
    Vector3 vecRot;
    
    void Start()
    {
        sliderHpBar = transform.GetChild(3).gameObject.GetComponent<Slider>();
        objEmblem = transform.GetChild(2).gameObject;
        objSelect = transform.GetChild(1).gameObject;
        objJob = transform.parent.gameObject;
        scriptCM = GameObject.Find("ClickManager").GetComponent<ClickManager>();

        Cam1 = scriptCM.cam1;
        Cam2 = scriptCM.cam2;
        CamAction = scriptCM.ActionCam;
        //직업에 따른 문양 변경, 색상 설정
        if (objJob.tag == "King")
        {
            objEmblem.GetComponent<Image>().sprite = sprArrEmblem[0];
            objEmblem.GetComponent<Image>().color = new Color(255 / 255f, 200f / 255f, 0f / 255f);
        }
        else if (objJob.tag == "War")
        {
            objEmblem.GetComponent<Image>().sprite = sprArrEmblem[1];
            objEmblem.GetComponent<Image>().color = new Color(255 / 255f, 100f / 255f, 0f/ 255f);
        }
        else if (objJob.tag == "Arc")
        {
            objEmblem.GetComponent<Image>().sprite = sprArrEmblem[2];
            objEmblem.GetComponent<Image>().color = new Color(0f / 255f, 200f / 255f, 255 / 255f);
        }
        else if (objJob.tag == "Mag")
        {
            objEmblem.GetComponent<Image>().sprite = sprArrEmblem[3];
            objEmblem.GetComponent<Image>().color = new Color(255 / 255f, 0f / 255f, 200f / 255f);
        }
        else if (objJob.tag == "Pri")
        {
            objEmblem.GetComponent<Image>().sprite = sprArrEmblem[4];
            objEmblem.GetComponent<Image>().color = new Color(0f / 255f, 255 / 255f, 100f / 255f);
        }

        sliderHpBar.maxValue = objJob.GetComponent<TacticMove>().health;
    }

    void LateUpdate()
    {
        sliderHpBar.value = objJob.GetComponent<TacticMove>().health;
        
        if(scriptCM.ActionCam.isActiveAndEnabled)   //체력바 카메라 시선 변경
        {
            vecRot = Quaternion.LookRotation(CamAction.transform.position - transform.forward).eulerAngles;
            
            transform.rotation = Quaternion.Euler(-vecRot);
        }
        else if (Cam2.isActiveAndEnabled)
        {
            vecRot.x = 90;
            vecRot.y = 0;
            vecRot.z = 0;

            transform.rotation = Quaternion.Euler(vecRot);
        }
        else
        {
            vecRot = Quaternion.LookRotation(Cam1.transform.position - transform.forward).eulerAngles;

            vecRot.y = 0;
            vecRot.z = 0;

            transform.rotation = Quaternion.Euler(-vecRot);
        }

        if (objJob.tag == "King" || objJob.tag == "War" || objJob.tag == "Arc" || objJob.tag == "Mag")   //선택 캐릭 이미지 활성화
        {
            if (objJob.GetComponent<TacticMove>().turn && objJob.GetComponent<TacticAttack>().turn)
            {
                objSelect.SetActive(true);
            }
            else
            {
                objSelect.SetActive(false);
            }
        }
        else
        {
            if (objJob.GetComponent<TacticMove>().turn && objJob.GetComponent<TacticHeal>().turn)
            {
                objSelect.SetActive(true);
            }
            else
            {
                objSelect.SetActive(false);
            }
        }
    }
}
