using System;

namespace Project06.Components
{
    [Serializable]
    public struct ShelterComponent
    {
        // 생성 시 체력
        public float maxHealth;
        // XZ 평면의 피격 판정 반지름(m). 적의 근접 판정이나 적 투사체가 이 원에 닿으면 맞는다. 적의 사거리와 정지 거리는 이 원의 경계부터 잰다.
        public float hitRadius;
    }
}
