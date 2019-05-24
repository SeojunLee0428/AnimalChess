using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginCam : MonoBehaviour
{
    float uptime;

    void Awake()
    {
        uptime = 0;
    }
    
    void Update()   //로그인 카메라 무빙
    {
        uptime += Time.deltaTime;

        if (uptime < 5.0f)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 2);
        }
        else
        {
            transform.Rotate(Vector3.down * Time.deltaTime * 2);

            if (uptime > 10.0f)
            {
                uptime = 0;
            }
        }
    }
}
