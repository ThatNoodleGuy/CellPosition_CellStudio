using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CellPositionCellManager : MonoBehaviour
{
    [Header("Cells")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Material[] interactionMaterials;

    [Header("Timer")]
    [SerializeField] private int currentBioTick = 0;
    [SerializeField] private float timeMultiplier = 1.0f; // Adjust the speed of time in your simulation
    [SerializeField] private TextMeshProUGUI bioticksTimerText;

    [Header("Misc")]
    [SerializeField] private CellPositionCSVReader csvReader; // Reference to the CSVReader component
    [SerializeField] private Button spawnCellsButton;
    [SerializeField] private Button startSimulationButton; // Assign in the Unity Inspector

    private List<GameObject> spawnedCells = new List<GameObject>();


    void Start()
    {
        spawnCellsButton.gameObject.SetActive(false);
        startSimulationButton.gameObject.SetActive(false); // Ensure the start button is also hidden initially
        StartCoroutine(csvReader.PreloadAllData(() =>
        {
            spawnCellsButton.gameObject.SetActive(true);
        }));
    }

    public void OnSpawnCellsButtonClicked()
    {
        spawnCellsButton.gameObject.SetActive(false); // Hide the spawn button
        StartCoroutine(PreloadDataAndInitializeCells());
        // Don't start the CheckForUpdates coroutine here
    }

    public void OnStartSimulationButtonClicked()
    {
        StartCoroutine(CheckForUpdates());
        startSimulationButton.gameObject.SetActive(false); // Optionally hide the start button after clicking
    }

    IEnumerator PreloadDataAndInitializeCells()
    {
        var agentIDs = csvReader.GetAllAgentIDs();
        int cellsPerBatch = 10; // Adjust based on performance

        for (int i = 0; i < agentIDs.Count; i++)
        {
            int agentID = agentIDs[i];
            GameObject cellObject = FindOrCreateCell(agentID);
            CellBehaviour cellBehaviour = cellObject.GetComponent<CellBehaviour>();
            var dataTimeline = csvReader.GetDataForAgent(agentID);
            if (dataTimeline.Count > 0)
            {
                // Pass the first data entry as the initial state
                cellBehaviour.Initialize(agentID, interactionMaterials, dataTimeline[0]);
            }

            foreach (var dataEntry in dataTimeline)
            {
                cellBehaviour.AddData(dataEntry);
            }

            if ((i + 1) % cellsPerBatch == 0 || i == agentIDs.Count - 1)
            {
                yield return null; // Yield after a batch is processed or at the end
            }
        }

        // After all cells are initialized
        startSimulationButton.gameObject.SetActive(true);
    }

    GameObject FindOrCreateCell(int agentID)
    {
        // Check if the cell already exists
        foreach (GameObject cell in spawnedCells)
        {
            if (cell.GetComponent<CellBehaviour>().agentID == agentID)
            {
                return cell;
            }
        }

        // If not, create a new cell
        GameObject newCell = Instantiate(cellPrefab, transform);
        newCell.GetComponent<CellBehaviour>().agentID = agentID;
        spawnedCells.Add(newCell);
        return newCell;
    }

    IEnumerator CheckForUpdates()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f / timeMultiplier);
            currentBioTick++;
            bioticksTimerText.text = "BioTick: " + currentBioTick;

            foreach (GameObject cellObject in spawnedCells)
            {
                CellBehaviour cellBehaviour = cellObject.GetComponent<CellBehaviour>();
                cellBehaviour.UpdateState(currentBioTick, interactionMaterials);
            }
        }
    }

    void Update()
    {
        bioticksTimerText.text = "BioTick: " + currentBioTick;
    }
}