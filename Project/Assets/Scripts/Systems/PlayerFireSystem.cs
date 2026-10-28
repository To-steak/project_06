using System.Collections.Generic;
using Project06.Entities;
using Project06.Records;
using Project06.Requests;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerFireSystem
    {
        /// <summary>
        /// 발사 대기 시간을 한 틱 줄이고, 발사 입력을 누르고 있으며 대기 시간이 끝났으면 총구에서 겨냥한 방향으로 투사체 생성을 요청한다.
        /// </summary>
        /// <param name="player">발사할 플레이어.</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐.</param>
        /// <param name="playerFiredRecords">이번 틱에 발사한 기록을 남길 목록.</param>
        public static void Tick(ref PlayerEntity player, Queue<ProjectileSpawnRequest> projectileSpawnRequests, List<PlayerFiredRecord> playerFiredRecords)
        {
            if (!player.isAlive || player.weaponIndex < 0)
            {
                return;
            }

            if (player.fireCooldownTicks > 0)
            {
                player.fireCooldownTicks--;
            }

            if (!player.isFiring || player.fireCooldownTicks > 0)
            {
                return;
            }

            Vector3 direction = player.aimDirection;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0f ? direction.normalized : player.facingDirection;

            projectileSpawnRequests.Enqueue(new ProjectileSpawnRequest
            {
                weaponIndex = player.weaponIndex,
                position = player.muzzlePosition,
                velocity = direction * player.weapon.projectileSpeed,
                range = player.weapon.projectileRange,
                damage = player.weapon.damage,
            });
            playerFiredRecords.Add(new PlayerFiredRecord
            {
                weaponIndex = player.weaponIndex,
                position = player.muzzlePosition,
                direction = direction,
            });

            player.fireCooldownTicks = Mathf.Max(1, player.weapon.fireIntervalTicks);
        }
    }
}
