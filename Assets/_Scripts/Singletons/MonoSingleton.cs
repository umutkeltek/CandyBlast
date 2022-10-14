using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T: MonoSingleton<T> // Generic singleton class to inherit from.
{
    private static volatile T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType((typeof(T))) as T;
            }
            
            return instance;
        }
    }
}