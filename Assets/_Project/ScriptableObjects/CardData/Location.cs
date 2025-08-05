using UnityEngine;

[CreateAssetMenu(fileName = "New Location", menuName = "Chaotic TCG/Location", order = 7)]
public class Location : ScriptableObject
{
    [Header("Code")]
    public string cardCode;

    [Header("Basic Information")]
    public string cardName;
    public Sprite artworkImage;
    public Rarity rarity;

    [Header("Initiative")]
    public InitiativeType initiative;

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
    public string tags;
    public string exclusive;

    [Header("Set Information")]
    public string cardSet;
    public string cardId;

    // Métodos utilitários
    public bool HasInitiative(InitiativeType initiativeType)
    {
        return initiative == initiativeType;
    }

    public string GetInitiativeString()
    {
        return initiative.ToString();
    }

    public bool IsSpecialLocation()
    {
        return isUnique || !string.IsNullOrEmpty(exclusive);
    }
}

public enum InitiativeType
{
    Power,
    Courage,
    Wisdom,
    Speed,
    None
}
