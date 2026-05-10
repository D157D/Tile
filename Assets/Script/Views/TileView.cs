using System;
using UnityEngine;
using UnityEngine.UI;

public class TileView : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public Button button;

    public TileModel Model { get; private set; }
    public RectTransform RectTransform { get; private set; }

    public event Action<TileView> OnClicked;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }

    public void Initialize(TileModel model, float width, float height)
    {
        Model = model;
        
        RectTransform.sizeDelta = new Vector2(width, height);
        float px = model.X * (width - GameConstants.TILE_WIDTH_OFFSET);
        float py = model.Y * (height - GameConstants.TILE_HEIGHT_OFFSET);
        RectTransform.anchoredPosition = new Vector2(px, py);

        if (iconImage == null)
        {
            Image[] allImages = GetComponentsInChildren<Image>();
            if (allImages.Length > 1) iconImage = allImages[1]; 
        }

        if (iconImage != null) iconImage.sprite = model.Icon;

        Model.OnBlockedChanged += HandleBlockedChanged;
        HandleBlockedChanged(Model.IsBlocked);
    }

    private void HandleBlockedChanged(bool isBlocked)
    {
        if (iconImage != null) iconImage.color = isBlocked ? new Color32(100, 100, 100, 255) : Color.white;
        if (button != null) button.interactable = !isBlocked;
    }

    private void OnDestroy()
    {
        if (Model != null)
        {
            Model.OnBlockedChanged -= HandleBlockedChanged;
        }
    }
}
