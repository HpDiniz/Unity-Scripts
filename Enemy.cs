using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    /*
    public float health = 70;
    PlayerMovement target;
    private Collider [] colliders;
    private Rigidbody [] rigidBodies;
    private Collider myCollider;
    private Animator myAnimator;
    // Start is called before the first frame update
    void Start()
    {
        colliders = GetComponentsInChildren<Collider>();
        rigidBodies = GetComponentsInChildren<Rigidbody>();
        myAnimator = GetComponent<Animator>();

        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float amount)
    {   
        Debug.Log(health);
        health = (health - amount < 0) ? 0 : (health - amount);
        CheckDeath();
    }

    void CheckDeath()
    {
        if(health <= 0)
        {
            target = null;
            myAnimator.enabled = false;

            foreach (Rigidbody rb in rigidBodies)
            {
                rb.isKinematic = true;
                rb.useGravity = true;
            }

        }
    }
    */
}
