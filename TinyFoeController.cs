using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinyFoeController : MonoBehaviour
{
    private Transform player;
    private Rigidbody2D rb;
    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
    }
    private void FixedUpdate() {
        if (!player && Vector2.Distance(transform.position, player.position) < 5f) {
            rb.velocity = new Vector2(rb.transform.right.x * 30f, 30f);
        }
    }
}
