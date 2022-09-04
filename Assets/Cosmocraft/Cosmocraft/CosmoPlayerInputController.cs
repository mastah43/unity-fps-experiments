using UnityEngine;

namespace Cosmocraft
{

    public class CosmoPlayerInputController : MonoBehaviour
    {
        private static readonly ILogger Logger = Debug.unityLogger;

        public GameObject inventoryPrefabSelected;
        public GameObject inventoryPrefab0;
        public GameObject inventoryPrefab1;
        public GameObject inventoryPrefab2;
        public GameObject inventoryPrefab3;
        public GameObject inventoryPrefab4;
        public GameObject inventoryPrefab5;
        public GameObject inventoryPrefab6;
        public GameObject inventoryPrefab7;
        public GameObject inventoryPrefab8;
        public GameObject inventoryPrefab9;
        public double timeSecsCraft = 0.3;
        public float distanceMaxCraft = 100;

        private double timeLastCraft;
        private Camera camera1;

        private void Awake()
        {
            camera1 = Camera.main;
        }

        void Start()
        {
            SelectInventory(0);
        }

        void Update()
        {
            if (CanProcessInput())
            {
                if (IsCraftButton() && CanCraftBoxTimely() && IsCraftPositionOkay(out var hit) && CanCraftOnHit(hit))
                {
                    CraftBox(hit);
                }

                HandleInputSelectInventory();
            }

        }

        private void HandleInputSelectInventory()
        {
            for (var inventorySlot = 0; inventorySlot <= 9; inventorySlot++)
            {
                var buttonName = "Inventory" + inventorySlot;
                if (Input.GetButton(buttonName))
                {
                    SelectInventory(inventorySlot);
                }
            }
        }

        public void SelectInventory(int button)
        {
            var fieldName = "inventoryPrefab" + button;
            inventoryPrefabSelected = (GameObject)typeof(CosmoPlayerInputController).GetField(fieldName).GetValue(this);
            Logger.Log($"selected inventory prefab {button}: {inventoryPrefabSelected}");
        }

        private void CraftBox(RaycastHit hit)
        {
            var boxCrafted = Instantiate(inventoryPrefabSelected, GetCraftPosition(hit), Quaternion.identity);
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
            return Physics.Raycast(camera1.ScreenPointToRay(Input.mousePosition), out hit,
                maxDistance: distanceMaxCraft);
        }

        private static bool IsCraftButton()
        {
            return Input.GetButton("Craft");
        }

        private bool CanCraftBoxTimely()
        {
            return Time.time - timeLastCraft > timeSecsCraft;
        }



        public Vector3 GetCraftPosition(RaycastHit hit)
        {
            var cameraPosition = camera1.transform.position;
            var cameraForward = camera1.transform.TransformDirection(Vector3.forward) * distanceMaxCraft;
            Debug.DrawRay(cameraPosition, cameraForward, Color.red, 3);
            //Debug.DrawRay(cameraPosition, hit.point - cameraPosition, Color.green, 3);

            // TODO use a different approach by finding the plane of the box we hit and use their position
            // for the new box since this approach here will have problems when the hit was close to the existing boxes edge

            var localPoint = hit.transform.InverseTransformPoint(hit.point);
            var localDir = localPoint.normalized;
            var upDot = Vector3.Dot(localDir, Vector3.up);
            var fwdDot = Vector3.Dot(localDir, Vector3.forward);
            var rightDot = Vector3.Dot(localDir, Vector3.right);
            var upPower = Mathf.Abs(upDot);
            var fwdPower = Mathf.Abs(fwdDot);
            var rightPower = Mathf.Abs(rightDot);

            var hitSide = new Vector3(
                x: fwdPower > upPower && fwdPower > rightPower ? (fwdDot > 0 ? 1 : -1) : 0,
                y: upPower > fwdPower && upPower > rightPower ? (upDot > 0 ? 1 : -1) : 0,
                z: rightPower > upPower && rightPower > fwdPower ? (rightPower > 0 ? 1 : -1) : 0);

            var hitColliderBounds = hit.transform.GetComponent<Collider>().bounds;

            // TODO handle if target transforms are too large - respect hit point

            Logger.Log("craft",
                $"Craft position: hit.point={hit.point}, hit side={hitSide}, hit.distance={hit.distance}, hit bounds extents={hitColliderBounds.extents}");
            var craftPosition = hitColliderBounds.center +
                                Vector3.Scale(hitColliderBounds.extents, hitSide) +
                                Vector3.Scale(inventoryPrefabSelected.GetComponent<Collider>().bounds.extents, hitSide);

            // TODO determine side of the collider to attach to to

            return craftPosition;

            /*
            var spaceToHit = GetCraftSize() / 3f; 
            var hitPointWithDistance =
                Vector3.MoveTowards(cameraPosition, hit.point, hit.distance - spaceToHit);
            Logger.Log("craft", $"Craft position: hit.point={hit.point}, hitPointWithDistance={hitPointWithDistance}, hit.distance={hit.distance}, spaceToHit={spaceToHit}");
            return new Vector3(
                x: GetGridAlignedCoordinateElement(hitPointWithDistance.x),
                y: GetGridAlignedCoordinateElement(hitPointWithDistance.y),
                z: GetGridAlignedCoordinateElement(hitPointWithDistance.z));
                */
        }

        private float GetCraftSize()
        {
            return inventoryPrefabSelected.transform.localScale.x;
        }

        private float GetGridAlignedCoordinateElement(float e) => e - e % GetCraftSize();

        private bool CanProcessInput()
        {
            return Cursor.lockState == CursorLockMode.Locked;
        }
    }

}