namespace Project06.Components
{
    public enum EnemyMeleeHitShape
    {
        // 적 앞쪽 offset 지점을 중심으로 한 XZ 원
        Circle,
        // 적 앞쪽 offset 지점에서 바라보는 방향으로 length만큼 뻗은 XZ 캡슐
        Capsule,
    }
}
