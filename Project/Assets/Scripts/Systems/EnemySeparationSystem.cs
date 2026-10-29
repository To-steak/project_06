using Project06.Components;
using Project06.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Project06.Systems
{
    public static class EnemySeparationSystem
    {
        /// <summary>
        /// 살아 있고 사망 중이 아닌 적의 피격 원(XZ 평면)이 플레이어, 쉘터나 다른 적과 겹치면 겹친 만큼 밀어낸다.
        /// 플레이어나 쉘터와 겹치면 적만 밀리고, 적끼리 겹치면 반씩 밀린다. 사망 중인 적은 밀지도 밀리지도 않는다. 파괴된 쉘터도 남아 있으므로 적을 민다.
        /// 마지막으로 이번 틱에 움직인 적이 NavMesh 밖으로 나가지 않도록, 직전 틱 위치에서 NavMesh 위로 갈 수 있는 곳까지만 옮긴다.
        /// EnemyAttackSystem 다음, ProjectileHitSystem 앞에 호출한다.
        /// </summary>
        /// <param name="enemies">밀어낼 적 저장소.</param>
        /// <param name="players">적을 밀어내는 플레이어 저장소. 읽기만 한다.</param>
        /// <param name="shelters">적을 밀어내는 쉘터 저장소. 읽기만 한다.</param>
        public static void Tick(EnemyStorage enemies, PlayerStorage players, ShelterStorage shelters)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                if (!CanBePushed(in enemy))
                {
                    continue;
                }

                PushFromPlayers(ref enemy, players);
                PushFromShelters(ref enemy, shelters);
                for (int otherIndex = index + 1; otherIndex < entities.Length; otherIndex++)
                {
                    ref EnemyEntity other = ref entities[otherIndex];
                    if (CanBePushed(in other))
                    {
                        PushApart(ref enemy, ref other);
                    }
                }
            }

            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                if (CanBePushed(in enemy))
                {
                    KeepOnNavMesh(ref enemy);
                }
            }
        }

        private static bool CanBePushed(in EnemyEntity enemy)
        {
            return enemy.isAlive && enemy.state != EnemyState.Dead;
        }

        // 살아 있고 사망 중이 아닌 플레이어마다 피격 원이 겹치면 적을 플레이어 반대쪽으로 겹친 만큼 옮긴다.
        private static void PushFromPlayers(ref EnemyEntity enemy, PlayerStorage players)
        {
            PlayerEntity[] entities = players.entities;
            for (int playerIndex = 0; playerIndex < entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref entities[playerIndex];
                if (!player.isAlive || player.isDead)
                {
                    continue;
                }

                if (TryGetOverlap(enemy.currentPosition, player.currentPosition, enemy.enemy.hitRadius + player.player.hitRadius, enemy.facingDirection, out Vector3 pushDirection, out float overlap))
                {
                    enemy.currentPosition += pushDirection * overlap;
                }
            }
        }

        // 살아 있는 쉘터마다 피격 원이 겹치면 적을 쉘터 반대쪽으로 겹친 만큼 옮긴다.
        private static void PushFromShelters(ref EnemyEntity enemy, ShelterStorage shelters)
        {
            ShelterEntity[] entities = shelters.entities;
            for (int shelterIndex = 0; shelterIndex < entities.Length; shelterIndex++)
            {
                ref ShelterEntity shelter = ref entities[shelterIndex];
                if (!shelter.isAlive)
                {
                    continue;
                }

                if (TryGetOverlap(enemy.currentPosition, shelter.position, enemy.enemy.hitRadius + shelter.shelter.hitRadius, enemy.facingDirection, out Vector3 pushDirection, out float overlap))
                {
                    enemy.currentPosition += pushDirection * overlap;
                }
            }
        }

        // 두 적의 피격 원이 겹치면 서로 반대쪽으로 겹친 만큼의 반씩 옮긴다.
        private static void PushApart(ref EnemyEntity enemy, ref EnemyEntity other)
        {
            if (TryGetOverlap(enemy.currentPosition, other.currentPosition, enemy.enemy.hitRadius + other.enemy.hitRadius, enemy.facingDirection, out Vector3 pushDirection, out float overlap))
            {
                Vector3 halfPush = pushDirection * (overlap * 0.5f);
                enemy.currentPosition += halfPush;
                other.currentPosition -= halfPush;
            }
        }

        // XZ 평면에서 position이 otherPosition과 minimumDistance보다 가까우면, position을 밀어낼 방향(길이 1)과 겹친 거리를 구한다.
        // 두 위치가 같으면 fallbackFacing의 반대쪽으로 민다.
        private static bool TryGetOverlap(Vector3 position, Vector3 otherPosition, float minimumDistance, Vector3 fallbackFacing, out Vector3 pushDirection, out float overlap)
        {
            pushDirection = Vector3.zero;
            overlap = 0f;
            Vector3 offset = position - otherPosition;
            offset.y = 0f;
            float sqrDistance = offset.sqrMagnitude;
            if (sqrDistance >= minimumDistance * minimumDistance)
            {
                return false;
            }

            float distance = Mathf.Sqrt(sqrDistance);
            pushDirection = distance > 0f ? offset / distance : -fallbackFacing;
            overlap = minimumDistance - distance;
            return pushDirection.sqrMagnitude > 0f;
        }

        // 직전 틱 위치에서 지금 위치까지 NavMesh 위로 갈 수 없으면, NavMesh 경계에 막힌 곳으로 옮긴다.
        private static void KeepOnNavMesh(ref EnemyEntity enemy)
        {
            if (NavMesh.Raycast(enemy.previousPosition, enemy.currentPosition, out NavMeshHit hit, NavMesh.AllAreas))
            {
                enemy.currentPosition = hit.position;
            }
        }
    }
}
