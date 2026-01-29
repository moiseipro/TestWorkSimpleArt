using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardModule
{
    [RequireComponent(typeof(Button))]
    public class SortButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text sortText;
        [SerializeField] private GameObject selectedVisual;
        [SerializeField] private Color baseColor, selectedColor;
        [SerializeField] private SortType sortType;
        public SortType SortType => sortType;

        private Button _filterButton;
        
        public Action<SortType> OnSortActivate;

        private void Awake()
        {
            _filterButton = GetComponent<Button>();
            _filterButton.onClick.AddListener(() => OnSortActivate?.Invoke(sortType));
        }

        public void SelectVisual()
        {
            selectedVisual.SetActive(true);
            sortText.color = selectedColor;
        }

        public void DeselectVisual()
        {
            selectedVisual.SetActive(false);
            sortText.color = baseColor;
        }

        private void OnDisable()
        {
            _filterButton.onClick.RemoveAllListeners();
        }
    }
}