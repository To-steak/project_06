using Project06.Entities;
using Project06.Presentation.Player.Views;
using UnityEngine;

namespace Project06.Presentation.Player.ViewSystems
{
    public static class PlayerViewSystem
    {
        private static readonly int MOVE_X_PARAMETER_HASH = Animator.StringToHash(MOVE_X_PARAMETER);
        private static readonly int MOVE_Z_PARAMETER_HASH = Animator.StringToHash(MOVE_Z_PARAMETER);

        // 플레이어가 바라보는 방향 기준 이동량. X는 오른쪽, Z는 앞쪽이 양수다.
        // 길이는 걷기 속도에서 1, 달리기 속도에서 2가 되도록 정규화한다.
        private const string MOVE_X_PARAMETER = "MoveX";
        private const string MOVE_Z_PARAMETER = "MoveZ";

        /// <summary>
        /// 플레이어 위치를 직전 틱과 현재 틱 사이로 보간해 View에 반영하고, 바라보는 방향 기준 이동량을 Animator에 넘긴다.
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

            if (playerView.animator != null)
            {
                Vector3 blendPosition = GetBlendPosition(in player);
                Vector3 localBlendPosition = playerView.transform.InverseTransformDirection(blendPosition);
                playerView.animator.SetFloat(MOVE_X_PARAMETER_HASH, localBlendPosition.x);
                playerView.animator.SetFloat(MOVE_Z_PARAMETER_HASH, localBlendPosition.z);
            }
        }

        // 월드 속도를 같은 방향, 걷기 속도에서 길이 1, 달리기 속도에서 길이 2인 벡터로 바꾼다.
        private static Vector3 GetBlendPosition(in PlayerEntity player)
        {
            float speed = player.velocity.magnitude;
            if (speed <= 0f)
            {
                return Vector3.zero;
            }

            float moveSpeed = player.movement.moveSpeed;
            float runSpeed = player.movement.runSpeed;
            float blendLength = speed <= moveSpeed
                ? Mathf.InverseLerp(0f, moveSpeed, speed)
                : 1f + Mathf.InverseLerp(moveSpeed, runSpeed, speed);

            return player.velocity / speed * blendLength;
        }
    }
}
