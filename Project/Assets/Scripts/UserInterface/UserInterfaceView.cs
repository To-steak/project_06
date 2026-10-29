using UnityEngine;

namespace Project06.UserInterface
{
    // 모든 UI View의 기반. Root 오브젝트에 붙고, 그 아래 Content/Header/Body/Footer를 인스펙터에서 연결해 둔다.
    // 계층은 인스펙터의 생성 버튼(UserInterfaceViewEditor)으로 만든다.
    public abstract class UserInterfaceView : MonoBehaviour
    {
        // View가 켜고 끄는 오브젝트. Vertical Layout Group과 CanvasGroup을 가진다.
        public RectTransform content;
        // Content의 맨 위 영역. Layout Element로 높이를 정한다.
        public RectTransform header;
        // Content의 가운데 영역. Layout Element로 높이를 정한다.
        public RectTransform body;
        // Content의 맨 아래 영역. Layout Element로 높이를 정한다.
        public RectTransform footer;

        /// <summary>
        /// Content를 켜거나 끈다.
        /// </summary>
        /// <param name="isVisible">보이게 하려면 true</param>
        public void SetVisible(bool isVisible)
        {
            content.gameObject.SetActive(isVisible);
        }
    }
}
