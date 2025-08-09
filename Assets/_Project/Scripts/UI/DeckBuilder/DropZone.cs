using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Configuration")]
    public string assignedCreatureId;
    public DropZoneType acceptOnlyCardType;
    public DeckBuilderController deckBuilderController;
    public string dropZoneId;

    private GameObject currentCardDisplay;
    private ScriptableObject currentCardAsset;

    public void OnDrop(PointerEventData eventData)
    {
        var draggable = eventData.pointerDrag.GetComponent<CardDraggable>();
        if (draggable == null)
            return;

        if (!CardMatchesDropZone(draggable.cardAsset))
            return;

        if (IsOccupied())
            return;

        if (draggable.quantity <= 0)
            return;

        currentCardAsset = draggable.cardAsset;
        assignedCreatureId = GetCardCode(currentCardAsset);

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

        if (deckBuilderController != null)
        {
            // deckBuilderController.UpdateCardQuantity(assignedCreatureId);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[DROPZONE] Mouse entrou na dropzone '{gameObject.name}'");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[DROPZONE] Mouse saiu da dropzone '{gameObject.name}'");
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
        return !string.IsNullOrEmpty(assignedCreatureId) && currentCardAsset != null;
    }

    public void OnRemoveCard(CardViewerBuild viewer, CardDraggable draggable)
    {
        if (viewer != null)
        {
            Destroy(viewer.gameObject);
            draggable.IncrementQuantity();
            assignedCreatureId = null;
            currentCardAsset = null;
            IsOccupied();
        }
        //argument CardDraggable draggable
        // if (draggable != null)
        // {
        //     draggable.ResetPosition();
        // }
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
}

public enum DropZoneType
{
    Creature,
    Battlegear,
    Attack,
    Location,
    Mugic,
}
