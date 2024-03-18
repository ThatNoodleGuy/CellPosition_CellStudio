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

    private Dictionary<int, GameObject> moleculeObjects = new Dictionary<int, GameObject>();

    private void Awake()
    {
        // Initialize moleculeObjects if needed
    }

    public void Initialize(int globalID, List<MoleculeCSVData> initialDataList = null)
    {
        this.globalID = globalID;
        foreach (var data in initialDataList)
        {
            UpdateOrCreateMoleculeObject(data);
        }
    }

    private void UpdateOrCreateMoleculeObject(MoleculeCSVData data)
    {
        GameObject moleculeObject;
        if (!moleculeObjects.TryGetValue(data.moleculeType, out moleculeObject))
        {
            // Create a new molecule representation as a child of the voxel
            moleculeObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // Or use a custom mesh
            moleculeObject.transform.SetParent(this.transform);
            moleculeObject.name = "Molecule_" + data.moleculeType;
            moleculeObjects[data.moleculeType] = moleculeObject;

            // Find and apply the material corresponding to the molecule type
            string materialName = $"moleculeType{data.moleculeType}";
            var material = Resources.Load<Material>(materialName); // Assumes the materials are located in a Resources folder
            if (material != null)
            {
                var renderer = moleculeObject.GetComponent<Renderer>();
                renderer.material = material;
            }
            else
            {
                Debug.LogWarning($"Material '{materialName}' not found. Ensure it's located in a Resources folder.");
            }
        }

        // Adjust the moleculeObject based on the concentration
        AdjustMoleculeVisualization(moleculeObject, data);
    }

    private void AdjustMoleculeVisualization(GameObject moleculeObject, MoleculeCSVData data)
    {
        // Example: adjust the size and color based on concentration
        float scale = Mathf.Clamp(data.concentration, 0.1f, 1f); // Adjust scaling logic based on actual concentration range
        moleculeObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void UpdateVoxelForBioTick(int bioTick, List<MoleculeCSVData> dataList)
    {
        foreach (var data in dataList)
        {
            UpdateOrCreateMoleculeObject(data);
        }
    }
}