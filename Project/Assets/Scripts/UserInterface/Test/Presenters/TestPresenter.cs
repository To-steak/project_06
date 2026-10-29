using Project06.UserInterface.Test.Views;
using UnityEngine;

namespace Project06.UserInterface.Test.Presenters
{
    // TestView의 체력 바 연출을 시험하는 Presenter. Runner 대신 시험용 체력을 직접 들고 있다.
    // 플레이 중 컴포넌트의 ⋮ 메뉴에서 Test Damage / Test Heal로 체력을 바꿔 View에 넘긴다.
    public class TestPresenter : MonoBehaviour
    {
        [SerializeField] private TestView _view;
        [SerializeField] private float _testMaxHealth = 100f;
        [SerializeField] private float _testDamage = 15f;
        [SerializeField] private float _testHeal = 20f;

        private float _testHealth;

        private void Start()
        {
            _testHealth = _testMaxHealth;
            _view.InitializeHealth(_testHealth, _testMaxHealth);
        }

        [ContextMenu("Test Damage")]
        private void TestDamage()
        {
            _testHealth = Mathf.Max(0f, _testHealth - _testDamage);
            _view.SetHealth(_testHealth, _testMaxHealth);
        }

        [ContextMenu("Test Heal")]
        private void TestHeal()
        {
            _testHealth = Mathf.Min(_testMaxHealth, _testHealth + _testHeal);
            _view.SetHealth(_testHealth, _testMaxHealth);
        }
    }
}
