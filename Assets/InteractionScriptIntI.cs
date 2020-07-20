using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public interface InteractionScriptI
{
    List<List<Tuple<float3,float3,float3,float3>>> InteractWithDescreteBeam(List<DiscreteBeam> discreteBeams, List<RaycastHit> hit);
}
