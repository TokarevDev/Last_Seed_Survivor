namespace Game.Presentation.UI.Rewards
{
    using DG.Tweening;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/UI/Reward Popup Animation Config")]
    public sealed class RewardPopupAnimationConfig : ScriptableObject
    {
        [Header("Popup Animation")]
        [SerializeField][Min(0f)] private float _rootFadeDuration = 0.12f;
        [SerializeField] private float _topEnterOffset = 160f;
        [SerializeField][Min(0f)] private float _topEnterDuration = 0.28f;
        [SerializeField] private float _rewardEnterOffset = -230f;
        [SerializeField][Min(0f)] private float _rewardEnterDuration = 0.32f;
        [SerializeField][Min(0f)] private float _rewardEnterStagger = 0.045f;
        [SerializeField] private float _actionEnterOffset = -90f;
        [SerializeField][Min(0f)] private float _actionEnterDuration = 0.16f;
        [SerializeField] private Ease _topEnterEase = Ease.OutCubic;
        [SerializeField] private Ease _rewardEnterEase = Ease.OutCubic;
        [SerializeField] private Ease _rewardScaleEase = Ease.OutBack;

        [Header("Reward Refresh Animation")]
        [SerializeField][Min(0f)] private float _refreshCardStagger = 0.055f;
        [SerializeField][Min(0f)] private float _refreshOutDuration = 0.12f;
        [SerializeField][Min(0f)] private float _refreshInDuration = 0.22f;
        [SerializeField] private Ease _refreshOutEase = Ease.InCubic;
        [SerializeField] private Ease _refreshInEase = Ease.OutBack;

        [Header("Selection Dismiss Animation")]
        [SerializeField][Min(0f)] private float _selectionFocusDuration = 0.22f;
        [SerializeField][Min(0f)] private float _selectionGrowDuration = 0.16f;
        [SerializeField][Min(0f)] private float _selectionExitDuration = 0.22f;
        [SerializeField][Min(0f)] private float _selectionScaleMultiplier = 1.05f;
        [SerializeField][Min(0f)] private float _selectionExitScaleMultiplier = 0.96f;
        [SerializeField] private float _selectionExitOffset = -230f;
        [SerializeField][Min(0f)] private float _unselectedExitDuration = 0.22f;
        [SerializeField][Min(0f)] private float _unselectedExitStagger = 0.045f;
        [SerializeField][Min(0f)] private float _unselectedExitScaleMultiplier = 0.96f;
        [SerializeField] private float _unselectedExitOffset = -230f;
        [SerializeField] private float _topExitOffset = 160f;
        [SerializeField] private float _actionExitOffset = -90f;
        [SerializeField] private Ease _selectionFocusEase = Ease.OutBack;
        [SerializeField] private Ease _selectionExitEase = Ease.InCubic;
        [SerializeField] private Ease _unselectedExitEase = Ease.InCubic;

        public RewardPopupAnimationSettings CreateSettings()
        {
            return new RewardPopupAnimationSettings(
                _rootFadeDuration,
                _topEnterOffset,
                _topEnterDuration,
                _rewardEnterOffset,
                _rewardEnterDuration,
                _rewardEnterStagger,
                _actionEnterOffset,
                _actionEnterDuration,
                _topEnterEase,
                _rewardEnterEase,
                _rewardScaleEase,
                _refreshCardStagger,
                _refreshOutDuration,
                _refreshInDuration,
                _refreshOutEase,
                _refreshInEase,
                _selectionFocusDuration,
                _selectionGrowDuration,
                _selectionExitDuration,
                _selectionScaleMultiplier,
                _selectionExitScaleMultiplier,
                _selectionExitOffset,
                _unselectedExitDuration,
                _unselectedExitStagger,
                _unselectedExitScaleMultiplier,
                _unselectedExitOffset,
                _topExitOffset,
                _actionExitOffset,
                _selectionFocusEase,
                _selectionExitEase,
                _unselectedExitEase);
        }

        private void OnValidate()
        {
            _rootFadeDuration = Mathf.Max(0f, _rootFadeDuration);
            _topEnterDuration = Mathf.Max(0f, _topEnterDuration);
            _rewardEnterDuration = Mathf.Max(0f, _rewardEnterDuration);
            _rewardEnterStagger = Mathf.Max(0f, _rewardEnterStagger);
            _actionEnterDuration = Mathf.Max(0f, _actionEnterDuration);
            _refreshCardStagger = Mathf.Max(0f, _refreshCardStagger);
            _refreshOutDuration = Mathf.Max(0f, _refreshOutDuration);
            _refreshInDuration = Mathf.Max(0f, _refreshInDuration);
            _selectionFocusDuration = Mathf.Max(0f, _selectionFocusDuration);
            _selectionGrowDuration = Mathf.Max(0f, _selectionGrowDuration);
            _selectionExitDuration = Mathf.Max(0f, _selectionExitDuration);
            _selectionScaleMultiplier = Mathf.Max(0f, _selectionScaleMultiplier);
            _selectionExitScaleMultiplier = Mathf.Max(0f, _selectionExitScaleMultiplier);
            _unselectedExitDuration = Mathf.Max(0f, _unselectedExitDuration);
            _unselectedExitStagger = Mathf.Max(0f, _unselectedExitStagger);
            _unselectedExitScaleMultiplier = Mathf.Max(0f, _unselectedExitScaleMultiplier);
        }
    }

}
