using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct EnemyEntity
    {
        // 살아 있는지 여부. false인 슬롯은 빈 슬롯 목록에 들어 있다.
        public bool isAlive;
        // 이 슬롯이 재사용된 횟수
        public int generation;
        // 적 종류 인덱스. 판 시작 시 Runner가 모은 적 Definition 목록의 인덱스이며, View가 프리팹을 고를 때 쓴다.
        public int enemyTypeIndex;
        // 이 적을 낸 스포너
        public EntityIdentifier spawner;
        // 직전 틱의 위치. View가 보간할 때 쓴다.
        public Vector3 previousPosition;
        // 현재 틱의 위치
        public Vector3 currentPosition;
        // 생성된 위치. 배회 목적지를 고르는 중심이다.
        public Vector3 spawnPosition;
        // XZ 평면에서 바라보는 방향. 길이는 1이다.
        public Vector3 facingDirection;
        // 현재 속도(m/s). 크기만 이동 거리에 쓰고, 실제 이동은 경로를 따른다.
        public Vector3 velocity;
        // 적 수치
        public EnemyComponent enemy;
        // 남은 체력
        public float health;
        // AI 상태
        public EnemyState state;
        // Idle에서 배회를 다시 시작할 때까지 남은 틱
        public int idleRemainingTicks;
        // Dead에서 소멸할 때까지 남은 틱
        public int deathRemainingTicks;
        // HitStun에서 추격으로 돌아갈 때까지 남은 틱
        public int hitStunRemainingTicks;
        // Attack에서 하고 있는 공격의 enemy.attacks 인덱스
        public int attackIndex;
        // Attack에서 타격 전이면 타격할 때까지, 타격 후면 추격으로 돌아갈 때까지 남은 틱
        public int attackRemainingTicks;
        // Attack에서 이번 공격의 타격을 이미 했는지 여부. true면 후딜레이 중이다.
        public bool hasAttackHit;
        // 공격별로 다시 시작할 수 있을 때까지 남은 틱. enemy.attacks와 같은 순서이며, 길이는 공격 수 이상이다. 슬롯이 재사용될 때 길이가 모자랄 때만 SpawnSystem이 새로 만든다.
        public int[] attackCooldownRemainingTicks;
        // 따라갈 NavMesh 경로의 코너. 슬롯마다 EnemyStorage가 미리 만들어 두고 재사용한다.
        public Vector3[] pathCorners;
        // pathCorners 중 유효한 코너 수
        public int pathCornerCount;
        // 다음에 향할 코너의 인덱스. pathCornerCount 이상이면 경로를 다 걸었다.
        public int pathCornerIndex;
        // 경로를 다시 계산할 때까지 남은 틱
        public int pathUpdateRemainingTicks;
    }
}
