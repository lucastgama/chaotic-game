using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInspector : MonoBehaviour
{
    [Header("Inspector Panel")]
    public GameObject inspectorPanel;
    public Button closeButton;

    [Header("Card Display")]
    public Image cardImage;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public TextMeshProUGUI cardStatsText;

    [Header("Game State Manager")]
    public GameStateManager gameStateManager;

    private GameState previousGameState;

    void Start()
    {
        if (inspectorPanel != null)
            inspectorPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseInspector);
    }

    public void ShowCardInspector(ScriptableObject card)
    {
        if (card == null || inspectorPanel == null)
            return;

        if (gameStateManager != null)
        {
            previousGameState = gameStateManager.GetCurrentState();
            gameStateManager.SetGameState(GameState.CardInspector);
        }

        PopulateCardData(card);

        inspectorPanel.SetActive(true);
    }

    void PopulateCardData(ScriptableObject card)
    {
        if (card is Creature creature)
        {
            SetCardImage(creature.artworkImage);
            SetCardName(creature.cardName);
        }
        else if (card is Attack attack)
        {
            SetCardImage(attack.artworkImage);
            SetCardName(attack.cardName);
        }
        else if (card is Battlegear battlegear)
        {
            SetCardImage(battlegear.artworkImage);
            SetCardName(battlegear.cardName);
        }
        else if (card is Location location)
        {
            SetCardImage(location.artworkImage);
            SetCardName(location.cardName);
        }
        else if (card is Mugic mugic)
        {
            SetCardImage(mugic.artworkImage);
            SetCardName(mugic.cardName);
        }
    }

    void SetCardImage(Sprite artwork)
    {
        if (cardImage != null && artwork != null)
            cardImage.sprite = artwork;
    }

    void SetCardName(string name)
    {
        if (cardNameText != null)
            cardNameText.text = name;
    }

    void SetCardDescription(string description)
    {
        if (cardDescriptionText != null)
            cardDescriptionText.text = description;
    }

    void SetCardStats(string stats)
    {
        if (cardStatsText != null)
            cardStatsText.text = stats;
    }

    public void CloseInspector()
    {
        if (inspectorPanel != null)
            inspectorPanel.SetActive(false);

        if (gameStateManager != null)
            gameStateManager.SetGameState(previousGameState);
    }
}
