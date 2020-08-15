using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class LensInteractionScript : MonoBehaviour, InteractionScriptI
{
    public GameObject LensObject;
    //radius1 and radius2 are the radii of the two spheres that define the lens. 1 is for the lens at z>0 and 2 for z<0. Z is consideret in local space.
    //thinkness is the distance between the two spherical surfaces at the edge of the lens.
    public double radius1, radius2, thinkness, length, coef;
    //delta is used for checking differences between the magnitudes of two vetors
    private float delta = 0.0000001f;

    /// <summary>
    /// Checks if the given values create a possible lens, which is if the two sferes overlap.
    /// </summary>
    public void Start()
    {
        if (Math.Abs(radius1) < length | Math.Abs(radius2) < length)
        {
            throw new Exception("One of the radiuses is less than the legth.");
        }
        if (radius1>0 & radius2 > 0)
        {
            if(radius1+radius2<= thinkness+ Math.Sqrt(-length * 2 + radius1*2) + Math.Sqrt(-length * 2 + radius2 * 2))
            {
                throw new ArgumentOutOfRangeException("The given values for the lens are impossible");
            }
        }
        if (radius1 * radius2 < 0)
        {
            if (radius1 < 0)
            {
                if (math.abs(radius1) - math.sqrt(radius1 * radius1 - length * length) + thinkness - (math.abs(radius2) - math.sqrt(radius2 * radius2 - length * length)) < 0)
                {
                    throw new ArgumentOutOfRangeException("The given values for the lens are impossible");
                }
            }
            else
            {
                if (math.abs(radius2) - math.sqrt(radius2 * radius2 - length * length) + thinkness - (math.abs(radius1) - math.sqrt(radius1 * radius1 - length * length)) < 0)
                {
                    throw new ArgumentOutOfRangeException("The given values for the lens are impossible");
                }
            }
        }
    }


    /// <summary>
    /// Intecation with discretebeam gets the beams and prepares them for multithreaded calculation and assigns the value after calculation.
    /// </summary>
    /// <param name="discreteBeams">All the beams to interact with</param>
    /// <param name="hit">The collision details for all beams</param>
    /// <returns></returns>
    public List<List<Tuple<float3, float3, float3, float3>>> InteractWithDescreteBeam(List<DiscreteBeam> discreteBeams, List<RaycastHit> hit)
    {


        if (discreteBeams.Count != hit.Count)
        {
            throw new ArgumentOutOfRangeException("The given lists are of different lengths.");
        }

        var parallelLensInteraction = new ParallelLensInteraction();


        var discreteBeamsDirection = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsPosition = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsPassingPosition1 = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsPassingPosition2 = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsPassingPosition3 = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);
        var discreteBeamsFinalDirection = new NativeArray<float3>(discreteBeams.Count, Allocator.TempJob);

        double zf, zb;

        if (radius1 < 0 & radius2 < 0)
        {
            zf = -Math.Sqrt(-length * length + radius1 * radius1) + thinkness / 2;
            zb = Math.Sqrt(-length * length + radius2 * radius2) - thinkness / 2;
        }
        else if (radius1 > 0 & radius2 > 0)
        {
            zf = Math.Sqrt(-length * length + radius1 * radius1) + thinkness / 2;
            zb = -Math.Sqrt(-length * length + radius2 * radius2) - thinkness / 2;
        }
        else if (radius1 < 0 & radius2 > 0)
        {
            zf = -Math.Sqrt(-length * length + radius1 * radius1) + thinkness / 2;
            zb = -Math.Sqrt(-length * length + radius2 * radius2) - thinkness / 2;
        }
        else if (radius2 < 0 & radius1 > 0)
        {
            zf = Math.Sqrt(-length * length + radius1 * radius1) + thinkness / 2;
            zb = Math.Sqrt(-length * length + radius2 * radius2) - thinkness / 2;
        }
        else
        {
            throw new Exception("Unsupported type of lens");
        }

        for (int i = 0; i < discreteBeams.Count; i++)
        {
            discreteBeamsDirection[i] = LensObject.transform.InverseTransformDirection(discreteBeams[i].DirectionOfPropagation);
            discreteBeamsPosition[i] = ReverseScalingOnInverse(LensObject.transform.InverseTransformPoint(discreteBeams[i].LastCoordinate), LensObject.transform.localScale);
        }

        parallelLensInteraction.discreteBeamsDirection = discreteBeamsDirection;
        parallelLensInteraction.discreteBeamsPosition = discreteBeamsPosition;
        parallelLensInteraction.discreteBeamsFinalDirection = discreteBeamsFinalDirection;
        parallelLensInteraction.discreteBeamsPassingPosition1 = discreteBeamsPassingPosition1;
        parallelLensInteraction.discreteBeamsPassingPosition2 = discreteBeamsPassingPosition2;
        parallelLensInteraction.discreteBeamsPassingPosition3 = discreteBeamsPassingPosition3;
        parallelLensInteraction.coef = coef;
        parallelLensInteraction.zb = zb;
        parallelLensInteraction.zf = zf;
        parallelLensInteraction.radius1 = radius1;
        parallelLensInteraction.radius2 = radius2;
        parallelLensInteraction.delta = delta;

        ;

        var handle = parallelLensInteraction.Schedule(discreteBeamsDirection.Length, 1);

        handle.Complete();

        var result = new List<List<Tuple<float3, float3, float3, float3>>>();

        for (int i = 0; i < discreteBeams.Count; i++)
        {
            result.Add(
                new List<Tuple<float3, float3, float3, float3>>
                {
                    new Tuple<float3,float3,float3, float3>(
                        TransformPoint(discreteBeamsPassingPosition1[i], LensObject.transform.localScale),
                        TransformPoint(discreteBeamsPassingPosition2[i], LensObject.transform.localScale),
                        float3.zero,
                        LensObject.transform.TransformDirection( discreteBeamsFinalDirection[i]))
                }
                );
        }
        ;
        discreteBeamsDirection.Dispose();
        discreteBeamsPosition.Dispose();
        discreteBeamsPassingPosition1.Dispose();
        discreteBeamsPassingPosition2.Dispose();
        discreteBeamsPassingPosition3.Dispose();
        discreteBeamsFinalDirection.Dispose();


        return result;

    }

    /// <summary>
    /// When conversion from local to global coordinates happen, this function makes the transition ignore the scale of the local coordinate system
    /// </summary>
    /// <param name="position">local position</param>
    /// <param name="scale">scale of gameObject</param>
    /// <returns></returns>
    private Vector3 ReverseScalingOnInverse(Vector3 position, Vector3 scale)
    {
        return new Vector3(position.x * scale.x, position.y * scale.y, position.z * scale.z);
    }

    /// <summary>
    /// Directly transforms point from global to local space and ignores the scale of the local coordinate system
    /// </summary>
    /// <param name="position"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    private Vector3 TransformPoint(Vector3 position, Vector3 scale)
    {
        return LensObject.transform.TransformPoint(new Vector3(position.x / scale.x, position.y / scale.y, position.z / scale.z));
    }
}


