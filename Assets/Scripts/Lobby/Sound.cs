using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sound : MonoBehaviour
{
    public GameObject objSound;
    //사운드 ui
    public Toggle togSound;
    public Toggle togMute;
    
    public Slider slidBgm;
    public Slider slidUi;
    //오디오 소스
    public AudioSource sourceBgm;
    public AudioSource sourceUi;
    public AudioSource sourcePlayer;
    public AudioSource sourceAction;
    //오디오 BGM 클립
    public AudioClip clipBgmLogin;
    public AudioClip clipBgmLobby;
    public AudioClip clipBgmWait;
    public AudioClip clipBgmPlay;
    public AudioClip clipBgmWin;
    public AudioClip clipBgmLose;
    //오디오 UI 클립
    public AudioClip clipBtn;
    public AudioClip clipToggle;
    public AudioClip clipAction;
    public AudioClip clipSkill;
    public AudioClip clipChoice;
    public AudioClip clipSelect;
    public AudioClip clipCharge;
    //오디오 액션 클립
    public AudioClip clipAttack;
    public AudioClip clipHeal;
    public AudioClip clipFire;
    public AudioClip clipSkillAttack;
    public AudioClip clipSkillArrow;
    public AudioClip clipSkillHeal;
    public AudioClip clipSkillFire;
    public AudioClip clipSkillMove;
    public AudioClip clipSwingAttack;
    public AudioClip clipSwingArrow;
    public AudioClip clipSwingJump;
    public AudioClip clipSwingFire;
    public AudioClip clipWalk1;
    public AudioClip clipWalk2;
    //오디오 캐릭터 클립
    public AudioClip clipPlayeOwn;
    public AudioClip clipPlayeEnem;
    
    void Start()
    {
        togMute.isOn = Controller.instance.soundData.isMute;
        slidBgm.value = Controller.instance.soundData.volBgm;
        slidUi.value = Controller.instance.soundData.volUi;
    }

    void Update()
    {
        if (togSound.isOn)   //사운드 토글
        {
            objSound.SetActive(true);
        }
        else
        {
            objSound.SetActive(false);
        }
        //사운드 Ui 연결
        Controller.instance.soundData.isMute = togMute.isOn;
        Controller.instance.soundData.volBgm = slidBgm.value;
        Controller.instance.soundData.volUi = slidUi.value;
        //사운드 음소거
        sourceBgm.mute = Controller.instance.soundData.isMute;
        sourceUi.mute = Controller.instance.soundData.isMute;
        sourcePlayer.mute = Controller.instance.soundData.isMute;
        sourceAction.mute = Controller.instance.soundData.isMute;
        //사운드 볼륨 컨트롤
        sourceBgm.volume = Controller.instance.soundData.volBgm;
        sourceUi.volume = Controller.instance.soundData.volUi;
        sourcePlayer.volume = Controller.instance.soundData.volUi;
        sourceAction.volume = Controller.instance.soundData.volUi;

        if (Controller.instance.soundData.clipBgm != eBgm.idle)   //BGM 사운드 실행
        {
            if (Controller.instance.soundData.clipBgm == eBgm.login)
            {
                sourceBgm.clip = clipBgmLogin;
            }
            else if (Controller.instance.soundData.clipBgm == eBgm.lobby)
            {
                sourceBgm.clip = clipBgmLobby;
            }
            else if (Controller.instance.soundData.clipBgm == eBgm.wait)
            {
                sourceBgm.clip = clipBgmWait;
            }
            else if (Controller.instance.soundData.clipBgm == eBgm.play)
            {
                sourceBgm.clip = clipBgmPlay;
            }
            else if (Controller.instance.soundData.clipBgm == eBgm.win)
            {
                sourceBgm.clip = clipBgmWin;
            }
            else if (Controller.instance.soundData.clipBgm == eBgm.lose)
            {
                sourceBgm.clip = clipBgmLose;
            }

            sourceBgm.loop = true;
            sourceBgm.Play();

            Controller.instance.soundData.clipBgm = eBgm.idle;
        }

        if (Controller.instance.soundData.clipUi != eUi.idle)   //UI 사운드 실행
        {
            if (Controller.instance.soundData.clipUi == eUi.button)
            {
                sourceUi.PlayOneShot(clipBtn);
            }
            else if (Controller.instance.soundData.clipUi == eUi.toggle)
            {
                sourceUi.PlayOneShot(clipToggle);
            }
            else if(Controller.instance.soundData.clipUi == eUi.action)
            {
                sourceUi.PlayOneShot(clipAction);
            }
            else if(Controller.instance.soundData.clipUi == eUi.skill)
            {
                sourceUi.PlayOneShot(clipSkill);
            }
            else if (Controller.instance.soundData.clipUi == eUi.choice)
            {
                sourceUi.PlayOneShot(clipChoice);
            }
            else if (Controller.instance.soundData.clipUi == eUi.select)
            {
                sourceUi.PlayOneShot(clipSelect);
            }
            else if (Controller.instance.soundData.clipUi == eUi.charge)
            {
                sourceUi.PlayOneShot(clipCharge);
            }

            Controller.instance.soundData.clipUi = eUi.idle;
        }

        if (Controller.instance.soundData.clipAction != eAction.idle)   //액션 사운드 실행
        {
            if (Controller.instance.soundData.clipAction == eAction.attack)
            {
                sourceAction.PlayOneShot(clipAttack);
            }
            else if (Controller.instance.soundData.clipAction == eAction.heal)
            {
                sourceAction.PlayOneShot(clipHeal);
            }
            else if (Controller.instance.soundData.clipAction == eAction.fire)
            {
                sourceAction.PlayOneShot(clipFire);
            }
            else if (Controller.instance.soundData.clipAction == eAction.skillAttack)
            {
                sourceAction.PlayOneShot(clipSkillAttack);
            }
            else if (Controller.instance.soundData.clipAction == eAction.skillArrow)
            {
                sourceAction.PlayOneShot(clipSkillArrow);
            }
            else if (Controller.instance.soundData.clipAction == eAction.skillFire)
            {
                sourceAction.PlayOneShot(clipSkillFire);
            }
            else if (Controller.instance.soundData.clipAction == eAction.skillHeal)
            {
                sourceAction.PlayOneShot(clipSkillHeal);
            }
            else if (Controller.instance.soundData.clipAction == eAction.swingAttack)
            {
                sourceAction.PlayOneShot(clipSwingAttack);
            }
            else if (Controller.instance.soundData.clipAction == eAction.swingArrow)
            {
                sourceAction.PlayOneShot(clipSwingArrow);
            }
            else if (Controller.instance.soundData.clipAction == eAction.swingFire)
            {
                sourceAction.PlayOneShot(clipSwingFire);
            }
            else if (Controller.instance.soundData.clipAction == eAction.swingJump)
            {
                sourceAction.PlayOneShot(clipSwingJump);
            }
            else if (Controller.instance.soundData.clipAction == eAction.walk1 && !sourceAction.isPlaying && !Controller.instance.soundData.isChangeWalk)
            {
                sourceAction.PlayOneShot(clipWalk1);
                Controller.instance.soundData.isChangeWalk = true;
            }
            else if (Controller.instance.soundData.clipAction == eAction.walk2 && !sourceAction.isPlaying && Controller.instance.soundData.isChangeWalk)
            {
                sourceAction.PlayOneShot(clipWalk2);
                Controller.instance.soundData.isChangeWalk = false;
            }

            Controller.instance.soundData.clipAction = eAction.idle;
        }

        if (Controller.instance.soundData.clipPlayer != ePlayer.idle)   //캐릭터 사운드 실행
        {
            if (Controller.instance.soundData.clipPlayer == ePlayer.own)
            {
                sourcePlayer.PlayOneShot(clipPlayeOwn);
                Controller.instance.soundData.clipPlayer = ePlayer.idle;
            }
            else if (Controller.instance.soundData.clipPlayer == ePlayer.enem && !sourcePlayer.isPlaying)
            {
                sourcePlayer.PlayOneShot(clipPlayeEnem);
                Controller.instance.soundData.clipPlayer = ePlayer.idle;
            }
            else if (Controller.instance.soundData.clipPlayer == ePlayer.skillMove && !sourcePlayer.isPlaying)
            {
                sourcePlayer.PlayOneShot(clipSkillMove);
                Controller.instance.soundData.clipPlayer = ePlayer.idle;
            }
        }
    }

    public void OnChangeToggle()   //사운드 토글 전환
    {
        Controller.instance.soundData.clipUi = eUi.toggle;
    }
}
