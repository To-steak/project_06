using Project06.Components;
using UnityEngine;

namespace Project06.Requests
{
    public struct ResourceNodeSpawnRequest
    {
        // 자원 노드가 생성될 월드 위치
        public Vector3 position;
        // 판 시작 시 씬에서 찾은 ResourceNodeAuthoring 목록의 인덱스
        public int authoringIndex;
        // 자원 노드 수치
        public ResourceNodeComponent node;
    }
}
