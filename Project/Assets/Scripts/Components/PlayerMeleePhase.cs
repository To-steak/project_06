namespace Project06.Components
{
    public enum PlayerMeleePhase
    {
        // 근접 공격 중이 아니다.
        None,
        // 선딜레이. 타격 전이다.
        Windup,
        // 타격 구간. 판정이 살아 있다.
        Active,
        // 후딜레이. 끝나면 다시 움직일 수 있다.
        Recovery,
    }
}
