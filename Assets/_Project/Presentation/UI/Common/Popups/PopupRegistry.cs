using System;
using System.Collections.Generic;
using Game.Core.Collections;

namespace Game.Presentation.UI.Common.Popups
{
    public enum PopupRegistrationResult
    {
        Registered,
        AlreadyRegistered,
        DuplicateId
    }

    public sealed class PopupRegistry
    {
        private readonly Dictionary<string, PopupView> _popupsById = new();
        private readonly OrderedReferenceSet<PopupView> _popups = new();
        private readonly Action<PopupView> _closeRequestedHandler;

        public PopupRegistry(Action<PopupView> closeRequestedHandler)
        {
            _closeRequestedHandler = closeRequestedHandler ??
                throw new ArgumentNullException(nameof(closeRequestedHandler));
        }

        public int Count => _popups.Count;
        public IReadOnlyList<PopupView> Items => _popups.Items;

        public bool TryGet(string popupId, out PopupView popup)
        {
            if (string.IsNullOrEmpty(popupId))
            {
                popup = null;
                return false;
            }

            return _popupsById.TryGetValue(popupId, out popup);
        }

        public PopupRegistrationResult Register(PopupView popup)
        {
            if (popup == null)
                throw new ArgumentNullException(nameof(popup));

            if (!_popups.Add(popup))
                return PopupRegistrationResult.AlreadyRegistered;

            popup.CloseRequested += _closeRequestedHandler;
            string popupId = popup.PopupId;

            if (_popupsById.ContainsKey(popupId))
                return PopupRegistrationResult.DuplicateId;

            try
            {
                _popupsById.Add(popupId, popup);
                return PopupRegistrationResult.Registered;
            }
            catch
            {
                popup.CloseRequested -= _closeRequestedHandler;
                _popups.RemoveAll(new[] { popup }, out _);
                throw;
            }
        }

        public void Clear()
        {
            for (int index = 0; index < _popups.Count; index++)
            {
                PopupView popup = _popups.Items[index];

                if (popup != null)
                    popup.CloseRequested -= _closeRequestedHandler;
            }

            _popupsById.Clear();
            _popups.Clear();
        }
    }
}
