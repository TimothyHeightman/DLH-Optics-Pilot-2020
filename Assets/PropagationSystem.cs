using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System.Linq;

public class PropagationSystem : MonoBehaviour
{
    private List<DiscreteBeam> _discreteBeams = new List<DiscreteBeam>();
    private List<GameObject> _lasers = new List<GameObject>();
    private float maxLengthOfPropagation = 100;
    private Dictionary<Guid, GameObject> _lineRenderersConnections = new Dictionary<Guid, GameObject>();
    private List<GameObject> _lines = new List<GameObject>();
    private float delta = 0.00000001f;

    [SerializeField]
    public GameObject prefabLine;

    public void AddNewBeams(List<DiscreteBeam> discreteBeams)
    {

        foreach (var el in discreteBeams)
        {
            _discreteBeams.Add(el);
        }
    }

    public void AddLaser(GameObject laser)
    {
        _lasers.Add(laser);
    }

    private Transform GetLaserTransform(Guid parentId, ParentType parentType)
    {
        if(parentType == ParentType.Beam)
        {
            throw new Exception("Unsupported type of parent");
        }
        ;
        GameObject laser = null;
        foreach(var el in _lasers)
        {
            var a = el.GetComponent<BeamGeneratorScript>();

            if (el.GetComponent<BeamGeneratorScript>().Id == parentId)
            {
                laser = el;
            }
        }
        if(laser == null)
        {
            throw new Exception("No instance of a laser has been found");
        }

        return laser.transform;
    }

    private bool ChechFloat3IsZero(float3 a)
    {
        return a.x == 0 & a.y == 0 & a.z == 0;
    }

    private void Update()
    {
        var descreteBeamsTointeractIndeces = new List<int>();

        for (int i = 0; i < _discreteBeams.Count; i++)
        {
            var a = _discreteBeams[i].DirectionOfPropagation;

            var c = ChechFloat3IsZero(a);
            ;

            if (!ChechFloat3IsZero(_discreteBeams[i].DirectionOfPropagation))
            {
                descreteBeamsTointeractIndeces.Add(i);
            }
        }

        ;

        if (descreteBeamsTointeractIndeces.Count == 0)
        {
            return;
        }

        var hit = new RaycastHit();
        Ray ray = new Ray();
        int layerMask = ~(1 << 8);

        var limitedBeamDirection = new NativeList<float3>(Allocator.TempJob);
        var limitedBeamPosition = new NativeList<float3>(Allocator.TempJob);
        var limitedBeamFinalPosition = new NativeList<float3>(Allocator.TempJob);
        var limitedBeamIndeces = new List<int>();

        var separateObjectInteractions = new Dictionary<GameObject, Tuple<List<int>, List<RaycastHit>>>();

        for (int i = 0; i < descreteBeamsTointeractIndeces.Count; i++)
        {
            ray.origin = _discreteBeams[descreteBeamsTointeractIndeces[i]].LastCoordinate;
            ray.direction = _discreteBeams[descreteBeamsTointeractIndeces[i]].DirectionOfPropagation;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (separateObjectInteractions.ContainsKey(hit.collider.gameObject))
                {
                    separateObjectInteractions[hit.collider.gameObject].Item1.Add(descreteBeamsTointeractIndeces[i]);
                    separateObjectInteractions[hit.collider.gameObject].Item2.Add(hit);
                }
                else
                {
                    separateObjectInteractions.Add(hit.collider.gameObject, new Tuple<List<int>, List<RaycastHit>>(
                        new List<int> { descreteBeamsTointeractIndeces[i] },
                        new List<RaycastHit> { hit}
                        ));
                }
                
            }
            else
            {
                limitedBeamDirection.Add(_discreteBeams[descreteBeamsTointeractIndeces[i]].DirectionOfPropagation);
                limitedBeamPosition.Add(_discreteBeams[descreteBeamsTointeractIndeces[i]].LastCoordinate);
                limitedBeamFinalPosition.Add(float3.zero);
                limitedBeamIndeces.Add(descreteBeamsTointeractIndeces[i]);
            }
        }

        var parallelLimitedPropagation = new ParallelLimitedPropagation();
        parallelLimitedPropagation.discreteBeamsDirection = limitedBeamDirection;
        parallelLimitedPropagation.discreteBeamsPosition = limitedBeamPosition;
        parallelLimitedPropagation.discreteBeamsFinalPosition = limitedBeamFinalPosition;
        parallelLimitedPropagation.maxLengthOfPropagation = maxLengthOfPropagation;

        var handleLimitedPropagation = parallelLimitedPropagation.Schedule(limitedBeamDirection.Length, 1);

        var limitedBeamFinalPositionList = new List<float3>();
        var limitedBeamIndecesList = new List<int>();

