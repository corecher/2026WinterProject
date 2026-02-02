using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HeavyVehicleController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float acceleration = 100f; // 더 강한 가속력
    
    [Header("회전 설정")]
    [SerializeField] private float rotationSpeed = 120f;
    
    [Header("점프 설정")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float groundCheckDistance = 0.6f; // 거리 줄임
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0); // 시작점 조정
    
    [Header("물리 설정")]
    [SerializeField] private float mass = 1000f;
    [SerializeField] private float drag = 0.5f; // 드래그 줄임
    [SerializeField] private float angularDrag = 3f;
    
    // 내부 변수
    private Rigidbody rb;
    private bool isGrounded;
    private float lastJumpTime = -999f;
    private Collider col;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        
        // Rigidbody 설정
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.isKinematic = false;
        
        // 무게중심 낮추기
        rb.centerOfMass = new Vector3(0, -0.3f, 0);
        
        // Physics Material 생성 (마찰 최소화)
        PhysicsMaterial physicsMaterial = new PhysicsMaterial("LowFriction");
        physicsMaterial.dynamicFriction = 0.1f;
        physicsMaterial.staticFriction = 0.1f;
        physicsMaterial.bounciness = 0.1f;
        physicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        physicsMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
        
        // Collider에 Physics Material 적용
        if (col != null)
        {
            col.material = physicsMaterial;
        }
        
        Debug.Log("HeavyVehicleController 초기화 완료!");
        Debug.Log($"Rigidbody - Mass: {rb.mass}, Drag: {rb.linearDamping}, IsKinematic: {rb.isKinematic}");
    }
    
    void Update()
    {
        // 점프 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }
    }
    
    void FixedUpdate()
    {
        // 지면 체크
        CheckGrounded();
        
        // 이동 입력
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.W)) moveInput = 1f;
        if (Input.GetKey(KeyCode.S)) moveInput = -1f;
        
        // 회전 입력
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;
        
        // 이동 처리 - ForceMode.Force로 변경 (더 강력)
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            Vector3 moveForce = transform.forward * moveInput * acceleration * rb.mass;
            rb.AddForce(moveForce, ForceMode.Force);
            
            // 최대 속도 제한
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > moveSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
            
            Debug.Log($"이동 중! 힘: {moveForce.magnitude:F1}, 속도: {rb.linearVelocity.magnitude:F1}");
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
        
        // 아래쪽으로 레이캐스트
        RaycastHit hit;
        isGrounded = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance);
        
        // 디버그 레이
        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
        
        if (isGrounded)
        {
            Debug.DrawLine(origin, hit.point, Color.yellow);
        }
    }
    
    void TryJump()
    {
        Debug.Log($"점프 시도 - 지면: {isGrounded}, 쿨다운: {Time.time >= lastJumpTime + jumpCooldown}");
        
        if (isGrounded && Time.time >= lastJumpTime + jumpCooldown)
        {
            // Y축 속도 초기화
            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            rb.linearVelocity = vel;
            
            // 점프!
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            lastJumpTime = Time.time;
            
            Debug.Log($"점프 성공! 힘: {jumpForce}");
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        // 충돌 정보 확인
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.blue);
        }
    }
    
}