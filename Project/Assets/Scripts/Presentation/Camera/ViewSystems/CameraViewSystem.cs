using Project06.Entities;
using Project06.Presentation.Camera.Views;
using Project06.Presentation.Player.Views;
using Unity.Cinemachine;
using UnityEngine;

namespace Project06.Presentation.Camera.ViewSystems
{
    public static class CameraViewSystem
    {
        /// <summary>
        /// 조준 카메라 수치와 메인 카메라를 받아 두고, 플레이어 View 기준 카메라 피벗의 기본 위치와 씬에 설정된 Camera Distance를 기록한다.
        /// Cinemachine 카메라가 플레이어 View의 카메라 피벗을 따라가게 한다. 판 시작 시 플레이어 View를 만든 다음 한 번 호출한다.
        /// </summary>
        /// <param name="mainCamera">씬의 메인 카메라. 없으면 카메라를 옮기지 않는다.</param>
        /// <param name="aimCameraDeadZone">조준 중 카메라를 옮기지 않는 화면 중앙 영역의 크기(0~1).</param>
        /// <param name="aimCameraMaxDistance">조준 중 커서를 화면 가장자리에 두었을 때 카메라가 옮겨 가는 수평 거리(m).</param>
        /// <param name="aimCameraDistance">조준 중 Camera Distance(m). 0 이하면 바꾸지 않는다.</param>
        /// <param name="aimCameraSmoothTime">카메라가 목표 위치와 거리를 따라가는 데 걸리는 대략적인 시간(초).</param>
        /// <param name="context">View 실행 상태. cameraView와 playerView를 먼저 채워 두어야 카메라가 플레이어를 따라가고 Camera Distance를 바꾼다.</param>
        public static void Initialize(UnityEngine.Camera mainCamera, float aimCameraDeadZone, float aimCameraMaxDistance, float aimCameraDistance, float aimCameraSmoothTime, ViewContext context)
        {
            context.mainCamera = mainCamera;
            context.aimCameraDeadZone = aimCameraDeadZone;
            context.aimCameraMaxDistance = aimCameraMaxDistance;
            context.aimCameraDistance = aimCameraDistance;
            context.aimCameraSmoothTime = aimCameraSmoothTime;
            context.cameraAimOffset = Vector3.zero;
            context.cameraAimOffsetVelocity = Vector3.zero;
            context.cameraDistanceVelocity = 0f;

            CameraView cameraView = context.cameraView;
            if (cameraView != null && cameraView.positionComposer != null)
            {
                context.defaultCameraDistance = cameraView.positionComposer.CameraDistance;
            }

            PlayerView playerView = context.playerView;
            context.cameraPivotLocalPosition = playerView != null && playerView.cameraPivot != null
                ? playerView.transform.InverseTransformPoint(playerView.cameraPivot.position)
                : Vector3.zero;

            // 플레이어 View는 판 시작 시 코드가 만들므로 씬에서 연결해 둘 수 없다. 여기서 따라갈 대상으로 넣고, 이전 위치에서 끌려오지 않도록 바로 맞춘다.
            if (cameraView != null && cameraView.cinemachineCamera != null && playerView != null && playerView.cameraPivot != null)
            {
                cameraView.cinemachineCamera.Target.TrackingTarget = playerView.cameraPivot;
                cameraView.cinemachineCamera.PreviousStateIsValid = false;
            }
        }

        /// <summary>
        /// 조준 중이면 커서가 화면 중앙에서 떨어진 정도(가장자리 = 1)로 카메라 피벗을 커서 쪽으로 수평 이동시키고, 조준 중이 아니면 기본 위치로 되돌린다.
        /// 데드존 안에서는 옮기지 않고, 데드존 경계에서 0, 화면 가장자리에서 최대 거리가 되도록 옮긴다. 화면 기준이라 커서를 어느 쪽 가장자리에 두어도 같은 거리만큼 옮긴다.
        /// 조준 중에는 Camera Distance를 조준용 거리로 줄이고, 조준 중이 아니면 씬에 설정되어 있던 거리로 되돌린다.
        /// 옮기는 양과 거리는 부드럽게 따라간다. PlayerViewSystem.Present 다음에 호출한다.
        /// </summary>
        /// <param name="player">카메라가 따라갈 플레이어.</param>
        /// <param name="cursorScreenPosition">커서의 화면 좌표(픽셀).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(in PlayerEntity player, Vector2 cursorScreenPosition, ViewContext context)
        {
            if (!player.isAlive)
            {
                return;
            }

            PresentPivot(in player, cursorScreenPosition, context);
            PresentDistance(in player, context);
        }

        private static void PresentPivot(in PlayerEntity player, Vector2 cursorScreenPosition, ViewContext context)
        {
            PlayerView playerView = context.playerView;
            if (playerView == null || playerView.cameraPivot == null)
            {
                return;
            }

            Vector3 targetOffset = player.isAiming ? GetAimOffset(cursorScreenPosition, context) : Vector3.zero;
            context.cameraAimOffset = Vector3.SmoothDamp(context.cameraAimOffset, targetOffset, ref context.cameraAimOffsetVelocity, context.aimCameraSmoothTime);
            // 피벗은 플레이어의 자식이라 회전을 따라가므로, 기본 위치는 플레이어 기준으로 구하고 오프셋은 월드 기준으로 더한다.
            playerView.cameraPivot.position = playerView.transform.TransformPoint(context.cameraPivotLocalPosition) + context.cameraAimOffset;
        }

        private static void PresentDistance(in PlayerEntity player, ViewContext context)
        {
            CameraView cameraView = context.cameraView;
            if (cameraView == null || cameraView.positionComposer == null || context.aimCameraDistance <= 0f)
            {
                return;
            }

            CinemachinePositionComposer positionComposer = cameraView.positionComposer;
            float targetDistance = player.isAiming ? context.aimCameraDistance : context.defaultCameraDistance;
            positionComposer.CameraDistance = Mathf.SmoothDamp(positionComposer.CameraDistance, targetDistance, ref context.cameraDistanceVelocity, context.aimCameraSmoothTime);
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
