using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
   public Text cellLabelPrefab;
   //private Canvas gridCanvas;
   //private HexMesh hexMesh;

   private int cellCountX, cellCountZ; 
   public HexCell cellPrefab;
   private HexCell[] cells;

   public Color defaultColor = Color.white;
   public Color touchedColor = Color.magenta;

   public Texture2D noiseSource;
   public int chunkCountX = 4, chunkCountZ = 3;

   public HexGridChunk chunkPrefab;
   private void OnEnable()
   {
      HexMetrics.noiseSource = noiseSource;
   }

   private HexGridChunk[] chunks;
   private void Awake()
   {
      HexMetrics.noiseSource = noiseSource;
     // gridCanvas = GetComponentInChildren<Canvas>();
     // hexMesh = GetComponentInChildren<HexMesh>();


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
            chunk.transform.SetParent(transform);
         } 
      }
   }

   public void ShowUI(bool visible)
   {
      for (int i = 0; i < chunks.Length; i++)
      {
         chunks[i].ShowUI(visible);
      }
   }
   
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
   /*void Start()
   {
      hexMesh.Triangulate(cells);
   }*/

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
   

   public HexCell GetCell(Vector3 position)
   {
      position = transform.InverseTransformPoint(position);
      HexCoordinates coordinates = HexCoordinates.FromPosition(position);
      int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
      return cells[index];
   }

   /*
   public void Refresh()
   {
      hexMesh.Triangulate(cells);
   }*/
   
   void CreateCell(int x, int z, int i)
   {
      Vector3 position;
      position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
      position.y = 0f;
      position.z = z * (HexMetrics.outerRadius * 1.5f);
      
      HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
     
      //cell.transform.SetParent(transform, false);
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
         // (bitwise operation for even number) even rows
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
     
      //instantiate text
      Text label = Instantiate(cellLabelPrefab);
      //, gridCanvas.transform, false);
      label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
      label.text = cell.coordinates.ToStringOnSeparateLines();
      cell.uiRect = label.rectTransform;

      //call the cell's elevation "setter"
      cell.Elevation = 0;

      AddCellToChunk(x, z, cell);

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
