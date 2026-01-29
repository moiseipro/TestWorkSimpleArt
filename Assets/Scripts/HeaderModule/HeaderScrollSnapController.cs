using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace HeaderModule
{
    public class HeaderScrollSnapController : MonoBehaviour
    {
        [SerializeField] private HorizontalScrollSnap scrollSnap;
        [SerializeField] private float switchInterval = 5f;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            _cts = new CancellationTokenSource();
            AutoScrollLoop(_cts.Token).Forget();
        }

        private async UniTaskVoid AutoScrollLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(switchInterval),
                    cancellationToken: ct);

                if (ct.IsCancellationRequested)
                    return;

                ScrollNext();
            }
        }

        private void ScrollNext()
        {
            int pages = scrollSnap.ChildObjects.Length;
            if (pages <= 1)
                return;

            int nextIndex = scrollSnap.CurrentPage + 1;

            if (nextIndex >= pages)
                nextIndex = 0;

            scrollSnap.GoToScreen(nextIndex);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
        }
    }
}