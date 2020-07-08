using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
            if(radius1+radius2<= thinkness+ Math.Sqrt(-length * 2 + radius1*2) + Math.Sqrt(-length * 2 + radius2 * 2))
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
        var incomingPoint = ReverseScalingOnInverse( LensObject.transform.InverseTransformPoint(descreteBeam.LastCoordinate), LensObject.transform.localScale);
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
            var secondInterceptionPoint = CalculatePointOfIntersection(firstInterceptionPoint, firstRefractedDirection, -radius2, zb);
            var secondRefractionDirection = CalculateRefractionDirection(firstRefractedDirection, secondInterceptionPoint, Convert.ToSingle(zb), Convert.ToSingle(1 / coef));

            ;

            return new List<Tuple<List<Vector3>, Vector3>>()
            {
                new Tuple<List<Vector3>, Vector3>
                (
                    new List<Vector3>()
                    {
                        TransformPoint(firstInterceptionPoint,LensObject.transform.localScale),
                        TransformPoint( secondInterceptionPoint,LensObject.transform.localScale)
                    },LensObject.transform.TransformDirection( secondRefractionDirection)
                )
            };
        }
        else
        {
            ;
            var firstInterceptionPoint = CalculatePointOfIntersection(incomingPoint, incomingDirection, radius2, zb);
            var firstRefractedDirection = CalculateRefractionDirection(incomingDirection, firstInterceptionPoint, Convert.ToSingle(zb), Convert.ToSingle(coef));
            var secondInterceptionPoint = CalculatePointOfIntersection(firstInterceptionPoint, firstRefractedDirection, -radius1, zf);
            var secondRefractionDirection = CalculateRefractionDirection(firstRefractedDirection, secondInterceptionPoint, Convert.ToSingle(zf), Convert.ToSingle(1 / coef));
            ;
            var a = TransformPoint(firstInterceptionPoint, LensObject.transform.localScale);
            var b = TransformPoint(secondInterceptionPoint, LensObject.transform.localScale);

            ;

            return new List<Tuple<List<Vector3>, Vector3>>()
            {
                new Tuple<List<Vector3>, Vector3>
                (
                    new List<Vector3>()
                    {
                        TransformPoint(firstInterceptionPoint,LensObject.transform.localScale),
                        TransformPoint( secondInterceptionPoint,LensObject.transform.localScale)
                    },LensObject.transform.TransformDirection( secondRefractionDirection)
                )
            };
            
        }
    }

    private Vector3 CalculatePointOfIntersection(Vector3 incomingPoint, Vector3 incomingDirection, double radius, double zp)
    {
        
        var b = 2 * incomingPoint.x * incomingDirection.x
            + 2 * incomingPoint.y * incomingDirection.y
            + 2 * incomingPoint.z * incomingDirection.z
            - 2* zp * incomingDirection.z;
        var a = incomingDirection.x * incomingDirection.x
            + incomingDirection.y * incomingDirection.y 
            + incomingDirection.z * incomingDirection.z;
        var c = 
            + zp * zp
            - radius * radius
            + incomingPoint.x * incomingPoint.x
            + incomingPoint.y * incomingPoint.y
            + incomingPoint.z * incomingPoint.z
            -2*zp*incomingPoint.z;

        if (b * b - 4 * a * c < 0 )
        {
            throw new ArgumentOutOfRangeException("The given ray does not intercept the lens.");
        }

        var t1= Convert.ToSingle((-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a));
        var t2 = Convert.ToSingle((-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a));

        var point1 = CalculatePointFromT(incomingPoint, incomingDirection, Convert.ToSingle(t1));
        var point2 = CalculatePointFromT(incomingPoint, incomingDirection, Convert.ToSingle(t2));
        ;
        if( (point1 - incomingDirection).normalized != (point1 - incomingDirection).normalized)
        {
            throw new Exception("Calculated values for interception point do not lie on the same line.");
        }

        if ((point1 - incomingPoint).normalized == incomingDirection)
        {
            if ((point2 - incomingPoint).normalized == incomingDirection)
            {
                if (radius < 0)
                {
                    if ((point1 - incomingPoint).magnitude < (point2 - incomingPoint).magnitude)
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
                    if ((point1 - incomingPoint).magnitude > (point2 - incomingPoint).magnitude)
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
            if ((point2 - incomingPoint).normalized == incomingDirection)
            {
                return point2;
            }
            else
            {
                throw new ArgumentException("Neither of the calculated intersection points satisfy the direction of propagation.");
            }
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
        ;
        if (newSin > 1)
        {
            throw new ArgumentOutOfRangeException("The beam propagating through the lens is experiencing total internal reflection;");
        }

        var tangent = (incomingDirection - Vector3.Project(incomingDirection, normal)).normalized;

        var a = tangent * Convert.ToSingle(newSin);
        var b = normal * Convert.ToSingle(Math.Sqrt(1 - newSin * newSin));

        var result = (tangent * Convert.ToSingle(newSin) + normal * Convert.ToSingle(Math.Sqrt(1 - newSin * newSin))).normalized;
        ;
        

        return result;


    }

    private Vector3 CalculatePointFromT(Vector3 incomingPoint, Vector3 incomingDirection, float t)
    {
        return new Vector3(t*incomingDirection.x + incomingPoint.x, t * incomingDirection.y + incomingPoint.y, t*incomingDirection.z + incomingPoint.z);
    }

    private Vector3 ReverseScalingOnInverse(Vector3 position, Vector3 scale)
    {
        return new Vector3(position.x*scale.x, position.y * scale.y, position.z * scale.z);
    }

    private Vector3 TransformPoint(Vector3 position, Vector3 scale)
    {
        return LensObject.transform.TransformPoint(new Vector3(position.x / scale.x, position.y / scale.y, position.z / scale.z));
    }
}
