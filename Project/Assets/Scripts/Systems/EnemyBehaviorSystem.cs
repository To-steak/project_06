using Project06.Components;
using Project06.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Project06.Systems
{
    public static class EnemyBehaviorSystem
    {
        /// <summary>
        /// 살아 있는 적마다 가장 가까운 살아 있는 플레이어와의 XZ 거리로 상태를 정하고, NavMesh 경로를 따라 한 틱만큼 옮긴다.
        /// 추격 중이 아닌 적은 생성 위치 주변을 걷고 서기를 반복하다가 감지 반지름 안에 플레이어가 들어오면 달려서 추격한다.
        /// 추격 중인 적은 플레이어가 해제 반지름 밖으로 나가면 Idle로 돌아가 감속해 멈춘다.
        /// Dead인 적은 움직이지 않고 사망 시간을 한 틱 줄이며, 사망 시간이 끝나면 소멸시킨다. HitStun인 적은 움직이지 않고 경직 시간을 한 틱 줄이며, 끝나면 추격한다.
        /// Attack인 적은 EnemyAttackSystem이 다루므로 건너뛴다.
        /// PlayerMoveSystem 다음, EnemyAttackSystem 앞에 호출한다.
        /// </summary>
        /// <param name="enemies">옮기고 소멸시킬 적 저장소.</param>
        /// <param name="players">쫓을 플레이어 저장소. 읽기만 한다.</param>
        /// <param name="random">배회 목적지와 대기 시간을 정할 난수 생성기.</param>
        public static void Tick(EnemyStorage enemies, PlayerStorage players, System.Random random)
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

                bool hasTarget = TryFindNearestPlayer(players, enemy.currentPosition, out Vector3 targetPosition, out float targetSqrDistance);
                UpdateState(ref enemy, hasTarget, targetSqrDistance, enemies.navigationPath, random);
                if (enemy.state == EnemyState.Chase)
                {
                    UpdateChasePath(ref enemy, targetPosition, enemies.navigationPath);
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

        // 제자리에 둔 채 경직 시간을 한 틱 줄이고, 끝나면 추격을 시작한다. 플레이어가 해제 반지름 밖이면 다음 틱에 Idle로 돌아간다.
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

        // XZ 평면에서 position에 가장 가까운 살아 있는 플레이어를 찾는다. 없으면 false를 돌려준다. EnemyAttackSystem도 쓴다.
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
                if (!player.isAlive)
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

        private static void UpdateState(ref EnemyEntity enemy, bool hasTarget, float targetSqrDistance, NavMeshPath navigationPath, System.Random random)
        {
            switch (enemy.state)
            {
                case EnemyState.Idle:
                case EnemyState.Wander:
                    float detectionRadius = enemy.enemy.detectionRadius;
                    if (hasTarget && targetSqrDistance <= detectionRadius * detectionRadius)
                    {
                        enemy.state = EnemyState.Chase;
                        // 추격을 시작한 틱에 바로 경로를 계산한다.
                        enemy.pathUpdateRemainingTicks = 0;
                        break;
                    }

                    if (enemy.state == EnemyState.Idle)
                    {
                        enemy.idleRemainingTicks--;
                        if (enemy.idleRemainingTicks <= 0)
                        {
                            StartWander(ref enemy, navigationPath, random);
                        }
                    }
                    else if (enemy.pathCornerIndex >= enemy.pathCornerCount)
                    {
                        StartIdle(ref enemy, random);
                    }
                    break;

                case EnemyState.Chase:
                    float loseRadius = enemy.enemy.loseRadius;
                    if (!hasTarget || targetSqrDistance > loseRadius * loseRadius)
                    {
                        StartIdle(ref enemy, random);
                    }
                    break;
            }
        }

        // Idle로 바꾸고 최소~최대 대기 시간 사이에서 무작위로 서 있을 시간을 정한다.
        private static void StartIdle(ref EnemyEntity enemy, System.Random random)
        {
            int minTicks = enemy.enemy.wanderWaitMinTicks;
            int maxTicks = Mathf.Max(minTicks, enemy.enemy.wanderWaitMaxTicks);
            enemy.state = EnemyState.Idle;
            enemy.idleRemainingTicks = random.Next(minTicks, maxTicks + 1);
        }

        // 생성 위치를 중심으로 한 배회 반지름 안에서 무작위 지점을 골라 NavMesh 위로 옮긴 뒤 그곳까지의 경로로 Wander를 시작한다.
        // NavMesh 위 지점이나 경로를 찾지 못하면 Idle로 한 번 더 기다렸다가 다시 고른다.
        private static void StartWander(ref EnemyEntity enemy, NavMeshPath navigationPath, System.Random random)
        {
            float radius = enemy.enemy.wanderRadius;
            float angle = (float)(random.NextDouble() * 2.0 * Mathf.PI);
            float distance = radius * Mathf.Sqrt((float)random.NextDouble());
            Vector3 destination = enemy.spawnPosition + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

            if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, Mathf.Max(radius, Mathf.Epsilon), NavMesh.AllAreas)
                || !TrySetPath(ref enemy, hit.position, navigationPath))
            {
                StartIdle(ref enemy, random);
                return;
            }

            enemy.state = EnemyState.Wander;
        }

        // 경로 갱신 간격이 지났으면 현재 위치에서 목표 위치까지 NavMesh 경로를 다시 계산한다. 경로를 찾지 못하면 다음 계산까지 경로 없이 둔다.
        private static void UpdateChasePath(ref EnemyEntity enemy, Vector3 targetPosition, NavMeshPath navigationPath)
        {
            enemy.pathUpdateRemainingTicks--;
            if (enemy.pathUpdateRemainingTicks > 0)
            {
                return;
            }

            enemy.pathUpdateRemainingTicks = Mathf.Max(1, enemy.enemy.pathUpdateIntervalTicks);
            TrySetPath(ref enemy, targetPosition, navigationPath);
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

        // 상태에 맞는 속력(배회는 걷기, 추격은 달리기, 그 밖에는 0)으로 다음 코너 쪽 목표 속도를 정해 가속한 뒤, 그 속력만큼 경로를 따라 옮긴다.
        private static void Move(ref EnemyEntity enemy, bool isInStopDistance)
        {
            float targetSpeed = 0f;
            if (enemy.state == EnemyState.Wander)
            {
                targetSpeed = enemy.enemy.walkSpeed;
            }
            else if (enemy.state == EnemyState.Chase && !isInStopDistance)
            {
                targetSpeed = enemy.enemy.runSpeed;
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
