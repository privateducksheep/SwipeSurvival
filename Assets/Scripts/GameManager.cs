using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.AllocatorManager;


public class GameManager : MonoBehaviour
{
    //Gameobjects
    public GameObject cardGameObject;
    public CardController mainCardController;
    public SpriteRenderer cardSpriteRenderer;
    public ResourceManager resourceManager;
    //Tweaking variables
    public float fRotationSpeed;
    public float fMovingSpeed;
    public float fSideMargin;
    public float fSideTrigger;
    public float divideValue;
    float alphaText;
    public Color textColor;
    public float fRotationCoefficient;
    Vector3 pos;
    //UI
    public TMP_Text characterDialogue;
    public TMP_Text actionQuote;
    public TMP_Text characterName;
    //Card variables
    private string leftQuote;
    private string rightQuote;
    public Card currentCard;
    public Card testCard;
    //Substituting the card
    public bool isSubstituting = false;
    public Vector3 cardRotation; //default
    public Vector3 currentRotation; //current rotation of card
    public Vector3 initRotation; //initial rotation of the card
    //Days Counter
    public int cardSwipeCount = 0;
    public int daysSurvived = 0;
    public TMP_Text daysSurvivedText;
    private int lastScreenWidth;
    private int lastScreenHeight;
    //Audio
    public AudioSource audioSource;
    public AudioClip flip;

    void Start()
    {
        UpdateResponsiveLayout();
        resourceManager.ResetAllCards();
        resourceManager.food = 50;
        resourceManager.water = 50;
        resourceManager.ammo = 50;
        resourceManager.morale = 50;
        resourceManager.medicine = 50;
        resourceManager.UpdateUI();
        GameStats.highScore = PlayerPrefs.GetInt("HighScore", 0);



        LoadCard(testCard);
    }

    void UpdateDialogue()
    {
        actionQuote.color = textColor;
        if (cardGameObject.transform.position.x < 0)
        {
            actionQuote.text = leftQuote;
        }
        else
        {
            actionQuote.text = rightQuote;
        }
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateResponsiveLayout();
        }

        //Dialogue text handling
        textColor.a = Mathf.Min((Mathf.Abs(cardGameObject.transform.position.x) - fSideMargin) / divideValue, 1);
        if (cardGameObject.transform.position.x > fSideTrigger)
        {

            if (Input.GetMouseButtonUp(0))
            {
                resourceManager.ApplyEffects(
                    currentCard.rightFood,
                    currentCard.rightWater,
                    currentCard.rightAmmo,
                    currentCard.rightMorale,
                    currentCard.rightMedicine
                );
                currentCard.Right();

                cardSwipeCount++;
                if (cardSwipeCount % 5 == 0)
                {
                    daysSurvived++;
                    UpdateDaysSurvivedUI();
                }

                audioSource.PlayOneShot(flip);

                if (currentCard == resourceManager.cards[14])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[15]);
                    return;
                }
                if (currentCard == resourceManager.cards[0])
                {
                    DeathSwipe();
                    return;
                }

