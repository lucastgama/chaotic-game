using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Creature", menuName = "Chaotic TCG/Creature", order = 5)]
public class Creature : ScriptableObject
{
    [Header("Code")]
    public string cardCode;

    [Header("Basic Information")]
    public string cardName;
    public Sprite artworkImage;
    public Tribe tribe;
    public Rarity rarity;

    [Header("Main Attributes")]
    public int courage;
    public int power;
    public int wisdom;
    public int speed;
    public int energy;

    [Header("Capabilities")]
    public int mugicCounters;

    [Tooltip("Creatures can have multiple elements")]
    public ElementType[] elements;

    [Header("Abilities")]
    [TextArea(3, 5)]
    public string abilityDescription;
    public AbilityTriggerType abilityTrigger;

    [Header("Information & Lore")]
    [TextArea(3, 5)]
    public string cardType;
    public string cardFlavorText;
    public string cardArtist;

    [Header("Brainwashed")]
    public bool isBrainwashed;

    [TextArea(3, 5)]
    public string brainwashedEffect;

    [Header("Art & Presentation")]
    public Color tribeColor;
    public string flavorText;
    public Sprite creaturePortrait;

    [Header("Set Information")]
    public string cardSet;
    public string cardId;

    [Header("Gameplay Mechanics")]
    public int moveRange = 2;
    public bool isInfected = false;

    public bool HasElement(ElementType elementToCheck)
    {
        if (elements == null || elements.Length == 0)
            return false;

        foreach (ElementType element in elements)
        {
            if (element == elementToCheck)
                return true;
        }

        return false;
    }

    public int GetModifiedCourage(BattleConditions conditions)
    {
        int modifiedValue = courage;

        if (conditions.isInMugicLocation && HasElement(ElementType.Fire))
            modifiedValue += 10;

        return modifiedValue;
    }

    public int GetModifiedPower(BattleConditions conditions)
    {
        int modifiedValue = power;

        if (conditions.isInBattleGear && HasElement(ElementType.Earth))
            modifiedValue += 5;

        return modifiedValue;
    }

    public int GetModifiedWisdom(BattleConditions conditions)
    {
        int modifiedValue = wisdom;

        if (conditions.hasTribalAdvantage && HasElement(ElementType.Air))
            modifiedValue += 5;

        return modifiedValue;
    }

    public int GetModifiedSpeed(BattleConditions conditions)
    {
        int modifiedValue = speed;

        if (conditions.adjacentAllies > 0 && HasElement(ElementType.Water))
            modifiedValue += (5 * conditions.adjacentAllies);

        return modifiedValue;
    }

    public int GetModifiedEnergy(BattleConditions conditions)
    {
        return energy;
    }

    public string GetElementsAsString()
    {
        if (elements == null || elements.Length == 0)
            return "None";

        string result = "";
        for (int i = 0; i < elements.Length; i++)
        {
            result += elements[i].ToString();
            if (i < elements.Length - 1)
                result += ", ";
        }

        return result;
    }

    public float GetElementalAdvantage(ElementType attackElement)
    {
        if (HasElement(attackElement))
            return 1.0f;

        if (attackElement == ElementType.Fire && HasElement(ElementType.Air))
            return 1.5f;

        if (attackElement == ElementType.Water && HasElement(ElementType.Fire))
            return 1.5f;

        if (attackElement == ElementType.Earth && HasElement(ElementType.Water))
            return 1.5f;

        if (attackElement == ElementType.Air && HasElement(ElementType.Earth))
            return 1.5f;

        return 1.0f;
    }
}

public enum Tribe
{
    Overworld,
    Underworld,
    Mipedian,
    Danian,
    Marillian,
    Generic
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    SuperRare,
    UltraRare,
    Promo
}

public enum ElementType
{
    Fire,
    Air,
    Earth,
    Water,
    None
}

public enum AbilityTriggerType
{
    Passive,
    OnAttack,
    OnDefend,
    OnDamage,
    OnEnterLocation,
    OnActivation,
    OnBattleStart,
    OnTurnStart,
    OnMugicCast
}

[System.Serializable]
public class BattleConditions
{
    public bool isInMugicLocation;
    public bool isInBattleGear;
    public bool hasTribalAdvantage;
    public int adjacentAllies;
    public ElementType attackElement;
}
