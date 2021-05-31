using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public GameObject explosion;

    protected float thrust = 2000f;
    protected int AP, damage;
    protected float velocity;

    void Start()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * thrust);
        Destroy(transform.root.gameObject, 7f);
    }

    protected virtual void OnCollisionEnter(Collision col)
    {
        try
        {
            col.gameObject.GetComponent<UnitController>().InflictDamage(AP, damage);
        }
        catch
        {
            Debug.Log("Couldn't find UnitController script");
        }

        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(transform.root.gameObject);
    }
}
