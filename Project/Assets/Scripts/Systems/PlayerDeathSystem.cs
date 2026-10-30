using Project06.Components;
using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerDeathSystem
    {
        /// <summary>
        /// 체력이 0 이하가 된 플레이어를 사망 상태로 바꾸고, 사망 중인 플레이어는 쓰러짐 → 일어남 단계를 진행한다.
        /// 사망하면 제자리에 쓰러져 멈추고 달리기·조준·발사·재장전·근접 공격이 모두 풀린다.
        /// 부활 시간이 끝나면 처음 생성된 위치와 방향으로 옮겨 체력과 정신력을 가득 채우고 일어나기 시작하며, 일어나는 시간이 끝나면 다시 조작할 수 있다. 탄약은 그대로 둔다.
        /// 플레이어가 데미지를 받는 EnemyAttackSystem, EnemyProjectileHitSystem, PlayerSanitySystem 다음, PlayerFireSystem 앞에 호출한다.
        /// </summary>
        /// <param name="players">사망과 부활을 처리할 플레이어 저장소.</param>
        public static void Tick(PlayerStorage players)
        {
            PlayerEntity[] entities = players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref PlayerEntity player = ref entities[index];
                if (!player.isAlive)
                {
                    continue;
                }

                if (player.isGettingUp)
                {
                    TickGetUp(ref player);
                }
                else if (player.isDead)
                {
                    TickDown(ref player);
                }
                else if (player.health <= 0f)
                {
                    Die(ref player);
                }
            }
        }

        private static void Die(ref PlayerEntity player)
        {
            player.isDead = true;
            player.respawnRemainingTicks = Mathf.Max(1, player.player.respawnDelayTicks);
            player.health = 0f;
            player.previousPosition = player.currentPosition;
            player.velocity = Vector3.zero;
            player.moveDirection = Vector3.zero;
            player.isRunning = false;
            player.isAiming = false;
            player.isFiring = false;
            player.isReloading = false;
            player.reloadRemainingTicks = 0;
            player.meleePhase = PlayerMeleePhase.None;
            player.meleeRemainingTicks = 0;
        }

        // 쓰러진 채 부활까지 남은 시간을 한 틱 줄이고, 끝나면 시작 위치로 옮겨 체력과 정신력을 채우고 일어나기 시작한다.
        private static void TickDown(ref PlayerEntity player)
        {
            player.previousPosition = player.currentPosition;
            player.respawnRemainingTicks--;
            if (player.respawnRemainingTicks > 0)
            {
                return;
            }

            player.respawnRemainingTicks = 0;
            player.isGettingUp = true;
            player.getUpRemainingTicks = Mathf.Max(1, player.player.getUpTicks);
            player.health = player.player.maxHealth;
            player.mentality = player.player.maxMentality;
            // 보간으로 사망 위치에서 미끄러져 오지 않도록 직전 위치도 함께 옮긴다.
            player.previousPosition = player.spawnPosition;
            player.currentPosition = player.spawnPosition;
            player.facingDirection = player.spawnFacingDirection;
        }

        // 제자리에 둔 채 일어나는 시간을 한 틱 줄이고, 끝나면 사망 상태를 풀어 다시 조작할 수 있게 한다.
        private static void TickGetUp(ref PlayerEntity player)
        {
            player.previousPosition = player.currentPosition;
            player.getUpRemainingTicks--;
            if (player.getUpRemainingTicks > 0)
            {
                return;
            }

            player.getUpRemainingTicks = 0;
            player.isGettingUp = false;
            player.isDead = false;
        }
    }
}
