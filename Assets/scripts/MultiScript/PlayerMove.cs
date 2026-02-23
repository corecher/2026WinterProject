using UnityEngine;
using Unity.Netcode; // 멀티플레이어 네임스페이스 추가

[RequireComponent(typeof(Rigidbody))]
// MonoBehaviour 대신 NetworkBehaviour를 상속받습니다.
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
    [SerializeField] private float maxBoostGauge = 100f;
    [SerializeField] private float boostRechargeRate = 8f; 
    [SerializeField] private float boostConsumeRate = 25f; 
    
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
    
    // 네트워크 변수 대신 로컬 변수로 유지 (이동 로직은 클라이언트 주도)
    private float currentBoostGauge;
    private bool isBoosting;
    
    private float collisionSlowdownTimer = 0f;
    private bool isSlowedDown = false;
    private bool isTimerAssigned = false;
    [Header("부스트 시야각(FOV) 설정")]
    [SerializeField] private float normalFOV = 60f;      // 기본 시야각
    [SerializeField] private float boostFOV = 80f;       // 부스트 시 시야각
    [SerializeField] private float fovChangeSpeed = 5f;  // FOV가 변하는 속도
    private Camera playerCamera;
    
    private Animator animator;
    private static readonly int JumpTrigger = Animator.StringToHash("Jump"); // 해시값을 사용해 성능 최적화
    private static readonly int IsGroundedBool = Animator.StringToHash("IsGrounded");

    // 카메라 추적을 위해 로컬 플레이어 확인용 이벤트 (선택 사항)
    public override void OnNetworkSpawn()
    {
        // 내 캐릭터라면 초기화
        if (IsOwner)
        {
            // 여기에 카메라 연결 로직 등을 넣을 수 있습니다.
            // 예: Camera.main.GetComponent<FollowCamera>().target = this.transform;
            currentBoostGauge = maxBoostGauge;
            TryFindTimer();
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();
        rb.mass = mass;
        // 멀티플레이어에서는 보간(Interpolate)이 켜져 있으면 다른 플레이어 움직임이 끊겨 보일 수 있으나
        // NetworkTransform 설정에 따라 다릅니다. 일단 유지합니다.
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        
        // 내 캐릭터가 아니면 물리에 의한 이동을 끄는 것이 좋습니다. (NetworkTransform이 위치를 잡아주므로)
        // 하지만 ClientNetworkTransform을 쓴다면 Kinematic을 끄면 안됩니다.
        // 일반적인 NetworkTransform 사용 시 아래 로직이 필요할 수 있습니다.
        // if (!IsOwner) rb.isKinematic = true; 

        rb.centerOfMass = new Vector3(0, -0.3f, 0);
        
        PhysicsMaterial physicsMaterial = new PhysicsMaterial("LowFriction");
        physicsMaterial.dynamicFriction = 0.1f;
        physicsMaterial.staticFriction = 0.1f;
        physicsMaterial.bounciness = 0.1f;
        physicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        physicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        
        if (col != null)
        {
            col.material = physicsMaterial;
        }
        if (IsOwner)
        { 
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null) playerCamera.fieldOfView = normalFOV;
        }
    }
    private void TryFindTimer()
    {
        // 1. 싱글톤 확인
        timer = NetworkGameTimer.Instance;

        // 2. 싱글톤에 없다면 씬의 모든 객체 중 검색 (비활성화된 객체 포함)
        if (timer == null)
        {
            timer = GameObject.FindFirstObjectByType<NetworkGameTimer>();
        }

        if (timer != null)
        {
            isTimerAssigned = true;
            Debug.Log($"[PlayerMove] 타이머 찾기 성공: {timer.name}");
        }
    }
    void Update()
    {
        // [중요] 내 캐릭터(IsOwner)가 아니면 입력을 받지 않습니다.
        if (!IsOwner) return;
        if (!isTimerAssigned)
        {
            TryFindTimer();
        }
        UpdateCameraFOV();
    // 타이머 체크 로직
    if (isTimerAssigned && timer != null)
    {
        if (!timer.CanMove) 
        {
            // 물리 정지 (카운트다운 중 밀림 방지)
            if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return; // 입력 처리 중단
        }
    }
        bool wantsToBoost = Input.GetKey(KeyCode.LeftShift);
        
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
        
        if (isSlowedDown)
        {
            collisionSlowdownTimer -= Time.deltaTime;
            if (collisionSlowdownTimer <= 0) isSlowedDown = false;
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }
    }
    void UpdateCameraFOV()
{
    if (playerCamera == null) return;

    // 부스터 사용 여부에 따라 목표 FOV 설정
    float targetFOV = isBoosting ? boostFOV : normalFOV;

    // Lerp를 사용하여 부드럽게 FOV 변경
    playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
}
    void FixedUpdate()
    {
        if (!IsOwner) return;

        CheckGrounded();
        if (animator != null)
        {
            animator.SetBool(IsGroundedBool, isGrounded);
        }
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.S)) moveInput = 1f;
        if (Input.GetKey(KeyCode.W)) moveInput = -1f;
        
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;
        
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            float speedMultiplier = 1f;
            if (isBoosting) speedMultiplier = boostSpeedMultiplier;
            if (isSlowedDown) speedMultiplier *= collisionSlowdownMultiplier;
            
            float currentAcceleration = acceleration * speedMultiplier;
            float currentMaxSpeed = moveSpeed * speedMultiplier;

            // 🔥 [핵심 수정] transform.forward에서 Y축 값을 제거하여 항상 지면과 평행한 방향을 구합니다.
            Vector3 forward = transform.forward;
            forward.y = 0; // 수직 성분 제거
            forward.Normalize(); // 방향만 남기기

            // 이제 캐릭터가 기울어져 있어도 항상 앞/뒤로만 힘이 가해집니다.
            Vector3 moveForce = forward * moveInput * currentAcceleration * rb.mass;
            rb.AddForce(moveForce, ForceMode.Force);
            
            // 속도 제한 로직
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > currentMaxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * currentMaxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }
        
        // 회전 로직
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float rotation = turnInput * rotationSpeed * Time.fixedDeltaTime;
            // Y축으로만 회전하도록 강제 (기울어짐 방지)
            Quaternion deltaRotation = Quaternion.Euler(0f, rotation, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }
    
    void CheckGrounded()
    {
        Vector3 origin = transform.position + groundCheckOffset;
        RaycastHit hit;
        isGrounded = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance);
        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }
    
    void TryJump()
    {
        if (isGrounded && Time.time >= lastJumpTime + jumpCooldown)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            rb.linearVelocity = vel;
            
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            lastJumpTime = Time.time;
            if (animator != null)
            {
                animator.SetTrigger(JumpTrigger);
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // [중요] 충돌 로직도 내 컴퓨터에서 일어난 것만 처리합니다.
        // 다른 클라이언트에서의 충돌은 위치 동기화로 해결됩니다.
        if (!IsOwner) return;

        if (collision.gameObject.CompareTag(obstacleTag))
        {
            float collisionSpeed = collision.relativeVelocity.magnitude;
            
            if (collisionSpeed >= minCollisionSpeed)
            {
                float speedRatio = Mathf.Clamp01(collisionSpeed / (moveSpeed * boostSpeedMultiplier));
                float gaugeLoss = maxBoostGauge * maxGaugeLossPercent * speedRatio;
                
                currentBoostGauge -= gaugeLoss;
                if (currentBoostGauge < 0) currentBoostGauge = 0;
                
                isSlowedDown = true;
                collisionSlowdownTimer = collisionSlowdownDuration;
                
                rb.linearVelocity *= collisionSlowdownMultiplier;
            }
        }
    }
}
