using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelManager : MonoBehaviour
{
    [Header("Voxel Data")]
    public int globalID;
    public float concentration;
    public int moleculeType;
    public int bioTick;

    public Material[] moleculeMaterials;
    private MeshRenderer voxelRenderer;
    private float maxConcentration = 1000f; // Set this to your maximum expected concentration

    private List<MoleculeCSVData> moleculeCSVData = new List<MoleculeCSVData>();

    private void Awake()
    {
        voxelRenderer = GetComponent<MeshRenderer>();
    }

    // Example function to initialize voxel data, assuming you have a similar structure for voxel data
    public void Initialize(int globalID, MoleculeCSVData initialData = null)
    {
        this.globalID = globalID;
        if (initialData != null)
        {
            SetVoxelProperties(initialData);
        }
    }

    // Similar to SetCellProperties but adapted for voxel properties
    private void SetVoxelProperties(MoleculeCSVData data)
    {
        // Set the properties of your voxel based on the data
        this.concentration = data.concentration;
        this.moleculeType = data.moleculeType;
        this.bioTick = data.bioTick;
    }

    public void AddData(MoleculeCSVData data)
    {
        moleculeCSVData.Add(data);
    }

    public void UpdateVoxelMaterial(int moleculeType, float concentration)
    {
        if (moleculeType >= 0 && moleculeType < moleculeMaterials.Length)
        {
            voxelRenderer.material = moleculeMaterials[moleculeType];
            // Adjust alpha based on concentration
            float alphaValue = Mathf.Lerp(0f, 1f, concentration / maxConcentration);
            alphaValue = Mathf.Clamp(alphaValue, 0f, 1f);
            Color color = voxelRenderer.material.color;
            color.a = alphaValue;
            voxelRenderer.material.color = color;
        }
        else
        {
            Debug.LogWarning("Invalid molecule type provided. Cannot update material.");
        }
    }

    public void UpdateVoxelForBioTick(int bioTick)
    {
        var relevantData = moleculeCSVData.FirstOrDefault(data => data.bioTick == bioTick);
        if (relevantData != null)
        {
            UpdateVoxelMaterial(relevantData.moleculeType, relevantData.concentration);
            UpdateVoxelData(relevantData);
        }
    }

    public void UpdateVoxelData(MoleculeCSVData data)
    {
        voxelRenderer = GetComponent<MeshRenderer>();
        // Select the material based on the molecule type
        // Ensuring the first material corresponds to moleculeType = 0
        if (data.moleculeType >= 0 && data.moleculeType < moleculeMaterials.Length)
        {
            voxelRenderer.material = moleculeMaterials[data.moleculeType];
        }
        else
        {
            Debug.LogWarning($"Invalid molecule type provided: {data.moleculeType}. Cannot update material.");
            return;
        }

        // Calculate and clamp the alpha value based on concentration
        float alphaValue = Mathf.Lerp(0f, 1f, data.concentration / maxConcentration);
        alphaValue = Mathf.Clamp(alphaValue, 0f, 1f);

        // Update the material's alpha value
        Color color = voxelRenderer.material.color;
        color.a = alphaValue;
        voxelRenderer.material.color = color;
    }

    public int GetVoxelGlobalID()
    {
        return globalID;
    }
}