using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
   private HexCell[] cells;
   private HexMesh hexMesh;
   private Canvas gridCanvas;

   void Awake()
   {
      gridCanvas = GetComponentInChildren<Canvas>();
      hexMesh = GetComponentInChildren<HexMesh>();

      cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
   }

  /**
   * void Start()
   {
      hexMesh.Triangulate(cells);
   }
   */

   public void AddCell(int index, HexCell cell)
   {
      cells[index] = cell;
      cell.chunk = this;
      cell.transform.SetParent(transform, false);
      cell.uiRect.SetParent(gridCanvas.transform, false);
   }

   public void Refresh()
   {
      //   hexMesh.Triangulate(cells);
      // because chunk doesn't do anything on enable / disable, we use this as a flag for when we need to triangulate
      enabled = true;
   }

   private void LateUpdate()
   {
      hexMesh.Triangulate(cells);
      enabled = false;
   }
}
