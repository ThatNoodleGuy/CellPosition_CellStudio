using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using UnityEngine;

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
    public float scaleX;
}

[System.Serializable]
public class MoleculeCSVData
{
    public int globalID;
    public float concentration;
    public int moleculeType;
    public int bioTick;
}

public class CSVReader : MonoBehaviour
{
    public static CSVReader instance;

    [Header("File Paths")]
    public string cellPositionCSVFilePath = "Assets/Resources/cellPosition.csv";
    public string moleculeCSVFilePath = "Assets/Resources/MolExpr.csv";

    [Header("Progress Tracking")]
    [SerializeField] private int cellPositionCSVLinesLoaded = 0;
    [SerializeField] private int moleculeCSVLinesLoaded = 0;

    [Header("Performance Settings")]
    public int linesPerBatch = 5000; // Adjust based on performance

    private Dictionary<int, List<CellPositionCSVData>> cellPositionData = new Dictionary<int, List<CellPositionCSVData>>();
    private Dictionary<int, List<MoleculeCSVData>> moleculeData = new Dictionary<int, List<MoleculeCSVData>>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public async Task LoadDataAsync()
    {
        await Task.WhenAll(
            LoadCellPositionDataAsync(),
            LoadMoleculeDataAsync()
        );
        Debug.Log("Data loading complete.");
    }

    private async Task LoadCellPositionDataAsync()
    {
        using (var reader = new StreamReader(cellPositionCSVFilePath))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                cellPositionCSVLinesLoaded++;

                // Skip the header
                if (cellPositionCSVLinesLoaded == 1) continue;

                var data = ParseCellPositionCSVLine(line);
                if (data != null)
                {
                    if (!cellPositionData.ContainsKey(data.agentID))
                        cellPositionData[data.agentID] = new List<CellPositionCSVData>();
                    cellPositionData[data.agentID].Add(data);
                }
            }
        }
        Debug.Log("Finished loading cell position data.");
    }

    private CellPositionCSVData ParseCellPositionCSVLine(string line)
    {
        string[] values = line.Split(',');
        if (values.Length >= 11) // Ensure all expected data is present
        {
            return new CellPositionCSVData
            {
                agentID = int.Parse(values[0]),
                bioTicks = float.Parse(values[1]),
                posX = float.Parse(values[2]),
                posY = float.Parse(values[3]),
                posZ = float.Parse(values[4]),
                interactionType = int.Parse(values[5]),
                otherCellID = int.Parse(values[6]),
                cellType = values[7],
                cylinderInteraction = int.Parse(values[8]),
                cellState = values.Length > 9 ? values[9] : "Unknown", // Optional; check if present
                scaleX = float.Parse(values[10])
            };
        }
        return null; // Line didn't match expected format
    }

    private async Task LoadMoleculeDataAsync()
    {
        using (var reader = new StreamReader(moleculeCSVFilePath))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                moleculeCSVLinesLoaded++;

                // Skip the header
                if (moleculeCSVLinesLoaded == 1) continue;

                var data = ParseMoleculeCSVLine(line);
                if (data != null)
                {
                    if (!moleculeData.ContainsKey(data.globalID))
                        moleculeData[data.globalID] = new List<MoleculeCSVData>();
                    moleculeData[data.globalID].Add(data);
                }
            }
        }
        Debug.Log("Finished loading molecule data.");
    }

    private void ProcessMoleculeDataBatch(List<MoleculeCSVData> batch)
    {
        foreach (var data in batch)
        {
            if (!moleculeData.ContainsKey(data.globalID))
            {
                moleculeData[data.globalID] = new List<MoleculeCSVData>();
            }
            moleculeData[data.globalID].Add(data);
        }
    }

    private MoleculeCSVData ParseMoleculeCSVLine(string line)
    {
        string[] values = line.Split(',');
        if (values.Length == 4) // Ensure all expected data is present
        {
            return new MoleculeCSVData
            {
                globalID = int.Parse(values[0]),
                concentration = float.Parse(values[1]),
                moleculeType = int.Parse(values[2]),
                bioTick = int.Parse(values[3])
            };
        }
        return null; // Line didn't match expected format
    }

    public List<CellPositionCSVData> GetCellDataForAgent(int agentID)
    {
        if (cellPositionData.TryGetValue(agentID, out var dataList))
        {
            return dataList;
        }
        return new List<CellPositionCSVData>();
    }

    public List<MoleculeCSVData> GetMoleculeDataForVoxel(int globalID, int bioTick)
    {
        if (moleculeData.TryGetValue(globalID, out var dataList))
        {
            return dataList.Where(data => data.bioTick == bioTick).ToList();
        }
        return new List<MoleculeCSVData>();
    }

    // Expose Loaded Lines Count for Editor
    public int CellPositionCSVLinesLoaded => cellPositionCSVLinesLoaded;
    public int MoleculeCSVLinesLoaded => moleculeCSVLinesLoaded;

    // Retrieves cell data for a specific agent by ID
    public List<CellPositionCSVData> GetDataForAgent(int agentID)
    {
        return GetCellDataForAgent(agentID); // Utilizes the previously defined method
    }

    // Retrieves molecule data for a specific voxel and bioTick
    public List<MoleculeCSVData> GetDataForVoxel(int globalID, int bioTick)
    {
        return GetMoleculeDataForVoxel(globalID, bioTick); // Utilizes the previously defined method
    }

    // Returns a list of all unique agent IDs
    public List<int> GetAllAgentIDs()
    {
        return cellPositionData.Keys.ToList();
    }

    // Returns a list of all unique voxel IDs
    public List<int> GetAllVoxelsIDs()
    {
        return moleculeData.Keys.ToList();
    }

    // Calculates the maximum bioTick across all cell position data
    public int GetMaxBioTick()
    {
        return cellPositionData.Values.SelectMany(list => list)
                                       .Max(data => (int)data.bioTicks);
    }

    // Accessor for cell position data dictionary
    public Dictionary<int, List<CellPositionCSVData>> CellPositionData => cellPositionData;

    // Attempts to parse a line of molecule data from the CSV, safely
    private bool TryParseMoleculeCSVLine(string[] values, out MoleculeCSVData data)
    {
        data = null;
        if (values.Length == 4 &&
            int.TryParse(values[0].Trim(), out int globalID) &&
            float.TryParse(values[1].Trim(), out float concentration) &&
            int.TryParse(values[2].Trim(), out int moleculeType) &&
            int.TryParse(values[3].Trim(), out int bioTick))
        {
            data = new MoleculeCSVData
            {
                globalID = globalID,
                concentration = concentration,
                moleculeType = moleculeType,
                bioTick = bioTick
            };
            return true;
        }
        return false;
    }

    // Returns all molecule data, flattened from the dictionary
    public List<MoleculeCSVData> GetAllMoleculeData()
    {
        return moleculeData.Values.SelectMany(list => list).ToList();
    }

}