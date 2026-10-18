using UnityEngine;

public class MachineFeedback : MonoBehaviour
{
    public DamageAmplifier amplifier;
    public TowerDamageBoost damageBoost;
    public TowerCooling cooling;
    public ParticleSystem chargeSparks;
    public ParticleSystem damageSparks;
    public ParticleSystem coolingSparks;
    public LineRenderer chargeArc;
    public int effectVersion;
    public CoolingFan fan;
    public ParticleSystem fanWind;
    public Renderer[] flashRenderers;
    public float flashInterval = 1.2f;
    public float flashDuration = 0.16f;
    private Material[][] originalMaterials;
    private Material whiteFlash;
    private Material goldFlash;
    private float flashTimer;
    private float flashRemaining;
    private bool flashActive;
    private bool alternateFlash;
    private bool hadCooling;
    private bool hadDamage;
    private bool wasBoosting;

    private void Start()
    {
        if (flashRenderers == null) flashRenderers = new Renderer[0];
        originalMaterials = new Material[flashRenderers.Length][];
        for (int i = 0; i < flashRenderers.Length; i++)
            if (flashRenderers[i] != null) originalMaterials[i] = flashRenderers[i].sharedMaterials;
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        whiteFlash = new Material(shader);
        whiteFlash.SetColor("_BaseColor", Color.white);
        goldFlash = new Material(shader);
        goldFlash.SetColor("_BaseColor", new Color(1f, 0.7f, 0.08f));
    }

    private void Update()
    {
        if (amplifier != null && chargeSparks != null)
        {
            bool fighting = amplifier.waves == null || amplifier.waves.CombatActive;
            bool charging = fighting && amplifier.connectedTowers > 0 && amplifier.activeSeconds <= 0f;
            var emission = chargeSparks.emission;
            emission.rateOverTime = 8f + 28f * Mathf.Clamp01(amplifier.charge / amplifier.RequiredChargeSeconds);
            SetPlaying(chargeSparks, charging);
            bool boosting = amplifier.activeSeconds > 0f;
            if (boosting && !wasBoosting) chargeSparks.Emit(48);
            wasBoosting = boosting;
            if (chargeArc != null)
            {
                chargeArc.enabled = charging || boosting;
                float strength = boosting ? 1f : Mathf.Clamp01(amplifier.charge / amplifier.RequiredChargeSeconds);
                chargeArc.startWidth = chargeArc.endWidth = 0.012f + 0.018f * strength;
                for (int i = 0; i < 7; i++)
                {
                    float jitter = i == 0 || i == 6 ? 0f : Mathf.Sin(Time.time * 32f + i * 8f) * (0.025f + 0.055f * strength);
                    chargeArc.SetPosition(i, new Vector3(Mathf.Lerp(-0.22f, 0.22f, i / 6f), 0.76f + jitter, jitter));
                }
            }
        }
        bool damaged = damageBoost != null && damageBoost.Multiplier > 1f;
        SetPlaying(damageSparks, damaged);
        bool cooled = cooling != null && cooling.source != null && cooling.speedMultiplier > 1f;
        SetPlaying(coolingSparks, cooled);
        flashTimer -= Time.deltaTime;
        flashRemaining -= Time.deltaTime;
        if (flashActive && (flashRemaining <= 0f || (!cooled && !damaged))) RestoreMaterials();
        if ((cooled || damaged) && (flashTimer <= 0f || (cooled && !hadCooling) || (damaged && !hadDamage)))
        {
            bool gold = damaged && (!cooled || alternateFlash);
            alternateFlash = !alternateFlash;
            for (int i = 0; i < flashRenderers.Length; i++)
            {
                if (flashRenderers[i] == null || originalMaterials[i] == null) continue;
                Material[] materials = new Material[originalMaterials[i].Length];
                for (int j = 0; j < materials.Length; j++) materials[j] = gold ? goldFlash : whiteFlash;
                flashRenderers[i].sharedMaterials = materials;
            }
            flashActive = true;
            flashRemaining = flashDuration;
            flashTimer = flashInterval;
        }
        hadCooling = cooled;
        hadDamage = damaged;
    }

    private void LateUpdate()
    {
        if (fan == null || fanWind == null) return;
        Transform nozzle = fan.blades != null ? fan.blades : fan.transform;
        Vector3 direction = fan.blades != null ? nozzle.TransformDirection(fan.bladeAxis) : nozzle.forward;
        fanWind.transform.SetPositionAndRotation(nozzle.position + direction * 0.08f, Quaternion.LookRotation(direction));
        SetPlaying(fanWind, fan.isActiveAndEnabled);
    }

    private void RestoreMaterials()
    {
        if (originalMaterials != null)
            for (int i = 0; i < flashRenderers.Length; i++)
                if (flashRenderers[i] != null && originalMaterials[i] != null)
                    flashRenderers[i].sharedMaterials = originalMaterials[i];
        flashActive = false;
    }

    private void OnDisable() { RestoreMaterials(); }
    private void OnDestroy()
    {
        if (whiteFlash != null) Destroy(whiteFlash);
        if (goldFlash != null) Destroy(goldFlash);
    }

    private void SetPlaying(ParticleSystem particles, bool active)
    {
        if (particles == null) return;
        if (active && !particles.isPlaying) particles.Play();
        if (!active && particles.isPlaying) particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
