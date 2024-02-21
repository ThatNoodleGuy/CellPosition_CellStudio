using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellManager : MonoBehaviour
{
    public int agentID;
    private List<CSVReader.CSVData> cellData = new List<CSVReader.CSVData>();
    private int currentDataIndex = 0;

    public void Initialize(int agentID, Material[] interactionMaterials, CSVReader.CSVData initialData = null)
    {
        this.agentID = agentID;
        if (initialData != null)
        {
            SetCellProperties(initialData, interactionMaterials);
        }
        // Note: You might still want to log or handle the case where initialData is null
    }

    public void AddData(CSVReader.CSVData data)
    {
        cellData.Add(data);
    }

    private void SetCellProperties(CSVReader.CSVData data, Material[] interactionMaterials)
    {
        gameObject.name = "Cell_" + data.agentID;
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        if (interactionMaterials != null && data.interactionType >= 0 && data.interactionType < interactionMaterials.Length)
        {
            GetComponent<Renderer>().material = interactionMaterials[data.interactionType];
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