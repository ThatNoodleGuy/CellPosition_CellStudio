using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Linq;

public class CylindersGenerator : MonoBehaviour
{
    public GameObject gameObjectPrefab;
    public int numberOfCylinders; // Declare numberOfCylinders as a class member

    void Start()
    {
        // Load the XML file from the Resources folder
        TextAsset xmlCylindersFile = Resources.Load<TextAsset>("Cylinders");
        if (xmlCylindersFile == null)
        {
            Debug.LogError("Failed to load Cylinders XML file.");
            return;
        }

        TextAsset xmlExampleReducedSV = Resources.Load<TextAsset>("E0");
        if (xmlExampleReducedSV == null)
        {
            Debug.LogError("Failed to load E0 XML file.");
            return;
        }

        GenerateCylindersFromXML(xmlCylindersFile.text, xmlExampleReducedSV.text);
    }

    void GenerateCylindersFromXML(string cylindersXml, string exampleReducedXml)
    {
        XmlDocument cylindersDoc = new XmlDocument();
        cylindersDoc.LoadXml(cylindersXml);

        XmlDocument exampleReducedDoc = new XmlDocument();
        exampleReducedDoc.LoadXml(exampleReducedXml);

        // Parse the E0.xml to retrieve the cylinder shift information
        XmlNodeList shiftNodes = exampleReducedDoc.GetElementsByTagName("CylindersShift");
        if (shiftNodes.Count > 0)
        {
            XmlNode shiftNode = shiftNodes[0];
            float shiftX = float.Parse(shiftNode.SelectSingleNode("x").InnerText);
            float shiftY = float.Parse(shiftNode.SelectSingleNode("y").InnerText);
            float shiftZ = float.Parse(shiftNode.SelectSingleNode("z").InnerText);

            Vector3 cylinderShift = new Vector3(shiftX, shiftY, shiftZ);
            //Debug.Log("Cylinder shift: " + cylinderShift);

            // Get all Cylinder nodes
            XmlNodeList cylinderNodes = cylindersDoc.SelectNodes("//Cylinder");
            numberOfCylinders = cylinderNodes.Count; // Store the number of cylinders

            int cylinders = 0;

            foreach (XmlNode cylinderNode in cylinderNodes)
            {
                // if (cylinders > 11)
                // {
                //     break;
                // }

                // Get direction
                float dirX = float.Parse(cylinderNode.SelectSingleNode("directionx").InnerText);
                float dirY = float.Parse(cylinderNode.SelectSingleNode("directiony").InnerText);
                float dirZ = float.Parse(cylinderNode.SelectSingleNode("directionz").InnerText);
                Vector3 direction = new Vector3(dirX, dirY, dirZ).normalized;

                // Get height
                float height = float.Parse(cylinderNode.SelectSingleNode("height").InnerText);
                float radius = float.Parse(cylinderNode.SelectSingleNode("radius").InnerText);

                // Get center coordinates
                float centerX = float.Parse(cylinderNode.SelectSingleNode("centerx").InnerText) + (height * 0.5f * dirX);
                float centerY = float.Parse(cylinderNode.SelectSingleNode("centery").InnerText) + (height * 0.5f * dirY);
                float centerZ = float.Parse(cylinderNode.SelectSingleNode("centerz").InnerText) + (height * 0.5f * dirZ);

                // Create position vector
                Vector3 position = new Vector3(centerX, centerY, centerZ) + cylinderShift;

                // Instantiate game object at the specified position with the correct orientation
                GameObject cylinder = Instantiate(gameObjectPrefab, position, Quaternion.identity);

                if (dirX == 1 && dirY == 0 && dirZ == 0)
                {
                    cylinder.transform.localScale = new Vector3(height, radius, radius);
                }
                else if (dirX == 0 && dirY == 1 && dirZ == 0)
                {
                    cylinder.transform.localScale = new Vector3(radius, height, radius);
                }
                else if (dirX == 0 && dirY == 0 && dirZ == 1)
                {
                    cylinder.transform.localScale = new Vector3(radius, radius, height);
                }

                // Assign name with ID
                int id = int.Parse(cylinderNode.SelectSingleNode("ID").InnerText);
                cylinder.name = "Cylinder_" + (id - 1);

                cylinder.transform.parent = transform;

                cylinders++;
            }
        }
        else
        {
            Debug.LogError("Cylinder shift information not found in XML.");
        }
    }
}