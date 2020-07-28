using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


// LOCKED VERSION OF LUKE'S SCRIPT WHICH CONSTRAINS THE LINE TO BE HORIZONTAL OR VERTICAL

/* Script controls the processs of constructing lines, as well as storing and displaying these.
 * Left click on mesh colliders to place the start and end points. 
 * Right clicking anywhere resets this process at any point.
 * One two points are placed, left clicking on any collider for a third time acts to save the line, and writes the distance to the console
 * 
 * IMPLEMENTATION: Attach to some GameObject to use as a controller, needs hooking up to a camera and a marker prefab
 * in the inspector. No need to attach anything to the line renderer component.
 * 
 * The marker prefab is just a primative 3d shape WITHOUT a mesh collider - it marks the start and end of a line being constructed
 * 
 * Then create a UI toggle, and for the On Value Changed event of the toggle, add the OnChange function of this script as a listener
 * 
 * This provides the basic functionality, excluding control of layers from the UI - do this from the inspector
*/


public class DetectorMeasurementControl_LOCKED : MonoBehaviour

{
    public Camera cam;
    public GameObject markerPrefab;
    public DetectorDisplayScript display;
    public Transform screenOrigin;

    GameObject markerOne; //Simple objects to mark the start and end of lines under construction
    GameObject markerTwo;
    bool modeActive; //Whether UI checkbox is ticked
    int clicks; //Used for line creation control flow

    [SerializeField] [Range(1,8)] int digits; //number of digits stored for position data - affects snapping intensity
    [SerializeField] [Range(1, 10)] int currentLayer;

    Vector3 startPoint; //Local variables of line under construction, before they are written to lineData 
    Vector3 endPoint;    
    string startTag;
    string endTag;

    private LineRenderer line;
    List<MeasurementLine> lineData = new List<MeasurementLine>();                   //Data structure holding line data
    public List<GameObject> lineObjects = new List<GameObject>();                  //Current list of lines being shown in the current layer
    Dictionary<string, Vector3> positionCache = new Dictionary<string, Vector3>(); //GameObjects are found by name for their global position and rotations
    Dictionary<string, Quaternion> rotationCache = new Dictionary<string, Quaternion>(); //These caches store them temporarily if they have been previously requested.

    // detector Parameters for the data-taking
    float screenHeight;
    float screenWidth;
    float screenResolution;
    float[,] matrix;
    bool horizontal; //true = horizontal measurement, false = vertical measurement
    [SerializeField] string file = "Assets/FRONTEND/DetectorScreen/img.txt";

    private void Start()
    {
        clicks = 0;
        modeActive = false;
        line = GetComponent<LineRenderer>();
    }

    public void OnChange(bool ticked)
        // Activated upon any change to checkbox
    {
        modeActive = ticked;

        if (ticked)
        {
            CreateTool();
            SetLineProperties(line);
        }
        else
        {            
            EndTool();
        }        
    }

