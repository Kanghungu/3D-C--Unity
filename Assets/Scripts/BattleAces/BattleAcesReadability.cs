using Game.UI;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// C축 가독성 — 코어 HP 위험 구간·미니맵 점 색/크기·(하단 HUD) 코어 문구 색을 한곳에서 고정.
    /// 표·체크리스트: <c>Assets/Docs/BATTLE_ACES_READABILITY.md</c> · 아트 톤: <see cref="BattleAcesArtDirection"/> · 승리 임박 사운드: <see cref="EnemyCoreVictoryImminentAudioNormalized"/>.
    /// </summary>
    public static class BattleAcesReadability
    {
        /// <summary>아군 지휘 코어 — 경고(미니맵·하단 HUD 강조 시작)</summary>
        public const float PlayerCoreWarningNormalized = 0.5f;

        /// <summary>아군 지휘 코어 — 위험(미니맵 색·크기 강조, 적 코어 승리 임박과 비슷한 체감)</summary>
        public const float PlayerCoreCriticalNormalized = 0.28f;

        /// <summary>적 코어 — 경고(승리 임박 전 단계)</summary>
        public const float EnemyCoreWarningNormalized = 0.55f;

        /// <summary>적 코어 — 위험(승리 임박·점 확대)</summary>
        public const float EnemyCoreCriticalNormalized = 0.32f;

        /// <summary>적 코어 — 승리 임박 **치명 1회 사운드** 전용(시각 ≤32%보다 이른 청각 힌트)</summary>
        public const float EnemyCoreVictoryImminentAudioNormalized = 0.22f;

        public enum CoreHpBand
        {
            Ok = 0,
            Warning = 1,
            Critical = 2
        }

        public static CoreHpBand GetPlayerCoreBand(float normalized01)
        {
            if (normalized01 <= PlayerCoreCriticalNormalized)
            {
                return CoreHpBand.Critical;
            }

            if (normalized01 <= PlayerCoreWarningNormalized)
            {
                return CoreHpBand.Warning;
            }

            return CoreHpBand.Ok;
        }

        public static CoreHpBand GetEnemyCoreBand(float normalized01)
        {
            if (normalized01 <= EnemyCoreCriticalNormalized)
            {
                return CoreHpBand.Critical;
            }

            if (normalized01 <= EnemyCoreWarningNormalized)
            {
                return CoreHpBand.Warning;
            }

            return CoreHpBand.Ok;
        }

        /// <summary>하단 선택 HUD — 코어 HP 한 줄 색(숫자 강조)</summary>
        public static Color GetPlayerCoreHudHpColor(CoreHpBand band)
        {
            return band switch
            {
                CoreHpBand.Critical => ImGuiGameUi.DefeatTint,
                CoreHpBand.Warning => ImGuiGameUi.AccentGold,
                _ => ImGuiGameUi.TextTitle
            };
        }

        public static void GetMinimapPlayerCoreDot(float hpNormalized, bool colorblindFriendly, out Color color, out float diameterPx)
        {
            CoreHpBand band = GetPlayerCoreBand(hpNormalized);
            diameterPx = band switch
            {
                CoreHpBand.Critical => 10.6f,
                CoreHpBand.Warning => 9.6f,
                _ => 9f
            };

            if (colorblindFriendly)
            {
                color = band switch
                {
                    CoreHpBand.Critical => BattleAcesArtDirection.MinimapColorblindAllyCoreCritical,
                    CoreHpBand.Warning => BattleAcesArtDirection.MinimapColorblindAllyCoreWarn,
                    _ => BattleAcesArtDirection.MinimapColorblindAllyCoreOk
                };
                return;
            }

            color = band switch
            {
                CoreHpBand.Critical => Color.Lerp(BattleAcesArtDirection.EnemyEmber, ImGuiGameUi.DefeatTint, 0.38f),
                CoreHpBand.Warning => Color.Lerp(BattleAcesArtDirection.PointTeal, ImGuiGameUi.AccentGold, 0.42f),
                _ => BattleAcesArtDirection.PointTeal
            };
        }

        public static void GetMinimapEnemyCoreDot(float hpNormalized, bool colorblindFriendly, out Color color, out float diameterPx)
        {
            CoreHpBand band = GetEnemyCoreBand(hpNormalized);
            diameterPx = band switch
            {
                CoreHpBand.Critical => 10.4f,
                CoreHpBand.Warning => 9.5f,
                _ => 9f
            };

            if (colorblindFriendly)
            {
                color = band switch
                {
                    CoreHpBand.Critical => new Color(1f, 0.92f, 0.35f, 1f),
                    CoreHpBand.Warning => new Color(1f, 0.62f, 0.22f, 1f),
                    _ => new Color(0.95f, 0.45f, 0.12f, 1f)
                };
                return;
            }

            color = band switch
            {
                CoreHpBand.Critical => Color.Lerp(BattleAcesArtDirection.EnemyEmber, new Color(1f, 0.82f, 0.22f, 1f), 0.4f),
                CoreHpBand.Warning => Color.Lerp(BattleAcesArtDirection.EnemyEmber, Color.white, 0.1f),
                _ => BattleAcesArtDirection.EnemyEmber
            };
        }

        public static void GetMinimapUnitDot(bool isPlayer, bool selected, bool colorblindFriendly, out Color color, out float diameterPx)
        {
            diameterPx = isPlayer && selected ? 6.8f : 4.5f;

            if (colorblindFriendly)
            {
                if (isPlayer)
                {
                    color = selected
                        ? BattleAcesArtDirection.MinimapColorblindAllyUnitSelected
                        : BattleAcesArtDirection.MinimapColorblindAllyUnitNormal;
                    if (!selected)
                    {
                        color.a = 0.95f;
                    }
                }
                else
                {
                    color = BattleAcesArtDirection.MinimapColorblindEnemyUnit;
                    color.a = 0.95f;
                }

                return;
            }

            color = isPlayer
                ? selected
                    ? Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.2f)
                    : Color.Lerp(BattleAcesArtDirection.PointTeal, BattleAcesArtDirection.GunmetalMid, 0.28f)
                : Color.Lerp(BattleAcesArtDirection.EnemyEmber, new Color(1f, 0.55f, 0.2f, 1f), 0.18f);
        }

        /// <summary>범례 첫 줄 — 색약 모드 문구 분기</summary>
        public static string BuildMinimapLegendCoreLine(bool korean, bool colorblindFriendly)
        {
            if (korean)
            {
                return colorblindFriendly
                    ? "코어 · 아군 파랑·시안(티얼 축)→위험 노랑 · 적 앰버·주황(낮을수록 큼)"
                    : "코어 · 아군 티얼(청록) · HP↓앰버 경고 · 적 앰버(낮을수록 큼)";
            }

            return colorblindFriendly
                ? "Cores · ally blue-cyan (teal axis)→yellow danger · enemy ember-orange (larger when low)"
                : "Cores · ally ritual teal · amber warn when low · enemy ember (larger when low)";
        }

        public static string BuildMinimapLegendUnitLine(bool korean)
        {
            return korean
                ? "유닛 · 선택 시 크고 밝음"
                : "Units · larger & brighter when selected";
        }

        /// <summary>아군 티얼 / 적 앰버 쪽으로 살짝 밀어 유닛 틴트 통일(정의 에셋 색 유지 + 가독성)</summary>
        public static Color EnhanceFactionUnitTint(Color fromDefinition, UnitTeam team)
        {
            if (team == UnitTeam.Player)
            {
                return Color.Lerp(fromDefinition, BattleAcesArtDirection.PointTeal, 0.22f);
            }

            return Color.Lerp(fromDefinition, BattleAcesArtDirection.EnemyEmber, 0.28f);
        }
    }
}
