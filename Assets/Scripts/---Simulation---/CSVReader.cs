using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("CellPosition CSV")]
    public string cellPositionCSVFilePath = "Assets/Resources/cellPosition.csv";
    public int cellPositionCSVLinesPerFrame = 5000; // Adjust based on performance
    [SerializeField] private int cellPositionCSVLinesLoaded = 0;
    private Dictionary<int, List<CellPositionCSVData>> cellPositionData = new Dictionary<int, List<CellPositionCSVData>>();

    [Header("Molecule CSV")]
    public string moleculeCSVFilePath = "Assets/Resources/MolExpr.csv";
    public int moleculeCSVLinesPerFrame = 5000; // Adjust based on performance
    [SerializeField] private int moleculeCSVLinesLoaded = 0;
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

            Debug.Log($"Total CellPosition CSV lines loaded: {cellPositionCSVLinesLoaded}");
            onCompleted?.Invoke(); // Invoke the completion callback
        }
    }

    public IEnumerator PreloadMoleculeData(Action onCompleted = null)
    {
        moleculeCSVLinesLoaded = 0; // Reset count at the start

        if (!File.Exists(moleculeCSVFilePath))
        {
            Debug.LogError("Molecule CSV file not found at path: " + moleculeCSVFilePath);
            yield break;
        }

        using (StreamReader reader = new StreamReader(moleculeCSVFilePath))
        {
            string line;
            int lineCount = 0;
            List<MoleculeCSVData> batch = new List<MoleculeCSVData>(); // Prepare a batch list

            while ((line = reader.ReadLine()) != null)
            {
                if (lineCount++ == 0) continue; // Skip the header line

                string[] values = line.Split(',');
                if (values.Length >= 4)
                {
                    if (TryParseMoleculeCSVLine(values, out MoleculeCSVData data))
                    {
                        // Add to batch instead of immediately to the dictionary
                        batch.Add(data);
                    }
                }

                // Process in batches
                if (lineCount % moleculeCSVLinesPerFrame == 0)
                {
                    ProcessMoleculeDataBatch(batch); // Process the current batch
                    batch.Clear(); // Clear the batch for the next round
                    yield return null; // Yield execution to keep the UI responsive
                }

                moleculeCSVLinesLoaded++;
            }

            // Process any remaining data in the batch
            if (batch.Count > 0)
            {
                ProcessMoleculeDataBatch(batch);
                batch.Clear();
            }

            Debug.Log($"Total MoleExpre CSV lines loaded: {moleculeCSVLinesLoaded}");
            onCompleted?.Invoke();
        }
    }

    public List<CellPositionCSVData> GetDataForAgent(int agentID)
    {
        if (cellPositionData.ContainsKey(agentID))
        {
            return cellPositionData[agentID];
        }
        return new List<CellPositionCSVData>();
    }

    public List<MoleculeCSVData> GetDataForVoxel(int globalID, int bioTick)
    {
        // Assuming moleculeData stores lists of MoleculeCSVData indexed by globalID
        if (moleculeData.TryGetValue(globalID, out List<MoleculeCSVData> allData))
        {
            // Filter the data for the specific bioTick
            return allData.Where(data => data.bioTick == bioTick).ToList();
        }
        return new List<MoleculeCSVData>();
    }

    public List<int> GetAllAgentIDs()
    {
        return cellPositionData.Keys.ToList();
    }

    public List<int> GetAllVoxelsIDs()
    {
        return moleculeData.Keys.ToList();
    }

    public int GetMaxBioTick()
    {
        int maxBioTick = 0;
        foreach (var list in cellPositionData.Values)
        {
            foreach (var data in list)
            {
                if (data.bioTicks > maxBioTick)
                    maxBioTick = (int)data.bioTicks;
            }
        }
        return maxBioTick;
    }

    public Dictionary<int, List<CellPositionCSVData>> CellPositionData
    {
        get { return cellPositionData; }
    }

    private bool TryParseMoleculeCSVLine(string[] values, out MoleculeCSVData data)
    {
        data = new MoleculeCSVData();
        return int.TryParse(values[0].Trim(), out data.globalID) &&
               float.TryParse(values[1].Trim(), out data.concentration) &&
               int.TryParse(values[2].Trim(), out data.moleculeType) &&
               int.TryParse(values[3].Trim(), out data.bioTick);
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
}