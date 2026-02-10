using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private Animator anim;

    public GameObject dustVFX;

    private AudioSystem bling;

    private void Start()
    {
        bling = AudioSystem.Instance;
        anim = GetComponent<Animator>();
        Vector3 dustPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Collider coll = gameObject.GetComponent<CapsuleCollider>();
            coll.enabled = false;
            bling.PlayCoin();
            GameManager.Instance.GetCoin();
            anim.SetTrigger("Collected");
            Instantiate(dustVFX, transform.position, Quaternion.identity);
            Destroy(gameObject, 1.5f);
        }
    }
}
