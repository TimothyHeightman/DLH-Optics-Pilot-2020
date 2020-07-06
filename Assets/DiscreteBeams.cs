using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DescreteBeams
{

    public List<GameObject> Rays { get; private set; }
    public List<Vector3> Directions { get; private set; }
    public List<double> InitialPhase { get; private set; }
    public List<double> Frequency { get; private set;}
    public List<double> Intensity { get; private set; }


    public DescreteBeams(List<GameObject> rays, double intensity, double frequency, double initialPhase)
    {
        if(frequency<0){
            throw new System.ArgumentOutOfRangeException("The given frequency is negative.");
        }
        if(intensity<0){
            throw new System.ArgumentOutOfRangeException("The given intensity is negative.");
        }
        if(rays.Contains(null)){
            throw new System.ArgumentNullException("Atleast one of the given GameObjects is null");
        }

        Rays = rays;
        InitialPhase = new List<double>(rays.Count);
        
        for(int i=0;i<InitialPhase.Count;i++){
            InitialPhase[i] = initialPhase%(2*Mathf.PI);
        }

        Intensity = new List<double>(rays.Count);
        
        for(int i=0;i<Intensity.Count;i++){
            Intensity[i] = intensity;
        }

        Frequency = new List<double>(rays.Count);
        
        for(int i=0;i<Frequency.Count;i++){
            Frequency[i] = frequency;
        }
    }

    public DescreteBeams(List<GameObject> rays, double intensity , double frequency, List<double> initialPhase ){
        if(frequency<=0){
            throw new System.ArgumentOutOfRangeException("The given frequency is negative or zero.");
        }
        if(intensity<0){
            throw new System.ArgumentOutOfRangeException("The given intensity is negative.");
        }
        if(rays.Contains(null)){
            throw new System.ArgumentNullException("Atleast one of the given GameObjects is null");
        }
        if(rays.Count != initialPhase.Count){
            throw new System.Exception("The different list have different counts.");
        }

        Rays = rays;
        InitialPhase = new List<double>(rays.Count);
        
        InitialPhase = initialPhase.Select(p => p%2*Mathf.PI).ToList();

        Intensity = new List<double>(rays.Count);
        
        for(int i=0;i<Intensity.Count;i++){
            Intensity[i] = intensity;
        }

        Frequency = new List<double>(rays.Count);
        
        for(int i=0;i<Frequency.Count;i++){
            Frequency[i] = frequency;
        }
    }

    public DescreteBeams(List<GameObject> rays, double intensity, List<double> frequency, List<double> initialPhase){
        if(frequency.TrueForAll(f=>f>0)){
            throw new System.ArgumentOutOfRangeException("Atleast one of the given frequencies is negative or zero.");
        }
        if(intensity<0){
            throw new System.ArgumentOutOfRangeException("The given intensity is negative.");
        }
        if(rays.Contains(null)){
            throw new System.ArgumentNullException("Atleast one of the given GameObjects is null");
        }
        if(rays.Count != initialPhase.Count | initialPhase.Count!=frequency.Count){
            throw new System.Exception("The different list have different counts.");
        }

        Rays = rays;
        InitialPhase = new List<double>(rays.Count);
        
        InitialPhase = initialPhase.Select(p => p%2*Mathf.PI).ToList();

        Intensity = new List<double>(rays.Count);
        
        for(int i=0;i<Intensity.Count;i++){
            Intensity[i] = intensity;
        }

        Frequency = frequency;
    }

    public DescreteBeams(List<GameObject> rays, List<double> intensity, List<double> frequency, List<double> initialPhase){
        if(frequency.TrueForAll(f=>f>0)){
            throw new System.ArgumentOutOfRangeException("Atleast one of the given frequencies is negative or zero.");
        }
        if(intensity.TrueForAll(i=>i>=0)){
            throw new System.ArgumentOutOfRangeException("Atleast one of the given intensities is negative.");
        }
        if(rays.Contains(null)){
            throw new System.ArgumentNullException("Atleast one of the given GameObjects is null");
        }
        if(rays.Count != InitialPhase.Count | initialPhase.Count!=frequency.Count | initialPhase.Count!=intensity.Count){
            throw new System.Exception("The different list have different counts.");
        }

        Rays = rays;
        InitialPhase = new List<double>(rays.Count);
        
        InitialPhase = initialPhase.Select(p => p%2*Mathf.PI).ToList();

        Intensity = intensity;

        Frequency = frequency;
    }

}
