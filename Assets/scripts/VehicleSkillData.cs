
using UnityEngine;

[CreateAssetMenu(fileName = "New Vehicle Skill", menuName = "Racing Game/Vehicle Skill")]
public class VehicleSkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName;
    public VehicleType vehicleType;
    
    [Header("쿨다운")]
    public float cooldownTime = 10f;
    
    [Header("포크레인 전용")]
    public float excavatorGrabRange = 5f;
    public float excavatorThrowForce = 20f;
    public float excavatorStunDuration = 3f;
    public float excavatorAnimationDelay = 0.5f;
    
    [Header("불도저 전용")]
    public float bulldozerShieldDuration = 5f;
    
    [Header("덤프트럭 전용")]
    public float dumpTruckDetectionRadius = 15f;
    public float dumpTruckProjectileSpeed = 15f;
    public float dumpTruckSlowPercent = 0.5f;
    public GameObject dirtProjectilePrefab;
}

public enum VehicleType
{
    Excavator,    
    Bulldozer,  
    DumpTruck    
}
