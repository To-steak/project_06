using Project06.Commands;
using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class CommandSystem
    {
        /// <summary>
        /// 이동 명령을 플레이어의 이동 방향과 달리기 여부에 반영한다.
        /// </summary>
        /// <param name="moveCommand">이번 틱에 적용할 이동 명령.</param>
        /// <param name="player">명령을 받을 플레이어.</param>
        public static void Tick(in MoveCommand moveCommand, ref PlayerEntity player)
        {
            if (!player.isAlive)
            {
                return;
            }

            Vector3 direction = moveCommand.direction;
            direction.y = 0f;
            player.moveDirection = Vector3.ClampMagnitude(direction, 1f);
            player.isRunning = moveCommand.run;
        }
    }
}
