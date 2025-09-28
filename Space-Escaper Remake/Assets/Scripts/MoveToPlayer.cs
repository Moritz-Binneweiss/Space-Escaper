using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveToPlayer : MonoBehaviour
{
    void Update()
    {
        transform.position += new Vector3(0, 0, -20) * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Destroy"))
        {
            Destroy(gameObject);
        }
    }
}
