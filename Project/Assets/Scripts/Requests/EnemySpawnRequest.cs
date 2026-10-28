using Project06.Components;
using Project06.Entities;
using UnityEngine;

namespace Project06.Requests
{
    public struct EnemySpawnRequest
    {
        // 적 종류 인덱스. 판 시작 시 Runner가 모은 적 Definition 목록의 인덱스다.
        public int enemyTypeIndex;
        // 이 적을 낸 스포너
        public EntityIdentifier spawner;
        // 적이 생성될 월드 위치
        public Vector3 position;
        // XZ 평면에서 바라보는 방향. 길이는 1이다.
        public Vector3 facingDirection;
        // 적 수치
        public EnemyComponent enemy;
    }
}
