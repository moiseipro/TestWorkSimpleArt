using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ImageLoadModule
{
    public interface IImageDownloadService
    {
        UniTask<Texture2D> DownloadTexture(
            string url,
            IProgress<float> progress,
            CancellationToken ct);

        UniTask<Sprite> DownloadSprite(
            string url,
            IProgress<float> progress,
            CancellationToken ct);

        bool TryGetCachedSprite(string url, out Sprite sprite);
    }
}