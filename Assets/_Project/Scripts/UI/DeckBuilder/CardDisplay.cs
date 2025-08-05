using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Image artwork;
    public TextMeshProUGUI nameText;

    public void SetCard(string cardId)
    {
        Creature creature = Resources.Load<Creature>($"Creatures/{cardId}");
        if (creature != null)
        {
            artwork.sprite = creature.artworkImage;
            nameText.text = creature.cardName;
        }
        else
        {
            Debug.LogWarning($"Carta com ID {cardId} não encontrada!");
        }
    }
}
