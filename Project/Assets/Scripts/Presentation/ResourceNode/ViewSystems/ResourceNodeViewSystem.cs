using Project06.Entities;
using Project06.Presentation.ResourceNode.Views;
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
        /// 다 캔 자원 노드의 오브젝트는 끄고, 재생했거나 아직 캐는 중인 자원 노드의 오브젝트는 켠다.
        /// </summary>
        /// <param name="resourceNodes">표시할 자원 노드 저장소.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(ResourceNodeStorage resourceNodes, ViewContext context)
        {
            ResourceNodeEntity[] entities = resourceNodes.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref ResourceNodeEntity resourceNode = ref entities[index];
                if (!resourceNode.isAlive || resourceNode.authoringIndex < 0 || resourceNode.authoringIndex >= context.resourceNodeViews.Length)
                {
                    continue;
                }

                ResourceNodeView view = context.resourceNodeViews[resourceNode.authoringIndex];
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
    }
}
