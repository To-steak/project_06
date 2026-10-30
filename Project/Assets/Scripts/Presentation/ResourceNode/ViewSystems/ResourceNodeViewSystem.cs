using System.Collections.Generic;
using Project06.Entities;
using Project06.Presentation.Audio.ViewSystems;
using Project06.Presentation.ResourceNode.Views;
using Project06.Records;
using UnityEngine;

namespace Project06.Presentation.ResourceNode.ViewSystems
{
    public static class ResourceNodeViewSystem
    {
        /// <summary>
        /// 씬에 놓인 자원 노드 View를 담아 둔다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="views">ResourceNodeAuthoring 순서의 자원 노드 View. 없는 칸은 null이어도 된다.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(ResourceNodeView[] views, ViewContext context)
        {
            context.resourceNodeViews = views;
        }

        /// <summary>
        /// 이번 프레임에 맞은 자원 노드마다 그 위치에서 타격음을 재생한다. 효과음 풀로 재생하므로 마지막 타격에 오브젝트가 꺼져도 소리는 끝까지 난다.
        /// 다 캔 자원 노드의 오브젝트는 끄고, 재생했거나 아직 캐는 중인 자원 노드의 오브젝트는 켠다.
        /// </summary>
        /// <param name="resourceNodes">표시할 자원 노드 저장소.</param>
        /// <param name="resourceNodeHitRecords">이번 프레임에 쌓인 자원 노드 피격 기록.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(ResourceNodeStorage resourceNodes, List<ResourceNodeHitRecord> resourceNodeHitRecords, ViewContext context)
        {
            ResourceNodeEntity[] entities = resourceNodes.entities;
            PlayHitSounds(entities, resourceNodeHitRecords, context);

            for (int index = 0; index < entities.Length; index++)
            {
                ref ResourceNodeEntity resourceNode = ref entities[index];
                ResourceNodeView view = resourceNode.isAlive ? GetView(in resourceNode, context) : null;
                if (view == null)
                {
                    continue;
                }

                bool isVisible = !resourceNode.isDepleted;
                if (view.gameObject.activeSelf != isVisible)
                {
                    view.gameObject.SetActive(isVisible);
                }
            }
        }

        // 맞은 자원 노드가 아직 그 엔티티이면 그 View의 타격음 중 하나를 골라 자원 노드 위치에서 재생한다.
        private static void PlayHitSounds(ResourceNodeEntity[] entities, List<ResourceNodeHitRecord> resourceNodeHitRecords, ViewContext context)
        {
            for (int recordIndex = 0; recordIndex < resourceNodeHitRecords.Count; recordIndex++)
            {
                EntityIdentifier identifier = resourceNodeHitRecords[recordIndex].node;
                ref ResourceNodeEntity resourceNode = ref entities[identifier.index];
                if (!resourceNode.isAlive || resourceNode.generation != identifier.generation)
                {
                    continue;
                }

                ResourceNodeView view = GetView(in resourceNode, context);
                if (view == null || view.hitClips == null || view.hitClips.Length == 0)
                {
                    continue;
                }

                AudioClip clip = view.hitClips[Random.Range(0, view.hitClips.Length)];
                SoundEffectViewSystem.Play(clip, resourceNode.position, context);
            }
        }

        // 자원 노드의 View를 찾는다. Authoring 인덱스가 범위를 벗어났거나 View가 없으면 null을 돌려준다.
        private static ResourceNodeView GetView(in ResourceNodeEntity resourceNode, ViewContext context)
        {
            ResourceNodeView[] views = context.resourceNodeViews;
            return views != null && resourceNode.authoringIndex >= 0 && resourceNode.authoringIndex < views.Length
                ? views[resourceNode.authoringIndex]
                : null;
        }
    }
}
