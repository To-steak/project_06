#if UNITY_EDITOR
using Project06.Components;
using Project06.Entities;
using Project06.Systems;
using UnityEditor;
using UnityEngine;

namespace Project06.Core
{
    // 씬 뷰 디버그 기즈모. 에디터에서만 컴파일된다.
    public partial class Runner
    {
        // 적 디버그 기즈모 색
        private static readonly Color ENEMY_DETECTION_RADIUS_COLOR = new Color(1f, 0.9f, 0.2f, 1f);
        private static readonly Color ENEMY_LOSE_RADIUS_COLOR = new Color(0.7f, 0.4f, 1f, 1f);
        private static readonly Color ENEMY_STOP_DISTANCE_COLOR = new Color(0.3f, 1f, 0.4f, 1f);
        private static readonly Color ENEMY_PATH_COLOR = new Color(0.2f, 0.8f, 1f, 1f);
        private static readonly Color ENEMY_MELEE_RANGE_COLOR = new Color(1f, 0f, 1f, 1f);
        private static readonly Color ENEMY_RANGED_RANGE_COLOR = new Color(1f, 0.5f, 0.1f, 1f);
        private static readonly Color ENEMY_MELEE_HIT_WINDUP_COLOR = new Color(0f, 1f, 0f, 1f);
        private static readonly Color ENEMY_MELEE_HIT_ACTIVE_COLOR = new Color(1f, 0f, 0f, 1f);
        // 플레이어 근접 판정 기즈모 색
        private static readonly Color PLAYER_MELEE_HIT_WINDUP_COLOR = new Color(0f, 1f, 0f, 1f);
        private static readonly Color PLAYER_MELEE_HIT_ACTIVE_COLOR = new Color(1f, 0f, 0f, 1f);
        // 피격 범위(hitRadius) 기즈모 색. 적과 플레이어가 같이 쓴다.
        private static readonly Color HIT_RADIUS_COLOR = new Color(1f, 1f, 1f, 1f);

        // 디버그용으로 살아 있는 플레이어와 사망 중이 아닌 적의 피격 범위를 그린다. 플레이어가 근접 공격 중이면 판정 상자도 그린다. 쉘터는 움직이지 않으므로 ShelterAuthoringGizmo가 그린다.
        // 적은 자기 위치를 중심으로 판정 원을 XZ 평면에 그리고, 남은 NavMesh 경로를 선으로 그린다.
        // 감지 원은 항상 그린다. 추격·공격 중이면 정지 원과 공격별 사거리 원을 그리고, 목표가 플레이어면 해제 원도 그린다.
        private void OnDrawGizmos()
        {
            if (_world == null)
            {
                return;
            }

            PlayerEntity[] players = _world.players.entities;
            for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
            {
                ref PlayerEntity player = ref players[playerIndex];
                if (player.isAlive && !player.isDead)
                {
                    Handles.color = HIT_RADIUS_COLOR;
                    Handles.DrawWireDisc(player.currentPosition, Vector3.up, player.player.hitRadius);
                    DrawPlayerMeleeHit(in player);
                }
            }

            EnemyEntity[] enemies = _world.enemies.entities;
            for (int index = 0; index < enemies.Length; index++)
            {
                ref EnemyEntity enemy = ref enemies[index];
                if (!enemy.isAlive || enemy.state == EnemyState.Dead)
                {
                    continue;
                }

                Vector3 position = enemy.currentPosition;
                Handles.color = HIT_RADIUS_COLOR;
                Handles.DrawWireDisc(position, Vector3.up, enemy.enemy.hitRadius);
                Handles.color = ENEMY_DETECTION_RADIUS_COLOR;
                Handles.DrawWireDisc(position, Vector3.up, enemy.enemy.detectionRadius);
                if (enemy.state == EnemyState.Chase || enemy.state == EnemyState.Attack)
                {
                    if (enemy.targetKind == EnemyTargetKind.Player)
                    {
                        Handles.color = ENEMY_LOSE_RADIUS_COLOR;
                        Handles.DrawWireDisc(position, Vector3.up, enemy.enemy.loseRadius);
                    }

                    Handles.color = ENEMY_STOP_DISTANCE_COLOR;
                    Handles.DrawWireDisc(position, Vector3.up, enemy.enemy.stopDistance);
                    DrawAttackRanges(in enemy);
                    DrawMeleeHit(in enemy);
                }

                Handles.color = ENEMY_PATH_COLOR;
                Vector3 lineStart = position;
                for (int cornerIndex = enemy.pathCornerIndex; cornerIndex < enemy.pathCornerCount; cornerIndex++)
                {
                    Vector3 corner = enemy.pathCorners[cornerIndex];
                    Handles.DrawLine(lineStart, corner);
                    lineStart = corner;
                }
            }
        }

