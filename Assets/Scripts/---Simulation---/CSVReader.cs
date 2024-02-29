using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CSVReader : MonoBehaviour
{
    public string csvFilePath = "Assets/Resources/CellPosition.csv";

    [System.Serializable]
    public class CSVData
    {
        public int agentID;
        public float bioTicks;
        public float posX, posY, posZ;
        public int interactionType;
        public int otherCellID;
        public string cellType;
        public int cellLifeState;
        public int cylinderInteraction;
    }

    // Add a dictionary to hold preloaded data organized by agentID
    private Dictionary<int, List<CSVData>> preloadedData = new Dictionary<int, List<CSVData>>();

    public IEnumerator PreloadAllData(Action onCompleted = null)
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError("CSV file not found at path: " + csvFilePath);
            yield break;
        }

        string[] lines = File.ReadAllLines(csvFilePath);
        int linesPerFrame = 5000; // Adjust based on performance

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] values = line.Split(',');
            if (values.Length >= 10)
            {
                CSVData data = new CSVData();
                if (int.TryParse(values[0].Trim(), out data.agentID) &&
                    float.TryParse(values[1].Trim(), out data.bioTicks) &&
                    float.TryParse(values[2].Trim(), out data.posX) &&
                    float.TryParse(values[3].Trim(), out data.posY) &&
                    float.TryParse(values[4].Trim(), out data.posZ) &&
                    int.TryParse(values[5].Trim(), out data.interactionType) &&
                    int.TryParse(values[6].Trim(), out data.otherCellID) &&
                    int.TryParse(values[8].Trim(), out data.cellLifeState) &&
                    int.TryParse(values[9].Trim(), out data.cylinderInteraction))
                {
                    // Direct assignment for strings, no need for TryParse
                    data.cellType = values[7].Trim();

                    if (!preloadedData.ContainsKey(data.agentID))
                    {
                        preloadedData[data.agentID] = new List<CSVData>();
                    }
                    preloadedData[data.agentID].Add(data);
                }
            }

            if (i % linesPerFrame == 0)
            {
                // Optionally, update a progress bar or log progress
                yield return null; // Yield execution to keep the UI responsive
            }
        }

        onCompleted?.Invoke(); // Invoke the completion callback
    }

    // Method to get preloaded data for a specific agentID
    public List<CSVData> GetDataForAgent(int agentID)
    {
        if (preloadedData.ContainsKey(agentID))
        {
            return preloadedData[agentID];
        }
        return new List<CSVData>();
    }

    public List<int> GetAllAgentIDs()
    {
        // Utilize LINQ to extract all unique agent IDs from the preloaded data
        return preloadedData.Keys.ToList();
    }
}