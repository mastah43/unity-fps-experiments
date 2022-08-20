using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;

public class CosmoPlayerInputController : MonoBehaviour
{
    private static readonly ILogger Logger = Debug.unityLogger;

    public GameObject boxBlueprint;
    public double timeSecsCraft = 0.3;
    public float distanceMaxCraft = 10;

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
        if (CanProcessInput() && IsCraftButton() && CanCraftBoxTimely() && IsCraftPositionOkay(out var hit) && CanCraftOnHit(hit))
        {
            CraftBox(hit);
        }
        
    }

    private void CraftBox(RaycastHit hit)
    {
        var boxCrafted = Instantiate(boxBlueprint, GetCraftPosition(hit), Quaternion.identity);
        boxCrafted.transform.SetParent(hit.transform);
        Logger.Log($"Crafted a box at: {boxCrafted.transform.position}");
        timeLastCraft = Time.time;
    }

    private bool CanCraftOnHit(RaycastHit hit)
    {
        return CanCraftToDistance(hit.distance);
    }

    private bool CanCraftToDistance(float hitDistance)
    {
        return hitDistance <= distanceMaxCraft;
    }

    private bool IsCraftPositionOkay(out RaycastHit hit)
    {
        return Physics.Raycast(camera1.ScreenPointToRay(Input.mousePosition), out hit, maxDistance: distanceMaxCraft);
    }

    private static bool IsCraftButton()
    {
        return Input.GetButton("Craft");
    }

    private bool CanCraftBoxTimely()
    {
        return Time.time - timeLastCraft > timeSecsCraft;
    }

    private Vector3 GetCraftPosition(RaycastHit hit)
    {
        // TODO use a different approach by finding the plane of the box we hit and use their position
        // for the new box since this approach here will have problems when the hit was close to the existing boxes edge
        var spaceToHit = (float)boxSize / 4f; 
        var hitPointWithDistance =
            Vector3.MoveTowards(camera1.transform.position, hit.point, hit.distance - spaceToHit);
        Logger.Log($"Craft position: hit.point={hit.point}, hit.distance={hit.distance}, spaceToHit={spaceToHit}");
        return new Vector3(
            x: GetGridAlignedCoordinateElement(hitPointWithDistance.x),
            y: GetGridAlignedCoordinateElement(hitPointWithDistance.y),
            z: GetGridAlignedCoordinateElement(hitPointWithDistance.z));
    }

    private float GetGridAlignedCoordinateElement(float e) => e - e % (float)boxSize;

    private bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }
}
