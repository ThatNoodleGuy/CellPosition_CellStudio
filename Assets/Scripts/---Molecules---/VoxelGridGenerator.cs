using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using TMPro;

public class VoxelGridGenerator : MonoBehaviour
{
    public static event Action OnDataLoadedComplete;
    public int linesPerFrame = 20000; // Adjust based on performance needs

    class ConcentrationData
    {
        public int VoxelID { get; set; }
        public float Concentration { get; set; }
        public int Biotick { get; set; }
    }

    public GameObject VoxelPrefab; // Assign in inspector
    [SerializeField] private int voxelCounter;
    private Vector3 areaStep;
    private Vector3 areaSize;
    private Dictionary<int, List<ConcentrationData>> concentrationByVoxel = new Dictionary<int, List<ConcentrationData>>();
    private Dictionary<int, GameObject> voxelReferences = new Dictionary<int, GameObject>();
    /*
        void OnEnable()
        {
            SimulationManager.OnBioTickUpdated += UpdateVoxelsForBiotick;
        }

        void OnDisable()
        {
            SimulationManager.OnBioTickUpdated -= UpdateVoxelsForBiotick;
        }
    */
    void Start()
    {
        StartCoroutine(LoadConcentrationData("Assets/Resources/MolExpr.csv"));
        LoadXML();
        CreateGrid();
    }

    IEnumerator LoadConcentrationData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found at path: " + filePath);
            yield break;
        }

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            int lineCount = 0;

            // Skip the header line
            reader.ReadLine();

            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(',');
                if (values.Length >= 4 && int.Parse(values[2]) != 1) // Ensure molecule type is not 1
                {
                    int voxelID = int.Parse(values[0]);
                    float concentration = float.Parse(values[1]);
                    int biotick = int.Parse(values[3]);

                    if (!concentrationByVoxel.ContainsKey(voxelID))
                    {
                        concentrationByVoxel[voxelID] = new List<ConcentrationData>();
                    }

                    concentrationByVoxel[voxelID].Add(new ConcentrationData { Concentration = concentration, Biotick = biotick });
                }

                if (++lineCount % linesPerFrame == 0)
                {
                    yield return null; // Yield execution to keep the UI responsive
                }
            }
        }

        OnDataLoadedComplete?.Invoke();
    }

    void LoadXML()
    {
        TextAsset xmlAsset = Resources.Load<TextAsset>("ExampleReduced_SV");
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlAsset.text); // Load the XML from the text asset

        XmlNode areaStepNode = xmlDoc.SelectSingleNode("//Environments/array_element_0/area_step");
        XmlNode areaSizeNode = xmlDoc.SelectSingleNode("//Environments/array_element_0/area_size");

        areaStep = new Vector3(
            float.Parse(areaStepNode.SelectSingleNode("x").InnerText),
            float.Parse(areaStepNode.SelectSingleNode("y").InnerText),
            float.Parse(areaStepNode.SelectSingleNode("z").InnerText));

        areaSize = new Vector3(
            float.Parse(areaSizeNode.SelectSingleNode("x").InnerText),
            float.Parse(areaSizeNode.SelectSingleNode("y").InnerText),
            float.Parse(areaSizeNode.SelectSingleNode("z").InnerText));
    }

    void CreateGrid()
    {
        voxelCounter = 0;

        for (float z = 0; z < areaSize.z; z += areaStep.z)
        {
            for (float y = 0; y < areaSize.y; y += areaStep.y)
            {
                for (float x = 0; x < areaSize.x; x += areaStep.x)
                {
                    GameObject voxel = Instantiate(VoxelPrefab, new Vector3(x, y, z), Quaternion.identity, this.transform);
                    voxel.transform.localScale = new Vector3(areaStep.x, areaStep.y, areaStep.z);
                    voxel.name = "Voxel_" + voxelCounter;
                    voxelReferences.Add(voxelCounter, voxel);
                    voxelCounter++;
                }
            }
        }
    }

    void UpdateVoxelsForBiotick(int biotick)
    {
        foreach (var voxelData in concentrationByVoxel)
        {
            var voxelId = voxelData.Key;
            if (voxelReferences.TryGetValue(voxelId, out GameObject voxel))
            {
                var dataList = voxelData.Value;
                var data = dataList.Find(d => d.Biotick == biotick);
                if (data != null)
                {
                    float alphaValue = Mathf.Clamp(data.Concentration / 1000f, 0.0f, 1f);
                    Material voxelMaterial = voxel.GetComponent<Renderer>().material;
                    Color color = voxelMaterial.color;
                    color.a = alphaValue;
                    voxelMaterial.color = color;
                }
            }
        }
    }
}