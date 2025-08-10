using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Configuration")]
    public string assignedCreatureId;
    public DropZoneType acceptOnlyCardType;
    public DeckBuilderController deckBuilderController;
    private DeckManager deckManager;
    public string dropZoneId;

    [Header("List Configuration")]
    public bool isListType = false;
    public Transform listContainer;
    public GameObject cardUIPrefab;

    private GameObject currentCardDisplay;
    private ScriptableObject currentCardAsset;

    private List<GameObject> listCards = new List<GameObject>();
    private List<ScriptableObject> listCardAssets = new List<ScriptableObject>();

    void Start()
    {
        deckManager = FindFirstObjectByType<DeckManager>();
        if (isListType && listContainer == null)
        {
            listContainer = transform;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggable = eventData.pointerDrag.GetComponent<CardDraggable>();
        if (draggable == null)
            return;

        if (!CardMatchesDropZone(draggable.cardAsset))
        {
            return;
        }

        if (draggable.quantity <= 0)
        {
            return;
        }

        if (deckManager != null)
        {
            string cardCode = GetCardCode(draggable.cardAsset);
            if (!deckManager.CanAddCard(cardCode, dropZoneId))
            {
                ShowRejectionFeedback();
                return;
            }
        }

        if (isListType)
        {
            HandleListDrop(draggable);
        }
        else
        {
            HandleSlotDrop(draggable);
        }
    }

    private void HandleListDrop(CardDraggable draggable)
    {
        string cardCode = GetCardCode(draggable.cardAsset);

        if (deckManager != null)
        {
            deckManager.AddCardToList(dropZoneId, cardCode);
        }

        listCardAssets.Add(draggable.cardAsset);

        GameObject cardUI = Instantiate(
            cardUIPrefab != null ? cardUIPrefab : draggable.gameObject,
            listContainer
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
                () => OnRemoveFromList(cardUI, draggable.cardAsset, draggable)
            );
        }

        listCards.Add(cardUI);
        draggable.DecreaseQuantity();
    }

    private void HandleSlotDrop(CardDraggable draggable)
    {
        if (IsOccupied())
        {
            return;
        }

        string cardCode = GetCardCode(draggable.cardAsset);

        if (deckManager != null)
        {
            deckManager.InsertCard(dropZoneId, cardCode);
        }

        currentCardAsset = draggable.cardAsset;
        assignedCreatureId = cardCode;

        currentCardDisplay = Instantiate(draggable.gameObject, transform);
        Destroy(currentCardDisplay.GetComponent<CardDraggable>());

        var viewer = currentCardDisplay.GetComponent<CardViewerBuild>();
        if (viewer != null)
        {
            viewer.Initialize(currentCardAsset, 1, true);
            RectTransform rect = viewer.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            draggable.DecreaseQuantity();
            viewer.removeBtn.onClick.AddListener(() => OnRemoveCard(viewer, draggable));
        }
    }

    private void ShowRejectionFeedback()
    {
        var image = GetComponent<Image>();
        if (image != null)
        {
            StartCoroutine(FlashRed());
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        var image = GetComponent<Image>();
        Color originalColor = image.color;

        image.color = Color.red;
        yield return new WaitForSeconds(0.2f);

        image.color = originalColor;
    }

    public void OnRemoveFromList(
        GameObject cardUI,
        ScriptableObject cardAsset,
        CardDraggable originalDraggable
    )
    {
        int index = listCards.IndexOf(cardUI);
        if (index >= 0)
        {
            listCards.RemoveAt(index);
            listCardAssets.RemoveAt(index);
            Destroy(cardUI);

            if (originalDraggable != null)
            {
                originalDraggable.IncrementQuantity();
            }

            if (deckManager != null)
            {
                string cardCode = GetCardCode(cardAsset);
                deckManager.RemoveCard(dropZoneId, cardCode);
            }
        }
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

    public bool IsOccupied()
    {
        if (isListType)
        {
            return false;
        }
        return !string.IsNullOrEmpty(assignedCreatureId) && currentCardAsset != null;
    }

    public void OnRemoveCard(CardViewerBuild viewer, CardDraggable draggable)
    {
        if (viewer != null)
        {
            if (deckManager != null)
            {
                deckManager.RemoveSlotCard(dropZoneId);
            }

            Destroy(viewer.gameObject);
            draggable.IncrementQuantity();

            assignedCreatureId = null;
            currentCardAsset = null;
            currentCardDisplay = null;
        }
    }

    private bool CardMatchesDropZone(ScriptableObject cardAsset)
    {
        switch (acceptOnlyCardType)
        {
            case DropZoneType.Creature:
                return cardAsset is Creature;
            case DropZoneType.Battlegear:
                return cardAsset is Battlegear;
            case DropZoneType.Attack:
                return cardAsset is Attack;
            case DropZoneType.Location:
                return cardAsset is Location;
            case DropZoneType.Mugic:
                return cardAsset is Mugic;
        }
        return false;
    }

    public List<ScriptableObject> GetListCards()
    {
        return new List<ScriptableObject>(listCardAssets);
    }

    public int GetListCount()
    {
        return listCards.Count;
    }

    public void OnPointerEnter(PointerEventData eventData) { }

    public void OnPointerExit(PointerEventData eventData) { }
}

public enum DropZoneType
{
    Creature,
    Battlegear,
    Attack,
    Location,
    Mugic,
}
