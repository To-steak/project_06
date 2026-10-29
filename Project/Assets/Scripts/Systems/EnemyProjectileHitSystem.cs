using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class EnemyProjectileHitSystem
    {
        /// <summary>
        /// 살아 있는 적 투사체마다 이번 틱에 날아갈 선분이 살아 있고 사망 중이 아닌 플레이어의 피격 원(XZ 평면)과 만나는지 검사한다. EnemyProjectileMoveSystem보다 먼저 호출한다.
        /// 가장 먼저 만나는 플레이어에게 데미지를 주고 투사체를 소멸시킨다. 높이(Y)는 판정에 쓰지 않는다.
        /// </summary>
        /// <param name="enemyProjectiles">검사할 적 투사체 저장소.</param>
        /// <param name="playerStorage">맞을 수 있는 플레이어 저장소.</param>
        public static void Tick(EnemyProjectileStorage enemyProjectiles, PlayerStorage playerStorage)
        {
            PlayerEntity[] players = playerStorage.entities;
            EnemyProjectileEntity[] entities = enemyProjectiles.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyProjectileEntity enemyProjectile = ref entities[index];
                if (!enemyProjectile.isAlive)
                {
                    continue;
                }

                Vector3 step = enemyProjectile.velocity * Time.fixedDeltaTime;
                float stepLength = step.magnitude;
                if (stepLength > enemyProjectile.remainingDistance && stepLength > 0f)
                {
                    step *= enemyProjectile.remainingDistance / stepLength;
                }

                int hitPlayerIndex = -1;
                float nearestHitRatio = float.MaxValue;
                for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
                {
                    ref PlayerEntity player = ref players[playerIndex];
                    if (!player.isAlive || player.isDead)
                    {
                        continue;
                    }

                    if (ProjectileHitSystem.TryGetHitRatio(enemyProjectile.currentPosition, step, player.currentPosition, player.player.hitRadius, out float hitRatio) && hitRatio < nearestHitRatio)
                    {
                        nearestHitRatio = hitRatio;
                        hitPlayerIndex = playerIndex;
                    }
                }

                if (hitPlayerIndex < 0)
                {
                    continue;
                }

                ref PlayerEntity hitPlayer = ref players[hitPlayerIndex];
                hitPlayer.health -= enemyProjectile.damage;

                enemyProjectile.isAlive = false;
                enemyProjectiles.freeIndices.Add(index);
            }
        }
    }
}
