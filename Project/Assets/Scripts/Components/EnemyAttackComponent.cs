using System;

namespace Project06.Components
{
    [Serializable]
    public struct EnemyAttackComponent
    {
        // 공격 종류. 근접은 타격 구간 동안 판정 모양에 닿은 플레이어에게 데미지를 주고, 원거리는 플레이어 쪽으로 투사체를 쏜다.
        public EnemyAttackKind kind;
        // XZ 평면의 사거리(m). 추격 중 플레이어가 이 안에 있으면 공격을 시작한다. 맞는지는 근접은 판정 모양이, 원거리는 투사체가 정한다.
        public float range;
        // 한 번의 데미지. 원거리는 투사체 한 발의 데미지다.
        public float damage;
        // 공격을 시작한 뒤 타격 구간이 시작되거나 투사체를 쏘기까지 걸리는 시간(틱)
        public int windupTicks;
        // 타격 구간이 끝나거나 투사체를 쏜 뒤 제자리에 서 있다가 추격으로 돌아가기까지 걸리는 시간(틱). 공격 애니메이션의 남은 길이에 맞춘다.
        public int recoveryTicks;
        // 이 공격을 시작한 뒤 다시 시작할 수 있을 때까지 걸리는 시간(틱). 선딜레이 시간도 포함한다.
        public int cooldownTicks;
        // 근접 전용 판정 모양과 타격 구간. 원거리 공격은 쓰지 않는다.
        public EnemyMeleeHitComponent meleeHit;
        // 원거리 전용 투사체 수치. 근접 공격은 쓰지 않는다.
        public EnemyProjectileComponent projectile;
    }
}
