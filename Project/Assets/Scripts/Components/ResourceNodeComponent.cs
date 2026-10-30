using System;

namespace Project06.Components
{
    [Serializable]
    public struct ResourceNodeComponent
    {
        // 자원 노드 종류. 주는 자원과 깎이는 근접 무기 데미지가 정해진다.
        public ResourceNodeKind kind;
        // 생성 시 체력. 0이 되면 다 캔 것이다.
        public float maxHealth;
        // XZ 평면의 피격 판정 반지름(m). 플레이어의 근접 판정 상자가 이 원에 닿으면 맞는다.
        public float hitRadius;
        // 가진 자원을 나눠 주는 몫의 수. 최대 체력을 이 수로 나눈 만큼 깎을 때마다 한 몫을 준다. 1보다 작으면 1로 본다.
        public int shareCount;
        // 한 몫마다 주는 자원 수. 다 캐면 이 값의 shareCount배를 준다. 쉘터에서 멀수록 크게 잡는다.
        public int resourcePerShare;
        // 다 캔 뒤 다시 생기기까지 걸리는 날 수. 다 캔 날로부터 이만큼 지난 날의 아침에 그 자리에 체력이 가득 찬 채 다시 생긴다. 0 이하면 다시 생기지 않는다.
        public int regrowDays;
    }
}
