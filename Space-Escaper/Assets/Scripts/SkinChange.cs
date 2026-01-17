using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinChange : MonoBehaviour
{
    public GameObject aristocrat;
    public GameObject aristocrat2;
    public GameObject aristocrat3;

    // Start is called before the first frame update
    void Start()
    {
        aristocrat.SetActive(true);
        aristocrat2.SetActive(false);
        aristocrat3.SetActive(false);
    }

    // Update is called once per frame
    void Update() { }

    public void ChangeSkin2()
    {
        aristocrat.SetActive(false);
        aristocrat2.SetActive(true);
        aristocrat3.SetActive(false);
    }

    public void ChangeSkin1()
    {
        aristocrat.SetActive(true);
        aristocrat2.SetActive(false);
        aristocrat3.SetActive(false);
    }

    public void ChangeSkin3()
    {
        aristocrat.SetActive(false);
        aristocrat2.SetActive(false);
        aristocrat3.SetActive(true);
    }
}
