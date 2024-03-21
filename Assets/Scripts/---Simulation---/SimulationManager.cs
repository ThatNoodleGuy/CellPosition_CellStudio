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

[System.Serializable]
public class CellTypeMaterialSet
{
    public string cellType;
    public Material[] materials; // 0 - NO_HIT, 1 - WALL_HIT, 2 - CYLINDER_HIT, 3 - CELL_HIT, 4 - CELL_DEAD
}

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager instance;

    [Header("Cells")]
    [SerializeField] private List<CellTypeMaterialSet> cellTypeMaterialSets;
    [SerializeField] private GameObject cellPrefab;
    private Dictionary<string, Material[]> cellMaterialsMap = new Dictionary<string, Material[]>();
    [SerializeField] private List<GameObject> spawnedCells = new List<GameObject>();

    [Header("Voxels")]
    [SerializeField] private GameObject voxelPrefab; // Assign in the Inspector
    private Vector3 gridSize;
    private Vector3 voxelDimensions;
    [SerializeField] private List<GameObject> spawnedVoxels = new List<GameObject>();

    [Header("Timer")]
    [SerializeField] private int currentBioTick = 0;
    [SerializeField] private float timeMultiplier = 1.0f; // Adjust the speed of time in your simulation
    private int bioTickStep = 50;

    [Header("UI")]
    [SerializeField] private Button spawnCellsButton;
    [SerializeField] private Button startSimulationButton;
    [SerializeField] private Button pauseSimulationButton;
    [SerializeField] private Button resumeSimulationButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI onScreenText;
    [SerializeField] private TextMeshProUGUI bioTickDisplay;
    [SerializeField] private Slider timeSlider;

    [Header("Misc")]
    [SerializeField] private CSVReader csvReader; // Reference to the CSVReader component
    private int simulationDuration;
    bool isCellDataLoaded = false;
    bool isVoxelDataLoaded = false;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        foreach (var set in cellTypeMaterialSets)
        {
            cellMaterialsMap[set.cellType] = set.materials;
        }

        timeSlider.gameObject.SetActive(false);
    }


    private void HandleDataLoaded()
    {
        // Assuming csvReader is an instance of CSVReader
        StartCoroutine(csvReader.PreloadCellPositionData(() =>
        {
            isCellDataLoaded = true;
            //isVoxelDataLoaded = true;
            CheckDataLoadingCompletion();
        }));

        StartCoroutine(csvReader.PreloadMoleculeData(() =>
        {

            GenerateVoxelGrid();
            CheckDataLoadingCompletion();
        }));

        void CheckDataLoadingCompletion()
        {
            if (isCellDataLoaded && isVoxelDataLoaded)
            {
                // Update the UI here based on your logic, for example:
                Debug.Log("All data loaded successfully.");
                onScreenText.text = "Preloading Data Complete. You can now spawn cells.";
                spawnCellsButton.gameObject.SetActive(true);
            }
        }
    }


    private void Start()
    {
        cellMaterialsMap = new Dictionary<string, Material[]>();
        foreach (var set in cellTypeMaterialSets)
        {
            cellMaterialsMap[set.cellType] = set.materials;
        }

        onScreenText.text = "Preloading Data, Please Wait...";

        LoadConfigurationFromXML("Assets/Resources/E0.xml");

        exitButton.onClick.AddListener(ExitSimulation);

        spawnCellsButton.gameObject.SetActive(false);
        startSimulationButton.gameObject.SetActive(false);
        pauseSimulationButton.gameObject.SetActive(false);
        resumeSimulationButton.gameObject.SetActive(false);
        timeSlider.gameObject.SetActive(false);
        HandleDataLoaded();
    }


    private void InitializeTimeSlider()
    {
        bioTickDisplay.text = "BioTick: " + currentBioTick + "/" + simulationDuration;

        // Calculate the number of steps based on the simulation duration and bioTickStep
        int maxSteps = simulationDuration / bioTickStep;
        timeSlider.maxValue = maxSteps;

        // Set the slider's current value based on the current bio tick and bioTickStep
        timeSlider.value = currentBioTick / bioTickStep;
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
        onScreenText.gameObject.SetActive(false);
        timeSlider.gameObject.SetActive(true);

        timeSlider.onValueChanged.AddListener(HandleSliderValueChanged);
        InitializeTimeSlider();
    }


    private IEnumerator PreloadDataAndInitializeCells()
    {
        var agentIDs = csvReader.GetAllAgentIDs();
        int cellsPerBatch = 1000; // Adjust based on performance

        for (int i = 0; i < agentIDs.Count; i++)
        {
            int agentID = agentIDs[i];
            var dataTimeline = csvReader.GetDataForAgent(agentID);
            if (dataTimeline.Count > 0)
            {
                GameObject cellObject = FindOrCreateCell(agentID, dataTimeline[0].cellType);
                CellManager cell = cellObject.GetComponent<CellManager>();

                // Find the correct material set based on the cell's type
                Material[] materialsForType = GetMaterialsForCellType(cell.cellType);

                // Initialize the CellManager with the specific materials
                cell.Initialize(agentID, dataTimeline[0].cellType, dataTimeline[0]);

                foreach (var dataEntry in dataTimeline)
                {
                    cell.AddData(dataEntry);
                }
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


    private GameObject FindOrCreateCell(int agentID, string cellType)
    {
        foreach (GameObject cell in spawnedCells)
        {
            CellManager cellManager = cell.GetComponent<CellManager>();
            if (cellManager.agentID == agentID)
            {
                return cell;
            }
        }

        GameObject newCell = Instantiate(cellPrefab, transform);
        CellManager newCellManager = newCell.GetComponent<CellManager>();
        newCellManager.agentID = agentID;
        newCellManager.cellType = cellType; // Set cell type right after instantiation.
        spawnedCells.Add(newCell);
        return newCell;
    }


    private IEnumerator CheckForUpdates()
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
                currentBioTick += bioTickStep;

                UpdateSliderValue(currentBioTick);

                timeAccumulator -= 1.0f; // Decrease accumulator by one tick, handling multiple ticks if necessary

                bioTickDisplay.text = "BioTick: " + currentBioTick + "/" + simulationDuration;

                // Update each cell for the currentBioTick
                foreach (GameObject cellObject in spawnedCells)
                {
                    CellManager cellManager = cellObject.GetComponent<CellManager>();
                    cellManager.UpdateState(currentBioTick);
                }

                UpdateVoxelsForBioTick(currentBioTick);
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

        XmlNode simulationDurationNode = xmlDoc.SelectSingleNode("//Model/simulation_duration");
        if (simulationDurationNode != null && int.TryParse(simulationDurationNode.InnerText, out int simulationDuration))
        {
            this.simulationDuration = simulationDuration;
        }

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

        Vector3 startPosition = transform.position;
        startPosition += voxelDimensions * 0.5f;

        for (int z = 0; z < gridCubeNumber.z; z++)
        {
            for (int y = 0; y < gridCubeNumber.y; y++)
            {
                for (int x = 0; x < gridCubeNumber.x; x++)
                {
                    // Calculate the position based on grid coordinates and voxel dimensions
                    Vector3 position = startPosition + new Vector3(x * voxelDimensions.x, y * voxelDimensions.y, z * voxelDimensions.z);

                    // Check if there is molecule data for the current voxel index
                    List<MoleculeCSVData> voxelDataList = csvReader.GetDataForVoxel(voxelIndex, currentBioTick);
                    if (voxelDataList.Count > 0)
                    {
                        // Instantiate voxel GameObject and initialize its properties using voxelData
                        GameObject voxelGO = Instantiate(voxelPrefab, position, Quaternion.identity, transform);
                        VoxelManager voxelManager = voxelGO.GetComponent<VoxelManager>();
                        voxelGO.transform.name = "Voxel_" + voxelIndex;
                        spawnedVoxels.Add(voxelGO);

                        // Initialize the voxel with all molecule data pertaining to it
                        if (voxelManager != null)
                        {
                            voxelManager.Initialize(voxelIndex, voxelDataList); // Adjusted to pass the whole list
                        }
                    }

                    voxelIndex++;
                }
            }
        }
        isVoxelDataLoaded = true;
    }


    public Material[] GetMaterialsForCellType(string cellType)
    {
        if (string.IsNullOrEmpty(cellType))
        {
            Debug.LogWarning("Cell type is null or empty.");
            return null;
        }

        if (cellMaterialsMap.TryGetValue(cellType, out Material[] materials))
        {
            return materials;
        }
        else
        {
            Debug.LogWarning($"Materials for cell type '{cellType}' not found.");
            return null;
        }
    }


    void HandleSliderValueChanged(float value)
    {
        currentBioTick = (int)value * bioTickStep;
        UpdateSimulationStateToCurrentBioTick();
    }


    void UpdateSimulationStateToCurrentBioTick()
    {
        foreach (GameObject cellObject in spawnedCells)
        {
            CellManager cellManager = cellObject.GetComponent<CellManager>();
            cellManager.UpdateStateToTick(currentBioTick);
        }
    }


    public void UpdateSliderMaxValue()
    {
        int maxBioTick = 0;
        foreach (var cellList in csvReader.CellPositionData.Values) // Or csvReader.CellPositionData.Values if using a property
        {
            foreach (var data in cellList)
            {
                if (data.bioTicks > maxBioTick)
                {
                    maxBioTick = (int)Math.Ceiling(data.bioTicks);
                }
            }
        }
        timeSlider.maxValue = maxBioTick;
        Debug.Log($"Max BioTick updated to: {maxBioTick}");
    }


    void UpdateSliderValue(int bioTick)
    {
        if (timeSlider != null)
        {
            // Convert the current bio tick to steps for the slider
            timeSlider.value = bioTick / bioTickStep;
        }
    }


    private void UpdateVoxelsForBioTick(int bioTick)
    {
        // Calculate global maximum concentrations for this bioTick
        var globalMaxConcentrationPerType = CalculateGlobalMaxConcentration();

        foreach (GameObject voxelObject in spawnedVoxels)
        {
            VoxelManager voxelManager = voxelObject.GetComponent<VoxelManager>();
            int globalID = voxelManager.globalID;

            // Ensure to get data for the current bioTick
            List<MoleculeCSVData> dataList = csvReader.GetDataForVoxel(globalID, bioTick);

            // Pass globalMaxConcentrationPerType to each voxel manager
            if (dataList != null && dataList.Count > 0)
            {
                voxelManager.UpdateVoxelForBioTick(bioTick, dataList, globalMaxConcentrationPerType);
            }
        }
    }

    /// a method to calculate the global maximum concentrations and update it every bioTick
    private Dictionary<int, float> CalculateGlobalMaxConcentration()
    {
        var globalMaxConcentrationPerType = new Dictionary<int, float>();

        // This assumes GetAllMoleculeData() provides all molecule data correctly
        var allMoleculeData = csvReader.GetAllMoleculeData();
        foreach (var data in allMoleculeData)
        {
            if (!globalMaxConcentrationPerType.ContainsKey(data.moleculeType) ||
                data.concentration > globalMaxConcentrationPerType[data.moleculeType])
            {
                globalMaxConcentrationPerType[data.moleculeType] = data.concentration;
            }
        }
        return globalMaxConcentrationPerType;
    }


    public float GetTimeMultiplier()
    {
        return timeMultiplier;
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