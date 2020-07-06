using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface InteractionScriptI
{
    List<Tuple<List<Vector3>,Vector3>> InteractWithDescreteBeam(DiscreteBeam descreteBeam, RaycastHit hit);
}
