using UnityEngine;

[CreateAssetMenu(fileName = "New Battlegear", menuName = "Chaotic TCG/Battlegear", order = 9)]
public class Battlegear : ScriptableObject
{
    [Header("Code")]
    public string cardCode;

    [Header("Basic Information")]
    public string cardName;
    public Sprite artworkImage;
    public Rarity rarity;

    [Header("Abilities")]
    [TextArea(3, 5)]
    public string abilityDescription;

    [Header("Information & Lore")]
    [TextArea(3, 5)]
    public string cardType;
    public string cardFlavorText;
    public string cardArtist;

    [Header("Art & Presentation")]
    public Sprite cardPortrait;

    [Header("Gameplay Mechanics")]
    public bool isUnique;
    public bool isLoyal;
    public bool isLegendary;
    public string tags;
    public string exclusive;

    [Header("Set Information")]
    public string cardSet;
    public string cardId;

    public bool CanEquip(Creature creature)
    {
        if (isLoyal && creature != null)
        {
            return false;
        }

        if (isUnique)
        {
            return true;
        }

        return true;
    }

    public bool IsSpecialBattlegear()
    {
        return isUnique || isLoyal || isLegendary || !string.IsNullOrEmpty(exclusive);
    }

    public string GetSpecialProperties()
    {
        var properties = new System.Collections.Generic.List<string>();

        if (isUnique)
            properties.Add("Unique");
        if (isLoyal)
            properties.Add("Loyal");
        if (isLegendary)
            properties.Add("Legendary");
        if (!string.IsNullOrEmpty(exclusive))
            properties.Add($"Exclusive: {exclusive}");

        return properties.Count > 0 ? string.Join(", ", properties) : "None";
    }

    public bool HasEquipmentRestrictions()
    {
        return isLoyal || !string.IsNullOrEmpty(exclusive);
    }
}
