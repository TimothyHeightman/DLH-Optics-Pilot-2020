using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayStage
{
    public GameObject ObjectsOfInteraction { get; private set; }
    public Vector3 EnterPoint { get; private set; }
    public List<Vector3> ExitPoint { get; private set; }
    public Vector3 EnterDirection { get; private set; }
    public List<Vector3> ExitDirection { get; private set; }
    public DiscreteBeam ParentBeam { get; private set; }


    public RayStage(GameObject objectOfInteraction, DiscreteBeam parentBeam, Vector3 enterPoint, List<Vector3> exitPoint , Vector3 enterDirection, List<Vector3> exitDirection)
    {
        ObjectsOfInteraction = objectOfInteraction;
        EnterDirection = enterDirection;
        ExitDirection = exitDirection;
        EnterPoint = enterPoint;
        ExitPoint = exitPoint;
        ParentBeam = parentBeam;
    }
}
