using Project06.Components;
using Project06.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Project06.Systems
{
    public static class EnemyBehaviorSystem
    {
        // 쉘터 경계에서 NavMesh 위 지점을 찾을 때 쉘터 피격 반지름에 더해 찾는 거리(m)
        private const float SHELTER_PATH_SAMPLE_MARGIN = 1f;

        /// <summary>
        /// 살아 있는 적마다 가장 가까운 살아 있는 플레이어, 파괴되지 않은 쉘터와의 XZ 거리로 상태와 목표를 정하고, NavMesh 경로를 따라 한 틱만큼 옮긴다.
        /// 감지 반지름 안에 플레이어가 들어오면 그 플레이어를 달려서 추격하고, 플레이어가 해제 반지름 밖으로 나가면 놓친 뒤 대기 시간(loseWaitTicks)만큼 서 있다가 쉘터로 걸어간다.
        /// 쉘터가 없으면 제자리에 서서 플레이어가 감지 반지름에 들어오기를 기다린다.
        /// 쉘터까지의 거리는 쉘터 피격 원의 경계부터 잰다.
        /// Dead인 적은 움직이지 않고 사망 시간을 한 틱 줄이며, 사망 시간이 끝나면 소멸시킨다. HitStun인 적은 움직이지 않고 경직 시간을 한 틱 줄이며, 끝나면 원래 목표를 다시 쫓는다.
        /// Attack인 적은 EnemyAttackSystem이 다루므로 건너뛴다.
        /// PlayerMoveSystem 다음, EnemyAttackSystem 앞에 호출한다.
        /// </summary>
        /// <param name="enemies">옮기고 소멸시킬 적 저장소.</param>
        /// <param name="players">쫓을 플레이어 저장소. 읽기만 한다.</param>
        /// <param name="shelters">향할 쉘터 저장소. 읽기만 한다.</param>
        public static void Tick(EnemyStorage enemies, PlayerStorage players, ShelterStorage shelters)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                if (!enemy.isAlive)
                {
                    continue;
                }

                if (enemy.state == EnemyState.Dead)
                {
                    TickDeath(ref enemy, index, enemies);
                    continue;
                }

                if (enemy.state == EnemyState.HitStun)
                {
                    TickHitStun(ref enemy);
                    continue;
                }

                if (enemy.state == EnemyState.Attack)
                {
                    continue;
                }

                bool hasPlayer = TryFindNearestPlayer(players, enemy.currentPosition, out Vector3 playerPosition, out float playerSqrDistance);
                bool hasShelter = TryFindNearestShelter(shelters, enemy.currentPosition, out int shelterIndex, out Vector3 shelterPosition, out float shelterSqrDistance);
                UpdateState(ref enemy, hasPlayer, playerSqrDistance, hasShelter);

                bool isTargetShelter = enemy.targetKind == EnemyTargetKind.Shelter;
                Vector3 targetPosition = isTargetShelter ? shelterPosition : playerPosition;
                float targetSqrDistance = isTargetShelter ? shelterSqrDistance : playerSqrDistance;
                if (enemy.state == EnemyState.Chase && TickPathUpdate(ref enemy))
                {
                    Vector3 destination = isTargetShelter
                        ? GetShelterPathDestination(in shelters.entities[shelterIndex], enemy.currentPosition)
                        : playerPosition;
                    TrySetPath(ref enemy, destination, enemies.navigationPath);
                }

                bool isInStopDistance = enemy.state == EnemyState.Chase && targetSqrDistance <= enemy.enemy.stopDistance * enemy.enemy.stopDistance;
                Move(ref enemy, isInStopDistance);
                UpdateFacing(ref enemy, isInStopDistance, targetPosition);
            }
        }

        // 제자리에 둔 채 사망 시간을 한 틱 줄이고, 끝나면 소멸시켜 슬롯을 비운다.
        private static void TickDeath(ref EnemyEntity enemy, int index, EnemyStorage enemies)
        {
            enemy.previousPosition = enemy.currentPosition;
            enemy.deathRemainingTicks--;
            if (enemy.deathRemainingTicks > 0)
            {
                return;
            }

            enemy.isAlive = false;
            enemies.freeIndices.Add(index);
        }

        // 제자리에 둔 채 경직 시간을 한 틱 줄이고, 끝나면 원래 목표의 추격을 다시 시작한다. 목표가 사라졌거나 멀어졌으면 다음 틱에 목표를 다시 고른다.
        private static void TickHitStun(ref EnemyEntity enemy)
        {
            enemy.previousPosition = enemy.currentPosition;
            enemy.hitStunRemainingTicks--;
            if (enemy.hitStunRemainingTicks > 0)
            {
                return;
            }

            enemy.state = EnemyState.Chase;
            enemy.pathUpdateRemainingTicks = 0;
        }

        // XZ 평면에서 position에 가장 가까운, 살아 있고 사망 중이 아닌 플레이어를 찾는다. 없으면 false를 돌려준다. EnemyAttackSystem도 쓴다.
        internal static bool TryFindNearestPlayer(PlayerStorage players, Vector3 position, out Vector3 targetPosition, out float targetSqrDistance)
        {
            return TryFindNearestPlayer(players, position, out _, out targetPosition, out targetSqrDistance);
        }

        // TryFindNearestPlayer와 같고, 찾은 플레이어의 슬롯 인덱스도 돌려준다. 없으면 -1이다.
        internal static bool TryFindNearestPlayer(PlayerStorage players, Vector3 position, out int targetIndex, out Vector3 targetPosition, out float targetSqrDistance)
        {
            targetIndex = -1;
            targetPosition = Vector3.zero;
            targetSqrDistance = float.MaxValue;
            bool hasTarget = false;
            PlayerEntity[] entities = players.entities;
            for (int playerIndex = 0; playerIndex < entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref entities[playerIndex];
                if (!player.isAlive || player.isDead)
                {
                    continue;
                }

                Vector3 offset = player.currentPosition - position;
                offset.y = 0f;
                float sqrDistance = offset.sqrMagnitude;
                if (sqrDistance < targetSqrDistance)
                {
                    targetSqrDistance = sqrDistance;
                    targetIndex = playerIndex;
                    targetPosition = player.currentPosition;
                    hasTarget = true;
                }
            }

            return hasTarget;
        }

        // XZ 평면에서 position에 가장 가까운, 살아 있고 파괴되지 않은 쉘터를 찾는다. 거리는 쉘터 피격 원의 경계부터 재며, 원 안이면 0이다. 없으면 false를 돌려준다.
        // EnemyAttackSystem도 쓴다.
        internal static bool TryFindNearestShelter(ShelterStorage shelters, Vector3 position, out int targetIndex, out Vector3 targetPosition, out float targetSqrDistance)
        {
            targetIndex = -1;
            targetPosition = Vector3.zero;
            targetSqrDistance = float.MaxValue;
            bool hasTarget = false;
            ShelterEntity[] entities = shelters.entities;
            for (int shelterIndex = 0; shelterIndex < entities.Length; shelterIndex++)
            {
                ref ShelterEntity shelter = ref entities[shelterIndex];
                if (!shelter.isAlive || shelter.isDestroyed)
                {
                    continue;
                }

                Vector3 offset = shelter.position - position;
                offset.y = 0f;
                float edgeDistance = Mathf.Max(0f, offset.magnitude - shelter.shelter.hitRadius);
                float sqrDistance = edgeDistance * edgeDistance;
                if (sqrDistance < targetSqrDistance)
                {
                    targetSqrDistance = sqrDistance;
                    targetIndex = shelterIndex;
                    targetPosition = shelter.position;
                    hasTarget = true;
                }
            }

            return hasTarget;
        }

        // 적의 목표 종류에 맞는 가장 가까운 목표의 위치와 XZ 거리의 제곱을 구한다. 쉘터는 피격 원의 경계부터 잰다. 목표가 없으면 false를 돌려준다.
        // EnemyAttackSystem이 공격할 목표를 고를 때 쓴다.
        internal static bool TryFindTarget(in EnemyEntity enemy, PlayerStorage players, ShelterStorage shelters, out Vector3 targetPosition, out float targetSqrDistance)
        {
            return enemy.targetKind == EnemyTargetKind.Shelter
                ? TryFindNearestShelter(shelters, enemy.currentPosition, out _, out targetPosition, out targetSqrDistance)
                : TryFindNearestPlayer(players, enemy.currentPosition, out targetPosition, out targetSqrDistance);
        }

        // 플레이어가 감지 반지름 안에 들어오면 플레이어를, 아니면 쉘터를 쫓기 시작한다. Idle이면 대기 시간이 끝난 뒤에 쉘터로 향한다.
        // 쫓던 플레이어가 해제 반지름 밖으로 나가면 놓친 뒤 대기 시간만큼 Idle로 서 있다가 쉘터로 바꾼다. 쫓을 쉘터가 없으면 Idle로 서 있는다.
        private static void UpdateState(ref EnemyEntity enemy, bool hasPlayer, float playerSqrDistance, bool hasShelter)
        {
            float detectionRadius = enemy.enemy.detectionRadius;
            bool isPlayerDetected = hasPlayer && playerSqrDistance <= detectionRadius * detectionRadius;
            switch (enemy.state)
            {
                case EnemyState.Idle:
                    if (isPlayerDetected)
                    {
                        StartChase(ref enemy, EnemyTargetKind.Player);
                        break;
                    }

                    if (enemy.idleRemainingTicks > 0)
                    {
                        enemy.idleRemainingTicks--;
                    }

                    if (enemy.idleRemainingTicks <= 0 && hasShelter)
                    {
                        StartChase(ref enemy, EnemyTargetKind.Shelter);
                    }
                    break;

                case EnemyState.Chase:
                    if (enemy.targetKind == EnemyTargetKind.Shelter)
                    {
                        if (isPlayerDetected)
                        {
                            StartChase(ref enemy, EnemyTargetKind.Player);
                        }
                        else if (!hasShelter)
                        {
                            StartIdle(ref enemy, 0);
                        }
                        break;
                    }

                    float loseRadius = enemy.enemy.loseRadius;
                    if (hasPlayer && playerSqrDistance <= loseRadius * loseRadius)
                    {
                        break;
                    }

                    StartIdle(ref enemy, enemy.enemy.loseWaitTicks);
                    break;
            }
        }

        // targetKind 목표를 쫓는 Chase로 바꾼다. 쫓기 시작한 틱에 바로 경로를 계산한다.
        private static void StartChase(ref EnemyEntity enemy, EnemyTargetKind targetKind)
        {
            enemy.state = EnemyState.Chase;
            enemy.targetKind = targetKind;
            enemy.pathUpdateRemainingTicks = 0;
        }

        // Idle로 바꾸고 쉘터로 향하기 전에 서 있을 시간을 정한다. 목표 속력이 0이 되어 남은 경로를 따라 감속해 멈춘다.
        private static void StartIdle(ref EnemyEntity enemy, int waitTicks)
        {
            enemy.state = EnemyState.Idle;
            enemy.idleRemainingTicks = waitTicks;
        }

        // 경로 갱신까지 남은 시간을 한 틱 줄이고, 간격이 지났으면 다음 간격을 걸고 true를 돌려준다. 경로를 찾지 못하면 다음 계산까지 경로 없이 둔다.
        private static bool TickPathUpdate(ref EnemyEntity enemy)
        {
            enemy.pathUpdateRemainingTicks--;
            if (enemy.pathUpdateRemainingTicks > 0)
            {
                return false;
            }

            enemy.pathUpdateRemainingTicks = Mathf.Max(1, enemy.enemy.pathUpdateIntervalTicks);
            return true;
        }

        // 쉘터 피격 원의 경계 중 position 쪽 지점을 경로 목적지로 구한다.
        // 쉘터가 NavMesh를 깎아 경계가 NavMesh 밖이어도 경로를 찾도록, 경계에서 가장 가까운 NavMesh 위 지점을 쓴다. 찾지 못하면 경계 지점을 그대로 쓴다.
        private static Vector3 GetShelterPathDestination(in ShelterEntity shelter, Vector3 position)
        {
            float hitRadius = shelter.shelter.hitRadius;
            Vector3 offset = position - shelter.position;
            offset.y = 0f;
            Vector3 edge = offset.sqrMagnitude > 0f ? shelter.position + offset.normalized * hitRadius : shelter.position;
            if (NavMesh.SamplePosition(edge, out NavMeshHit hit, hitRadius + SHELTER_PATH_SAMPLE_MARGIN, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return edge;
        }

        // 현재 위치에서 destination까지 NavMesh 경로를 계산해 코너를 담는다. 찾지 못하면 경로를 비우고 false를 돌려준다.
        private static bool TrySetPath(ref EnemyEntity enemy, Vector3 destination, NavMeshPath navigationPath)
        {
            enemy.pathCornerCount = 0;
            enemy.pathCornerIndex = 0;
            if (!NavMesh.CalculatePath(enemy.currentPosition, destination, NavMesh.AllAreas, navigationPath)
                || navigationPath.status == NavMeshPathStatus.PathInvalid)
            {
                return false;
            }

            enemy.pathCornerCount = navigationPath.GetCornersNonAlloc(enemy.pathCorners);
            // 첫 코너는 출발 위치다.
            enemy.pathCornerIndex = 1;
            return true;
        }

        // 상태에 맞는 속력(쉘터로 향할 때는 걷기, 플레이어 추격은 달리기, 그 밖에는 0)으로 다음 코너 쪽 목표 속도를 정해 가속한 뒤, 그 속력만큼 경로를 따라 옮긴다.
        private static void Move(ref EnemyEntity enemy, bool isInStopDistance)
        {
            float targetSpeed = 0f;
            if (enemy.state == EnemyState.Chase && !isInStopDistance)
            {
                targetSpeed = enemy.targetKind == EnemyTargetKind.Shelter ? enemy.enemy.walkSpeed : enemy.enemy.runSpeed;
            }

            Vector3 targetVelocity = Vector3.zero;
            if (targetSpeed > 0f && enemy.pathCornerIndex < enemy.pathCornerCount)
            {
                Vector3 toCorner = enemy.pathCorners[enemy.pathCornerIndex] - enemy.currentPosition;
                targetVelocity = toCorner.normalized * targetSpeed;
            }

            float maxDelta = enemy.enemy.acceleration * Time.fixedDeltaTime;
            enemy.velocity = Vector3.MoveTowards(enemy.velocity, targetVelocity, maxDelta);

            enemy.previousPosition = enemy.currentPosition;
            FollowPath(ref enemy, enemy.velocity.magnitude * Time.fixedDeltaTime);
        }

        // 남은 코너를 차례로 지나며 distance만큼 옮긴다. 경로를 다 걸으면 멈춘다.
        private static void FollowPath(ref EnemyEntity enemy, float distance)
        {
            while (distance > 0f && enemy.pathCornerIndex < enemy.pathCornerCount)
            {
                Vector3 corner = enemy.pathCorners[enemy.pathCornerIndex];
                Vector3 toCorner = corner - enemy.currentPosition;
                float cornerDistance = toCorner.magnitude;
                if (cornerDistance > distance)
                {
                    enemy.currentPosition += toCorner * (distance / cornerDistance);
                    return;
                }

                enemy.currentPosition = corner;
                enemy.pathCornerIndex++;
                distance -= cornerDistance;
            }

            if (enemy.pathCornerIndex >= enemy.pathCornerCount)
            {
                enemy.velocity = Vector3.zero;
            }
        }

        // 추격 중 정지 거리 안이면 목표를 바라보고, 아니면 이번 틱에 움직인 XZ 방향을 바라본다. 움직이지 않았으면 방향을 유지한다.
        private static void UpdateFacing(ref EnemyEntity enemy, bool isInStopDistance, Vector3 targetPosition)
        {
            Vector3 direction = isInStopDistance
                ? targetPosition - enemy.currentPosition
                : enemy.currentPosition - enemy.previousPosition;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0f)
            {
                enemy.facingDirection = direction.normalized;
            }
        }
    }
}
