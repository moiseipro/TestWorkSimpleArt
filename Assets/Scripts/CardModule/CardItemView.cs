using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ImageLoadModule;
using PopupModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace CardModule
{
    public class CardItemView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RawImage image;
        [SerializeField] private GameObject loadingBlock;
        [SerializeField] private Image loadingIcon;
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private Image premiumBadge;

        [Header("Animation")]
        [SerializeField] private float showDuration = 0.25f;
        [SerializeField] private float hideDuration = 0.2f;
        [SerializeField] private float hiddenScale = 0.9f;

        private string _url;
        private IImageDownloadService _service;

        private CancellationTokenSource _cts;
        private Tween _loadingTween;
        private Tween _visibilityTween;

        private CanvasGroup _canvasGroup;

        private bool _isLoaded;
        private bool _isLoading;
        private bool _isVisible;

        public RectTransform Rect => transform as RectTransform;

        public void Init(string url, IImageDownloadService service, bool isPremium = false)
        {
            _url = url;
            _service = service;

            premiumBadge.gameObject.SetActive(isPremium);

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * hiddenScale;
            gameObject.SetActive(false);
            _isVisible = false;
        }
        
        public void Toggle(bool isShow)
        {
            if (isShow)
                Show();
            else
                Hide();
        }
        
        public void Show()
        {
            if (_isVisible)
                return;

            _isVisible = true;
            gameObject.SetActive(true);

            _visibilityTween?.Kill();
            
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * hiddenScale;

            _visibilityTween = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, showDuration))
                .Join(transform.DOScale(1f, showDuration).SetEase(Ease.OutBack))
                .SetUpdate(true);
        }

        public void Hide()
        {
            if (!_isVisible)
                return;

            _isVisible = false;

            CancelLoad();

            _visibilityTween?.Kill();
            _visibilityTween = null;
            
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * hiddenScale;
            gameObject.SetActive(false);
        }

        public void StartLoad()
        {
            if (_isLoaded || _isLoading)
                return;

            _isLoading = true;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            image.texture = null;
            loadingBlock.SetActive(true);

            StartLoadingAnimation();
            LoadAsync(_cts.Token).Forget();
        }

        public void CancelLoad()
        {
            if (!_isLoading || _isLoaded)
                return;

            _cts?.Cancel();
            StopLoadingAnimation();
            loadingBlock.SetActive(false);
            _isLoading = false;
        }

        private async UniTaskVoid LoadAsync(CancellationToken ct)
        {
            var progress = new Progress<float>(p =>
            {
                loadingText.text = $"{p:P0}";
            });

            var tex = await _service.DownloadTexture(_url, progress, ct);

            if (ct.IsCancellationRequested || tex == null)
                return;

            image.texture = tex;
            loadingBlock.SetActive(false);

            StopLoadingAnimation();
            _isLoaded = true;
            _isLoading = false;
        }

        private void StartLoadingAnimation()
        {
            _loadingTween?.Kill();
            loadingIcon.rectTransform.rotation = Quaternion.identity;

            _loadingTween = loadingIcon.rectTransform
                .DORotate(new Vector3(0, 0, -360f), 1f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetUpdate(true);
        }

        private void StopLoadingAnimation()
        {
            _loadingTween?.Kill();
            _loadingTween = null;
        }

        private void OnDisable()
        {
            _visibilityTween?.Kill();
            _loadingTween?.Kill();
            _cts?.Cancel();
        }
    }
}