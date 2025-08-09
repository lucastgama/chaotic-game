using System.Collections.Generic;
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

    void Start()
    {
        LoadPlayerData();
        SetupButtonListeners();
        ShowAllCards();
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
    }

    void PrintCollectionToConsole()
    {
        if (playerData == null || playerData.collection == null)
        {
            Debug.LogWarning("Coleção está vazia ou não carregada.");
            return;
        }

        Debug.Log($"Coleção de {playerData.nickname}:");

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
                // CreateCardUI(entry);
                Debug.Log($"Criando UI para {cardType} com ID: {entry.cardId}");
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
            Debug.LogWarning($"[DeckBuilder] Carta '{entry.cardId}' não encontrada em {path}");
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
        Debug.Log("Fui chamado");
        var entry = playerData.collection.Find(e => e.cardId == cardId);

        if (entry != null && entry.qty > 0)
        {
            entry.qty--;
            Debug.Log($"Quantidade da carta '{cardId}' agora é {entry.qty}");
            ShowAllCards();
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string id;
    public string nickname;
    public List<CollectionEntry> collection;
}

[System.Serializable]
public class CollectionEntry
{
    public string cardId;
    public int qty;
}
