using System.Collections.Generic;
using Project06.Presentation.Player.Views;
using Project06.Presentation.Weapon.Views;
using UnityEngine;
using UnityEngine.Pool;

namespace Project06.Presentation
{
    public class ViewContext
    {
        // 씬에 배치된 플레이어 View
        public PlayerView playerView;
        // 무기별 총 프리팹. DefinitionCatalog.weapons와 같은 순서이며, 프리팹이 없는 무기는 null이다.
        public GameObject[] weaponPrefabs;
        // 무기별로 한 번 생성해 둔 총 View. 아직 장착한 적 없는 무기는 null이다.
        public WeaponView[] weaponViews;
        // 지금 손에 표시 중인 총의 무기 인덱스. 음수면 표시 중인 총이 없다.
        public int equippedWeaponIndex = -1;
        // 지금 손에 표시 중인 총 View. 없으면 null이다.
        public WeaponView equippedWeaponView;
        // 아직 내보내지 않은 탄피의 배출 예정 시각(Time.time 기준 초). 장착한 총이 바뀌면 비운다.
        public List<float> pendingShellEjectionTimes = new List<float>();
        // 장전음을 마지막으로 처리한 장전 경과 틱. 장전 중이 아니면 -1이다.
        public int presentedReloadElapsedTicks = -1;
        // 무기별 투사체 View 풀. DefinitionCatalog.weapons와 같은 순서이며, 프리팹이 없는 무기는 null이다.
        public ObjectPool<GameObject>[] projectilePools;
        // 투사체 저장소 슬롯별로 표시 중인 View. 표시 중이 아니면 null이다.
        public GameObject[] projectileViews;
        // 투사체 저장소 슬롯별로 표시 중인 View가 가리키는 엔티티의 generation
        public int[] projectileViewGenerations;
        // 투사체 저장소 슬롯별로 표시 중인 View를 꺼낸 풀의 인덱스
        public int[] projectileViewPoolIndices;
        // 종류별 적 View 풀. 적 종류 인덱스와 같은 순서이며, 프리팹이 없는 종류는 null이다.
        public ObjectPool<GameObject>[] enemyPools;
        // 적 저장소 슬롯별로 표시 중인 View. 표시 중이 아니면 null이다.
        public GameObject[] enemyViews;
        // 적 저장소 슬롯별로 표시 중인 View가 가리키는 엔티티의 generation
        public int[] enemyViewGenerations;
        // 적 저장소 슬롯별로 표시 중인 View를 꺼낸 풀의 인덱스
        public int[] enemyViewPoolIndices;
    }
}
