using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TrayView : MonoBehaviour
{
    private RectTransform trayRect;
    private GameConfig config;
    
    private Dictionary<TileModel, TileView> tileViews = new Dictionary<TileModel, TileView>();
    private Dictionary<TileModel, CancellationTokenSource> moveCts = new Dictionary<TileModel, CancellationTokenSource>();

    public void Initialize(RectTransform rectTransform, GameConfig gameConfig)
    {
        trayRect = rectTransform;
        config = gameConfig;
    }

    public void AddTileView(TileModel model, TileView view)
    {
        tileViews[model] = view;
        view.transform.SetParent(trayRect, true);
    }

    public async Task UpdateTrayLayoutAsync(IReadOnlyList<TileModel> trayTiles)
    {
        List<Task> moveTasks = new List<Task>();
        for (int i = 0; i < trayTiles.Count; i++)
        {
            var model = trayTiles[i];
            if (tileViews.TryGetValue(model, out var view))
            {
                view.transform.SetAsLastSibling();
                float startX = -(config.maxTraySlots * (config.tileWidth + GameConstants.TILE_WIDTH_OFFSET)) / 2f + (config.tileWidth / 2f) + 10f;
                float targetX = startX + i * (config.tileWidth + GameConstants.TILE_WIDTH_OFFSET);
                Vector2 targetPos = new Vector2(targetX, 0);

                if (moveCts.TryGetValue(model, out var existingCts))
                {
                    existingCts.Cancel();
                    existingCts.Dispose();
                }
                
                var cts = new CancellationTokenSource();
                moveCts[model] = cts;

                moveTasks.Add(MoveToPositionAsync(view.RectTransform, targetPos, cts.Token));
            }
        }

        try {
            await Task.WhenAll(moveTasks);
        } catch (OperationCanceledException) { }
    }

    private async Task MoveToPositionAsync(RectTransform rect, Vector2 targetAnchoredPos, CancellationToken token)
    {
        if (rect == null) return;

        Vector2 startPos = rect.anchoredPosition;
        Vector3 startScale = rect.localScale;
        Vector2 startSize = rect.sizeDelta;
        Vector2 targetSize = startSize;

        float elapsedTime = 0;

        while (elapsedTime < config.animationDuration)
        {
            if (rect == null) return;
            token.ThrowIfCancellationRequested();

            float t = elapsedTime / config.animationDuration;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetAnchoredPos, t);
            rect.localScale = Vector3.Lerp(startScale, Vector3.one, t);
            rect.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        if (rect != null)
        {
            rect.anchoredPosition = targetAnchoredPos;
            rect.localScale = Vector3.one;
            rect.sizeDelta = targetSize;
        }
    }

    public async Task AnimateMatchesAsync(List<TileModel> matchedModels)
    {
        List<Task> destroyTasks = new List<Task>();
        foreach (var model in matchedModels)
        {
            if (tileViews.TryGetValue(model, out var view))
            {
                tileViews.Remove(model);
                if (moveCts.TryGetValue(model, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                    moveCts.Remove(model);
                }
                destroyTasks.Add(ScaleAndDestroyAsync(view.gameObject));
            }
        }
        await Task.WhenAll(destroyTasks);
    }

    private async Task ScaleAndDestroyAsync(GameObject go)
    {
        if (go == null) return;
        RectTransform rect = go.GetComponent<RectTransform>();
        float elapsedTime = 0;
        Vector3 startScale = rect.localScale;

        while (elapsedTime < GameConstants.TILE_SHRINK_DURATION)
        {
            if (rect == null) return;
            rect.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsedTime / GameConstants.TILE_SHRINK_DURATION);
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }
        if (go != null) Destroy(go);
    }

    public void Clear()
    {
        foreach (var cts in moveCts.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        moveCts.Clear();

        foreach (var kvp in tileViews)
        {
            if (kvp.Value != null) Destroy(kvp.Value.gameObject);
        }
        tileViews.Clear();
    }
}
