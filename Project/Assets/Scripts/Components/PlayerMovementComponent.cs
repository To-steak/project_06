using System;

namespace Project06.Components
{
    [Serializable]
    public struct PlayerMovementComponent
    {
        // 걷기 속도. 초당 이동 거리(m)
        public float moveSpeed;
        // 달리기 속도. 초당 이동 거리(m)
        public float runSpeed;
        // 목표 속도로 다가가는 초당 속도 변화량(m/s²). 가속과 감속에 모두 쓴다.
        public float acceleration;
    }
}
