using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] 
    private HexCell[] neighbors;
    public HexCoordinates coordinates;
    public Color color;

    public RectTransform uiRect;

    public Vector3 Position => transform.localPosition;
    
    //height of cell
    public int Elevation
    {
        get { return elevation; } 
        set
        {
          elevation = value;
          
          // hex cell position change
          Vector3 position = transform.localPosition;
          position.y = value * HexMetrics.elevationStep;
          position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
          transform.localPosition = position;
          
          // ui position change
          Vector3 uiPosition = uiRect.localPosition;
          uiPosition.z = -position.y//elevation * -HexMetrics.elevationStep;
          uiRect.localPosition = uiPosition;
        } 
    }

    private int elevation;

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int) direction].elevation);
    }

    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }
    
    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int) direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int) direction] = cell;
        cell.neighbors[(int) direction.Opposite()] = this;
    }
}
