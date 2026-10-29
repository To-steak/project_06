using System;

namespace Project06.Components
{
    [Serializable]
    public struct PlayerComponent
    {
        // 생성 시 체력
        public float maxHealth;
        // XZ 평면의 피격 판정 반지름(m). 적 투사체가 이 원을 지나면 맞는다.
        public float hitRadius;
        // 체력이 0 이하가 된 뒤 쓰러진 채 있다가 시작 위치에서 일어나기 시작할 때까지 걸리는 시간(틱). 1보다 작으면 1로 본다.
        public int respawnDelayTicks;
        // 부활한 뒤 일어나서 다시 조작할 수 있을 때까지 걸리는 시간(틱). 일어나는 애니메이션(DownToUp) 길이에 맞춘다. 1보다 작으면 1로 본다.
        public int getUpTicks;
        // 생성 시 정신력. 부활할 때도 이 값으로 채운다.
        public float maxMentality;
        // 정신력이 0인 동안 초당 줄어드는 체력
        public float zeroMentalityHealthLossPerSecond;
    }
}
