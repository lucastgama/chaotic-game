using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureDisplay : MonoBehaviour
{
    public Creature creatureData;

    [Header("Basic Card Elements")]
    public Image artworkImage;

    [Header("Attribute Display")]
    public TextMeshProUGUI courageText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI wisdomText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI energyText;

    [Header("Capability Display")]
    public TextMeshProUGUI mugicCountersText;
    public TextMeshProUGUI elementsText;

    [Header("Optional Display Elements")]
    public Image cardBorder;
    public GameObject[] elementIcons;

    void Start()
    {
        if (creatureData != null)
        {
            UpdateCardDisplay();
        }
    }

    public void UpdateCardDisplay()
    {
        if (creatureData == null)
        {
            Debug.LogWarning("No creature data assigned to display!");
            return;
        }

        if (artworkImage != null && creatureData.artworkImage != null)
        {
            artworkImage.sprite = creatureData.artworkImage;
        }

        if (courageText != null)
            courageText.text = creatureData.courage.ToString();

        if (powerText != null)
            powerText.text = creatureData.power.ToString();

        if (wisdomText != null)
            wisdomText.text = creatureData.wisdom.ToString();

        if (speedText != null)
            speedText.text = creatureData.speed.ToString();

        if (energyText != null)
            energyText.text = creatureData.energy.ToString();

        if (elementsText != null)
            elementsText.text = GetElementsAsString();

        if (cardBorder != null)
        {
            cardBorder.color = creatureData.tribeColor;
        }

        UpdateElementIcons();
    }

    private string GetElementsAsString()
    {
        if (creatureData.elements == null || creatureData.elements.Length == 0)
            return "None";

        string result = "";
        for (int i = 0; i < creatureData.elements.Length; i++)
        {
            result += creatureData.elements[i].ToString();
            if (i < creatureData.elements.Length - 1)
                result += ", ";
        }

        return result;
    }

    private void UpdateElementIcons()
    {
        if (elementIcons == null || elementIcons.Length == 0)
            return;

        foreach (GameObject icon in elementIcons)
        {
            if (icon != null)
                icon.SetActive(false);
        }

        if (creatureData.elements != null)
        {
            foreach (ElementType element in creatureData.elements)
            {
                int elementIndex = (int)element;
                if (elementIndex >= 0 && elementIndex < elementIcons.Length)
                {
                    if (elementIcons[elementIndex] != null)
                        elementIcons[elementIndex].SetActive(true);
                }
            }
        }
    }

    public void SetCreature(Creature newCreature)
    {
        creatureData = newCreature;
        UpdateCardDisplay();
    }
}
