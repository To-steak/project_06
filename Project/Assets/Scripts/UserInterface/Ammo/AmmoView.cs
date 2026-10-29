using TMPro;

namespace Project06.UserInterface.Ammo.Views
{
    // 탄창 탄약과 예비 탄약을 "탄창 / 예비" 글자로 보여 주는 View
    public class AmmoView : UserInterfaceView
    {
        // "12 / 30" 형식의 탄약 글자
        public TMP_Text ammoText;

        /// <summary>
        /// 탄약 글자를 바꾼다.
        /// </summary>
        /// <param name="magazineAmmo">탄창에 남은 탄약</param>
        /// <param name="reserveAmmo">지금 구경의 예비 탄약</param>
        public void SetAmmo(int magazineAmmo, int reserveAmmo)
        {
            ammoText.text = $"{magazineAmmo} / {reserveAmmo}";
        }
    }
}
