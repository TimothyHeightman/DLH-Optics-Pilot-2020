using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//[ExecuteInEditMode]
public class BeamGeneratorScript : MonoBehaviour
{
    //the initial set ot discreteBeams
    private List<DiscreteBeam> _descreteBeams = new List<DiscreteBeam>();
    //represents the amount of beams to be generated. It is the number of beams between the center beam and the side. 0 corresponds to one beam at the middle.
    private int resolution = 100;

    //The laser to generete the beams from
    public GameObject laser;
    //intensity of all beams
    public double intensity;
    //Id of the laser
    public Guid Id;


    // Start is called before the first frame update
    void Start()
    {

        Id = Guid.NewGuid();
        // add the laser to the propagation system
        GameObject.Find("PropagationSystem").GetComponent<PropagationSystem>().AddLaser(laser);
        DescreateBeamCreator();
        //pass the generated beams
        GameObject.Find("PropagationSystem").GetComponent<PropagationSystem>().AddNewBeams(_descreteBeams);
        
    }

    /// <summary>
    /// Creates all the beams generated from the laser depending on initial properties
    /// </summary>
    private void DescreateBeamCreator()
    {
        //calculates the radius from the size of the orientation gameobject part of the laser
        var radius = laser.transform.Find("BeamOrientationController").transform.localScale.x * 10;
        //delta is the distance between two beems in horizontal or vertical direction
        float delta = Convert.ToSingle(radius / (resolution + 0.5));

        // initial position for all beams
        var positions = new List<Vector3>();
        // represents if beam is in the outer layer of beams to visualise it
        var endingPositions = new List<bool>();

        
        // generate the positions and find if they are at the end
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

    // this was used to delete all visualisations of the beams, old version
    private void DeleteAllDiscreteBeams()
    {
        foreach( Transform child in laser.transform.Find("ProjectorGlass").transform)
        {
            Destroy(child.gameObject);
        }
    }

    
}
