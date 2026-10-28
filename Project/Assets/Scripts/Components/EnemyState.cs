namespace Project06.Components
{
    public enum EnemyState
    {
        // 제자리에 서 있다. 감지 반지름 안에 플레이어가 들어오면 Chase가 된다.
        Idle,
        // 플레이어를 NavMesh 경로로 쫓는다. 플레이어가 해제 반지름 밖으로 나가면 Idle이 된다.
        Chase,
    }
}
