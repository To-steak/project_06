using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Project06.Entities
{
    public class EnemyStorage
    {
        // 적 슬롯 배열. 길이가 동시에 존재할 수 있는 적의 최대 수다.
        public EnemyEntity[] entities;
        // 비어 있는 슬롯 인덱스 목록
        public List<int> freeIndices;
        // 경로 계산에 돌려 쓰는 NavMesh 경로. 매 계산마다 할당하지 않으려고 하나만 둔다.
        public NavMeshPath navigationPath;

        // 적 하나가 담을 수 있는 경로 코너의 최대 수. 넘는 코너는 버리고 다음 경로 계산 때 이어 간다.
        public const int MAX_PATH_CORNER_COUNT = 32;

        /// <summary>
        /// 모든 슬롯이 빈 저장소를 만든다. 슬롯마다 경로 코너 배열을 미리 만들고, 공격 쿨다운 배열은 빈 배열로 둔다.
        /// </summary>
        /// <param name="capacity">동시에 존재할 수 있는 적의 최대 수.</param>
        public EnemyStorage(int capacity)
        {
            entities = new EnemyEntity[capacity];
            freeIndices = new List<int>(capacity);
            for (int index = capacity - 1; index >= 0; index--)
            {
                freeIndices.Add(index);
                entities[index].pathCorners = new Vector3[MAX_PATH_CORNER_COUNT];
                entities[index].attackCooldownRemainingTicks = System.Array.Empty<int>();
            }

            navigationPath = new NavMeshPath();
        }
    }
}
