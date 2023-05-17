using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceShardController : MonoBehaviour
{
    
    public bool playerHitIce = false;

    // set instance
    public static IceShardController Instance {get; private set;}

    private void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("Player")) {

        }
    }

}
