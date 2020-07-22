using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorBehaviour : MonoBehaviour
{
    DetectorDisplayScript display;

    private void Start()
    {
        display = GetComponent<DetectorDisplayScript>();
    }

    public void DisplayInterferencePattern(float[,] inputMatrix)
    {   
        display.Fill(inputMatrix);
    }

    //populates an example matrix for testing purposes
    //private float[,] TestMatrix()
    //{
    //    // create the output
    //    float[,] output = new float[WIDTH, HEIGHT];

    //    // For Testing Purposes, populate the matrix with random noise
    //    for (int i = 0; i < WIDTH; i++)
    //    {
    //        for (int j = 0; j < HEIGHT; j++)
    //        {
    //            output[i, j] = Mathf.Pow(Mathf.Sin(i*360/WIDTH), 2);
    //        }
    //    }

    //    Debug.Log("Matrix Generation Complete");
    //    return output;
    //}
}
