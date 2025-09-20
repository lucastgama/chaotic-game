using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuilderController : MonoBehaviour
{
    [Header("Botões de Categoria (ainda não usados)")]
    public Button allCardsButton;
    public Button creaturesButton;
    public Button battleGearButton;
    public Button attacksButton;
    public Button locationsButton;
    public Button mugicButton;

    [Header("UI Prefab e Container")]
    public GameObject creatureCardUIPrefab;
    public Transform creatureCardContainer;

    [Header("JSON do Jogador")]
    public TextAsset playerJson;

    private PlayerData playerData;

    [Header("Iniciar jogo")]
    public Button startGameButton;

    [Header("Carregar Deck Salvo")]
    public TMP_Dropdown savedDecksDropdown;
    public Button loadDeckButton;

    void Start()
    {
        ShowInterface();
        LoadPlayerData();
        SetupButtonListeners();
        PopulateSavedDecksDropdown();
        ShowAllCards();
    }

    void ShowInterface()
    {
        var currentState = GameManager.Instance.CurrentState;
        var battleMode = GameManager.Instance.battleMode;
        var currentBattleMode = GameManager.Instance.CurrentBattleMode;

        bool shouldShowObject = currentState == GameState.DeckSetup;

        if (shouldShowObject)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    void LoadPlayerData()
    {
        playerData = JsonUtility.FromJson<PlayerData>(playerJson.text);
        if (playerData == null)
        {
            Debug.LogError("Falha ao carregar dados do jogador!");
        }
    }

    void SetupButtonListeners()
    {
        if (creaturesButton != null)
            creaturesButton.onClick.AddListener(() => ShowCardsByType("creature"));

        if (attacksButton != null)
            attacksButton.onClick.AddListener(() => ShowCardsByType("attack"));

        if (battleGearButton != null)
            battleGearButton.onClick.AddListener(() => ShowCardsByType("battlegear"));

        if (mugicButton != null)
            mugicButton.onClick.AddListener(() => ShowCardsByType("mugic"));

        if (locationsButton != null)
            locationsButton.onClick.AddListener(() => ShowCardsByType("location"));

        if (allCardsButton != null)
            allCardsButton.onClick.AddListener(() => ShowAllCards());

        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => startGame());

        if (loadDeckButton != null)
            loadDeckButton.onClick.AddListener(() => LoadSelectedDeck());
    }

    void PopulateSavedDecksDropdown()
    {
        if (savedDecksDropdown == null || playerData?.decks == null)
            return;

        savedDecksDropdown.ClearOptions();

        int currentBattleMode = GameManager.Instance.GetCurrentBattleModeValue();

        List<string> deckOptions = new List<string>();
        deckOptions.Add("-- Selecione um Deck --");

        List<SavedDeck> validDecks = GetValidDecksForCurrentBattleMode(currentBattleMode);

        foreach (var deck in validDecks)
        {
            deckOptions.Add(deck.name);
        }

        savedDecksDropdown.AddOptions(deckOptions);
    }

    List<SavedDeck> GetValidDecksForCurrentBattleMode(int battleMode)
    {
        List<SavedDeck> validDecks = new List<SavedDeck>();

        if (playerData?.decks == null)
            return validDecks;

        string deckCategory = GetDeckCategoryByBattleMode(battleMode);

        switch (deckCategory)
        {
            case "solo":
                if (playerData.decks.solo != null)
                {
                    foreach (var deck in playerData.decks.solo)
                    {
                        if (deck.battleMode == battleMode)
                            validDecks.Add(deck);
                    }
                }
                break;

            case "trio":
                if (playerData.decks.trio != null)
                {
                    foreach (var deck in playerData.decks.trio)
                    {
                        if (deck.battleMode == battleMode)
                            validDecks.Add(deck);
                    }
                }
                break;

            case "squad":
                if (playerData.decks.squad != null)
                {
                    foreach (var deck in playerData.decks.squad)
                    {
                        if (deck.battleMode == battleMode)
                            validDecks.Add(deck);
                    }
                }
                break;
        }

        return validDecks;
    }

    string GetDeckCategoryByBattleMode(int battleMode)
    {
        switch (battleMode)
        {
            case 1:
                return "solo";
            case 3:
                return "trio";
            case 6:
                return "squad";
            default:
                return "solo";
        }
    }

    void LoadSelectedDeck()
    {
        if (savedDecksDropdown == null || savedDecksDropdown.value == 0)
        {
            return;
        }

        int currentBattleMode = GameManager.Instance.GetCurrentBattleModeValue();
        List<SavedDeck> validDecks = GetValidDecksForCurrentBattleMode(currentBattleMode);

        int deckIndex = savedDecksDropdown.value - 1;

        if (deckIndex >= 0 && deckIndex < validDecks.Count)
        {
            SavedDeck selectedDeck = validDecks[deckIndex];
            LoadDeckIntoBuilder(selectedDeck);
        }
    }

    void LoadDeckIntoBuilder(SavedDeck deck)
    {
        ClearCurrentDeck();

        foreach (var cardEntry in deck.cards)
        {
            LoadSingleCardIntoDropZone(cardEntry);
        }

        RefreshCardDisplay();
    }

    void ClearCurrentDeck()
    {
        DropZone[] allDropZones = FindObjectsByType<DropZone>(FindObjectsSortMode.None);

        foreach (var dropZone in allDropZones)
        {
            if (!dropZone.isListType && dropZone.IsOccupied())
            {
                if (dropZone.currentCardDisplay != null)
                {
                    var viewer = dropZone.currentCardDisplay.GetComponent<CardViewerBuild>();
                    if (viewer != null)
                    {
                        var originalDraggable = FindCardDraggableByAsset(dropZone.currentCardAsset);
                        if (originalDraggable != null)
                        {
                            dropZone.OnRemoveCard(viewer, originalDraggable);
                        }
                    }
                }
            }
            else if (dropZone.isListType)
            {
                while (dropZone.GetListCount() > 0)
                {
                    var listCards = dropZone.listCards;
                    var listAssets = dropZone.listCardAssets;

                    if (listCards.Count > 0 && listAssets.Count > 0)
                    {
                        var cardAsset = listAssets[0];
                        var cardUI = listCards[0];
                        var originalDraggable = FindCardDraggableByAsset(cardAsset);

                        if (originalDraggable != null)
                        {
                            dropZone.OnRemoveFromList(cardUI, cardAsset, originalDraggable);
                        }
                    }
                }
            }
        }
    }

    void LoadSingleCardIntoDropZone(SavedCardEntry cardEntry)
    {
        var collectionEntry = playerData.collection.Find(c => c.cardId == cardEntry.cardId);
        if (collectionEntry == null || collectionEntry.qty <= 0)
        {
            return;
        }

        DropZone targetDropZone = FindDropZoneByPosition(cardEntry.position);
        if (targetDropZone == null)
        {
            Debug.LogWarning($"DropZone não encontrada para posição: {cardEntry.position}");
            return;
        }

        CardDraggable cardDraggable = FindCardDraggableByCardId(cardEntry.cardId);
        if (cardDraggable == null)
        {
            Debug.LogWarning($"CardDraggable não encontrado para carta: {cardEntry.cardId}");
            return;
        }

        SimulateCardDrop(cardDraggable, targetDropZone);
    }

    void SimulateCardDrop(CardDraggable draggable, DropZone dropZone)
    {
        string cardCode = GetCardCode(draggable.cardAsset);

        DeckManager deckManager = FindFirstObjectByType<DeckManager>();
        if (deckManager != null && !deckManager.CanAddCard(cardCode, dropZone.dropZoneId))
        {
            return;
        }

        if (dropZone.isListType)
        {
            if (deckManager != null)
            {
                deckManager.AddCardToList(dropZone.dropZoneId, cardCode);
            }

            dropZone.listCardAssets.Add(draggable.cardAsset);

            GameObject cardUI = Instantiate(
                dropZone.cardUIPrefab != null ? dropZone.cardUIPrefab : draggable.gameObject,
                dropZone.listContainer
            );

            var newDraggable = cardUI.GetComponent<CardDraggable>();
            if (newDraggable != null)
            {
                Destroy(newDraggable);
            }

            var viewer = cardUI.GetComponent<CardViewerBuild>();
            if (viewer != null)
            {
                viewer.Initialize(draggable.cardAsset, 1, true);
                viewer.removeBtn.onClick.RemoveAllListeners();
                viewer.removeBtn.onClick.AddListener(
                    () => dropZone.OnRemoveFromList(cardUI, draggable.cardAsset, draggable)
                );
            }

            dropZone.listCards.Add(cardUI);
            draggable.DecreaseQuantity();
        }
        else
        {
            if (dropZone.IsOccupied())
            {
                Debug.LogWarning($"DropZone {dropZone.dropZoneId} já está ocupada");
                return;
            }

            if (deckManager != null)
            {
                deckManager.InsertCard(dropZone.dropZoneId, cardCode);
            }

            dropZone.currentCardAsset = draggable.cardAsset;
            dropZone.assignedCreatureId = cardCode;

            dropZone.currentCardDisplay = Instantiate(draggable.gameObject, dropZone.transform);
            Destroy(dropZone.currentCardDisplay.GetComponent<CardDraggable>());

            var viewer = dropZone.currentCardDisplay.GetComponent<CardViewerBuild>();
            if (viewer != null)
            {
                viewer.Initialize(dropZone.currentCardAsset, 1, true);
                RectTransform rect = viewer.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                draggable.DecreaseQuantity();
                viewer.removeBtn.onClick.AddListener(
                    () => dropZone.OnRemoveCard(viewer, draggable)
                );
            }
        }
    }

    DropZone FindDropZoneByPosition(string position)
    {
        DropZone[] allDropZones = FindObjectsByType<DropZone>(FindObjectsSortMode.None);

        foreach (var dropZone in allDropZones)
        {
            if (dropZone.dropZoneId == position)
            {
                return dropZone;
            }
        }

        return null;
    }

    CardDraggable FindCardDraggableByCardId(string cardId)
    {
        CardDraggable[] allDraggables = FindObjectsByType<CardDraggable>(FindObjectsSortMode.None);

        foreach (var draggable in allDraggables)
        {
            string draggableCardId = GetCardCode(draggable.cardAsset);
            if (draggableCardId == cardId)
            {
                return draggable;
            }
        }

        foreach (var draggable in allDraggables)
        {
            if (draggable.cardAsset != null)
            {
                if (draggable.cardAsset is Creature c && c.cardCode == cardId)
                    return draggable;
                if (draggable.cardAsset is Attack a && a.cardCode == cardId)
                    return draggable;
                if (draggable.cardAsset is Battlegear b && b.cardCode == cardId)
                    return draggable;
                if (draggable.cardAsset is Location l && l.cardCode == cardId)
                    return draggable;
                if (draggable.cardAsset is Mugic m && m.cardCode == cardId)
                    return draggable;
            }
        }

        return null;
    }

    CardDraggable FindCardDraggableByAsset(ScriptableObject asset)
    {
        CardDraggable[] allDraggables = FindObjectsByType<CardDraggable>(FindObjectsSortMode.None);

        foreach (var draggable in allDraggables)
        {
            if (draggable.cardAsset == asset)
            {
                return draggable;
            }
        }

        return null;
    }

    string GetCardCode(ScriptableObject card)
    {
        if (card is Creature c)
            return c.cardCode;
        if (card is Attack a)
            return a.cardCode;
        if (card is Battlegear b)
            return b.cardCode;
        if (card is Location l)
            return l.cardCode;
        if (card is Mugic m)
            return m.cardCode;
        return "Unknown";
    }

    void RefreshCardDisplay()
    {
        RefreshCollectionCardsOnly();

        UpdateCollectionQuantities();
    }

    void RefreshCollectionCardsOnly()
    {
        CardDraggable[] collectionDraggables =
            creatureCardContainer.GetComponentsInChildren<CardDraggable>();

        foreach (var draggable in collectionDraggables)
        {
            if (draggable != null)
            {
                draggable.UpdateVisual();
            }
        }
    }

    void UpdateCollectionQuantities()
    {
        DeckManager deckManager = FindFirstObjectByType<DeckManager>();
        if (deckManager == null)
            return;

        foreach (var deckCard in deckManager.deckJson.cards)
        {
            var collectionEntry = playerData.collection.Find(c => c.cardId == deckCard.cardId);
        }
    }

    void PrintCollectionToConsole()
    {
        if (playerData == null || playerData.collection == null)
        {
            return;
        }

        foreach (var entry in playerData.collection)
        {
            string cardId = entry.cardId;
            int qty = entry.qty;

            string typePrefix = cardId.Split('-')[0];
            string resourcePath = "";

            switch (typePrefix)
            {
                case "creature":
                    resourcePath = $"Data/Creatures/{cardId}";
                    var creature = Resources.Load<Creature>(resourcePath);
                    if (creature != null)
                    {
                        Debug.Log(
                            $"[Creature] Nome: {creature.cardName} | Quantidade: {qty} | Sprite: {(creature.artworkImage != null ? creature.artworkImage.name : "Sem imagem")}"
                        );
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"Creature com ID '{cardId}' não encontrada em {resourcePath}"
                        );
                    }
                    break;

                case "attack":
                    resourcePath = $"Data/Attacks/{cardId}";
                    var attack = Resources.Load<Attack>(resourcePath);
                    if (attack != null)
                    {
                        Debug.Log(
                            $"[Attack] Nome: {attack.cardName} | Quantidade: {qty} | Sprite: {(attack.artworkImage != null ? attack.artworkImage.name : "Sem imagem")}"
                        );
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"Attack com ID '{cardId}' não encontrado em {resourcePath}"
                        );
                    }
                    break;
                case "battlegear":
                    resourcePath = $"Data/Battlegear/{cardId}";
                    var battlegear = Resources.Load<Battlegear>(resourcePath);
                    if (battlegear != null)
                    {
                        Debug.Log(
                            $"[Battlegear] Nome: {battlegear.cardName} | Quantidade: {qty} | Sprite: {(battlegear.artworkImage != null ? battlegear.artworkImage.name : "Sem imagem")}"
                        );
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"Battlegear com ID '{cardId}' não encontrado em {resourcePath}"
                        );
                    }
                    break;
                case "location":
                    resourcePath = $"Data/Locations/{cardId}";
                    var location = Resources.Load<Location>(resourcePath);
                    if (location != null)
                    {
                        Debug.Log(
                            $"[Location] Nome: {location.cardName} | Quantidade: {qty} | Sprite: {(location.artworkImage != null ? location.artworkImage.name : "Sem imagem")}"
                        );
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"Location com ID '{cardId}' não encontrada em {resourcePath}"
                        );
                    }
                    break;
                case "mugic":
                    resourcePath = $"Data/Mugic/{cardId}";
                    var mugic = Resources.Load<Mugic>(resourcePath);
                    if (mugic != null)
                    {
                        Debug.Log(
                            $"[Mugic] Nome: {mugic.cardName} | Quantidade: {qty} | Sprite: {(mugic.artworkImage != null ? mugic.artworkImage.name : "Sem imagem")}"
                        );
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"Mugic com ID '{cardId}' não encontrado em {resourcePath}"
                        );
                    }
                    break;

                default:
                    Debug.LogWarning($"Tipo de carta desconhecido: {cardId}");
                    break;
            }
        }
    }

    void ShowAllCards()
    {
        foreach (Transform child in creatureCardContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var entry in playerData.collection)
        {
            CreateCardUI(entry);
        }
    }

    void ShowCardsByType(string cardType)
    {
        foreach (Transform child in creatureCardContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var entry in playerData.collection)
        {
            if (entry.cardId.StartsWith(cardType))
            {
                CreateCardUI(entry);
            }
        }
    }

    void CreateCardUI(CollectionEntry entry)
    {
        ScriptableObject card = null;
        string path = "";

        if (entry.cardId.StartsWith("creature"))
        {
            path = $"Data/Creatures/{entry.cardId}";
            card = Resources.Load<Creature>(path);
        }
        else if (entry.cardId.StartsWith("attack"))
        {
            path = $"Data/Attacks/{entry.cardId}";
            card = Resources.Load<Attack>(path);
        }
        else if (entry.cardId.StartsWith("battlegear"))
        {
            path = $"Data/Battlegear/{entry.cardId}";
            card = Resources.Load<Battlegear>(path);
        }
        else if (entry.cardId.StartsWith("location"))
        {
            path = $"Data/Locations/{entry.cardId}";
            card = Resources.Load<Location>(path);
        }
        else if (entry.cardId.StartsWith("mugic"))
        {
            path = $"Data/Mugic/{entry.cardId}";
            card = Resources.Load<Mugic>(path);
        }

        if (card == null)
        {
            return;
        }

        GameObject cardUI = Instantiate(creatureCardUIPrefab, creatureCardContainer);

        CardDraggable draggable = cardUI.GetComponent<CardDraggable>();
        if (draggable != null)
        {
            draggable.cardAsset = card;
            draggable.quantity = entry.qty;
        }

        CardViewerBuild viewer = cardUI.GetComponent<CardViewerBuild>();
        if (viewer != null)
        {
            viewer.Initialize(card, entry.qty);
        }
    }

    public void UpdateCardQuantity(string cardId)
    {
        var entry = playerData.collection.Find(e => e.cardId == cardId);

        if (entry != null && entry.qty > 0)
        {
            entry.qty--;
            ShowAllCards();
        }
    }

    public void startGame()
    {
        DeckManager deckManager = FindFirstObjectByType<DeckManager>();
        bool isDeckValid = deckManager.IsDeckValid();
        if (true)
        {
            GameManager.Instance.SetGameState(GameState.BattlePhase);
            Destroy(this.gameObject);
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string id;
    public string nickname;
    public List<CollectionEntry> collection;
    public PlayerDecks decks;
}

[System.Serializable]
public class PlayerDecks
{
    public List<SavedDeck> solo;
    public List<SavedDeck> trio;
    public List<SavedDeck> squad;
}

[System.Serializable]
public class SavedDeck
{
    public string id;
    public string name;
    public int battleMode;
    public List<SavedCardEntry> cards;
}

[System.Serializable]
public class SavedCardEntry
{
    public string cardId;
    public string position;
}

[System.Serializable]
public class CollectionEntry
{
    public string cardId;
    public int qty;
}
