using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridMaker : MonoBehaviour
{

    //Prefab to instantiate to create the grid
    public GameObject gridCellPrefab;

    public GameObject textCellPrefab;

    //Width and height of the grid
    public int gridWidth;
    public int gridHeight;

    //Offset between grid cells
    public float offset = 0.05f;

    //Width of the actual grid cell
    float gridSizeX;

    void GetSize()
    {
        MeshRenderer renderer = gridCellPrefab.GetComponent<MeshRenderer>();
        gridSizeX = renderer.bounds.size.x;
    }

    public void CreateGrid()
    {
        GetSize();
        DeleteGrid();

        //Loop through how many cells wide we want
        for (int i = 0; i < gridWidth + 1; i++)
        {
            //Loop through how many cells high we want
            for (int j = 0; j < gridHeight + 1; j++)
            {
                //If we're on the edge 
                if (i == 0 || j == 0)
                {
                    GameObject instance = Instantiate(textCellPrefab, this.transform);
                    //Get it's position 
                    Vector3 position = instance.transform.position;

                    instance.transform.SetAsFirstSibling();

                    //Modify the position based on where we are within the loop
                    position.x = (i * (gridSizeX + offset));
                    //Flip the y pos so we are creating it top -> bottom not bottom -> top
                    position.z = (j * (gridSizeX + offset)) * -1;

                    //Set the new position
                    instance.transform.position = position;

                    //Label this as a text object
                    instance.name = "Text";

                    TextMesh txtMesh = instance.GetComponentInChildren<TextMesh>();
                    if(i == 0 && j == 0)
                    {
                        txtMesh.text = "";
                    }
                    //If we're on the left
                    else if (i == 0)
                    {
                        txtMesh.text = "edge";
                        //Get the row we are on and convert to string
                        string asciiString = (j - 1).ToString();
                        //Convert the row number to it's ascii value
                        char c = asciiString[0];
                        //Conver to a number so we can modify it mathematically
                        int asciiVal = c;

                        //Add 17 on so it becomes a letter's ascii value 
                        //i.e '0' = 48 and we want that to become 'A'= 65 
                        asciiVal += 17;

                        //Convert the ascii value back to it's character value
                        c = (char)asciiVal;

                        txtMesh.text = c.ToString();
                        instance.name = "Text";
                    }
                    else if (j == 0)
                    {

                        txtMesh.text = i.ToString();
                    }
                }
                else
                {
                    //Create a new grid cell
                    GameObject instance = Instantiate(gridCellPrefab, this.transform);
                    //Get it's position 
                    Vector3 position = instance.transform.position;

                    //Modify the position based on where we are within the loop
                    position.x = (i * (gridSizeX + offset));
                    //Flip the y pos so we are creating it top -> bottom not bottom -> top
                    position.z = (j * (gridSizeX + offset)) * -1;

                    //Set the new position
                    instance.transform.position = position;

                    RenameCell(instance, i - 1, j - 1);
                }
            }
        }

        //Get the position of the transform parent (the gameobject this script is attached to)
        Vector3 overallPos = this.transform.position;

        //Move the grid to the left by half of it's width
        overallPos.x = -((gridWidth * gridSizeX) / 2);
        //Move the grid up by half of it's height
        overallPos.z = ((gridHeight * gridSizeX) / 2);

        //Set the position of the transform parent to this new value
        this.transform.position = overallPos;
    }

    void RenameCell(GameObject instance, int row, int collumn)
    {
        //Get the row we are on and convert to string
        string asciiString = collumn.ToString();
        //Convert the row number to it's ascii value
        char c = asciiString[0];
        //Conver to a number so we can modify it mathematically
        int asciiVal = c;

        //Add 17 on so it becomes a letter's ascii value 
        //i.e '0' = 48 and we want that to become 'A'= 65 
        asciiVal += 17;

        //Convert the ascii value back to it's character value
        c = (char)asciiVal;

        //Change the text mesh's text to display the character and then the collumn number
        string finalString = c + (row + 1).ToString();
        //gridText.text = finalString;

        instance.name = finalString;
    }

    public void DeleteGrid()
    {
        int childCount = this.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }

        this.transform.localPosition = new Vector3(0, 0, 0);
    }
}
