using Project06.Presentation.Weapon.Views;
using UnityEngine;

namespace Project06.Presentation.Player.Views
{
    public class PlayerView : MonoBehaviour
    {
        // 플레이어 모델의 Animator
        public Animator animator;
        // 플레이어가 들고 있는 총의 View
        public WeaponView weaponView;
    }
}
