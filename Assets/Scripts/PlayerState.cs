using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    public bool hasMetElderlyBunkerMan = false;
    public bool hasMetMap = false;
    public bool hasMetCorpse = false;
    public bool hasMetBitemark = false;
    public bool hasMetBitemarkBeg = false;
    public bool hasMetBirden = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(gameObject);  
        }
    }
}

