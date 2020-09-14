using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public float turretHealth = 50.0f;

    private float currentHealth;
    // Start is called before the first frame update and when component is enabled
    void Start()
    {
        currentHealth = turretHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Compare tag, disable script, play animation? move to next target once health is 0, let animation finish delete game object
            // similar to tank destroyed in Desert Storm
        }
    }
}
