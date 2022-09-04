using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Cosmocraft
{

    [RequireComponent(typeof(Health))]
    public class DeathToExplosion : MonoBehaviour
    {
        [Header("Death")] [Tooltip("vfx prefab spawned when the enemy dies")]
        public GameObject DeathVfx;

        [Tooltip("The point at which the death VFX is spawned")]
        public Transform DeathVfxSpawnPoint;

        [Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
        public float DeathDuration = 0f;

        Health health;

        void Start()
        {
            health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, DeathToExplosion>(health, this, gameObject);
            health.OnDie += OnDie;

            // TODO loading prefab at runtime is slower and also does not simply work with the given path
            //DeathVfx ??= Resources.Load("Assets/FPS/Prefabs/VFX/VFX_Exploration") as GameObject;

            DeathVfxSpawnPoint = transform;
            // TODO how to test for reference not assigned?
            //DeathVfxSpawnPoint = ReferenceEquals(DeathVfxSpawnPoint, null) ? transform : DeathVfxSpawnPoint;
            //DeathVfxSpawnPoint ??= transform;
        }

        void Update()
        {

        }

        void OnDie()
        {
            // spawn a particle system when dying
            var vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
            Destroy(vfx, 5f);

            Destroy(gameObject, DeathDuration);
        }
    }

}