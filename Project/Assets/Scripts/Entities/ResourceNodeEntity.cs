using Project06.Components;
using UnityEngine;

namespace Project06.Entities
{
    public struct ResourceNodeEntity
    {
        // 살아 있는지 여부. false인 슬롯은 빈 슬롯 목록에 들어 있다.
        public bool isAlive;
        // 이 슬롯이 재사용된 횟수
        public int generation;
        // 판 시작 시 씬에서 찾은 ResourceNodeAuthoring 목록의 인덱스. View가 씬에 놓인 모델을 찾을 때 쓴다.
        public int authoringIndex;
        // 자원 노드의 월드 위치
        public Vector3 position;
        // 자원 노드 수치
        public ResourceNodeComponent node;
        // 남은 체력
        public float health;
        // 지금까지 자원을 준 몫의 수(0~shareCount). 깎인 체력이 최대 체력을 shareCount로 나눈 만큼 넘을 때마다 하나씩 는다.
        public int harvestedShareCount;
        // 다 캤는지 여부. 체력이 0 이하가 되면 true가 된다. 슬롯은 살아 있지만 맞지 않으며 View가 숨긴다. 재생하면 false로 돌아간다.
        public bool isDepleted;
        // 다 캔 뒤 재생할 때까지 남은 아침의 수. 아침마다 1 줄고 0이 되면 재생한다. 0 이하로 다 캤으면 재생하지 않는다.
        public int regrowRemainingDays;
    }
}
