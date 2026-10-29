using System;

namespace Project06.Components
{
    [Serializable]
    public struct DayNightComponent
    {
        // 낮의 길이(틱). 낮에는 적 스포너가 적을 내지 않는다. 1보다 작으면 1로 본다.
        public int dayTicks;
        // 밤의 길이(틱). 밤에만 적 스포너가 적을 낸다. 1보다 작으면 1로 본다.
        public int nightTicks;
        // 마지막 날. 이 날의 밤은 끝나지 않고 보스 스포너가 보스를 낸다. 보스를 처치하면 승리한다. 1보다 작으면 마지막 날 없이 낮밤이 계속 돈다.
        public int finalDay;
    }
}
