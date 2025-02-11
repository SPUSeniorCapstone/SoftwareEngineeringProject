using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameUI : MonoBehaviour
{
    UIDocument document;

    public BuildMenu BuildMenu;

    public ProductionMenu ProductionMenu;

    public Label ResourceCounter;

    public Label NodeCounter;

    public Label VersionLabel;

    public VisualElement BottomBar;
    public VisualElement TopBar;
    public AspectRatioPanel MiniMap;

    public Label Framerate;
    float deltaTime = 0;
    int count = 0;
    public Button destroyBuildingButton;

    public bool HideUI = false;
    public bool hideMiniMap = false;

    private void Awake()
    {
        document = GetComponent<UIDocument>();

    }

    private void Update()
    {
        if(count == 10)
        {
            Framerate.text = "Frame Rate: " + Helpers.FloatToString(1 / (deltaTime / 10));
            count = 0;
            deltaTime = 0;
        }
        else
        {
            deltaTime += Time.deltaTime;
            count++;
        }

        if (GameController.Main.inputController.HideUI.Down())
        {
            if (!HideUI)
            {
                document.rootVisualElement.style.display = DisplayStyle.None;
                HideUI = true;
            }
            else
            {
                document.rootVisualElement.style.display = DisplayStyle.Flex;
                HideUI = false;
            }

        }
        if (GameController.Main.InputController.ToggleMiniMap.Down())
        {
            if (!hideMiniMap)
            {
                MiniMap.style.display = DisplayStyle.None;
                hideMiniMap = true;
            }
            else
            {
                MiniMap.style.display = DisplayStyle.Flex;
                hideMiniMap = false;
            }
        }
    }

    private void OnEnable()
    {
        if (document != null)
        {
            BuildMenu = new BuildMenu(document.rootVisualElement.Q("BuildMenu"));
            ProductionMenu = new ProductionMenu(document.rootVisualElement.Q("ProductionMenu"));
            ResourceCounter = document.rootVisualElement.Q("numberResource") as Label;
            NodeCounter = document.rootVisualElement.Q("numberNodes") as Label;
            Framerate = document.rootVisualElement.Q("Framerate") as Label;
            destroyBuildingButton = document.rootVisualElement.Q("DeleteButton") as Button;
            destroyBuildingButton.visible = false;
            BottomBar = document.rootVisualElement.Q("BottomBar");
            TopBar = document.rootVisualElement.Q("Top");
            MiniMap = document.rootVisualElement.Q("MiniMap") as AspectRatioPanel;
            VersionLabel = document.rootVisualElement.Q("VersionLabel") as Label;
            VersionLabel.text = "Kairos-" + Application.version;

            destroyBuildingButton.RegisterCallback<ClickEvent>(DeleteStructure);


            ResourceCounter.text = FormatNum(GameController.Main.resourceCount, true);
            NodeCounter.text = FormatNum(GameController.Main.StructureController.CorruptionNodes.Count, false);
        }
        else
        {
            Debug.LogError("Missing Game UI");
        }
    }

    public string FormatNum(int num, bool greater)
    {
        if (num < 10 && greater)
        {
            return "000" + num.ToString();
        }
        else if (num < 100 && greater)
        {
            return "00" + num.ToString();
        }
        else if ((num < 1000 && greater ) || (num < 10 && !greater))
        {
            return "0" + num.ToString();
        }
        else
        {
            return num.ToString();
        }
    }



    public void UpdateNodes(int count)
    {
        NodeCounter.text = FormatNum(count, false);
    }

    public void OnStruture(bool enable)
    {
        destroyBuildingButton.visible = enable;
    }

    public void DeleteStructure(ClickEvent click)
    {
        GameController.Main.StructureController.SellStructure();
    }
}



public class BuildMenu
{

    public VisualElement mainElement;

    public Button strongholdButton;
    public LabelAutoFit strongholdInfo;

    public Button barracksButton;
    public LabelAutoFit barracksInfo; 

