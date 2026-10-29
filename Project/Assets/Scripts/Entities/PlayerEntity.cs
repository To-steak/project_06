using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct PlayerEntity
    {
        // 살아 있는지 여부. false인 슬롯은 빈 슬롯 목록에 들어 있다.
        public bool isAlive;
        // 이 슬롯이 재사용된 횟수
        public int generation;
        // 직전 틱의 위치. View가 보간할 때 쓴다.
        public Vector3 previousPosition;
        // 현재 틱의 위치
        public Vector3 currentPosition;
        // XZ 평면의 이동 방향. 길이는 0~1이다.
        public Vector3 moveDirection;
        // XZ 평면에서 바라보는 방향. 길이는 1이다.
        public Vector3 facingDirection;
        // 달리고 있는지 여부. 달리기 입력을 누르고 이동 입력이 있을 때 true이며, 재장전 중이면 false다.
        public bool isRunning;
        // XZ 평면의 현재 속도(m/s)
        public Vector3 velocity;
        // 플레이어 수치
        public PlayerComponent player;
        // 남은 체력
        public float health;
        // 남은 정신력. 0이면 체력이 줄어든다.
        public float mentality;
        // 사망 중인지 여부. 쓰러진 순간부터 부활해 다 일어날 때까지 true다. 이 동안 슬롯은 살아 있지만 입력을 받지 않고, 적의 추격·공격·밀어내기 대상이 아니며 맞지 않는다.
        public bool isDead;
        // 사망 중 부활해 일어나는 중인지 여부. 쓰러져 있는 동안은 false다.
        public bool isGettingUp;
        // 쓰러져 있는 동안 부활까지 남은 틱
        public int respawnRemainingTicks;
        // 일어나는 동안 다시 조작할 수 있을 때까지 남은 틱
        public int getUpRemainingTicks;
        // 부활할 월드 위치. 처음 생성된 위치다.
        public Vector3 spawnPosition;
        // 부활할 때 바라볼 XZ 평면의 방향. 처음 생성될 때의 방향이며 길이는 1이다.
        public Vector3 spawnFacingDirection;
        // 이동 수치
        public PlayerMovementComponent movement;
        // 조준하고 있는지 여부. 조준 입력을 누르고 있어도 달리거나 재장전 중이면 false다.
        public bool isAiming;
        // 마지막으로 구한 조준점의 월드 위치. 마우스가 가리키는 지점을 총구 높이의 수평면에 투영한 위치다.
        public Vector3 aimPosition;
        // 발사 입력을 누르고 있는지 여부. 재장전 중이면 false다.
        public bool isFiring;
        // 재장전 중인지 여부
        public bool isReloading;
        // 재장전이 끝날 때까지 남은 틱
        public int reloadRemainingTicks;
        // 장착한 무기의 DefinitionCatalog.weapons 인덱스. 음수면 무기가 없다.
        public int weaponIndex;
        // 장착한 무기의 수치
        public WeaponComponent weapon;
        // 투사체가 출발할 총구의 월드 위치
        public Vector3 muzzlePosition;
        // 다음 발사까지 남은 틱
        public int fireCooldownTicks;
        // 장착한 무기가 쓰는 구경의 DefinitionCatalog.calibers 인덱스. 음수면 구경이 없어 재장전할 수 없다.
        public int caliberIndex;
        // 장착한 무기의 탄창에 남은 탄 수
        public int magazineAmmo;
        // 구경별 예비 탄약 수. DefinitionCatalog.calibers와 같은 순서다.
        public int[] reserveAmmo;
    }
}
