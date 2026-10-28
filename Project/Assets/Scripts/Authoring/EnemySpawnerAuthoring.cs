using Project06.Components;
using Project06.Definitions;
using UnityEngine;

namespace Project06.Authoring
{
    // 적 스포너의 위치와 바라보는 방향을 씬에 표시한다. 낸 적은 이 방향을 바라본다.
    // 새로 붙이면 시작하자마자 적 한 마리를 내고 끝나는 스포너가 된다.
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        // 이 스포너가 낼 적의 종류
        public EnemyDefinition definition;
        // 생성 간격·수·범위
        public EnemySpawnerComponent spawner = new EnemySpawnerComponent
        {
            firstSpawnDelayTicks = 0,
            intervalTicks = 50,
            maxAliveCount = 1,
            totalSpawnCount = 1,
            spawnRadius = 0f,
        };
    }
}
