using TMPro;

namespace Project06.UserInterface.DayNight.Views
{
    // 날짜, 낮밤, 다음 시간대까지 남은 시간을 "3일째 밤 · 02:15" 글자로 보여 주는 View
    public class DayNightView : UserInterfaceView
    {
        // 날짜와 남은 시간 글자
        public TMP_Text dayNightText;

        /// <summary>
        /// 날짜와 남은 시간 글자를 바꾼다. 마지막 밤이면 남은 시간 대신 "마지막 밤"이라고 쓴다.
        /// </summary>
        /// <param name="dayCount">며칠째인지</param>
        /// <param name="isNight">밤이면 true</param>
        /// <param name="remainingSeconds">다음 시간대까지 남은 시간(초)</param>
        /// <param name="isFinalNight">마지막 밤이면 true</param>
        public void SetDayNight(int dayCount, bool isNight, int remainingSeconds, bool isFinalNight)
        {
            string timeOfDay = isNight ? "밤" : "낮";
            dayNightText.text = isFinalNight
                ? $"{dayCount}일째 {timeOfDay} · 마지막 밤"
                : $"{dayCount}일째 {timeOfDay} · {remainingSeconds / 60:00}:{remainingSeconds % 60:00}";
        }
    }
}
