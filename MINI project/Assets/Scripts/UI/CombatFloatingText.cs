using TMPro;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 중 피해/회복/방어 수치를 대상 머리 위에 띄워 상승·페이드하는 1회용 연출.
    /// 프리팹 없이 절차적으로 생성되며 수명 종료 시 자동 파괴된다.
    /// 방어로 경감된 피격은 색/라벨을 구분해 "경감됐다"를 즉시 읽히게 한다.
    /// </summary>
    public sealed class CombatFloatingText : MonoBehaviour
    {
        private const float LifetimeSec = 0.9f;
        private const float RiseSpeed = 1.2f;
        private const float BaseFontSize = 4.5f;

        private TextMeshPro label;
        private Color baseColor;
        private float elapsed;
        private Camera cachedCamera;

        /// <summary>피해 수치 표시. guarded=true 면 방어 경감 스타일(파랑 + "방어").</summary>
        public static void SpawnDamage(Vector3 worldPos, int amount, bool guarded)
        {
            if (amount <= 0)
            {
                return;
            }

            Color color = guarded ? new Color(0.55f, 0.78f, 1f, 1f) : new Color(1f, 0.85f, 0.25f, 1f);
            string text = guarded ? Loc.Format("combat_guarded_damage_fmt", amount) : ("-" + amount);
            Spawn(worldPos, text, color, guarded ? 0.85f : 1f);
        }

        /// <summary>회복 수치 표시(초록 +).</summary>
        public static void SpawnHeal(Vector3 worldPos, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Spawn(worldPos, "+" + amount, new Color(0.3f, 1f, 0.5f, 1f), 1f);
        }

        /// <summary>임의 텍스트 부유 연출 생성.</summary>
        public static void Spawn(Vector3 worldPos, string text, Color color, float scale)
        {
            GameObject go = new GameObject("CombatFloatingText");
            go.transform.position = worldPos;
            CombatFloatingText fx = go.AddComponent<CombatFloatingText>();
            fx.Init(text, color, scale);
        }

        private void Init(string text, Color color, float scale)
        {
            label = gameObject.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = BaseFontSize * Mathf.Max(0.1f, scale);
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.color = color;
            label.rectTransform.sizeDelta = new Vector2(5f, 1.4f);

            MeshRenderer renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 100;
            }

            baseColor = color;
            FaceCamera();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * (RiseSpeed * Time.deltaTime);
            FaceCamera();

            if (label != null)
            {
                float alpha = 1f - Mathf.Clamp01(elapsed / LifetimeSec);
                label.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            }

            if (elapsed >= LifetimeSec)
            {
                Destroy(gameObject);
            }
        }

        private void FaceCamera()
        {
            if (cachedCamera == null)
            {
                cachedCamera = Camera.main;
            }

            if (cachedCamera != null)
            {
                transform.rotation = cachedCamera.transform.rotation;
            }
        }
    }
}
