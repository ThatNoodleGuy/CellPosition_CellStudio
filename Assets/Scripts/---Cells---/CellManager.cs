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

    [SerializeField] private string interaction;

    [Header("Cell UI")]
    [SerializeField] private GameObject cellInfoPanel;
    [SerializeField] private TextMeshProUGUI cellIdText;
    [SerializeField] private TextMeshProUGUI cellTypeText;
    [SerializeField] private TextMeshProUGUI cellStateText;
    [SerializeField] private Button closeCellUIButton;

    private List<CSVReader.CellPositionCSVData> cellData = new List<CSVReader.CellPositionCSVData>();
    private int currentDataIndex = 0;
    private bool sizeHasBeenSet = false; // Flag to track if size has been set
    private int interactionType; // Visible in the Unity Editor

    private Dictionary<string, SizeRange> cellTypeSizeRanges = new Dictionary<string, SizeRange>()
    {
        {"Monocyte", new SizeRange { minSize = new Vector3(7f, 7f, 7f), maxSize = new Vector3(9f, 9f, 9f) }},
        {"TCell", new SizeRange { minSize = new Vector3(4f, 4f, 4f), maxSize = new Vector3(6f, 6f, 6f) }},
        // Additional cell types and their size ranges
    };

    private void Start()
    {
        HideCellUI();
        closeCellUIButton.onClick.AddListener(HideCellUI);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Checks for left mouse click
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f)) // Raycasts to detect clicks on objects
            {
                CellManager clickedCell = hit.collider.GetComponent<CellManager>();
                if (clickedCell != null)
                {
                    // If the hit object has a CellManager, show its info using its own method
                    clickedCell.UpdateCellUI(clickedCell.agentID, clickedCell.cellType, clickedCell.cellState);
                }
            }
        }
    }

    public void Initialize(int agentID, Material[] interactionMaterials, CSVReader.CellPositionCSVData initialData = null)
    {
        this.agentID = agentID;
        if (initialData != null)
        {
            SetCellProperties(initialData, interactionMaterials);
            // Ensure the size is set only once upon initialization
            if (!sizeHasBeenSet)
            {
                SetCellSize(initialData.cellType);
                sizeHasBeenSet = true;
            }
        }
    }

    public void AddData(CSVReader.CellPositionCSVData data)
    {
        cellData.Add(data);
    }

    private void SetCellProperties(CSVReader.CellPositionCSVData data, Material[] interactionMaterials)
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

    public void UpdateState(int bioTick, Material[] interactionMaterials)
    {
        if (currentDataIndex < cellData.Count && cellData[currentDataIndex].bioTicks <= bioTick)
        {
            SetCellProperties(cellData[currentDataIndex], interactionMaterials);
            currentDataIndex++;
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
        cellInfoPanel.SetActive(true);
    }

    public void HideCellUI()
    {
        cellInfoPanel.SetActive(false);
    }
}