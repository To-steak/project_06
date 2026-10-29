using System.Collections.Generic;
using Project06.Entities;
using Project06.Presentation.Player.Views;
using Project06.Records;
using UnityEngine;

namespace Project06.Presentation.Player.ViewSystems
{
    public static class PlayerViewSystem
    {
        private static readonly int MOVE_X_PARAMETER_HASH = Animator.StringToHash(MOVE_X_PARAMETER);
        private static readonly int MOVE_Z_PARAMETER_HASH = Animator.StringToHash(MOVE_Z_PARAMETER);
        private static readonly int FIRE_PARAMETER_HASH = Animator.StringToHash(FIRE_PARAMETER);
        private static readonly int IS_RELOADING_PARAMETER_HASH = Animator.StringToHash(IS_RELOADING_PARAMETER);
        private static readonly int IS_DEAD_PARAMETER_HASH = Animator.StringToHash(IS_DEAD_PARAMETER);

        // 플레이어가 바라보는 방향 기준 이동량. X는 오른쪽, Z는 앞쪽이 양수다.
        // 길이는 걷기 속도에서 1, 달리기 속도에서 2가 되도록 정규화한다.
        private const string MOVE_X_PARAMETER = "MoveX";
        private const string MOVE_Z_PARAMETER = "MoveZ";
        // 발사한 프레임에 켜는 Trigger
        private const string FIRE_PARAMETER = "Fire";
        // 재장전 중인지 여부(Bool). 재장전이 끝나거나 취소되면 false가 된다.
        private const string IS_RELOADING_PARAMETER = "IsReloading";
        // 사망해 쓰러져 있는지 여부(Bool). true가 되면 GoDown을 재생해 마지막 자세를 유지하고, 부활해 일어나기 시작하면 false가 되어 DownToUp을 재생한다.
        private const string IS_DEAD_PARAMETER = "IsDead";

        /// <summary>
        /// 플레이어 위치를 직전 틱과 현재 틱 사이로 보간하고 바라보는 방향으로 회전해 View에 반영한 뒤, 바라보는 방향 기준 이동량과 재장전·사망 여부를 Animator에 넘긴다.
        /// 이번 프레임에 발사 기록이 있으면 발사 애니메이션을 한 번 재생한다.
        /// </summary>
        /// <param name="player">표시할 플레이어.</param>
        /// <param name="playerFiredRecords">이번 프레임에 쌓인 플레이어 발사 기록.</param>
        /// <param name="interpolation">직전 틱에서 현재 틱까지 진행한 비율(0~1).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(in PlayerEntity player, List<PlayerFiredRecord> playerFiredRecords, float interpolation, ViewContext context)
        {
            PlayerView playerView = context.playerView;
            if (playerView == null || !player.isAlive)
            {
                return;
            }

            playerView.transform.position = Vector3.Lerp(player.previousPosition, player.currentPosition, interpolation);
            if (player.facingDirection.sqrMagnitude > 0f)
            {
                playerView.transform.rotation = Quaternion.LookRotation(player.facingDirection, Vector3.up);
            }

            if (playerView.animator != null)
            {
                Vector3 blendPosition = GetBlendPosition(in player);
                Vector3 localBlendPosition = playerView.transform.InverseTransformDirection(blendPosition);
                playerView.animator.SetFloat(MOVE_X_PARAMETER_HASH, localBlendPosition.x);
                playerView.animator.SetFloat(MOVE_Z_PARAMETER_HASH, localBlendPosition.z);
                playerView.animator.SetBool(IS_RELOADING_PARAMETER_HASH, player.isReloading);
                playerView.animator.SetBool(IS_DEAD_PARAMETER_HASH, player.isDead && !player.isGettingUp);
                if (playerFiredRecords.Count > 0)
                {
                    playerView.animator.SetTrigger(FIRE_PARAMETER_HASH);
                }
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
