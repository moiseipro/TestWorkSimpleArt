using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ImageLoadModule
{
    public class ImageDownloadService : IImageDownloadService
    {
        private readonly SemaphoreSlim _semaphore;
        
        private readonly Dictionary<string, Texture2D> _textureCache = new();
        private readonly Dictionary<string, Sprite> _spriteCache = new();
        
        private readonly Dictionary<string, UniTask<Texture2D>> _inFlight =
            new();

        public ImageDownloadService(int maxParallelDownloads = 4)
        {
            _semaphore = new SemaphoreSlim(maxParallelDownloads);
        }

        public async UniTask<Texture2D> DownloadTexture(
            string url,
            IProgress<float> progress,
            CancellationToken ct)
        {
            if (_textureCache.TryGetValue(url, out var cached))
            {
                progress?.Report(1f);
                return cached;
            }
            
            if (_inFlight.TryGetValue(url, out var inFlightTask))
                return await inFlightTask;

            var task = DownloadInternal(url, progress, ct);
            _inFlight[url] = task;

            try
            {
                var tex = await task;
                if (tex != null)
                    _textureCache[url] = tex;

                return tex;
            }
            finally
            {
                _inFlight.Remove(url);
            }
        }

        public async UniTask<Sprite> DownloadSprite(
            string url,
            IProgress<float> progress,
            CancellationToken ct)
        {
            // 1. Уже есть спрайт
            if (_spriteCache.TryGetValue(url, out var cachedSprite))
            {
                progress?.Report(1f);
                return cachedSprite;
            }

            // 2. Есть текстура, но ещё нет спрайта
            if (_textureCache.TryGetValue(url, out var cachedTexture))
            {
                var spriteFromCache = CreateSprite(cachedTexture);
                _spriteCache[url] = spriteFromCache;
                progress?.Report(1f);
                return spriteFromCache;
            }

            // 3. Ничего нет — качаем
            var tex = await DownloadTexture(url, progress, ct);
            if (tex == null)
                return null;

            var sprite = CreateSprite(tex);
            _spriteCache[url] = sprite;

            return sprite;
        }

        private static Sprite CreateSprite(Texture2D tex)
        {
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
        }
        
        public bool TryGetCachedSprite(string url, out Sprite sprite)
        {
            if (_spriteCache.TryGetValue(url, out sprite))
                return true;

            if (_textureCache.TryGetValue(url, out var tex))
            {
                sprite = CreateSprite(tex);
                _spriteCache[url] = sprite;
                return true;
            }

            sprite = null;
            return false;
        }

        private async UniTask<Texture2D> DownloadInternal(
            string url,
            IProgress<float> progress,
            CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);

            try
            {
                using var request =
                    UnityWebRequestTexture.GetTexture(url);

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    progress?.Report(operation.progress);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        $"[ImageDownload] {url} -> {request.error}");
                    return null;
                }

                progress?.Report(1f);
                return DownloadHandlerTexture.GetContent(request);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}