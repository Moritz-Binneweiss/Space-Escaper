using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public GameObject maincam;
    public GameObject shopcam;

    // Start is called before the first frame update
    void Start()
    {
        maincam.SetActive(true);
        shopcam.SetActive(false);
    }

    // Update is called once per frame
    void Update() { }

    public void ShopCamera()
    {
        maincam.SetActive(false);
        shopcam.SetActive(true);
    }

    public void MainCamera()
    {
        maincam.SetActive(true);
        shopcam.SetActive(false);
    }
}
