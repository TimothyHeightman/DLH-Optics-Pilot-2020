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
    public ParentType ParentType { get; } // parent type is used to identify if the beams was created from a laser or is a dispersed/divided/.. from another beam
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

    /// <summary>
    /// Function is used to reverse the propagation of a this beam to some position
    /// </summary>
    /// <param name="movememntPositionIndex">Index to represent the cutting point</param>
    /// <returns></returns>
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

