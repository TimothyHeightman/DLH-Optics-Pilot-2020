using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;



public class IntensityCalcCopy : MonoBehaviour
{
    public DetectorBehaviour detector;

    [SerializeField] string file = "Assets/FRONTEND/DetectorScreen/img.txt";


    //grating parameters (for no grating)
    public int gratingDim = 10;//assuming a square grating 

    //slit parameters (so far only single slit)
    public int slitWidth = 2;
    public int slitHeight = 4;

    //slit location with respect to centre of grating (not currently used)
    //public float slitWOffset;
    //public float slitHOffset;

    public int resolution = 100;

    private int[,] bitmap;
    private float[,] output;//matrix with 2DFFT

    // Start is called before the first frame update
    void Start()
    {
        bitmap = new int[resolution, resolution];
        //buildGrating();
        fill();
    }

    public void OnButtonPress()
    {
        // when user pressed the display button
        FFT bitMapFFT = new FFT(bitmap);
        bitMapFFT.ForwardFFT();
        output = bitMapFFT.FFTLog;
        print(output);

        //send to the detector for diplay
        detector.DisplayInterferencePattern(output);
    }

    private void buildGrating() {
        //populate with code for grating and slit mesh if necessary
    }


    //creates high res bitmap for grating and slit 
    private void fill() {
        float beginSlitWidth, endSlitWidth, beginSlitHeight, endSlitHeight;
        beginSlitWidth = ((gratingDim - slitWidth) / 2) * (resolution / gratingDim) - 1;
        endSlitWidth = beginSlitWidth + (slitWidth * (resolution / gratingDim)) + 1;

        beginSlitHeight = ((gratingDim - slitHeight) / 2) * (resolution / gratingDim) - 1;
        endSlitHeight = beginSlitHeight + (slitHeight * (resolution / gratingDim)) + 1;

        Debug.Log(beginSlitWidth);

        Debug.Log(endSlitWidth);

        Debug.Log(beginSlitHeight);

        Debug.Log(endSlitHeight);


        for (int i = (int)beginSlitHeight; i < endSlitHeight; i++) {
            for(int j = (int)beginSlitWidth; j < endSlitWidth; j++) {
                bitmap[i, j] = 1;
            }
        }
    }

    //text outputs

    private void print(float[,] output) {
        using (TextWriter tw = new StreamWriter(file)) {
            for (int i = 0; i < output.GetLength(0); i++) {
                for (int j = 0; j < output.GetLength(1); j++) {
                    //tw.Write(output[i, j].ToString() + "\t");
                    tw.Write(output[i, j].ToString("#.000") + "\t");
                }
                tw.WriteLine();
            }
        }
        Debug.Log("File saved: " + file);
    }

    //private void print(double[,] output) {
    //    using (TextWriter tw = new StreamWriter(file)) {
    //        for(int i = 0; i < output.GetLength(0); i++) {
    //            for(int j = 0; j < output.GetLength(1); j++) {
    //                tw.Write(output[i, j].ToString("#.000") + "\t");
    //            }
    //            tw.WriteLine();
    //        }
    //    }
    //}

    //private void print(int[,] output) {
    //    using (TextWriter tw = new StreamWriter(file)) {
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
