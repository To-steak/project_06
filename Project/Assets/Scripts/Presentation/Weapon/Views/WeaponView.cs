using UnityEngine;

namespace Project06.Presentation.Weapon.Views
{
    public class WeaponView : MonoBehaviour
    {
        // 총구 위치. 투사체가 여기서 출발하며, 이 높이의 수평면에서 커서를 겨냥한다.
        public Transform muzzle;
        // 발사할 때 재생할 총구 섬광. 없어도 된다.
        public ParticleSystem muzzleFlash;
        // 발사할 때 재생할 발사음. 없어도 된다.
        public AudioSource fireAudioSource;
    }
}
