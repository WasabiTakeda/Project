using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRange : MonoBehaviour
{
    
    // basics
    private Transform player;
    private Animator anim;
    public bool turnRight;

    private Vector3 previousPosition;
    private Vector3 lastPosition;

    private Vector3 goToPosition;


    // references
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectile;

    // audio sources
    [SerializeField] private AudioSource shoot;

    float next_time_shoot;

    public static EnemyRange Instance {get; private set;}

    void Start()
    {

        next_time_shoot = Time.time + 2.0f;

        anim = GetComponent<Animator>();
        
        // find player by tag
        player = GameObject.FindWithTag("Player").transform;

    }

    public void PlayAudio() {
        shoot.Play();
    }

    void Update()
    {

        // set attack at distance
        if (Vector2.Distance(player.position, transform.position) < 5 && !EnemyController.Instance.isDestroyed) {
            if (Time.time > next_time_shoot) {
                anim.SetTrigger("shoot");
                next_time_shoot += 2.0f;
            }
        }   

        if(isRight()) {
            turnRight = true;
        } else {
            turnRight = false;
        }
    
    }

    // return angle on direction
    public bool isRight() {
        if (transform.position != previousPosition) {
            lastPosition = (transform.position - previousPosition).normalized;
            previousPosition = transform.position;
        }

        bool isRight = (lastPosition.x > .1f) ? true : false;

        return isRight;
    }

    public void Shoot() {
        // shoot ...
        GameObject enemyProjectile = Instantiate(projectile, shootPoint.transform.position, Quaternion.identity);
        enemyProjectile.transform.right = transform.right.normalized;
    }

}
