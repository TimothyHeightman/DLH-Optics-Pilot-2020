using JetBrains.Annotations;
using System.IO;
using UnityEngine;

public class IntensityCalcFloat : MonoBehaviour
{
    public DetectorBehaviour detector;
    [SerializeField] string file = "Assets/FRONTEND/DetectorScreen/img.txt";

    private GameObject gratCentre = null;

    //grating parameters (for no grating)
    public float maxSlitDim = 0.006f;//assuming a square grating 

    //slit parameters (so far only single slit)
    public float slitWidth = 0.00006f;
    public float slitHeight = 0.005f;

    //slit location with respect to centre of grating (not currently used)
    //public float slitWOffset;
    //public float slitHOffset;

    public int resolution;
    [Range(0, 13)] public int resolutionPower;

    private int[,] bitmap;
    private float[,] output;//matrix with 2DFFT

    // Start is called before the first frame update
    void Start()
    {
        resolution = Mathf.RoundToInt(Mathf.Pow(2, resolutionPower));  //QUICK FIX: Only currently works if resolution is a power of 2, due to fft algorithm
        positionObject();
        bitmap = new int[resolution, resolution];
        //buildGrating();
        fill();
    }


    public void OnButtonPress()
    {
        // when user pressed the display button, generate the FFT and display on screen
        FFT bitMapFFT = new FFT(bitmap);
        bitMapFFT.ForwardFFT();
        output = bitMapFFT.FFTLog;
        print(output);
        detector.DisplayInterferencePattern(output);
    }


    private void buildGrating() {
        //populate with code for grating and slit mesh if necessary
    }

    //to create a temporary gameobject in the centre of the grating 
    private void positionObject() {

        Vector3 temp = transform.parent.position;
        temp.y = temp.y + 1;

        gratCentre = Instantiate(
            gratCentre = new GameObject(),
            temp,
            Quaternion.identity           
        );
    }

    //to get the centre of the grating 
    public Vector3 getGratCentrePos() {
        return gratCentre.transform.position;
    }

    //creates high res bitmap for grating and slit 
    private void fill()
    {
        float beginSlitWidth, endSlitWidth, beginSlitHeight, endSlitHeight;
        beginSlitWidth = ((maxSlitDim - slitWidth) / 2) * (resolution / maxSlitDim) - 1;
        endSlitWidth = beginSlitWidth + (slitWidth * (resolution / maxSlitDim)) + 1;
        beginSlitHeight = ((maxSlitDim - slitHeight) / 2) * (resolution / maxSlitDim) - 1;
        endSlitHeight = beginSlitHeight + (slitHeight * (resolution / maxSlitDim)) + 1;

        Debug.Log((int)beginSlitWidth);

        Debug.Log((int)endSlitWidth);

        Debug.Log((int)beginSlitHeight);

        Debug.Log((int)endSlitHeight);



        for (int i = (int)beginSlitHeight; i < (int)endSlitHeight; i++)
        {
            for (int j = (int)beginSlitWidth; j < (int)endSlitWidth; j++)
            {
                bitmap[i, j] = 1;
            }
        }
    }

    //text outputs

    private void print(float[,] output)
    {
        using (TextWriter tw = new StreamWriter(file))
        {
            for (int i = 0; i < output.GetLength(0); i++)
            {
                for (int j = 0; j < output.GetLength(1); j++)
                {
                    //tw.Write(output[i, j].ToString() + "\t");
                    tw.Write(output[i, j].ToString("#.000") + "\t");
                }
                tw.WriteLine();
            }
        }
        Debug.Log("File saved: " + file);
    }

    //private void print(double[,] output) {
    //    using (TextWriter tw = new StreamWriter("file")) {
    //        for(int i = 0; i < output.GetLength(0); i++) {
    //            for(int j = 0; j < output.GetLength(1); j++) {
    //                tw.Write(output[i, j].ToString("#.000") + "\t");
    //            }
    //            tw.WriteLine();
    //        }
    //    }
    //}

    //private void print(int[,] output) {
    //    using (TextWriter tw = new StreamWriter("file")) {
    //        for (int i = 0; i < output.GetLength(0); i++) {
    //            for (int j = 0; j < output.GetLength(1); j++) {
    //                tw.Write(output[i, j] + "\t");
    //            }
    //            tw.WriteLine();
    //        }
    //    }
    //}


    //converters for arrays, not currently necessary
    private double[,] convertToDouble(FFT.COMPLEX[,] input) {
        double[,] temp = new double[input.GetLength(0), input.GetLength(1)];
        for (int i = 0; i < temp.GetLength(0); i++) {
            for (int j = 0; j < temp.GetLength(1); j++) {
                temp[i, j] = input[i, j].Magnitude();
            }
        }
        return temp;
    }

    private double[,] convertToDouble(int[,] input) {
        double[,] temp = new double[input.GetLength(0), input.GetLength(1)];
        for (int i = 0; i < temp.GetLength(0); i++) {
            for (int j = 0; j < temp.GetLength(1); j++) {
                temp[i, j] = input[i, j];
            }
        }
        return temp;
    }

    private double[,] convertToDouble(float[,] input) {
        double[,] temp = new double[input.GetLength(0), input.GetLength(1)];
        for (int i = 0; i < temp.GetLength(0); i++) {
            for (int j = 0; j < temp.GetLength(1); j++) {
                temp[i, j] = input[i, j];
            }
        }
        return temp;
    }


}
