using Project06.Entities;
using Project06.Presentation.Player.Views;
using UnityEngine;

namespace Project06.Presentation.Camera.ViewSystems
{
    public static class CameraViewSystem
    {
        /// <summary>
        /// 조준 카메라 수치와 메인 카메라를 받아 두고, 플레이어 View 기준 카메라 피벗의 기본 위치를 기록한다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="mainCamera">씬의 메인 카메라. 없으면 카메라를 옮기지 않는다.</param>
        /// <param name="aimCameraDeadZone">조준 중 카메라를 옮기지 않는 화면 중앙 영역의 크기(0~1).</param>
        /// <param name="aimCameraMaxDistance">조준 중 커서를 화면 가장자리에 두었을 때 카메라가 옮겨 가는 수평 거리(m).</param>
        /// <param name="aimCameraSmoothTime">카메라가 목표 위치를 따라가는 데 걸리는 대략적인 시간(초).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(UnityEngine.Camera mainCamera, float aimCameraDeadZone, float aimCameraMaxDistance, float aimCameraSmoothTime, ViewContext context)
        {
            context.mainCamera = mainCamera;
            context.aimCameraDeadZone = aimCameraDeadZone;
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
        /// 조준 중이면 커서가 화면 중앙에서 떨어진 정도(가장자리 = 1)로 카메라 피벗을 커서 쪽으로 수평 이동시키고, 조준 중이 아니면 기본 위치로 되돌린다.
        /// 데드존 안에서는 옮기지 않고, 데드존 경계에서 0, 화면 가장자리에서 최대 거리가 되도록 옮긴다. 화면 기준이라 커서를 어느 쪽 가장자리에 두어도 같은 거리만큼 옮긴다.
        /// 옮기는 양은 부드럽게 따라간다. PlayerViewSystem.Present 다음에 호출한다.
        /// </summary>
        /// <param name="player">카메라가 따라갈 플레이어.</param>
        /// <param name="cursorScreenPosition">커서의 화면 좌표(픽셀).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(in PlayerEntity player, Vector2 cursorScreenPosition, ViewContext context)
        {
            PlayerView playerView = context.playerView;
            if (playerView == null || playerView.cameraPivot == null || !player.isAlive)
            {
                return;
            }

            Vector3 targetOffset = player.isAiming ? GetAimOffset(cursorScreenPosition, context) : Vector3.zero;
            context.cameraAimOffset = Vector3.SmoothDamp(context.cameraAimOffset, targetOffset, ref context.cameraAimOffsetVelocity, context.aimCameraSmoothTime);
            // 피벗은 플레이어의 자식이라 회전을 따라가므로, 기본 위치는 플레이어 기준으로 구하고 오프셋은 월드 기준으로 더한다.
            playerView.cameraPivot.position = playerView.transform.TransformPoint(context.cameraPivotLocalPosition) + context.cameraAimOffset;
        }

        // 커서의 화면 중앙 기준 위치를 -1~1로 정규화하고 데드존을 뺀 비율만큼, 카메라의 오른쪽·앞쪽을 바닥에 눕힌 방향으로 옮길 월드 오프셋을 구한다.
        private static Vector3 GetAimOffset(Vector2 cursorScreenPosition, ViewContext context)
        {
            UnityEngine.Camera mainCamera = context.mainCamera;
            if (mainCamera == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return Vector3.zero;
            }

            Vector2 screenOffset = new Vector2(
                cursorScreenPosition.x / Screen.width * 2f - 1f,
                cursorScreenPosition.y / Screen.height * 2f - 1f);
            screenOffset = Vector2.ClampMagnitude(screenOffset, 1f);
            float screenDistance = screenOffset.magnitude;
            float deadZone = context.aimCameraDeadZone;
            if (screenDistance <= deadZone || deadZone >= 1f)
            {
                return Vector3.zero;
            }

            float ratio = (screenDistance - deadZone) / (1f - deadZone);
            Vector2 screenDirection = screenOffset / screenDistance;

            Transform cameraTransform = mainCamera.transform;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
            // 카메라가 바로 아래를 보면 앞쪽이 바닥에 눕지 않으므로 화면 위쪽을 쓴다.
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up);
            }

            forward.Normalize();
            return (right * screenDirection.x + forward * screenDirection.y) * (ratio * context.aimCameraMaxDistance);
        }
    }
}
