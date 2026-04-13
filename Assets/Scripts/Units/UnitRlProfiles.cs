using UnityEngine;

namespace Game.Units
{
    /// <summary>
    /// Battle Aces RTS 무게감 — 이동(Nav·비행) 튜닝값. 기본값은 기존 전장 체감에 가깝게 둠.
    /// </summary>
    [System.Serializable]
    public struct MoveProfileData
    {
        [Tooltip("NavMeshAgent 가 있을 때 가속·각속도 적용")]
        public bool applyToNavAgent;

        public float navAcceleration;
        public float navAngularSpeedDeg;

        [Range(0.2f, 1f)]
        [Tooltip("애니 기반 타격 대기 중 이동 속도 배율")]
        public float windupMoveSpeedMultiplier;

        public bool enableCrowdSeparation;
        [Tooltip("같은 팀 유닛과 이 거리 이하면 살짝 밀어냄")]
        public float separationRadius;

        public float separationPushPerSecond;

        [Tooltip("비행 유닛 수평 속도가 목표까지 이 가속으로 스무딩")]
        public float flyHorizontalAcceleration;

        public static MoveProfileData CreateDefault()
        {
            return new MoveProfileData
            {
                applyToNavAgent = true,
                navAcceleration = 26f,
                navAngularSpeedDeg = 520f,
                windupMoveSpeedMultiplier = 0.74f,
                enableCrowdSeparation = false,
                separationRadius = 1.18f,
                separationPushPerSecond = 0.42f,
                flyHorizontalAcceleration = 22f,
            };
        }
    }

    /// <summary>
    /// Mecanim 파라미터 이름 + 애니 이벤트 기반 타격 사용 여부.
    /// </summary>
    [System.Serializable]
    public struct AnimCombatProfileData
    {
        [Tooltip("켜면 타격은 AnimStrike() 이벤트까지 지연(브릿지 컴포넌트 필요)")]
        public bool useAnimDrivenStrike;

        [Tooltip("애니 이벤트 누락 시 이 시간 후 강제 타격")]
        public float strikeFallbackTimeoutUnscaled;

        [Tooltip("Die 트리거 후 오브젝트 파괴까지 대기(언스케일 초). 0이면 즉시 파괴. Animator+Die 있을 때만 적용")]
        public float deathDestroyDelayUnscaled;

        public string speedFloatParam;
        public string attackTriggerParam;
        public string dieTriggerParam;
        public string inCombatBoolParam;

        public static AnimCombatProfileData CreateDefault()
        {
            return new AnimCombatProfileData
            {
                useAnimDrivenStrike = false,
                strikeFallbackTimeoutUnscaled = 0.95f,
                deathDestroyDelayUnscaled = 0f,
                speedFloatParam = "Speed",
                attackTriggerParam = "Attack",
                dieTriggerParam = "Die",
                inCombatBoolParam = "InCombat",
            };
        }
    }
}
