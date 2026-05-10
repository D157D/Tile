using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Dependencies")]
    public GameConfig config;
    public RectTransform boardArea;
    public RectTransform trayArea;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip matchSound;

    public event Action OnGameWon;
    public event Action OnGameLost;

    private BoardModel boardModel;
    private TrayModel trayModel;
    private TrayView trayView;

    private RectTransform boardContent;
    private Dictionary<TileModel, TileView> tileViews = new Dictionary<TileModel, TileView>();
    
    private bool isHandlingMatch = false;

    private void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        trayView = trayArea.GetComponent<TrayView>();
        if (trayView == null) trayView = trayArea.gameObject.AddComponent<TrayView>();
        trayView.Initialize(trayArea, config);
    }

    public void StartGame(int level)
    {
        ClearBoard();

        if (config == null)
        {
            Debug.LogError("GameConfig is missing! Please assign it in the inspector.");
            return;
        }

        if (boardContent == null)
        {
            GameObject contentGO = new GameObject("BoardContent");
            boardContent = contentGO.AddComponent<RectTransform>();
            boardContent.SetParent(boardArea, false);
            boardContent.anchorMin = new Vector2(0.5f, 0.5f);
            boardContent.anchorMax = new Vector2(0.5f, 0.5f);
            boardContent.anchoredPosition = Vector2.zero;
        }

        trayModel = new TrayModel(config.maxTraySlots);
        trayModel.OnTileAddedToTray += HandleTileAddedToTray;
        trayModel.OnTilesMatched += HandleTilesMatched;
        trayModel.OnTrayFull += () => OnGameLost?.Invoke();
        trayModel.OnTrayChanged += CheckWinCondition;

        boardModel = new BoardModel();

        GenerateBoard(level);
    }

    private void GenerateBoard(int level)
    {
        List<TileModel> models;

        if (level <= LevelData.AllLayouts.Length)
        {
            TilePos[] currentLayout = LevelData.GetLayout(level);
            models = LevelGenerator.FillPredefined(currentLayout, config.icons);
        }
        else
        {
            models = LevelGenerator.GenerateProcedural(level, config.icons, config.maxTraySlots);
        }

        models = models.OrderBy(m => m.Z).ToList();

        float minX = models.Min(p => p.X);
        float maxX = models.Max(p => p.X);
        float minY = models.Min(p => p.Y);
        float maxY = models.Max(p => p.Y);

        float layoutWidth = (maxX - minX) * (config.tileWidth - GameConstants.TILE_WIDTH_OFFSET) + config.tileWidth;
        float layoutHeight = (maxY - minY) * (config.tileHeight - GameConstants.TILE_HEIGHT_OFFSET) + config.tileHeight;

        float availableWidth = boardArea.rect.width > 0 ? boardArea.rect.width - GameConstants.BOARD_PADDING : 1000f;
        float availableHeight = boardArea.rect.height > 0 ? boardArea.rect.height - GameConstants.BOARD_PADDING : 1000f;

        float scaleX = availableWidth / layoutWidth;
        float scaleY = availableHeight / layoutHeight;
        float scale = Mathf.Min(scaleX, scaleY, 1f);

        boardContent.localScale = new Vector3(scale, scale, 1f);

        float offsetX = (minX + maxX) / 2f * (config.tileWidth - GameConstants.TILE_WIDTH_OFFSET);
        float offsetY = (minY + maxY) / 2f * (config.tileHeight - GameConstants.TILE_HEIGHT_OFFSET);
        boardContent.anchoredPosition = new Vector2(-offsetX * scale, -offsetY * scale);

        for (int i = 0; i < models.Count; i++)
        {
            TileModel model = models[i];
            int id = model.Id;

            GameObject tileGO = config.tilePrefab != null ? Instantiate(config.tilePrefab, boardContent) : new GameObject($"Tile_{id}");
            if (config.tilePrefab == null) tileGO.transform.SetParent(boardContent, false);
            tileGO.name = $"Tile_{id}";

            TileView view = tileGO.GetComponent<TileView>();
            if (view == null) view = tileGO.AddComponent<TileView>();

            view.Initialize(model, config.tileWidth, config.tileHeight);
            view.OnClicked += HandleTileClicked;

            tileViews[model] = view;
        }

        boardModel.Initialize(models);
    }

    private void HandleTileClicked(TileView view)
    {
        TileModel model = view.Model;
        if (!canTap(model)) return;

        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);

        trayModel.AddTile(model);
        boardModel.UpdateBlockedStatus();
    }

    private async void HandleTileAddedToTray(TileModel model, int index)
    {
        if (tileViews.TryGetValue(model, out var view))
        {
            trayView.AddTileView(model, view);
        }
        await trayView.UpdateTrayLayoutAsync(trayModel.GetTiles());
    }

    private async void HandleTilesMatched(List<TileModel> matchedModels)
    {
        isHandlingMatch = true;
        
        if (shouldAnimate())
        {
            await Task.Delay(TimeSpan.FromSeconds(config.animationDuration));
        }
        else
        {
            await Task.Delay(TimeSpan.FromSeconds(GameConstants.TILE_MATCH_DELAY));
        }

        if (audioSource != null && matchSound != null) audioSource.PlayOneShot(matchSound);
        
        await trayView.AnimateMatchesAsync(matchedModels);
        await Task.Delay(TimeSpan.FromSeconds(GameConstants.TILE_DESTROY_DELAY));
        
        foreach(var m in matchedModels) tileViews.Remove(m);
        
        await trayView.UpdateTrayLayoutAsync(trayModel.GetTiles());
        
        isHandlingMatch = false;
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (hasMatched()) return;
        if (boardModel.GetRemainingBoardTilesCount() == 0 && trayModel.GetCount() == 0)
        {
            OnGameWon?.Invoke();
        }
    }

    private bool isExposed(TileModel model) => model.Status == TileStatus.Board && !model.IsBlocked;
    private bool canTap(TileModel model) => isExposed(model) && trayModel.CanAdd();
    private bool shouldAnimate() => config != null;
    private bool hasMatched() => isHandlingMatch;

    public void ClearBoard()
    {
        foreach (var kvp in tileViews)
        {
            if (kvp.Value != null && kvp.Value.gameObject != null) Destroy(kvp.Value.gameObject);
        }
        tileViews.Clear();
        if (trayView != null) trayView.Clear();
    }
}
