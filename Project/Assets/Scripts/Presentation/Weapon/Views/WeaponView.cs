using UnityEngine;

namespace Project06.Presentation.Weapon.Views
{
    public class WeaponView : MonoBehaviour
    {
        // 손이 쥐는 지점. 이 Transform이 손 소켓과 위치·회전이 겹치도록 무기를 붙인다. 없으면 프리팹 루트를 소켓에 맞춘다.
        public Transform grip;
    }
}
