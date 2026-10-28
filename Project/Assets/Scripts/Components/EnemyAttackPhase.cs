namespace Project06.Components
{
    public enum EnemyAttackPhase
    {
        // 선딜레이. 타격 전이다.
        Windup,
        // 타격 구간. 근접은 판정이 살아 있고, 원거리는 이 구간에 들어오는 틱에 투사체를 쏜다.
        Active,
        // 후딜레이. 끝나면 추격으로 돌아간다.
        Recovery,
    }
}
