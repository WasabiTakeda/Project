using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileFollowController : MonoBehaviour
{
    
    // basic  
    private Rigidbody2D rb;
    private SpriteRenderer rend;
    private GameObject player;
    [SerializeField] private AudioSource flies;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rend = GetComponent<SpriteRenderer>();

        if (player == null) {
            player = GameObject.FindWithTag("Player").gameObject;
        }

    }

    private void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("Player")) {
            Destroy(this.gameObject);
        }
    }

    void FixedUpdate() {

        if (player != null) {
            rb.position = Vector2.MoveTowards(transform.position, player.transform.position, Time.deltaTime * 10f);
            TargetedQuaternion(player.transform.position);
        }

        Destroy(this.gameObject, 1.5f);

    }

    // target rotation
    private void TargetedQuaternion(Vector3 target) {
        if ((target.x - transform.position.x) > 0) {
            transform.rotation = Quaternion.Euler(0,180,0);
        } else if ((target.x - transform.position.x) < 0) {
            transform.rotation = Quaternion.Euler(0,360,0);
        }
    }

    public void FliesSound() {
        flies.Play();
    }

}
