using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShell : Projectile
{
    void Start()
    {
        AP = 5;
        damage = 5;
        GetComponent<Rigidbody>().AddForce(transform.forward * thrust);
        Destroy(transform.root.gameObject, 7f);
    }
}
