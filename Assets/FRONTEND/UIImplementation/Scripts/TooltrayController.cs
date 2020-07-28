using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltrayController : MonoBehaviour
{
    public Button helpButton, labScriptButton;
    public List<GameObject> dynamicButtons;

    public GameObject buttonPrefab;
    public GameObject dynamicTray;
    RectTransform tooltrayRect;

    float minheight = 215f;
    float buttonUnitheight = 76f;

    int extraButtons;
    int maxDynamicButtons = 3;

    public List<Tool> activeTools;
    
    void Start()
    {
        //Creates the blank buttons before we apply tools to them, held in dynamicTools


        activeTools = new List<Tool>();
        dynamicButtons = new List<GameObject>();
        for (int i = 0; i < maxDynamicButtons; i++)
        {
            GameObject tempButton = Instantiate(buttonPrefab);
            tempButton.transform.SetParent(dynamicTray.transform); //Places buttons in parents vertical layout
            tempButton.transform.localScale = new Vector3(1, 1, 1);
            dynamicButtons.Add(tempButton);
        }
        
        tooltrayRect = gameObject.GetComponent<RectTransform>();            

        helpButton.onClick.AddListener(startHelp);       
        labScriptButton.onClick.AddListener(startLabScript);
    }

    public void SetTrayContents(Mode desiredMode)
    {
        if (activeTools != null)
        {
            foreach (var tool in activeTools)
            {
                tool.enabled = false;
            }
            activeTools.Clear();
        }        

        switch (desiredMode)
        {     
            //Depending on the current mode different tools are created/enabled

            case Mode.Calibrate:                
                extraButtons = 3;

                CreateTool<InventoryTool>(dynamicButtons[0]);
                CreateTool<MoveTool>(dynamicButtons[1]);
                CreateTool<RotateTool>(dynamicButtons[2]);                
                break;

            case Mode.Measure:
                extraButtons = 2;

                CreateTool<DistanceMeasTool>(dynamicButtons[0]);
                CreateTool<AngleMeasTool>(dynamicButtons[1]);
                dynamicButtons[2].SetActive(false); 
                break;

            case Mode.Explore:
                extraButtons = 3;

                CreateTool<InventoryTool>(dynamicButtons[0]);
                CreateTool<MoveTool>(dynamicButtons[1]);
                CreateTool<InvestigateTool>(dynamicButtons[2]);
                break;

            case Mode.DataTake:
                extraButtons = 1;

                CreateTool<TakeDataTool>(dynamicButtons[0]);
                dynamicButtons[1].SetActive(false);
                dynamicButtons[2].SetActive(false);
                break;
        }
        tooltrayRect.sizeDelta = new Vector2(tooltrayRect.sizeDelta.x, minheight + (extraButtons * buttonUnitheight));
    }

    void CreateTool<T>(GameObject rootObject) where T : Tool
    {
        //Generic method to create Tools from their classes that inherit from Tool

        T tool = rootObject.GetComponent<T>();
        if (tool == null) //check to see if we already have a instance of this Tool attached
        {
            rootObject.AddComponent<T>();
            tool = rootObject.GetComponent<T>();
            rootObject.GetComponent<Button>().onClick.AddListener(tool.ButtonInteract);
        }
        tool.enabled = true;
        rootObject.SetActive(true);
        activeTools.Add(tool);
    }

    void startHelp()
    {
        Debug.Log("Help button pressed");
    }

    void startLabScript()
    {
        Debug.Log("Lab script button pressed");
    }

}
