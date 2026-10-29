using Project06.Components;
using UnityEngine;

namespace Project06.Requests
{
    public struct ShelterSpawnRequest
    {
        // 쉘터가 생성될 월드 위치
        public Vector3 position;
        // 쉘터 수치
        public ShelterComponent shelter;
    }
}
