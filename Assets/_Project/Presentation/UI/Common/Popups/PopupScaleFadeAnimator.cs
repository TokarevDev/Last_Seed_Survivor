namespace Game.Presentation.UI.Common.Popups
{
    using System;
    using System.Collections.Generic;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class PopupScaleFadeAnimator
    {
        private const string AnimatedContentRootName = "AnimatedContentRoot";

        private readonly RectTransform _contentRoot;
        private readonly CanvasGroup _contentCanvasGroup;
        private readonly Image _backgroundImage;
        private readonly Vector3 _contentBaseScale;
        private readonly float _backgroundBaseAlpha;

        private Sequence _sequence;

        public PopupScaleFadeAnimator(Component popup, RectTransform contentRoot)
        {
            if (popup == null)
                throw new ArgumentNullException(nameof(popup));

            _contentRoot = contentRoot != null
                ? contentRoot
                : CreateAnimatedContentRoot(popup.transform, popup.gameObject.layer);
            _contentBaseScale = _contentRoot != null
                ? _contentRoot.localScale
                : Vector3.one;

            if (_contentRoot != null
                && !_contentRoot.TryGetComponent(out _contentCanvasGroup))
            {
                _contentCanvasGroup = _contentRoot.gameObject.AddComponent<CanvasGroup>();
            }

            if (popup.TryGetComponent(out _backgroundImage))
                _backgroundBaseAlpha = _backgroundImage.color.a;
        }

        public void PlayShow(float duration, float startScale)
        {
            KillActiveSequence();

            float safeDuration = Mathf.Max(0f, duration);
            float safeScale = Mathf.Clamp(startScale, 0.5f, 1f);

            if (safeDuration <= 0f)
            {
                Restore();
                return;
            }

            SetState(safeScale, 0f, 0f);
            _sequence = DOTween.Sequence().SetUpdate(true);

            if (_contentRoot != null)
            {
                _sequence.Join(
                    _contentRoot
                        .DOScale(_contentBaseScale, safeDuration)
                        .SetEase(Ease.InOutSine));
            }

            if (_contentCanvasGroup != null)
            {
                _sequence.Join(
                    _contentCanvasGroup
                        .DOFade(1f, safeDuration)
                        .SetEase(Ease.OutSine));
            }

            if (_backgroundImage != null && _backgroundBaseAlpha > 0f)
            {
                _sequence.Join(
                    _backgroundImage
                        .DOFade(_backgroundBaseAlpha, safeDuration)
                        .SetEase(Ease.OutSine));
            }

            _sequence.OnComplete(() => _sequence = null);
        }

        public void PlayHide(
            float duration,
            float targetScale,
            Action onComplete)
        {
            KillActiveSequence();

            float safeDuration = Mathf.Max(0f, duration);
            float safeScale = Mathf.Clamp(targetScale, 0.5f, 1f);

            if (safeDuration <= 0f)
            {
                onComplete?.Invoke();
                return;
            }

            _sequence = DOTween.Sequence().SetUpdate(true);

            if (_contentRoot != null)
            {
                _sequence.Join(
                    _contentRoot
                        .DOScale(_contentBaseScale * safeScale, safeDuration)
                        .SetEase(Ease.InOutSine));
            }

            if (_contentCanvasGroup != null)
            {
                _sequence.Join(
                    _contentCanvasGroup
                        .DOFade(0f, safeDuration)
                        .SetEase(Ease.InSine));
            }

            if (_backgroundImage != null && _backgroundBaseAlpha > 0f)
            {
                _sequence.Join(
                    _backgroundImage
                        .DOFade(0f, safeDuration)
                        .SetEase(Ease.InSine));
            }

            _sequence.OnComplete(() =>
            {
                _sequence = null;
                onComplete?.Invoke();
            });
        }

        public void CancelAndRestore()
        {
            KillActiveSequence();
            Restore();
        }

        public void Restore()
        {
            SetState(1f, 1f, _backgroundBaseAlpha);
        }

        private void KillActiveSequence()
        {
            _sequence?.Kill(false);
            _sequence = null;
        }

        private void SetState(
            float contentScaleMultiplier,
            float contentAlpha,
            float backgroundAlpha)
        {
            float safeScale = Mathf.Clamp(contentScaleMultiplier, 0.5f, 1f);

            if (_contentRoot != null)
                _contentRoot.localScale = _contentBaseScale * safeScale;

            if (_contentCanvasGroup != null)
                _contentCanvasGroup.alpha = Mathf.Clamp01(contentAlpha);

            if (_backgroundImage != null)
            {
                Color color = _backgroundImage.color;
                color.a = Mathf.Clamp01(backgroundAlpha);
                _backgroundImage.color = color;
            }
        }

        private static RectTransform CreateAnimatedContentRoot(
            Transform popupRoot,
            int layer)
        {
            int childCount = popupRoot.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = popupRoot.GetChild(i);

                if (child != null && child.name == AnimatedContentRootName)
                    return child as RectTransform;
            }

            if (childCount <= 0)
                return popupRoot as RectTransform;

            List<RectTransform> contentChildren = new(childCount);

            for (int i = 0; i < childCount; i++)
            {
                if (popupRoot.GetChild(i) is RectTransform childRect)
                    contentChildren.Add(childRect);
            }

            GameObject contentObject = new(AnimatedContentRootName)
            {
                layer = layer
            };
            RectTransform contentRoot = contentObject.AddComponent<RectTransform>();
            contentRoot.SetParent(popupRoot, false);
            contentRoot.anchorMin = Vector2.zero;
            contentRoot.anchorMax = Vector2.one;
            contentRoot.offsetMin = Vector2.zero;
            contentRoot.offsetMax = Vector2.zero;
            contentRoot.pivot = new Vector2(0.5f, 0.5f);
            contentRoot.localScale = Vector3.one;
            contentRoot.SetAsLastSibling();

            for (int i = 0; i < contentChildren.Count; i++)
                contentChildren[i].SetParent(contentRoot, false);

            return contentRoot;
        }
    }
}
