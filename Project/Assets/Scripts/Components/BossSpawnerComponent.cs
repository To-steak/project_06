using System;

namespace Project06.Components
{
    [Serializable]
    public struct BossSpawnerComponent
    {
        // 마지막 날의 밤이 시작된 뒤 보스를 낼 때까지의 틱. 0이면 바로 낸다.
        public int firstSpawnDelayTicks;
    }
}
