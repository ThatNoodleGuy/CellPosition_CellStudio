using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CellManager : MonoBehaviour
{
    [System.Serializable]
    public struct SizeRange
    {
        public Vector3 minSize;
        public Vector3 maxSize;
    }

    [Header("Cell Info")]
    public int agentID;
    public string cellType;
    public string cellState;
    public int cylinderInteraction;
    private string interaction;

    [Header("Cell UI")]
    [SerializeField] private GameObject cellCanvas;
    [SerializeField] private TextMeshProUGUI cellIdText;
    [SerializeField] private TextMeshProUGUI cellTypeText;
    [SerializeField] private TextMeshProUGUI cellStateText;
    [SerializeField] private Button closeCellUIButton;

    private SimulationManager simulationManager;
    private Material[] interactionMaterials;
    private List<CellPositionCSVData> cellData = new List<CellPositionCSVData>();
    private int currentDataIndex = 0;
    private bool sizeHasBeenSet = false; // Flag to track if size has been set
    private int interactionType;
    private static bool isPanelOpen = false;


    private Dictionary<string, SizeRange> cellTypeSizeRanges = new Dictionary<string, SizeRange>()
    {
        {"Monocyte", new SizeRange { minSize = new Vector3(7f, 7f, 7f), maxSize = new Vector3(9f, 9f, 9f) }},
        {"TCell", new SizeRange { minSize = new Vector3(4f, 4f, 4f), maxSize = new Vector3(6f, 6f, 6f) }},
        // Additional cell types and their size ranges
    };


    private void Awake()
    {
        simulationManager = SimulationManager.instance; // Assuming there's a public static instance
    }


    private void Start()
    {
        HideCellUI();
        FetchMaterials();
        closeCellUIButton.onClick.AddListener(HideCellUI);
    }


    private void OnMouseDown()
    {
        if (SimulationManager.instance.GetTimeMultiplier() == 0 && !isPanelOpen)
        {
            UpdateCellUI(agentID, cellType, cellState);
            isPanelOpen = true;
        }
    }


    public void Initialize(int agentID, string cellType, CellPositionCSVData initialData)
    {
        this.agentID = agentID;
        this.cellType = cellType;

        // Ensure materials are fetched after the cell type is set.
        FetchMaterials();

        if (initialData != null)
        {
            SetCellProperties(initialData, interactionMaterials);
            if (!sizeHasBeenSet)
            {
                SetCellSize(cellType);
                sizeHasBeenSet = true;
            }
        }
    }

    public void AddData(CellPositionCSVData data)
    {
        cellData.Add(data);
    }

    private void SetCellProperties(CellPositionCSVData data, Material[] interactionMaterials)
    {
        gameObject.name = "Cell_" + data.agentID;
        cellType = data.cellType;
        this.interactionType = data.interactionType; // Update interaction type here
        interaction = data.interactionType.ToString();
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        cellState = data.cellState;
        cylinderInteraction = data.cylinderInteraction;

        if (interactionMaterials != null && data.interactionType >= 0 && data.interactionType < interactionMaterials.Length)
        {
            GetComponent<Renderer>().material = interactionMaterials[data.interactionType];
        }

        // Only set size if it hasn't been set yet
        if (!sizeHasBeenSet)
        {
            SetCellSize(cellType);
            sizeHasBeenSet = true;
        }
    }

    private void SetCellSize(string cellType)
    {
        if (cellTypeSizeRanges.TryGetValue(cellType, out SizeRange sizeRange))
        {
            Vector3 randomSize = new Vector3(
                UnityEngine.Random.Range(sizeRange.minSize.x, sizeRange.maxSize.x),
                UnityEngine.Random.Range(sizeRange.minSize.y, sizeRange.maxSize.y),
                UnityEngine.Random.Range(sizeRange.minSize.z, sizeRange.maxSize.z));
            transform.localScale = randomSize;
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f); // Default size if not specified
        }
    }

    public void UpdateState(int bioTick)
    {
        while (currentDataIndex < cellData.Count && cellData[currentDataIndex].bioTicks <= bioTick)
        {
            var data = cellData[currentDataIndex++];
            // Apply material based on the interaction type, including dead cell logic
            int materialIndex = GetMaterialIndexBasedOnState(data);
            GetComponent<Renderer>().material = interactionMaterials[materialIndex];
            interaction = data.interactionType.ToString();
            transform.position = new Vector3(data.posX, data.posY, data.posZ);
        }
    }

    private int GetMaterialIndexBasedOnState(CellPositionCSVData data)
    {
        if (data.cellState == "APOPTOSIS")
        {
            return 4; // Assuming the dead cell material is at index 4
        }
        // Example for mapping other states, ensure these map correctly to your data
        switch (data.interactionType)
        {
            case 0: return 0; // NO_HIT
            case 1: return 1; // WALL_HIT
            case 2: return 2; // CYLINDER_HIT
            case 3: return 3; // CELL_HIT
            default: return 0; // Default to NO_HIT if unsure
        }
    }

    public string GetCellState()
    {
        return cellState;
    }

    public void UpdateCellUI(int agentID, string cellType, string cellState)
    {
        cellIdText.text = "ID: " + agentID;
        cellTypeText.text = "Type: " + cellType;
        cellStateText.text = "State: " + cellState;
        cellCanvas.SetActive(true);
    }

    public void HideCellUI()
    {
        cellCanvas.SetActive(false);
        isPanelOpen = false;
    }

    void FetchMaterials()
    {
        if (!string.IsNullOrEmpty(cellType))
        {
            interactionMaterials = simulationManager.GetMaterialsForCellType(cellType);
            if (interactionMaterials == null)
            {
                Debug.LogError($"Materials for cell type '{cellType}' not found.");
            }
        }
        else
        {
            Debug.LogError("Cell type is not set before fetching materials.");
        }
    }

    public void UpdateStateToTick(int bioTick)
    {
        // Find the closest state at or before bioTick
        CellPositionCSVData closestData = null;
        foreach (var data in cellData)
        {
            if (data.bioTicks <= bioTick)
            {
                closestData = data;
            }
            else
            {
                break; // Assumes cellData is sorted by bioTicks
            }
        }

        if (closestData != null)
        {
            SetCellProperties(closestData, interactionMaterials); // Use SetCellProperties to apply the state
        }
    }
}