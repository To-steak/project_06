using Project06.Presentation.Player.Views;
using UnityEngine;
using UnityEngine.Pool;

namespace Project06.Presentation
{
    public class ViewContext
    {
        // 씬에 배치된 플레이어 View
        public PlayerView playerView;
        // 무기별 투사체 View 풀. DefinitionCatalog.weapons와 같은 순서이며, 프리팹이 없는 무기는 null이다.
        public ObjectPool<GameObject>[] projectilePools;
        // 투사체 저장소 슬롯별로 표시 중인 View. 표시 중이 아니면 null이다.
        public GameObject[] projectileViews;
        // 투사체 저장소 슬롯별로 표시 중인 View가 가리키는 엔티티의 generation
        public int[] projectileViewGenerations;
        // 투사체 저장소 슬롯별로 표시 중인 View를 꺼낸 풀의 인덱스
        public int[] projectileViewPoolIndices;
    }
}