                else
                {
                    NewCard();
                }
            }
        }
        else if (cardGameObject.transform.position.x > fSideMargin)
        {

        }

        else if (cardGameObject.transform.position.x > -fSideMargin)
        {
            textColor.a = 0;

        }
        else if (cardGameObject.transform.position.x > -fSideTrigger)
        {

        }
        else
        {

            if (Input.GetMouseButtonUp(0))
            {
                resourceManager.ApplyEffects(
                    currentCard.leftFood,
                    currentCard.leftWater,
                    currentCard.leftAmmo,
                    currentCard.leftMorale,
                    currentCard.leftMedicine
                );
                currentCard.Left();

                cardSwipeCount++;
                if (cardSwipeCount % 5 == 0)
                {
                    daysSurvived++;
                    UpdateDaysSurvivedUI();
                }

                audioSource.PlayOneShot(flip);

                if (currentCard == resourceManager.cards[12])
                {
                    PlayerState.Instance.hasMetElderlyBunkerMan = true;
                    NewCard();
                    return;
                }
                if (currentCard == resourceManager.cards[16])
                {
                    PlayerState.Instance.hasMetMap = true;
                    NewCard();
                    return;
                }
                if (currentCard == resourceManager.cards[17])
                {
                    PlayerState.Instance.hasMetCorpse = true;
                    NewCard();
                    return;
                }
                if (currentCard == resourceManager.cards[4])
                {
                    PlayerState.Instance.hasMetBirden = true;
                    NewCard();
                    return;
                }

                if (currentCard == resourceManager.cards[13])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[14]);
                    return;
                }

                if (currentCard == resourceManager.cards[14])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[15]);
                    return;
                }

                if (currentCard == resourceManager.cards[18])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[19]);
                    return;
                }

                //bitemark -> bitemark beg
                if (currentCard == resourceManager.cards[21])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[22]);
                    return;
                }

                //bitemark beg -> clementine joins
                if (currentCard == resourceManager.cards[22])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[23]);
                    return;
                }

                //Birden steal -> search
                if (currentCard == resourceManager.cards[26])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[27]);
                    return;
                }

                //Birden search -> Child
                if (currentCard == resourceManager.cards[27])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[28]);
                    return;
                }

                //birden child -> zombie horde
                if (currentCard == resourceManager.cards[28])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[29]);
                    return;
                }

                //birden zombie -> save child
                if (currentCard == resourceManager.cards[29])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[30]);
                    return;
                }

                //alina -> supplies
                if (currentCard == resourceManager.cards[31])
                {
                    if (CheckDeath())
                        return;
                    LoadCard(resourceManager.cards[32]);
                    return;
                }

                //wandering trader
                if (currentCard == resourceManager.cards[6])
                {
                    if (CheckDeath())
                        return;

                    int roll = Random.Range(0, 2); // returns 0 or 1

                    if (roll == 0)
                        LoadCard(resourceManager.cards[11]);
                    else
                        LoadCard(resourceManager.cards[25]);

                    return;
                }

                if (currentCard == resourceManager.cards[0])
                {
                    DeathSwipe();
                    return;
                }

                else 
                {
                    NewCard();
                }
            }
        }

                UpdateDialogue();
        //Movement
        if (Input.GetMouseButton(0) && mainCardController.isMouseOver)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cardGameObject.transform.position = pos;
            cardGameObject.transform.eulerAngles = new Vector3(0, 0, cardGameObject.transform.position.x * fRotationCoefficient);
        }
        else if (!isSubstituting)
        {
            cardGameObject.transform.position = Vector2.MoveTowards(cardGameObject.transform.position, new Vector2(0, 1), fMovingSpeed);
            cardGameObject.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (isSubstituting)
        {
            cardGameObject.transform.eulerAngles = Vector3.MoveTowards(cardGameObject.transform.eulerAngles, cardRotation, fRotationSpeed);
        }
        //UI
        //display.text = "" + textColor.a;

        //Rotating the card
        if (cardGameObject.transform.eulerAngles == cardRotation)
        {
            isSubstituting = false;
        }
    }

    public void LoadCard(Card card)
    {
        cardSpriteRenderer.sprite = resourceManager.sprites[(int)card.sprite];
        leftQuote = card.leftQuote;
        rightQuote = card.rightQuote;
        currentCard = card;
        characterDialogue.text = card.dialogue;
        characterName.text = card.characterName;
        //Resetting the position of the card
        cardGameObject.transform.position = new Vector2(0, 1);
        cardGameObject.transform.eulerAngles = new Vector3(0, 0, 0);
        //Initialization of the substituion
        isSubstituting = true;

        cardGameObject.transform.eulerAngles = initRotation;
    }

    public bool CheckDeath()
    {
        if (resourceManager.food == 0 ||
            resourceManager.water == 0 ||
            resourceManager.ammo == 0 ||
            resourceManager.morale == 0 ||
            resourceManager.medicine == 0)
        {
            LoadCard(resourceManager.cards[0]);
            return true;
        }
        return false;
    }


    public void NewCard()
    {
        if (CheckDeath())
            return;

        List<Card> validCards = new List<Card>();


        //conditionals
        foreach (Card card in resourceManager.cards)
        {
            if (card == GameStats.prevCard || card == resourceManager.cards[0] || card == resourceManager.cards[14] || card == resourceManager.cards[15] || card == resourceManager.cards[19] || card == resourceManager.cards[11] || card == resourceManager.cards[24] || card == resourceManager.cards[25] || card == resourceManager.cards[27] || card == resourceManager.cards[28] || card == resourceManager.cards[29] || card == resourceManager.cards[30] || card == resourceManager.cards[32]) 
                continue;

            if (card.appearsOnceOnly && card.hasBeenShown)
                continue;
            if (card.requiresElderlyBunkerMan && !PlayerState.Instance.hasMetElderlyBunkerMan)
                continue;
            if (card.requiresMap && !PlayerState.Instance.hasMetMap)
                continue;
            if (card.requiresCorpse && !PlayerState.Instance.hasMetCorpse)
                continue;
            if (card.requiresBitemark && !PlayerState.Instance.hasMetBitemark)
                continue;
            if (card.requiresBitemarkBeg && !PlayerState.Instance.hasMetBitemarkBeg)
                continue;
            if (card.requiresBirden && !PlayerState.Instance.hasMetBirden)
                continue;


            validCards.Add(card);
        }

        if (validCards.Count > 0)
        {
            Card next = validCards[Random.Range(0, validCards.Count)];

            GameStats.prevCard = next; 

            if (next.appearsOnceOnly)
            {
                next.hasBeenShown = true;
            }
            LoadCard(next);
        }

        else
        {
            LoadCard(resourceManager.cards[0]);
        }
    }

    public void DeathSwipe()
    {
        GameStats.daysSurvived = daysSurvived;
        SceneManager.LoadSceneAsync("DeathScene");

        if (GameStats.daysSurvived > GameStats.highScore)
        {
            GameStats.highScore = GameStats.daysSurvived;
            PlayerPrefs.SetInt("HighScore", GameStats.highScore);
            PlayerPrefs.Save();
        }
    }

    void UpdateDaysSurvivedUI()
    {
        if (daysSurvivedText != null)
        {
            daysSurvivedText.text = "Days Survived : " + daysSurvived.ToString();
        }
    }

    /// <summary>
    /// Keeps the world-space day counter inside the camera on every aspect ratio.
    /// The rest of the HUD is handled by the Canvas Scaler, but this label is a
    /// TextMeshPro object attached to the gameplay hierarchy rather than Canvas UI.
    /// </summary>
    void UpdateResponsiveLayout()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (daysSurvivedText == null)
            return;

        Camera gameplayCamera = Camera.main;
        if (gameplayCamera == null || !gameplayCamera.orthographic)
            return;

        RectTransform dayCounter = daysSurvivedText.rectTransform;
        dayCounter.pivot = new Vector2(0f, 1f);

        float distanceFromCamera = Mathf.Abs(
            gameplayCamera.transform.position.z - dayCounter.position.z
        );
        Vector3 safeTopLeft = gameplayCamera.ViewportToWorldPoint(
            new Vector3(0.025f, 0.95f, distanceFromCamera)
        );

        dayCounter.position = new Vector3(
            safeTopLeft.x,
            safeTopLeft.y,
            dayCounter.position.z
        );
    }


}
