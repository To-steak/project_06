using System;

namespace Project06.Components
{
    [Serializable]
    public struct MeleeWeaponComponent
    {
        // 공격을 시작한 뒤 타격 구간이 시작되기까지 걸리는 시간(틱)
        public int windupTicks;
        // 선딜레이가 끝난 뒤 판정이 살아 있는 시간(틱). 이 동안 매 틱 검사하고, 한 번 맞히면 이 공격으로는 더 맞히지 않는다. 1보다 작으면 1로 본다.
        public int activeTicks;
        // 타격 구간이 끝난 뒤 제자리에 서 있다가 다시 움직일 수 있기까지 걸리는 시간(틱). 공격 애니메이션의 남은 길이에 맞춘다.
        public int recoveryTicks;
        // 이 공격을 시작한 뒤 다시 시작할 수 있을 때까지 걸리는 시간(틱). 선딜레이 시간도 포함한다.
        public int cooldownTicks;
        // 판정 상자가 시작되는 지점까지 플레이어 위치에서 바라보는 방향으로 떨어진 거리(m)
        public float hitOffset;
        // 판정 상자의 바라보는 방향 길이(m)
        public float hitLength;
        // 판정 상자의 좌우 폭(m)
        public float hitWidth;
        // 적에게 주는 데미지
        public float enemyDamage;
        // 건물(쉘터 등 플레이어의 구조물)을 때렸을 때 회복시키는 체력. 최대 체력을 넘지 않는다.
        public float buildingRepair;
    }
}
