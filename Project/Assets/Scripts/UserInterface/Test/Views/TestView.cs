using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Project06.UserInterface.Test.Views
{
    // DOTween 연출을 시험하는 체력 바 View. 체력 값은 TestPresenter가 넘겨준다.
    // 깎이면 앞쪽 바가 바로 줄고 뒤쪽 바가 잠시 뒤 따라 줄며, 바가 흔들리고 앞쪽 바가 번쩍인다. 차면 앞쪽 바가 부드럽게 차오른다.
    public class TestView : UserInterfaceView
    {
        // 체력 바 전체. Body 안에 두며 피격 때 흔든다. 레이아웃 그룹이 배치하는 오브젝트가 아니어야 한다.
        public RectTransform healthBar;
        // 현재 체력을 보여 주는 앞쪽 채움 이미지. Image Type은 Filled(Horizontal)이다.
        public Image healthFillImage;
        // 깎인 만큼 늦게 따라 줄어드는 뒤쪽 채움 이미지. 앞쪽 이미지 뒤에 두며 Image Type은 Filled(Horizontal)이다.
        public Image healthDelayedFillImage;

        // 뒤쪽 바가 줄기 시작하기까지 기다리는 시간(초)
        [SerializeField] private float _delayedFillDelay = 0.4f;
        // 뒤쪽 바가 줄어드는 데 걸리는 시간(초)
        [SerializeField] private float _delayedFillDuration = 0.5f;
        // 회복할 때 앞쪽 바가 차오르는 데 걸리는 시간(초)
        [SerializeField] private float _healFillDuration = 0.3f;
        // 피격 때 바를 흔드는 시간(초)과 세기(픽셀)
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private float _shakeStrength = 8f;
        // 피격 때 앞쪽 바가 번쩍이는 색과 원래 색으로 돌아오는 시간(초)
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private float _flashDuration = 0.2f;

        // 앞쪽 바의 원래 색. 번쩍인 뒤 이 색으로 돌아온다.
        private Color _fillColor;

        /// <summary>
        /// 연출 없이 체력 바를 바로 새 체력으로 맞춘다. 처음 표시할 때 쓴다.
        /// </summary>
        /// <param name="health">현재 체력</param>
        /// <param name="maxHealth">최대 체력. 0 이하면 바를 비운다.</param>
        public void InitializeHealth(float health, float maxHealth)
        {
            float ratio = GetRatio(health, maxHealth);
            healthFillImage.DOKill();
            healthDelayedFillImage.DOKill();
            healthFillImage.color = _fillColor;
            healthFillImage.fillAmount = ratio;
            healthDelayedFillImage.fillAmount = ratio;
        }

        /// <summary>
        /// 체력 바를 새 체력으로 바꾼다. 줄었으면 피격 연출을, 늘었으면 회복 연출을 한다.
        /// </summary>
        /// <param name="health">현재 체력</param>
        /// <param name="maxHealth">최대 체력. 0 이하면 바를 비운다.</param>
        public void SetHealth(float health, float maxHealth)
        {
            float ratio = GetRatio(health, maxHealth);
            float previousRatio = healthFillImage.fillAmount;

            healthFillImage.DOKill();
            healthDelayedFillImage.DOKill();
            healthFillImage.color = _fillColor;

            if (ratio < previousRatio)
            {
                PresentDamage(ratio);
            }
            else
            {
                PresentHeal(previousRatio, ratio);
            }
        }

        private void Awake()
        {
            _fillColor = healthFillImage.color;
        }

        private static float GetRatio(float health, float maxHealth)
        {
            return maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;
        }

        // 앞쪽 바는 바로 줄이고, 뒤쪽 바는 잠시 뒤 따라 줄인다. 바를 흔들고 앞쪽 바를 번쩍인다.
        private void PresentDamage(float ratio)
        {
            healthFillImage.fillAmount = ratio;
            healthDelayedFillImage.DOFillAmount(ratio, _delayedFillDuration)
                .SetDelay(_delayedFillDelay)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject);

            // 흔들던 중이면 제자리로 돌려놓고 새로 흔든다.
            healthBar.DOComplete();
            healthBar.DOShakeAnchorPos(_shakeDuration, _shakeStrength)
                .SetLink(gameObject);

            healthFillImage.color = _flashColor;
            healthFillImage.DOColor(_fillColor, _flashDuration)
                .SetLink(gameObject);
        }

        // 뒤쪽 바는 바로 새 값으로 맞추고, 앞쪽 바는 이전 값에서 부드럽게 차오른다.
        private void PresentHeal(float previousRatio, float ratio)
        {
            healthDelayedFillImage.fillAmount = ratio;
            healthFillImage.fillAmount = previousRatio;
            healthFillImage.DOFillAmount(ratio, _healFillDuration)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject);
        }
    }
}
