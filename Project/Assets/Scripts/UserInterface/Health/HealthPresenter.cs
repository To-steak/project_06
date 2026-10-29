using Project06.UserInterface.Health.Views;
using UnityEngine;

namespace Project06.UserInterface.Health.Presenters
{
    // Runner가 매 프레임 넘기는 플레이어 체력을 HealthView에 보여 준다. 표시할 값이 바뀔 때만 View를 부른다.
    public class HealthPresenter : MonoBehaviour
    {
        [SerializeField] private HealthView _view;

        private bool _hasPresented;
        private int _presentedHealth;
        private int _presentedMaxHealth;

        /// <summary>
        /// 체력을 정수로 올림해 보여 준다. 직전에 보여 준 값과 같으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="health">현재 체력</param>
        /// <param name="maxHealth">최대 체력</param>
        public void Present(float health, float maxHealth)
        {
            // 0.4처럼 남은 체력이 0으로 보이지 않도록 올림한다.
            int displayedHealth = Mathf.Max(0, Mathf.CeilToInt(health));
            int displayedMaxHealth = Mathf.CeilToInt(maxHealth);
            if (_hasPresented && displayedHealth == _presentedHealth && displayedMaxHealth == _presentedMaxHealth)
            {
                return;
            }

            _hasPresented = true;
            _presentedHealth = displayedHealth;
            _presentedMaxHealth = displayedMaxHealth;
            _view.SetHealth(displayedHealth, displayedMaxHealth);
        }
    }
}
