using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine.Advertisements;
using System;

public class GameManager : MonoBehaviour
{
    private const int COIN_SCORE_AMOUNT = 1;

   public static GameManager Instance { set; get; }

    private bool isGameStarted = false;
    private PlayerMotor motor;
    


    //UI and the UI fields
    public Animator gameCanvas, menuAnim, coinAnim, shopAnim;
    public Text scoreText, coinText, hiscoreText, modifierText, menuCoinText;
    private float score, coinScore, modifierScore;
    private int lastScore, menuCoinScore;

    //Shop
    public List<int> shipPrices;
    private int unlockedShips = 1;
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
    public Text deadScoreText, deadCoinText;

    private bool msbeg = true;

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

        //Advertisement
        Advertisement.Initialize("3852577");

        // GPS
        GooglePlayGames.BasicApi.PlayGamesClientConfiguration config = new GooglePlayGames.BasicApi.PlayGamesClientConfiguration.Builder().EnableSavedGames().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
        OnConnectionResponse(PlayGamesPlatform.Instance.localUser.authenticated);
        Social.localUser.Authenticate((bool success) =>
        {
            OnConnectionResponse(success);
        });

        //Shop for Pc
        menuCoinScore = PlayerPrefs.GetInt("MenuCoins");
        currentShip = PlayerPrefs.GetInt("CurrentShip");
        currentShop = PlayerPrefs.GetInt("CurrentShop");

        foreach (Transform t in shipContainer)
            t.gameObject.SetActive(false);
        shipContainer.GetChild(currentShip).gameObject.SetActive(true);
        shipContainer.GetChild(0).gameObject.SetActive(true);

        foreach (Transform t in buttonContainer)
            t.gameObject.SetActive(false);
        buttonContainer.GetChild(currentShip - 1).gameObject.SetActive(true);

