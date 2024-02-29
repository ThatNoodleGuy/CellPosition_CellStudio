using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellManager : MonoBehaviour
{
    [System.Serializable]
    public struct SizeRange
    {
        public Vector3 minSize;
        public Vector3 maxSize;
    }

    public int agentID;
    public string cellType;
    public TextMeshPro cellTypeText;
    public int cellLifeState;
    public int cylinderInteraction;

    [SerializeField] private string interaction;

    private List<CSVReader.CSVData> cellData = new List<CSVReader.CSVData>();
    private int currentDataIndex = 0;
    private bool sizeHasBeenSet = false; // Flag to track if size has been set
    private int interactionType; // Visible in the Unity Editor

    private Dictionary<string, SizeRange> cellTypeSizeRanges = new Dictionary<string, SizeRange>()
    {
        {"Monocyte", new SizeRange { minSize = new Vector3(7f, 7f, 7f), maxSize = new Vector3(9f, 9f, 9f) }},
        {"TCell", new SizeRange { minSize = new Vector3(4f, 4f, 4f), maxSize = new Vector3(6f, 6f, 6f) }},
        // Additional cell types and their size ranges
    };

    public void Initialize(int agentID, Material[] interactionMaterials, CSVReader.CSVData initialData = null)
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

    public void AddData(CSVReader.CSVData data)
    {
        cellData.Add(data);
    }

    private void SetCellProperties(CSVReader.CSVData data, Material[] interactionMaterials)
    {
        gameObject.name = "Cell_" + data.agentID;
        cellType = data.cellType;
        this.interactionType = data.interactionType; // Update interaction type here
        interaction = data.interactionType.ToString();
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
        cellLifeState = data.cellLifeState;
        cylinderInteraction = data.cylinderInteraction;

        if (interactionMaterials != null && data.interactionType >= 0 && data.interactionType < interactionMaterials.Length)
        {
            GetComponent<Renderer>().material = interactionMaterials[data.interactionType];
        }

        if (cellTypeText != null)
        {
            cellTypeText.text = cellType;
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

    public int GetCellLifeState()
    {
        return cellLifeState;
    }
}