using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ScriptableObject cardAsset;
    public int quantity = 0;
    public GameObject dragVisualPrefab;

    private GameObject draggingObject;
    private Canvas canvas;

    void Start()
    {
        CardViewerBuild view = GetComponent<CardViewerBuild>();
        if (view != null)
        {
            cardAsset = view.cardAsset;
        }
        canvas = GetComponentInParent<Canvas>();
        UpdateVisual();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantity <= 0)
        {
            return;
        }

        draggingObject = Instantiate(dragVisualPrefab, canvas.transform);

        var draggingImage = draggingObject.GetComponent<Image>();
        if (draggingImage != null)
        {
            draggingImage.raycastTarget = false;
        }

        var allImages = draggingObject.GetComponentsInChildren<Image>();
        foreach (var img in allImages)
        {
            img.raycastTarget = false;
        }

        RectTransform rt = draggingObject.GetComponent<RectTransform>();
        if (rt != null)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out pos
            );
            rt.localPosition = pos;
        }

        Transform originalArtwork = transform.Find("CardDetails/Artwork");

        if (originalArtwork != null)
        {
            Image originalImage = originalArtwork.GetComponent<Image>();
            Image dragImage = draggingObject.GetComponent<Image>();

            if (originalImage != null && dragImage != null)
            {
                dragImage.sprite = originalImage.sprite;
                dragImage.SetNativeSize();
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingObject)
        {
            RectTransform rt = draggingObject.GetComponent<RectTransform>();
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out pos
            );
            rt.localPosition = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        foreach (var hoveredObject in eventData.hovered)
        {
            var dropZone = hoveredObject.GetComponent<DropZone>();
            if (dropZone != null)
            {
                Debug.Log($"[DRAG] DropZone encontrada: {hoveredObject.name}");
            }
        }

        if (draggingObject)
            Destroy(draggingObject);
    }

    public void DecreaseQuantity()
    {
        quantity--;
        if (quantity < 0)
            quantity = 0;
        UpdateVisual();
    }

    public void IncrementQuantity()
    {
        quantity++;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        var images = GetComponentsInChildren<Image>();
        if (images != null)
        {
            Color colorToSet = quantity > 0 ? Color.white : new Color(0.1f, 0.1f, 0.1f);

            foreach (var img in images)
            {
                img.color = colorToSet;
            }
        }
    }
}
