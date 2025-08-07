using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardViewerBuild : MonoBehaviour
{
    [Header("Card Display Components")]
    public Image artwork;
    public TextMeshProUGUI courageText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI wisdomText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI quantityText;

    public ScriptableObject cardAsset;

    public void Initialize(ScriptableObject asset, int quantity = 0)
    {
        cardAsset = asset;
        UpdateCardView(quantity);
    }

    private void UpdateCardView(int quantity)
    {
        if (cardAsset == null)
        {
            Debug.LogWarning("CardViewerBuild: cardAsset está vazio.");
            return;
        }

        Sprite image = null;

        if (cardAsset is Creature creature)
        {
            courageText.text = creature.courage.ToString();
            powerText.text = creature.power.ToString();
            wisdomText.text = creature.wisdom.ToString();
            speedText.text = creature.speed.ToString();
            energyText.text = creature.energy.ToString();
            image = creature.artworkImage;
        }
        else if (cardAsset is Attack attack)
        {
            image = attack.artworkImage;
            courageText.transform.parent.gameObject.SetActive(false);
            powerText.transform.parent.gameObject.SetActive(false);
            wisdomText.transform.parent.gameObject.SetActive(false);
            speedText.transform.parent.gameObject.SetActive(false);
            energyText.transform.parent.gameObject.SetActive(false);
        }
        else if (cardAsset is Battlegear battlegear)
        {
            image = battlegear.artworkImage;
            courageText.transform.parent.gameObject.SetActive(false);
            powerText.transform.parent.gameObject.SetActive(false);
            wisdomText.transform.parent.gameObject.SetActive(false);
            speedText.transform.parent.gameObject.SetActive(false);
            energyText.transform.parent.gameObject.SetActive(false);
        }
        else if (cardAsset is Location location)
        {
            Transform child = this.transform.Find("CardDetails/Artwork");
            if (child != null)
            {
                child.transform.localEulerAngles = new Vector3(0, 0, 90);
                child.transform.localPosition = new Vector2(0f, 0f);
                child.transform.localScale = new Vector3(1.4f, 0.71f, 1f);
            }
            image = location.artworkImage;
            courageText.transform.parent.gameObject.SetActive(false);
            powerText.transform.parent.gameObject.SetActive(false);
            wisdomText.transform.parent.gameObject.SetActive(false);
            speedText.transform.parent.gameObject.SetActive(false);
            energyText.transform.parent.gameObject.SetActive(false);
        }
        else if (cardAsset is Mugic mugic)
        {
            image = mugic.artworkImage;
            courageText.transform.parent.gameObject.SetActive(false);
            powerText.transform.parent.gameObject.SetActive(false);
            wisdomText.transform.parent.gameObject.SetActive(false);
            speedText.transform.parent.gameObject.SetActive(false);
            energyText.transform.parent.gameObject.SetActive(false);
        }

        quantityText.text = quantity.ToString();

        if (artwork != null && image != null)
        {
            artwork.sprite = image;
        }
    }
}
