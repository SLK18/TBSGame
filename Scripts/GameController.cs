using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnPhase
{
    playerTurn,
    enemyTurn,
    victory,
    defeat,
    tie
}

public enum Team
{
    blue,
    red
}

public enum Identifier
{
    player,
    enemy
}

public class GameController : MonoBehaviour
{
    public LineRenderer unitSelectionToRaycastLine, validMoveLine, validAttackLine;
    public TextMesh distanceTextBox;
    public TurnPhase currentPhase;
    public GameObject LineOfSightIndicator;

    private Vector3 unitSelectionToRaycastLineFirstPos, unitSelectionToRayCastLineSecondPos;

    //Adds to the Y value of the waypoint/cursor
    private Vector3 WaypointHeightOffset = new Vector3(0, 0f, 0), CursorHeightOffset = new Vector3(0, 7f, 0),
        DistanceLineHeightOffset = new Vector3(0, 7f, 0), DistanceTextBoxHeightOffset = new Vector3(0, 1f, 0);
    private UnitController unitSelected = null;
    private Team opposingTeam = Team.red;
    private bool playerCommandInputEnabled;

    private List<GameObject> allyList, enemyList;
    const int UnitLayerMask = 3;
    private IEnumerator commandCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        allyList = new List<GameObject>();
        enemyList = new List<GameObject>();


