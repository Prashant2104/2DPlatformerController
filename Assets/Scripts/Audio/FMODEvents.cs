using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{
    public static FMODEvents instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [field: SerializeField]
    public EventReference jumpSFX { get; private set; }

    [field: SerializeField]
    public EventReference dashSFX { get; private set; }
}
