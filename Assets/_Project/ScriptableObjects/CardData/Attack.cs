using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Chaotic TCG/Attack", order = 6)]
public class Attack : ScriptableObject
{
    [Header("Code")]
    public string cardCode;

    [Header("Basic Information")]
    public string cardName;
    public Sprite artworkImage;
    public Rarity rarity;

    [Header("Battle Points & Damage")]
    public int buildPoints;
    public int baseDamage;

    [Header("Elemental Damage")]
    public int fireDamage;
    public int airDamage;
    public int earthDamage;
    public int waterDamage;

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

    public int GetTotalElementalDamage()
    {
        return fireDamage + airDamage + earthDamage + waterDamage;
    }

    public int GetElementalDamage(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => fireDamage,
            ElementType.Air => airDamage,
            ElementType.Earth => earthDamage,
            ElementType.Water => waterDamage,
            _ => 0
        };
    }

    public bool HasElementalDamage(ElementType element)
    {
        return GetElementalDamage(element) > 0;
    }

    public ElementType[] GetElementalTypes()
    {
        var elements = new System.Collections.Generic.List<ElementType>();

        if (fireDamage > 0)
            elements.Add(ElementType.Fire);
        if (airDamage > 0)
            elements.Add(ElementType.Air);
        if (earthDamage > 0)
            elements.Add(ElementType.Earth);
        if (waterDamage > 0)
            elements.Add(ElementType.Water);

        return elements.ToArray();
    }

    public int CalculateTotalDamage(ElementType[] creatureElements = null)
    {
        int total = baseDamage;

        if (creatureElements != null)
        {
            foreach (var element in creatureElements)
            {
                total += GetElementalDamage(element);
            }
        }

        return total;
    }

    public string GetElementalDamageString()
    {
        var damages = new System.Collections.Generic.List<string>();

        if (fireDamage > 0)
            damages.Add($"Fire: {fireDamage}");
        if (airDamage > 0)
            damages.Add($"Air: {airDamage}");
        if (earthDamage > 0)
            damages.Add($"Earth: {earthDamage}");
        if (waterDamage > 0)
            damages.Add($"Water: {waterDamage}");

        return damages.Count > 0 ? string.Join(", ", damages) : "None";
    }
}
