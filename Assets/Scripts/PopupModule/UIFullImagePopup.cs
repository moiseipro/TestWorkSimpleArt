using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PopupModule
{
    public class UIFullImagePopup : UIPopupView
    {
        [SerializeField] private Image _fullImage;
        
        [Inject]
        protected void Constructor(UIPopupService popupService)
        {
            popupService.Register(this);
            
        }
        
        public void Show(Sprite sprite)
        {
            _fullImage.sprite = sprite;
            Show();
        }
    }
}