        // 플레이어 근접 공격의 선딜레이와 타격 구간 동안 판정 상자를 그린다. 선딜레이는 초록, 타격 구간은 빨강으로 그린다.
        private static void DrawPlayerMeleeHit(in PlayerEntity player)
        {
            if (player.meleePhase != PlayerMeleePhase.Windup && player.meleePhase != PlayerMeleePhase.Active)
            {
                return;
            }

            PlayerMeleeSystem.GetHitBox(in player, out Vector3 center, out Vector3 forward, out float halfLength, out float halfWidth);
            Vector3 lengthAxis = forward * halfLength;
            Vector3 widthAxis = Vector3.Cross(Vector3.up, forward) * halfWidth;
            Handles.color = player.meleePhase == PlayerMeleePhase.Active ? PLAYER_MELEE_HIT_ACTIVE_COLOR : PLAYER_MELEE_HIT_WINDUP_COLOR;
            Handles.DrawPolyLine(
                center + lengthAxis + widthAxis,
                center + lengthAxis - widthAxis,
                center - lengthAxis - widthAxis,
                center - lengthAxis + widthAxis,
                center + lengthAxis + widthAxis);
        }

        // 근접 공격의 선딜레이와 타격 구간 동안 판정 모양을 그린다. 선딜레이는 초록, 타격 구간은 빨강으로 그린다.
        private static void DrawMeleeHit(in EnemyEntity enemy)
        {
            if (enemy.state != EnemyState.Attack || enemy.attackPhase == EnemyAttackPhase.Recovery)
            {
                return;
            }

            EnemyAttackComponent attack = enemy.enemy.attacks[enemy.attackIndex];
            if (attack.kind != EnemyAttackKind.Melee)
            {
                return;
            }

            float radius = EnemyAttackSystem.GetMeleeHitSegment(in enemy, in attack.meleeHit, out Vector3 start, out Vector3 end);
            Handles.color = enemy.attackPhase == EnemyAttackPhase.Active ? ENEMY_MELEE_HIT_ACTIVE_COLOR : ENEMY_MELEE_HIT_WINDUP_COLOR;
            Handles.DrawWireDisc(start, Vector3.up, radius);
            if (start == end)
            {
                return;
            }

            Handles.DrawWireDisc(end, Vector3.up, radius);
            Vector3 side = Vector3.Cross(Vector3.up, enemy.facingDirection) * radius;
            Handles.DrawLine(start + side, end + side);
            Handles.DrawLine(start - side, end - side);
        }

        // 공격마다 사거리 원을 그린다. 근접은 분홍, 원거리는 주황이다.
        private static void DrawAttackRanges(in EnemyEntity enemy)
        {
            EnemyAttackComponent[] attacks = enemy.enemy.attacks;
            if (attacks == null)
            {
                return;
            }

            for (int attackIndex = 0; attackIndex < attacks.Length; attackIndex++)
            {
                Handles.color = attacks[attackIndex].kind == EnemyAttackKind.Melee ? ENEMY_MELEE_RANGE_COLOR : ENEMY_RANGED_RANGE_COLOR;
                Handles.DrawWireDisc(enemy.currentPosition, Vector3.up, attacks[attackIndex].range);
            }
        }
    }
}
#endif
