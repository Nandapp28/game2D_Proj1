// P7 — game feel saat serangan kena: guncangan kamera, efek tebasan (VfxPool), hit-stop, percikan.
// Hanya mendengarkan event BattleSystem. (Versi demo: monster kalah memudar lewat MonsterPanel.PlayFaint.)
using System.Collections;
using UnityEngine;

public class BattleFeel : MonoBehaviour
{
    [SerializeField] private Transform cameraRig;               // Main Camera Scn_Battle
    [SerializeField] private VfxPool hitVfx;
    [SerializeField] private PixelBurst burst;                  // versi demo
    [SerializeField] private SpriteRenderer playerBody;
    [SerializeField] private SpriteRenderer enemyBody;
    [SerializeField] private float shakeAmount = 0.15f;
    [SerializeField] private float shakeSeconds = 0.2f;
    [SerializeField] private float criticalShakeSeconds = 0.35f;
    [SerializeField] private float hitStopSeconds = 0.06f;

    private BattleSystem battle;
    private Vector3 cameraHome;

    public void Construct(BattleSystem battleSystem)
    {
        battle = battleSystem;
        cameraHome = cameraRig.localPosition;
        battle.Hit += OnHit;
        battle.Healed += OnHealed;
    }

    void OnDestroy()
    {
        if (battle != null) { battle.Hit -= OnHit; battle.Healed -= OnHealed; }
        Time.timeScale = 1f;                                   // jangan sampai tertinggal lambat
    }

    private void OnHit(MonsterInstance target, int damage)
    {
        SpriteRenderer body = target == battle.Player ? playerBody : enemyBody;
        hitVfx.Play(body.bounds.center);
        Color c = battle.LastHitElement.HasValue ? StarterGiver.ElementColor(battle.LastHitElement.Value) : Color.white;
        if (burst != null) burst.Play(body.bounds.center, c, battle.LastHitElement.HasValue ? 22 : 12, 90f, 0.5f);
        StartCoroutine(Shake(battle.LastHitCritical ? criticalShakeSeconds : shakeSeconds));
        StartCoroutine(HitStop());
    }

    private void OnHealed(MonsterInstance target)
    {
        if (burst != null) burst.Play(playerBody.bounds.center, HpColors.High, 16, 50f, 0.6f);
    }

    private IEnumerator Shake(float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            float k = 1f - t / seconds;                        // makin lama makin kecil
            cameraRig.localPosition = cameraHome + (Vector3)(Random.insideUnitCircle * shakeAmount * k);
            yield return null;
        }
        cameraRig.localPosition = cameraHome;
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(hitStopSeconds);
        Time.timeScale = 1f;
    }
}
