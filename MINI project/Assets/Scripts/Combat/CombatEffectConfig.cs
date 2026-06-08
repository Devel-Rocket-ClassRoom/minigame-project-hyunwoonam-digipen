using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 이펙트 프리팹 에셋별 표시 보정. 이펙트 프리팹 루트에 부착(선택).
    /// CombatEffectPresenter가 스폰 시 읽어 타겟 앵커 기준 위치/스케일/회전/수명/추적을 적용한다.
    /// 컴포넌트가 없으면 프리젠터는 기존 기본 동작(앵커, identity, 프리팹 스케일, 2초)을 쓴다.
    /// </summary>
    public sealed class CombatEffectConfig : MonoBehaviour
    {
        /// <summary>이펙트 스케일 보정.</summary>
        public Vector3 LocalScale = Vector3.one;

        /// <summary>타겟 앵커 기준 위치 오프셋.</summary>
        public Vector3 Offset = Vector3.zero;

        /// <summary>회전(오일러 각).</summary>
        public Vector3 Euler = Vector3.zero;

        /// <summary>수명(초). 0이면 프리젠터 기본값(2초).</summary>
        public float LifetimeSec = 0f;

        /// <summary>true면 타겟 트랜스폼 자식으로 부착되어 따라다닌다.</summary>
        public bool AttachToTarget = false;
    }
}
