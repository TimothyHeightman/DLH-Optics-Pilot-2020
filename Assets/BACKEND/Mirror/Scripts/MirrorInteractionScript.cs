using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public class MirrorInteractionScript : MonoBehaviour, InteractionScriptI
{
    /// <summary>
    /// Sets up the paralezation of the reflection for all beams
    /// </summary>
    /// <param name="discreteBeams">All the beams</param>
    /// <param name="hit">Beams`s corresponding collision details</param>
    /// <returns></returns>
    public List<List<Tuple<float3, float3, float3, float3>>> InteractWithDescreteBeam(List<DiscreteBeam> discreteBeams,List<RaycastHit> hit)
    {
        if(discreteBeams.Count != hit.Count)
        {
            throw new ArgumentOutOfRangeException("The given lists are of different lengths.");
        }

        var parallelReflection = new ParallelReflection();


        var discreteBeamsDirection = new NativeArray<float3>(discreteBeams.Count,Allocator.TempJob);
        var discreteBeamsPosition = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsPassingPosition1 = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsPassingPosition2 = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsPassingPosition3 = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsFinalDirection = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var raycastHitPositions = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var raycastNormal = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);

        for (int i=0;i< discreteBeams.Count; i++)
        {
            discreteBeamsDirection[i] = discreteBeams[i].DirectionOfPropagation;
            discreteBeamsPosition[i] = discreteBeams[i].LastCoordinate;
            raycastHitPositions[i] = ConvertToFloat3( hit[i].point);
            raycastNormal[i] = ConvertToFloat3(hit[i].normal.normalized);


        }

        parallelReflection.discreteBeamsDirection = discreteBeamsDirection;
        parallelReflection.discreteBeamsPosition = discreteBeamsPosition;
        parallelReflection.discreteBeamsFinalDirection = discreteBeamsFinalDirection;
        parallelReflection.discreteBeamsPassingPosition1 = discreteBeamsPassingPosition1;
        parallelReflection.discreteBeamsPassingPosition2 = discreteBeamsPassingPosition2;
        parallelReflection.discreteBeamsPassingPosition3 = discreteBeamsPassingPosition3;
        parallelReflection.raycastHitPositions = raycastHitPositions;
        parallelReflection.raycastNormal = raycastNormal;

        var handle = parallelReflection.Schedule(discreteBeamsDirection.Length, 1);

        handle.Complete();

        var result = new List<List<Tuple<float3,float3,float3, float3>>>();

        for(int i=0;i< discreteBeams.Count; i++)
        {
            result.Add(
                new List<Tuple<float3, float3, float3, float3>>
                {
                    new Tuple<float3,float3,float3, float3>(discreteBeamsPassingPosition1[i],discreteBeamsPassingPosition2[i],discreteBeamsPassingPosition3[i],discreteBeamsFinalDirection[i])
                }
                );
        }

        discreteBeamsDirection.Dispose();
        discreteBeamsPosition.Dispose();
        discreteBeamsPassingPosition1.Dispose();
        discreteBeamsPassingPosition2.Dispose();
        discreteBeamsPassingPosition3.Dispose();
        discreteBeamsFinalDirection.Dispose();
        raycastHitPositions.Dispose();
        raycastNormal.Dispose();

        return result;

    }

    private float3 ConvertToFloat3(Vector3 vector3)
    {
        return new float3(vector3.x, vector3.y, vector3.z);
    }

}

/// <summary>
/// Job for reflection
/// </summary>
internal struct ParallelReflection: IJobParallelFor
{
    public NativeArray<float3> discreteBeamsDirection;

    public NativeArray<float3> discreteBeamsPosition;

    public NativeArray<float3> discreteBeamsPassingPosition1;
    public NativeArray<float3> discreteBeamsPassingPosition2;
    public NativeArray<float3> discreteBeamsPassingPosition3;

    public NativeArray<float3> discreteBeamsFinalDirection;

    public NativeArray<float3> raycastHitPositions;

    public NativeArray<float3> raycastNormal;

    /// <summary>
    /// Reflects single beam
    /// </summary>
    /// <param name="index"></param>
    public void Execute(int index)
    {
        var cos = Vector3.Dot(discreteBeamsDirection[index], raycastNormal[index]);

        var DirectionOfPropagation = discreteBeamsDirection[index] - ConvertToFloat3(cos * raycastNormal[index]* 2);

        discreteBeamsPassingPosition1[index] = raycastHitPositions[index];
        discreteBeamsPassingPosition2[index] = float3.zero;
        discreteBeamsPassingPosition3[index] = float3.zero;

        discreteBeamsFinalDirection[index] = ConvertToFloat3(DirectionOfPropagation);
    }

    private float3 ConvertToFloat3(Vector3 vector3)
    {
        return new float3(vector3.x, vector3.y, vector3.z);
    }
}
