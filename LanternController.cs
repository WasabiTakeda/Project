using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternController : MonoBehaviour
{

    private Transform player;
    private Vector3 offSet = new Vector3(-.5f,1.5f,0f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    private void Start() {

        player = GameObject.FindWithTag("Player").transform;

    }

    private void LateUpdate() {
        // targeted position
        Vector3 target = player.position + offSet;
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
    }

}
