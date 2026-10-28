using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct EnemyEntity
    {
        // 살아 있는지 여부. 체력이 0 이하가 되면 false가 되고 다시 살아나지 않는다.
        public bool isAlive;
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
