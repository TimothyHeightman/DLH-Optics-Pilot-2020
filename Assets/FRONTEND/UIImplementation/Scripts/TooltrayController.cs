using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltrayController : MonoBehaviour
{

    //Need to expand this game objects height dependent on the number of icons
    RectTransform tooltrayRect;
    
    void Start()
    {
        tooltrayRect = gameObject.GetComponent<RectTransform>();
        tooltrayRect.sizeDelta = new Vector2(tooltrayRect.sizeDelta.x, tooltrayRect.sizeDelta.y + 60);
    }

   
    void Update()
    {
        
    }
}
