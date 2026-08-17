using UnityEngine;

[CreateAssetMenu]
public class Card : ScriptableObject
{

    public int cardID;
    public string cardName;
    public CardSprite sprite;
    public string dialogue;
    public string leftQuote;
    public string rightQuote;
    public string characterName;
    public int leftFood;
    public int leftWater;
    public int leftAmmo;
    public int leftMorale;
    public int leftMedicine;

    public int rightFood;
    public int rightWater;
    public int rightAmmo;
    public int rightMorale;
    public int rightMedicine;

    //Conditionals
    public bool hasBeenShown = false;
    public bool appearsOnceOnly;
    public bool requiresElderlyBunkerMan;
    public bool requiresMap;
    public bool requiresCorpse;
    public bool requiresBitemark;
    public bool requiresBitemarkBeg;
    public bool requiresBirden;




    public void Left()
    {
        Debug.Log(cardName + " swiped left");
    }
    public void Right()
    {
        Debug.Log(cardName + " swiped right");
    }
}
