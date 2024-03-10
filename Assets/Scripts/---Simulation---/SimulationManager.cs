using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimulationManager : MonoBehaviour
{
    [Header("Cells")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Material[] interactionMaterials;
    [SerializeField] private List<GameObject> spawnedCells = new List<GameObject>();
    [SerializeField] private Material deadCellMaterial;

    [Header("Voxels")]
    [SerializeField] private GameObject voxelPrefab; // Assign in the Inspector
    [SerializeField] private List<GameObject> spawnedVoxels = new List<GameObject>();
    private Vector3 gridSize;
    private Vector3 voxelDimensions;

    [Header("Timer")]
    [SerializeField] private int currentBioTick = 0;
    [SerializeField] private float timeMultiplier = 1.0f; // Adjust the speed of time in your simulation

    [Header("UI")]
    [SerializeField] private Button spawnCellsButton;
    [SerializeField] private Button startSimulationButton;
    [SerializeField] private Button pauseSimulationButton;
    [SerializeField] private Button resumeSimulationButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI onScreenText;

    [Header("Misc")]
    [SerializeField] private CSVReader csvReader; // Reference to the CSVReader component
    bool isCellDataLoaded = false;
    bool isVoxelDataLoaded = false;

    void HandleDataLoaded()
    {
        // Assuming csvReader is an instance of CSVReader
        StartCoroutine(csvReader.PreloadCellPositionData(() =>
        {
            isCellDataLoaded = true;
            CheckDataLoadingCompletion();
        }));

        StartCoroutine(csvReader.PreloadMoleculeData(() =>
        {
            LoadConfigurationFromXML("Assets/Resources/ExampleReduced_SV.xml");
            GenerateVoxelGrid();
            CheckDataLoadingCompletion();
        }));

        void CheckDataLoadingCompletion()
        {
            if (isCellDataLoaded && isVoxelDataLoaded)
            {
                // Update the UI here based on your logic, for example:
                Debug.Log("All data loaded successfully.");
                onScreenText.text = "Preloading Data Complete. You can now spawn cells or start the simulation.";
                spawnCellsButton.gameObject.SetActive(true);
            }
        }
    }

    void Start()
    {
        onScreenText.text = "Preloading Data, Please Wait...";

        exitButton.onClick.AddListener(ExitSimulation);

        spawnCellsButton.gameObject.SetActive(false);
        startSimulationButton.gameObject.SetActive(false);
        pauseSimulationButton.gameObject.SetActive(false);
        resumeSimulationButton.gameObject.SetActive(false);
        HandleDataLoaded();
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
        startSimulationButton.gameObject.SetActive(false);
        pauseSimulationButton.gameObject.SetActive(true);
        resumeSimulationButton.gameObject.SetActive(false);
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
                    if (cellManager.GetCellState() == "APOPTOSIS") // if cellState is 5 - #"APOPTOSIS"
                    {
                        cellManager.gameObject.GetComponent<MeshRenderer>().material = deadCellMaterial;
                    }
                    else
                    {
                        cellManager.UpdateState(currentBioTick, interactionMaterials);
                    }
                }

                foreach (GameObject voxelObject in spawnedVoxels)
                {
                    VoxelManager voxelManager = voxelObject.GetComponent<VoxelManager>();
                    voxelManager.UpdateVoxelForBioTick(currentBioTick);
                }
            }
        }
    }

    public void PauseSimulation()
    {
        timeMultiplier = 0;
        pauseSimulationButton.gameObject.SetActive(false);
        resumeSimulationButton.gameObject.SetActive(true);
    }

    public void ResumeSimulation()
    {
        timeMultiplier = 1.0f;
        pauseSimulationButton.gameObject.SetActive(true);
        resumeSimulationButton.gameObject.SetActive(false);
    }

    void LoadConfigurationFromXML(string filePath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);

        XmlNode areaSizeNode = xmlDoc.SelectSingleNode("//area_size");
        gridSize = new Vector3(
            float.Parse(areaSizeNode.SelectSingleNode("x").InnerText),
            float.Parse(areaSizeNode.SelectSingleNode("y").InnerText),
            float.Parse(areaSizeNode.SelectSingleNode("z").InnerText));

        XmlNode areaStepNode = xmlDoc.SelectSingleNode("//area_step");
        voxelDimensions = new Vector3(
            float.Parse(areaStepNode.SelectSingleNode("x").InnerText),
            float.Parse(areaStepNode.SelectSingleNode("y").InnerText),
            float.Parse(areaStepNode.SelectSingleNode("z").InnerText));
    }

    void GenerateVoxelGrid()
    {
        int voxelIndex = 0;

        Vector3 gridCubeNumber = new Vector3(
            gridSize.x / voxelDimensions.x,
            gridSize.y / voxelDimensions.y,
            gridSize.z / voxelDimensions.z);

        Vector3 startPosition = transform.position; // Adjust as needed

        for (int z = 0; z < gridCubeNumber.z; z++)
        {
            for (int y = 0; y < gridCubeNumber.y; y++)
            {
                for (int x = 0; x < gridCubeNumber.x; x++)
                {
                    // Calculate the position based on grid coordinates and voxel dimensions
                    Vector3 position = startPosition + new Vector3(x * voxelDimensions.x, y * voxelDimensions.y, z * voxelDimensions.z);

                    List<MoleculeCSVData> voxelDataList = csvReader.GetDataForVoxel(voxelIndex);
                    if (voxelDataList.Count > 0)
                    {
                        // Assuming we take the first entry as the data example
                        MoleculeCSVData data = voxelDataList[0];

                        // Instantiate voxel GameObject and initialize its properties using voxelData
                        GameObject voxelGO = Instantiate(voxelPrefab, position, Quaternion.identity);
                        voxelGO.transform.parent = transform;
                        VoxelManager voxelManager = voxelGO.GetComponent<VoxelManager>();
                        voxelGO.transform.name = "Voxel_" + voxelIndex;
                        spawnedVoxels.Add(voxelGO);

                        if (voxelManager != null)
                        {
                            voxelManager.Initialize(data.globalID, data);

                            voxelManager.UpdateVoxelData(data);
                        }
                    }

                    voxelIndex++;
                }
            }
        }
        isVoxelDataLoaded = true;
    }

    public void ExitSimulation()
    {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false; // Exit play mode in Unity Editor
#else
        Application.Quit(); // Close the app in a standalone build
#endif
    }
}