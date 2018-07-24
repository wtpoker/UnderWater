﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    //Stuff
	private int currentShots = 1;
    public bool projectileDestroyed = false;
    public bool playersSpawned = false;

    //Camera
    private CameraManager cam;

    //Announcer
    public Text announcer;

    //Players
    public List<GameObject> vikings;
    private int vikingIndex = 0;
    public List<GameObject> nerds;
    private int nerdIndex = 0;
    public List<GameObject> bandits;
    private int banditIndex = 0;
    private List<List<GameObject>> fractions = new List<List<GameObject>>();
    public List<string> playerTurnOrder;

    private GameObject currentPlayer;
    private int currentFraction = 0;

    //Cards(Buttons)
    public Button level2FirstCard;
    public Button level2SecondCard;
    public Button level2ThirdCard;
    public Button level3FirstCard;
    public Button level3SecondCard;

    public Sprite Level3RedCard1;
    public Sprite Level3RedCard2;
    public Sprite Level3BlueCard1;
    public Sprite Level3BlueCard2;
    public Sprite Level3YellowCard1;
    public Sprite Level3YellowCard2;

    private Image level2FirstCardImage;
    private Image level2SecondCardImage;
    private Image level2ThirdCardImage;
    private Image level3FirstCardImage;
    private Image level3SecondCardImage;

    public bool percChosen = false;

    //Win
    public GameObject win;
    public AudioClip winTheme;

    //FX
    public AudioClip switchPlayerSound;
    public AudioClip lockInSound;

    void Awake () {
        if (instance == null)
        {
            instance = this;
        } else
        {
            if (instance != this)
            {
            Destroy(gameObject);
            }
        }
        cam = GameObject.Find("Main Camera").GetComponent<CameraManager>();
        announcer = GameObject.Find("Announcer").GetComponent<Text>();

        //Cards
        level3FirstCardImage = level3FirstCard.GetComponent<Image>();
        level3SecondCardImage = level3SecondCard.GetComponent<Image>();
        level3FirstCard.onClick.AddListener((UnityEngine.Events.UnityAction)this.Level3FirstCard);
        level3SecondCard.onClick.AddListener((UnityEngine.Events.UnityAction)this.Level3SecondCard);
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(SetupGame());
	}

    private IEnumerator SetupGame()
    {
        yield return new WaitUntil(() => playersSpawned);
        Cursor.visible = true;
        announcer.gameObject.SetActive(false);
        PreparePlayers();
        StartCoroutine(SwitchPlayer());
    }

    private IEnumerator SwitchPlayer()
    {
        if (fractions[currentFraction] == vikings)
        {
            currentPlayer = fractions[currentFraction][vikingIndex];
            vikingIndex++;
            if(vikingIndex > vikings.Count-1)
            {
                vikingIndex = 0;
            }
        }
        if (fractions[currentFraction] == nerds)
        {
            currentPlayer = fractions[currentFraction][nerdIndex];
            nerdIndex++;
            if (nerdIndex > nerds.Count - 1)
            {
                nerdIndex = 0;
            }
        }
        if (fractions[currentFraction] == bandits)
        {
            currentPlayer = fractions[currentFraction][banditIndex];
            banditIndex++;
            if (banditIndex > bandits.Count - 1)
            {
                banditIndex = 0;
            }
        }
        SoundManager.PlayAudioClip(switchPlayerSound);
        cam.fullscreen = false;
        cam.player = currentPlayer;
        cam.transPlayer = true;
        yield return new WaitUntil(() => !cam.transPlayer);
        currentPlayer.GetComponent<PlayerController>().SetActive();
        percChosen = false;
    }

    public IEnumerator HasFired(Projectile projectile){
		currentShots--;
        if (currentShots <= 0){
            currentPlayer.GetComponent<PlayerController>().SetPassive();
            Debug.Log("Start Waiting");
            yield return new WaitUntil(() => projectileDestroyed);
            Debug.Log("Stop Waiting");
            projectileDestroyed = false;
            StartCoroutine(FinishPlayerTurn());
        }
	}

    private IEnumerator FinishPlayerTurn()
    {
        if (currentPlayer.GetComponent<PolygonCollider2D>().enabled)
        {
            cam.player = currentPlayer;
            cam.transPlayer = true;
            yield return new WaitUntil(() => !cam.transPlayer);

            PlayerStats CurrentPlayerStats = currentPlayer.GetComponent<PlayerStats>();
            if (CurrentPlayerStats.turnExperience > 0)
            {
                StartCoroutine(CurrentPlayerStats.AddExperience());
                yield return new WaitUntil(() => CurrentPlayerStats.finishedTurn);
            }
        }
        CheckLivingPlayers();
        StartCoroutine(SwitchPlayer());
    }

    private void CheckLivingPlayers()
    {
        for (int i = 0; i < fractions.Count; i++)
        {
            for (int j = 0; j < fractions[i].Count; j++)
            {
                if (!fractions[i][j].GetComponent<PolygonCollider2D>().enabled)
                {
                    if (fractions[i] == vikings)
                    {
                        if (fractions[i].IndexOf(fractions[i][j]) < vikingIndex)
                        {
                            vikingIndex--;
                        }
                    }
                    if (fractions[i] == nerds)
                    {
                        if (fractions[i].IndexOf(fractions[i][j]) < nerdIndex)
                        {
                            nerdIndex--;
                        }
                    }
                    if (fractions[i] == bandits)
                    {
                        if (fractions[i].IndexOf(fractions[i][j]) < banditIndex)
                        {
                            banditIndex--;
                        }
                    }
                    Debug.Log(fractions[i][j].GetComponent<PlayerStats>().fraction + " aus der SpielerListe gelöscht");
                    fractions[i].Remove(fractions[i][j]);
                }
            }
            if (fractions[i].Count == 0)
            {
                Debug.Log("Check");
                if (fractions.IndexOf(fractions[i]) <= currentFraction)
                {
                    currentFraction--;
                }
                fractions.Remove(fractions[i]);
            }
        }

        currentFraction++;
        if (currentFraction > fractions.Count-1)
        {
            currentFraction = 0;
        }
        if (fractions.Count <= 1)
        {
            win.SetActive(true);
            SoundManager.PlayAudioClip(winTheme);
            return;
        }
    }

    public void PreparePlayers()
    {
        RandomizeList(vikings);
        RandomizeList(nerds);
        RandomizeList(bandits);
        //PlayerOrder
        foreach (string fraction in playerTurnOrder)
        {
            if (fraction.Equals("Viking"))
            {
                fractions.Add(vikings);
            }
            if (fraction.Equals("Nerd"))
            {
                fractions.Add(nerds);
            }
            if (fraction.Equals("Bandit"))
            {
                fractions.Add(bandits);
            }
        }

        foreach (List<GameObject> fraction in fractions)
        {
            foreach (GameObject player in fraction)
            {
                player.GetComponent<Rigidbody2D>().isKinematic = true;
            }
        }
    }

    public void CurrentPlayerGetsExp(int exp)
    {
        currentPlayer.GetComponent<PlayerStats>().ExpGain(exp);
    }

    private void RandomizeList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    //LevelUpPercs
    public IEnumerator LevelUp()
    {
        PlayerStats playerStats = currentPlayer.GetComponent<PlayerStats>();
        if (currentPlayer.GetComponent<PlayerStats>().skillPath == "Red")
        {
            level3FirstCardImage.sprite = Level3RedCard1;
            level3SecondCardImage.sprite = Level3RedCard2;
        }

        if (currentPlayer.GetComponent<PlayerStats>().skillPath == "Blue")
        {
            level3FirstCardImage.sprite = Level3BlueCard1;
            level3SecondCardImage.sprite = Level3BlueCard2;
        }

        if (currentPlayer.GetComponent<PlayerStats>().skillPath == "Yellow")
        {
            level3FirstCardImage.sprite = Level3YellowCard1;
            level3SecondCardImage.sprite = Level3YellowCard2;
        }
        level3FirstCard.gameObject.SetActive(true);
        level3SecondCard.gameObject.SetActive(true);

        level3FirstCardImage.canvasRenderer.SetAlpha(0f);
        level3SecondCardImage.canvasRenderer.SetAlpha(0f);

        level3FirstCardImage.CrossFadeAlpha(1f, 0.2f, false);
        level3SecondCardImage.CrossFadeAlpha(1f, 0.2f, false);

        yield return new WaitForSeconds(0.2f);
        level3FirstCard.enabled = true;
        level3SecondCard.enabled = true;
        }
    }

    //Level3 First Card
    private void Level3FirstCard()
    {
        SoundManager.PlayAudioClip(lockInSound);
        StartCoroutine(DisableLevel3Cards());
        currentPlayer.GetComponent<PlayerStats>().spreadShot = true;
    }

    //Level3 Second Card
    private void Level3SecondCard()
    {
        SoundManager.PlayAudioClip(lockInSound);
        StartCoroutine(DisableLevel3Cards());
        currentPlayer.GetComponent<PlayerStats>().doubleShot = true;
    }

    private IEnumerator DisableLevel3Cards()
    {
        level3FirstCard.enabled = false;
        level3SecondCard.enabled = false;
        level3FirstCardImage.CrossFadeAlpha(0f, 0.12f, false);
        level3SecondCardImage.CrossFadeAlpha(0f, 0.12f, false);
        yield return new WaitForSeconds(0.12f);
        level3FirstCard.gameObject.SetActive(false);
        level3SecondCard.gameObject.SetActive(false);
        percChosen = true;
    }
}
