using Unity.Cinemachine;
using UnityEngine;

namespace Project06.Presentation.Camera.Views
{
    public class CameraView : MonoBehaviour
    {
        // 플레이어의 Camera Pivot을 따라가는 Cinemachine 카메라의 Position Composer. 조준 중 Camera Distance를 줄일 때 쓴다.
        public CinemachinePositionComposer positionComposer;
    }
}
