using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HeavyVehicleController : MonoBehaviour
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
    [SerializeField] private float boostRechargeRate = 8f; // 초당 충전량
    [SerializeField] private float boostConsumeRate = 25f; // 초당 소모량
    
    [Header("충돌 페널티 설정 (장애물 구현 시 사용)")]
    [SerializeField] private float collisionSlowdownDuration = 1.5f;
    [SerializeField] private float collisionSlowdownMultiplier = 0.5f;
    [SerializeField] private float maxGaugeLossPercent = 0.5f;
    [SerializeField] private float minCollisionSpeed = 5f;
    [SerializeField] private string obstacleTag = "Obstacle"; // 장애물 태그
    
    [Header("물리 설정")]
    [SerializeField] private float mass = 1000f;
    [SerializeField] private float drag = 0.5f;
    [SerializeField] private float angularDrag = 3f;
    
    private Rigidbody rb;
    private bool isGrounded;
    private float lastJumpTime = -999f;
    private Collider col;
    
    private float currentBoostGauge;
    private bool isBoosting;
    
    private float collisionSlowdownTimer = 0f;
    private bool isSlowedDown = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.isKinematic = false;
        
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
        
        currentBoostGauge = maxBoostGauge;
        
    }
    
    void Update()
    {
        bool wantsToBoost = Input.GetKey(KeyCode.LeftShift);
        
        isBoosting = wantsToBoost && currentBoostGauge > 0 && !isSlowedDown;
        
        if (isBoosting)
        {
            currentBoostGauge -= boostConsumeRate * Time.deltaTime;
            if (currentBoostGauge < 0)
            {
                currentBoostGauge = 0;
            }
        }
        else
        {
            // 부스트 미사용시 게이지 천천히 충전
            currentBoostGauge += boostRechargeRate * Time.deltaTime;
            if (currentBoostGauge > maxBoostGauge)
            {
                currentBoostGauge = maxBoostGauge;
            }
        }
        
        // 충돌 느려짐 타이머 (임시 아직 장애물 없음)
        if (isSlowedDown)
        {
            collisionSlowdownTimer -= Time.deltaTime;
            if (collisionSlowdownTimer <= 0)
            {
                isSlowedDown = false;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }
    }
    
    void FixedUpdate()
    {
        CheckGrounded();
        
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.W)) moveInput = 1f;
        if (Input.GetKey(KeyCode.S)) moveInput = -1f;
        
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;
        
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            float speedMultiplier = 1f;
            
            if (isBoosting)
            {
                speedMultiplier = boostSpeedMultiplier;
            }
            
            // 충돌 페널티 적용 (임시로 해둠 아직 장애물 없음)
            if (isSlowedDown)
            {
                speedMultiplier *= collisionSlowdownMultiplier;
            }
            
            float currentAcceleration = acceleration * speedMultiplier;
            float currentMaxSpeed = moveSpeed * speedMultiplier;
            
            Vector3 moveForce = transform.forward * moveInput * currentAcceleration * rb.mass;
            rb.AddForce(moveForce, ForceMode.Force);
            
            // 최대 속도 제한
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > currentMaxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * currentMaxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }
        
        // 회전 처리
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
            
            Debug.Log("점프!");
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // 장애물 태그 확인
        if (collision.gameObject.CompareTag(obstacleTag))
        {
            float collisionSpeed = collision.relativeVelocity.magnitude;
            
            // 일정 속도 이상으로 충돌했을 때만 페널티
            if (collisionSpeed >= minCollisionSpeed)
            {
                // 속도에 비례한 게이지 감소 (최대 50%)
                float speedRatio = Mathf.Clamp01(collisionSpeed / (moveSpeed * boostSpeedMultiplier));
                float gaugeLoss = maxBoostGauge * maxGaugeLossPercent * speedRatio;
                
                currentBoostGauge -= gaugeLoss;
                if (currentBoostGauge < 0)
                {
                    currentBoostGauge = 0;
                }
                
                // 속도 느려짐 효과
                isSlowedDown = true;
                collisionSlowdownTimer = collisionSlowdownDuration;
                
                // 속도 즉시 감소
                rb.linearVelocity *= collisionSlowdownMultiplier;
            }
        }
    }

    public float GetBoostPercent()
    {
        return currentBoostGauge / maxBoostGauge;
    }
    
    //디버깅 편하려고 GPT시킨 GUI
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 22;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        
        // 배경
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.Box(new Rect(5, 5, 550, 320), "");
        GUI.color = Color.white;
        
        // 속도 표시
        if (isBoosting)
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(10, 10, 550, 30), $"🔥 부스트 활성화! 속도: {rb.linearVelocity.magnitude:F2} m/s", style);
            style.normal.textColor = Color.white;
        }
        else if (isSlowedDown)
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(10, 10, 550, 30), $"💥 충돌 페널티! 속도: {rb.linearVelocity.magnitude:F2} m/s", style);
            style.normal.textColor = Color.white;
        }
        else
        {
            GUI.Label(new Rect(10, 10, 550, 30), $"속도: {rb.linearVelocity.magnitude:F2} m/s", style);
        }
        
        // 부스트 게이지 바
        float gaugePercent = currentBoostGauge / maxBoostGauge;
        
        GUI.Label(new Rect(10, 40, 550, 30), $"부스트 게이지: {currentBoostGauge:F1} / {maxBoostGauge}", style);
        
        // 게이지 바 배경
        GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        GUI.Box(new Rect(10, 70, 530, 30), "");
        
        // 게이지 바 (색상: 게이지 양에 따라 변경)
        if (gaugePercent > 0.5f)
            GUI.color = new Color(0, 1, 0, 0.8f); // 초록색
        else if (gaugePercent > 0.25f)
            GUI.color = new Color(1, 1, 0, 0.8f); // 노란색
        else
            GUI.color = new Color(1, 0, 0, 0.8f); // 빨간색
        
        GUI.Box(new Rect(10, 70, 530 * gaugePercent, 30), "");
        GUI.color = Color.white;
        
        // 게이지 상태 텍스트
        string gaugeStatus = "";
        if (isBoosting)
            gaugeStatus = "⚡ 소모 중";
        else if (currentBoostGauge >= maxBoostGauge)
            gaugeStatus = "✓ 충전 완료";
        else
            gaugeStatus = "⟳ 충전 중...";
        
        GUI.Label(new Rect(10, 105, 550, 30), gaugeStatus, style);
        
        // 기타 정보
        GUI.Label(new Rect(10, 135, 550, 30), $"지면: {(isGrounded ? "접촉 ✓" : "공중 ✗")}", style);
        GUI.Label(new Rect(10, 165, 550, 30), $"최대 속도: {(isBoosting ? moveSpeed * boostSpeedMultiplier : moveSpeed):F1} m/s", style);
        
        // 입력 상태
        string inputStatus = "";
        if (Input.GetKey(KeyCode.W)) inputStatus += "[W] ";
        if (Input.GetKey(KeyCode.S)) inputStatus += "[S] ";
        if (Input.GetKey(KeyCode.A)) inputStatus += "[A] ";
        if (Input.GetKey(KeyCode.D)) inputStatus += "[D] ";
        if (Input.GetKey(KeyCode.Space)) inputStatus += "[SPACE] ";
        if (Input.GetKey(KeyCode.LeftShift)) inputStatus += "[SHIFT 부스트] ";
        
        GUI.Label(new Rect(10, 195, 550, 30), $"입력: {(inputStatus.Length > 0 ? inputStatus : "없음")}", style);
    }
}