using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CellPositionCSVReader : MonoBehaviour
{
    private List<CSVData> dataList = new List<CSVData>();

    public string csvFilePath = "Assets/Resources/Data/CellPosition.csv"; // Define the full file path here

    [System.Serializable]
    public class CSVData
    {
        public int agentID; // Consider using short if values are within -32768 to 32767
        public float bioTicks; // Keep as float if you need to store fractional values
        public float posX, posY, posZ; // Position values typically need to be floats
        public int interactionType; // Could also be short or byte, depending on the range
        public int otherCellID; // Consider using short if applicable
    }

    void Start()
    {
        // Check if the file exists
        if (File.Exists(csvFilePath))
        {
            string[] lines = File.ReadAllLines(csvFilePath);

            // Skip the first line (header) by starting the loop at index 1
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] values = line.Split(',');

                if (values.Length >= 7)
                {
                    CSVData data = new CSVData();
                    if (int.TryParse(values[0].Trim(), out data.agentID) &&
                        float.TryParse(values[1].Trim(), out data.bioTicks) &&
                        float.TryParse(values[2].Trim(), out data.posX) &&
                        float.TryParse(values[3].Trim(), out data.posY) &&
                        float.TryParse(values[4].Trim(), out data.posZ) &&
                        int.TryParse(values[6].Trim(), out data.otherCellID))
                    {
                        data.interactionType = int.Parse(values[5].Trim());
                        dataList.Add(data);
                    }
                    else
                    {
                        Debug.LogError("Error parsing values from line: " + line);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + csvFilePath);
        }
    }

    public static void ParseCSVData(string csvText, List<CSVData> dataList)
    {
        // Split the CSV text into lines
        string[] lines = csvText.Split('\n');

        // Loop through each line (skip the header line if present)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Split the line into values
            string[] values = line.Split(',');

            if (values.Length < 7) continue; // Ensure you have at least 7 columns

            // Create a CSVData object and populate it
            CSVData data = new CSVData();
            data.agentID = int.Parse(values[0]);
            data.bioTicks = int.Parse(values[1]);
            data.posX = float.Parse(values[2]);
            data.posY = float.Parse(values[3]);
            data.posZ = float.Parse(values[4]);
            data.interactionType = int.Parse(values[5].Trim());
            data.otherCellID = int.Parse(values[6]);

            // Add the data to the dataList
            dataList.Add(data);
        }
    }
}