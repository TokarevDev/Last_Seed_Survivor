using Game.Presentation.UI.Common.Popups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    public sealed class PopupScaleFadeAnimatorTests
    {
        [Test]
        public void CancelAndRestore_RestoresBaseScaleAndVisibleAlphas()
        {
            GameObject popupObject = new("Popup", typeof(RectTransform), typeof(Image));
            GameObject contentObject = new(
                "Content",
                typeof(RectTransform),
                typeof(CanvasGroup));
            RectTransform contentRoot = contentObject.GetComponent<RectTransform>();
            contentRoot.SetParent(popupObject.transform, false);
            contentRoot.localScale = new Vector3(1.2f, 0.8f, 1f);
            CanvasGroup contentCanvasGroup = contentObject.GetComponent<CanvasGroup>();
            contentCanvasGroup.alpha = 0.75f;
            Image background = popupObject.GetComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.6f);

            try
            {
                PopupScaleFadeAnimator animator = new(
                    popupObject.transform,
                    contentRoot);

                animator.PlayShow(1f, 0.5f);
                animator.CancelAndRestore();

                Assert.That(contentRoot.localScale, Is.EqualTo(new Vector3(1.2f, 0.8f, 1f)));
                Assert.That(contentCanvasGroup.alpha, Is.EqualTo(1f));
                Assert.That(background.color.a, Is.EqualTo(0.6f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(popupObject);
            }
        }
    }
}