/// <summary>
/// The job that defines the lens interaction
/// </summary>
internal struct ParallelLensInteraction : IJobParallelFor
{
    public NativeArray<float3> discreteBeamsDirection;
    public NativeArray<float3> discreteBeamsPosition;

    public NativeArray<float3> discreteBeamsPassingPosition1;
    public NativeArray<float3> discreteBeamsPassingPosition2;
    public NativeArray<float3> discreteBeamsPassingPosition3;

    public NativeArray<float3> discreteBeamsFinalDirection;

    public double radius1, radius2, zf,zb,coef,delta;

    public void Execute(int index)
    {
        RefractSingleBeam(index);
    }

    /// <summary>
    /// refracts one beam by finding the actual collision position refracting once finding second collision position and refracting a second time
    /// </summary>
    /// <param name="index">Index of descretebeam</param>
    private void RefractSingleBeam(int index)
    {
        ;
        if (discreteBeamsPosition[index].z > 0)
        {
            var firstInterceptionPoint = CalculatePointOfIntersection(discreteBeamsPosition[index], discreteBeamsDirection[index], radius1, zf);
            var firstRefractedDirection = CalculateRefractionDirection(discreteBeamsDirection[index], firstInterceptionPoint, Convert.ToSingle(zf), Convert.ToSingle(coef));
            var secondInterceptionPoint = CalculatePointOfIntersection(firstInterceptionPoint, firstRefractedDirection, -radius2, zb);
            var secondRefractionDirection = CalculateRefractionDirection(firstRefractedDirection, secondInterceptionPoint, Convert.ToSingle(zb), Convert.ToSingle(1 / coef));

            discreteBeamsPassingPosition1[index] = firstInterceptionPoint;
            discreteBeamsPassingPosition2[index] = secondInterceptionPoint;
            discreteBeamsPassingPosition3[index] = float3.zero;
            discreteBeamsFinalDirection[index] = secondRefractionDirection;

        }
        else
        {
            ;
            var firstInterceptionPoint = CalculatePointOfIntersection(discreteBeamsPosition[index], discreteBeamsDirection[index], radius2, zb);
            var firstRefractedDirection = CalculateRefractionDirection(discreteBeamsDirection[index], firstInterceptionPoint, Convert.ToSingle(zb), Convert.ToSingle(coef));
            var secondInterceptionPoint = CalculatePointOfIntersection(firstInterceptionPoint, firstRefractedDirection, -radius1, zf);
            var secondRefractionDirection = CalculateRefractionDirection(firstRefractedDirection, secondInterceptionPoint, Convert.ToSingle(zf), Convert.ToSingle(1 / coef));
            ;

            discreteBeamsPassingPosition1[index] = firstInterceptionPoint;
            discreteBeamsPassingPosition2[index] = secondInterceptionPoint;
            discreteBeamsPassingPosition3[index] = float3.zero;
            discreteBeamsFinalDirection[index] = secondRefractionDirection;
        }
    }

