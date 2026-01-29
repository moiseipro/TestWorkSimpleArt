using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using ImageLoadModule;
using PopupModule;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace CardModule
{
    public class CardScrollController : MonoBehaviour
    {
        [SerializeField] private CardItemView itemPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private ScrollRect scrollRect;

        [SerializeField] private string baseUrl =
            "http://data.ikppbb.com/test-task-unity-data/pics/";

        [SerializeField] private int maxImages = 66;
        [SerializeField] private SortController sortController;

        [Inject] private UIPopupService _uiPopupService;
        [Inject] private IImageDownloadService _downloadService;
        [Inject] private IObjectResolver _objectResolver;
        
        private readonly List<CardItemView> _items = new();

        private RectTransform _viewport;

        private void Awake()
        {
            _downloadService = new ImageDownloadService(maxParallelDownloads: 4);

            _viewport = scrollRect.viewport;
            
            sortController.OnSortChange += OnSortChange;

            CreateItems();
            scrollRect.onValueChanged.AddListener(_ => UpdateVisibility());
        }
        
        private void Start()
        {
            UpdateVisibility();
        }

        private void OnSortChange(SortType sortType)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                switch (sortType)
                {
                    case SortType.All:
                        _items[i].Show();
                        break;
                    case SortType.Odd:
                        _items[i].Toggle(i % 2 != 1);
                        break;
                    case SortType.Even:
                        _items[i].Toggle(i % 2 == 1);
                        break;
                }
            }
            ForceLayoutRebuild();
            UpdateVisibility();
        }

        private void ForceLayoutRebuild()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        }

        private void CreateItems()
        {
            for (int i = 1; i <= maxImages; i++)
            {
                var item = _objectResolver.Instantiate(itemPrefab, content);
                var url = $"{baseUrl}{i}.jpg";
                
                var isPremium = i % 4 == 0;
                item.Init(url, _downloadService, isPremium);
                item.GetAsyncPointerClickTrigger().Subscribe(_ =>
                {
                    if (isPremium)
                    {
                        _uiPopupService.Get<UIPremiumPopup>().Show();
                    }
                    else
                    {
                        if (_downloadService.TryGetCachedSprite(url, out Sprite sprite))
                        {
                            _uiPopupService.Get<UIFullImagePopup>().Show(sprite);
                        }
                        
                    }
                });

                _items.Add(item);
            }
        }

        private void UpdateVisibility()
        {
            var viewportWorld = GetWorldRect(_viewport);

            foreach (var item in _items)
            {
                var itemRect = GetWorldRect(item.Rect);

                if (itemRect.Overlaps(viewportWorld))
                    item.StartLoad();
                else
                    item.CancelLoad();
            }
        }

        private static Rect GetWorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return new Rect(
                corners[0],
                corners[2] - corners[0]);
        }
    }
}