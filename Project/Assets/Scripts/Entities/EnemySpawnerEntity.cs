using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct EnemySpawnerEntity
    {
        // 살아 있는지 여부. false인 슬롯은 빈 슬롯 목록에 들어 있다.
        public bool isAlive;
        // 이 슬롯이 재사용된 횟수
        public int generation;
        // 스포너의 월드 위치
        public Vector3 position;
        // XZ 평면에서 바라보는 방향. 길이는 1이며, 낸 적이 이 방향을 바라본다.
        public Vector3 facingDirection;
        // 생성 간격·수·범위
        public EnemySpawnerComponent spawner;
        // 낼 적의 종류 인덱스
        public int enemyTypeIndex;
        // 낼 적의 수치
        public EnemyComponent enemy;
        // 다음 적을 낼 때까지 남은 틱
        public int spawnRemainingTicks;
        // 이번 밤에 낸 적의 수. 밤이 시작될 때 0으로 되돌린다.
        public int spawnedCount;
    }
}
