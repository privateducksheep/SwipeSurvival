using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    void Start()
    {
        resourceManager.food = 50;
        resourceManager.water = 50;
        resourceManager.ammo = 50;
        resourceManager.morale = 50;
        resourceManager.medicine = 50;
        resourceManager.UpdateUI();

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
                if (currentCard == resourceManager.cards[0])
                {
                    DeathSwipe();
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
                if (currentCard == resourceManager.cards[0])
                {
                    DeathSwipe();
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

    public void NewCard()
    {
        if (resourceManager.food == 0 ||
        resourceManager.water == 0 ||
        resourceManager.ammo == 0 ||
        resourceManager.morale == 0 ||
        resourceManager.medicine == 0)
        {
            LoadCard(resourceManager.cards[0]);
            return;
        }
        
        int rollDice = Random.Range(1, resourceManager.cards.Length);
        {
            LoadCard(resourceManager.cards[rollDice]);
        }
    }

    public void DeathSwipe()
    {
        SceneManager.LoadSceneAsync("DeathScene");
    }

}