    public Button archerTowerButton;
    public LabelAutoFit archerTowerInfo;

    public Button purifierButton;
    public LabelAutoFit purifierInfo;

    public int STRONGHOLD_COST = 1000;

    public int BARRACKS_COST = 150;

    public int ARCHER_TOWER_COST = 300;

    public int PURIFIER_COST = 500;


  

    public BuildMenu(VisualElement element)
    {
        Init(element);
    }

    public void Init(VisualElement element)
    {
        mainElement = element;

        strongholdButton = element.Q("StrongholdButton") as Button;
        strongholdButton.RegisterCallback<ClickEvent>(StrongholdButton_OnClick);

        barracksButton = element.Q("BarracksButton") as Button;
        barracksButton.RegisterCallback<ClickEvent>(Barracks_Button_OnClick);

        archerTowerButton = element.Q("ArcherTowerButton") as Button;
        archerTowerButton.RegisterCallback<ClickEvent>(ArcherTowerButton_OnClick);

        purifierButton = element.Q("PurifierButton") as Button;
        purifierButton.RegisterCallback<ClickEvent>(ResourceButton_OnClick);
    }

    public void EnableBuildMenu(bool enable)
    {
        mainElement.visible = enable;
        strongholdButton.visible = enable;
        barracksButton.visible = enable;
        archerTowerButton.visible = enable;
        purifierButton.visible = enable;
        if (enable)
        {
            mainElement.style.display = (DisplayStyle.Flex);
        }
        else
        {
            mainElement.style.display = (DisplayStyle.None);
        }
        //mainElement.SetEnabled(enable);
    }

    private void StrongholdButton_OnClick(ClickEvent cl)
    {

        if (GameController.Main.resourceCount >= STRONGHOLD_COST) 
        {
            GameController.Main.StructureController.BuildOrder("stronghold");

        }
    }
    private void Barracks_Button_OnClick(ClickEvent cl)
    {
        if (GameController.Main.resourceCount >= BARRACKS_COST)
        {
            GameController.Main.StructureController.BuildOrder("barracks");
        }
    }
    private void ArcherTowerButton_OnClick(ClickEvent cl)
    {
        if (GameController.Main.resourceCount >= ARCHER_TOWER_COST)
        {
            GameController.Main.StructureController.BuildOrder("archertower");
        }
    }
    private void ResourceButton_OnClick(ClickEvent cl)
    {
        if (GameController.Main.resourceCount >= PURIFIER_COST)
        {
            GameController.Main.StructureController.BuildOrder("purifier");
        }
    }
}

public class ProductionMenu 
{


    public VisualElement mainElement;

    public Button infantryButton;
    public LabelAutoFit infantryInfo;

    public Button archerButton;
    public LabelAutoFit archerInfo;

    public Button collectorButton;
    public LabelAutoFit collectorInfo;

    public Button builderButton;
    public LabelAutoFit builderInfo;

    static int INFANTRY_COST = 10;

    static int ARCHER_COST = 20;

    static int BUILDER_COST = 50;

    static int RC_COST = 25;


    public ProductionMenu(VisualElement element)
    {
        Init(element);
    }

    public void Init(VisualElement element)
    {
        mainElement = element;

        infantryButton = element.Q("InfantryButton") as Button;
        infantryButton.RegisterCallback<MouseUpEvent>(InfantryButton_OnClick);
        infantryInfo = element.Q("InfantryInfo") as LabelAutoFit;
        if (infantryInfo == null)
        {
            Debug.Log("Help me");
        }

        archerButton = element.Q("ArcherButton") as Button;
        archerButton.RegisterCallback<MouseUpEvent>(ArcherButton_OnClick);
        archerInfo = element.Q("ArcherInfo") as LabelAutoFit;

        collectorButton = element.Q("RCButton") as Button;
        collectorButton.RegisterCallback<MouseUpEvent>(RCButton_OnClick);
        collectorInfo = element.Q("CollectorInfo") as LabelAutoFit;

        builderButton = element.Q("BuilderButton") as Button;
        builderButton.RegisterCallback<MouseUpEvent>(BuilderButton_OnClick);
        builderInfo = element.Q("BuilderInfo") as LabelAutoFit;

        // neccessary?
        //archerButton.visible = false;
        //archerInfo.visible = false;
        //infantryButton.visible = false;
        //infantryInfo.visible = false;
        //collectorButton.visible = false;
        //collectorInfo.visible = false;
        //builderButton.visible = false;
        //builderInfo.visible = false;
    }

