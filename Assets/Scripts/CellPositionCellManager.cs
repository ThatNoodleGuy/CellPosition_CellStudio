using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class CellPositionCellManager : MonoBehaviour
{
    [Header("Cells")]
    public GameObject cellPrefab; // Reference to your cell prefab
    public Material[] interactionMaterials;

    [Header("Timer")]
    public int currentBioTick = 0;
    public float timeMultiplier = 1.0f;
    public int timeUpdateCheck = 50;
    public TextMeshProUGUI bioticksTimerText;

    private List<CellPositionCSVReader.CSVData> dataList = new List<CellPositionCSVReader.CSVData>();
    private string csvFilePath = "Assets/Resources/Data/CellPosition.csv";
    private List<GameObject> spawnedCells = new List<GameObject>();

    void Start()
    {
        LoadCSVData();
        SpawnInitialCells(); // Initial spawn
        StartCoroutine(CheckForUpdates());
    }

    void Update()
    {
        bioticksTimerText.text = "BioTick: " + currentBioTick;
    }

    private void LoadCSVData()
    {
        if (File.Exists(csvFilePath))
        {
            string csvText = File.ReadAllText(csvFilePath);

            CellPositionCSVReader.ParseCSVData(csvText, dataList);
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + csvFilePath);
        }
    }

    void SpawnInitialCells()
    {
        // Create and position cells for bioTick = 0
        List<CellPositionCSVReader.CSVData> initialData = dataList.FindAll(data => data.bioTicks == 0);

        foreach (CellPositionCSVReader.CSVData data in initialData)
        {
            GameObject cell = Instantiate(cellPrefab);
            Vector3 cellPosition = new Vector3(data.posX, data.posY, data.posZ);
            cell.transform.position = cellPosition;
            cell.transform.parent = transform;
            cell.name = "Cell_" + data.agentID;

            TextMeshPro textMeshPro = cell.GetComponentInChildren<TextMeshPro>();

            if (textMeshPro != null)
            {
                textMeshPro.text = "Cell: " + data.agentID;
            }
            else
            {
                Debug.LogWarning("TextMeshPro component not found in child object.");
            }

            // Set the material based on interaction type
            Renderer cellRenderer = cell.GetComponent<Renderer>();
            if (cellRenderer != null)
            {
                // Ensure that the interaction type is a valid index in the interactionMaterials array
                if (data.interactionType >= 0 && data.interactionType < interactionMaterials.Length)
                {
                    Material matchingMaterial = interactionMaterials[data.interactionType];

                    if (matchingMaterial != null)
                    {
                        cellRenderer.material = matchingMaterial;
                    }
                    else
                    {
                        Debug.LogWarning("No matching material found for interaction type: " + data.interactionType);
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid interaction type index: " + data.interactionType);
                }
            }
            else
            {
                Debug.LogWarning("Renderer component not found in child object.");
            }

            // Add the spawned cell to the list
            spawnedCells.Add(cell);
        }
    }

    IEnumerator CheckForUpdates()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f / timeMultiplier);

            // Increment the bioTick counter
            currentBioTick++;

            // Check if it's time to update (every X bioTicks)
            if (currentBioTick % timeUpdateCheck == 0)
            {
                // Load the updated CSV data
                LoadCSVData();

                // Update cell positions for the current bioTick
                UpdateCellPositions(currentBioTick);
            }
        }
    }

    void UpdateCellPositions(int bioTick)
    {
        // Update the positions and materials of existing cells based on the current bioTick
        foreach (GameObject cell in spawnedCells)
        {
            int agentID = int.Parse(cell.GetComponentInChildren<TextMeshPro>().text.Replace("Cell: ", ""));
            CellPositionCSVReader.CSVData data = dataList.Find(d => d.agentID == agentID && d.bioTicks == bioTick);

            if (data != null)
            {
                Vector3 cellPosition = new Vector3(data.posX, data.posY, data.posZ);
                cell.transform.position = cellPosition;

                // Ensure that the interaction type is a valid index in the interactionMaterials array
                if (data.interactionType >= 0 && data.interactionType < interactionMaterials.Length)
                {
                    Material material = interactionMaterials[data.interactionType];
                    Renderer cellRenderer = cell.GetComponent<Renderer>();
                    if (cellRenderer != null)
                    {
                        cellRenderer.material = material;
                    }
                    else
                    {
                        Debug.LogWarning("Renderer component not found in child object.");
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid interaction type index: " + data.interactionType);
                }
            }
        }
    }
}