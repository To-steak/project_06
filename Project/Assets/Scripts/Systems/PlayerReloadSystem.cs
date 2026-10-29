using Project06.Commands;
using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerReloadSystem
    {
        /// <summary>
        /// 재장전 입력이나 빈 탄창으로 발사하려는 입력이 있으면 재장전을 시작하고, 재장전 중이면 취소 입력을 처리하거나 남은 시간을 한 틱 줄인다.
        /// 재장전이 끝나는 틱에만 탄창을 채운다. 탄창에 남아 있던 탄은 예비 탄약으로 돌려보낸 뒤 탄창 용량만큼 다시 꺼낸다.
        /// CommandSystem보다 먼저 호출해야 같은 틱에 바뀐 재장전 여부로 달리기·조준·발사를 정한다.
        /// </summary>
        /// <param name="reloadCommand">이번 틱에 적용할 재장전 명령.</param>
        /// <param name="fireCommand">이번 틱의 발사 명령. 빈 탄창 자동 재장전 판단에 쓴다.</param>
        /// <param name="players">재장전할 플레이어 저장소. 살아 있고 사망 중이 아닌 플레이어 모두에게 같은 명령을 적용한다.</param>
        public static void Tick(in ReloadCommand reloadCommand, in FireCommand fireCommand, PlayerStorage players)
        {
            PlayerEntity[] entities = players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref PlayerEntity player = ref entities[index];
                if (!player.isAlive || player.isDead || player.weaponIndex < 0)
                {
                    continue;
                }

                Reload(in reloadCommand, in fireCommand, ref player);
            }
        }

        private static void Reload(in ReloadCommand reloadCommand, in FireCommand fireCommand, ref PlayerEntity player)
        {
            if (player.isReloading)
            {
                if (reloadCommand.cancel)
                {
                    player.isReloading = false;
                    player.reloadRemainingTicks = 0;
                }
                else
                {
                    player.reloadRemainingTicks--;
                    if (player.reloadRemainingTicks <= 0)
                    {
                        CompleteReload(ref player);
                    }
                }
            }
            else
            {
                bool isFiringWithEmptyMagazine = fireCommand.fire && player.magazineAmmo <= 0;
                if ((reloadCommand.reload || isFiringWithEmptyMagazine) && CanReload(in player))
                {
                    player.isReloading = true;
                    player.reloadRemainingTicks = Mathf.Max(1, player.weapon.reloadTicks);
                }
            }
        }

        // 탄창이 가득 차 있지 않고, 장착한 무기의 구경에 예비 탄약이 남아 있으면 재장전할 수 있다.
        private static bool CanReload(in PlayerEntity player)
        {
            if (player.magazineAmmo >= player.weapon.magazineCapacity)
            {
                return false;
            }

            return GetReserveAmmo(in player) > 0;
        }

        private static void CompleteReload(ref PlayerEntity player)
        {
            player.isReloading = false;
            player.reloadRemainingTicks = 0;

            if (player.reserveAmmo == null || player.caliberIndex < 0 || player.caliberIndex >= player.reserveAmmo.Length)
            {
                return;
            }

            int reserve = player.reserveAmmo[player.caliberIndex] + player.magazineAmmo;
            int loaded = Mathf.Min(player.weapon.magazineCapacity, reserve);
            player.magazineAmmo = loaded;
            player.reserveAmmo[player.caliberIndex] = reserve - loaded;
        }

        private static int GetReserveAmmo(in PlayerEntity player)
        {
            if (player.reserveAmmo == null || player.caliberIndex < 0 || player.caliberIndex >= player.reserveAmmo.Length)
            {
                return 0;
            }

            return player.reserveAmmo[player.caliberIndex];
        }
    }
}
