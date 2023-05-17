using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{

    // objects
    // projectile system
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private GameObject projectile;
    private Transform player;
    private float fireProjectileDistance = 2f;
    private float instantiateTimer;
    private float timeOutDuration = 2f;
    private float projectileLifetime = 1f;

    void Start() 
    {
        player = GameObject.FindWithTag("Player").transform;
        instantiateTimer = timeOutDuration;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // check for the projectile fire distance...
        if (Vector2.Distance(transform.position, player.position) < fireProjectileDistance) {
            // shoot...
            DoShoot();
        }
    }

    // shoot func
    void DoShoot() {
        instantiateTimer -= Time.deltaTime;
        if (instantiateTimer <= 0) {
            shoot();
            instantiateTimer = timeOutDuration;
        }
    }
    void shoot() {
        var destroyAfter = (GameObject) Instantiate(projectile, projectilePoint.position, projectilePoint.rotation);
        Destroy(destroyAfter, projectileLifetime);
    }

}
