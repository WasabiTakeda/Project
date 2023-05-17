using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    
    public static BossHealthUI Instance {get; private set;}

    public GameObject bossHealthObj;
    public Slider bossHealth;
    
    private void Start() {

        Instance = this;

    }

    // healt update
    public void BossHealthUpdate(float bossHP, float currentHP) {
        bossHealth.maxValue = bossHP;
        bossHealth.value = currentHP;
    }

    public void MoveIn() {
        LeanTween.moveY(bossHealthObj, 70, 1f);
    }

    public void MoveOut() {
        LeanTween.moveY(bossHealthObj, -200, 1f);
    }
    
}
