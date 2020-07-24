using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//[ExecuteInEditMode]
public class BeamGeneratorScript : MonoBehaviour
{
    private List<DiscreteBeam> _descreteBeams = new List<DiscreteBeam>();
    private int resolution = 10;

    public GameObject laser;
    public double intensity;
    public Guid Id;


    // Start is called before the first frame update
    void Start()
    {
        Id = Guid.NewGuid();
        GameObject.Find("PropagationSystem").GetComponent<PropagationSystem>().AddLaser(laser);
        DescreateBeamCreator();
        GameObject.Find("PropagationSystem").GetComponent<PropagationSystem>().AddNewBeams(_descreteBeams);
        
    }

    private void DescreateBeamCreator()
    {
        var radius = laser.transform.Find("BeamOrientationController").transform.localScale.x * 10;
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
                    positions.Add(new Vector3(delta * i, 0, delta * j));
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

        Debug.Log("Created beams are : " + positions.Count);

        for (int i = 0; i < positions.Count; i++)
        {
            ;
            _descreteBeams.Add(new DiscreteBeam(Id, ParentType.Laser , laser.transform.Find("BeamOrientationController").transform.TransformPoint( positions[i]), laser.transform.Find("BeamOrientationController").transform.TransformDirection(-Vector3.up), endingPositions[i], intensity, 10, 0));
        }
    }


    void OnApplicationQuit()
    {
        //DeleteAllDiscreteBeams();
    }

    private void DeleteAllDiscreteBeams()
    {
        foreach( Transform child in laser.transform.Find("ProjectorGlass").transform)
        {
            Destroy(child.gameObject);
        }
    }

    
}
