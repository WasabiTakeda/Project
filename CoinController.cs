using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AudioSource coinPickup;

    private int totalCoins;

    private void Start() {

        // pop up speed
        rb.velocity = transform.up * 4f;
        totalCoins = PlayerPrefs.GetInt(PlayerObjectsSaver.COIN_KEY, 0);

    }

    private void OnCollisionEnter2D(Collision2D collision) {
        // player...
        if (collision.gameObject.CompareTag("Player")) {
            coinPickup.Play();
            totalCoins++;
            PlayerPrefs.SetInt(PlayerObjectsSaver.COIN_KEY, totalCoins);

            Destroy(this.gameObject, 0.05f);
        }
    }

}
