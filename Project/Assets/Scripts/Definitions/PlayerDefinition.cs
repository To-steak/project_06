using Project06.Components;
using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "PlayerDefinition", menuName = "Project06/Definitions/Player")]
    public class PlayerDefinition : ScriptableObject
    {
        // 플레이어 생성 시 엔티티에 복사할 플레이어 수치
        public PlayerComponent player;
        // 플레이어 생성 시 엔티티에 복사할 이동 수치
        public PlayerMovementComponent movement;
        // 플레이어 생성 시 장착할 무기. DefinitionCatalog.weapons에 들어 있어야 한다.
        public WeaponDefinition startWeapon;
        // 조준 중 카메라를 옮기지 않는 화면 중앙 영역의 크기(0~1). 화면 중앙에서 가장자리까지를 1로 본 커서 거리가 이 안이면 카메라를 옮기지 않는다.
        [Range(0f, 1f)]
        public float aimCameraDeadZone;
        // 조준 중 커서를 화면 가장자리에 두었을 때 카메라가 플레이어에서 커서 쪽으로 옮겨 가는 수평 거리(m)
        public float aimCameraMaxDistance;
        // 카메라가 목표 위치를 따라가는 데 걸리는 대략적인 시간(초)
        public float aimCameraSmoothTime;
    }
}
