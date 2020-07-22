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

    int WIDTH;
    int HEIGHT;
    Texture2D screenTexture;
    float[,] rgbMatrix;
    float[,] convertedMatrix;


    private void Start()
    {
        //fetch the screenWidth / screenHeight from the current gameobject
        screenWidth = transform.localScale.x;
        screenHeight = transform.localScale.y;
    }

    public void Fill(float[,] InputMatrix)
    {
        // 1. Take inputMatrix and decode parameters (WIDTH/HEIGHT)
        WIDTH = InputMatrix.GetLength(0);
        HEIGHT = InputMatrix.GetLength(1);

        // 2. Convert the inputMatrix to RGB
        // currently not needed
        //rgbMatrix = ConvertToRGB(InputMatrix);


        // Convert from kx-space to x-space
        convertedMatrix = ConvertToXSpace(InputMatrix);

        // 3. Display the rgbMatrix to the screen
        Display(InputMatrix);
    }

    private float[,] ConvertToXSpace(float[,] inputMatrix)
    {
        float[,] converted;

        converted = inputMatrix;
        // need to convert the inputMatrix from x-space to k-space and renormalise.

        return converted;
    }

    private float[,] ConvertToRGB(float[,] inputMatrix)
    {
        //Currently not needed
        return inputMatrix;
    }

    private void Display(float[,] InputMatrix)
    {
        screenTexture = new Texture2D(WIDTH, HEIGHT, TextureFormat.ARGB32, false);

        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                Color pixelColor = new Color(255f, 0f, 0f, InputMatrix[i, j]);
                screenTexture.SetPixel(i, j, pixelColor);
            }
        }

        screenTexture.Apply();

        IMG.GetComponent<Image>().sprite = Sprite.Create(screenTexture, new Rect(0, 0, WIDTH, HEIGHT), new Vector2(0.5f, 0.5f));
        Debug.Log("Matrix Display Complete");

        emailManager.SetActive(true);
    }
}