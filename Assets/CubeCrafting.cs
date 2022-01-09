using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.FPS.Game;

public class CubeCrafting : MonoBehaviour
{ 
    public GameObject boxBlueprint;
    public int xSize = 3;
    public int zSize = 3;
    public double timeSecsCraft = 0.3;
    public float offsetX = -20;
    public float offsetY = 3;
    public float offsetZ = 0;

    double timeLastCraft;
    int builtCount;
    float boxSize;


    // Start is called before the first frame update
    void Start()
    {
        boxSize = 0.5f;
        // TODO how to dynamically get the size of the cube?
        // boxSize = GetComponent<Renderer>().bounds.extents.x/2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeLastCraft > timeSecsCraft)
        {
            float x = builtCount % xSize;
            float y = builtCount / (xSize*zSize);
            float z = builtCount / xSize % zSize;
            Instantiate(boxBlueprint, new Vector3(x * boxSize + offsetX, y * boxSize + offsetY, z * boxSize + offsetZ), Quaternion.identity);
            builtCount++;
            timeLastCraft = Time.time;
        }
    }

}
