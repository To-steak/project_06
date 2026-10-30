using System.Collections.Generic;
using Project06.Presentation.Camera.Views;
using Project06.Presentation.Enemy.Views;
using Project06.Presentation.Player.Views;
using Project06.Presentation.Pooling;
using Project06.Presentation.ResourceNode.Views;
using Project06.Presentation.Sky.Views;
using Project06.Presentation.Weapon.Views;
using UnityEngine;

namespace Project06.Presentation
{
    public class ViewContext
    {
        // 판 시작 시 플레이어 프리팹으로 만든 플레이어 View. 프리팹이 없으면 null이다.
        public PlayerView playerView;
        // 무기별 근접 공격 Trigger 해시. DefinitionCatalog.weapons와 같은 순서이며, 0이면 Trigger가 없다.
        public int[] meleeAttackTriggerHashes;
        // 무기별 무기 프리팹. DefinitionCatalog.weapons와 같은 순서이며, 프리팹이 없는 무기는 null이다.
        public GameObject[] weaponPrefabs;
        // 무기별로 한 번 생성해 둔 무기 View. 아직 장착한 적 없는 무기는 null이다.
        public WeaponView[] weaponViews;
        // 지금 손에 표시 중인 무기의 인덱스. 음수면 표시 중인 무기가 없다.
        public int equippedWeaponIndex = -1;
        // 지금 손에 표시 중인 무기 View. 없으면 null이다.
        public WeaponView equippedWeaponView;
        // 지금 손에 표시 중인 무기의 총 View. 총이 아니거나 무기가 없으면 null이다.
        public GunView equippedGunView;
        // 지금 손에 표시 중인 무기의 근접 무기 View. 근접 무기가 아니거나 무기가 없으면 null이다.
        public MeleeWeaponView equippedMeleeWeaponView;
        // 아직 내보내지 않은 탄피의 배출 예정 시각(Time.time 기준 초). 장착한 총이 바뀌면 비운다.
        public List<float> pendingShellEjectionTimes = new List<float>();
        // 장전음을 마지막으로 처리한 장전 경과 틱. 장전 중이 아니면 -1이다.
        public int presentedReloadElapsedTicks = -1;
        // 무기별 투사체 View 풀. DefinitionCatalog.weapons와 같은 순서이며, 프리팹이 없는 무기는 null이다.
        public ViewPool<GameObject>[] projectilePools;
        // 투사체 저장소 슬롯별로 표시 중인 View. 표시 중이 아니면 null이다.
        public GameObject[] projectileViews;
        // 투사체 저장소 슬롯별로 표시 중인 View가 가리키는 엔티티의 generation
        public int[] projectileViewGenerations;
        // 투사체 저장소 슬롯별로 표시 중인 View를 꺼낸 풀의 인덱스
        public int[] projectileViewPoolIndices;
        // 종류별 적 View 풀. 적 종류 인덱스와 같은 순서이며, 프리팹이 없는 종류는 null이다.
        public ViewPool<EnemyView>[] enemyPools;
        // 적 저장소 슬롯별로 표시 중인 View. 표시 중이 아니면 null이다.
        public EnemyView[] enemyViews;
        // 적 저장소 슬롯별로 표시 중인 View가 가리키는 엔티티의 generation
        public int[] enemyViewGenerations;
        // 적 저장소 슬롯별로 표시 중인 View를 꺼낸 풀의 인덱스
        public int[] enemyViewPoolIndices;
        // 종류별 공격 Trigger 해시. 적 종류 인덱스와 같은 순서이며, 안쪽은 공격 목록과 같은 순서다. 0이면 Trigger가 없다.
        public int[][] enemyAttackTriggerHashes;
        // 적 종류별 적 투사체 View 풀. 적 종류 인덱스와 같은 순서이며, 투사체 프리팹이 없는 종류는 null이다.
        public ViewPool<GameObject>[] enemyProjectilePools;
        // 적 투사체 저장소 슬롯별로 표시 중인 View. 표시 중이 아니면 null이다.
        public GameObject[] enemyProjectileViews;
        // 적 투사체 저장소 슬롯별로 표시 중인 View가 가리키는 엔티티의 generation
        public int[] enemyProjectileViewGenerations;
        // 적 투사체 저장소 슬롯별로 표시 중인 View를 꺼낸 풀의 인덱스
        public int[] enemyProjectileViewPoolIndices;
        // 씬에 놓인 자원 노드 View. ResourceNodeAuthoring 순서이며, View가 없는 칸은 null이다.
        public ResourceNodeView[] resourceNodeViews;
        // 씬의 메인 카메라. 커서의 화면 방향을 월드 수평 방향으로 바꿀 때 쓴다.
        // Project06.Presentation.Camera 네임스페이스와 이름이 겹치므로 전체 이름으로 쓴다.
        public UnityEngine.Camera mainCamera;
        // 씬에 배치된 카메라 View. 없으면 Camera Distance를 바꾸지 않는다.
        public CameraView cameraView;
        // 조준 중 Camera Distance(m). 0 이하면 바꾸지 않는다.
        public float aimCameraDistance;
        // 판 시작 시 씬에 설정되어 있던 Camera Distance(m). 조준하지 않을 때 이 값으로 돌아간다.
        public float defaultCameraDistance;
        // Camera Distance를 부드럽게 따라가게 할 때 쓰는 변화 속도
        public float cameraDistanceVelocity;
        // 조준 중 카메라를 옮기지 않는 화면 중앙 영역의 크기(0~1). 화면 중앙에서 가장자리까지를 1로 본다.
        public float aimCameraDeadZone;
        // 조준 중 커서를 화면 가장자리에 두었을 때 카메라가 옮겨 가는 수평 거리(m)
        public float aimCameraMaxDistance;
        // 카메라가 목표 위치를 따라가는 데 걸리는 대략적인 시간(초)
        public float aimCameraSmoothTime;
        // 판 시작 시 플레이어 View 기준 카메라 피벗의 로컬 위치
        public Vector3 cameraPivotLocalPosition;
        // 카메라 피벗을 기본 위치에서 옮겨 둔 현재 월드 오프셋
        public Vector3 cameraAimOffset;
        // cameraAimOffset을 부드럽게 따라가게 할 때 쓰는 변화 속도
        public Vector3 cameraAimOffsetVelocity;
        // 씬에 배치된 하늘 View. 없으면 해와 달을 움직이지 않는다.
        public SkyView skyView;
        // 해와 달이 뜨고 지는 수평 방향. 판 시작 시 씬에 놓인 해의 Y 회전이다.
        public Quaternion skyYawRotation;
        // 하늘 View의 해에 붙은 라이트. 없으면 해 빛을 바꾸지 않는다.
        public Light skySunLight;
        // 하늘 View의 달에 붙은 라이트. 없으면 달 빛을 바꾸지 않는다.
        public Light skyMoonLight;
        // 낮과 밤 사이 노을 구간의 길이(틱)
        public int skyTwilightTicks;
        // 어두운 정도(한낮 0, 노을 0.5, 한밤 1)에 따른 해 빛 색. null이면 바꾸지 않는다.
        public Gradient skySunColor;
        // 어두운 정도에 따른 해 빛 세기. null이면 바꾸지 않는다.
        public AnimationCurve skySunIntensity;
        // 달 빛 색
        public Color skyMoonColor;
        // 어두운 정도에 따른 달 빛 세기. null이면 달 빛 색과 세기를 바꾸지 않는다.
        public AnimationCurve skyMoonIntensity;
        // 어두운 정도에 따른 위쪽 환경광 색. null이면 바꾸지 않는다.
        public Gradient skyAmbientSkyColor;
        // 어두운 정도에 따른 옆쪽 환경광 색. null이면 바꾸지 않는다.
        public Gradient skyAmbientEquatorColor;
        // 어두운 정도에 따른 아래쪽 환경광 색. null이면 바꾸지 않는다.
        public Gradient skyAmbientGroundColor;
        // 어두운 정도에 따른 안개 색. null이면 바꾸지 않는다.
        public Gradient skyFogColor;
    }
}
