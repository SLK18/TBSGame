using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTRShell : Projectile
{
    void Start()
    {
        AP = 2;
        damage = 2;
        GetComponent<Rigidbody>().AddForce(transform.forward * thrust);
        Destroy(transform.root.gameObject, 7f);
    }
}
