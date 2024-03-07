using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CSVReader : MonoBehaviour
{
    [Header("CellPosition CSV")]
    public string cellPositionCSVFilePath = "Assets/Resources/CellPosition.csv";
    public int cellPositionCSVLinesPerFrame = 5000; // Adjust based on performance
    [SerializeField] private int cellPositionCSVLinesLoaded = 0;
    private Dictionary<int, List<CellPositionCSVData>> cellPositionData = new Dictionary<int, List<CellPositionCSVData>>();

    [Header("Molecule CSV")]
    public string moleculeCSVFilePath = "Assets/Resources/MolExpr.csv";
    public int moleculeCSVLinesPerFrame = 5000; // Adjust based on performance
    [SerializeField] private int moleculeCSVLinesLoaded = 0;
    private Dictionary<int, VoxelData> voxelDataDictionary = new Dictionary<int, VoxelData>();

    [System.Serializable]
    public class CellPositionCSVData
    {
        public int agentID;
        public float bioTicks;
        public float posX, posY, posZ;
        public int interactionType;
        public int otherCellID;
        public string cellType;
        public int cylinderInteraction;
        public string cellState;
    }

    [System.Serializable]
    public class VoxelData
    {
        public int voxelID;
        public float concentration;
        public int moleculeType;
        public int bioTick;
    }

    public IEnumerator PreloadCellPositionData(Action onCompleted = null)
    {
        cellPositionCSVLinesLoaded = 0;

        if (!File.Exists(cellPositionCSVFilePath))
        {
            Debug.LogError("CSV file not found at path: " + cellPositionCSVFilePath);
            yield break;
        }

        using (StreamReader reader = new StreamReader(cellPositionCSVFilePath))
        {
            string line;
            int lineCount = 0;

            while ((line = reader.ReadLine()) != null)
            {
                // Skip the header line
                if (lineCount++ == 0) continue;

                string[] values = line.Split(',');
                if (values.Length >= 10)
                {
                    CellPositionCSVData data = new CellPositionCSVData();
                    if (int.TryParse(values[0].Trim(), out data.agentID) &&
                        float.TryParse(values[1].Trim(), out data.bioTicks) &&
                        float.TryParse(values[2].Trim(), out data.posX) &&
                        float.TryParse(values[3].Trim(), out data.posY) &&
                        float.TryParse(values[4].Trim(), out data.posZ) &&
                        int.TryParse(values[5].Trim(), out data.interactionType) &&
                        int.TryParse(values[6].Trim(), out data.otherCellID) &&
                        int.TryParse(values[8].Trim(), out data.cylinderInteraction))
                    {
                        // Direct assignment for strings, no need for TryParse
                        data.cellType = values[7].Trim();
                        data.cellState = values[9].Trim();

                        if (!cellPositionData.ContainsKey(data.agentID))
                        {
                            cellPositionData[data.agentID] = new List<CellPositionCSVData>();
                        }
                        cellPositionData[data.agentID].Add(data);
                    }
                }

                if (lineCount % cellPositionCSVLinesPerFrame == 0)
                {
                    // Optionally, update a progress bar or log progress
                    yield return null; // Yield execution to keep the UI responsive
                }

                cellPositionCSVLinesLoaded++;
            }

            Debug.Log($"Total lines loaded: {cellPositionCSVLinesLoaded}");
            onCompleted?.Invoke(); // Invoke the completion callback
        }
    }

    public IEnumerator PreloadVoxelData(string voxelCsvFilePath, Action onCompleted = null)
    {
        moleculeCSVLinesLoaded = 0; // Reset count at the start

        if (!File.Exists(voxelCsvFilePath))
        {
            Debug.LogError("Voxel CSV file not found at path: " + voxelCsvFilePath);
            yield break;
        }

        using (StreamReader reader = new StreamReader(voxelCsvFilePath))
        {
            string line;
            int lineCount = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (lineCount++ == 0) continue; // Skip the header line

                string[] values = line.Split(',');
                if (values.Length >= 4)
                {
                    int voxelID = int.Parse(values[0].Trim());
                    float concentration = float.Parse(values[1].Trim());
                    int moleculeType = int.Parse(values[2].Trim());
                    int bioTick = int.Parse(values[3].Trim());

                    // Create a new VoxelData object and fill it with the parsed data
                    VoxelData voxelData = new VoxelData
                    {
                        voxelID = voxelID,
                        concentration = concentration,
                        moleculeType = moleculeType,
                        bioTick = bioTick
                    };

                    // Update the voxel data structure, e.g., a dictionary
                    voxelDataDictionary[voxelID] = voxelData;
                    moleculeCSVLinesLoaded++; // Increment the loaded line counter
                }

                if (lineCount % moleculeCSVLinesPerFrame == 0)
                {
                    yield return null; // Yield execution to keep the UI responsive
                }
            }
        }

        Debug.Log($"Total voxel lines loaded: {moleculeCSVLinesLoaded}");
        onCompleted?.Invoke(); // Invoke the completion callback
    }
    
    // Method to get preloaded data for a specific agentID
    public List<CellPositionCSVData> GetDataForAgent(int agentID)
    {
        if (cellPositionData.ContainsKey(agentID))
        {
            return cellPositionData[agentID];
        }
        return new List<CellPositionCSVData>();
    }

    public List<int> GetAllAgentIDs()
    {
        // Utilize LINQ to extract all unique agent IDs from the preloaded data
        return cellPositionData.Keys.ToList();
    }

    // Method to get preloaded voxel data for a specific VoxelID
    public List<VoxelData> GetVoxelDataForVoxelID(int voxelID)
    {
        if (voxelDataDictionary.ContainsKey(voxelID))
        {
            return new List<VoxelData> { voxelDataDictionary[voxelID] };
        }
        return new List<VoxelData>();
    }

    public List<int> GetAllVoxelIDs()
    {
        // Utilize LINQ to extract all unique voxel IDs from the preloaded data
        return voxelDataDictionary.Keys.ToList();
    }
}