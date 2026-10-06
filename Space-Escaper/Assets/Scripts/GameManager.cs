using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const int COIN_SCORE_AMOUNT = 1;
    private const int STARTER_SHIP = 1;

    public static GameManager Instance { set; get; }

    private bool isGameStarted = false;
    private PlayerMotor motor;

    //UI and the UI fields
    public Animator gameCanvas,
        menuAnim,
        coinAnim,
        shopAnim;
    public Text scoreText,
        coinText,
        hiscoreText,
        modifierText,
        menuCoinText;
    private float score,
        coinScore,
        modifierScore;
    private int lastScore,
        menuCoinScore;

    // Coins of this run already added to MenuCoins. A revive keeps coinScore
    // running, so a second death may only bank what was collected since.
    private int bankedCoins;

    //Shop
    public List<int> shipPrices;
    private int unlockedShips;
    private int currentShip = 0;
    private int currentShop = 0;
    public Transform shipContainer;
    public Transform buttonContainer;
    public Transform shopShipContainer;
    public Transform shopSpriteContainer;
    public Transform skinButtonContainer;
    public Transform flameContainer;
    public GameObject spaceport;
    public GameObject hangar;

    private float reviveScore;
    public GameObject reviveButton;

    //Deathmenu
    public Animator deathMenuAnim;
    public Text deadScoreText,
        deadCoinText;

    private void Awake()
    {
        Time.timeScale = 1f;
        Instance = this;
        modifierScore = 1f;
        motor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();

        modifierText.text = "x" + modifierScore.ToString("0.0");
        coinText.text = coinScore.ToString("0");
        scoreText.text = score.ToString("0");

        menuCoinText.text = PlayerPrefs.GetInt("MenuCoins").ToString();

        hiscoreText.text = PlayerPrefs.GetInt("Hiscore").ToString();

        //Shop for Pc
        menuCoinScore = PlayerPrefs.GetInt("MenuCoins");
        currentShip = PlayerPrefs.GetInt("CurrentShip", STARTER_SHIP);
        currentShop = PlayerPrefs.GetInt("CurrentShop");

        // Child 0 of shipContainer is the flame container, not a ship. A missing
        // or broken value used to land there: no visible ship, and GetChild(-1)
        // below threw on every fresh install.
        if (currentShip < STARTER_SHIP || currentShip >= shipContainer.childCount)
        {
            currentShip = STARTER_SHIP;
            currentShop = 0;
        }
        selectedShop = currentShop;

        // The starter ship is always owned, and so is the ship being flown. Written
        // back right away, otherwise switching ships would lock the old one again.
        unlockedShips = PlayerPrefs.GetInt("UnlockedShips") | 1 << STARTER_SHIP | 1 << currentShip;
        PlayerPrefs.SetInt("UnlockedShips", unlockedShips);

        foreach (Transform t in shipContainer)
            t.gameObject.SetActive(false);
        shipContainer.GetChild(currentShip).gameObject.SetActive(true);
        shipContainer.GetChild(0).gameObject.SetActive(true);

        foreach (Transform t in buttonContainer)
            t.gameObject.SetActive(false);
        buttonContainer.GetChild(currentShip - 1).gameObject.SetActive(true);

        foreach (Transform t in shopShipContainer)
            t.gameObject.SetActive(false);
        shopShipContainer.GetChild(currentShip - 1).gameObject.SetActive(true);

        foreach (Transform t in shopSpriteContainer)
            t.gameObject.SetActive(false);
        shopSpriteContainer.GetChild(1).gameObject.SetActive(true);

        foreach (Transform t in skinButtonContainer)
            t.gameObject.SetActive(false);
        skinButtonContainer.GetChild(currentShop).gameObject.SetActive(true);

        foreach (Transform t in flameContainer)
            t.gameObject.SetActive(false);
        //flameContainer.GetChild(currentShop).gameObject.SetActive(true);
        //flameContainer.GetChild(currentShop).gameObject.GetComponent<ParticleSystem>().enableEmission = false;

        reviveButton.SetActive(true);
        spaceport.SetActive(true);
        hangar.SetActive(false);
    }

    private void Update()
    {
        if (isGameStarted)
        {
            //Bump the Score up
            score += (Time.deltaTime * modifierScore * 3);
            if (lastScore != (int)score)
            {
                lastScore = (int)score;
                scoreText.text = lastScore.ToString();
            }
        }

        //if (msbeg == true)
        //{
        //   menuCoinText.text = PlayerPrefs.GetInt("MenuCoins").ToString();
        //   hiscoreText.text = PlayerPrefs.GetInt("Hiscore").ToString();

        // }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3);
        spaceport.SetActive(false);
    }

    public void Play()
    {
        isGameStarted = true;
        AudioSystem.Instance.PlayGameMusic();
        motor.StartRunning();
        FindAnyObjectByType<CameraMotor>().IsMoving = true;
        gameCanvas.SetTrigger("Show");
        menuAnim.SetTrigger("Hide");
        //flameContainer.GetChild(currentShop).gameObject.GetComponent<ParticleSystem>().enableEmission = true;
        flameContainer.GetChild(currentShop).gameObject.SetActive(true);
        StartCoroutine(Wait());
    }

    public void GetCoin()
    {
        coinAnim.SetTrigger("Collect");
        coinScore++;
        coinText.text = coinScore.ToString("0");
        score += COIN_SCORE_AMOUNT;
        scoreText.text = ((int)score).ToString();
    }

    public void UpdateModifier(float modifierAmount)
    {
        modifierScore = 1.0f + modifierAmount;
        modifierText.text = "x" + modifierScore.ToString("0.0");
    }

    public void OnPlayButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void OnDeath()
    {
        // Shown and saved as the same whole number. The death screen used to
        // round while the highscore was truncated: 41.7 showed 42 but saved 41.
        int finalScore = (int)score;

        gameCanvas.SetTrigger("Hide");
        deadScoreText.text = finalScore.ToString();
        deadCoinText.text = coinScore.ToString("0");
        deathMenuAnim.SetTrigger("Dead");

        // Paused, not stopped, so a revive continues the track where it was.
        // Going back to the menu reloads the scene, which starts the menu music.
        AudioSystem.Instance.PauseMusic();

        int runCoins = (int)coinScore;
        menuCoinScore = PlayerPrefs.GetInt("MenuCoins") + runCoins - bankedCoins;
        bankedCoins = runCoins;

        reviveScore = score;

        PlayerPrefs.SetInt("MenuCoins", menuCoinScore);

        shipContainer.GetChild(currentShip).gameObject.GetComponent<Renderer>().enabled = false;

        //flameContainer.GetChild(currentShop).gameObject.GetComponent<ParticleSystem>().enableEmission = false;
        flameContainer.GetChild(currentShop).gameObject.SetActive(false);

        //Check if this is a Highscore
        if (finalScore > PlayerPrefs.GetInt("Hiscore"))
            PlayerPrefs.SetInt("Hiscore", finalScore);
    }

    public void RequestRevive()
    {
        reviveButton.SetActive(false);
        Revive();
    }

    public void Revive()
    {
        deathMenuAnim.SetTrigger("Allive");
        gameCanvas.SetTrigger("Show");
        score = reviveScore;
        shipContainer.GetChild(currentShip).gameObject.GetComponent<Renderer>().enabled = true;
        flameContainer.GetChild(currentShop).gameObject.SetActive(true);
        motor.StartRunning();
        AudioSystem.Instance.ResumeMusic();
    }

    public void ShopOn()
    {
        menuAnim.SetTrigger("Hide");
        shopAnim.SetTrigger("Show");
        hangar.SetActive(true);
    }

    public void ShopOff()
    {
        menuAnim.SetTrigger("Show");
        shopAnim.SetTrigger("Hide");
        hangar.SetActive(false);
    }

    private int selectedShop = 0;

    public void ShopLeft()
    {
        if (selectedShop <= 0)
        {
            selectedShop = 2;
        }
        else
        {
            selectedShop -= 1;
        }
        SelectShipModel();
    }

    public void ShopRight()
    {
        if (selectedShop >= 2)
        {
            selectedShop = 0;
        }
        else
        {
            selectedShop += 1;
        }
        SelectShipModel();
    }

    void SelectShipModel()
    {
        int i = 0;
        foreach (Transform t in skinButtonContainer)
        {
            if (i == selectedShop)
                t.gameObject.SetActive(true);
            else
                t.gameObject.SetActive(false);
            i++;
        }

        int[] shops = new int[3];
        shops[0] = 1;
        shops[1] = 4;
        shops[2] = 7;

        SetShopMenu(shops[selectedShop]);
    }

    public void SetShopMenu(int ind)
    {
        foreach (Transform t in buttonContainer)
            t.gameObject.SetActive(false);

        buttonContainer.GetChild(ind - 1).gameObject.SetActive(true);

        foreach (Transform t in shopShipContainer)
            t.gameObject.SetActive(false);

        shopShipContainer.GetChild(ind - 1).gameObject.SetActive(true);

        foreach (Transform t in shopSpriteContainer)
            t.gameObject.SetActive(false);
        //if unlocked already
        if ((unlockedShips & 1 << ind) == 1 << ind)
        {
            if (ind == currentShip)
            {
                shopSpriteContainer.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                shopSpriteContainer.GetChild(0).gameObject.SetActive(true);
            }
        }
        else
        {
            shopSpriteContainer.GetChild(ind + 1).gameObject.SetActive(true);
        }
    }

    public void TryBuyingShip(int index)
    {
        //if unlocked already
        if ((unlockedShips & 1 << index) == 1 << index)
        {
            AudioSystem.Instance.PlaySelectShip();

            //Physical change
            foreach (Transform t in shipContainer)
                t.gameObject.SetActive(false);

            shipContainer.GetChild(index).gameObject.SetActive(true);
            shipContainer.GetChild(0).gameObject.SetActive(true);

            currentShip = index;

            PlayerPrefs.SetInt("CurrentShip", currentShip);

            currentShop = selectedShop;
            PlayerPrefs.SetInt("CurrentShop", currentShop);

            foreach (Transform t in shopSpriteContainer)
                t.gameObject.SetActive(false);
            shopSpriteContainer.GetChild(1).gameObject.SetActive(true);

            foreach (Transform t in flameContainer)
                t.gameObject.SetActive(false);
            //flameContainer.GetChild(selectedShop).gameObject.SetActive(true);
            //flameContainer.GetChild(selectedShop).gameObject.GetComponent<ParticleSystem>().enableEmission = false;
        }
        else
        {
            if (menuCoinScore >= shipPrices[index - 1])
            {
                AudioSystem.Instance.PlayBuyShip();

                menuCoinScore -= shipPrices[index - 1];

                menuCoinText.text = menuCoinScore.ToString();

                PlayerPrefs.SetInt("MenuCoins", menuCoinScore);

                //Unlock in array
                unlockedShips |= 1 << index;
                PlayerPrefs.SetInt("UnlockedShips", unlockedShips);

                //Physical Change
                foreach (Transform t in shipContainer)
                    t.gameObject.SetActive(false);

                //if (index == 0)
                //return;

                shipContainer.GetChild(index).gameObject.SetActive(true);
                shipContainer.GetChild(0).gameObject.SetActive(true);

                foreach (Transform t in flameContainer)
                    t.gameObject.SetActive(false);
                //flameContainer.GetChild(selectedShop).gameObject.SetActive(true);
                //flameContainer.GetChild(selectedShop).gameObject.GetComponent<ParticleSystem>().enableEmission = false;

                currentShip = index;

                PlayerPrefs.SetInt("CurrentShip", currentShip);

                currentShop = selectedShop;
                PlayerPrefs.SetInt("CurrentShop", currentShop);

                // Coins were spent - write to disk now instead of waiting for
                // the app to be paused or closed.
                PlayerPrefs.Save();

                foreach (Transform t in shopSpriteContainer)
                    t.gameObject.SetActive(false);
                shopSpriteContainer.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
}
