using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CellBehaviour : MonoBehaviour
{
    public int AgentID { get; private set; }
    public float BioTicks { get; private set; }
    public int InteractionType { get; private set; }
    public int OtherCellID { get; private set; }

    [SerializeField] private TextMeshPro textMeshPro;

    public void Initialize(CellPositionCSVReader.CSVData data, Material[] interactionMaterials)
    {
        AgentID = data.agentID;
        BioTicks = data.bioTicks;
        InteractionType = data.interactionType;
        OtherCellID = data.otherCellID;

        // Update position
        transform.position = new Vector3(data.posX, data.posY, data.posZ);

        // Set the text if applicable
        if (textMeshPro != null)
        {
            textMeshPro.text = $"Cell: {AgentID}";
        }

        // Set material based on interaction type
        if (InteractionType >= 0 && InteractionType < interactionMaterials.Length)
        {
            Renderer cellRenderer = GetComponent<Renderer>();
            if (cellRenderer != null)
            {
                cellRenderer.material = interactionMaterials[InteractionType];
            }
        }
    }
}
