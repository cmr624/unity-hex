using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
   public HexCell cellPrefab;
   public HexGridChunk chunkPrefab;
   public int chunkCountX = 4, chunkCountZ = 3;
   
   public Text cellLabelPrefab;
   public Color defaultColor = Color.white;
   public Color touchedColor = Color.magenta;
   
   public Texture2D noiseSource;
   
   private HexCell[] cells;
   private HexGridChunk[] chunks;
   private int cellCountX, cellCountZ; 

   // TODO unclear why we do this OnEnable and Awake
   private void OnEnable()
   {
      HexMetrics.noiseSource = noiseSource;
   }

   private void Awake()
   {
      HexMetrics.noiseSource = noiseSource;
      cellCountX = chunkCountX * HexMetrics.chunkSizeX;
      cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
      CreateChunks();
      CreateCells();
   }

   void CreateChunks()
   {
      chunks = new HexGridChunk[chunkCountX * chunkCountZ];
      
      for (int z = 0, i = 0; z < chunkCountZ; z++)
      {
         for (int x = 0; x < chunkCountX; x++)
         {
            HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
            // make chunk hex grid child 
            chunk.transform.SetParent(transform);
         } 
      }
   }

   // public for editor to toggle UI for all chunks
   // ui function
   public void ShowUI(bool visible)
   {
      for (int i = 0; i < chunks.Length; i++)
      {
         chunks[i].ShowUI(visible);
      }
   }
  // get cell by position in world space
   public HexCell GetCell(Vector3 position)
   {
      //transforms world position to local position
      position = transform.InverseTransformPoint(position);
      // get cell from coordinates
      HexCoordinates coordinates = HexCoordinates.FromPosition(position);
      int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
      return cells[index];
   }
   // finds any cell in the entire grid, not within chunk
   public HexCell GetCell(HexCoordinates coordinates)
   {
      int z = coordinates.Z;
      if (z < 0 || z >= cellCountZ)
      {
         return null;
      }
      int x = coordinates.X + z / 2;
      if (x < 0 || x >= cellCountX)
      {
         return null;
      }
      return cells[x + z * cellCountX];
   }

   void CreateCells()
   {
       cells = new HexCell[cellCountZ * cellCountX];
       for (int z = 0, i = 0; z < cellCountZ; z++)
       {
           for (int x = 0; x < cellCountX; x++)
           {
              CreateCell(x, z, i++);    
           }
       }
   }
   
   void CreateCell(int x, int z, int i)
   {
      Vector3 position;
      position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
      position.y = 0f;
      position.z = z * (HexMetrics.outerRadius * 1.5f);
      
      // create the cell and assign it our calculated position 
      HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
      cell.transform.localPosition = position;
      cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
      cell.Color = defaultColor;
      if (x > 0)
      {
        //set all W -> E neighbors on every row
        cell.SetNeighbor(HexDirection.W, cells[i - 1]); 
      }

      if (z > 0)
      {
         // bitwise operation for even number rows
         if ((z & 1) == 0)
         {
           cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
           if (x > 0)
           {
              cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
           }
         }
         //odd rows
         else
         {
            cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
            if (x < cellCountX - 1)
            {
              cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]); 
            }
         }
         
      }
     
      //instantiate text for label
      InstantiateLabel(cell, position); 

      //call the cell's elevation "setter"
      cell.Elevation = 0;

      AddCellToChunk(x, z, cell);
   }

   void InstantiateLabel(HexCell cell, Vector3 position)
   {
      Text label = Instantiate(cellLabelPrefab);
      label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
      label.text = cell.coordinates.ToStringOnSeparateLines();
      cell.uiRect = label.rectTransform;
   }

   void AddCellToChunk(int x, int z, HexCell cell)
   {
      int chunkX = x / HexMetrics.chunkSizeX;
      int chunkZ = z / HexMetrics.chunkSizeZ;
      HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

      int localX = x - chunkX * HexMetrics.chunkSizeX;
      int localZ = z - chunkZ * HexMetrics.chunkSizeZ;

      chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
   }
}
