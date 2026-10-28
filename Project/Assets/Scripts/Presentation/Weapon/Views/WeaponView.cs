using UnityEngine;

namespace Project06.Presentation.Weapon.Views
{
    public class WeaponView : MonoBehaviour
    {
        // 손이 쥐는 지점. 이 Transform이 손 소켓과 위치·회전이 겹치도록 총을 붙인다. 없으면 프리팹 루트를 소켓에 맞춘다.
        public Transform grip;
        // 총 부품(슬라이드 등)을 움직이는 Animator. 없으면 총 애니메이션을 재생하지 않는다.
        public Animator animator;
        // 총구 위치. 투사체가 여기서 출발하며, 이 높이의 수평면에서 커서를 겨냥한다.
        public Transform muzzle;
        // 발사할 때 재생할 총구 섬광. 없어도 된다.
        public ParticleSystem muzzleFlash;
        // 발사음을 재생할 AudioSource. 없으면 발사음을 내지 않는다.
        public AudioSource fireAudioSource;
        // 발사할 때마다 하나를 무작위로 골라 재생할 발사음. 비어 있으면 fireAudioSource의 clip을 재생한다.
        public AudioClip[] fireClips;
        // 장전음을 재생할 AudioSource. fireAudioSource와 같은 컴포넌트를 연결해도 된다. 없으면 장전음을 내지 않는다.
        public AudioSource reloadAudioSource;
        // 장전 단계별 소리. 순서대로 reloadClipTicks의 같은 칸에 적힌 틱에 재생한다.
        public AudioClip[] reloadClips;
        // 장전을 시작한 틱부터 각 장전음을 재생할 때까지의 틱. reloadClips와 같은 순서이며, 무기의 reloadTicks보다 작아야 재생된다.
        public int[] reloadClipTicks;
    }
}
