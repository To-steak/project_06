using Project06.Commands;
using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class CommandSystem
    {
        /// <summary>
        /// 이동 명령을 플레이어의 이동 방향과 달리기 여부에, 바라보기 명령을 바라보는 방향에, 조준·발사 명령을 조준·발사 상태에 반영한다.
        /// 바라보는 방향은 총구의 앞쪽 직선이 조준점을 지나도록 정하고, 조준점은 플레이어에 기록한다.
        /// 조준 입력을 누르고 있어야 발사하고, 발사 중에는 달리지 못하며, 달리면 조준이 풀린다.
        /// 재장전 중에는 달리기·조준·발사를 모두 막는다.
        /// </summary>
        /// <param name="moveCommand">이번 틱에 적용할 이동 명령.</param>
        /// <param name="lookCommand">이번 틱에 적용할 바라보기 명령.</param>
        /// <param name="aimCommand">이번 틱에 적용할 조준 명령.</param>
        /// <param name="fireCommand">이번 틱에 적용할 발사 명령.</param>
        /// <param name="players">명령을 받을 플레이어 저장소. 살아 있는 플레이어 모두에게 같은 명령을 적용한다.</param>
        public static void Tick(in MoveCommand moveCommand, in LookCommand lookCommand, in AimCommand aimCommand, in FireCommand fireCommand, PlayerStorage players)
        {
            PlayerEntity[] entities = players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref PlayerEntity player = ref entities[index];
                if (!player.isAlive)
                {
                    continue;
                }

                ApplyCommands(in moveCommand, in lookCommand, in aimCommand, in fireCommand, ref player);
            }
        }

        private static void ApplyCommands(in MoveCommand moveCommand, in LookCommand lookCommand, in AimCommand aimCommand, in FireCommand fireCommand, ref PlayerEntity player)
        {
            Vector3 direction = moveCommand.direction;
            direction.y = 0f;
            player.moveDirection = Vector3.ClampMagnitude(direction, 1f);

            player.isFiring = fireCommand.fire && aimCommand.aim && !player.isReloading;
            player.isRunning = moveCommand.run && !player.isFiring && !player.isReloading && player.moveDirection.sqrMagnitude > 0f;
            player.isAiming = aimCommand.aim && !player.isRunning && !player.isReloading;

            if (lookCommand.hasTarget)
            {
                player.aimPosition = lookCommand.targetPosition;
                FaceMuzzleLineToTarget(in lookCommand, ref player);
            }

            player.muzzlePosition = fireCommand.origin;
        }

        // 총구에서 플레이어 앞쪽으로 뻗은 직선이 조준점을 지나도록 바라보는 방향을 정한다.
        // 총구가 플레이어 중심에서 옆으로 벗어나 있으면 그만큼 조준점보다 안쪽으로 돈다. 조준점이 그 옆 간격보다 가까우면 방향을 유지한다.
        private static void FaceMuzzleLineToTarget(in LookCommand lookCommand, ref PlayerEntity player)
        {
            Vector3 toTarget = lookCommand.targetPosition - player.currentPosition;
            toTarget.y = 0f;
            float lateralOffset = lookCommand.muzzleOffset.x;
            float distanceSquared = toTarget.sqrMagnitude;
            if (distanceSquared <= lateralOffset * lateralOffset || distanceSquared <= 0f)
            {
                return;
            }

            float targetAngle = Mathf.Atan2(toTarget.x, toTarget.z);
            float offsetAngle = Mathf.Asin(lateralOffset / Mathf.Sqrt(distanceSquared));
            float facingAngle = targetAngle - offsetAngle;
            player.facingDirection = new Vector3(Mathf.Sin(facingAngle), 0f, Mathf.Cos(facingAngle));
        }
    }
}
