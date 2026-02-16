using UnityEngine;

// 덤프트럭의 흙 유도탄
public class DirtProjectile : MonoBehaviour
{
    private GameObject target;
    private float speed;
    private float slowPercent;
    private bool hasHit = false;
    
    public void Initialize(GameObject targetPlayer, float projectileSpeed, float slowPercentage)
    {
        target = targetPlayer;
        speed = projectileSpeed;
        slowPercent = slowPercentage;
        
        Destroy(gameObject, 5f);
    }
    
    void Update()
    {
        if (hasHit || target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        transform.rotation = Quaternion.LookRotation(direction);
        
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance < 1f)
        {
            HitTarget();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        if (other.gameObject == target)
        {
            HitTarget();
        }
    }
    
    void HitTarget()
    {
        hasHit = true;
        
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.linearVelocity *= (1f - slowPercent);
            Debug.Log($"{target.name}의 속도 {slowPercent * 100}% 감소!");
        }
        
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }
}
