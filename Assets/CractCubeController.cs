using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

[RequireComponent(typeof(Health))]
public class CractCubeController : MonoBehaviour
{
    [Header("Death")]

    [Tooltip("vfx prefab spawned when this object dies")]
    public GameObject DeathVfx;

    [Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
    public float DeathDuration = 0f;

    Health health;

    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();
        DebugUtility.HandleErrorIfNullGetComponent<Health, CractCubeController>(health, this, gameObject);
        health.OnDie += OnDie;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDie()
    {
        // spawn a particle system when dying
        var vfx = Instantiate(DeathVfx, gameObject.transform.position, Quaternion.identity);
        Destroy(vfx, 5f);

        Destroy(gameObject, DeathDuration);
    }
}
