using System;

namespace Project06.Components
{
    [Serializable]
    public struct WeaponComponent
    {
        // 무기 종류. 원거리는 아래 연사·투사체·탄창 수치를, 근접은 melee 수치를 쓴다.
        public WeaponKind kind;
        // 연사 간격(틱). 발사 입력을 누르고 있으면 이 간격마다 한 발씩 쏜다. 1 이상이어야 한다.
        public int fireIntervalTicks;
        // 투사체 속도. 초당 이동 거리(m)
        public float projectileSpeed;
        // 투사체 사거리(m). 이만큼 날아가면 사라진다.
        public float projectileRange;
        // 투사체 한 발의 데미지
        public float damage;
        // 탄창 용량
        public int magazineCapacity;
        // 재장전에 걸리는 틱. 이 시간이 지나야 탄창이 채워진다.
        public int reloadTicks;
        // 근접 전용 수치. 원거리 무기는 쓰지 않는다.
        public MeleeWeaponComponent melee;
    }
}
