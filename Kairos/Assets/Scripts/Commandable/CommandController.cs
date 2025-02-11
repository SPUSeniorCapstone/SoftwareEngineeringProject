using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CommandController : MonoBehaviour
{
    public GameObject wayPoint;
    public float debugY;
    public float fadeTime;
    public float fadeTimer = 5f;

    public int stepHeight = 1;

    public float groupJoinDistance = 4f;

    /// <summary>
    /// Command Group Master List
    /// </summary>
    public List<CommandGroup> CommandGroups
    {
        get { return commandGroups; }
    }
    public List<CommandGroup> commandGroups = new List<CommandGroup>();

    public bool attackCommand = false;

    [SerializeField] CommandGroup commandGroup;
    [SerializeField] GameObject playerFaction;

    void Update()
    {
        if (!GameController.Main.paused)
        {
            if (!GameController.Main.StructureController.cancel && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {

                if (wayPoint.activeSelf)
                {
                    if (Time.time - fadeTime > fadeTimer)
                    {
                        wayPoint.SetActive(false);
                    }

                }

                if (GameController.Main.InputController.Command.Down())
                {
                    Debug.Log("Mouse1 down");
                    if (GameController.Main.SelectionController.onEnemy)
                    {
                        //-----------------------------
                        // less ugly way to get this, change later
                        attackCommand = true;
                        if (GameController.Main.SelectionController.currentlySelect.Count > 0)
                        {
                            AttackWithSelected(GameController.Main.SelectionController.actionTarget);
                        }
                    }
                    else if (GameController.Main.SelectionController.actionTarget != null)
                    {
                        foreach (var s in GameController.Main.SelectionController.currentlySelect)
                        {
                            if (s.GetComponent<Unit>() != null)
                            {
                                s.GetComponent<Unit>().PerformTaskOn(GameController.Main.SelectionController.actionTarget.GetComponent<Selectable>());
                            }
                        }
                    }
                    else
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000, LayerMask.GetMask("Terrain")))
                        {
                            wayPoint.SetActive(true);
                            fadeTime = Time.time;
                            Vector3 point = hit.point;
                            point.y = GameController.Main.WorldController.World.GetHeight(point.x, point.z) + 0.05f;
                            wayPoint.transform.position = point;
                            attackCommand = false;
                            if (GameController.Main.SelectionController.currentlySelect.Count > 0)
                            {
                                MoveSelected(point);
                            }
                        }
                    }
                }
            }
        }
    }
    public void MoveSelected(Vector3 target)
    {
        if (GameController.Main.SelectionController.currentlySelect.Count == 1 && GameController.Main.SelectionController.currentlySelect[0].GetComponent<ProductionStructure>() == null)
        {
            var t = GameController.Main.SelectionController.currentlySelect[0];
            var u = t.GetComponent<Unit>();
            if (u != null)
            {
                CommandGroup old = u.commandGroup;
                if (old != null)
                {
                    old.unitList.Remove(u);
                }
                u.ClearTarget();
                u.MoveTo(target);
            }
            return;
        }

        var CG = Instantiate<CommandGroup>(commandGroup, playerFaction.transform);

        List<CommandGroup> tempList = new List<CommandGroup>();

        CG.followSpeed = -1;
        foreach (Selectable selectable in GameController.Main.SelectionController.currentlySelect)
        {
            Unit unit = selectable.GetComponent<Unit>();
            ProductionStructure production = selectable.GetComponent<ProductionStructure>();
            if (unit != null)
            {
                //CommandGroup old = unit.commandGroup;
                if (unit.commandGroup != null)
                {
                    // here, a unit is set to null, but doesnt set to idle
                    //foreach(Unit lunit in old.unitList)
                    //{
                    //    lunit.commandGroup = null;
                    //}
                    unit.commandGroup.unitList.Remove(unit);
                    unit.commandGroup = null;
                    //old.unitList.Clear();
                }
                if (unit.commandGroup != null)
                {
                    Debug.Log("WHAT THE HELL HAPPENED???");
                }
                // makes more command groups
                if (Vector3.Distance(unit.transform.position, CG.centerVector) > groupJoinDistance && CG.unitList.Count != 0)
                {
                    if (tempList.Count == 0)
                    {
                        Debug.Log("Making first new group");
                        var subCG = Instantiate<CommandGroup>(commandGroup, playerFaction.transform);
                        subCG.ParentCommandGroup = CG;
                        unit.commandGroup = subCG;
                        subCG.AddUnit(unit);
                        tempList.Add(subCG);
                    }
                    else
                    {
                        foreach(CommandGroup group in tempList)
                        {
                            if (Vector3.Distance(unit.transform.position, group.centerVector) <= groupJoinDistance)
                            {
                                unit.commandGroup = group;
                                group.AddUnit(unit);
                                Debug.Log("Joining already made group");
                            }
                        }
                        if (unit.commandGroup == null)
                        {
                            Debug.Log("MAKING NEW GROUP");
                            var subCG = Instantiate<CommandGroup>(commandGroup, playerFaction.transform);
                            subCG.ParentCommandGroup = CG;
                            unit.commandGroup = subCG;
                            subCG.AddUnit(unit);
                            tempList.Add(subCG);
                        }
                    }
                    
                }
                else
                {
                    Debug.Log("joining cg");
                    unit.commandGroup = CG;
                    CG.AddUnit(unit);
                }  
                if (unit.commandGroup == null)
                {
                    Debug.Log("AHAHAHAHAHAH");
                }
            }

            if (production != null && unit == null)
            {
                production.rallyPoint.GetComponentInChildren<MeshRenderer>().enabled = true;
                production.rallyPoint.transform.position = wayPoint.transform.position;
            }
        }
        CG.CalculateCenter();
        //Debug.Log("AFTER LOOP: cg.entites = " + cg.entities[0].name);
        CG.pathTask = GameController.Main.PathFinder.FindPath(CG.transform.position, target, stepHeight, false);
        CG.retrievingPath = true;
        commandGroups.Add(CG);
        foreach (CommandGroup group in tempList)
        {
            group.CalculateCenter();
            group.pathTask = GameController.Main.PathFinder.FindPath(group.transform.position, target, stepHeight, false);
            group.retrievingPath = true;
            commandGroups.Add(group);
        }
   


        //DEBUG
        if (GetComponent<CheckPathFinding>() != null)
            GetComponent<CheckPathFinding>().task = CG.pathTask;

        Debug.Log(tempList + " list and list count " + tempList.Count);


        //cg.SetGroupTarget(target);

        // this doesn't work, changes length while in the loop
        //int count = commandGroups.Count;
        //for (int i = 0; i < count; i++)
        //{
        //    //Debug.Log(i.ToString() + " " +commandGroups.ElementAt(i));
        //    commandGroups.ElementAt(i).CheckIfEmpty();

        //}
    }
    public void AttackWithSelected(GameObject target)
    {
        if (GameController.Main.SelectionController.currentlySelect.Count == 1 && GameController.Main.SelectionController.currentlySelect[0].GetComponent<ProductionStructure>() == null)
        {
            var t = GameController.Main.SelectionController.currentlySelect[0];
            var u = t.GetComponent<Unit>();
            if (u != null)
            {
                u.ClearTarget();
                u.PerformTaskOn(target.GetComponent<Selectable>());
                u.AttackCommand();
            }
            return;
        }

        var cg = Instantiate<CommandGroup>(commandGroup, playerFaction.transform);

        cg.followSpeed = -1;
        foreach (Selectable selectable in GameController.Main.SelectionController.currentlySelect)
        {
            Unit unit = selectable.GetComponent<Unit>();
            ProductionStructure production = selectable.GetComponent<ProductionStructure>();
            //if (entity == null)
            //{
            //    entity = selectable.GetComponentInParent<Entity>();
            //}
            if (unit != null)
            {
                //Debug.Log("entity does not = null (MOVESELECTED)");
                //entity.pathindex = 0;
                CommandGroup old = unit.commandGroup;
                if (old != null)
                {
                    old.unitList.Remove(unit);
                }
                unit.commandGroup = cg;
                cg.unitList.Add(unit);
                unit.ClearTarget();
                //if (entity.movementSpeed < cg.followSpeed || cg.followSpeed == -1)
                //{
                //    cg.followSpeed = entity.movementSpeed;
                //}
                //entity.GetComponent<Unit>().isAttacking = false;
                ////entity.idle = false;
            }

            if (production != null)
            {
                Debug.Log("Structure not null");
                production.rallyPoint.GetComponentInChildren<MeshRenderer>().enabled = true;
                production.rallyPoint.transform.position = wayPoint.transform.position;
            }
        }

        cg.CalculateCenter();
        //Debug.Log("AFTER LOOP: cg.entites = " + cg.entities[0].name);
        cg.pathTask = GameController.Main.PathFinder.FindPath(cg.transform.position, target.transform.position, stepHeight, false);

        //DEBUG
        if (GetComponent<CheckPathFinding>() != null)
            GetComponent<CheckPathFinding>().task = cg.pathTask;


        commandGroups.Add(cg);
        foreach (Unit unit in cg.unitList)
        {
            unit.PerformTaskOn(target.GetComponent<Selectable>());
            unit.AttackCommand();
        }
    }
}
