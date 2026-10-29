using TMPro;

namespace Project06.UserInterface.Health.Views
{
    // 플레이어 체력을 "현재 / 최대" 글자로 보여 주는 View
    public class HealthView : UserInterfaceView
    {
        // "85 / 100" 형식의 체력 글자
        public TMP_Text healthText;

        /// <summary>
        /// 체력 글자를 바꾼다.
        /// </summary>
        /// <param name="health">현재 체력</param>
        /// <param name="maxHealth">최대 체력</param>
        public void SetHealth(int health, int maxHealth)
        {
            healthText.text = $"{health} / {maxHealth}";
        }
    }
}
