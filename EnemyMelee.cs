using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee : MonoBehaviour
{

    // basics
    private Transform player;
    private Animator anim;

    // references
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1;
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private AudioSource melee;

    // draw gizmos
    void OnDrawGizmosSelected() {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    // Start is called before the first frame update
    void Start()
    {

        anim = GetComponent<Animator>();
        
        // find player by tag
        player = GameObject.FindWithTag("Player").transform;

    }

    public void PlayAudio() {
        melee.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
        // set attack at distance
        if (Vector2.Distance(player.position, transform.position) < 3 && !EnemyController.Instance.isDestroyed) {
            anim.SetTrigger("attack");
        } 

    }

    // melee attack damage trigger
    public void meleeDamagePlayer(float damage) {
        // check for player overlapping
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D player in hitPlayer) {
            player.GetComponent<PlayerController>().playerTakeMeleeDamage(damage);
        }
    }

}