    private void Update()
    {
        if (modeActive)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition); //Get user input based on click from camera in game view
            RaycastHit hit;                                    //Currently calling this every frame for debugging purposes
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.yellow);

            if (Physics.Raycast(ray, out hit))
            {
                switch (clicks)
                {
                    case 0:
                        // first marker
                        startPoint = RoundVector(hit.point, digits); //round position of position marker to arbritrary precision
                        markerOne.transform.position = startPoint;
                        line.SetPosition(0, startPoint);
                        startTag = hit.collider.name;

                        markerOne.SetActive(true);

                        if (Input.GetMouseButtonDown(0)) //if left click whilst over mesh collided
                        {
                            clicks += 1;
                            //Debug.Log("start point: " + startPoint);
                        }
                        break;

                    case 1:
                        // second marker
                        // for the detector - the second point must be horizontal/vertical relative to the first point
                        endPoint = RoundVector(hit.point, digits);

                        // direction from first point to current mouse position
                        Vector3 dir = endPoint - startPoint;
                        // normalised projections along the x/y axes
                        float x_proj = Vector3.Dot(dir, new Vector3(1, 0));
                        float y_proj = Vector3.Dot(dir, new Vector3(0, 1));

                        //HORIZONTAL OR VERTICAL LOCKING
                        if (Mathf.Abs(x_proj) > Mathf.Abs(y_proj))
                        {
                            //greater projection along the horizontal axis - so lock horizontally
                            endPoint.x = x_proj + startPoint.x;
                            endPoint.y = startPoint.y;
                            horizontal = true;
                        }
                        else
                        {
                            //greater projection along the vertical axis - so lock vertically
                            endPoint.x = startPoint.x;
                            endPoint.y = y_proj + startPoint.y;
                            horizontal = false;
                        }


                        markerTwo.transform.position = endPoint;
                        line.SetPosition(1, endPoint);
                        endTag = hit.collider.name;

                        markerOne.SetActive(true);
                        markerTwo.SetActive(true);
                        line.enabled = true;

                        if (Input.GetMouseButtonDown(0))
                        {                            
                            clicks += 1;
                            //Debug.Log("end point: " + endPoint);
                        }
                        break;

                    case 2:
                        // set line and output length
                        if (Input.GetMouseButtonDown(0))
                        {
                            // first fetch the appropriate row/column of data and write to text file
                            GenerateData();

                            // then deal with the line
                            StoreLine();
                            DisableMarkers();
                            clicks = 0;
                            DrawLine(lineData[lineData.Count - 1]); //Local variables are cleared so reload line from storage without markers
                            
                            //Debug.Log(GetDistance(lineData[lineData.Count - 1])); //Distance output - feel free to hook up to UI
                        }
                        break;

                    default:
                        //catchall for any edge cases
                        clicks = 0; 
                        break;
                }
            }
            else if (clicks < 2)
            {
                DisableMarkers(); //Make lines and markers not visable if we hover away from objects, unless we have a complete line
            }

            if (Input.GetMouseButtonDown(1)) //right click resets measurement
            {
                DisableMarkers();
                clicks = 0;
            }
        }      
                        
    }


    // this function selects a subset of points from the Intensity matrix depending on the line drawn by the user - and writes this to the text file.
    private void GenerateData()
    {
        // 1. Fetch the screen parameters and the input matrix
        FetchScreenParameters();

        // 2. Generate copies of the start / end position but in LOCAL coordinates
        Vector3 localStartPoint = startPoint - screenOrigin.position;
        Vector3 localEndPoint = endPoint - screenOrigin.position;

        // 3. From the screen parameters, and the local start/end points - calculate the appropriate row/column bounds
        float x1 = localStartPoint.x; // horizontal bound 1
        float x2 = localEndPoint.x; // horizontal bound 1
        float y1 = localStartPoint.y; // vertical bound 1
        float y2 = localEndPoint.y; //vertical bound 2

        float x_left = Mathf.Min(x1, x2); // leftmost horizontal bound
        float x_right = Mathf.Max(x1, x2); // rightmost horizontal bound
        float y_bottom = Mathf.Min(y1, y2); // lowest vertical bound
        float y_top = Mathf.Max(y1, y2); // highest vertical bound

        // 4. transform these bounds from local coordinate space to matrix space
        x_left = x_left * screenResolution / screenWidth;
        x_right = x_right * screenResolution / screenWidth;
        y_bottom = y_bottom * screenResolution / screenHeight;
        y_top = y_top * screenResolution / screenHeight;

        // 5. loop through all the data within the bounds and export to a text file
        FetchUserData((int)x_left, (int)x_right, (int)y_bottom, (int)y_top);
    }

    private void FetchUserData(int x_left, int x_right, int y_bottom, int y_top)
    {
        using (TextWriter tw = new StreamWriter(file))
        {
            tw.Write("x (m)" + "\t" + "y (m)" + "\t" + "Intensity");
            tw.WriteLine();

            for (int i = x_left; i <= x_right; i++)
            {
                for (int j = y_bottom; j <= y_top; j++)
                {
                    float position_x = (i - x_left) * screenWidth / screenResolution;
                    float position_y = (j - y_bottom) * screenHeight / screenResolution;
                    float intensity = matrix[i, j];
                    tw.Write(position_x.ToString("#.000") + "\t" + position_y.ToString("#.000") + "\t" +  intensity.ToString("#.000"));
                    tw.WriteLine();
                }
            }
        }
        Debug.Log("File Saved: " + file);
    }

    private void FetchScreenParameters()
    {
        //sets the parameters which will be used by the measurement tool
        screenHeight = display.ScreenHeight;
        screenWidth = display.ScreenWidth;
        screenResolution = display.Resolution;
        matrix = display.Matrix;
    }


    Vector3 RoundVector(Vector3 objectPosition, int digits) //Method to handle rounding of transform components
    {
        //INPUT vector3 origin which gives a local origin to centre positions around

        float x = Round(objectPosition.x, digits);
        float y = Round(objectPosition.y, digits);
        float z = Round(objectPosition.z, digits);

        return new Vector3(x, y, z);
    }

    private float Round(float component, int digits) //Performs rounding to arbritrary accuracy
    {
        float multiplier = Mathf.Pow(10, digits);
        return Mathf.Round(component * multiplier) / multiplier; 
    }
    private void DisableMarkers()
    {
        if (clicks != 1)
        {
            markerOne.SetActive(false);
        }
        markerTwo.SetActive(false);
        line.enabled = false;
    }



    private void CreateTool()   //Handles creation of tool upon ticking of box in UI to enable tool
    {        
        markerOne = Instantiate(markerPrefab);
        markerTwo = Instantiate(markerPrefab);
        line.enabled = false;
        LoadLines();
    }

    private void EndTool()     //disable and destroy all objects, reset clicks to zero
    { 
        clicks = 0;
        GameObject.Destroy(markerOne);
        GameObject.Destroy(markerTwo);
        line.enabled = false;
        ClearAllLines();        
    }

    void SetLineProperties(LineRenderer line)
    {
        line.positionCount = 2;
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
        line.generateLightingData = true;
        line.numCornerVertices = 10;
        //line.material.color = Color.red;
        line.receiveShadows = false;
        line.shadowBias = 100f;
        Debug.Log(line.endWidth);
    }

    public void OnLayerChange(int newLayer)
    {
        if (modeActive)
        {
            currentLayer = newLayer;
            ClearAllLines();
            LoadLines();
        }
    }

    private void StoreLine()
    {
        Vector3 globalStart = FindGlobalPos(startTag);          //Get required global positions and rotations, as we convert the global points 
        Vector3 globalEnd = FindGlobalPos(endTag);              //of the line into local positions (at zero rotation)
        Quaternion startRotation = FindGlobalRotation(startTag);
        Quaternion endRotation = FindGlobalRotation(endTag);

        Vector3 localStart = Quaternion.Inverse(startRotation) * (startPoint - globalStart) ;
        Vector3 localEnd = Quaternion.Inverse(endRotation) * (endPoint - globalEnd);


        lineData.Add(new MeasurementLine(localStart, localEnd, startTag, endTag, currentLayer));
    }

    void DrawLine(MeasurementLine lineData)
    {
        GameObject lineObject = new GameObject();
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        SetLineProperties(line);

        line.SetPosition(0, (FindGlobalRotation(lineData.StartTag) * lineData.StartLocation) + FindGlobalPos(lineData.StartTag)); //adds the local position of the line to the global position of the relevant object
        line.SetPosition(1, (FindGlobalRotation(lineData.EndTag) * lineData.EndLocation) + FindGlobalPos(lineData.EndTag));

        line.name = lineData.Layer.ToString();
        lineObjects.Add(lineObject);
    }

    void ClearAllLines()
    {
        foreach (var item in lineObjects)
        {
            Destroy(item);
        }
        lineObjects.Clear();
        positionCache.Clear();
        rotationCache.Clear();
    }

    void LoadLines()
    {
        foreach (var item in lineData)
        {
            if (item.Layer == currentLayer)
            {
                DrawLine(item);
                Debug.Log(GetDistance(item));
            }
        }
    }


    Vector3 FindGlobalPos(string Name)
    //search positioncache dictionary for name
    //if not present then find gameobject in scene by name
    //return this vector
    {
        if (positionCache.TryGetValue(Name, out Vector3 globalPos))
        {
            return globalPos;
        }
        else
        { 
            GameObject currentObject = GameObject.Find(Name);
            Vector3 position = currentObject.transform.position;
            positionCache.Add(Name, position); //add this position to cache for future reference
            return position;
        }
    }

    Quaternion FindGlobalRotation(string Name)
    //search rotationcache dictionary for name
    //if not present then find gameobject in scene by name
    //return this quaternion
    {
        if (rotationCache.TryGetValue(Name, out Quaternion globalRot))
        {
            return globalRot;
        }
        else
        {
            GameObject currentObject = GameObject.Find(Name);
            Quaternion rotation = currentObject.transform.rotation;
            rotationCache.Add(Name, rotation); //add this rotation to cache for future reference
            return rotation;
        }
    }   

    float GetDistance(MeasurementLine lineData)             //Returns distance from an entry of lineData
    {
        Vector3 globalStart = lineData.StartLocation + FindGlobalPos(lineData.StartTag);
        Vector3 globalEnd = lineData.EndLocation + FindGlobalPos(lineData.EndTag);
        return (Vector3.Distance(globalStart, globalEnd));
    }       

}
