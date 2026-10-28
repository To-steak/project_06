using System.Collections.Generic;
using Project06.Commands;
using Project06.Entities;
using Project06.Records;
using Project06.Requests;

namespace Project06.Core
{
    public class World
    {
        // 플레이어 엔티티
        public PlayerEntity player;
        // 투사체 엔티티 저장소
        public ProjectileStorage projectiles = new ProjectileStorage(MAX_PROJECTILE_COUNT);
        // 틱 마지막에 SpawnSystem이 처리할 투사체 생성 요청
        public Queue<ProjectileSpawnRequest> projectileSpawnRequests = new Queue<ProjectileSpawnRequest>();
        // 플레이어가 발사한 기록. LateUpdate 마지막에 비운다.
        public List<PlayerFiredRecord> playerFiredRecords = new List<PlayerFiredRecord>();
        // Update에서 쌓고 Tick에서 소비하는 입력 명령
        public CommandBuffer commandBuffer = new CommandBuffer();

        // 동시에 존재할 수 있는 투사체의 최대 수
        public const int MAX_PROJECTILE_COUNT = 256;
    }
}
