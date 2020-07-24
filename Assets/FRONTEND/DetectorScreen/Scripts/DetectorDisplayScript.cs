using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Script to handle the display functionality of the detector.
// 1. Takes a normalised inputMatrix (the output of the grating intensity matrix) of values 0-1 corresponding to relative intensity
// 2. Create a 2D texture of the same dimensions
// 3. Loop through all grid locations and populate the texture with colour depending on intensity
// 4. Apply the texture to the Image

public class DetectorDisplayScript : MonoBehaviour
{
    public Image IMG;
    public GameObject emailManager;

    [SerializeField] float screenDistance;
    float screenHeight;
    float screenWidth;
    int resolution; // size of the square matrix
    Texture2D screenTexture;
    float[,] inputMatrix;
    float[,] rgbMatrix;


    // public accessors - used by the measurement controller
    public float ScreenHeight { get { return screenHeight; } }
    public float ScreenWidth { get { return screenWidth; } }
    public float Resolution { get { return resolution; } }
    public float[,] Matrix { get { return inputMatrix; } }


    private void Start()
    {
        //fetch the screenWidth / screenHeight from the current gameobject
        screenWidth = transform.localScale.x;
        screenHeight = transform.localScale.y;
    }

    public void Fill(float[,] InputMatrix)
    {
        // 1. make local copy of input matrix
        inputMatrix = InputMatrix;

        // 1. Take inputMatrix and decode parameters (resolution)
        resolution = inputMatrix.GetLength(0);

        // 2. Convert the inputMatrix to RGB
        // currently not needed
        //rgbMatrix = ConvertToRGB(inputMatrix);

        // 3. Display the rgbMatrix to the screen
        Display(inputMatrix);
    }

    private float[,] ConvertToRGB(float[,] inputMatrix)
    {
        //Currently not needed
        return inputMatrix;
    }

    private void Display(float[,] InputMatrix)
    {
        screenTexture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                Color pixelColor = new Color(255f, 0f, 0f, InputMatrix[i, j]);
                screenTexture.SetPixel(i, j, pixelColor);
            }
        }

        screenTexture.Apply();

        IMG.GetComponent<Image>().sprite = Sprite.Create(screenTexture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
        Debug.Log("Matrix Display Complete");

        emailManager.SetActive(true);
    }
}