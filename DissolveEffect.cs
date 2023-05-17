using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DissolveEffect : MonoBehaviour
{
    
    [SerializeField] private Material material;

    private float dissolveAmount = 1f;
    private float dissolveSpeed;
    private bool isDissolving;

    private void Update() {
        if(!isDissolving) {
            dissolveAmount = Mathf.Clamp01(dissolveAmount + Time.deltaTime);
            material.SetFloat("_DissolveAmount", dissolveAmount);
        } else {
            dissolveAmount = Mathf.Clamp01(dissolveAmount - Time.deltaTime);
            material.SetFloat("_DissolveAmount", dissolveAmount);
        }
    }

    public void StartDissolve() {
        isDissolving = true;
    }

}
