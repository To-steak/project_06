using System;

namespace Project06.Components
{
    [Serializable]
    public struct EnemyComponent
    {
        // 생성 시 체력
        public float maxHealth;
        // XZ 평면의 피격 판정 반지름(m). 투사체가 이 원을 지나면 맞는다.
        public float hitRadius;
    }
}
