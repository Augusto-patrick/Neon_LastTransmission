using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Setup")]
    public Camera playerCamera;
    public ParticleSystem muzzleFlash;

    [Header("Shooting")]
    public float range = 100f;
    public float fireRate = 0.25f;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int currentAmmo = 30;
    public int reserveAmmo = 120;

    [Header("Reload")]
    public float reloadTime = 2f;

    [Header("Muzzle Light")]
    public float muzzleLightRange = 8f;
    public float muzzleLightIntensity = 5f;
    public float muzzleLightLife = 0.06f;

    [Header("Tracer")]
    public float tracerLife = 0.08f;
    public float tracerStartWidth = 0.05f;
    public float tracerEndWidth = 0.03f;

    [Header("Impact")]
    public float impactLife = 0.12f;
    public float impactScale = 0.15f;

    private float nextFireTime = 0f;
    private bool isReloading = false;

    private Transform muzzleTip;
    private Light muzzleLight;

    void Start()
    {
        if (muzzleFlash != null)
        {
            muzzleTip = muzzleFlash.transform;
        }

        CreateMuzzleLight();
    }

    void Update()
    {
        if (ScoreboardUI.MenuOpen)
            return;

        // Reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            if (currentAmmo < magazineSize && reserveAmmo > 0)
            {
                StartCoroutine(Reload());
            }
        }

        // Shoot
        if (Input.GetMouseButtonDown(0)
            && Time.time >= nextFireTime
            && !isReloading)
        {
            if (currentAmmo > 0)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
            else
            {
                Debug.Log("Out of ammo! Press R to reload.");
            }
        }
    }

    void Shoot()
    {
        currentAmmo--;

        // Muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        FlashMuzzle();

        // Raycast
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        RaycastHit[] hits = Physics.RaycastAll(ray, range);

        RaycastHit nearest = default;
        bool foundNearest = false;

        RaycastHit hit = default;
        bool foundZombie = false;

        for (int i = 0; i < hits.Length; i++)
        {
            if (!foundNearest || hits[i].distance < nearest.distance)
            {
                nearest = hits[i];
                foundNearest = true;
            }

            ZombieHealth check =
                hits[i].collider.GetComponentInParent<ZombieHealth>();

            if (check == null)
                continue;

            if (!foundZombie || hits[i].distance < hit.distance)
            {
                hit = hits[i];
                foundZombie = true;
            }
        }

        Vector3 endPoint = ray.origin + ray.direction * range;

        if (foundNearest)
        {
            endPoint = nearest.point;
            SpawnImpactSpark(nearest.point, nearest.normal);
        }

        SpawnTracer(endPoint);

        ZombieHealth zombie = null;

        if (foundZombie)
        {
            zombie =
                hit.collider.GetComponentInParent<ZombieHealth>();
        }
        else
        {
            zombie = FindZombieNearRay(ray);
        }

        if (zombie != null)
        {
            Debug.Log("Robot Hit");

            zombie.TakeDamage(25f);

            ZombieHitEffect hitEffect =
                zombie.GetComponentInParent<ZombieHitEffect>();

            if (hitEffect == null)
            {
                hitEffect =
                    zombie.GetComponentInChildren<ZombieHitEffect>();
            }

            if (hitEffect != null)
            {
                hitEffect.Hit();
            }

            ZombieAI zombieAI =
                zombie.GetComponentInParent<ZombieAI>();

            if (zombieAI != null)
            {
                Vector3 hitDirection =
                    zombie.transform.position -
                    playerCamera.transform.position;

                hitDirection.y = 0f;

                zombieAI.Knockback(hitDirection);
            }
        }

        Debug.Log("Ammo: " + currentAmmo + "/" + reserveAmmo);
    }

    ZombieHealth FindZombieNearRay(Ray ray)
    {
        ZombieHealth best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < ZombieHealth.All.Count; i++)
        {
            ZombieHealth candidate = ZombieHealth.All[i];

            if (candidate == null)
                continue;

            Vector3 zombiePosition = candidate.transform.position;
            Vector3 toZombie = zombiePosition - ray.origin;

            float alongRay =
                Vector3.Dot(toZombie, ray.direction);

            if (alongRay <= 0f || alongRay > range)
                continue;

            Vector3 closestPoint =
                ray.origin + ray.direction * alongRay;

            float distanceFromRay =
                Vector3.Distance(closestPoint, zombiePosition);

            if (distanceFromRay <= 1.5f && alongRay < bestDistance)
            {
                best = candidate;
                bestDistance = alongRay;
            }
        }

        return best;
    }

    void CreateMuzzleLight()
    {
        GameObject lightObj = new GameObject("MuzzleLight");
        lightObj.transform.SetParent(transform, false);

        if (muzzleTip != null && muzzleTip != transform)
        {
            lightObj.transform.position = muzzleTip.position;
        }

        muzzleLight = lightObj.AddComponent<Light>();
        muzzleLight.type = LightType.Point;
        muzzleLight.range = muzzleLightRange;
        muzzleLight.intensity = 0f;
        muzzleLight.color = new Color(1f, 0.85f, 0.5f);
    }

    void FlashMuzzle()
    {
        if (muzzleLight == null)
            return;

        muzzleLight.intensity = muzzleLightIntensity;

        MuzzleFlashFade fade =
            muzzleLight.gameObject.GetComponent<MuzzleFlashFade>();

        if (fade == null)
        {
            fade =
                muzzleLight.gameObject.AddComponent<MuzzleFlashFade>();
        }

        fade.life = muzzleLightLife;
        fade.Play();
    }

    void SpawnTracer(Vector3 end)
    {
        Vector3 start = muzzleTip != null
            ? muzzleTip.position
            : transform.position;

        GameObject go = new GameObject("Tracer");
        go.transform.SetParent(transform, true);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = tracerStartWidth;
        lr.endWidth = tracerEndWidth;

        Material mat = CreateGlowMaterial(
            new Color(1f, 0.9f, 0.5f)
        );

        if (mat != null)
        {
            lr.sharedMaterial = mat;
        }

        TracerFade fade = go.AddComponent<TracerFade>();
        fade.life = tracerLife;
    }

    void SpawnImpactSpark(Vector3 point, Vector3 normal)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "ImpactSpark";
        go.transform.position = point + normal * 0.02f;
        go.transform.localScale = Vector3.one * impactScale;

        Destroy(go.GetComponent<Collider>());

        Renderer renderer = go.GetComponent<Renderer>();

        Material mat = CreateGlowMaterial(
            new Color(1f, 0.85f, 0.4f)
        );

        if (mat != null)
        {
            renderer.sharedMaterial = mat;
        }

        ImpactFade fade = go.AddComponent<ImpactFade>();
        fade.life = impactLife;
    }

    Material CreateGlowMaterial(Color color)
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
            return null;

        Material material = new Material(shader);
        material.color = color;

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor(
                "_EmissionColor",
                color * 1.8f
            );
        }

        return material;
    }

    IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;

        Debug.Log("Reload complete!");
        Debug.Log("Ammo: " + currentAmmo + "/" + reserveAmmo);
    }

    class MuzzleFlashFade : MonoBehaviour
    {
        private Light lightComponent;

        public float life = 0.06f;

        private float endTime;

        void Awake()
        {
            lightComponent = GetComponent<Light>();
        }

        public void Play()
        {
            endTime = Time.time + life;
        }

        void Update()
        {
            if (lightComponent == null)
                return;

            if (Time.time > endTime)
            {
                lightComponent.intensity = 0f;
            }
        }
    }

    class TracerFade : MonoBehaviour
    {
        public float life = 0.08f;

        private float endTime;

        void Start()
        {
            endTime = Time.time + life;
        }

        void Update()
        {
            if (Time.time >= endTime)
            {
                Destroy(gameObject);
            }
        }
    }

    class ImpactFade : MonoBehaviour
    {
        public float life = 0.12f;

        private float endTime;
        private Renderer rendererComponent;

        void Start()
        {
            endTime = Time.time + life;
            rendererComponent = GetComponent<Renderer>();
        }

        void Update()
        {
            if (Time.time >= endTime)
            {
                Destroy(gameObject);
                return;
            }

            if (rendererComponent != null)
            {
                Material material = rendererComponent.material;

                if (material != null &&
                    material.HasProperty("_Color"))
                {
                    Color color = material.color;

                    float t =
                        (endTime - Time.time) / life;

                    material.color = new Color(
                        color.r,
                        color.g,
                        color.b,
                        t
                    );
                }
            }
        }
    }
}