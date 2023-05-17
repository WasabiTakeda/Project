using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandwormController : MonoBehaviour
{
   
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private AudioSource wormOut;
    private Transform player;

    // attached
    private Animator anim;

    // draw gizmos
    void OnDrawGizmosSelected() {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void Start() {
        anim = GetComponent<Animator>();

        player = GameObject.FindWithTag("Player").transform;
    }

    private void FixedUpdate() {
        // set attack at distance
        if (Vector2.Distance(player.position, transform.position) < 5) {
            anim.SetBool("isNear", true);
        } else {
            anim.SetBool("isNear", false);
        }
    }

    // melee attack damage trigger
    public void meleeDamagePlayer() {
        wormOut.Play();
        // check for player overlapping
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D player in hitPlayer) {
            StartCoroutine(player.GetComponent<PlayerController>().PoisonDmg());
        }
    }

}
