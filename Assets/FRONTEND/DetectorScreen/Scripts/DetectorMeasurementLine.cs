using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

/*Class that forms the custom data structure for each line created.

Locations stored are relative to global transform of the GameObject given by the relevant tag,
thereby these are local positions and are rotation independent.

Tags are the names of the GameObjects to which the lines were attached.

Layer will only be used for Desktop, allows multiple layers of lines to be stored.

This class is instantiated as elements of a list held by the measurement control GameObject/ associated script.
*/
public class DetectorMeasurementLine
{   // Class to store details of line object
    public Vector3 StartLocation { get; set; }
    public Vector3 EndLocation { get; set; }
    public string StartTag { get; set; }
    public string EndTag { get; set; }
    public int Layer { get; set; }

    public DetectorMeasurementLine(Vector3 startLocation, Vector3 endLocation, string startTag, string endTag, int layer)
    {
        this.StartLocation = startLocation;
        this.EndLocation = endLocation;
        this.StartTag = startTag;
        this.EndTag = endTag;
        this.Layer = layer;
    }

}
