using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBeamInteraction : MonoBehaviour, MultiBeamInteractionI
{
    public void MultiBeamGetter(List<DiscreteBeam> discreteBeams, List<RaycastHit> raycastHits)
    {
        Debug.Log("The given beams are : " + discreteBeams.Count + " and the given RaycastHits are : "+raycastHits.Count);
    }

}
