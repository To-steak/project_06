using System;

namespace Project06.Components
{
    [Serializable]
    public struct EnemyAttackComponent
    {
        // 공격 종류. 근접은 타격 시점에 사거리 안의 플레이어에게 데미지를 주고, 원거리는 플레이어 쪽으로 투사체를 쏜다.
        public EnemyAttackKind kind;
        // XZ 평면의 사거리(m). 추격 중 플레이어가 이 안에 있으면 공격을 시작한다. 근접은 타격 시점에도 이 안에 있어야 맞는다.
        public float range;
        // 한 번의 데미지. 원거리는 투사체 한 발의 데미지다.
        public float damage;
        // 공격을 시작한 뒤 타격하거나 투사체를 쏘기까지 걸리는 시간(틱)
        public int windupTicks;
        // 타격하거나 투사체를 쏜 뒤 제자리에 서 있다가 추격으로 돌아가기까지 걸리는 시간(틱). 공격 애니메이션의 남은 길이에 맞춘다.
        public int recoveryTicks;
        // 이 공격을 시작한 뒤 다시 시작할 수 있을 때까지 걸리는 시간(틱). 선딜레이 시간도 포함한다.
        public int cooldownTicks;
        // 원거리 전용 투사체 수치. 근접 공격은 쓰지 않는다.
        public EnemyProjectileComponent projectile;
    }
}
