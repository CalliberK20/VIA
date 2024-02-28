using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Stats", menuName = "Scriptable Objects/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Base Stats")]
    public float Speed = 1f;
    public EnemyState DefaultState;
    public float StoppingDistance = 0f;
    public float UpdateSpeed = 0.1f;

    [Space()]

    [Header("Wander State")]
    public float MaxWanderDistance = 4f;

    [Header("Combat State")]
    [Tooltip("Tags that this enemy will be aggressive against")]
    public List<string> AggroTags;
    [Tooltip("Minimum player range to aggro this enemy")]
    public float AggroRange= 4f;
    [Tooltip("Maximum distance to aggro nearby enemies")]
    public float AlertRange = 4f;
    [Tooltip("Agent stopping distance for combat")]
    public float CombatStoppingDistance = 2f;
    [Tooltip("Agent movement speed multiplier during combat")]
    public float CombatSpeedMultiplier = 1.2f;

    [Space()]

    [Header("Pack Behavior")]
    [Tooltip("Minimum distance from other enemies")]
    public float MinDistanceFromEnemy = 1.5f;
    [Tooltip("Determines if this Enemy is a pack Alpha")]
    public bool IsAlpha;
    [Tooltip("(Alpha Only) Maximum distance members of the pack can be from the Alpha")]
    public float MaxDistanceFromAlpha = 3f;
    [Tooltip("Minimum distance needed to break off from reset state")]
    public float MinimumResetDistance = 1f;

    
}
