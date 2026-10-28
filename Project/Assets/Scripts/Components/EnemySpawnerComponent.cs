using System;

namespace Project06.Components
{
    [Serializable]
    public struct EnemySpawnerComponent
    {
        // 스포너가 생긴 뒤 첫 적을 낼 때까지의 틱. 0이면 스포너가 생긴 다음 틱에 바로 낸다.
        public int firstSpawnDelayTicks;
        // 적을 낸 뒤 다음 적을 낼 때까지의 틱. 1보다 작으면 1로 본다.
        public int intervalTicks;
        // 이 스포너가 낸 적이 동시에 살아 있을 수 있는 최대 수. 다 차면 타이머가 멈춘다. 0이면 제한이 없다.
        public int maxAliveCount;
        // 이 스포너가 낼 적의 총수. 다 내면 스포너가 소멸한다. 0이면 제한이 없다.
        public int totalSpawnCount;
        // 스포너 위치를 중심으로 적이 생성될 XZ 평면 원의 반지름(m). 0이면 정확히 스포너 위치에 낸다.
        public float spawnRadius;
    }
}
