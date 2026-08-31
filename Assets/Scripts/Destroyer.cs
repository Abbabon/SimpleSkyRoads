using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Destroyable")){
            Destroy(collision.gameObject);

            GameManager.Instance.AsteroidPassed();
        }
    }
}
