 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor;
using Unity.Mathematics;

public enum ParentType{
    Laser,
    Beam
}

public struct DiscreteBeam
{
    private float3 _directionOfPropagation;
    public List<float3> movementCoordinates { get; set; }
    public float3 LastCoordinate { get => movementCoordinates[movementCoordinates.Count - 1]; }
    public float3 DirectionOfPropagation
    {
        get
        {
            return _directionOfPropagation;
        }
        set
        {
            var factor = Convert.ToSingle( Math.Sqrt( value.x* value.x + value.y* value.y + value.z * value.z));
            if (factor == 0)
            {
                _directionOfPropagation.x = 0;
                _directionOfPropagation.y = 0;
                _directionOfPropagation.z = 0;
            }
            else
            {
                _directionOfPropagation.x = value.x / factor;
                _directionOfPropagation.y = value.y / factor;
                _directionOfPropagation.z = value.z / factor;
            }
        }
    }
    public Guid Id { get; }
    public Guid ParentId { get; }
    public ParentType ParentType { get; }
    public bool Visualizes { get; }

    public double InitialPhase { get; private set; }
    public double Frequency { get; private set; }
    public double InitialIntensity { get; private set; }


    public DiscreteBeam(Guid parentId, ParentType parentType,float3 initialCoordinates, float3 initialDirection, bool visualizes ,double initialIntensity, double frequency, double initialPhase)
    {

        if (frequency < 0)
        {
            throw new ArgumentOutOfRangeException("The given frequency is negative.");
        }
        if (initialIntensity < 0)
        {
            throw new ArgumentOutOfRangeException("The given intensity is negative.");
        }
        if(parentId == Guid.Empty)
        {
            throw new ArgumentException("The given Parent Id is Guid.zero.");
        }

        ParentType = parentType;

        ParentId = parentId;

        Visualizes = visualizes;

        _directionOfPropagation = initialDirection;

        movementCoordinates = new List<float3>()
        {
            initialCoordinates
        };

        Id = Guid.NewGuid() ;




        InitialPhase = initialPhase % (2 * Mathf.PI);

        InitialIntensity = initialIntensity;

        Frequency = frequency;
    }

    public DiscreteBeam CutBeam(int movememntPositionIndex)
    {
        if (movememntPositionIndex == movementCoordinates.Count - 1)
        {
            return this; 
        }
        DirectionOfPropagation = movementCoordinates[movememntPositionIndex + 1] - movementCoordinates[movememntPositionIndex];
        movementCoordinates = movementCoordinates.GetRange(0, movememntPositionIndex+1);
        return this;
    }

}

