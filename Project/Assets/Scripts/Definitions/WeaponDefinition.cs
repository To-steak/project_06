using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Project06/Definitions/Weapon")]
    public class WeaponDefinition : ScriptableObject
    {
        // 장착 시 엔티티에 복사할 무기 수치
        public WeaponComponent weapon;
        // 이 무기가 쓰는 구경. DefinitionCatalog.calibers에 들어 있어야 한다. 같은 구경을 쓰는 무기는 예비 탄약을 공유한다.
        public CaliberDefinition caliber;
        // 플레이어 손에 붙일 총 프리팹. 루트에 WeaponView가 있어야 하며, WeaponView.grip이 손 소켓에 맞춰진다.
        public GameObject weaponPrefab;
        // 발사한 투사체를 표시할 프리팹. 투사체의 앞쪽(Z+)이 날아가는 방향을 향한다.
        public GameObject projectilePrefab;
    }
}
