using UnityEngine;

[CreateAssetMenu(fileName = "New Mugic", menuName = "Chaotic TCG/Mugic", order = 8)]
public class Mugic : ScriptableObject
{
    [Header("Code")]
    public string cardCode;

    [Header("Basic Information")]
    public string cardName;
    public Sprite artworkImage;
    public Rarity rarity;

    [Header("Tribe & Cost")]
    public Tribe tribe;
    public int cost;

    [Header("Abilities")]
    [TextArea(3, 5)]
    public string abilityDescription;

    [Header("Musical Notes")]
    [TextArea(2, 3)]
    public string notes;

    [TextArea(2, 3)]
    public string shownotes;

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
    public string altVersion;
    public bool isAnimated;

    [Header("Set Information")]
    public string cardSet;
    public string cardId;

    public bool CanPlayWithTribe(Tribe creatureTribe)
    {
        return tribe == creatureTribe || tribe == Tribe.Generic;
    }

    public bool HasSufficientCounters(int availableCounters)
    {
        return availableCounters >= cost;
    }

    public string GetNotesDisplay()
    {
        return !string.IsNullOrEmpty(shownotes) ? shownotes : notes;
    }

    public bool IsSpecialMugic()
    {
        return isUnique || !string.IsNullOrEmpty(exclusive);
    }

    public string GetTribeRequirement()
    {
        return tribe == Tribe.Generic ? "Any Tribe" : tribe.ToString();
    }
}
