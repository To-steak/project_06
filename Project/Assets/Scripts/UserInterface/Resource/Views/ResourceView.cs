using TMPro;

namespace Project06.UserInterface.Resource.Views
{
    // 플레이어가 가진 나무와 돌의 수를 글자로 보여 주는 View
    public class ResourceView : UserInterfaceView
    {
        // 가진 나무 수 글자
        public TMP_Text woodText;
        // 가진 돌 수 글자
        public TMP_Text stoneText;

        /// <summary>
        /// 나무와 돌 글자를 바꾼다.
        /// </summary>
        /// <param name="wood">가진 나무 수</param>
        /// <param name="stone">가진 돌 수</param>
        public void SetResources(int wood, int stone)
        {
            woodText.text = $"나무 {wood}";
            stoneText.text = $"돌 {stone}";
        }
    }
}
