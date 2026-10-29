using Project06.UserInterface.Mentality.Views;
using UnityEngine;

namespace Project06.UserInterface.Mentality.Presenters
{
    // Runner가 매 프레임 넘기는 플레이어 정신력을 MentalityView에 보여 준다. 표시할 값이 바뀔 때만 View를 부른다.
    public class MentalityPresenter : MonoBehaviour
    {
        [SerializeField] private MentalityView _view;

        private bool _hasPresented;
        private int _presentedMentality;
        private int _presentedMaxMentality;

        /// <summary>
        /// 정신력을 정수로 올림해 보여 준다. 직전에 보여 준 값과 같으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="mentality">현재 정신력</param>
        /// <param name="maxMentality">최대 정신력</param>
        public void Present(float mentality, float maxMentality)
        {
            // 0.4처럼 남은 정신력이 0으로 보이지 않도록 올림한다.
            int displayedMentality = Mathf.Max(0, Mathf.CeilToInt(mentality));
            int displayedMaxMentality = Mathf.CeilToInt(maxMentality);
            if (_hasPresented && displayedMentality == _presentedMentality && displayedMaxMentality == _presentedMaxMentality)
            {
                return;
            }

            _hasPresented = true;
            _presentedMentality = displayedMentality;
            _presentedMaxMentality = displayedMaxMentality;
            _view.SetMentality(displayedMentality, displayedMaxMentality);
        }
    }
}
