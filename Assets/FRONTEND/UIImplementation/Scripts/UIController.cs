using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

enum Mode
{
    Calibrate,
    Measure,
    Explore,
    DataTake
}

public class UIController : MonoBehaviour
{
    public Button calibrate, measure, explore, data;
    public Camera cam;

    Mode currentMode;

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
        cam.transform.position = new Vector3(0, 10, 0);
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);
        Debug.Log("calibrate");
    }

    public void measureClick()
    {
        currentMode = Mode.Measure;
        cam.transform.position = new Vector3(0, 0, -15);
        cam.transform.rotation = Quaternion.identity;
        Debug.Log("Measure");
    }

    public void exploreClick()
    {
        currentMode = Mode.Explore;
        cam.transform.position = new Vector3(0, 0, -10);
        Debug.Log("Explore");
    }

    public void dataClick()
    {
        currentMode = Mode.DataTake;
        cam.transform.position = new Vector3(0, 0, -5);
        Debug.Log("Data");
    }


    // Update is called once per frame
    void Update()
    {
        
    }

}