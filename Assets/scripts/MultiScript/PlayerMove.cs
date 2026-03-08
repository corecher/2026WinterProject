using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : NetworkBehaviour 
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float acceleration = 100f;
    
    [Header("회전 설정")]
    [SerializeField] private float rotationSpeed = 120f;
    
    [Header("점프 설정")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);
    
    [Header("부스트 설정")]
    [SerializeField] private float boostSpeedMultiplier = 1.5f; 
    [SerializeField] public float maxBoostGauge = 100f;
    [SerializeField] private float boostRechargeRate = 8f; 
    [SerializeField] private float boostConsumeRate = 25f; 

    // ==========================================
    // 💡 [분노/각성 추가] 설정
    // ==========================================
    [Header("분노 및 각성 설정")]
    [SerializeField] public float maxRageGauge = 100f;     // 최대 분노 게이지
    [SerializeField] private float ragePerItem = 100f;       // 아이템 1개당 증가량 (5%)
    [SerializeField] private float awakenDuration = 20f;   // 각성 지속 시간
    [SerializeField] private float awakenSpeedMultiplier = 1.5f; // 각성 시 기본 속도 증가량
    [SerializeField] private string ragePointTag = "RagePoint";  // 분노 포인트 아이템 태그

    public float currentRageGauge = 0f;
    public bool isAwakened = false;
    private float awakenTimer = 0f;
    [SerializeField] private GameObject awakenEffectObject;
    // ==========================================
    
    [Header("충돌 페널티 설정")]
    [SerializeField] private float collisionSlowdownDuration = 1.5f;
    [SerializeField] private float collisionSlowdownMultiplier = 0.5f;
    [SerializeField] private float maxGaugeLossPercent = 0.5f;
    [SerializeField] private float minCollisionSpeed = 5f;
    [SerializeField] private string obstacleTag = "Obstacle"; 
    
    [Header("물리 설정")]
    [SerializeField] private float mass = 1000f;
    [SerializeField] private float drag = 0.5f;
    [SerializeField] private float angularDrag = 3f;
    private NetworkGameTimer timer;
    private Rigidbody rb;
    private bool isGrounded;
    private float lastJumpTime = -999f;
    private Collider col;
    
    public float currentBoostGauge;
    private bool isBoosting;
    
    private float collisionSlowdownTimer = 0f;
    private bool isSlowedDown = false;
    private bool isTimerAssigned = false;
    
    [Header("부스트 시야각(FOV) 설정")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float boostFOV = 80f;
    [SerializeField] private float fovChangeSpeed = 5f;
    private Camera playerCamera;
    
    private Animator animator;
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int IsGroundedBool = Animator.StringToHash("IsGrounded");

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            currentBoostGauge = maxBoostGauge;
            currentRageGauge = 0f; // 초기화
            TryFindTimer();
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        animator = GetComponent<Animator>();
        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        
        rb.centerOfMass = new Vector3(0, -0.3f, 0);
        
        PhysicsMaterial physicsMaterial = new PhysicsMaterial("LowFriction");
        physicsMaterial.dynamicFriction = 0.1f;
        physicsMaterial.staticFriction = 0.1f;
        physicsMaterial.bounciness = 0.1f;
        physicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        physicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        
        if (col != null) col.material = physicsMaterial;
        
        if (IsOwner)
        { 
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null) playerCamera.fieldOfView = normalFOV;
        }
    }

    private void TryFindTimer()
    {
        timer = NetworkGameTimer.Instance;
        if (timer == null) timer = GameObject.FindFirstObjectByType<NetworkGameTimer>();
        if (timer != null) isTimerAssigned = true;
    }

    void Update()
    {
        if (!IsOwner) return;
        if (!isTimerAssigned) TryFindTimer();
        
        UpdateCameraFOV();

        if (isTimerAssigned && timer != null && !timer.CanMove) 
        {
            if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        bool wantsToBoost = Input.GetKey(KeyCode.LeftShift);
        
        // ==========================================
        // 💡 [분노/각성 추가] 게이지 및 스킬 처리
        // ==========================================
        if (isAwakened)
        {
            // 각성 중: 부스트 100% 고정, 타이머 차감
            currentBoostGauge = maxBoostGauge;
            isBoosting = wantsToBoost && !isSlowedDown; // 쉬프트 누르면 속도 효과 적용

            awakenTimer -= Time.deltaTime;
            if (awakenTimer <= 0)
            {
                isAwakened = false;
                currentRageGauge = 0f; // 종료 시 분노 게이지 초기화
                Debug.Log("각성 상태 종료!");
            }
        }
        else
        {
            // 평상시: 기존 부스트 로직
            isBoosting = wantsToBoost && currentBoostGauge > 0 && !isSlowedDown;
            
            if (isBoosting)
            {
                currentBoostGauge -= boostConsumeRate * Time.deltaTime;
                if (currentBoostGauge < 0) currentBoostGauge = 0;
            }
            else
            {
                currentBoostGauge += boostRechargeRate * Time.deltaTime;
                if (currentBoostGauge > maxBoostGauge) currentBoostGauge = maxBoostGauge;
            }
        }
        // ==========================================

        if (isSlowedDown)
        {
            collisionSlowdownTimer -= Time.deltaTime;
            if (collisionSlowdownTimer <= 0) isSlowedDown = false;
        }
        if (awakenEffectObject != null)
        {
            // 현재 오브젝트의 켜짐/꺼짐 상태가 isAwakened 상태와 다를 때만 SetActive 실행 (성능 최적화)
            if (awakenEffectObject.activeSelf != isAwakened)
            {
                awakenEffectObject.SetActive(isAwakened);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space)) TryJump();
    }

    void UpdateCameraFOV()
    {
        if (playerCamera == null) return;
        float targetFOV = isBoosting ? boostFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        CheckGrounded();
        if (animator != null) animator.SetBool(IsGroundedBool, isGrounded);
        
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.S)) moveInput = 1f;
        if (Input.GetKey(KeyCode.W)) moveInput = -1f;
        
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;
        
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            // ==========================================
            // 💡 [분노/각성 추가] 이동 속도 배율 처리
            // ==========================================
            float speedMultiplier = 1f;
            
            // 각성 상태면 기본 속도 1.5배 증가
            if (isAwakened) speedMultiplier *= awakenSpeedMultiplier; 
            
            // 부스트 중이면 부스트 배율 곱하기 (각성 중 부스트 시 속도 엄청 빨라짐)
            if (isBoosting) speedMultiplier *= boostSpeedMultiplier; 
            
            // 충돌 감속 처리
            if (isSlowedDown) speedMultiplier *= collisionSlowdownMultiplier;
            
            float currentAcceleration = acceleration * speedMultiplier;
            float currentMaxSpeed = moveSpeed * speedMultiplier;

            Vector3 forward = transform.forward;
            forward.y = 0; 
            forward.Normalize(); 

            Vector3 moveForce = forward * moveInput * currentAcceleration * rb.mass;
            rb.AddForce(moveForce, ForceMode.Force);
            
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > currentMaxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * currentMaxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }
        
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float rotation = turnInput * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, rotation, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }
    
    void CheckGrounded()
    {
        Vector3 origin = transform.position + groundCheckOffset;
        RaycastHit hit;
        isGrounded = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance);
    }
    
    public void TryJump()
    {
        if (isGrounded && Time.time >= lastJumpTime + jumpCooldown)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            rb.linearVelocity = vel;
            
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            lastJumpTime = Time.time;
            if (animator != null) animator.SetTrigger(JumpTrigger);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag(obstacleTag))
        {
            float collisionSpeed = collision.relativeVelocity.magnitude;
            
            if (collisionSpeed >= minCollisionSpeed)
            {
                float speedRatio = Mathf.Clamp01(collisionSpeed / (moveSpeed * boostSpeedMultiplier));
                float gaugeLoss = maxBoostGauge * maxGaugeLossPercent * speedRatio;
                
                // 각성 상태가 아닐 때만 부스트 게이지 깎임
                if (!isAwakened) 
                {
                    currentBoostGauge -= gaugeLoss;
                    if (currentBoostGauge < 0) currentBoostGauge = 0;
                }
                
                isSlowedDown = true;
                collisionSlowdownTimer = collisionSlowdownDuration;
                rb.linearVelocity *= collisionSlowdownMultiplier;
            }
        }
    }

    // ==========================================
    // 💡 [분노/각성 추가] Trigger, 로직, 스킬 함수
    // ==========================================
    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        // 분노 아이템 획득 처리
        if (other.CompareTag(ragePointTag))
        {
            if (!isAwakened)
            {
                currentRageGauge += ragePerItem; // 5% 증가
                
                if (currentRageGauge >= maxRageGauge)
                {
                    StartAwaken();
                }
            }

            // 아이템 파괴 (로컬에서 끄거나, ServerRpc로 완전 파괴)
            // 맵에 뿌려진 아이템이 NetworkObject라면 ServerRpc를 호출해 파괴해야 모든 사람 화면에서 사라집니다.
            other.gameObject.SetActive(false); 
        }
    }

    private void StartAwaken()
    {
        currentRageGauge = maxRageGauge;
        isAwakened = true;
        awakenTimer = awakenDuration;
        currentBoostGauge = maxBoostGauge; // 즉시 부스트 풀충전
        
        Debug.Log("각성 상태 돌입! 20초간 기본 속도 증가 & 스킬 사용 가능");
    }
}
