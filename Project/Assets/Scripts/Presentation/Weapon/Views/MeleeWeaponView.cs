using UnityEngine;

namespace Project06.Presentation.Weapon.Views
{
    // 근접 무기 프리팹 루트에 WeaponView와 함께 붙인다. 휘두르는 소리를 가진다. 나무·바위 타격음은 ResourceNodeView가 가진다.
    public class MeleeWeaponView : MonoBehaviour
    {
        // 휘두르는 소리를 재생할 AudioSource. 없으면 소리를 내지 않는다.
        public AudioSource audioSource;
        // 나무·바위를 맞히지 않은 공격에서 하나를 무작위로 골라 재생할 휘두르는 소리. 허공이면 타격 구간이 끝날 때, 적·쉘터를 맞히면 맞힌 순간에 난다. 비어 있으면 재생하지 않는다.
        public AudioClip[] swingClips;
    }
}
