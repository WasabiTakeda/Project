using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaLockdownController : MonoBehaviour
{
    public static AreaLockdownController instance {get; private set;}
    [SerializeField] private GameObject[] spawns;
    [SerializeField] private Transform spawnPoint;
    public GameObject areaCam;
    public AudioSource areaMusic;
    private BoxCollider2D box;
    public bool hasStarted;
    private int counter = 0;
    private float next_time_spawn;
    private void Start() {
        hasStarted = false;
        box = GetComponent<BoxCollider2D>();
    }
    private void Update() {
        if (PlayerController.Instance.isPlayerDead) {
            areaMusic.Stop();
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            if (hasStarted == false) {
                hasStarted = true;
                areaCam.SetActive(true);
                areaMusic.Play();
                box.enabled = false;
                StartCoroutine(SpawnEnemy());
                StartCoroutine(LockdownTimer());
            } 
        }
    }
    // spawn enemy
    private IEnumerator SpawnEnemy() {
        yield return new WaitForSeconds(1f);
        int rand = Random.Range(0,3);
        Instantiate(spawns[rand], spawnPoint.position + new Vector3(0f,-2f,0f), Quaternion.identity);
        StartCoroutine(SpawnEnemy());
    }
    // switch lockdown state
    private IEnumerator LockdownTimer() {
        yield return new WaitForSeconds(20f);
        areaCam.SetActive(false);
        areaMusic.Stop();
        StopAllCoroutines();
    }
}