        foreach (Transform t in shopShipContainer)
            t.gameObject.SetActive(false);
        shopShipContainer.GetChild(currentShip).gameObject.SetActive(true);

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
        if(isGameStarted)
        {
            
            //Bump the Score up
            score += (Time.deltaTime * modifierScore * 3);
            if(lastScore != (int)score)
            {
                lastScore = (int)score;
                scoreText.text = score.ToString("0");
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
        motor.StartRunning();
        FindObjectOfType<KameraMotor>().IsMoving = true;
        gameCanvas.SetTrigger("Show");
        menuAnim.SetTrigger("Hide");
        //flameContainer.GetChild(currentShop).gameObject.GetComponent<ParticleSystem>().enableEmission = true;
        flameContainer.GetChild(selectedShop).gameObject.SetActive(true);
        StartCoroutine(Wait());
    }

    public void GetCoin()
    {
        coinAnim.SetTrigger("Collect");
        coinScore++;
        coinText.text = coinScore.ToString("0");
        score += COIN_SCORE_AMOUNT;
        scoreText.text = score.ToString("0");
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
        gameCanvas.SetTrigger("Hide");
        deadScoreText.text = score.ToString("0");
        deadCoinText.text = coinScore.ToString("0");
        deathMenuAnim.SetTrigger("Dead");

        menuCoinScore = PlayerPrefs.GetInt("MenuCoins");
        menuCoinScore += (int) coinScore;

        reviveScore = score;

        PlayerPrefs.SetInt("MenuCoins", menuCoinScore);

        shipContainer.GetChild(PlayerPrefs.GetInt("CurrentShip")).gameObject.GetComponent<Renderer>().enabled = false;

        //flameContainer.GetChild(currentShop).gameObject.GetComponent<ParticleSystem>().enableEmission = false;
        flameContainer.GetChild(selectedShop).gameObject.SetActive(false);


        //Check if this is a Highscore
        if (score > PlayerPrefs.GetInt("Hiscore"))
        {
            float s = score;
            if (s % 1 == 0)
                s += 1;
            PlayerPrefs.SetInt("Hiscore", (int)s);

            ReportScore((int)score);
        }

        OpenSave(true);
    }

    public void RequestRevive()
    {
        ShowOptions sod = new ShowOptions();
        sod.resultCallback = Revive;

        Advertisement.Show("rewardedVideo",sod);

        reviveButton.SetActive(false);
    }

    public void Revive(ShowResult sr)
    {
        if(sr == ShowResult.Finished)
        {
            deathMenuAnim.SetTrigger("Allive");
            gameCanvas.SetTrigger("Show");
            score = reviveScore;
            shipContainer.GetChild(PlayerPrefs.GetInt("CurrentShip")).gameObject.GetComponent<Renderer>().enabled = true;
            //flameContainer.GetChild(currentShop).gameObject.GetComponent<ParticleSystem>().enableEmission = true;
            flameContainer.GetChild(selectedShop).gameObject.SetActive(true);
            motor.StartRunning();
        }
        else
        {
            OnPlayButton();
        }
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

    // Google Play Services

    private string GetSaveString()
    {
        string r = "";
        r += PlayerPrefs.GetInt("Hiscore").ToString();
        r += "|";
        r += PlayerPrefs.GetInt("MenuCoins").ToString();
        r += "|";
        r += unlockedShips.ToString();

        return r;
    }

    private void LoadSaveString(string save)
    {
        string[] data = save.Split('|');

        PlayerPrefs.SetInt("Hiscore", int.Parse(data[0]));
        PlayerPrefs.SetInt("MenuCoins", int.Parse(data[1]));
        unlockedShips = int.Parse(data[2]);

        menuCoinScore = PlayerPrefs.GetInt("MenuCoins");

        menuCoinText.text = PlayerPrefs.GetInt("MenuCoins").ToString();
        hiscoreText.text = PlayerPrefs.GetInt("Hiscore").ToString();
    }

    private void OnConnectionResponse(bool authenticated)
    {
        if (authenticated)
        {
            OpenSave(true);
            OpenSave(false);
        }
        else
        {
          
        }
    }

    //Leaderboard
    public void OnLeaderboardClick()
    {
        if(Social.localUser.authenticated)
        {
            Social.ShowLeaderboardUI();
        }
    }

    public void ReportScore(int score)
    {
        Social.ReportScore(score, GPGSIds.leaderboard_highscore, (bool success) =>
        {
        });
    }

    // Cloud Saving
    private bool isSaving = false;
    public void OpenSave(bool saving)
    {
        if(Social.localUser.authenticated)
        {
            isSaving = saving;
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution("SpaceEscaper",GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork,ConflictResolutionStrategy.UseLongestPlaytime, SaveGameOpened);
        }
    }

    private void SaveGameOpened(SavedGameRequestStatus status, ISavedGameMetadata meta)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            if(isSaving)//Writting
            {
                byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(GetSaveString());
                SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder().WithUpdatedDescription("Saved at " + DateTime.Now.ToString()).Build();

                ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(meta, update, data, SaveUpdate);
            }
            else//Reading
            {
                ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(meta, SaveRead);
            }
        }
    }

    //Success save
    private void SaveUpdate(SavedGameRequestStatus status, ISavedGameMetadata meta)
    {
        
    }

    //Load
    private void SaveRead(SavedGameRequestStatus status, byte[] data)
    {
        if(status == SavedGameRequestStatus.Success)
        {
            string saveData = System.Text.ASCIIEncoding.ASCII.GetString(data);
            LoadSaveString(saveData);
        }
    }

    private int selectedShop = 0;

    public void ShopLeft()
    {
        if(selectedShop <= 0)
        {
            selectedShop = 2    ;
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
            if (ind == PlayerPrefs.GetInt("CurrentShip"))
            {
                shopSpriteContainer.GetChild(1).gameObject.SetActive(true);
                Debug.Log(ind);
            }
            else
            {
                shopSpriteContainer.GetChild(0).gameObject.SetActive(true);
                Debug.Log(ind);
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
            //Physical change
            foreach (Transform t in shipContainer)
                t.gameObject.SetActive(false);

            shipContainer.GetChild(index).gameObject.SetActive(true);
            shipContainer.GetChild(0).gameObject.SetActive(true);

            currentShip = index;
            
            PlayerPrefs.SetInt("CurrentShip", currentShip);

            Debug.Log("Hey " + PlayerPrefs.GetInt("CurrentShip"));

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
                menuCoinScore -= shipPrices[index - 1];

                menuCoinText.text = menuCoinScore.ToString();

                PlayerPrefs.SetInt("MenuCoins", menuCoinScore);

                //Unlock in array
                unlockedShips += 1 << index;

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

                foreach (Transform t in shopSpriteContainer)
                    t.gameObject.SetActive(false);
                shopSpriteContainer.GetChild(1).gameObject.SetActive(true);


                OpenSave(true);
            }
        }
    }

}
