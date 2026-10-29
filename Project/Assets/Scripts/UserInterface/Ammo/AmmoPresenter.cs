using Project06.UserInterface.Ammo.Views;
using UnityEngine;

namespace Project06.UserInterface.Ammo.Presenters
{
    // Runner가 매 프레임 넘기는 탄약을 AmmoView에 보여 준다. 표시할 값이 바뀔 때만 View를 부른다.
    public class AmmoPresenter : MonoBehaviour
    {
        [SerializeField] private AmmoView _view;

        private bool _hasPresented;
        private int _presentedMagazineAmmo;
        private int _presentedReserveAmmo;

        /// <summary>
        /// 탄약을 보여 준다. 직전에 보여 준 값과 같으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="magazineAmmo">탄창에 남은 탄약</param>
        /// <param name="reserveAmmo">지금 구경의 예비 탄약</param>
        public void Present(int magazineAmmo, int reserveAmmo)
        {
            if (_hasPresented && magazineAmmo == _presentedMagazineAmmo && reserveAmmo == _presentedReserveAmmo)
            {
                return;
            }

            _hasPresented = true;
            _presentedMagazineAmmo = magazineAmmo;
            _presentedReserveAmmo = reserveAmmo;
            _view.SetAmmo(magazineAmmo, reserveAmmo);
        }
    }
}
