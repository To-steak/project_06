using Project06.Components;
using Project06.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Project06.Systems
{
    public static class EnemyBehaviorSystem
    {
        /// <summary>
        /// 살아 있는 적마다 가장 가까운 살아 있는 플레이어와의 XZ 거리로 상태를 정하고, 추격 중이면 NavMesh 경로를 따라 한 틱만큼 옮긴다.
        /// Idle인 적은 감지 반지름 안에 플레이어가 들어오면 추격을 시작하고, 추격 중인 적은 플레이어가 해제 반지름 밖으로 나가면 Idle로 돌아가 감속해 멈춘다.
        /// PlayerMoveSystem 다음, ProjectileHitSystem 앞에 호출한다.
        /// </summary>
        /// <param name="enemies">옮길 적 저장소.</param>
        /// <param name="players">쫓을 플레이어 저장소. 읽기만 한다.</param>
        public static void Tick(EnemyStorage enemies, PlayerStorage players)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                if (!enemy.isAlive)
                {
                    continue;
                }

                bool hasTarget = TryFindNearestPlayer(players, enemy.currentPosition, out Vector3 targetPosition, out float targetSqrDistance);
                UpdateState(ref enemy, hasTarget, targetSqrDistance);
                if (enemy.state == EnemyState.Chase)
                {
                    UpdatePath(ref enemy, targetPosition, enemies.navigationPath);
                }

                bool isInStopDistance = hasTarget && targetSqrDistance <= enemy.enemy.stopDistance * enemy.enemy.stopDistance;
                Move(ref enemy, isInStopDistance);
                UpdateFacing(ref enemy, isInStopDistance, targetPosition);
            }
        }

        // XZ 평면에서 position에 가장 가까운 살아 있는 플레이어를 찾는다. 없으면 false를 돌려준다.
        private static bool TryFindNearestPlayer(PlayerStorage players, Vector3 position, out Vector3 targetPosition, out float targetSqrDistance)
        {
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
                    targetPosition = player.currentPosition;
                    hasTarget = true;
                }
            }

            return hasTarget;
        }

        private static void UpdateState(ref EnemyEntity enemy, bool hasTarget, float targetSqrDistance)
        {
            switch (enemy.state)
            {
                case EnemyState.Idle:
                    float detectionRadius = enemy.enemy.detectionRadius;
                    if (hasTarget && targetSqrDistance <= detectionRadius * detectionRadius)
                    {
                        enemy.state = EnemyState.Chase;
                        // 추격을 시작한 틱에 바로 경로를 계산한다.
                        enemy.pathUpdateRemainingTicks = 0;
                    }
                    break;

                case EnemyState.Chase:
                    float loseRadius = enemy.enemy.loseRadius;
                    if (!hasTarget || targetSqrDistance > loseRadius * loseRadius)
                    {
                        enemy.state = EnemyState.Idle;
                    }
                    break;
            }
        }

        // 경로 갱신 간격이 지났으면 현재 위치에서 목표 위치까지 NavMesh 경로를 다시 계산한다. 경로를 찾지 못하면 다음 계산까지 경로 없이 둔다.
        private static void UpdatePath(ref EnemyEntity enemy, Vector3 targetPosition, NavMeshPath navigationPath)
        {
            enemy.pathUpdateRemainingTicks--;
            if (enemy.pathUpdateRemainingTicks > 0)
            {
                return;
            }

            enemy.pathUpdateRemainingTicks = Mathf.Max(1, enemy.enemy.pathUpdateIntervalTicks);
            enemy.pathCornerCount = 0;
            enemy.pathCornerIndex = 0;
            if (!NavMesh.CalculatePath(enemy.currentPosition, targetPosition, NavMesh.AllAreas, navigationPath)
                || navigationPath.status == NavMeshPathStatus.PathInvalid)
            {
                return;
            }

            enemy.pathCornerCount = navigationPath.GetCornersNonAlloc(enemy.pathCorners);
            // 첫 코너는 출발 위치다.
            enemy.pathCornerIndex = 1;
        }

        // 추격 중이고 정지 거리 밖이면 다음 코너 쪽 목표 속도로, 아니면 0으로 가속한 뒤 그 속력만큼 경로를 따라 옮긴다.
        private static void Move(ref EnemyEntity enemy, bool isInStopDistance)
        {
            Vector3 targetVelocity = Vector3.zero;
            if (enemy.state == EnemyState.Chase && !isInStopDistance && enemy.pathCornerIndex < enemy.pathCornerCount)
            {
                Vector3 toCorner = enemy.pathCorners[enemy.pathCornerIndex] - enemy.currentPosition;
                targetVelocity = toCorner.normalized * enemy.enemy.moveSpeed;
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
            Vector3 direction = enemy.state == EnemyState.Chase && isInStopDistance
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
