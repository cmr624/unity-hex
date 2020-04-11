using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] 
    private HexCell[] neighbors;
    public HexCoordinates coordinates;
    Color color;

    public Color Color
    {
        get { return color; }
        set
        {
            if (color == value)
            {
                return;
            }

            color = value;
            Refresh();
        }
    }
    public RectTransform uiRect;

    public Vector3 Position => transform.localPosition;

    public HexGridChunk chunk;
    //height of cell
    public int Elevation
    {
        get { return elevation; } 
        set
        {
          if (elevation == value)
          {
            return;
          }
          elevation = value;
          
          // hex cell position change
          Vector3 position = transform.localPosition;
          position.y = value * HexMetrics.elevationStep;
          position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
          transform.localPosition = position;
          
          // ui position change
          Vector3 uiPosition = uiRect.localPosition;
          uiPosition.z = -position.y;//elevation * -HexMetrics.elevationStep;
          uiRect.localPosition = uiPosition;
         
          // river refreshing to remove illegal rivers on elevation change
          if (hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation)
          {
              RemoveOutgoingRiver();
          }

          if (hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation)
          {
              RemoveIncomingRiver();
          }
          
          Refresh();
          
        } 
    }
    private int elevation = int.MinValue;

    
    
    
    // rivers!!!!
    public float StreamBedY
    {
        get { return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep; }
    }
    
    
    private bool hasIncomingRiver, hasOutgoingRiver;
    private HexDirection incomingRiver, outgoingRiver;

    public bool HasIncomingRiver
    {
        get => hasIncomingRiver;
    }
    public bool HasOutgoingRiver
    {
        get => hasOutgoingRiver;
    }
    public HexDirection IncomingRiver
    {
        get => incomingRiver;
    }
    public HexDirection OutGoingRiver
    {
        get => outgoingRiver;
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get { return hasIncomingRiver != hasOutgoingRiver; }
    }

    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return hasIncomingRiver && incomingRiver == direction ||
               hasOutgoingRiver && outgoingRiver == direction;
    }


    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }
    
    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
        {
            return;
        }

        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
        {
            return;
        }

        hasOutgoingRiver = false;
        RefreshSelfOnly();
        
        // make sure neighbor doesn't have river anymore from this cell
        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }

        HexCell neighbor = GetNeighbor(direction);
        // rivers cannot flow uphill
        if (!neighbor || elevation < neighbor.elevation)
        {
            return;
        }
        // remove outgoing / incoming river (if it conflicts) 
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }

        hasOutgoingRiver = true;
        outgoingRiver = direction;
        RefreshSelfOnly();
        
        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }
    
    
    
    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }
    
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

    // refreshes all neighbors (used when coloring, to account for blending)
    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }
}
