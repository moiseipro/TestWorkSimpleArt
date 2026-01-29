using System;
using System.Collections.Generic;

namespace PopupModule
{
    public class UIPopupService
    {
        private readonly Dictionary<Type, UIPopupView> _popups = new();

        // ---------- REGISTRATION ----------

        public void Register<T>(T popup) where T : UIPopupView
        {
            var type = typeof(T);

            _popups[type] = popup;
            popup.Hide();
        }

        // ---------- SHOW / HIDE ----------

        public void Show<T>() where T : UIPopupView
        {
            if (_popups.TryGetValue(typeof(T), out var popup))
                popup.Show();
        }

        public void Hide<T>() where T : UIPopupView
        {
            if (_popups.TryGetValue(typeof(T), out var popup))
                popup.Hide();
        }

        public bool IsOpen<T>() where T : UIPopupView
        {
            return _popups.TryGetValue(typeof(T), out var popup)
                   && popup.IsActive;
        }

        // ---------- GET ----------

        public T Get<T>() where T : UIPopupView
        {
            return _popups.TryGetValue(typeof(T), out var popup)
                ? popup as T
                : null;
        }
    }
}