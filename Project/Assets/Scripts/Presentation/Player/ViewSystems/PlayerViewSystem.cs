using Project06.Entities;
using Project06.Presentation.Player.Views;
using UnityEngine;

namespace Project06.Presentation.Player.ViewSystems
{
    public static class PlayerViewSystem
    {
        /// <summary>
        /// 플레이어 위치를 직전 틱과 현재 틱 사이로 보간해 View에 반영한다.
        /// </summary>
        /// <param name="player">표시할 플레이어.</param>
        /// <param name="interpolation">직전 틱에서 현재 틱까지 진행한 비율(0~1).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(in PlayerEntity player, float interpolation, ViewContext context)
        {
            PlayerView playerView = context.playerView;
            if (playerView == null || !player.isAlive)
            {
                return;
            }

            playerView.transform.position = Vector3.Lerp(player.previousPosition, player.currentPosition, interpolation);
        }
    }
}
