using UnityEngine;

public class CardController : MonoBehaviour
{
    public Card card;
    public BoxCollider2D thisCard;

    private void Awake()
    {
        thisCard = gameObject.GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// True if a world point is on the card. Syncs physics first, so it is
    /// correct even on the frame the card was moved.
    /// </summary>
    public bool Contains(Vector2 worldPoint)
    {
        Physics2D.SyncTransforms();
        return thisCard.OverlapPoint(worldPoint);
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
    WHISPERER,
    ELDERLYMAN,
    ELDERLYBUNKERMAN,
    ZOMBIEBUNKER,
    BUNKERLOOT,
    MAP,
    CORPSE,
    DRAIN,
    CANNIBAL,
    CLEMENTINE,
    TEXTCARD,
    TUTORIAL,
    BIRDENCHILD,
    ALINA
}