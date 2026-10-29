using TMPro;
using UnityEngine.Serialization;

namespace Project06.UserInterface.Mentality.Views
{
    // 플레이어 정신력을 "현재 / 최대" 글자로 보여 주는 View
    public class MentalityView : UserInterfaceView
    {
        // "85 / 100" 형식의 정신력 글자
        [FormerlySerializedAs("sanityText")]
        public TMP_Text mentalityText;

        /// <summary>
        /// 정신력 글자를 바꾼다.
        /// </summary>
        /// <param name="mentality">현재 정신력</param>
        /// <param name="maxMentality">최대 정신력</param>
        public void SetMentality(int mentality, int maxMentality)
        {
            mentalityText.text = $"{mentality} / {maxMentality}";
        }
    }
}
