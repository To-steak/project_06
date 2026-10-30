using UnityEngine;

namespace Project06.Definitions
{
    [CreateAssetMenu(fileName = "DefinitionCatalog", menuName = "Project06/Definitions/Catalog")]
    public class DefinitionCatalog : ScriptableObject
    {
        // 낮밤 Definition
        public DayNightDefinition dayNight;
        // 하늘 조명 Definition
        public SkyDefinition sky;
        // 모든 무기 Definition. 엔티티는 무기를 이 배열의 인덱스로 가리킨다.
        public WeaponDefinition[] weapons;
        // 모든 구경 Definition. 엔티티는 구경을 이 배열의 인덱스로 가리킨다.
        public CaliberDefinition[] calibers;
        // 월드 위치에서 효과음을 재생할 때 복제할 AudioSource 프리팹. 3D 소리 설정(Spatial Blend, 감쇠 거리)을 이 프리팹에서 정한다. 비어 있으면 효과음을 재생하지 않는다.
        public AudioSource soundEffectPrefab;
    }
}
