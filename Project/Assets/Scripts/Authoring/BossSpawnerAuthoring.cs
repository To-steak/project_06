using Project06.Components;
using Project06.Definitions;
using UnityEngine;

namespace Project06.Authoring
{
    // 보스 스포너의 위치와 바라보는 방향을 씬에 표시한다. 마지막 날(DayNightDefinition의 finalDay) 밤에 이 위치에서 이 방향을 바라보는 보스를 낸다.
    // 보스를 처치하면 승리하고, 처치하기 전에 쉘터가 파괴되면 패배한다.
    public class BossSpawnerAuthoring : MonoBehaviour
    {
        // 이 스포너가 낼 보스의 종류
        public EnemyDefinition definition;
        // 대기 시간
        public BossSpawnerComponent spawner = new BossSpawnerComponent
        {
            firstSpawnDelayTicks = 0,
        };
    }
}
