using Project06.UserInterface.DayNight.Views;
using UnityEngine;

namespace Project06.UserInterface.DayNight.Presenters
{
    // Runner가 매 프레임 넘기는 낮밤 상태를 DayNightView에 보여 준다. 표시할 값이 바뀔 때만 View를 부르므로 글자는 1초에 한 번 바뀐다.
    public class DayNightPresenter : MonoBehaviour
    {
        [SerializeField] private DayNightView _view;

        private bool _hasPresented;
        private int _presentedDayCount;
        private bool _presentedIsNight;
        private int _presentedRemainingSeconds;
        private bool _presentedIsFinalNight;

        /// <summary>
        /// 날짜와 남은 시간을 보여 준다. 남은 시간은 초 단위로 올림한다. 직전에 보여 준 값과 같으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="dayCount">며칠째인지</param>
        /// <param name="isNight">밤이면 true</param>
        /// <param name="remainingTicks">다음 시간대까지 남은 틱</param>
        /// <param name="isFinalNight">마지막 밤이면 true</param>
        public void Present(int dayCount, bool isNight, int remainingTicks, bool isFinalNight)
        {
            int remainingSeconds = Mathf.Max(0, Mathf.CeilToInt(remainingTicks * Time.fixedDeltaTime));
            if (_hasPresented
                && dayCount == _presentedDayCount
                && isNight == _presentedIsNight
                && remainingSeconds == _presentedRemainingSeconds
                && isFinalNight == _presentedIsFinalNight)
            {
                return;
            }

            _hasPresented = true;
            _presentedDayCount = dayCount;
            _presentedIsNight = isNight;
            _presentedRemainingSeconds = remainingSeconds;
            _presentedIsFinalNight = isFinalNight;
            _view.SetDayNight(dayCount, isNight, remainingSeconds, isFinalNight);
        }
    }
}
