using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct BossSpawnerEntity
    {
        // 살아 있는지 여부. false인 슬롯은 빈 슬롯 목록에 들어 있다.
        public bool isAlive;
        // 이 슬롯이 재사용된 횟수
        public int generation;
        // 스포너의 월드 위치. 보스가 정확히 이 위치에 생긴다.
        public Vector3 position;
        // XZ 평면에서 바라보는 방향. 길이는 1이며, 낸 보스가 이 방향을 바라본다.
        public Vector3 facingDirection;
        // 대기 시간
        public BossSpawnerComponent spawner;
        // 낼 보스의 적 종류 인덱스
        public int enemyTypeIndex;
        // 낼 보스의 수치
        public EnemyComponent enemy;
        // 마지막 날의 밤이 된 뒤 보스를 낼 때까지 남은 틱
        public int spawnRemainingTicks;
        // 보스를 이미 냈는지 여부. 한 번 내면 다시 내지 않는다.
        public bool hasSpawned;
    }
}
