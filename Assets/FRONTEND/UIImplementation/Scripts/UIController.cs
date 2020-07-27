using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum Mode
{
    Calibrate,
    Measure,
    Explore,
    DataTake
}

public class UIController : MonoBehaviour
{
    public TooltrayController tooltray;
    public Button calibrate, measure, explore, data;
    public Camera cam;

    public Mode currentMode;

    void Start()
    {
        calibrateClick();        //by default launch into this mode (change this for intro mode in the future)

        calibrate.onClick.AddListener(calibrateClick);
        measure.onClick.AddListener(measureClick);
        explore.onClick.AddListener(exploreClick);
        data.onClick.AddListener(dataClick);
    }

    public void calibrateClick()
    {
        currentMode = Mode.Calibrate;
        tooltray.SetTrayContents(currentMode);
        Debug.Log("calibrate");
    }

    public void measureClick()
    {
        currentMode = Mode.Measure;
        tooltray.SetTrayContents(currentMode);
        Debug.Log("Measure");
    }

    public void exploreClick()
    {
        currentMode = Mode.Explore;
        tooltray.SetTrayContents(currentMode);
        Debug.Log("Explore");
    }

    public void dataClick()
    {
        currentMode = Mode.DataTake;
        tooltray.SetTrayContents(currentMode);
        Debug.Log("Data");
    }


    // Update is called once per frame
    void Update()
    {
        
    }

}