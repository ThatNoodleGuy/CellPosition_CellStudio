using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class CellPositionCellManager : MonoBehaviour
{
    [Header("Cells")]
    public GameObject cellPrefab;
    public Material[] interactionMaterials;

    [Header("Timer")]
    public int currentBioTick = 0;
    public float timeMultiplier = 1.0f;
    public int timeUpdateCheck = 50;
    public TextMeshProUGUI bioticksTimerText;

    [Header("Misc")]
    public CellPositionCSVReader csvReader; // Reference to the CSVReader component

    private List<CellPositionCSVReader.CSVData> dataList = new List<CellPositionCSVReader.CSVData>();
    private List<GameObject> spawnedCells = new List<GameObject>();

    private int currentBatchStartLine = 1; // Start from line 1 to skip header
    private bool dataLoadingComplete = false;

    void Start()
    {
        LoadCSVDataBatch();
        SpawnInitialCells();
        StartCoroutine(CheckForUpdates());
    }

    void Update()
    {
        bioticksTimerText.text = "BioTick: " + currentBioTick;
    }

    void LoadCSVDataBatch()
    {
        if (!dataLoadingComplete)
        {
            bool reachedEnd;
            List<CellPositionCSVReader.CSVData> batchData = csvReader.ReadCSVDataInBatches(currentBatchStartLine, out reachedEnd);

            if (batchData.Count > 0)
            {
                dataList.AddRange(batchData);
                currentBatchStartLine += batchData.Count;
            }

            if (reachedEnd)
            {
                dataLoadingComplete = true;
            }
        }
    }

    void SpawnInitialCells()
    {
        List<CellPositionCSVReader.CSVData> initialData = dataList.FindAll(data => data.bioTicks == 0);

        foreach (CellPositionCSVReader.CSVData data in initialData)
        {
            GameObject cellObject = Instantiate(cellPrefab, new Vector3(data.posX, data.posY, data.posZ), Quaternion.identity, transform);
            cellObject.name = "Cell_" + data.agentID;

            CellBehaviour cellBehaviour = cellObject.GetComponent<CellBehaviour>();
            if (cellBehaviour != null)
            {
                cellBehaviour.Initialize(data, interactionMaterials);
            }
            else
            {
                Debug.LogWarning("CellBehaviour component not found on cell prefab.");
            }

            // Add the spawned cell to the list
            spawnedCells.Add(cellObject);
        }
    }

    IEnumerator CheckForUpdates()
    {
        bool isDataAvailable = true; // Flag to check if data is still available

        while (isDataAvailable)
        {
            yield return new WaitForSeconds(1.0f / timeMultiplier);

            // Increment the bioTick counter
            currentBioTick++;

            // Check if it's time to update (every X bioTicks)
            if (currentBioTick % timeUpdateCheck == 0)
            {
                // Load the updated CSV data
                LoadCSVDataBatch();

                // Check if there are updates available for the current bioTick
                List<CellPositionCSVReader.CSVData> currentData = dataList.FindAll(data => data.bioTicks == currentBioTick);
                if (currentData.Count > 0)
                {
                    // Update cell positions for the current bioTick
                    UpdateCellPositions(currentBioTick);
                }
                else
                {
                    // If no data is found for the current bioTick, it means we've reached the end of the CSV data
                    isDataAvailable = false;
                    bioticksTimerText.text = "End of Simulation Data.";
                }
            }
        }
        bioticksTimerText.text = "End of Simulation Data.";
    }

    void UpdateCellPositions(int bioTick)
    {
        foreach (GameObject cellObject in spawnedCells)
        {
            CellBehaviour cellBehaviour = cellObject.GetComponent<CellBehaviour>();
            if (cellBehaviour != null)
            {
                // Find data for the current bioTick and the specific agentID.
                CellPositionCSVReader.CSVData data = dataList.Find(d => d.agentID == cellBehaviour.AgentID && d.bioTicks == bioTick);
                if (data != null)
                {
                    // Update the cell's position and potentially other properties
                    cellObject.transform.position = new Vector3(data.posX, data.posY, data.posZ);

                    // Update material or other properties as needed
                    UpdateCellMaterial(cellBehaviour, data);
                }
            }
        }
    }

    void UpdateCellMaterial(CellBehaviour cellBehaviour, CellPositionCSVReader.CSVData data)
    {
        if (data.interactionType >= 0 && data.interactionType < interactionMaterials.Length)
        {
            Renderer cellRenderer = cellBehaviour.GetComponent<Renderer>();
            if (cellRenderer != null)
            {
                cellRenderer.material = interactionMaterials[data.interactionType];
            }
        }
        else
        {
            Debug.LogWarning($"Invalid interaction type index: {data.interactionType} for cell {cellBehaviour.AgentID}");
        }
    }
}