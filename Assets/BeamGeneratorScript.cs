using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[ExecuteInEditMode]
public class BeamGeneratorScript : MonoBehaviour
{
    private List<DiscreteBeam> _descreteBeams = new List<DiscreteBeam>();
    private int resolution = 100;

    public GameObject laser;
    public double intensity;
    public GameObject prefabLine;
    
    


    // Start is called before the first frame update
    void Start()
    {
        DescreateBeamCreator();

        foreach(var beam in _descreteBeams)
        {
            beam.Propagate();
        }

        
    }


    private void DescreateBeamCreator()
    {
        var radius = laser.transform.Find("Projector").transform.localScale.x / 4;
        float delta = Convert.ToSingle(radius / (resolution + 0.5));

        var positions = new List<Vector3>();
        var endingPositions = new List<bool>();

        for (int i = -resolution; i <= resolution; i++)
        {
            var first = true;
            for (int j = -resolution; j <= resolution; j++)
            {
                if ((i * i + j * j) * delta * delta < radius * radius)
                {
                    positions.Add(new Vector3(delta * i, delta * j, 0));
                    if (i*i == resolution*resolution | j*j == resolution * resolution)
                    {
                        endingPositions.Add(true);
                    }
                    else
                    {
                        endingPositions.Add(first);
                    }
                    first = false;
                }
                else if (j > 0)
                {
                    endingPositions[endingPositions.Count - 1] = true;
                    break;
                }
            }
        }
        ;
        for(int i = 0; i < positions.Count; i++)
        {
            if (endingPositions[i]) {
                _descreteBeams.Add(new DiscreteBeam(laser, prefabLine,positions[i] , intensity, 10, 0));
            }
            else
            {
                _descreteBeams.Add(new DiscreteBeam(laser, positions[i], intensity, 10, 0));
            }
        }
    }

    void OnApplicationQuit()
    {
        DeleteAllDiscreteBeams();
    }

    private void DeleteAllDiscreteBeams()
    {
        foreach( Transform child in laser.transform.Find("ProjectorGlass").transform)
        {
            Destroy(child.gameObject);
        }
    }

    
}


/*
private void SingleRayPropagator(DescreteBeam descreteBeam, GameObject beamLine)
{
    RaycastHit hit;
    Ray ray = new Ray();
    LineRenderer beamLineRenderer = beamLine.GetComponent<LineRenderer>();
    ray.origin = beamLineRenderer.GetPosition(0) + beamLine.transform.position;

    ray.direction = beamLine.transform.rotation*(beamLineRenderer.GetPosition(1) 
        - beamLineRenderer.GetPosition(0)).normalized;

    if (Physics.Raycast(  ray , out hit))
    {
        if(hit.collider.GetComponent<TypeHolder>().Type == "Mirror")
        {
            ReflectingSingleRays(descreteBeam, beamLine, hit);
        }
    }
}

private void ReflectingSingleRays(DescreteBeam descreteBeam, GameObject beamLine, RaycastHit hit)
{
    var lineRenderer = beamLine.GetComponent<LineRenderer>();
    var positionCountBuffer = lineRenderer.positionCount;
    lineRenderer.SetPosition(positionCountBuffer - 1, beamLine.transform.InverseTransformPoint(hit.point));
    var incommingDirectionRealWolrld = beamLine.transform.rotation*
        (lineRenderer.GetPosition(positionCountBuffer- 1) 
        - lineRenderer.GetPosition(positionCountBuffer - 2));

    lineRenderer.positionCount += 1;

    var cos = Vector3.Dot(incommingDirectionRealWolrld.normalized, hit.normal.normalized);

    var a = hit.point + incommingDirectionRealWolrld
        - cos * incommingDirectionRealWolrld.magnitude * hit.normal.normalized * 2;

    lineRenderer.SetPosition(positionCountBuffer, beamLine.transform.InverseTransformPoint( a) );
    ;
}

private void DescreateBeamCreator()
{
    var radius = laser.transform.Find("Projector").transform.localScale.x/4;
    float delta = Convert.ToSingle( radius / (resolution + 0.5));

    for(int i=-resolution;i<= resolution; i++)
    {
        for(int j = -resolution; j <= resolution; j++)
        {
            if( (i*i + j*j)*delta*delta<radius*radius)
            {
                var buf = laser.transform.Find("ProjectorGlass").transform;
                _descreteBeams.Add(new DescreteBeam(new Vector3(i * delta, j * delta, 0), 0, intensity));
                _beamLines.Add(Instantiate(prefabLine, buf, false));
                _beamLines[_beamLines.Count - 1].transform.localPosition = _descreteBeams[_descreteBeams.Count - 1].Positions[0];

            }
            else if (j > 0)
            {
                break;
            }
        }
    }
}
*/
