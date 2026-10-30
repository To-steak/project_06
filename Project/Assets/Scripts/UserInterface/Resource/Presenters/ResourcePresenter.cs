using Project06.UserInterface.Resource.Views;
using UnityEngine;

namespace Project06.UserInterface.Resource.Presenters
{
    // Runner가 매 프레임 넘기는 플레이어 자원을 ResourceView에 보여 준다. 표시할 값이 바뀔 때만 View를 부른다.
    public class ResourcePresenter : MonoBehaviour
    {
        [SerializeField] private ResourceView _view;

        private bool _hasPresented;
        private int _presentedWood;
        private int _presentedStone;

        /// <summary>
        /// 자원을 보여 준다. 직전에 보여 준 값과 같으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="wood">가진 나무 수</param>
        /// <param name="stone">가진 돌 수</param>
        public void Present(int wood, int stone)
        {
            if (_hasPresented && wood == _presentedWood && stone == _presentedStone)
            {
                return;
            }

            _hasPresented = true;
            _presentedWood = wood;
            _presentedStone = stone;
            _view.SetResources(wood, stone);
        }
    }
}
