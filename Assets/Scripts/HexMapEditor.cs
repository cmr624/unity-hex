using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;

    
    private bool applyColor;
    private bool applyElevation = true;
    
    
    private int activeElevation;
    private void Awake()
    {
        SelectColor(0);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            EditCell(hexGrid.GetCell(hit.point));
        }
    }


    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }
    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }

    void EditCell(HexCell cell)
    {
        if (applyColor)
        {
            cell.Color = activeColor;
        }

        if (applyElevation)
        {
            cell.Elevation = activeElevation;
        }
    }

    // slider to select from an elevation range
    // UI slider function - requires a float param
    public void SetElevation(float elevation)
    {
        activeElevation = (int) elevation;
    }
}
