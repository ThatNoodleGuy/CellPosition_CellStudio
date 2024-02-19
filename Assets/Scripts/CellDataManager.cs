using System.Collections.Generic;
using UnityEngine;

public class CellDataManager : MonoBehaviour
{
    public Dictionary<int, List<CSVData>> dataByBioTick = new Dictionary<int, List<CSVData>>();
    public string csvResourcePath = "Data/CellPosition";

    [System.Serializable]
    public class CSVData
    {
        public int agentID;
        public float bioTicks;
        public float posX;
        public float posY;
        public float posZ;
        public int interactionType;
        public int otherCellID;
    }

    void Start()
    {
        LoadAndParseCSVData(csvResourcePath);
    }

    private void LoadAndParseCSVData(string resourcePath)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);
        if (csvFile != null)
        {
            string csvText = csvFile.text;
            ParseCSVDataAndOrganize(csvText);
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + resourcePath);
        }
    }

    public void ParseCSVDataAndOrganize(string csvText)
    {
        string[] lines = csvText.Split('\n');
        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 7) continue;

            CSVData data = new CSVData()
            {
                agentID = int.Parse(values[0]),
                bioTicks = float.Parse(values[1]),
                posX = float.Parse(values[2]),
                posY = float.Parse(values[3]),
                posZ = float.Parse(values[4]),
                interactionType = int.Parse(values[5].Trim()),
                otherCellID = int.Parse(values[6])
            };

            int bioTickKey = Mathf.FloorToInt(data.bioTicks);
            if (!dataByBioTick.ContainsKey(bioTickKey))
            {
                dataByBioTick[bioTickKey] = new List<CSVData>();
            }
            dataByBioTick[bioTickKey].Add(data);
        }
    }
}