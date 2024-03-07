using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelManager : MonoBehaviour
{
    [Header("Voxel Data")]
    public int voxelID;
    public float concentration;
    public int moleculeType;
    public int bioTick;

    public Material[] moleculeMaterials;
    private Renderer voxelRenderer;
    private float maxConcentration = 1000f; // Set this to your maximum expected concentration

    // Assuming VoxelData is a structure similar to CellPositionCSVData
    private List<VoxelData> voxelData = new List<VoxelData>();

    void Start()
    {
        // Initialize the Renderer component
        voxelRenderer = GetComponent<Renderer>();
    }

    // Example function to initialize voxel data, assuming you have a similar structure for voxel data
    public void Initialize(int voxelID, VoxelData initialData = null)
    {
        this.voxelID = voxelID;
        if (initialData != null)
        {
            SetVoxelProperties(initialData);
        }
    }

    // Similar to SetCellProperties but adapted for voxel properties
    private void SetVoxelProperties(VoxelData data)
    {
        // Set the properties of your voxel based on the data
        this.concentration = data.concentration;
        this.moleculeType = data.moleculeType;
        this.bioTick = data.bioTick;
    }

    public void AddData(VoxelData data)
    {
        voxelData.Add(data);
    }

    public void UpdateVoxelMaterial(int moleculeType)
    {
        if (moleculeType >= 0 && moleculeType < moleculeMaterials.Length)
        {
            // Use the moleculeType as an index to select the appropriate material from the array
            voxelRenderer.material = moleculeMaterials[moleculeType];
        }
        else
        {
            Debug.LogWarning("Invalid molecule type provided. Cannot update material.");
        }
    }

    public void UpdateVoxelData(VoxelData voxelData)
    {
        // Select the material based on the molecule type
        // Ensuring the first material corresponds to moleculeType = 0
        if (voxelData.moleculeType >= 0 && voxelData.moleculeType < moleculeMaterials.Length)
        {
            voxelRenderer.material = moleculeMaterials[voxelData.moleculeType];
        }
        else
        {
            Debug.LogWarning($"Invalid molecule type provided: {voxelData.moleculeType}. Cannot update material.");
            return;
        }

        // Calculate and clamp the alpha value based on concentration
        float alphaValue = Mathf.Lerp(0f, 1f, voxelData.concentration / maxConcentration);
        alphaValue = Mathf.Clamp(alphaValue, 0f, 1f);

        // Update the material's alpha value
        Color color = voxelRenderer.material.color;
        color.a = alphaValue;
        voxelRenderer.material.color = color;
    }
}