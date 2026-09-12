using Game.Presentation.Validation;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Worm
{
    public sealed class WormSectionHpView : MonoBehaviour
    {
        private const int HpTextBufferSize = 16;

        [SerializeField, RequiredViewReference] private TMP_Text _text;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private string _sortingLayerName = "UI";
        [SerializeField] private int _sortingOrder = 2600;
        [SerializeField, Min(1f)] private int _fullScaleHp = 10000;
        [SerializeField, Min(0f)] private float _minScale = 0.9f;
        [SerializeField, Min(0f)] private float _maxScale = 1f;

        private Transform _target;
        private bool _isVisible = true;
        private readonly char[] _hpTextBuffer = new char[HpTextBufferSize];

        private void Awake()
        {
            if (_text == null)
            {
                Debug.LogError("WormSectionHpView: TMP_Text is not assigned.", this);
                return;
            }

            if (!_text.TryGetComponent(out MeshRenderer meshRenderer))
            {
                Debug.LogError("MeshRenderer not found on TMP_Text", this);
                return;
            }

            meshRenderer.sortingLayerName = _sortingLayerName;
            meshRenderer.sortingOrder = _sortingOrder;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                SetVisible(false);
                return;
            }

            if (!_target.gameObject.activeInHierarchy)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            transform.position = _target.position;
        }

        private void OnValidate()
        {
            if (_text == null)
                Debug.LogError("WormSectionHpView: TMP_Text is not assigned.", this);

            if (string.IsNullOrWhiteSpace(_sortingLayerName))
                Debug.LogError("WormSectionHpView: Sorting layer name is empty.", this);

            _fullScaleHp = Mathf.Max(1, _fullScaleHp);
            _minScale = Mathf.Max(0f, _minScale);
            _maxScale = Mathf.Max(_minScale, _maxScale);
        }

        public void Bind(Transform target, int currentHp)
        {
            _target = target;
            SetVisible(true);
            SetValue(currentHp);
        }

        public void Unbind()
        {
            _target = null;
            SetVisible(false);
        }

        public void SetValue(int current)
        {
            if (WormHpFormatter.TryFormat(current, _hpTextBuffer, out int length))
                _text.SetCharArray(_hpTextBuffer, 0, length);
            else
                _text.text = WormHpFormatter.Format(current);

            float t = Mathf.InverseLerp(0, _fullScaleHp, current);
            float scale = Mathf.Lerp(_maxScale, _minScale, t);

            if (_visualRoot != null)
                _visualRoot.localScale = Vector3.one * scale;
        }

        private void SetVisible(bool visible)
        {
            if (_isVisible == visible)
                return;

            _isVisible = visible;

            if (_visualRoot != null)
                _visualRoot.gameObject.SetActive(visible);
            else if (_text != null)
                _text.enabled = visible;
        }
    }

}
