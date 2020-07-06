using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class LensInteractionScript : MonoBehaviour, InteractionScriptI
{
    public GameObject LensObject;
    public double radius1, radius2, thinkness, length, coef;

    public void Start()
    {
        if(radius1>0 & radius2 > 0)
        {
            if(radius1+radius2>= thinkness+ Math.Sqrt(length * 2 + radius1*2) + Math.Sqrt(length * 2 + radius2 * 2))
            {
                throw new ArgumentOutOfRangeException("The given values for the lens are impossible");
            }
        }
        if( Math.Abs( radius1) < length | Math.Abs(radius2) < length)
        {
            throw new Exception("One of the radiuses is less than the legth."); 
        }
    }

    public List<Tuple<List<Vector3>, Vector3>> InteractWithDescreteBeam(DiscreteBeam descreteBeam, RaycastHit hit)
    {
        return RefractSingleBeam(descreteBeam);
    }

    private List<Tuple<List<Vector3>, Vector3>> RefractSingleBeam(DiscreteBeam descreteBeam)
    {
        ;
        var incomingPoint = LensObject.transform.InverseTransformPoint(descreteBeam.LastCoordinate);
        var incomingDirection = LensObject.transform.InverseTransformDirection(descreteBeam.DirectionOfPropagation);

        double zf, zb;

        if (radius1 < 0 & radius2 < 0)
        {
            zf = -Math.Sqrt(-length * length + radius1 * radius1) + thinkness / 2;
            zb = Math.Sqrt(-length * length + radius2 * radius2) - thinkness / 2;
        }
        else if (radius1 > 0 & radius2 > 0)
        {
            zf = Math.Sqrt(- length * length + radius1 * radius1) + thinkness / 2;
            zb = -Math.Sqrt(- length * length + radius2 * radius2) - thinkness / 2;
        }
        else
        {
            throw new Exception("Unsupported type of lens");
        }

        if (incomingPoint.z > 0)
        {
            var firstInterceptionPoint = CalculatePointOfIntersection(incomingPoint, incomingDirection, radius1, zf);
            var firstRefractedDirection = CalculateRefractionDirection(incomingDirection, firstInterceptionPoint, Convert.ToSingle(zf), Convert.ToSingle(coef));
            var secondInterceptionPoint = CalculatePointOfIntersection(firstInterceptionPoint, firstRefractedDirection, radius2, zb);
            var secondRefractionDirection = CalculateRefractionDirection(firstRefractedDirection, secondInterceptionPoint, Convert.ToSingle(zb), Convert.ToSingle(1 / coef));

            return new List<Tuple<List<Vector3>, Vector3>>()
            {
                new Tuple<List<Vector3>, Vector3>
                (
                    new List<Vector3>()
                    {
                        firstInterceptionPoint,secondInterceptionPoint
                    },secondRefractionDirection
                )
            };
        }
        else
        {
            
            var firstInterceptionPoint = CalculatePointOfIntersection(incomingPoint, incomingDirection, radius2, zb);
            var firstRefractedDirection = CalculateRefractionDirection(incomingDirection, firstInterceptionPoint, Convert.ToSingle(zb), Convert.ToSingle(coef));
            var secondInterceptionPoint = CalculatePointOfIntersection(firstInterceptionPoint, firstRefractedDirection, radius1, zf);
            var secondRefractionDirection = CalculateRefractionDirection(firstRefractedDirection, secondInterceptionPoint, Convert.ToSingle(zf), Convert.ToSingle(1 / coef));

            return new List<Tuple<List<Vector3>, Vector3>>()
            {
                new Tuple<List<Vector3>, Vector3>
                (
                    new List<Vector3>()
                    {
                        LensObject.transform.TransformPoint( firstInterceptionPoint),LensObject.transform.TransformPoint(secondInterceptionPoint)
                    },LensObject.transform.TransformDirection( secondRefractionDirection)
                )
            };
            
        }
    }

    private Vector3 CalculatePointOfIntersection(Vector3 incomingPoint, Vector3 incomingDirection, double radius, double zp)
    {
        
        var b = 2 * incomingPoint.x * incomingDirection.x / incomingDirection.z
            + 2 * incomingPoint.y * incomingDirection.y / incomingDirection.z
            - 2 * incomingDirection.x * incomingDirection.x * incomingPoint.z / (incomingDirection.z * incomingDirection.z)
            - 2 * incomingDirection.y * incomingDirection.y * incomingPoint.z / (incomingDirection.z * incomingDirection.z)
            - 2 * zp;
        var a = incomingDirection.x * incomingDirection.x / (incomingDirection.z * incomingDirection.z)
            + incomingDirection.y * incomingDirection.y / (incomingDirection.z * incomingDirection.z)
            + 1;
        var c = -2 * incomingPoint.z * incomingPoint.x * incomingDirection.x / incomingDirection.z
            - 2 * incomingPoint.z * incomingPoint.y * incomingDirection.y / incomingDirection.z
            + 2 * incomingDirection.x * incomingDirection.x * incomingPoint.z * incomingPoint.z / (incomingDirection.z * incomingDirection.z)
            + 2 * incomingDirection.y * incomingDirection.y * incomingPoint.z * incomingPoint.z / (incomingDirection.z * incomingDirection.z)
            + zp * zp
            - radius * radius
            + incomingPoint.x * incomingPoint.x
            + incomingPoint.y * incomingPoint.y;

        if (b * b - 4 * a * c < 0 )
        {
            throw new ArgumentOutOfRangeException("The given ray does not intercept the lens.");
        }

        var z1 = (-b + Math.Sqrt(b*b - 4*a*c))/2*a;
        var z2 = (-b + Math.Sqrt(b * b - 4 * a * c)) / 2 * a;

        ;

        if( (incomingPoint.z-z1)* (incomingPoint.z - z1) > (incomingPoint.z - z2)* (incomingPoint.z - z2))
        {
            return CalculatePointFromZ( incomingPoint,  incomingDirection, Convert.ToSingle( z2));
        }
        else
        {
            return CalculatePointFromZ(incomingPoint, incomingDirection, Convert.ToSingle( z1));
        }
    }

    private Vector3 CalculateRefractionDirection(Vector3 incomingDirection, Vector3 interceptionPoint, float zp, float totalCoef)
    {
        var normal = new Vector3(2*interceptionPoint.x,2*interceptionPoint.y,2*(interceptionPoint.z-zp)).normalized;

        var cos = Vector3.Dot(normal, incomingDirection);

        if (cos < 0)
        {
            cos = -cos;
            normal = -normal;
        }

        var sin = Math.Abs(Vector3.Cross(normal, incomingDirection).magnitude);
        
        var newSin = sin / totalCoef;

        var tangent = (incomingDirection - Vector3.Project(incomingDirection, normal)).normalized;

        var result = (tangent * Convert.ToSingle(newSin) + normal * Convert.ToSingle(Math.Sqrt(1 - newSin * newSin))).normalized;

        if (result.magnitude != 1)
        {
            throw new Exception("The new Vector is not normalized. Magnitude is :"  + result.magnitude + " .");
        }

        return result;


    }

    private Vector3 CalculatePointFromZ(Vector3 incomingPoint, Vector3 incomingDirection, float z)
    {
        var zTerm = (z - incomingPoint.z) / incomingDirection.z;
        return new Vector3(zTerm*incomingDirection.x + incomingPoint.x, zTerm * incomingDirection.y + incomingPoint.y, z);
    }


}
