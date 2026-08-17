using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public Sprite[] sprites;
    public Card[] cards;
    public int food;
    public int water;
    public int ammo;
    public int morale;
    public int medicine;

    // UI text fields
    public TMP_Text foodValue;
    public TMP_Text waterValue;
    public TMP_Text ammoValue;
    public TMP_Text moraleValue;
    public TMP_Text medicineValue;

    public void UpdateUI()
    {
        foodValue.text = food.ToString() + "%"; ;
        waterValue.text = water.ToString() + "%"; ;
        ammoValue.text = ammo.ToString() + "%"; ;
        moraleValue.text = morale.ToString() + "%"; ;
        medicineValue.text = medicine.ToString() + "%"; ;
    }

    public void ResetAllCards()
    {
        foreach (Card card in cards)
        {
            card.hasBeenShown = false;
        }
    }

    public void ApplyEffects(int dFood, int dWater, int dAmmo, int dMorale, int dMedicine)
    {
        food += dFood;
        water += dWater;
        ammo += dAmmo;
        morale += dMorale;
        medicine += dMedicine;

        Debug.Log($"Resources updated: Food={food}, Water={water}, Ammo={ammo}, Morale={morale}, Medicine={medicine}");

        // Clamp values (optional)
        food = Mathf.Clamp(food, 0, 100);
        water = Mathf.Clamp(water, 0, 100);
        ammo = Mathf.Clamp(ammo, 0, 100);
        morale = Mathf.Clamp(morale, 0, 100);
        medicine = Mathf.Clamp(medicine, 0, 100);

        UpdateUI();
    }
}
