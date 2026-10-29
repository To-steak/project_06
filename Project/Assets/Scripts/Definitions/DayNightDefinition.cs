using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "DayNightDefinition", menuName = "Project06/Definitions/Day Night")]
    public class DayNightDefinition : ScriptableObject
    {
        // 판 시작 시 낮밤 상태에 복사할 낮밤 수치. 기본값은 고정 틱 0.02초 기준 낮·밤 각 2분 30초, 5일째 밤이 마지막이다.
        public DayNightComponent dayNight = new DayNightComponent
        {
            dayTicks = 7500,
            nightTicks = 7500,
            finalDay = 5,
        };
    }
}
