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
        // 월드 위치
        public Vector3 position;
        // XZ 평면에서 바라보는 방향. 길이는 1이다.
        public Vector3 facingDirection;
        // 적 수치
        public EnemyComponent enemy;
        // 남은 체력
        public float health;
    }
}
