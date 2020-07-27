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

     List<Tool> activeTools;
    
    void Start()
    {
        dynamicButtons = new List<GameObject>();
        for (int i = 0; i < maxDynamicButtons; i++)
        {
            GameObject tempButton = Instantiate(buttonPrefab);
            tempButton.transform.SetParent(dynamicTray.transform);
            tempButton.transform.localScale = new Vector3(1, 1, 1);
            dynamicButtons.Add(tempButton);
        }
        
        tooltrayRect = gameObject.GetComponent<RectTransform>();            

        helpButton.onClick.AddListener(startHelp);
        labScriptButton.onClick.AddListener(startLabScript);

    }


    void Update()
    {

    }

    public void SetTrayContents(Mode desiredMode)
    {     

        switch (desiredMode)
        {
            case Mode.Calibrate:                
                extraButtons = 3;                
                dynamicButtons[0].AddComponent<InventoryTool>();
                dynamicButtons[1].AddComponent<MoveTool>();
                dynamicButtons[2].AddComponent<RotateTool>();
                break;

            case Mode.Measure:
                extraButtons = 2;
                dynamicButtons[0].AddComponent<DistanceMeasTool>();
                dynamicButtons[1].AddComponent <AngleMeasTool>();
                dynamicButtons[2].SetActive(false); 
                break;

            case Mode.Explore:
                extraButtons = 3;
                dynamicButtons[0].AddComponent<InventoryTool>();
                dynamicButtons[1].AddComponent<MoveTool>();
                dynamicButtons[2].SetActive(false);
                break;

            case Mode.DataTake:
                extraButtons = 1;
                dynamicButtons[0].AddComponent<TakeDataTool>();
                dynamicButtons[1].SetActive(false);
                dynamicButtons[2].SetActive(false);
                break;
        }
        for (int i = 0; i < extraButtons; i++)
        {
            dynamicButtons[i].SetActive(true);
        }
        tooltrayRect.sizeDelta = new Vector2(tooltrayRect.sizeDelta.x, minheight + (extraButtons * buttonUnitheight));
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
