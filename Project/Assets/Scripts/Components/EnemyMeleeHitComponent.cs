using System;

namespace Project06.Components
{
    [Serializable]
    public struct EnemyMeleeHitComponent
    {
        // 판정 모양
        public EnemyMeleeHitShape shape;
        // 판정이 시작되는 지점까지 적 위치에서 바라보는 방향으로 떨어진 거리(m). 원은 이 지점이 중심이다.
        public float offset;
        // 판정 반지름(m). 플레이어 피격 반지름과 겹치면 맞는다.
        public float radius;
        // 캡슐 전용. offset 지점에서 바라보는 방향으로 뻗는 길이(m)
        public float length;
        // 선딜레이가 끝난 뒤 판정이 살아 있는 시간(틱). 이 동안 매 틱 검사하고, 한 번 맞히면 이 공격으로는 더 맞히지 않는다. 1보다 작으면 1로 본다.
        public int activeTicks;
    }
}
