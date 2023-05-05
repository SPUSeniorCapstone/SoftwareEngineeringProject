using System.Collections.Generic;
using UnityEngine;

public class ProductionStructure : Structure
{

    public int unitsQueued;
    protected float timeLeft = 5;
    protected float originialTime = 5;
    public GameObject unitToSpawn;
    public List<GameObject> unitTypes;
    public GameObject spawnPoint;
    public Queue<GameObject> buildQue;



    private void Update()
    {
        if (unitsQueued > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                timeLeft = originialTime;
                SpawnUnits(buildQue.Dequeue());
                unitsQueued--;
            }
        }
    }
    new public void Start()
    {
        base.Start();
        buildQue = new Queue<GameObject>();
        if (Preview != null)
        {
            Destroy(Preview);
        }
    }

    //Function for when stronghold is clicked activate structureMenuUI
    public void OnMouseDown()
    {
        //if (GameController.Main.SelectionController.currentlySelect[0].gameObject == this)
        //{
        //    GameController.Main.StructureController.selected = this;
        //    GameController.Main.UIController.MenuController.structureMenuUI.SetActive(true);
        //}
    }

    //Add a unit to queue if train units button is clicked
    public void QueueUnits(GameObject unit)
    {
        if (buildQue == null)
        {
            Debug.Log("WHY NULL?!");
        }
        buildQue.Enqueue(unit);
        unitsQueued++;
    }

    public void DequeueUnits()
    {
        if (buildQue.Count != 0)
        {
            buildQue.Dequeue();
            unitsQueued--;
        }
    }

    //Spawn unit function
    public virtual void SpawnUnits(GameObject unit)
    {
        GameObject tree = Instantiate(unit, spawnPoint.transform.position, Quaternion.identity, GameController.Main.StructureController.PlayerUnits.transform);
        // does this work?
        if (rallyPoint.activeSelf)
        {
            if (tree.GetComponent<Unit>() != null)
            {
                tree.GetComponent<Unit>().MoveTo(rallyPoint.transform.position);
            }
        }
    }
    public override void OnSelect()
    {
        GameController.Main.StructureController.selected = this;
        GameController.Main.UIController.EnableProductionMenu(true);
        rallyPoint.GetComponentInChildren<MeshRenderer>().material.shader = GameController.Main.highlight;
        //GameController.Main.UIController.MenuController.structureMenuUI.SetActive(true);
    }
    public override void OnDeselect()
    {
        GameController.Main.StructureController.selected = null;
        GameController.Main.UIController.EnableProductionMenu(false);
        rallyPoint.GetComponentInChildren<MeshRenderer>().material.shader = GameController.Main.unHighlight;
        //GameController.Main.UIController.MenuController.structureMenuUI.SetActive(false);
    }
}
