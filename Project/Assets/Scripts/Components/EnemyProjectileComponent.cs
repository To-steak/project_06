using System;

namespace Project06.Components
{
    [Serializable]
    public struct EnemyProjectileComponent
    {
        // 투사체 속력. 초당 이동 거리(m)
        public float speed;
        // 투사체가 사라질 때까지 날아가는 거리(m)
        public float distance;
        // 투사체가 출발하는 높이. 적 위치(발밑)로부터의 높이(m)
        public float height;
    }
}
