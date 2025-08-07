using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Configuration")]
    public string assignedCardId;
    public Image slotImage;
    public bool acceptOnlyCreatures = false;

    [Header("Card Display Components")]
    public GameObject cardDisplayPrefab;

    private GameObject currentCardDisplay;
    private ScriptableObject currentCardAsset;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[DROPZONE] DropZone '{gameObject.name}' recebeu algo!");

        if (eventData.pointerDrag == null)
        {
            Debug.LogWarning("[DROPZONE] Nenhum objeto sendo arrastado!");
            return;
        }

        CardDraggable draggable = eventData.pointerDrag.GetComponent<CardDraggable>();
        if (draggable == null)
        {
            Debug.LogWarning("[DROPZONE] Objeto não tem CardDraggable.");
            return;
        }

        Debug.Log($"[DROPZONE] Quantidade disponível: {draggable.quantity}");

        if (draggable.quantity <= 0)
        {
            Debug.LogWarning("[DROPZONE] Carta sem quantidade disponível.");
            return;
        }

        if (!string.IsNullOrEmpty(assignedCardId))
        {
            Debug.LogWarning($"[DROPZONE] Slot já ocupado com '{assignedCardId}'");
            return;
        }

        if (acceptOnlyCreatures && !(draggable.cardAsset is Creature))
        {
            Debug.LogWarning("[DROPZONE] Este slot aceita apenas criaturas!");
            return;
        }

        assignedCardId = GetCardId(draggable.cardAsset);
        currentCardAsset = draggable.cardAsset;

        CreateCardDisplay();

        draggable.DecreaseQuantity();

        Debug.Log(
            $"[DROPZONE] Carta '{assignedCardId}' colocada no slot '{gameObject.name}' com sucesso!"
        );
    }

    void CreateCardDisplay()
    {
        if (currentCardDisplay != null)
        {
            Destroy(currentCardDisplay);
        }

        if (cardDisplayPrefab != null)
        {
            currentCardDisplay = Instantiate(cardDisplayPrefab, transform);
            CardView cardView = currentCardDisplay.GetComponent<CardView>();
            if (cardView != null)
            {
                cardView.cardAsset = currentCardAsset;
            }
        }
        else if (slotImage != null)
        {
            SetSlotImage();
        }

        if (currentCardDisplay != null)
        {
            RectTransform cardRect = currentCardDisplay.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchoredPosition = Vector2.zero;
                cardRect.localScale = Vector3.one;

                RectTransform slotRect = GetComponent<RectTransform>();
                if (slotRect != null)
                {
                    cardRect.sizeDelta = slotRect.sizeDelta * 0.9f; // 90% do tamanho do slot nao sei se quero isso, arrumar 
                }
            }
        }
    }

    void SetSlotImage()
    {
        if (slotImage != null && currentCardAsset != null)
        {
            Sprite cardSprite = GetCardSprite(currentCardAsset);
            if (cardSprite != null)
            {
                slotImage.sprite = cardSprite;
                slotImage.color = Color.white;
            }
        }
    }

    Sprite GetCardSprite(ScriptableObject card)
    {
        if (card is Creature c)
            return c.artworkImage;
        if (card is Attack a)
            return a.artworkImage;
        if (card is Battlegear b)
            return b.artworkImage;
        if (card is Location l)
            return l.artworkImage;
        if (card is Mugic m)
            return m.artworkImage;
        return null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[DROPZONE] Mouse entrou na dropzone '{gameObject.name}'");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[DROPZONE] Mouse saiu da dropzone '{gameObject.name}'");
    }

    string GetCardId(ScriptableObject card)
    {
        if (card is Creature c)
            return c.cardCode ?? c.name;
        if (card is Attack a)
            return a.cardCode ?? a.name;
        if (card is Battlegear b)
            return b.cardCode ?? b.name;
        if (card is Location l)
            return l.cardCode ?? l.name;
        if (card is Mugic m)
            return m.cardCode ?? m.name;
        return "unknown";
    }

    public void ClearSlot()
    {
        assignedCardId = "";
        currentCardAsset = null;

        if (currentCardDisplay != null)
        {
            Destroy(currentCardDisplay);
            currentCardDisplay = null;
        }

        if (slotImage != null)
        {
            slotImage.sprite = null;
            slotImage.color = new Color(1, 1, 1, 0.3f);
        }
    }

    public ScriptableObject GetCardAsset()
    {
        return currentCardAsset;
    }

    public bool IsOccupied()
    {
        return !string.IsNullOrEmpty(assignedCardId) && currentCardAsset != null;
    }
}
