using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmoPlayerInputController : MonoBehaviour
{
    private static readonly ILogger logger = Debug.unityLogger;

    public GameObject boxBlueprint;
    public double timeSecsCraft = 0.3;

    private double timeLastCraft;
    private double boxSize;
    private Camera camera1;

    private void Awake()
    {
        camera1 = Camera.main;
    }

    void Start()
    {
        boxSize = boxBlueprint.transform.localScale.x;
    }

    void Update()
    {
        if (IsCraftButton())
        {
            if (CanCraftBoxTimely())
            {
                if (IsCraftPositionOkay(out var hit))
                {
                    Vector3 craftPosition = GetGridAlignedCoordinate(hit);

                    logger.Log($"Crafting a box at: {craftPosition}");
                    Instantiate(boxBlueprint, craftPosition, Quaternion.identity);
                    timeLastCraft = Time.time;
                }
            }
        }
        
    }

    private bool IsCraftPositionOkay(out RaycastHit hit)
    {
        return Physics.Raycast(camera1.ScreenPointToRay(Input.mousePosition), out hit, 100);
    }

    private static bool IsCraftButton()
    {
        return Input.GetButton("Craft");
    }

    private bool CanCraftBoxTimely()
    {
        return Time.time - timeLastCraft > timeSecsCraft;
    }

    private Vector3 GetGridAlignedCoordinate(RaycastHit hit)
    {
        Vector3 coordinate = hit.point;
        coordinate.x = GetGridAlignedCoordinateElement(coordinate.x);
        coordinate.y = GetGridAlignedCoordinateElement(coordinate.y);
        coordinate.z = GetGridAlignedCoordinateElement(coordinate.z);
        return coordinate;
    }

    private float GetGridAlignedCoordinateElement(float x)
    {
        return x - (x % (float)boxSize);
    }

    private bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }
}
