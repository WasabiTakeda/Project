using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileController : MonoBehaviour
{

    // basic  
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool rightLaunch;
    private SpriteRenderer rend;
    public float projectileSpeed = 20f;
    private GameObject player;

    public static EnemyProjectileController Instance {get; private set;}
    
    void Start()
    {
        if (rightLaunch) {
            rb.velocity = -transform.right * projectileSpeed;
        } else {
            rb.velocity = transform.right * projectileSpeed;
        }

        if (player == null) {
            player = GameObject.FindWithTag("Player").gameObject;
        }
    }

    private void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("Player")) {
            Destroy(this.gameObject);
        }
    }

    void Update() {

        // if(gameObject.name == "IceShard") {
        //     Debug.Log("ice");
        // } else if(gameObject.name == "GolemArm") {
        //     Debug.Log("arm");
        // }
        //Debug.Log(gameObject.name);

        //Destroy(this.gameObject, 2f);

    }

}
