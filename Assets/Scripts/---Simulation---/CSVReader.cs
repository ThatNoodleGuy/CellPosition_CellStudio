using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CSVReader : MonoBehaviour
{
    public string csvFilePath = "Assets/Resources/CellPosition.csv";

    public int linesPerFrame = 5000; // Adjust based on performance

    [System.Serializable]
    public class CSVData
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

    private int linesLoaded = 0;

    // Add a dictionary to hold preloaded data organized by agentID
    private Dictionary<int, List<CSVData>> preloadedData = new Dictionary<int, List<CSVData>>();

    public IEnumerator PreloadAllData(Action onCompleted = null)
    {
        linesLoaded = 0;

        if (!File.Exists(csvFilePath))
        {
            Debug.LogError("CSV file not found at path: " + csvFilePath);
            yield break;
        }

        using (StreamReader reader = new StreamReader(csvFilePath))
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
                    CSVData data = new CSVData();
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

                        if (!preloadedData.ContainsKey(data.agentID))
                        {
                            preloadedData[data.agentID] = new List<CSVData>();
                        }
                        preloadedData[data.agentID].Add(data);
                    }
                }

                if (lineCount % linesPerFrame == 0)
                {
                    // Optionally, update a progress bar or log progress
                    yield return null; // Yield execution to keep the UI responsive
                }

                linesLoaded++;
            }
            Debug.Log($"Total lines loaded: {linesLoaded}");

            onCompleted?.Invoke(); // Invoke the completion callback
        }
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