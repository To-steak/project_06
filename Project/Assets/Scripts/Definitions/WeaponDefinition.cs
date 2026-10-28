using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Project06/Definitions/Weapon")]
    public class WeaponDefinition : ScriptableObject
    {
        // 장착 시 엔티티에 복사할 무기 수치
        public WeaponComponent weapon;
        // 발사한 투사체를 표시할 프리팹. 투사체의 앞쪽(Z+)이 날아가는 방향을 향한다.
        public GameObject projectilePrefab;
    }
}
