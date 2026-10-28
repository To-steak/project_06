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
        // 조준 중 카메라가 플레이어에서 조준점 쪽으로 옮겨 가는 비율(0~1). 0.5 이상이면 커서를 화면 끝에 둘 때 플레이어가 반대쪽 끝까지 밀려난다.
        [Range(0f, 1f)]
        public float aimCameraWeight;
        // 조준 중 카메라가 플레이어에서 옮겨 갈 수 있는 최대 수평 거리(m)
        public float aimCameraMaxDistance;
        // 카메라가 목표 위치를 따라가는 데 걸리는 대략적인 시간(초)
        public float aimCameraSmoothTime;
    }
}
