using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface MultiBeamInteractionI
{
    void MultiBeamGetter(List<DiscreteBeam> discreteBeams,List<RaycastHit> raycastHits);
}
