namespace Project06.Components
{
    public enum EnemyState
    {
        // 제자리에 서서 대기 시간이 끝나기를 기다린다. 끝나면 배회 목적지를 골라 Wander가 된다.
        Idle,
        // 생성 위치 주변의 배회 목적지로 걷는다. 도착하면 Idle이 된다.
        Wander,
        // 플레이어를 NavMesh 경로로 쫓는다. 플레이어가 해제 반지름 밖으로 나가면 Idle이 된다.
        Chase,
        // 체력이 0 이하가 되어 사망 애니메이션을 보여 주는 중이다. 움직이지 않고 맞지 않으며, 사망 시간이 끝나면 소멸한다.
        Dead,
    }
}
