using Project06.Entities;
using Project06.Presentation.Player.Views;
using UnityEngine;

namespace Project06.Presentation.Camera.ViewSystems
{
    public static class CameraViewSystem
    {
        /// <summary>
        /// 조준 카메라 수치를 받아 두고, 플레이어 View 기준 카메라 피벗의 기본 위치를 기록한다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="aimCameraWeight">조준 중 카메라가 조준점 쪽으로 옮겨 가는 비율(0~1).</param>
        /// <param name="aimCameraMaxDistance">조준 중 카메라가 옮겨 갈 수 있는 최대 수평 거리(m).</param>
        /// <param name="aimCameraSmoothTime">카메라가 목표 위치를 따라가는 데 걸리는 대략적인 시간(초).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(float aimCameraWeight, float aimCameraMaxDistance, float aimCameraSmoothTime, ViewContext context)
        {
            context.aimCameraWeight = aimCameraWeight;
            context.aimCameraMaxDistance = aimCameraMaxDistance;
            context.aimCameraSmoothTime = aimCameraSmoothTime;
            context.cameraAimOffset = Vector3.zero;
            context.cameraAimOffsetVelocity = Vector3.zero;

            PlayerView playerView = context.playerView;
            context.cameraPivotLocalPosition = playerView != null && playerView.cameraPivot != null
                ? playerView.transform.InverseTransformPoint(playerView.cameraPivot.position)
                : Vector3.zero;
        }

        /// <summary>
        /// 조준 중이면 카메라 피벗을 플레이어에서 조준점 쪽으로 비율만큼, 최대 거리 안에서 수평으로 옮기고, 조준 중이 아니면 기본 위치로 되돌린다.
        /// 옮기는 양은 부드럽게 따라간다. PlayerViewSystem.Present 다음에 호출한다.
        /// </summary>
        /// <param name="player">카메라가 따라갈 플레이어.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(in PlayerEntity player, ViewContext context)
        {
            PlayerView playerView = context.playerView;
            if (playerView == null || playerView.cameraPivot == null || !player.isAlive)
            {
                return;
            }

            Vector3 playerPosition = playerView.transform.position;
            Vector3 targetOffset = Vector3.zero;
            if (player.isAiming)
            {
                Vector3 toAim = player.aimPosition - playerPosition;
                toAim.y = 0f;
                targetOffset = Vector3.ClampMagnitude(toAim * context.aimCameraWeight, context.aimCameraMaxDistance);
            }

            context.cameraAimOffset = Vector3.SmoothDamp(context.cameraAimOffset, targetOffset, ref context.cameraAimOffsetVelocity, context.aimCameraSmoothTime);
            // 피벗은 플레이어의 자식이라 회전을 따라가므로, 기본 위치는 플레이어 기준으로 구하고 오프셋은 월드 기준으로 더한다.
            playerView.cameraPivot.position = playerView.transform.TransformPoint(context.cameraPivotLocalPosition) + context.cameraAimOffset;
        }
    }
}
