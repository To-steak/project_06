using System.Collections.Generic;
using Project06.Commands;
using Project06.Entities;
using Project06.Records;
using Project06.Requests;

namespace Project06.Core
{
    public class World
    {
        // 플레이어 엔티티 저장소
        public PlayerStorage players = new PlayerStorage(MAX_PLAYER_COUNT);
        // 투사체 엔티티 저장소
        public ProjectileStorage projectiles = new ProjectileStorage(MAX_PROJECTILE_COUNT);
        // 적 엔티티 저장소
        public EnemyStorage enemies = new EnemyStorage(MAX_ENEMY_COUNT);
        // 적 투사체 엔티티 저장소
        public EnemyProjectileStorage enemyProjectiles = new EnemyProjectileStorage(MAX_ENEMY_PROJECTILE_COUNT);
        // 적 스포너 엔티티 저장소
        public EnemySpawnerStorage enemySpawners = new EnemySpawnerStorage(MAX_ENEMY_SPAWNER_COUNT);
        // 쉘터 엔티티 저장소
        public ShelterStorage shelters = new ShelterStorage(MAX_SHELTER_COUNT);
        // 틱 마지막에 SpawnSystem이 처리할 플레이어 생성 요청
        public Queue<PlayerSpawnRequest> playerSpawnRequests = new Queue<PlayerSpawnRequest>();
        // 틱 마지막에 SpawnSystem이 처리할 쉘터 생성 요청
        public Queue<ShelterSpawnRequest> shelterSpawnRequests = new Queue<ShelterSpawnRequest>();
        // 틱 마지막에 SpawnSystem이 처리할 적 스포너 생성 요청
        public Queue<EnemySpawnerSpawnRequest> enemySpawnerSpawnRequests = new Queue<EnemySpawnerSpawnRequest>();
        // 틱 마지막에 SpawnSystem이 처리할 적 생성 요청
        public Queue<EnemySpawnRequest> enemySpawnRequests = new Queue<EnemySpawnRequest>();
        // 틱 마지막에 SpawnSystem이 처리할 투사체 생성 요청
        public Queue<ProjectileSpawnRequest> projectileSpawnRequests = new Queue<ProjectileSpawnRequest>();
        // 틱 마지막에 SpawnSystem이 처리할 적 투사체 생성 요청
        public Queue<EnemyProjectileSpawnRequest> enemyProjectileSpawnRequests = new Queue<EnemyProjectileSpawnRequest>();
        // 플레이어가 발사한 기록. LateUpdate 마지막에 비운다.
        public List<PlayerFiredRecord> playerFiredRecords = new List<PlayerFiredRecord>();
        // 적이 공격을 시작한 기록. LateUpdate 마지막에 비운다.
        public List<EnemyAttackStartedRecord> enemyAttackStartedRecords = new List<EnemyAttackStartedRecord>();
        // 적이 경직에 걸린 기록. LateUpdate 마지막에 비운다.
        public List<EnemyHitStunnedRecord> enemyHitStunnedRecords = new List<EnemyHitStunnedRecord>();
        // Update에서 쌓고 Tick에서 소비하는 입력 명령
        public CommandBuffer commandBuffer = new CommandBuffer();
        // 판마다 같은 시드로 만드는 난수 생성기. System은 UnityEngine.Random 대신 이것을 받아 쓴다.
        public System.Random random = new System.Random(RANDOM_SEED);

        // 동시에 존재할 수 있는 플레이어의 최대 수
        public const int MAX_PLAYER_COUNT = 1;
        // 동시에 존재할 수 있는 투사체의 최대 수
        public const int MAX_PROJECTILE_COUNT = 256;
        // 동시에 존재할 수 있는 적의 최대 수
        public const int MAX_ENEMY_COUNT = 128;
        // 동시에 존재할 수 있는 적 투사체의 최대 수
        public const int MAX_ENEMY_PROJECTILE_COUNT = 256;
        // 동시에 존재할 수 있는 적 스포너의 최대 수
        public const int MAX_ENEMY_SPAWNER_COUNT = 64;
        // 동시에 존재할 수 있는 쉘터의 최대 수
        public const int MAX_SHELTER_COUNT = 1;
        // 난수 생성기의 시드
        public const int RANDOM_SEED = 0;
    }
}
