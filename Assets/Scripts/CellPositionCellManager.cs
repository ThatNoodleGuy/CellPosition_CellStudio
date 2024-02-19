using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellPositionCellManager : MonoBehaviour
{
    public CellDataManager cellDataManager; // Reference to the CellDataManager script
    public GameObject cellPrefab; // Reference to your cell prefab
    public Material[] interactionMaterials; // Array of materials for different interaction types
    public TextMeshProUGUI bioticksTimerText; // UI Text to display current bioTick

    private int currentBioTick = 0;
    public float timeMultiplier = 1.0f;
    public int timeUpdateCheck = 50; // Interval to check for updates
    private List<GameObject> spawnedCells = new List<GameObject>();

    void Start()
    {
        SpawnInitialCells(); // Initial spawn
        StartCoroutine(CheckForUpdates());
    }

    void Update()
    {
        bioticksTimerText.text = "BioTick: " + currentBioTick;
    }

    private void SpawnInitialCells()
    {
        if (cellDataManager.dataByBioTick.TryGetValue(0, out List<CellDataManager.CSVData> initialData))
        {
            foreach (var data in initialData)
            {
                SpawnCell(data);
            }
        }
    }

    private GameObject SpawnCell(CellDataManager.CSVData data)
    {
        GameObject cell = Instantiate(cellPrefab, new Vector3(data.posX, data.posY, data.posZ), Quaternion.identity);
        cell.transform.parent = transform;
        cell.name = "Cell_" + data.agentID;

        TextMeshPro textMesh = cell.GetComponentInChildren<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = "Cell: " + data.agentID;
        }
        else
        {
            Debug.LogWarning("TextMeshPro component not found in the cell prefab.");
        }

        UpdateCellMaterial(cell, data.interactionType);
        spawnedCells.Add(cell);
        return cell;
    }

    IEnumerator CheckForUpdates()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f / timeMultiplier);
            currentBioTick++;
            if (currentBioTick % timeUpdateCheck == 0)
            {
                UpdateCellPositions(currentBioTick);
            }
        }
    }

    private void UpdateCellPositions(int bioTick)
    {
        if (cellDataManager.dataByBioTick.TryGetValue(bioTick, out List<CellDataManager.CSVData> cellsForBioTick))
        {
            foreach (var cellData in cellsForBioTick)
            {
                GameObject cellGameObject = spawnedCells.Find(cell => cell.name == "Cell_" + cellData.agentID);
                if (cellGameObject != null)
                {
                    cellGameObject.transform.position = new Vector3(cellData.posX, cellData.posY, cellData.posZ);
                    UpdateCellMaterial(cellGameObject, cellData.interactionType);
                }
            }
        }
    }

    private void UpdateCellMaterial(GameObject cell, int interactionType)
    {
        Renderer cellRenderer = cell.GetComponent<Renderer>();
        if (cellRenderer != null && interactionType >= 0 && interactionType < interactionMaterials.Length)
        {
            cellRenderer.material = interactionMaterials[interactionType];
        }
    }
}