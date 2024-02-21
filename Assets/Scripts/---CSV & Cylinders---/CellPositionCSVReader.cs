using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CellPositionCSVReader : MonoBehaviour
{
    public int batchSize = 40000; // Editor-changeable variable for batch processing
    public string csvFilePath = "Assets/Resources/CellPosition.csv";

    [System.Serializable]
    public class CSVData
    {
        public int agentID;
        public float bioTicks;
        public float posX, posY, posZ;
        public int interactionType;
        public int otherCellID;
    }

    public List<CSVData> ReadCSVDataInBatches(int startLine, out bool reachedEnd)
    {
        List<CSVData> dataList = new List<CSVData>();
        reachedEnd = false;

        if (File.Exists(csvFilePath))
        {
            string[] lines = File.ReadAllLines(csvFilePath);

            // Calculate the end line for the current batch
            int endLine = Mathf.Min(startLine + batchSize, lines.Length);
            reachedEnd = endLine == lines.Length;

            for (int i = startLine; i < endLine; i++)
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
                        int.TryParse(values[5].Trim(), out data.interactionType) &&
                        int.TryParse(values[6].Trim(), out data.otherCellID))
                    {
                        dataList.Add(data);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + csvFilePath);
            reachedEnd = true; // Ensure reachedEnd is true if file is missing
        }

        return dataList;
    }
}