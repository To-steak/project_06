using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct PlayerEntity
    {
        // 살아 있는지 여부
        public bool isAlive;
        // 직전 틱의 위치. View가 보간할 때 쓴다.
        public Vector3 previousPosition;
        // 현재 틱의 위치
        public Vector3 currentPosition;
        // XZ 평면의 이동 방향. 길이는 0~1이다.
        public Vector3 moveDirection;
        // XZ 평면에서 바라보는 방향. 길이는 1이다.
        public Vector3 facingDirection;
        // 달리고 있는지 여부. 달리기 입력을 누르고 이동 입력이 있을 때 true이며, 발사 중이면 false다.
        public bool isRunning;
        // XZ 평면의 현재 속도(m/s)
        public Vector3 velocity;
        // 이동 수치
        public PlayerMovementComponent movement;
        // 조준하고 있는지 여부. 조준 입력을 누르고 있어도 달리면 false다.
        public bool isAiming;
        // 조준 입력과 발사 입력을 함께 누르고 있는지 여부
        public bool isFiring;
        // 장착한 무기의 DefinitionCatalog.weapons 인덱스. 음수면 무기가 없다.
        public int weaponIndex;
        // 장착한 무기의 수치
        public WeaponComponent weapon;
        // 투사체가 출발할 총구의 월드 위치
        public Vector3 muzzlePosition;
        // 총구에서 겨냥한 XZ 평면의 방향. 정규화하지 않은 값이며, 길이가 0이면 바라보는 방향으로 쏜다.
        public Vector3 aimDirection;
        // 다음 발사까지 남은 틱
        public int fireCooldownTicks;
    }
}
