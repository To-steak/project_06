using UnityEngine;

namespace Project06.Presentation.Weapon.Views
{
    public class WeaponView : MonoBehaviour
    {
        // 손이 쥐는 지점. 이 Transform이 손 소켓과 위치·회전이 겹치도록 총을 붙인다. 없으면 프리팹 루트를 소켓에 맞춘다.
        public Transform grip;
        // 오른손 손가락 본이 맞출 회전. PlayerView.rightHandFingers와 같은 순서이며, 비어 있는 칸의 손가락은 애니메이션을 그대로 따른다.
        public Transform[] rightHandFingerTargets;
        // 총구 위치. 투사체가 여기서 출발하며, 이 높이의 수평면에서 커서를 겨냥한다.
        public Transform muzzle;
        // 발사할 때 재생할 총구 섬광. 없어도 된다.
        public ParticleSystem muzzleFlash;
        // 발사할 때 재생할 발사음. 없어도 된다.
        public AudioSource fireAudioSource;
    }
}
