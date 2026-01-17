using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public AudioSource Explosion;
    public AudioSource Button;
    public AudioSource Engine;
    public AudioSource Coin;
    public AudioSource MenuMusic;
    public AudioSource GameMusic;

    public Button sFXButton;
    public Sprite sFXOn;
    public Sprite sFXOff;

    public Button musicButton;
    public Sprite musicOn;
    public Sprite musicOff;

    private bool isMuted;
    private bool isMusicMuted;

    void Start()
    {
        isMusicMuted = PlayerPrefs.GetInt("MUSICMUTED") == 1;
        musicButton.GetComponent<Image>().sprite = musicOn;
        isMuted = PlayerPrefs.GetInt("SFXMUTED") == 1;
        sFXButton.GetComponent<Image>().sprite = sFXOn;
        Engine.enabled = true;
        Engine.mute = true;
        Button.enabled = true;
        Explosion.enabled = true;
        Coin.enabled = true;
        MenuMusic.enabled = true;
        MenuMusic.mute = false;
        GameMusic.enabled = true;
    }

    public void MutePressed()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt("SFXMUTED", isMuted ? 1 : 0);
    }

    public void MusicMutePressed()
    {
        isMusicMuted = !isMusicMuted;
        PlayerPrefs.SetInt("MUSICMUTED", isMusicMuted ? 1 : 0);
    }

    void Update()
    {
        if (PlayerPrefs.GetInt("SFXMUTED") == 1)
        {
            sFXButton.GetComponent<Image>().sprite = sFXOn;
            Engine.enabled = true;
            Button.enabled = true;
            Explosion.enabled = true;
            Coin.enabled = true;
        }
        else
        {
            sFXButton.GetComponent<Image>().sprite = sFXOff;
            Engine.enabled = false;
            Button.enabled = false;
            Explosion.enabled = false;
            Coin.enabled = false;
        }

        if (PlayerPrefs.GetInt("MUSICMUTED") == 1)
        {
            musicButton.GetComponent<Image>().sprite = musicOn;
            MenuMusic.enabled = true;
            GameMusic.enabled = true;
        }
        else
        {
            musicButton.GetComponent<Image>().sprite = musicOff;
            MenuMusic.enabled = false;
            GameMusic.enabled = false;
        }
    }

    public void PlayExplosion()
    {
        Explosion.Play();
    }

    public void PlayButton()
    {
        Button.Play();
    }

    public void PlayEngine()
    {
        Engine.Play();
    }

    public void PlayCoin()
    {
        Coin.Play();
    }

    public void StartEngine()
    {
        Engine.mute = false;
    }

    public void StopEngine()
    {
        Engine.mute = true;
    }

    public void StartMenuMusic()
    {
        MenuMusic.mute = false;
    }

    public void StopMenuMusic()
    {
        MenuMusic.mute = true;
        GameMusic.Play();
    }
}
