using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    // objects
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 0f;
    [SerializeField] private GameObject projectileImpactEffect;
    public AudioSource impactAudio;
    private Transform player;
    private float destroyDistance = 20f;
    
    void Start()
    {
        // projectile speed...
        rb.velocity = transform.right * speed;
        player = GameObject.FindWithTag("Player").transform;
    }

    void FixedUpdate() {

        float angulo = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
        
        // destroy stray projectiles...
        if (Vector2.Distance(transform.position, player.position) > destroyDistance) {
            Destroy(this.gameObject);
        }

    }

    // check if collided object references...
    public void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("Enemy") || coll.gameObject.CompareTag("Boss") || coll.gameObject.layer == LayerMask.NameToLayer("Terrain")) {
            impactAudio.Play();
            // impact effect
            GameObject impactPref = Instantiate(projectileImpactEffect, transform.position + new Vector3(0.5f, 0.5f, 0f), Quaternion.identity);
            Destroy(impactPref, 1f);
            Destroy(this.gameObject, .3f);
        }
    }
}
