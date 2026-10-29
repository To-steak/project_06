using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerSanitySystem
    {
        /// <summary>
        /// 쉘터에서 떨어진 거리에 따라 플레이어의 정신력을 줄이거나 회복하고, 정신력이 0인 플레이어의 체력을 줄인다.
        /// 파괴되지 않은 쉘터 중 안전 반지름을 가장 적게 넘어선 쉘터를 기준으로 한다. 넘어선 거리에 비례해 줄고, 안전 반지름 안이면 회복된다.
        /// 기준이 될 쉘터가 없으면 정신력은 그대로다. 사망 중인 플레이어는 건너뛴다.
        /// 플레이어가 움직이는 PlayerMoveSystem 다음, 체력을 보고 사망을 정하는 PlayerDeathSystem 앞에 호출한다.
        /// </summary>
        /// <param name="players">정신력과 체력을 바꿀 플레이어 저장소.</param>
        /// <param name="shelters">거리를 잴 쉘터 저장소.</param>
        public static void Tick(PlayerStorage players, ShelterStorage shelters)
        {
            PlayerEntity[] entities = players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref PlayerEntity player = ref entities[index];
                if (!player.isAlive || player.isDead)
                {
                    continue;
                }

                // 정신력이 이미 0이었던 틱부터 체력을 줄인다. 이번 틱에 0이 되면 다음 틱부터 줄어든다.
                if (player.mentality <= 0f)
                {
                    player.health -= player.player.zeroMentalityHealthLossPerSecond * Time.fixedDeltaTime;
                }

                int shelterIndex = FindNearestShelterIndex(shelters, player.currentPosition, out float excessDistance);
                if (shelterIndex < 0)
                {
                    continue;
                }

                ref ShelterEntity shelter = ref shelters.entities[shelterIndex];
                float mentalityDelta = excessDistance > 0f
                    ? -excessDistance * shelter.shelter.mentalityLossPerSecondPerMeter * Time.fixedDeltaTime
                    : shelter.shelter.mentalityRecoveryPerSecond * Time.fixedDeltaTime;
                player.mentality = Mathf.Clamp(player.mentality + mentalityDelta, 0f, player.player.maxMentality);
            }
        }

        // 파괴되지 않은 쉘터 중 안전 반지름을 가장 적게 넘어선 쉘터의 인덱스를 구한다. 없으면 -1이다.
        // excessDistance는 안전 반지름을 넘어선 XZ 평면의 거리(m)이며, 안전 반지름 안이면 0이다.
        private static int FindNearestShelterIndex(ShelterStorage shelters, Vector3 position, out float excessDistance)
        {
            int nearestIndex = -1;
            excessDistance = 0f;
            ShelterEntity[] entities = shelters.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref ShelterEntity shelter = ref entities[index];
                if (!shelter.isAlive || shelter.isDestroyed)
                {
                    continue;
                }

                Vector3 offset = position - shelter.position;
                offset.y = 0f;
                float safeRadius = Mathf.Max(0f, shelter.shelter.mentalitySafeRadius);
                float sqrDistance = offset.sqrMagnitude;
                float candidateExcessDistance = sqrDistance <= safeRadius * safeRadius
                    ? 0f
                    : Mathf.Sqrt(sqrDistance) - safeRadius;
                if (nearestIndex < 0 || candidateExcessDistance < excessDistance)
                {
                    nearestIndex = index;
                    excessDistance = candidateExcessDistance;
                }
            }

            return nearestIndex;
        }
    }
}
