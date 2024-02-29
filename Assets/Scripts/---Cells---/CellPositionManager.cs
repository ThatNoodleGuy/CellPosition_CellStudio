using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CellPositionManager : MonoBehaviour
{
    [Header("Cells")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Material[] interactionMaterials;
    [SerializeField] private List<GameObject> spawnedCells = new List<GameObject>();
    [SerializeField] private Material deadCellMaterial;

    [Header("Timer")]
    [SerializeField] private int currentBioTick = 0;
    [SerializeField] private float timeMultiplier = 1.0f; // Adjust the speed of time in your simulation

    [Header("Misc")]
    [SerializeField] private CSVReader csvReader; // Reference to the CSVReader component
    [SerializeField] private Button spawnCellsButton;
    [SerializeField] private Button startSimulationButton; // Assign in the Unity Inspector
    [SerializeField] private TextMeshProUGUI onScreenText;

    void Start()
    {
        onScreenText.text = "Preloading Data, Please Wait...";

        spawnCellsButton.gameObject.SetActive(false);
        startSimulationButton.gameObject.SetActive(false); // Ensure the start button is also hidden initially
        StartCoroutine(csvReader.PreloadAllData(() =>
        {
            onScreenText.text = "Preloading Data Complete, Please Spawn Cells In...";
            spawnCellsButton.gameObject.SetActive(true);
        }));
    }

    public void OnSpawnCellsButtonClicked()
    {
        spawnCellsButton.gameObject.SetActive(false); // Hide the spawn button
        onScreenText.text = "Please Wait...";
        StartCoroutine(PreloadDataAndInitializeCells());
    }

    public void OnStartSimulationButtonClicked()
    {
        StartCoroutine(CheckForUpdates());
        startSimulationButton.gameObject.SetActive(false); // Optionally hide the start button after clicking
    }

    IEnumerator PreloadDataAndInitializeCells()
    {
        var agentIDs = csvReader.GetAllAgentIDs();
        int cellsPerBatch = 1000; // Adjust based on performance

        for (int i = 0; i < agentIDs.Count; i++)
        {
            int agentID = agentIDs[i];
            GameObject cellObject = FindOrCreateCell(agentID);
            CellManager cell = cellObject.GetComponent<CellManager>();
            var dataTimeline = csvReader.GetDataForAgent(agentID);
            if (dataTimeline.Count > 0)
            {
                // Pass the first data entry as the initial state
                cell.Initialize(agentID, interactionMaterials, dataTimeline[0]);
            }

            foreach (var dataEntry in dataTimeline)
            {
                cell.AddData(dataEntry);
            }

            if ((i + 1) % cellsPerBatch == 0 || i == agentIDs.Count - 1)
            {
                yield return null; // Yield after a batch is processed or at the end
            }
        }

        // After all cells are initialized
        onScreenText.text = "Simulation Is Ready, Press Start";
        startSimulationButton.gameObject.SetActive(true);
    }

    GameObject FindOrCreateCell(int agentID)
    {
        // Check if the cell already exists
        foreach (GameObject cell in spawnedCells)
        {
            if (cell.GetComponent<CellManager>().agentID == agentID)
            {
                return cell;
            }
        }

        // If not, create a new cell
        GameObject newCell = Instantiate(cellPrefab, transform);
        newCell.GetComponent<CellManager>().agentID = agentID;
        spawnedCells.Add(newCell);
        return newCell;
    }

    IEnumerator CheckForUpdates()
    {
        float timeAccumulator = 0f; // Accumulate time here

        while (true)
        {
            // Wait for the next frame
            yield return null;

            // Accumulate delta time multiplied by the timeMultiplier
            timeAccumulator += Time.deltaTime * timeMultiplier;

            // Check if at least one whole bio tick has passed
            while (timeAccumulator >= 1.0f)
            {
                currentBioTick++;
                timeAccumulator -= 1.0f; // Decrease accumulator by one tick, handling multiple ticks if necessary

                onScreenText.text = "BioTick: " + currentBioTick;

                // Update each cell for the currentBioTick
                foreach (GameObject cellObject in spawnedCells)
                {
                    CellManager cellManager = cellObject.GetComponent<CellManager>();
                    if (cellManager.GetCellLifeState() == 1)
                    {
                        cellManager.UpdateState(currentBioTick, interactionMaterials);
                    }
                    else
                    {
                        cellManager.gameObject.GetComponent<MeshRenderer>().material = deadCellMaterial;
                    }
                }
            }
        }
    }
}