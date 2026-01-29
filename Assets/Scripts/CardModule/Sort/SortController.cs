using System;
using System.Linq;
using UnityEngine;


namespace CardModule
{
    public class SortController : MonoBehaviour
    {
        [SerializeField] private SortType startedSortType;
        private SortButton[] _filterButtons;

        private SortType _currentSortType;
        
        public Action<SortType> OnSortChange;
        
        private void Awake()
        {
            _filterButtons = GetComponentsInChildren<SortButton>();
            _currentSortType = startedSortType;
            foreach (var filterButton in _filterButtons)
            {
                filterButton.OnSortActivate += OnSortActivate;
            }

            
        }

        private void Start()
        {
            OnSortActivate(_currentSortType);
        }

        private void OnSortActivate(SortType sortType)
        {
            _currentSortType = sortType;
            
            foreach (var filterButton in _filterButtons)
            {
                if (filterButton.SortType == sortType)
                {
                    filterButton.SelectVisual();
                }
                else
                {
                    filterButton.DeselectVisual();
                }
            }
            
            OnSortChange?.Invoke(_currentSortType);
        }
    }
}