using UnityEngine;


public class RagePoint : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        RageGaugeManager rageManager = other.GetComponent<RageGaugeManager>();
        if (rageManager != null)
        {
            rageManager.AddRage(5f); 
            Destroy(gameObject);
            Debug.Log($"{other.name}이(가) 분노 포인트 획득!");
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}

