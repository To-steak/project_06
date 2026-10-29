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
        // 정신력이 줄지 않는 XZ 평면의 안전 반지름(m). 쉘터 중심부터 잰다. 플레이어가 이 원 안에 있으면 정신력이 회복된다.
        public float mentalitySafeRadius;
        // 안전 반지름을 넘어선 거리 1m마다 초당 줄어드는 정신력
        public float mentalityLossPerSecondPerMeter;
        // 안전 반지름 안에서 초당 회복되는 정신력
        public float mentalityRecoveryPerSecond;
    }
}
