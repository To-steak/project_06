using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerMoveSystem
    {
        /// <summary>
        /// 플레이어 속도를 목표 속도로 가속한 뒤 한 틱만큼 옮긴다.
        /// </summary>
        /// <param name="players">옮길 플레이어 저장소.</param>
        public static void Tick(PlayerStorage players)
        {
            PlayerEntity[] entities = players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref PlayerEntity player = ref entities[index];
                if (!player.isAlive || player.isDead)
                {
                    continue;
                }

                Move(ref player);
            }
        }

        private static void Move(ref PlayerEntity player)
        {
            float targetSpeed = player.isRunning ? player.movement.runSpeed : player.movement.moveSpeed;
            Vector3 targetVelocity = player.moveDirection * targetSpeed;
            float maxDelta = player.movement.acceleration * Time.fixedDeltaTime;
            player.velocity = Vector3.MoveTowards(player.velocity, targetVelocity, maxDelta);

            player.previousPosition = player.currentPosition;
            player.currentPosition += player.velocity * Time.fixedDeltaTime;
        }
    }
}
