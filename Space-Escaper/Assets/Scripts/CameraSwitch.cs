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
