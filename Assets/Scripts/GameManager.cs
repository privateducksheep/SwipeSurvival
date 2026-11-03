using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    //Gameobjects
    public GameObject cardGameObject;
    public CardController mainCardController;
    public SpriteRenderer cardSpriteRenderer;
    public ResourceManager resourceManager;
    //Tweaking variables (speeds are per frame at 60 fps; scaled by frame time below)
    public float fRotationSpeed;
    public float fMovingSpeed;
    public float fSideMargin;
    public float fSideTrigger;
    public float divideValue;
    public Color textColor;
    public float fRotationCoefficient;
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
    //Dragging the card
    private bool isDragging = false;
    private float pressX;
    private readonly Vector2 restPosition = new Vector2(0, 1);
    //Days Counter
    public int cardSwipeCount = 0;
    public int daysSurvived = 0;
    public TMP_Text daysSurvivedText;
    //Audio
    public AudioSource audioSource;
    public AudioClip flip;

    //The tweak values above were tuned per frame at this frame rate
    const float ReferenceFrameRate = 60f;

    /// <summary>
    /// Story cards that lead straight into a follow-up card, as indexes into
    /// resourceManager.cards. Cards whose two answers are the same
    /// (for example "Fight" / "Fight") continue whichever way you swipe.
    /// </summary>
    static readonly Dictionary<int, int> leftStoryNext = new Dictionary<int, int>
    {
        { 13, 14 }, //elderly man's bunker rumour -> zombie bunker
        { 14, 15 }, //zombie bunker -> bunker loot
        { 18, 19 }, //storm drain -> cannibal
        { 21, 22 }, //bitemark -> bitemark beg
        { 22, 23 }, //bitemark beg -> clementine joins
        { 26, 27 }, //Birden steal -> search
        { 27, 28 }, //Birden search -> child
        { 28, 29 }, //Birden child -> zombie horde
        { 29, 30 }, //Birden zombie horde -> save child
        { 31, 32 }, //alina -> supplies
    };

    static readonly Dictionary<int, int> rightStoryNext = new Dictionary<int, int>
    {
        { 14, 15 }, //zombie bunker -> bunker loot
        { 29, 30 }, //Birden zombie horde -> save child
    };

    //Wandering trader (6) leads to one of these at random
    static readonly int[] traderOutcomes = { 11, 25 };
    //Never dealt at random: death (0) and the tutorial (24)
    static readonly int[] neverDealt = { 0, 24 };

    void Start()
    {
        resourceManager.ResetAllCards();
        resourceManager.food = 50;
        resourceManager.water = 50;
        resourceManager.ammo = 50;
        resourceManager.morale = 50;
        resourceManager.medicine = 50;
        resourceManager.UpdateUI();
        UpdateDaysSurvivedUI();
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
        float cardX = cardGameObject.transform.position.x;

        //On narrow screens (phones held upright) the swipe distances shrink with
        //the view, so a swipe never needs the card dragged off the screen
        float viewHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float swipeScale = Mathf.Min(1f, viewHalfWidth * 0.45f / fSideTrigger);
        float sideTrigger = fSideTrigger * swipeScale;
        float sideMargin = fSideMargin * swipeScale;

        //Dialogue text handling
        textColor.a = Mathf.Min((Mathf.Abs(cardX) - sideMargin) / (divideValue * swipeScale), 1);
        if (Mathf.Abs(cardX) <= sideMargin)
        {
            textColor.a = 0;
        }
        UpdateDialogue();

        //Picking up the card (only when the press starts on the card itself)
        Vector2 pointer = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && mainCardController.Contains(pointer))
        {
            isDragging = true;
            isSubstituting = false;
            pressX = pointer.x;
        }

        //Letting go of the card. Checking the button state rather than the
        //release event also lets go if the release happened outside the window.
        //A press near the edge with no real drag does not count as a swipe.
        if (isDragging && !Input.GetMouseButton(0))
        {
            isDragging = false;
            bool dragged = Mathf.Abs(pointer.x - pressX) > sideMargin;
            if (dragged && cardX > sideTrigger)
            {
                Swipe(true);
                return;
            }
            if (dragged && cardX < -sideTrigger)
            {
                Swipe(false);
                return;
            }
        }

        //Movement
        float frames = Mathf.Min(Time.deltaTime, 0.05f) * ReferenceFrameRate;
        if (isDragging)
        {
            cardGameObject.transform.position = pointer;
            cardGameObject.transform.eulerAngles = new Vector3(0, 0, cardGameObject.transform.position.x * fRotationCoefficient);
        }
        else if (!isSubstituting)
        {
            cardGameObject.transform.position = Vector2.MoveTowards(cardGameObject.transform.position, restPosition, fMovingSpeed * frames);
            cardGameObject.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            //Rotating the card. The angle is tracked here rather than read back from
            //the transform, because Unity can report a 180 degree turn either way.
            currentRotation = Vector3.MoveTowards(currentRotation, cardRotation, fRotationSpeed * frames);
            cardGameObject.transform.position = restPosition;
            cardGameObject.transform.eulerAngles = currentRotation;
            if (currentRotation == cardRotation)
            {
                isSubstituting = false;
            }
        }
    }

    void Swipe(bool right)
    {
        audioSource.PlayOneShot(flip);

        //The death card only ends the run: it is not a decision, so it must not
        //apply effects or count towards the days survived
        if (currentCard == resourceManager.cards[0])
        {
            DeathSwipe();
            return;
        }

        if (right)
        {
            resourceManager.ApplyEffects(
                currentCard.rightFood,
                currentCard.rightWater,
                currentCard.rightAmmo,
                currentCard.rightMorale,
                currentCard.rightMedicine
            );
            currentCard.Right();
        }
        else
        {
            resourceManager.ApplyEffects(
                currentCard.leftFood,
                currentCard.leftWater,
                currentCard.leftAmmo,
                currentCard.leftMorale,
                currentCard.leftMedicine
            );
            currentCard.Left();
        }

        cardSwipeCount++;
        if (cardSwipeCount % 5 == 0)
        {
            daysSurvived++;
            UpdateDaysSurvivedUI();
        }

        int cardIndex = System.Array.IndexOf(resourceManager.cards, currentCard);

        //Accepting these cards unlocks later story cards
        if (!right)
        {
            if (cardIndex == 12)
                PlayerState.Instance.hasMetElderlyBunkerMan = true;
            if (cardIndex == 16)
                PlayerState.Instance.hasMetMap = true;
            if (cardIndex == 17)
                PlayerState.Instance.hasMetCorpse = true;
            if (cardIndex == 4)
                PlayerState.Instance.hasMetBirden = true;
        }

        if (CheckDeath())
            return;

        //wandering trader: trading medicine is a gamble
        if (!right && cardIndex == 6)
        {
            LoadCard(resourceManager.cards[traderOutcomes[Random.Range(0, traderOutcomes.Length)]]);
            return;
        }

        Dictionary<int, int> storyNext = right ? rightStoryNext : leftStoryNext;
        if (storyNext.TryGetValue(cardIndex, out int nextIndex))
        {
            LoadCard(resourceManager.cards[nextIndex]);
            return;
        }

        NewCard();
    }

    public void LoadCard(Card card)
    {
        cardSpriteRenderer.sprite = resourceManager.sprites[(int)card.sprite];
        leftQuote = card.leftQuote;
        rightQuote = card.rightQuote;
        currentCard = card;
        if (card.appearsOnceOnly)
        {
            card.hasBeenShown = true;
        }
        characterDialogue.text = card.dialogue;
        characterName.text = card.characterName;
        //Resetting the position of the card
        isDragging = false;
        cardGameObject.transform.position = restPosition;
        //Initialization of the substituion
        isSubstituting = true;
        currentRotation = initRotation;
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

        //Follow-up story cards only appear through their story, never at random
        HashSet<Card> excluded = new HashSet<Card>();
        foreach (int index in neverDealt)
            excluded.Add(resourceManager.cards[index]);
        foreach (int index in traderOutcomes)
            excluded.Add(resourceManager.cards[index]);
        foreach (int index in leftStoryNext.Values)
            excluded.Add(resourceManager.cards[index]);
        foreach (int index in rightStoryNext.Values)
            excluded.Add(resourceManager.cards[index]);

        List<Card> validCards = new List<Card>();

        //conditionals
        foreach (Card card in resourceManager.cards)
        {
            if (card == GameStats.prevCard || excluded.Contains(card))
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
        GameStats.RecordHighScore(daysSurvived);
        SceneManager.LoadSceneAsync("DeathScene");
    }

    void UpdateDaysSurvivedUI()
    {
        if (daysSurvivedText != null)
        {
            daysSurvivedText.text = "Days Survived : " + daysSurvived.ToString();
        }
    }


}
