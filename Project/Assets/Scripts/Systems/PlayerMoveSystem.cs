using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerMoveSystem
    {
        /// <summary>
        /// 플레이어 속도를 목표 속도로 가속한 뒤 한 틱만큼 옮긴다.
        /// </summary>
        /// <param name="player">옮길 플레이어.</param>
        public static void Tick(ref PlayerEntity player)
        {
            if (!player.isAlive)
            {
                return;
            }

            float targetSpeed = player.isRunning ? player.movement.runSpeed : player.movement.moveSpeed;
            Vector3 targetVelocity = player.moveDirection * targetSpeed;
            float maxDelta = player.movement.acceleration * Time.fixedDeltaTime;
            player.velocity = Vector3.MoveTowards(player.velocity, targetVelocity, maxDelta);

            player.previousPosition = player.currentPosition;
            player.currentPosition += player.velocity * Time.fixedDeltaTime;
        }
    }
}
