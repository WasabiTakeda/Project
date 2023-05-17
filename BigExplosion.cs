using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigExplosion : MonoBehaviour
{
    [SerializeField] private Transform explosionPoint;
    [SerializeField] private float explosionRadius;
    [SerializeField] private LayerMask enemyLayer;
    
    // draw gizmos
    void OnDrawGizmosSelected() {
        if (explosionPoint == null) return;
        Gizmos.DrawWireSphere(explosionPoint.position, explosionRadius);
    }
    
    public void Explode() {
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(explosionPoint.position, explosionRadius, enemyLayer);
        foreach(Collider2D enemy in overlaps) {
            enemy.GetComponent<EnemyController>().EnemyTakeExplosionDamage(1f);
        }
    }

}
