using Project06.Components;

namespace Project06.Entities
{
    public struct DayNightEntity
    {
        // 살아 있는지 여부. false인 슬롯은 빈 슬롯 목록에 들어 있다.
        public bool isAlive;
        // 이 슬롯이 재사용된 횟수
        public int generation;
        // 낮밤 길이
        public DayNightComponent dayNight;
        // 지금 밤인지 여부
        public bool isNight;
        // 지금 시간대가 끝날 때까지 남은 틱
        public int remainingTicks;
        // 지금이 몇 번째 날인지. 첫 낮이 1이며, 밤이 끝나고 낮이 될 때 1 늘어난다.
        public int dayCount;
    }
}
