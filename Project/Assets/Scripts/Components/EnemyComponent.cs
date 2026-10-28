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
        // 추격 속도. 초당 이동 거리(m)
        public float moveSpeed;
        // 목표 속도로 다가가는 초당 속도 변화량(m/s²). 가속과 감속에 모두 쓴다.
        public float acceleration;
        // XZ 평면의 감지 반지름(m). Idle일 때 플레이어가 이 안에 들어오면 추격을 시작한다.
        public float detectionRadius;
        // XZ 평면의 추격 해제 반지름(m). 추격 중 플레이어가 이 밖으로 나가면 Idle로 돌아간다. 감지 반지름보다 커야 경계에서 상태가 깜빡이지 않는다.
        public float loseRadius;
        // XZ 평면의 정지 거리(m). 추격 중 플레이어가 이 안에 있으면 멈추고 플레이어를 바라본다.
        public float stopDistance;
        // 추격 중 NavMesh 경로를 다시 계산하는 간격(틱). 1보다 작으면 매 틱 계산한다.
        public int pathUpdateIntervalTicks;
    }
}
