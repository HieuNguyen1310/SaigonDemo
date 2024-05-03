using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerDynamic : MonoBehaviour
{
    
    

    [SerializeField] UnityEvent onTriggerEnter;

    [SerializeField] UnityEvent onTriggerExit;

    void OnTriggerEnter(Collider other) 
    {
        onTriggerEnter.Invoke();
    }

    void OnTriggerExit(Collider other) 
    {
        onTriggerExit.Invoke();
    }
}
