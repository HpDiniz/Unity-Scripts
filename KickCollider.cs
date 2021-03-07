using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickCollider : MonoBehaviour
{   
    /*
    void Update()
    {
         if( GetComponent<MeshFilter>().mesh.bounds.Contains())
            {
                Debug.Log("Bounding box contains hidden object!");
            }
    }
    */
    
    void OnTriggerStay(Collider other) 
    {   
        Debug.Log("Trigger: " + other.name);
    }

    void OnTriggerEnter(Collider other) 
    {   
        Debug.Log("Trigger: " + other.name);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision: " + collision.gameObject.name);
    }

    void OnCollisionStay(Collision collision)
    {
        Debug.Log("Collision: " + collision.gameObject.name);
    }
    
}
