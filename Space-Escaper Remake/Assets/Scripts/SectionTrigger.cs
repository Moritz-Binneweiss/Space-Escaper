using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class SectionTrigger : MonoBehaviour
{
    public GameObject sectionToActivate;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trigger"))
        {
            Instantiate(sectionToActivate, new Vector3(70, 80, 30), Quaternion.identity);
        }
    }
}
