using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurretedVehicleController : UnitController
{
    public GameObject turret, barrel, shell;

    private const float RotationSpeed = 120f, RequiredAccuracy = .99999f;
    private const float SpawnDistance = 12f;

    //Ref https://answers.unity.com/questions/1153173/turret-rotation-based-on-its-own-local-rotation.html
    public override IEnumerator Aim(GameObject g)
    {
        bool isDoneAiming = false;
        Vector3 dir = new Vector3(0, 0, 0);
        Vector3 dir2 = new Vector3(0, 0, 0);
        Quaternion newRot = new Quaternion(0, 0, 0, 0);
        Quaternion newRot2 = new Quaternion(0, 0, 0, 0);
        
        while (!isDoneAiming)
        {
            try
            {
                dir = (g.transform.position - turret.transform.position).normalized;
                newRot = Quaternion.LookRotation(dir, turret.transform.up);
                turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, newRot, RotationSpeed * Time.deltaTime);
                turret.transform.localEulerAngles = new Vector3(0f, turret.transform.localEulerAngles.y, 0f);


                dir2 = (g.transform.position - barrel.transform.position).normalized;
                newRot2 = Quaternion.LookRotation(dir2, barrel.transform.right);
                barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, newRot2, RotationSpeed * Time.deltaTime);
                barrel.transform.localEulerAngles = new Vector3(barrel.transform.localEulerAngles.x, 0f, 0f);

                //Where numbers approaching 1 are more accurate/facing the target
                if (Vector3.Dot(barrel.transform.forward, dir2) >= RequiredAccuracy)
                    isDoneAiming = true;
            }
            catch
            {
                //Was destroyed while aiming
                isDoneAiming = true;
            }
            
            yield return null;
        }
    }

    public override IEnumerator Attack(GameObject target)
    {
        yield return Aim(target);

        try
        {
            Vector3 spos = barrel.transform.position + (barrel.transform.forward * SpawnDistance);
            GameObject newShell = Instantiate(shell, spos, barrel.transform.rotation);
            newShell.GetComponent<ATGMController>().SetTarget(target);
        }
        catch
        {
            //Did not find projectile guidance function
        }
    }
}