/*
public class DiscreteBeam
{
    private Vector3 _directionOfPropagation;
    private GameObject _beamLine { get; set; }
    private DiscreteBeam _ParentBeam { get; set; }
    private List<DiscreteBeam> _ChildBeams { get; set; }
    private GameObject _Laser{ get; set; }
    private List<GameObject> _ObjectsOfInteraction {get; set; } 
    private List<Vector3> _movementCoordinates { get; set; }
    public Vector3 LastCoordinate { get => _movementCoordinates[_movementCoordinates.Count - 1]; }
    public bool Visualises { get => _beamLine != null; }
    public Vector3 DirectionOfPropagation 
    {
        get
        {
            return _directionOfPropagation;
        }
        private set
        {
            _directionOfPropagation = value.normalized;
        }
    }
    public double InitialPhase { get; private set; }
    public double Frequency { get; private set; }
    public double InitialIntensity { get; private set; }
    

    private float maxLengthOfPropagation;

    /*
    public DiscreteBeam(GameObject laser, GameObject prefabLine, Vector3 initialLocalCoordinates, double initialIntensity, double frequency, double initialPhase)
    {
        if(frequency<0){
            throw new System.ArgumentOutOfRangeException("The given frequency is negative.");
        }
        if(initialIntensity < 0){
            throw new System.ArgumentOutOfRangeException("The given intensity is negative.");
        }
        if(prefabLine==null){
            throw new System.ArgumentNullException("The given GameObjects for ray is null");
        }
        if(laser==null){
            throw new System.ArgumentNullException("The given GameObjects for laser is null");
        }

        _Laser = laser;
        
        InitialPhase = initialPhase%(2*Mathf.PI);

        InitialIntensity = initialIntensity;

        Frequency = frequency;

        _ParentBeam = null;

        _ChildBeams = new List<DiscreteBeam>();

        maxLengthOfPropagation = 100;

        _movementCoordinates = new List<Vector3>()
        {
            _Laser.transform.Find("ProjectorGlass").transform.position + initialLocalCoordinates
        };

        _beamLine = MonoBehaviour.Instantiate(prefabLine, _Laser.transform.Find("ProjectorGlass").transform, false);
        _beamLine.layer = 8;

        _beamLine.transform.localPosition = initialLocalCoordinates;

        var lineRenderer = _beamLine.GetComponent<LineRenderer>();

        DirectionOfPropagation = (_beamLine.transform.rotation*(lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0))).normalized;

        _beamLine.GetComponent<LineRenderer>().positionCount = 1;

        
    }

    public DiscreteBeam(GameObject laser, Vector3 initialLocalCoordinates, double initialIntensity, double frequency, double initialPhase)
    {
        if (frequency < 0)
        {
            throw new System.ArgumentOutOfRangeException("The given frequency is negative.");
        }
        if (initialIntensity < 0)
        {
            throw new System.ArgumentOutOfRangeException("The given intensity is negative.");
        }
        
        if (laser == null)
        {
            throw new System.ArgumentNullException("The given GameObjects for laser is null");
        }

        _Laser = laser;

        InitialPhase = initialPhase % (2 * Mathf.PI);

        DirectionOfPropagation = Vector3.forward;

        _movementCoordinates = new List<Vector3>()
        {
            _Laser.transform.Find("ProjectorGlass").transform.position + initialLocalCoordinates
        };

        InitialIntensity = initialIntensity;

        Frequency = frequency;

        _ParentBeam = null;

        _ChildBeams = new List<DiscreteBeam>();

        maxLengthOfPropagation = 100;
    }

    public void Propagate()
    {
        RaycastHit hit;
        Ray ray = new Ray();

        while (true)
        {

            ray.origin = _movementCoordinates[_movementCoordinates.Count-1];
            ray.direction = DirectionOfPropagation;
            ;
            
            int layerMask =~ (1<<8);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity ,layerMask))
            {
                ;
                var a = hit.collider.gameObject.layer;
                InteractwithObject(hit);
            }
            else
            {
                ;
                LimitedPropagation();
                break;
            }
        }
    }

    private void InteractwithObject(RaycastHit hit)
    {
        List<Tuple<List<Vector3>, Vector3>> result;

        try
        {
            result = hit.collider.gameObject.GetComponent<InteractionScriptI>().InteractWithDescreteBeam(this, hit);

        }
        catch(NullReferenceException)
        {
            AddRayPoint(hit.point);
            DirectionOfPropagation = Vector3.zero;
            return;
        }

        if (result == null)
        {
            throw new ArgumentNullException("The results of the interaction are returned as null.");
        }

        if(result[0].Item1.Count == 0)
        {
            throw new ArgumentException("The interaction has not returned any points.");
        }

        if((result[0].Item1[0]- _movementCoordinates[_movementCoordinates.Count-1]).normalized != DirectionOfPropagation)
        {
            throw new Exception("Interaction does not return the right first intesection point. It returns the direction:"
                +(result[0].Item1[0] - _movementCoordinates[_movementCoordinates.Count - 1]).normalized.ToString()
                +".Actual direction is:"
                + DirectionOfPropagation.ToString()
                );
        }


        AddRayPoints(result[0].Item1);

        DirectionOfPropagation = result[0].Item2;
    }

    private void AddRayPoint(Vector3 point)
    {
        if (_beamLine != null)
        {
            var lineRenderer = _beamLine.GetComponent<LineRenderer>();
            lineRenderer.positionCount++;

            lineRenderer.SetPosition(lineRenderer.positionCount - 1, _beamLine.transform.InverseTransformPoint(point));
        }
        _movementCoordinates.Add(point);
    }

    private void AddRayPoints(List<Vector3> points)
    {
        for (int j = 0; j < points.Count; j++)
        {
            AddRayPoint(points[j]);
        }
    }

    private void LimitedPropagation()
    {
        if(DirectionOfPropagation == Vector3.zero)
        {
            return;
        }
        if (_beamLine != null)
        {
            LineRenderer beamLineRenderer = _beamLine.GetComponent<LineRenderer>();
            beamLineRenderer.positionCount++;
            var count = beamLineRenderer.positionCount;
            beamLineRenderer.SetPosition(count - 1, beamLineRenderer.GetPosition(count - 2) + _beamLine.transform.InverseTransformDirection(DirectionOfPropagation) * maxLengthOfPropagation);
        }
        _movementCoordinates.Add(_movementCoordinates[_movementCoordinates.Count - 1] + DirectionOfPropagation * maxLengthOfPropagation);
    }
    /*
    public Tuple<Vector3,Vector3> GetLastPositionAndDirection()
    {
        var lineRenderer = beamLine.GetComponent<LineRenderer>();

        return new Tuple<Vector3, Vector3>(lineRenderer.GetPosition(lineRenderer.positionCount-1).,DirectionOfPropagation);
    }
    

}
*/