    /// <summary>
    /// Calculates the collision position between a beam and the sphere part of a lens
    /// </summary>
    /// <param name="incomingPoint">Point from which ray is comming</param>
    /// <param name="incomingDirection">Direction in which beam is comming</param>
    /// <param name="radius">radius of current sphere</param>
    /// <param name="zp">position of the centre of the sphere(x=0,y=0)</param>
    /// <returns></returns>
    private float3 CalculatePointOfIntersection(float3 incomingPoint, float3 incomingDirection, double radius, double zp)
    {

        var b = 2 * incomingPoint.x * incomingDirection.x
            + 2 * incomingPoint.y * incomingDirection.y
            + 2 * incomingPoint.z * incomingDirection.z
            - 2 * zp * incomingDirection.z;
        var a = incomingDirection.x * incomingDirection.x
            + incomingDirection.y * incomingDirection.y
            + incomingDirection.z * incomingDirection.z;
        var c =
            +zp * zp
            - radius * radius
            + incomingPoint.x * incomingPoint.x
            + incomingPoint.y * incomingPoint.y
            + incomingPoint.z * incomingPoint.z
            - 2 * zp * incomingPoint.z;

        if (b * b - 4 * a * c < 0)
        {
            throw new ArgumentOutOfRangeException("The given ray does not intercept the lens.");
        }

        var t1 = Convert.ToSingle((-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a));
        var t2 = Convert.ToSingle((-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a));

        var point1 = CalculatePointFromT(incomingPoint, incomingDirection, Convert.ToSingle(t1));
        var point2 = CalculatePointFromT(incomingPoint, incomingDirection, Convert.ToSingle(t2));
        ;

        if (!chechTwofloat3sEqual(Normalize(point1 - incomingPoint) ,Normalize(point2 - incomingPoint)) & !chechTwofloat3sEqual(Normalize(point1 - incomingPoint),- Normalize(point2 - incomingPoint))&
            !chechTwofloat3sEqual(point1 - incomingPoint,float3.zero) & !chechTwofloat3sEqual(point2 - incomingPoint, float3.zero))
        {
            throw new Exception("Calculated values for interception point do not lie on the same line.");
        }

        if (chechTwofloat3sEqual(Normalize(point1 - incomingPoint), incomingDirection))
        {
            if (chechTwofloat3sEqual(Normalize(point2 - incomingPoint), incomingDirection))
            {
                if (radius < 0)
                {
                    if (Magnitude2(point1 - incomingPoint) < Magnitude2(point2 - incomingPoint))
                    {
                        return point1;
                    }
                    else
                    {
                        return point2;
                    }
                }
                else
                {
                    if (Magnitude2(point1 - incomingPoint) > Magnitude2(point2 - incomingPoint))
                    {
                        return point1;
                    }
                    else
                    {
                        return point2;
                    }
                }
            }
            else
            {
                return point1;
            }
        }
        else
        {
            if (chechTwofloat3sEqual( Normalize(point2 - incomingPoint), incomingDirection))
            {
                return point2;
            }
            else
            {
                throw new ArgumentException("Neither of the calculated intersection points satisfy the direction of propagation.");
            }
        }
    }

