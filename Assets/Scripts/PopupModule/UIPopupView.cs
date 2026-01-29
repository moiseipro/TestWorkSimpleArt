using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PopupModule
{
    public abstract class UIPopupView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] protected Button closeButton;
        [SerializeField] protected CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] protected float showDuration = 0.25f;
        [SerializeField] protected float hideDuration = 0.2f;
        [SerializeField] protected Ease showEase = Ease.OutBack;
        [SerializeField] protected Ease hideEase = Ease.InBack;
        [SerializeField] protected float startScale = 0.9f;

        protected bool _isActive;
        public bool IsActive => _isActive;

        private Tween _currentTween;
        private RectTransform _rectTransform;

        protected virtual void Awake()
        {
            _rectTransform = transform as RectTransform;

            if (canvasGroup == null)
                canvasGroup = gameObject.GetComponent<CanvasGroup>()
                               ?? gameObject.AddComponent<CanvasGroup>();

            closeButton?.onClick.AddListener(Hide);
            
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            _rectTransform.localScale = Vector3.one * startScale;

            gameObject.SetActive(false);
        }

        public virtual void Show()
        {
            if (_isActive)
                return;

            _isActive = true;
            gameObject.SetActive(true);

            KillTween();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            _rectTransform.localScale = Vector3.one * startScale;

            _currentTween = DOTween.Sequence()
                .Join(canvasGroup.DOFade(1f, showDuration))
                .Join(_rectTransform.DOScale(1f, showDuration).SetEase(showEase));
        }

        public virtual void Hide()
        {
            if (!_isActive)
                return;

            _isActive = false;

            KillTween();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            _currentTween = DOTween.Sequence()
                .Join(canvasGroup.DOFade(0f, hideDuration))
                .Join(_rectTransform.DOScale(startScale, hideDuration).SetEase(hideEase))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
        }

        private void KillTween()
        {
            if (_currentTween != null && _currentTween.IsActive())
                _currentTween.Kill();
        }
    }
}