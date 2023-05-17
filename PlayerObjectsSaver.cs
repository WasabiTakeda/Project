using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectsSaver : MonoBehaviour
{
    public static PlayerObjectsSaver Instance {get; private set;}

    // damages
    public const string MELEE_KEY = "meleeDmg";
    public const string PROJECTILE_KEY = "projectileDmg";

    // total coins
    public const string COIN_KEY = "totalCoins";

    private void Awake()
    {

        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

}