    public void EnableProductionMenu(bool enable)
    {
        mainElement.visible = enable;
        infantryButton.visible = enable;
        infantryInfo.parent.visible = enable;
        archerButton.visible = enable;
        archerInfo.parent.visible = enable;
        collectorButton.visible = enable;
        collectorInfo.parent.visible = enable;
        builderButton.visible = enable;
        builderInfo.parent.visible = enable;

        if (enable)
        {
            mainElement.style.display = (DisplayStyle.Flex);
        }
        else
        {
            mainElement.style.display = (DisplayStyle.None);
        }
     
    }

    private void InfantryButton_OnClick(MouseUpEvent cl)
    {
        if (cl.button == 0)
        {
            if (Input.GetKey(KeyCode.LeftShift) && GameController.Main.resourceCount >= INFANTRY_COST * 5)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameController.Main.UpdateResource(INFANTRY_COST);
                    GameController.Main.StructureController.TrainInfantry();
                }
            }
            if (GameController.Main.resourceCount >= INFANTRY_COST)
            {
                GameController.Main.UpdateResource(INFANTRY_COST);
                GameController.Main.StructureController.TrainInfantry();
            }
        }
        else if (cl.button == 1)
        {
            GameController.Main.StructureController.UntrainInfantry(INFANTRY_COST);
        }
    }

    private void ArcherButton_OnClick(MouseUpEvent cl)
    {

        if (cl.button == 0)
        {
            if (Input.GetKey(KeyCode.LeftShift) && GameController.Main.resourceCount >= ARCHER_COST * 5)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameController.Main.UpdateResource(ARCHER_COST);
                    GameController.Main.StructureController.TrainArcher();
                }
            }
            if (GameController.Main.resourceCount >= ARCHER_COST)
            {
                GameController.Main.UpdateResource(ARCHER_COST);
                GameController.Main.StructureController.TrainArcher();
            }
        }
        else if (cl.button == 1)
        {
            GameController.Main.StructureController.UntrainArcher(ARCHER_COST);
        }
    }
    private void RCButton_OnClick(MouseUpEvent cl)
    {
        if (cl.button == 0)
        {
            if (Input.GetKey(KeyCode.LeftShift) && GameController.Main.resourceCount >= RC_COST * 5)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameController.Main.UpdateResource(RC_COST);
                    GameController.Main.StructureController.TrainCollector();
                }
            }
            if (GameController.Main.resourceCount >= RC_COST)
            {
                GameController.Main.UpdateResource(RC_COST);
                GameController.Main.StructureController.TrainCollector();
            }
        }
        else if (cl.button == 1)
        {
            GameController.Main.StructureController.UntrainCollector(RC_COST);
        }
    }
    private void BuilderButton_OnClick(MouseUpEvent cl)
    {
       if (cl.button == 0)
        {
            if (Input.GetKey(KeyCode.LeftShift) && GameController.Main.resourceCount >= BUILDER_COST * 5)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameController.Main.UpdateResource(BUILDER_COST);
                    GameController.Main.StructureController.TrainBuilder();
                }
            }
            if (GameController.Main.resourceCount >= BUILDER_COST)
            {
                GameController.Main.UpdateResource(BUILDER_COST);
                GameController.Main.StructureController.TrainBuilder();
            }
        }
        else if (cl.button == 1)
        {
            GameController.Main.StructureController.UntrainBuilder(BUILDER_COST);
        }
    }
}
