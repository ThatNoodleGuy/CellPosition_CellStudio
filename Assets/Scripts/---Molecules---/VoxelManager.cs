using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoleculeAlphaData
{
    public int moleculeType;
    public float alphaValue;
}

public class VoxelManager : MonoBehaviour
{
    [Header("Voxel Data")]
    public int globalID;

    private Dictionary<int, GameObject> moleculeObjects = new Dictionary<int, GameObject>();
    private float totalConcentration;
    private const float negligibleConcentrationThreshold = 0.01f; // Adjust as needed


    /// Initializes the voxel manager with a global ID and an optional list of initial molecule data.
    public void Initialize(int globalID, List<MoleculeCSVData> initialDataList = null)
    {
        this.globalID = globalID;

        if (initialDataList != null)
        {
            CalculateTotalConcentration(initialDataList);
        }
    }


    /// Calculates the total concentration of all molecules in the voxel.
    private void CalculateTotalConcentration(List<MoleculeCSVData> dataList)
    {
        totalConcentration = dataList.Sum(data => data.concentration);
    }


    /// Updates an existing molecule object or creates a new one if it doesn't exist.
    /// Applies visualization adjustments based on molecule concentration.
    private void UpdateOrCreateMoleculeObject(MoleculeCSVData data, Dictionary<int, float> globalMaxConcentrationPerType)
    {
        if (!moleculeObjects.TryGetValue(data.moleculeType, out GameObject moleculeObject))
        {
            moleculeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(moleculeObject.GetComponent<Collider>());
            moleculeObject.transform.SetParent(this.transform, false);
            moleculeObject.name = $"Molecule_{data.moleculeType}";
            moleculeObjects[data.moleculeType] = moleculeObject;
            ApplyMaterial(moleculeObject, data.moleculeType);
        }

        AdjustMoleculeVisualization(moleculeObject, data, globalMaxConcentrationPerType);
    }


    /// Assigns a material to a molecule object based on its type. Ensures each molecule has a unique material instance.
    private void ApplyMaterial(GameObject moleculeObject, int moleculeType)
    {
        string materialName = $"moleculeType{moleculeType}";
        Material originalMaterial = Resources.Load<Material>(materialName);
        if (originalMaterial != null)
        {
            Renderer renderer = moleculeObject.GetComponent<Renderer>();
            // Instantiate a new Material instance based on the original material
            renderer.material = new Material(originalMaterial);
        }
        else
        {
            Debug.LogWarning($"Material '{materialName}' not found. Ensure it's located in a Resources folder.");
        }
    }


    /// Updates the visualization of molecules within the voxel for a specific bio tick.
    public void UpdateVoxelForBioTick(int bioTick, List<MoleculeCSVData> dataList, Dictionary<int, float> globalMaxConcentrationPerType)
    {
        // No need to calculate globalMaxConcentrationPerType here; it's passed in directly

        moleculeObjects.Values.ToList().ForEach(Destroy);
        moleculeObjects.Clear();

        foreach (var data in dataList)
        {
            if (data.concentration > negligibleConcentrationThreshold)
            {
                UpdateOrCreateMoleculeObject(data, globalMaxConcentrationPerType);
            }
        }
    }

    /// Adjusts the visibility and alpha value of a molecule object based on its concentration.
    private void AdjustMoleculeVisualization(GameObject moleculeObject, MoleculeCSVData data, Dictionary<int, float> globalMaxConcentrationPerType)
    {
        Renderer renderer = moleculeObject.GetComponent<Renderer>();
        if (data.concentration > negligibleConcentrationThreshold)
        {
            float maxConcentration = globalMaxConcentrationPerType[data.moleculeType];
            float relativeConcentration = data.concentration / maxConcentration;
            float alphaValue = Mathf.Clamp(relativeConcentration, 0.001f, 0.99f); // Ensures no full opacity
            Color newColor = renderer.material.color;
            newColor.a = alphaValue;
            renderer.material.color = newColor; // Apply the new color with adjusted alpha
            renderer.enabled = true;
        }
        else
        {
            renderer.enabled = false;
        }
    }

}