using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public GameObject Rhand;
    public GameObject posTarget;
    public GameObject arrow;
    public GameObject Carrow;

    public Vector3 CharPos;
    public float speed;

    Vector3 ros;
    Vector3 posMine;

    CArrow Car;

    bool isShoot;

    void Start()
    {
        Car = Carrow.GetComponent<CArrow>();
        Car.RenderEnFalse();

        isShoot = false;

        ros.x = CharPos.x;
        ros.y = CharPos.y + 0.6f;
        ros.z = CharPos.z;
    }

    public CArrow GetCArrow()   //CArrow 반환
    {
        return Car;
    }
  
    void Update()
    {
        if (isShoot == false&& Car.RenderEnb == true)   //화살 미발사시 렌더 끔
        {
            Car.RenderEnFalse();
        }
        else if (isShoot == true)
        {
            if (Car.RenderEnb == false )   //화살 준비
            {
                arrow.transform.position = Rhand.transform.position;
                arrow.transform.LookAt(posTarget.transform);
                Car.RenderEnTrue();
            }
            else if (arrow.transform.position.x == posTarget.transform.position.x && arrow.transform.position.z == posTarget.transform.position.z)   //화살 원위치
            {
                arrow.transform.position = Rhand.transform.position;
                isShoot = false;
                Car.RenderEnFalse();
            }
            else if (arrow.transform.position.x != posTarget.transform.position.x && arrow.transform.position.z != posTarget.transform.position.z)   //화살 발사
            {
                Vector3 var = posTarget.transform.position;
                var tacy = var.y + 0.5f;
                var = new Vector3(var.x, tacy, var.z);
                arrow.transform.position = Vector3.MoveTowards(arrow.transform.position, var, speed * Time.deltaTime);
            }
        }
    }  

    public void Shoot()   //화살 발사 체크
    {
        isShoot = true;
    }
}
