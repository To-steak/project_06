using Project06.Components;
using UnityEngine;

namespace Project06.Requests
{
    public struct EnemySpawnerSpawnRequest
    {
        // 스포너가 생성될 월드 위치
        public Vector3 position;
        // XZ 평면에서 바라보는 방향. 길이는 1이다.
        public Vector3 facingDirection;
        // 생성 간격·수·범위
        public EnemySpawnerComponent spawner;
        // 낼 적의 종류 인덱스. 판 시작 시 Runner가 모은 적 Definition 목록의 인덱스다.
        public int enemyTypeIndex;
        // 낼 적의 수치
        public EnemyComponent enemy;
    }
}