        foreach (var el in separateObjectInteractions.Keys)
        {
            var discreteBeamsForInteraction = new List<DiscreteBeam>();
            foreach(var index in separateObjectInteractions[el].Item1)
            {
                discreteBeamsForInteraction.Add(_discreteBeams[index]);
            }
            try
            {
                var result = el.GetComponent<InteractionScriptI>().InteractWithDescreteBeam(discreteBeamsForInteraction, separateObjectInteractions[el].Item2);
                if (result.Count != separateObjectInteractions[el].Item1.Count)
                {
                    throw new ArgumentOutOfRangeException("The returned list size is out of range.");
                }
                ;
                for (int i = 0; i < separateObjectInteractions[el].Item1.Count; i++)
                {
                    AssignValuesAfterInteraction(separateObjectInteractions[el].Item1[i], result[i][0].Item1, result[i][0].Item2, result[i][0].Item3, result[i][0].Item4);
                }

            }
            catch (NullReferenceException)
            {
                for(int i=0;i< separateObjectInteractions[el].Item1.Count; i++)
                {
                    limitedBeamFinalPositionList.Add(separateObjectInteractions[el].Item2[i].point);
                    limitedBeamIndecesList.Add(separateObjectInteractions[el].Item1[i]);
                }
            }

            
        }

        handleLimitedPropagation.Complete();

        

        for (int i=0;i< limitedBeamFinalPosition.Length;i++)
        {
            limitedBeamFinalPositionList.Add(limitedBeamFinalPosition[i]);
            limitedBeamIndecesList.Add(limitedBeamIndeces[i]);
        }
        AssignValuesForLimitedPropagation(limitedBeamIndecesList, limitedBeamFinalPositionList);


        limitedBeamDirection.Dispose();
        limitedBeamPosition.Dispose();
        limitedBeamFinalPosition.Dispose();

        Project();
    }

    private void AssignValuesAfterInteraction(int beamindex,float3 movementPoint1, float3 movementPoint2, float3 movementPoint3, float3 direction)
    {
        var beam = _discreteBeams[beamindex];

        if( ChechFloat3IsZero( movementPoint1))
        {
            throw new ArgumentOutOfRangeException("The returned point of intersection is zero.");
        }
        if(chechTwofloat3sEqual(movementPoint1 - beam.LastCoordinate, beam.DirectionOfPropagation) )
        {
            throw new ArgumentException("The returned point of intersection does not belong on the line of propagation.");
        }

        beam.movementCoordinates.Add(movementPoint1);
        if (!ChechFloat3IsZero(movementPoint2))
        {
            beam.movementCoordinates.Add(movementPoint2);
        }
        if (!ChechFloat3IsZero(movementPoint3))
        {
            beam.movementCoordinates.Add(movementPoint3);
        }

        beam.DirectionOfPropagation = direction;

        _discreteBeams[beamindex] = beam;
    }

    private void AssignValuesForLimitedPropagation(List<int> beamindeces, List<float3> positions)
    {
        if (beamindeces.Count != positions.Count)
        {
            throw new ArgumentOutOfRangeException("The given lists are not of the same length.");
        }

        for(int i = 0; i < beamindeces.Count; i++)
        {
            _discreteBeams[beamindeces[i]].movementCoordinates.Add(positions[i]);
            var buf = _discreteBeams[beamindeces[i]];
            ;
            buf.DirectionOfPropagation = float3.zero;
            _discreteBeams[beamindeces[i]] = buf;
        }
    }

    private void Project()
    {
        foreach(var el in _discreteBeams)
        {
            if (el.Visualizes)
            {
                if (_lineRenderersConnections.ContainsKey(el.Id))
                {
                    UpdatePointsOnLinerenderer(el,_lineRenderersConnections[el.Id]);
                }
                else
                {
                    ;
                    _lines.Add(Instantiate(prefabLine, GetLaserTransform(el.ParentId, el.ParentType), true).gameObject);
                    _lines[_lines.Count - 1].layer = 8;
                    _lineRenderersConnections.Add(el.Id, _lines[_lines.Count-1]   ) ;
                    UpdatePointsOnLinerenderer(el, _lineRenderersConnections[el.Id]);
                }
            }
        }
    }

    private void UpdatePointsOnLinerenderer(DiscreteBeam discreteBeam, GameObject line)
    {
        var lineRenderer = line.GetComponent<LineRenderer>();

        lineRenderer.positionCount = discreteBeam.movementCoordinates.Count;

        for(int i = 0; i < discreteBeam.movementCoordinates.Count; i++)
        {
            if (!chechTwofloat3sEqual(discreteBeam.movementCoordinates[i], Vector3ToFloat3(lineRenderer.GetPosition(i))))
            {
                lineRenderer.SetPosition(i, discreteBeam.movementCoordinates[i]);
            }
        }
    }

    private void OnDestroy()
    {
        
    }

    private bool chechTwofloat3sEqual(float3 a, float3 b)
    {
        return (a.x - b.x) * (a.x - b.x) < delta & (a.y - b.y) * (a.y - b.y) < delta & (a.z - b.z) * (a.z - b.z) < delta;
    }

    private float3 Vector3ToFloat3(Vector3 vector3)
    {
        return new float3(vector3.x, vector3.y, vector3.z);
    }

}


internal struct ParallelLimitedPropagation : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float3> discreteBeamsDirection;
    [ReadOnly]
    public NativeArray<float3> discreteBeamsPosition;

    public NativeArray<float3> discreteBeamsFinalPosition;

    public float maxLengthOfPropagation;

    public void Execute(int index)
    {
        discreteBeamsFinalPosition[index] = discreteBeamsPosition[index] + discreteBeamsDirection[index] * maxLengthOfPropagation;
    }
}