namespace Game.Presentation.UI.Rewards
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/UI/Reward Popup Action Presentation Config")]
    public sealed class RewardPopupActionPresentationConfig : ScriptableObject
    {
        [Header("Text")]
        [SerializeField] private string _attemptsFormat = "attempts left: x{0}";
        [SerializeField] private string _guaranteeFormat = "guarantee: {0}";
        [SerializeField] private string _adGuaranteeFormat = "guarantee: {0}";

        [Header("Layout")]
        [SerializeField] private float _singleActionButtonAnchoredX = 350f;

        [Header("Colors")]
        [SerializeField] private Color32 _numberColor = new(105, 255, 120, 255);
        [SerializeField] private Color32 _commonRarityColor = new(95, 220, 130, 255);
        [SerializeField] private Color32 _rareRarityColor = new(80, 180, 255, 255);
        [SerializeField] private Color32 _legendaryRarityColor = new(255, 155, 70, 255);

        public float SingleActionButtonAnchoredX => _singleActionButtonAnchoredX;

        public RewardPopupActionControls.TextSettings CreateTextSettings()
        {
            return new RewardPopupActionControls.TextSettings(
                _attemptsFormat,
                _guaranteeFormat,
                _adGuaranteeFormat,
                _numberColor,
                _commonRarityColor,
                _rareRarityColor,
                _legendaryRarityColor);
        }
    }

}
