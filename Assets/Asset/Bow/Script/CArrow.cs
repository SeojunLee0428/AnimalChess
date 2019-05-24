using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CArrow : MonoBehaviour
{
    public bool RenderEnb;

    void Start()
    {
        RenderEnb = false;
        this.GetComponent<Renderer>().enabled = false;
    }

    public void RenderEnTrue()
    {
        this.GetComponent<Renderer>().enabled = true;
        RenderEnb = true;
    }

    public void RenderEnFalse()
    {
        this.GetComponent<Renderer>().enabled = false;
        RenderEnb = false;
    }
}
