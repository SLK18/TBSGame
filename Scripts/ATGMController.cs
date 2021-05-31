using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATGMController : Projectile
{
    private GameObject target;
    private Rigidbody rb;

    void Start()
    {
        AP = 5;
        damage = 5;
        thrust = 5f;
        rb = gameObject.GetComponent<Rigidbody>();
        Destroy(transform.root.gameObject, 20f);
    }

    public void SetTarget(GameObject t)
    {
        target = t;
    }

    void Update()
    {
        try
        {
            transform.LookAt(target.transform.position);
        }
        catch
        {
            //Target gets destroyed mid flight
        }
        rb.AddRelativeForce(Vector3.forward * thrust);
    }
}