    /// <summary>
    /// Caclulates direction after refraction
    /// </summary>
    /// <param name="incomingDirection">Direction in which beam is comming</param>
    /// <param name="interceptionPoint">Interception Point</param>
    /// <param name="zp">position of the centre of the sphere(x=0,y=0)</param>
    /// <param name="totalCoef">Fraction of the refraction coeficients, nominator is for outer space, denominator for inside space</param>
    /// <returns></returns>
    private float3 CalculateRefractionDirection(float3 incomingDirection, float3 interceptionPoint, float zp, float totalCoef)
    {
        var normal = Normalize( new float3(2 * interceptionPoint.x, 2 * interceptionPoint.y, 2 * (interceptionPoint.z - zp)));

        var cos = Vector3.Dot(normal, incomingDirection);

        if (cos < 0)
        {
            cos = -cos;
            normal = -normal;
        }

        var sin = Math.Abs(Vector3.Cross(normal, incomingDirection).magnitude);

        var newSin = sin / totalCoef;
        ;
        if (newSin > 1)
        {
            throw new ArgumentOutOfRangeException("The beam propagating through the lens is experiencing total internal reflection;");
        }

        var tangent = Normalize(incomingDirection - ConvertToFloat3( Vector3.Project(incomingDirection, normal)));

        var a = tangent * Convert.ToSingle(newSin);
        var b = normal * Convert.ToSingle(Math.Sqrt(1 - newSin * newSin));

        var result = Normalize(tangent * Convert.ToSingle(newSin) + normal * Convert.ToSingle(Math.Sqrt(1 - newSin * newSin)));

        return result;

    }

    private float3 Normalize(float3 float3)
    {
        var result = new float3();
        var factor = Convert.ToSingle(Math.Sqrt(float3.x * float3.x + float3.y * float3.y + float3.z * float3.z));
        if (factor == 0)
        {
            result.x = 0;
            result.y = 0;
            result.z = 0;
        }
        else
        {
            result.x = float3.x / factor;
            result.y = float3.y / factor;
            result.z = float3.z / factor;
        }
        return result;
    }

    private bool chechTwofloat3sEqual(float3 a, float3 b)
    {
        return (a.x - b.x) * (a.x - b.x) < delta & (a.y - b.y) * (a.y - b.y) < delta & (a.z - b.z) * (a.z - b.z) < delta;
    }

    private float Magnitude2(float3 float3)
    {
        return float3.x * float3.x + float3.y * float3.y + float3.z * float3.z;
    }

    /// <summary>
    /// Calculates coordinates of a beam propagated by certain distance
    /// </summary>
    /// <param name="incomingPoint"></param>
    /// <param name="incomingDirection"></param>
    /// <param name="t">propagation distance</param>
    /// <returns></returns>
    private float3 CalculatePointFromT(float3 incomingPoint, float3 incomingDirection, float t)
    {
        return new float3(t * incomingDirection.x + incomingPoint.x, t * incomingDirection.y + incomingPoint.y, t * incomingDirection.z + incomingPoint.z);
    }

    private float3 ConvertToFloat3(Vector3 vector3)
    {
        return new float3(vector3.x, vector3.y, vector3.z);
    }
}