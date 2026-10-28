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
        /// 발사 대기 시간을 한 틱 줄이고, 발사 입력을 누르고 있으며 대기 시간이 끝났고 탄창에 탄이 있으면 한 발을 소비해 총구에서 바라보는 방향으로 투사체 생성을 요청한다.
        /// 바라보는 방향은 총구의 앞쪽 직선이 조준점을 지나도록 정해져 있으므로, 투사체는 조준점을 지난다.
        /// </summary>
        /// <param name="players">발사할 플레이어 저장소.</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐.</param>
        /// <param name="playerFiredRecords">이번 틱에 발사한 기록을 남길 목록.</param>
        public static void Tick(PlayerStorage players, Queue<ProjectileSpawnRequest> projectileSpawnRequests, List<PlayerFiredRecord> playerFiredRecords)
        {
            PlayerEntity[] entities = players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref PlayerEntity player = ref entities[index];
                if (!player.isAlive || player.weaponIndex < 0)
                {
                    continue;
                }

                Fire(ref player, projectileSpawnRequests, playerFiredRecords);
            }
        }

        private static void Fire(ref PlayerEntity player, Queue<ProjectileSpawnRequest> projectileSpawnRequests, List<PlayerFiredRecord> playerFiredRecords)
        {
            if (player.fireCooldownTicks > 0)
            {
                player.fireCooldownTicks--;
            }

            if (!player.isFiring || player.fireCooldownTicks > 0 || player.magazineAmmo <= 0)
            {
                return;
            }

            player.magazineAmmo--;

            Vector3 direction = player.facingDirection;

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
