using Project06.Components;
using UnityEngine;

namespace Project06.Requests
{
    public struct PlayerSpawnRequest
    {
        // 플레이어가 생성될 월드 위치
        public Vector3 position;
        // XZ 평면에서 바라보는 방향. 길이는 1이다.
        public Vector3 facingDirection;
        // 플레이어 수치
        public PlayerComponent player;
        // 이동 수치
        public PlayerMovementComponent movement;
        // 장착할 무기의 DefinitionCatalog.weapons 인덱스. 음수면 무기가 없다.
        public int weaponIndex;
        // 장착할 무기의 수치
        public WeaponComponent weapon;
        // 장착할 무기가 쓰는 구경의 DefinitionCatalog.calibers 인덱스. 음수면 구경이 없어 재장전할 수 없다.
        public int caliberIndex;
        // 장착할 무기의 탄창에 들어 있는 탄 수
        public int magazineAmmo;
        // 구경별 예비 탄약 수. DefinitionCatalog.calibers와 같은 순서다.
        public int[] reserveAmmo;
    }
}
