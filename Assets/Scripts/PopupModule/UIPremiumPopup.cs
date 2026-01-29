using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PopupModule
{
    public class UIPremiumPopup : UIPopupView
    {
        [SerializeField] private Button continueButton;

        [Inject]
        protected void Constructor(UIPopupService popupService)
        {
            popupService.Register(this);
        }

        protected override void Awake()
        {
            base.Awake();
            continueButton.onClick.AddListener(Hide);
        }
    }
}