using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MirrorInteractionScript : MonoBehaviour, InteractionScriptI
{

    public List<Tuple<List<Vector3>, Vector3>> InteractWithDescreteBeam(DiscreteBeam descreteBeam, RaycastHit hit)
    {
        return ReflectingSingleRays(descreteBeam,hit);
    }

    private List<Tuple<List<Vector3>, Vector3>> ReflectingSingleRays(DiscreteBeam descreteBeam, RaycastHit hit)
    {
        var cos = Vector3.Dot(descreteBeam.DirectionOfPropagation, hit.normal.normalized);

        var DirectionOfPropagation = descreteBeam.DirectionOfPropagation - cos * hit.normal.normalized * 2;

        return new List<Tuple<List<Vector3>, Vector3>>()
        {
            new Tuple<List<Vector3>, Vector3> (new List<Vector3>(){ hit.point},DirectionOfPropagation)
        };
    }
}
