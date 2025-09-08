using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Mecha_Stone_Golem : Enemy_Boss
{
    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float attackRange = 3f; // Khoảng cách tấn công

    [Header("Attack Range Offset")]
    [SerializeField] private Vector3 attackRangeOffset = Vector3.zero; // Điều chỉnh vị trí tâm tầm đánh
    [SerializeField] private float attackRangeHeight = 2f; // Chiều cao của capsule tấn công

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 2f; // Thời gian hồi chiêu tấn công
    [SerializeField] private string attackTriggerName = "Attack"; // Tên trigger trong Animator
    [SerializeField] private float attackDamage = 40f; // Damage tấn công cơ bản

    [Header("Glow Skill")]
    [SerializeField] private float glowCooldown = 5f; // Thời gian hồi chiêu Glow
    [SerializeField] private string glowTriggerName = "Glow"; // Tên trigger trong Animator
    [SerializeField] private string glowStateName = "Glow"; // Tên state trong Animator
    [SerializeField] private float damageReduction = 0.5f; // Giảm 50% damage
    [SerializeField] private float healthRegenPercent = 0.05f; // Hồi 5% máu đã mất
    [SerializeField] private float glowBuffDuration = 5f; // Thời gian tồn tại buff Glow
    [SerializeField] private GameObject glowEffect; // Effect hiển thị khi có buff Glow

    [Header("Shoot Skill")]
    [SerializeField] private string shootTriggerName = "Shoot"; // Tên trigger trong Animator
    [SerializeField] private string shootStateName = "Shoot"; // Tên state trong Animator
    [SerializeField] private float shootCooldown = 3.5f; // Thời gian hồi chiêu Shoot
    [SerializeField] private GameObject projectilePrefab; // Prefab của projectile
    [SerializeField] private Transform shootPoint; // Điểm bắn projectile
    [SerializeField] private float shootRange = 8f; // Tầm bắn tối thiểu
    [SerializeField] private float shootRangeMax = 12f; // Tầm bắn tối đa

    [Header("Laser Skill (Not Used)")]
    [SerializeField] private string laserTriggerName = "Laser";
    [SerializeField] private string laserStateName = "Laser";
    [SerializeField] private float laserCooldown = 10f;
    [SerializeField] private GameObject castingPrefabs; // Laser_Cast
    [SerializeField] private GameObject laserBeamPrefabs;
    [SerializeField] private Transform castPoint;
    [SerializeField] private Transform laserBeamPoint;

    [Header("Immune Skill")]
    [SerializeField] private string immuneTriggerName = "Immune"; // Tên trigger trong Animator
    [SerializeField] private string immuneStateName = "Immune"; // Tên state trong Animator
    [SerializeField] private float virtualArmor = 250f; // Lớp giáp ảo
    [SerializeField] private float armorDamageReduction = 0.5f; // Giảm 50% damage cho giáp ảo
    [SerializeField] private float immuneHealthThreshold = 0.35f; // Ngưỡng máu để kích hoạt Immune (35%)
    [SerializeField] private GameObject immuneEffect; // Effect hiển thị khi trong trạng thái Immune

    [Header("Fire Ball Summon")]
    [SerializeField] private GameObject fireBallPrefab; // Prefab cầu lửa
    [SerializeField] private int fireBallCount = 6; // Số cầu lửa mỗi đợt
    [SerializeField] private float fireBallRadius = 8f; // Bán kính spawn xung quanh player
    [SerializeField] private bool fireBallSpawnAroundPlayer = true; // Spawn xung quanh player
    [SerializeField] private float fireBallDelay = 2f; // Thời gian chờ trước khi bắt đầu triệu hồi (2 giây)
    [SerializeField] private float fireBallBatchInterval = 3f; // Khoảng cách giữa mỗi đợt cầu lửa (3 giây)
    [SerializeField] private float fireBallSpawnHeight = 10f; // Độ cao spawn cầu lửa
    [SerializeField] private float fireBallSpawnDelay = 0.2f; // Khoảng cách giữa mỗi cầu lửa trong đợt (0.2 giây)

    [Header("SFX")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip skillClip;
    [SerializeField] private AudioClip immuneClip;
    [SerializeField] private AudioClip fireBallClip;
    [SerializeField] private AudioClip[] shootClip;

    private NavMeshAgent navAgent;
    private bool isMoving = false;
    private bool useNavMesh = false;
    private float lastAttackTime;
    private float lastGlowTime;
    private float lastShootTime;
    private float lastLaserTime;
    private bool isGlowing = false;
    private bool hasGlowBuff = false;
    private bool isImmune = false;
    private bool hasUsedImmune = false; // Đánh dấu đã sử dụng skill Immune
    private float currentVirtualArmor;
    private float originalMaxHealth;
    private float glowBuffEndTime;


    protected override void Start()
    {
        base.Start();

        // Khởi tạo NavMeshAgent với kiểm tra an toàn
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            // Kiểm tra xem có NavMesh trong scene không
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                useNavMesh = true;
                navAgent.updateRotation = false;
                navAgent.updateUpAxis = false;
                navAgent.speed = chaseSpeed;
                navAgent.enabled = true;
            }
            else
            {
                // Nếu không có NavMesh, tắt NavMeshAgent
                useNavMesh = false;
                navAgent.enabled = false;
                Debug.LogWarning("Mecha Stone Golem: No NavMesh found, using direct movement instead.");
            }
        }

        // Thiết lập target nếu chưa có
        if (target == null && player != null)
        {
            target = player.transform;
        }

        // Lưu máu tối đa ban đầu
        originalMaxHealth = maxHealth;

        // Khởi tạo giáp ảo
        currentVirtualArmor = 0f;
        hasUsedImmune = false;

        // Tắt effect Immune ban đầu
        if (immuneEffect != null)
        {
            immuneEffect.SetActive(false);
        }

        // Tắt effect Glow ban đầu
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        // Kiểm tra xem có player trong tầm không
        if (player == null && target == null) return;

        // Nếu đang trong trạng thái Immune, chỉ cập nhật parameter Armor và không làm gì khác
        if (isImmune)
        {
            UpdateArmorParameter();
            return;
        }

        Vector3 playerPos = target != null ? target.position : player.transform.position;

        // Kiểm tra xem player có trong tầm đánh không
        bool playerInAttackRange = IsPlayerInAttackRange(playerPos);

        // Kiểm tra và sử dụng skill Glow
        TryUseGlowSkill();

        // Kiểm tra và sử dụng skill Shoot
        TryUseShootSkill();


        // Kiểm tra và sử dụng skill Immune
        TryUseImmuneSkill();

        // Kiểm tra hết hạn buff Glow
        CheckGlowBuffExpiration();

        // Boss luôn tìm đến player (trừ khi đang Glow)
        if (playerInAttackRange && !isGlowing)
        {
            // Trong tầm đánh - dừng di chuyển và tấn công
            StopMoving();
            TryAttack();
        }
        else if (!isGlowing)
        {
            // Ngoài tầm đánh - di chuyển đến gần player
            MoveToPlayer();
        }
        else
        {
            // Đang trong trạng thái Glow - dừng di chuyển
            StopMoving();
        }

        // Cập nhật parameter Armor trong Animator
        UpdateArmorParameter();
    }

    private void MoveToPlayer()
    {
        if (player == null && target == null) return;

        Vector3 playerPos = target != null ? target.position : player.transform.position;

        // Tính toán vị trí đích bên cạnh player
        Vector3 targetPosition = CalculatePositionBesidePlayer(playerPos);

        if (useNavMesh && navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            // Sử dụng NavMeshAgent để di chuyển
            navAgent.isStopped = false;
            navAgent.speed = chaseSpeed;
            navAgent.SetDestination(targetPosition);
            FlipByAgentVelocity();
        }
        else
        {
            // Fallback: di chuyển trực tiếp nếu không có NavMeshAgent
            Vector2 target2 = targetPosition;
            transform.position = Vector2.MoveTowards(transform.position, target2, chaseSpeed * Time.deltaTime);
            FlipEnemy(true);
        }

        isMoving = true;
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
    }

    // Kiểm tra xem player có trong tầm đánh không
    private bool IsPlayerInAttackRange(Vector3 playerPos)
    {
        Vector3 attackCenter = transform.position + GetFlippedOffset();
        float distanceToPlayer = Vector2.Distance(attackCenter, playerPos);
        return distanceToPlayer <= attackRange;
    }

    // Tính toán vị trí bên cạnh player
    private Vector3 CalculatePositionBesidePlayer(Vector3 playerPos)
    {
        // Tính hướng từ player đến boss
        Vector3 directionFromPlayerToBoss = (transform.position - playerPos).normalized;

        // Đảm bảo chỉ di chuyển theo trục X (bên cạnh player)
        directionFromPlayerToBoss.y = 0;
        directionFromPlayerToBoss.z = 0;

        // Chuẩn hóa lại vector
        if (directionFromPlayerToBoss.magnitude > 0.1f)
        {
            directionFromPlayerToBoss.Normalize();
        }
        else
        {
            // Nếu boss đang ở trên player, chọn hướng mặc định
            directionFromPlayerToBoss = Vector3.right;
        }

        // Tính vị trí đích cách player một khoảng attackRange
        Vector3 targetPosition = playerPos + directionFromPlayerToBoss * attackRange;

        return targetPosition;
    }




    private void TryUseGlowSkill()
    {
        if (isGlowing) return; // Đang trong trạng thái Glow

        // Kiểm tra cooldown Glow
        bool glowOffCooldown = Time.time >= lastGlowTime + glowCooldown;

        // Kiểm tra xem có đang trong animation Glow không
        bool inGlowAnimation = false;
        if (animator != null)
        {
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            var nextState = animator.GetNextAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);
            inGlowAnimation = currentState.IsName(glowStateName) || (inTransition && nextState.IsName(glowStateName));
        }

        // Sử dụng skill Glow nếu hết cooldown và không đang trong animation
        if (glowOffCooldown && !inGlowAnimation)
        {
            SoundFXManager.Instance.PlaySoundFXClip(skillClip, transform, 0.5f);
            lastGlowTime = Time.time;
            isGlowing = true;
            if (animator != null)
            {
                animator.SetTrigger(glowTriggerName);
            }
            StartCoroutine(GlowSkillCoroutine());
        }
    }

    private IEnumerator GlowSkillCoroutine()
    {
        // Chờ animation Glow bắt đầu
        yield return null;

        // Chờ animation Glow kết thúc
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float safetyTimer = 0f;
            const float maxWaitGlow = 3f; // Đủ cho clip Glow

            while (stateInfo.IsName(glowStateName) && stateInfo.normalizedTime < 1f && safetyTimer < maxWaitGlow)
            {
                yield return null;
                safetyTimer += Time.deltaTime;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }

        // Kích hoạt hiệu ứng sau khi Glow kết thúc
        ActivateGlowBuff();

        // Kết thúc trạng thái Glow
        isGlowing = false;
    }

    private void ActivateGlowBuff()
    {
        hasGlowBuff = true;
        glowBuffEndTime = Time.time + glowBuffDuration;

        // Kích hoạt effect Glow
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
        }

        // Hồi máu 5% lượng máu đã mất (chỉ 1 lần)
        float lostHealth = originalMaxHealth - currentHealth;
        float healAmount = lostHealth * healthRegenPercent;
        currentHealth = Mathf.Min(currentHealth + healAmount, originalMaxHealth);

        // Cập nhật HP bar
        gameUI.UpdateBossHp(currentHealth, maxHealth);

        Debug.Log($"Mecha Stone Golem: Glow buff activated! Healed {healAmount:F1} HP, Damage reduction: {damageReduction * 100}% for {glowBuffDuration} seconds");
    }

    private void CheckGlowBuffExpiration()
    {
        if (hasGlowBuff && Time.time >= glowBuffEndTime)
        {
            hasGlowBuff = false;

            // Tắt effect Glow
            if (glowEffect != null)
            {
                glowEffect.SetActive(false);
            }

            Debug.Log("Mecha Stone Golem: Glow buff expired!");
        }
    }

    private void TryUseShootSkill()
    {
        if (isGlowing) return; // Không bắn khi đang Glow

        // Kiểm tra cooldown Shoot
        bool shootOffCooldown = Time.time >= lastShootTime + shootCooldown;

        // Kiểm tra xem có đang trong animation Shoot không
        bool inShootAnimation = false;
        if (animator != null)
        {
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            var nextState = animator.GetNextAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);
            inShootAnimation = currentState.IsName(shootStateName) || (inTransition && nextState.IsName(shootStateName));
        }

        // Kiểm tra khoảng cách đến player
        if (player == null && target == null) return;
        Vector3 playerPos = target != null ? target.position : player.transform.position;
        float distanceToPlayer = Vector2.Distance(transform.position, playerPos);

        // Chỉ bắn khi player trong tầm bắn
        bool playerInShootRange = distanceToPlayer >= shootRange && distanceToPlayer <= shootRangeMax;

        // Sử dụng skill Shoot nếu hết cooldown, không đang trong animation và player trong tầm
        if (shootOffCooldown && !inShootAnimation && playerInShootRange)
        {
            SoundFXManager.Instance.PlayRandomSoundFXClip(shootClip, transform, 1f);
            lastShootTime = Time.time;
            if (animator != null)
            {
                animator.SetTrigger(shootTriggerName);
            }
            StartCoroutine(ShootSkillCoroutine());
        }
    }



    private IEnumerator ShootSkillCoroutine()
    {
        // Chờ animation Shoot bắt đầu
        yield return null;

        // Chờ animation Shoot kết thúc
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float safetyTimer = 0f;
            const float maxWaitShoot = 2f; // Đủ cho clip Shoot

            while (stateInfo.IsName(shootStateName) && stateInfo.normalizedTime < 1f && safetyTimer < maxWaitShoot)
            {
                yield return null;
                safetyTimer += Time.deltaTime;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }
    }



    private void ShootProjectile()
    {
        if (projectilePrefab == null || shootPoint == null) return;
        if (player == null && target == null) return;

        // Tạo projectile tại điểm bắn
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

        // Lấy component Golem_Projectile và set target
        Golem_Projectile projectileScript = projectile.GetComponent<Golem_Projectile>();
        if (projectileScript != null)
        {
            Transform playerTarget = target != null ? target : player.transform;
            projectileScript.SetTarget(playerTarget);
        }

        Debug.Log("Mecha Stone Golem: Shot projectile at player!");
    }

    // Phương thức này sẽ được gọi bởi Animation Event
    public void OnShootAnimationEvent()
    {
        ShootProjectile();
    }



    private void TryUseImmuneSkill()
    {
        // Kiểm tra xem đã sử dụng skill Immune chưa
        if (hasUsedImmune) return;

        // Kiểm tra xem có đang trong trạng thái Immune không
        if (isImmune) return;

        // Kiểm tra ngưỡng máu để kích hoạt Immune (dưới 35% máu tối đa)
        float healthPercentage = currentHealth / originalMaxHealth;
        if (healthPercentage > immuneHealthThreshold) return;

        // Kiểm tra xem có đang trong animation Immune không
        bool inImmuneAnimation = false;
        if (animator != null)
        {
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            var nextState = animator.GetNextAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);
            inImmuneAnimation = currentState.IsName(immuneStateName) || (inTransition && nextState.IsName(immuneStateName));
        }

        // Sử dụng skill Immune nếu đủ điều kiện và không đang trong animation
        if (!inImmuneAnimation)
        {
            hasUsedImmune = true; // Đánh dấu đã sử dụng
            isImmune = true;
            currentVirtualArmor = virtualArmor;

            // TẮT BUFF GLOW NẾU ĐANG CÓ
            if (hasGlowBuff)
            {
                hasGlowBuff = false;
                glowBuffEndTime = 0f; // Reset thời gian buff

                // Tắt effect Glow
                if (glowEffect != null)
                {
                    glowEffect.SetActive(false);
                }

                Debug.Log("Mecha Stone Golem: Glow buff deactivated due to Immune activation!");
            }

            // DỪNG DI CHUYỂN NGAY LẬP TỨC khi kích hoạt Immune
            StopMovingImmediately();

            // Kích hoạt effect Immune
            if (immuneEffect != null)
            {
                SoundFXManager.Instance.PlaySoundFXClip(immuneClip, transform, 0.8f);
                immuneEffect.SetActive(true);
            }

            if (animator != null)
            {
                animator.SetTrigger(immuneTriggerName);
            }
            StartCoroutine(ImmuneSkillCoroutine());
            Debug.Log($"Mecha Stone Golem: Immune skill activated at {healthPercentage * 100:F1}% health!");
        }
    }

    private IEnumerator ImmuneSkillCoroutine()
    {
        // Chờ animation Immune bắt đầu
        yield return null;

        // Chờ animation Immune kết thúc
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float safetyTimer = 0f;
            const float maxWaitImmune = 3f; // Đủ cho clip Immune

            while (stateInfo.IsName(immuneStateName) && stateInfo.normalizedTime < 1f && safetyTimer < maxWaitImmune)
            {
                yield return null;
                safetyTimer += Time.deltaTime;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }

        // Kích hoạt hiệu ứng sau khi Immune kết thúc
        ActivateImmuneBuff();
    }

    private void ActivateImmuneBuff()
    {
        Debug.Log($"Mecha Stone Golem: Immune buff activated! Virtual armor: {currentVirtualArmor}, Damage reduction: {armorDamageReduction * 100}%");

        // Bắt đầu triệu hồi cầu lửa sau 2 giây
        StartCoroutine(SummonFireBallsAfterDelay());
    }

    private IEnumerator SummonFireBallsAfterDelay()
    {
        // Chờ 2 giây trước khi bắt đầu triệu hồi
        yield return new WaitForSeconds(fireBallDelay);

        // Triệu hồi cầu lửa lần lượt
        StartCoroutine(SummonFireBallsSequentially());
    }

    private IEnumerator SummonFireBallsSequentially()
    {
        if (fireBallPrefab == null || (player == null && target == null)) yield break;

        Debug.Log("Mecha Stone Golem: Starting to summon fire balls in batches!");

        int batchNumber = 0;

        // Triệu hồi cầu lửa theo đợt cho đến khi hết trạng thái Immune
        while (isImmune)
        {
            batchNumber++;
            Debug.Log($"Mecha Stone Golem: Starting batch #{batchNumber} with {fireBallCount} fire balls");

            // Spawn đợt cầu lửa
            yield return StartCoroutine(SpawnFireBallBatch(batchNumber));

            // Chờ trước khi spawn đợt tiếp theo
            yield return new WaitForSeconds(fireBallBatchInterval);
        }

        Debug.Log("Mecha Stone Golem: Stopped summoning fire balls - Immune state ended!");
    }

    private IEnumerator SpawnFireBallBatch(int batchNumber)
    {
        if (fireBallPrefab == null || (player == null && target == null)) yield break;

        Vector3 playerPos = target != null ? target.position : player.transform.position;

        // Tạo danh sách vị trí spawn cho đợt này
        List<Vector3> spawnPositions = new List<Vector3>();
        List<Vector3> targetPositions = new List<Vector3>();
        List<bool> isGuaranteedHit = new List<bool>(); // Đánh dấu cầu lửa nào chắc chắn trúng

        // 1 vị trí chắc chắn trúng player (ngay trên đầu player)
        spawnPositions.Add(playerPos + Vector3.up * fireBallSpawnHeight);
        targetPositions.Add(playerPos); // Target là vị trí player
        isGuaranteedHit.Add(true);

        // Spawn các cầu lửa còn lại xung quanh player trong radius
        for (int i = 0; i < fireBallCount - 1; i++)
        {
            Vector3 spawnPosition;
            Vector3 targetPosition;

            if (fireBallSpawnAroundPlayer)
            {
                // Spawn ở trên cao, nhưng target là vị trí dưới đất xung quanh player
                Vector2 randomPoint = Random.insideUnitCircle * fireBallRadius;
                spawnPosition = playerPos + new Vector3(randomPoint.x, 0, randomPoint.y) + Vector3.up * fireBallSpawnHeight;
                targetPosition = playerPos + new Vector3(randomPoint.x, 0, randomPoint.y); // Vị trí đích ở dưới đất
            }
            else
            {
                // Spawn random trong map
                Vector2 randomPoint = Random.insideUnitCircle * fireBallRadius;
                spawnPosition = new Vector3(randomPoint.x, fireBallSpawnHeight, randomPoint.y);
                targetPosition = new Vector3(randomPoint.x, 0, randomPoint.y); // Vị trí đích ở dưới đất
            }

            spawnPositions.Add(spawnPosition);
            targetPositions.Add(targetPosition);
            isGuaranteedHit.Add(false);
        }

        // Xáo trộn thứ tự spawn để không biết cái nào sẽ trúng player
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            Vector3 temp = spawnPositions[i];
            Vector3 tempTarget = targetPositions[i];
            bool tempHit = isGuaranteedHit[i];
            int randomIndex = Random.Range(i, spawnPositions.Count);
            spawnPositions[i] = spawnPositions[randomIndex];
            targetPositions[i] = targetPositions[randomIndex];
            isGuaranteedHit[i] = isGuaranteedHit[randomIndex];
            spawnPositions[randomIndex] = temp;
            targetPositions[randomIndex] = tempTarget;
            isGuaranteedHit[randomIndex] = tempHit;
        }

        // Spawn từng cầu lửa lần lượt
        SoundFXManager.Instance.PlaySoundFXClip(fireBallClip, transform, 1f);
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            // Tạo cầu lửa
            GameObject fireBall = Instantiate(fireBallPrefab, spawnPositions[i], Quaternion.identity);

            // Set target cho cầu lửa
            Fire_Ball_Prefabs fireBallScript = fireBall.GetComponent<Fire_Ball_Prefabs>();
            if (fireBallScript != null)
            {
                // Kiểm tra xem cầu lửa này có chắc chắn trúng player không
                if (isGuaranteedHit[i])
                {
                    // Cầu lửa chắc chắn trúng: bắn vào vị trí cố định của player (không theo dõi)
                    fireBallScript.SetTargetPosition(targetPositions[i]);
                    Debug.Log($"Mecha Stone Golem: Batch #{batchNumber} - Fire ball {i + 1} (guaranteed hit - fixed position)");
                }
                else
                {
                    // Cầu lửa random, rơi xuống vị trí đích
                    fireBallScript.SetTargetPosition(targetPositions[i]);
                    Debug.Log($"Mecha Stone Golem: Batch #{batchNumber} - Fire ball {i + 1} (target position)");
                }
            }

            // Chờ một chút trước khi spawn cầu lửa tiếp theo
            yield return new WaitForSeconds(fireBallSpawnDelay);
        }

        Debug.Log($"Mecha Stone Golem: Completed batch #{batchNumber}");
    }



    private void UpdateArmorParameter()
    {
        if (animator != null)
        {
            // Cập nhật parameter Armor dựa trên giáp ảo hiện tại
            int armorValue = Mathf.RoundToInt(currentVirtualArmor);
            animator.SetInteger("Armor", armorValue);

            // Kiểm tra nếu giáp ảo bị vỡ hoàn toàn
            if (currentVirtualArmor <= 0 && isImmune)
            {
                isImmune = false;

                // Tắt effect Immune
                if (immuneEffect != null)
                {
                    immuneEffect.SetActive(false);
                }

                // Khôi phục di chuyển bình thường
                if (useNavMesh && navAgent != null && navAgent.enabled)
                {
                    navAgent.speed = chaseSpeed; // Khôi phục tốc độ
                }

                Debug.Log("Mecha Stone Golem: Virtual armor destroyed! Immune state ended. Movement restored.");
            }
        }
    }

    private void StopMoving()
    {
        if (useNavMesh && navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
        }

        isMoving = false;
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
    }

    private void StopMovingImmediately()
    {
        // Dừng NavMeshAgent ngay lập tức
        if (useNavMesh && navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero; // Dừng velocity
            navAgent.ResetPath(); // Reset path để dừng hoàn toàn
        }

        // Dừng di chuyển
        isMoving = false;
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        Debug.Log("Mecha Stone Golem: Stopped moving immediately for Immune state!");
    }

    private void TryAttack()
    {
        if (player == null && target == null) return;

        // Kiểm tra cooldown tấn công
        bool offCooldown = Time.time >= lastAttackTime + attackCooldown;

        // Kiểm tra xem có đang tấn công không
        bool inAttackNow = false;
        if (animator != null)
        {
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            var nextState = animator.GetNextAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);
            inAttackNow = currentState.IsName("Attack") || (inTransition && nextState.IsName("Attack"));
        }

        // Tấn công nếu hết cooldown và không đang tấn công
        if (offCooldown && !inAttackNow)
        {
            lastAttackTime = Time.time;
            if (animator != null)
            {
                animator.SetTrigger(attackTriggerName);
            }
        }
    }

    // Được gọi bởi Animation Event khi attack animation kết thúc
    public void OnAttackAnimationEvent()
    {
        SoundFXManager.Instance.PlaySoundFXClip(attackClip, transform, 0.5f);
        DealAttackDamage();
    }

    private void DealAttackDamage()
    {
        if (player == null && target == null) return;

        Vector3 playerPos = target != null ? target.position : player.transform.position;

        // Kiểm tra xem player có trong tầm đánh không
        if (IsPlayerInAttackRange(playerPos))
        {
            // Tìm component Player và gây damage
            Player playerScript = target != null ? target.GetComponent<Player>() : player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(attackDamage);
                Debug.Log($"Mecha Stone Golem: Dealt {attackDamage} damage to player!");
            }
        }
    }

    private void FlipByAgentVelocity()
    {
        if (useNavMesh && navAgent != null && navAgent.enabled)
        {
            Vector2 velocity = navAgent.velocity;
            if (velocity.x > 0.1f)
            {
                transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            }
            else if (velocity.x < -0.1f)
            {
                transform.localScale = new Vector3(-1.3f, 1.3f, 1.3f);
            }
        }
    }

    // Flip theo hướng Player (fallback)
    private void FlipEnemy(bool shouldFlip)
    {
        if (player != null && shouldFlip)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            if (direction.x > 0.1f)
            {
                transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            }
            else if (direction.x < -0.1f)
            {
                transform.localScale = new Vector3(-1.3f, 1.3f, 1.3f);
            }
        }
    }

    // Vẽ gizmos để debug
    private void OnDrawGizmosSelected()
    {
        // Tầm tấn công từ boss với offset đã flip
        Vector3 attackCenter = transform.position + GetFlippedOffset();

        // Vẽ capsule tấn công
        Gizmos.color = Color.red;
        DrawWireCapsule(attackCenter, attackRange, attackRangeHeight);

        // Hiển thị offset bằng một điểm nhỏ
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(attackCenter, 0.1f);

        // Đường kẻ từ boss đến tâm tấn công
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, attackCenter);

        // Vẽ tầm bắn Shoot
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shootRange);
        Gizmos.DrawWireSphere(transform.position, shootRangeMax);
    }

    // Vẽ wire capsule
    private void DrawWireCapsule(Vector3 center, float radius, float height)
    {
        // Vẽ phần thân capsule (hình trụ)
        Vector3 top = center + Vector3.up * (height / 2f);
        Vector3 bottom = center - Vector3.up * (height / 2f);

        // Vẽ các đường tròn ở đầu và cuối
        DrawWireCircle(top, radius, Vector3.up);
        DrawWireCircle(bottom, radius, Vector3.up);

        // Vẽ các đường thẳng nối
        Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
        Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
        Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
        Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
    }

    // Vẽ wire circle
    private void DrawWireCircle(Vector3 center, float radius, Vector3 normal)
    {
        int segments = 16;
        Vector3 prevPoint = center;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }

            prevPoint = point;
        }
    }

    // Lấy offset đã được flip theo hướng của Boss
    private Vector3 GetFlippedOffset()
    {
        Vector3 flippedOffset = attackRangeOffset;

        // Nếu Boss đang nhìn sang trái (scale.x < 0), flip offset theo trục X
        if (transform.localScale.x < 0)
        {
            flippedOffset.x = -attackRangeOffset.x;
        }

        return flippedOffset;
    }

    public override void TakeDamage(float damage)
    {
        // Áp dụng giảm damage nếu có buff Glow
        float finalDamage = damage;
        if (hasGlowBuff)
        {
            finalDamage = damage * (1f - damageReduction);
        }

        // Xử lý giáp ảo nếu đang trong trạng thái Immune
        if (isImmune && currentVirtualArmor > 0)
        {
            // Tính damage cho giáp ảo (giảm 50%)
            float armorDamage = finalDamage * (1f - armorDamageReduction);
            currentVirtualArmor -= armorDamage;

            // Đảm bảo giáp ảo không âm
            currentVirtualArmor = Mathf.Max(currentVirtualArmor, 0f);

            Debug.Log($"Mecha Stone Golem: Virtual armor took {armorDamage:F1} damage, remaining: {currentVirtualArmor:F1}");

            // Nếu giáp ảo còn, không nhận damage vào máu thật
            if (currentVirtualArmor > 0)
            {
                return;
            }
        }

        // Nhận damage vào máu thật
        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(currentHealth, 0);
        gameUI.UpdateBossHp(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }
}