        GameObject[] all = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject g in all)
        {
            try
            {
                if (g.GetComponent<UnitController>().team == Team.blue)
                    allyList.Add(g);
                else if (g.GetComponent<UnitController>().team == Team.red)
                    enemyList.Add(g);
            }
            catch
            {
                //GameObject is not a unit
            }
        }

        currentPhase = TurnPhase.playerTurn;
        playerCommandInputEnabled = true;
    }

    void Update()
    {
        try
        {
            //Make cursor follow selected
            transform.position = unitSelected.transform.position + CursorHeightOffset;
            gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
        catch
        {
            //Unit is null
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        if (playerCommandInputEnabled)
                ScanForCommandInput();

        if (unitSelected != null && playerCommandInputEnabled)
            DrawUnitIndicators();
        else
        {
            unitSelectionToRaycastLine.gameObject.SetActive(false);
            LineOfSightIndicator.SetActive(false);
        }
            
        

        if (CheckForVictory() && CheckForDefeat())
            currentPhase = TurnPhase.tie;
        else if (CheckForVictory())
            currentPhase = TurnPhase.victory;
        else if (CheckForDefeat())
            currentPhase = TurnPhase.defeat;
    }

    void ScanForCommandInput()
    {
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000))
            {
                try
                {
                    Debug.Log(hit.collider.transform.root.gameObject.name);
                    if(hit.collider.GetComponent<UnitController>().team == Team.blue)
                        unitSelected = hit.collider.GetComponent<UnitController>();
                }
                catch
                {
                    Debug.Log("Clicked non-unit");
                    unitSelected = null;
                }
            }
        }
        //Right click to give waypoint to send for navigation for the unit
        if (Input.GetMouseButtonDown(1) && unitSelected != null)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                try
                {
                    GameObject tempTarget = hit.collider.gameObject;
                    float distance = Vector3.Distance(tempTarget.transform.position, unitSelected.transform.position);
                    if (distance < unitSelected.attackRange)
                        SetAttackTargetCommand(tempTarget);
                    else
                        SetMoveToCommand(hit.point);
                    playerCommandInputEnabled = false;

                }
                catch
                {
                    SetMoveToCommand(hit.point);
                    playerCommandInputEnabled = false;
                }

                StartCoroutine(DispatchCommand(Identifier.player));

            }
        }
    }

    void SetAttackTargetCommand(GameObject target)
    {
        if (target.GetComponent<UnitController>().team == opposingTeam)
        {

            if (commandCoroutine != null)
                StopCoroutine(commandCoroutine);

            commandCoroutine = unitSelected.Attack(target);
        }
    }

    void SetMoveToCommand(Vector3 targetPosition)
    {
        //Cap movement to unit max move range
        float distance = Vector3.Distance(unitSelected.transform.position, targetPosition);
        float fractionOfDistance = 1f - (unitSelected.moveRange / distance);
        targetPosition = Vector3.Lerp(targetPosition, unitSelected.transform.position, fractionOfDistance);

        targetPosition += WaypointHeightOffset;

        if (commandCoroutine != null)
            StopCoroutine(commandCoroutine);

        commandCoroutine = unitSelected.Navigate(targetPosition);
    }


    private IEnumerator DispatchCommand(Identifier whoIsDispatching)
    {
        Debug.Log(whoIsDispatching + " has called the command " + commandCoroutine);
        yield return commandCoroutine;
        yield return new WaitForSeconds(2f);

        if (whoIsDispatching == Identifier.player)
        {
            playerCommandInputEnabled = false;
            unitSelected = null;
            currentPhase = TurnPhase.enemyTurn;
            EnemyTurn();
        }
        else if(whoIsDispatching == Identifier.enemy)
        {
            playerCommandInputEnabled = true;
            unitSelected = null;
            currentPhase = TurnPhase.playerTurn;
        }
        allyList.RemoveAll(unit => unit == null);
        enemyList.RemoveAll(unit => unit == null);
    }

    private void EnemyTurn()
    {
        opposingTeam = Team.blue;

        allyList.RemoveAll(unit => unit == null);
        enemyList.RemoveAll(unit => unit == null);

        if (CheckForVictory() || CheckForDefeat())
        {
            opposingTeam = Team.red;
            return;
        }

        //Get ally center of mass to push units towards
        Vector3 allyCenterOfMass = new Vector3(0,0,0);
        foreach(GameObject g in allyList)
        {
            allyCenterOfMass += g.transform.position;
        }
        allyCenterOfMass /= allyList.Count;

        //Pick a random unit from enemylist to move/attack
        int randomIndex = Random.Range(0, enemyList.Count);
        Debug.Log(randomIndex + " " + enemyList.Count);

        GameObject unitSelectedGameObject = enemyList[randomIndex];
        unitSelected = unitSelectedGameObject.GetComponent<UnitController>();

        //Randomly move or attack
        int decision = Random.Range(0, 2);
        if (decision == 0)
        {
            SetMoveToCommand(allyCenterOfMass);
        }
        else
        {
            List<GameObject> unitsWithinRange = new List<GameObject>();
            List<GameObject> attackableUnits = new List<GameObject>();

            //Find a allyUnit within its attack range
            foreach(GameObject unit in allyList)
            {
                if(Vector3.Distance(unitSelectedGameObject.transform.position, unit.transform.position) <= unitSelected.attackRange)
                    unitsWithinRange.Add(unit);
            }

            if(unitsWithinRange.Count > 0)
            {
                //Among those filter out those without line of sight
                foreach (GameObject unit in unitsWithinRange)
                {
                    if(unitSelected.HasLineOfSight(unit.transform.position))
                    {
                        attackableUnits.Add(unit);
                    }
                }

                if (attackableUnits.Count == 0)
                {
                    SetMoveToCommand(allyCenterOfMass);
                }
                else
                {
                    //Randomly pick a unit to attack
                    randomIndex = Random.Range(0, attackableUnits.Count);
                    GameObject randomUnit = attackableUnits[randomIndex];
                    SetAttackTargetCommand(randomUnit);
                }
                
            }
            else
            {
                SetMoveToCommand(allyCenterOfMass);
            }

        }

        StartCoroutine(DispatchCommand(Identifier.enemy));

        opposingTeam = Team.red;
    }

    private bool CheckForVictory()
    {
        if (enemyList.Count == 0)
            return true;
        return false;
    }

    private bool CheckForDefeat()
    {
        if (allyList.Count == 0)
            return true;
        return false;
    }

    void DrawUnitIndicators()
    {
        //This is used to cancel out jitter
        Vector3 BumpHeightOffset = new Vector3(0, .01f, 0);

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000000))
        {
            unitSelectionToRaycastLine.gameObject.SetActive(true);
            unitSelectionToRaycastLineFirstPos = unitSelected.transform.position + DistanceLineHeightOffset;
            unitSelectionToRayCastLineSecondPos = hit.point + DistanceLineHeightOffset;

            distanceTextBox.transform.position = unitSelectionToRayCastLineSecondPos + DistanceTextBoxHeightOffset;
            distanceTextBox.text = Vector3.Distance(unitSelected.transform.position, hit.point).ToString();
            distanceTextBox.transform.rotation = Quaternion.LookRotation(distanceTextBox.transform.position - Camera.main.transform.position);

            unitSelectionToRaycastLine.SetPosition(0, unitSelectionToRaycastLineFirstPos);
            unitSelectionToRaycastLine.SetPosition(1, unitSelectionToRayCastLineSecondPos);

            DrawLineOnLineOnLimit(unitSelectionToRaycastLine, validMoveLine, unitSelected.moveRange);
            DrawLineOnLineOnLimit(unitSelectionToRaycastLine, validAttackLine, unitSelected.attackRange);

            if (unitSelected.HasLineOfSight(hit.point + BumpHeightOffset))
            {
                LineOfSightIndicator.SetActive(true);
                LineOfSightIndicator.transform.position = hit.point;
            }
            else
            {
                LineOfSightIndicator.SetActive(false);
            }
            
        }
    }

    //Where line2 is to be drawn based on line1 given a limit. Expects two point lines
    void DrawLineOnLineOnLimit(LineRenderer line1, LineRenderer line2, float limit)
    {
        Vector3 line1FirstPos = line1.GetPosition(0);
        Vector3 line1SecondPos = line1.GetPosition(1);
        float distance = Vector3.Distance(line1FirstPos, line1SecondPos);

        Vector3 validLineSecondPos = line1SecondPos;

        line2.SetPosition(0, line1FirstPos);

        if (distance > limit)
        {
            if (distance != 0)
            {
                float fractionOfDistance = 1f - (limit / distance);
                validLineSecondPos = Vector3.Lerp(line1SecondPos, line1FirstPos, fractionOfDistance);
            }
        }

        line2.SetPosition(1, validLineSecondPos);
    }
}
