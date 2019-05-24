using UnityEngine;
using System.Collections;

public class BowString : MonoBehaviour {

    [Range(0.0f, 1.0f)]
    public float factor;

    Vector3 firstPosition;
    Vector3 lastPosition;

    public bool stretching;
    public bool releasing;

    public float stretchSpeed = 0.5f;
    public float releaseSpeed = 0.9f;
    int st;

    void Start () {
        firstPosition = transform.localPosition;
        lastPosition = transform.localPosition + Vector3.up * 0.45f;
        st = 0;
    }
	
	void Update () {
        
        if (stretching)
        {
            factor = 0.7f;

            if(st==2)
            {
                Release();
            }
            st++;
            
        }
        if (releasing)
        {
           
            factor -= releaseSpeed* Time.deltaTime;

            if (factor < 0.0f)
            {
                factor = 0.0f;
                st = 0;
            }
        }

        transform.localPosition = Vector3.Lerp(firstPosition, lastPosition, factor);
    }

    public void Stretch()
    {
        
        stretching = true;
        releasing = false;
    }

   public  void Release()
    {
        
        releasing = true;
        stretching = false;
    }
}
