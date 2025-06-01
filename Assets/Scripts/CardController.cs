using UnityEngine;

public class CardController : MonoBehaviour
{
    public Card card;
    public BoxCollider2D thisCard;
    public bool isMouseOver;

    private void Start()
    {
        thisCard = gameObject.GetComponent<BoxCollider2D>();
    }
    private void OnMouseOver()
    {
        isMouseOver = true;
    }
    private void OnMouseExit()
    {
        isMouseOver = false;
    }
    
}

public enum CardSprite
{
    DEATH,
    MAN,
    WOMAN,
    ZOMBIE,
    WANDERINGTRADER,
    CAMPFIRESONGS,
    MYSTERIOUSILLNESS,
    GUNSHOTS,
    AMBULANCE,
    DESPERATEMOTHER,
    NIGHTWATCH,
    ABANDONEDBUNKER,
    FERALDOG,
    SCAVENGERSJOURNAL,
    RAINSTORM,
    RADIOBROADCAST,
    SICKCHILD,
    BARRICADEREPAIRS,
    ROGUETRADER,
    FLASHFLOOD,
    OLDPHOTOGRAPHS,
    BOOBYTRAPPEDSUPPLYBOX,
    FRIENDLYSTRANGER,
    WATERRATION,
    RUINEDGROCERYSTORE,
    CRACKEDRESERVOIR,
    WHISPERER
}