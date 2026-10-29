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
        // 배회하거나 쉘터로 향할 때의 속도. 초당 이동 거리(m)
        public float walkSpeed;
        // 플레이어 추격 속도. 초당 이동 거리(m)
        public float runSpeed;
        // 목표 속도로 다가가는 초당 속도 변화량(m/s²). 가속과 감속에 모두 쓴다.
        public float acceleration;
        // XZ 평면의 감지 반지름(m). 플레이어를 쫓고 있지 않을 때 플레이어가 이 안에 들어오면 그 플레이어를 쫓기 시작한다.
        public float detectionRadius;
        // XZ 평면의 추격 해제 반지름(m). 쫓던 플레이어가 이 밖으로 나가면 Idle로 돌아가고, 쉘터가 있으면 놓친 뒤 대기 시간이 지나 쉘터를 쫓는다. 감지 반지름보다 커야 경계에서 상태가 깜빡이지 않는다.
        public float loseRadius;
        // 쫓던 플레이어를 놓친 뒤 쉘터로 향하기까지 제자리에 서 있는 시간(틱). 이 동안 플레이어가 감지 반지름에 들어오면 다시 쫓는다.
        public int loseWaitTicks;
        // XZ 평면의 정지 거리(m). 추격 중 목표가 이 안에 있으면 멈추고 목표를 바라본다. 쉘터는 피격 원의 경계부터 잰다.
        public float stopDistance;
        // 추격 중 NavMesh 경로를 다시 계산하는 간격(틱). 1보다 작으면 매 틱 계산한다.
        public int pathUpdateIntervalTicks;
        // XZ 평면의 배회 반지름(m). 생성 위치를 중심으로 이 안에서 배회 목적지를 고른다.
        public float wanderRadius;
        // 배회 목적지에 도착한 뒤 다시 걷기까지 서 있는 최소 시간(틱)
        public int wanderWaitMinTicks;
        // 배회 목적지에 도착한 뒤 다시 걷기까지 서 있는 최대 시간(틱)
        public int wanderWaitMaxTicks;
        // 체력이 0 이하가 된 뒤 사망 애니메이션을 보여 주고 사라지기까지 걸리는 시간(틱)
        public int deathDurationTicks;
        // 투사체에 맞고 살아남았을 때 경직에 걸릴 확률(0~1)
        public float hitStunChance;
        // 경직 시간(틱). 피격 애니메이션 길이에 맞춘다.
        public int hitStunTicks;
        // 공격 목록. 여러 공격이 가능하면 앞에 있는 공격을 고른다. 비어 있으면 공격하지 않는다.
        public EnemyAttackComponent[] attacks;
    }
}
