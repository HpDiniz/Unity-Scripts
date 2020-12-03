using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPosition : MonoBehaviour
{

    public void Invisible()
    {
        this.gameObject.transform.localScale = new Vector3(0f,0f,0f);
    }
}
