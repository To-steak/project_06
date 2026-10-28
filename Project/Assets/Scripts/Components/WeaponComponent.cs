using System;

namespace Project06.Components
{
    [Serializable]
    public struct WeaponComponent
    {
        // 연사 간격(틱). 발사 입력을 누르고 있으면 이 간격마다 한 발씩 쏜다. 1 이상이어야 한다.
        public int fireIntervalTicks;
        // 투사체 속도. 초당 이동 거리(m)
        public float projectileSpeed;
        // 투사체 사거리(m). 이만큼 날아가면 사라진다.
        public float projectileRange;
        // 투사체 한 발의 데미지
        public float damage;
    }
}
