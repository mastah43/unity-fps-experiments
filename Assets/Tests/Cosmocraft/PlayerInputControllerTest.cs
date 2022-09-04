using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Cosmocraft;

namespace Tests.Cosmocraft
{
    public class PlayerInputControllerTest
    {
        private static readonly ILogger Logger = Debug.unityLogger;
        private readonly GameObject cubePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Crafting/CraftCube.prefab");

        [UnityTest]
        public IEnumerator CraftPosition()
        {
            var cubeHit = Object.Instantiate(cubePrefab, new Vector3(10, -0.5f, -0.5f), Quaternion.identity);
            Logger.Log($"Added cube to hit: {cubeHit}");
            return new MonoBehaviourTest<MyMonoBehaviourTest>();
        }
    }

    class MyMonoBehaviourTest : MonoBehaviour, IMonoBehaviourTest
    {
        private static readonly ILogger Logger = Debug.unityLogger;
        private int frameCount;
        public bool IsTestFinished => frameCount > 10;

        void Start()
        {
            var controller = gameObject.AddComponent<CosmoPlayerInputController>();
            Logger.Log($"added player controller {controller}");
            controller.SelectInventory(0);
            Physics.Raycast(new Ray(new Vector3(0, 0, 0), new Vector3(10, 0, 0)), out var craftRayHit);
            var craftPos = controller.GetCraftPosition(craftRayHit);
            Logger.Log($"--> craftPos={craftPos}");
        }

        void Update()
        {
            frameCount++;
        }
    }

}
