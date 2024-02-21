using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellBehaviour : MonoBehaviour
{
    public int agentID;
    private List<CellPositionCSVReader.CSVData> cellData = new List<CellPositionCSVReader.CSVData>();
    private int currentDataIndex = 0;

    [SerializeField] private TextMeshPro textMeshPro;

    public void Initialize(int agentID, Material[] interactionMaterials, CellPositionCSVReader.CSVData initialData = null)
    {
        this.agentID = agentID;
        if (initialData != null)
        {
            SetCellProperties(initialData, interactionMaterials);
        }
        // Note: You might still want to log or handle the case where initialData is null
    }

    public void AddData(CellPositionCSVReader.CSVData data)
    {
        cellData.Add(data);
    }

    private void SetCellProperties(CellPositionCSVReader.CSVData data, Material[] interactionMaterials)
    {
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        if (interactionMaterials != null && data.interactionType >= 0 && data.interactionType < interactionMaterials.Length)
        {
            GetComponent<Renderer>().material = interactionMaterials[data.interactionType];
        }

        if (textMeshPro != null)
        {
            textMeshPro.text = $"Cell: {agentID}";
        }
    }

    public void UpdateState(int bioTick, Material[] interactionMaterials)
    {
        if (currentDataIndex < cellData.Count && cellData[currentDataIndex].bioTicks <= bioTick)
        {
            SetCellProperties(cellData[currentDataIndex], interactionMaterials);
            currentDataIndex++;
        }
    }
}