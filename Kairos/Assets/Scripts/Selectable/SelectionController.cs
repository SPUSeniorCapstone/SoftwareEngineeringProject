using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionController : MonoBehaviour
{
    public bool onEnemy;
    // originally enemy, but i think should be generalized
    public GameObject actionTarget;
    private Vector3 startPosition;
    public List<Selectable> masterSelect = new List<Selectable>();
    public List<Selectable> currentlySelect = new List<Selectable>();
    public bool AllowDoubleClicks = false;

    // this only exists to try and fix clicking on something that just got instantiated (trying to prevent it, works only sometimes)
    public bool testCooldown = true;

    [SerializeField] private GameObject selectionAreaTransform;

    Vector3 lowerLeft;
    Vector3 upperRight;
    Vector3 lowerRight;
    Vector3 upperLeft;


    public Dictionary<KeyCode, List<Selectable>> hotKeys = new Dictionary<KeyCode, List<Selectable>>();
    private KeyCode[] keyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
     };

    // Update is called once per frame
    void Update()
    {
        if (!GameController.Main.paused)
        {
            // logic for hot keying units (needs add buildings and prioritizing units over buildings in selection)
            // as of right now, allows for a unit to be hot keyed to multiple keys, allowing for "sub hot keys" 
            // i.e. '2' is your swordsman hot key, but perhaps '3' is for your two handed swords and '4' is for your shield and swords men, whil '2' selects both of them
            if (Input.GetKey(KeyCode.LeftControl))/*&&
            Input.GetKeyDown(KeyCode.Alpha2))*/
            {
                /*
                foreach (Entity entity in selectedEntityList)           //<- is it faster to do a massive OR statment than iterate through the key array?
                        {
                            entity.hoykey = KeyCode.Alpha2;
                        }*/
                foreach (KeyCode keyCode in keyCodes)
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        hotKeys.Remove(keyCode);
                        List<Selectable> list = new List<Selectable>(currentlySelect);
                        // no need to local store hot jey right?
                        //foreach (Entity entity in list)
                        //{
                        //    entity.hotkey = keyCode;
                        //}
                        hotKeys.Add(keyCode, list);
                    }
                }
            }
            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    // to prevent error about list being modifed while doing stuff
                    // make a new hard copy, not a reference
                    List<Selectable> temp = new List<Selectable>(currentlySelect);
                    foreach (Selectable selectable in temp)
                    {
                        selectable.selected = false;
                        selectable.Deactivate();
                        currentlySelect.Remove(selectable);
                    }
                    // don't add hot key to total list (does this cancel shift?) <-yes
                    if (hotKeys.ContainsKey(keyCode))
                    {
                        foreach (Selectable hotSelect in hotKeys[keyCode])
                        {
                            if (hotSelect != null)
                            {
                                if (!hotSelect.selected)
                                {
                                    currentlySelect.Add(hotSelect);
                                    GameController.Main.UIController.StratView.SetUnitView(hotSelect.gameObject);
                                }
                                hotSelect.selected = true;
                                hotSelect.Activate();
                            }
                        }
                    }     
                } 
            }

            // single click select, or click and drag let go
            if (GameController.Main.InputController.Select.KeyUp())
            {
                selectionAreaTransform.gameObject.SetActive(false);

                // can make this better by only looking through a list of player units instead of master select
                foreach (Selectable selectable in masterSelect)
                {
                    var point = Camera.main.WorldToScreenPoint(selectable.transform.position);
                    if (point.x > lowerLeft.x && point.x < upperRight.x && point.y > lowerLeft.y && point.y < upperRight.y && !selectable.faction)
                    {
                        if (selectable.GetComponent<Structure>() == null)
                        {
                            if (selectable.selected == false)
                            {
                                currentlySelect.Add(selectable);

                                // what to add so this only fires if the current unit view isn't part of currentlyselect after new selection occurs?
                                GameController.Main.UIController.StratView.SetUnitView(selectable.gameObject);

                            }
                            selectable.selected = true;
                            selectable.Activate();
                        }
                        // structure not equal null
                        else
                        {
                            if (selectable.selected)
                            {
                                selectable.selected = false;
                                selectable.Deactivate();
                                currentlySelect.Remove(selectable);
                            }
                        }
                    }
                    // this will check if the mouse click ray hit a selectable (uses oneclick instead of selectable)
                    // does this work as intended? what if you flick the mouse?
                    else
                    {
                        // I NEED A CONDITION TO NOT DESELECT IF THE DOUBLE CLICK IS DONE
                        if (selectable.selected == true && selectable.gameObject != GameController.Main.UIController.StratView.inspectee && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                        {
                            if (!selectable.massSelected)
                            {
                                //selectable.selected = false;
                                //selectable.Deactivate();
                                //currentlySelect.Remove(selectable);
                            }
                        }

                        Selectable oneClick = GetMouseWorldPosition3D();
                        // this if detects if cliked on terrain
                        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                        {
                            // unneccesary?
                            //if (selectable.selected == false)
                            //{
                            //    currentlySelect.Remove(selectable);
                            //}
                            // terrain
                            if (oneClick == null)
                            {
                                selectable.selected = false;
                                selectable.Deactivate();
                                currentlySelect.Remove(selectable);
                            }
                            ////different mass select
                            //else if (selectable.massSelected)
                            //{
                            //    selectable.selected = false;
                            //    selectable.Deactivate();
                            //    currentlySelect.Remove(selectable);
                            //}
                            // different selectable

                            //(oneClick != selectable && !oneClick.massSelected)
                            // !selectable.massSelected || selectable.massSelected && currentlySelect.Contains(selectable)
                            // ||    (Input.GetKey(KeyCode.LeftShift) && GameController.Main.UIController.StratView.inspectee.GetComponent<ProductionStructure>() != null && oneClick.GetComponent<Unit>() == null)
                            //else if ((Input.GetKey(KeyCode.LeftShift) && GameController.Main.UIController.StratView.inspectee.GetComponent<Unit>() != null && oneClick.GetComponent<ProductionStructure>() != null))
                            //{
                            //    selectable.selected = false;
                            //    selectable.Deactivate();
                            //    currentlySelect.Remove(selectable);
                            //}
                            //||
                            //    (GameController.Main.UIController.StratView.inspectee.GetComponent<ProductionStructure>() == null && oneClick.GetComponent<Unit>() != null

                            else if (!oneClick.massSelected && oneClick != selectable && !Input.GetKey(KeyCode.LeftShift) ||
                                (Input.GetKey(KeyCode.LeftShift) && oneClick != selectable &&
                                (GameController.Main.UIController.StratView.inspectee.GetComponent<ProductionStructure>() == null && oneClick.GetComponent<Unit>() != null 
                                || GameController.Main.UIController.StratView.inspectee.GetComponent<Unit>() == null && oneClick.GetComponent<ProductionStructure>() != null)))
                                 
                            {
                                selectable.selected = false;
                                selectable.Deactivate();
                                currentlySelect.Remove(selectable);
                            } 
                        }
                    }
                }
                // ping ui to check
                if (currentlySelect.Count > 0) { }
                    //GameController.Main.UIController.StratView.SetUnitView(currentlySelect[0].gameObject);
                else GameController.Main.UIController.StratView.SetUnitView(null);
            }

            // click and drag box
            if (GameController.Main.InputController.Select.Pressed())
            {
                if (GameController.Main.InputController.Select.Down())
                {
                    startPosition = Input.mousePosition;
                    selectionAreaTransform.gameObject.SetActive(true);
                }
                Vector3 currentMousePosition = Input.mousePosition;
                float z = currentMousePosition.z;
                lowerLeft = new Vector3(
                        Mathf.Min(startPosition.x, currentMousePosition.x),
                        Mathf.Min(startPosition.y, currentMousePosition.y),
                        z
                );
                upperRight = new Vector3(
                    Mathf.Max(startPosition.x, currentMousePosition.x),
                     Mathf.Max(startPosition.y, currentMousePosition.y),
                     z
                   );
                // shouldn't matter: just the lower left and upper ight corner shouls matter right?
                lowerRight = new Vector3(
                   upperRight.x,
                   lowerLeft.y,
                   z
                  );
                upperLeft = new Vector3(
                   lowerLeft.x,
                   upperRight.y,
                   z
                  );

                //================================================================================================================

                var line = selectionAreaTransform.GetComponent<LineRenderer>();
                line.positionCount = 5;
                List<Vector3> vector3s = new List<Vector3>();
                vector3s.Add(MouseToWorld(upperLeft));
                vector3s.Add(MouseToWorld(upperRight));
                vector3s.Add(MouseToWorld(lowerRight));
                vector3s.Add(MouseToWorld(lowerLeft));
                vector3s.Add(MouseToWorld(upperLeft));
                line.SetPositions(vector3s.ToArray());
            }
        }
    }


    public void DeselectAll()
    {
        List<Selectable> temp = new List<Selectable>(currentlySelect);
        foreach (Selectable selectable in temp)
        {
            selectable.selected = false;
            selectable.Deactivate();
            currentlySelect.Remove(selectable);
        }
    }

    // should this be in a helper class?
    // returns a screen to world point with a constant vector.z
    private Vector3 MouseToWorld(Vector3 vector)
    {
        vector.z = 2.5f;
        return Camera.main.ScreenToWorldPoint(vector);
    }
    private Selectable GetMouseWorldPosition3D()
    {
        Ray ray;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        if (Physics.Raycast(ray, out hitData, 1000) && (hitData.transform.GetComponentInParent<Selectable>() || hitData.transform.GetComponent<Selectable>())) // <- will this be problematic?
        {
            Debug.Log(hitData.transform.name);
            return hitData.transform.GetComponentInParent<Selectable>();
        }
        else if (hitData.transform != null) { //Debug.Log(hitData.transform.name);
                                              }
        return null;
    }
}
