using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerMoveSystem
    {
        /// <summary>
        /// 플레이어를 이동 방향과 속도에 따라 한 틱만큼 옮긴다.
        /// </summary>
        /// <param name="player">옮길 플레이어.</param>
        public static void Tick(ref PlayerEntity player)
        {
            if (!player.isAlive)
            {
                return;
            }

            player.previousPosition = player.currentPosition;
            float distance = player.movement.moveSpeed * Time.fixedDeltaTime;
            player.currentPosition += player.moveDirection * distance;
        }
    }
}
