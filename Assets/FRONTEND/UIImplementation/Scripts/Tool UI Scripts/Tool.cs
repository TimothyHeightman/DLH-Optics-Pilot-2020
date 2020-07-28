using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Class to provide the default behaviour for buttons. Implements IButtonInteraction allowing
 * for a shared way to set listeners when these tools are created at runtime when needed. */

public class Tool : MonoBehaviour, IButtonInteraction
{
    public Sprite imageSource;

    void Start()
    {
        if (imageSource != null)
        {
            gameObject.GetComponent<Image>().sprite = imageSource;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void ButtonInteract()
    //For inherited classes, this is where we link the tool scripts to the button interaction
    {

    }



}
