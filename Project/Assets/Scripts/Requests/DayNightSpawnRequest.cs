using Project06.Components;

namespace Project06.Requests
{
    public struct DayNightSpawnRequest
    {
        // 낮밤 길이. 생성되면 첫 낮부터 시작한다.
        public DayNightComponent dayNight;
    }
}
