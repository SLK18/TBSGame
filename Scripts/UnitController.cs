using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitController : MonoBehaviour
{
    public Team team = Team.blue;
    public TextMesh unitInfo;
    public float moveRange, attackRange;
    public abstract IEnumerator Attack(GameObject g);
    public abstract IEnumerator Aim(GameObject g);

    protected int armor, health, AP, damage;
    protected const int UnitLayerMask = 3;

    private const float DistanceToComplete = 10f;
    private float currDistance = 0f;
    private const float MaxMoveSpeed = 50f, acceleration = 50f;

    void Start()
    {
        unitInfo.text = health.ToString();
    }

    void Update()
    {
        unitInfo.text = health.ToString();
        unitInfo.transform.rotation = Quaternion.LookRotation(unitInfo.transform.position - Camera.main.transform.position);
    }

    public IEnumerator Navigate(Vector3 targetWaypoint)
    {
        bool navigationComplete = false;
        Rigidbody rb;
        float totalVel = 0f;

        while (!navigationComplete)
        {
            try
            {
                rb = gameObject.GetComponent<Rigidbody>();
                
                transform.rotation = Quaternion.LookRotation(targetWaypoint - transform.position);

                if (CheckForObstacle())
                    transform.position += new Vector3(0, 1f, 0);

                targetWaypoint = new Vector3(targetWaypoint.x, transform.position.y, targetWaypoint.z);



                totalVel = Mathf.Abs(rb.velocity[0]) + Mathf.Abs(rb.velocity[1]) + Mathf.Abs(rb.velocity[2]);
                if (totalVel < MaxMoveSpeed)
                    rb.AddForce((targetWaypoint - transform.position).normalized * acceleration);
                

                currDistance = Vector3.Distance(targetWaypoint, transform.position);
                if (currDistance < DistanceToComplete)
                    navigationComplete = true;
            }
            catch
            {
                navigationComplete = true;
                //Was destroyed while navigating
            }

            yield return null;
        }

        try 
        { 
            Debug.Log("Nav complete");
            //Get rid of pitch and roll
            transform.localRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            //Get rid of velocity
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        catch
        {
            //Was destroyed while navigating
        }
        
    }

    private bool CheckForObstacle()
    {
        RaycastHit hitForward, hitLeft, hitRight;
        const float ScanDistance = 30f;

        //Scan three forward directions for terrain
        if (Physics.Raycast(transform.position, transform.forward, out hitForward, ScanDistance))
        {
            if (hitForward.collider != null)
                return true;
        }

        if (Physics.Raycast(transform.position, transform.forward - (transform.right / 2f), out hitLeft, ScanDistance))
        {
            if (hitLeft.collider != null)
                return true;
        }

        if (Physics.Raycast(transform.position, transform.forward + (transform.right / 2f), out hitRight, ScanDistance))
        {
            if (hitRight.collider != null)
                return true;
        }

        return false;
    }

    public bool HasLineOfSight(Vector3 targetPosition)
    {
        return !Physics.Linecast(transform.position, targetPosition, UnitLayerMask);
    }

    public void InflictDamage(int armorPierce, int dmg)
    {
        Debug.Log("inflict called " + armorPierce + " " + dmg);
        if (armorPierce >= armor)
        {
            health -= dmg;
            if (health <= 0)
            {
                Destroy(transform.root.gameObject);
            }
        }
    }
}
