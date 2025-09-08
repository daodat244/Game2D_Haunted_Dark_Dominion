using UnityEngine;

public class Pistol : MonoBehaviour
{
    private float rotatateOffset = 180f;

    [Header("Stats")]
    [SerializeField] private float damage = 10f; // 👈 damage thuộc về súng
    [SerializeField] private float shotDelay = 0.15f;
    [SerializeField] private Transform firePos;
    [SerializeField] private GameObject bulletPrefabs;

    [Header("Ammo")]
    [SerializeField] private int maxAmmo = 24;
    public int currentAmmo { get; private set; }

    [Header("Reload")]
    [SerializeField] private float reloadDuration = 3f;
    public bool isReloading { get; private set; }
    private float reloadTimer;
    private float nextShot;

    [Header("SFX")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;

    private Player player;

    void Start()
    {
        currentAmmo = maxAmmo;
        player = GetComponentInParent<Player>(); // giả sử Pistol là con của Player
    }

    void Update()
    {
        RotateGun();
        HandleShootingInput();
        HandleReloadInput();
        HandleReloadProcess();
    }

    void RotateGun()
    {
        if (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width ||
            Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height)
        {
            return;
        }

        Vector3 displacement = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(displacement.y, displacement.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotatateOffset);

        if (angle < -90 || angle > 90)
            transform.localScale = new Vector3(-0.5f, 0.5f, 1);
        else
            transform.localScale = new Vector3(-0.5f, -0.5f, 1);
    }

    void HandleShootingInput()
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();
    }

    void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isReloading)
        {
            StartReload();
            SoundFXManager.Instance.PlaySoundFXClip(reloadClip, transform, 0.7f);
        }
    }

    void HandleReloadProcess()
    {
        if (!isReloading) return;

        reloadTimer -= Time.deltaTime;
        if (reloadTimer <= 0f)
        {
            FinishReload();
        }
    }

    public void Shoot()
    {
        Player player = FindFirstObjectByType<Player>(); // hoặc truyền reference
        if ((currentAmmo > 0 || (player != null && player.HasInfiniteAmmo)) && Time.time > nextShot && !isReloading)
        {
            nextShot = Time.time + shotDelay;
            SoundFXManager.Instance.PlaySoundFXClip(shootClip, transform, 0.1f);

            GameObject bullet = Instantiate(bulletPrefabs, firePos.position, firePos.rotation);

            float totalDamage = damage;
            if (player != null)
                totalDamage += player.BaseDamage * player.DamageMultiplier;

            bullet.GetComponent<PlayerBullet>().SetDamage(totalDamage);

            if (player == null || !player.HasInfiniteAmmo)
                currentAmmo--;
        }
    }

    public void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadDuration;
    }

    private void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    public void ApplyReloadUpgrade(float reduction)
    {
        reloadDuration -= reduction;
        reloadDuration = Mathf.Max(0.5f, reloadDuration); // không cho nhỏ hơn 0.5 giây
    }
}
