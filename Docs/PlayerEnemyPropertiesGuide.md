# Player and Enemy Properties System Guide

## Overview

The Player and Enemy Properties system uses Scriptable Objects with Odin Inspector to provide a flexible, data-driven approach to character configuration. This system allows for easy balancing, multiple character types, and runtime property modifications.

## Architecture

### Core Components

1. **PlayerProperties.cs** - ScriptableObject containing all player stats
2. **EnemyProperties.cs** - ScriptableObject containing all enemy stats
3. **Property Management Methods** - Runtime property modification system
4. **Example Configurations** - Pre-balanced character types

### Key Benefits

- **Data-Driven Design**: All stats stored in Scriptable Objects
- **Easy Balancing**: Modify stats without code changes
- **Multiple Character Types**: Create different configurations
- **Runtime Modifications**: Change properties during gameplay
- **Odin Inspector Integration**: Visual editing experience
- **Validation**: Automatic range checking and validation

## Setup Guide

### 1. Basic Setup

```csharp
// Add to Player GameObject:
- PlayerController (existing)
- PlayerProperties (ScriptableObject reference)

// Add to Enemy GameObject:
- Enemy (existing) 
- EnemyProperties (ScriptableObject reference)
- EnemyAIControllerNavMesh (for AI)
```

### 2. Create Property Assets

1. **Player Properties**:
   - Right-click in Project window → Create → The Promised Run → Player → Player Properties
   - Configure movement, combat, health, and audio settings

2. **Enemy Properties**:
   - Right-click → Create → The Promised Run → Enemy → Enemy Properties
   - Configure combat, movement, health, and AI settings

### 3. Assign Properties

```csharp
// In PlayerController:
public void ApplyPlayerProperties(PlayerProperties properties) {
    properties.ApplyToPlayer(this);
    playerProperties = properties;
}

// In Enemy:
public void ApplyEnemyProperties(EnemyProperties properties) {
    properties.ApplyToEnemy(this);
    enemyProperties = properties;
}
```

## Property Categories

### Player Properties

#### Movement Settings
- **moveSpeed**: Base movement speed
- **jumpForce**: Jump height/force
- **rotationSpeed**: Turn speed
- **fallGravityMultiplier**: Gravity multiplier when falling
- **airControl**: Air movement control (0-1)

#### Health Settings
- **maxHealth**: Maximum health points
- **healthRegenRate**: Health regeneration per second
- **healthRegenDelay**: Delay before regen starts

#### Combat Settings
- **attackCooldown**: Time between attacks
- **comboWindow**: Time window for combo attacks
- **maxComboCount**: Maximum combo hits
- **damageMultiplier**: Damage output multiplier

#### System Overload Settings
- **overloadDuration**: How long overload lasts
- **overloadCooldown**: Cooldown between overloads
- **maxChaosThreshold**: Chaos needed for overload
- **chaosDecayRate**: Chaos reduction rate
- **chaosPerHit**: Chaos gained per attack

#### Detection Settings
- **enemyDetectionRadius**: Range for enemy detection
- **enemyDetectionAngle**: Detection angle in degrees

#### Juice Settings
- **landingImpactForce**: Landing feedback strength
- **jumpJuiceDuration**: Jump effect duration
- **attackJuiceDuration**: Attack effect duration
- **hurtJuiceDuration**: Hurt effect duration

#### Audio Settings
- **footstepVolume**: Footstep sound volume
- **jumpVolume**: Jump sound volume
- **attackVolume**: Attack sound volume
- **hurtVolume**: Hurt sound volume

### Enemy Properties

#### Combat Stats
- **baseDamage**: Base attack damage
- **attackRange**: Attack range
- **attackCooldown**: Time between attacks
- **attackWindupTime**: Attack preparation time
- **attackRecoveryTime**: Attack recovery time

#### Movement Stats
- **moveSpeed**: Movement speed
- **rotationSpeed**: Turn speed
- **acceleration**: Acceleration rate
- **deceleration**: Deceleration rate
- **maxSpeed**: Maximum movement speed

#### Health Stats
- **maxHealth**: Maximum health points
- **healthRegenRate**: Health regeneration rate
- **healthRegenDelay**: Regeneration delay

#### Defense Stats
- **damageResistance**: Damage reduction multiplier
- **knockbackResistance**: Knockback reduction
- **stunResistance**: Stun duration reduction

#### Detection Stats
- **detectionRadius**: Target detection range
- **detectionAngle**: Detection angle in degrees
- **targetUpdateInterval**: Target check frequency
- **loseTargetTime**: Time before losing target

#### AI Behavior
- **patrolRadius**: Patrol area size
- **patrolSpeed**: Movement speed while patrolling
- **patrolWaitTime**: Wait time at patrol points
- **chaseSpeed**: Movement speed while chasing
- **chaseUpdateInterval**: Chase update frequency

#### Audio Settings
- **footstepVolume**: Footstep sound volume
- **attackVolume**: Attack sound volume
- **hurtVolume**: Hurt sound volume
- **deathVolume**: Death sound volume

#### Visual Effects
- **hitFlashDuration**: Hit effect duration
- **deathFadeDuration**: Death fade duration
- **attackEffectScale**: Attack effect size

## Example Configurations

### Player Types

#### Standard Player
```csharp
moveSpeed: 8f
jumpForce: 12f
maxHealth: 100f
attackCooldown: 0.15f
comboWindow: 0.6f
maxComboCount: 3
```

#### Agile Player
```csharp
moveSpeed: 12f
jumpForce: 15f
maxHealth: 75f
attackCooldown: 0.1f
comboWindow: 0.8f
maxComboCount: 4
```

#### Tank Player
```csharp
moveSpeed: 5f
jumpForce: 8f
maxHealth: 150f
attackCooldown: 0.25f
comboWindow: 0.4f
maxComboCount: 2
```

### Enemy Types

#### Standard Enemy
```csharp
baseDamage: 15f
attackRange: 2f
moveSpeed: 5f
maxHealth: 50f
detectionRadius: 10f
```

#### Fast Enemy
```csharp
baseDamage: 10f
attackRange: 1.5f
moveSpeed: 8f
maxHealth: 30f
detectionRadius: 12f
```

#### Tank Enemy
```csharp
baseDamage: 25f
attackRange: 3f
moveSpeed: 3f
maxHealth: 100f
detectionRadius: 8f
```

#### Ranged Enemy
```csharp
baseDamage: 20f
attackRange: 15f
moveSpeed: 4f
maxHealth: 40f
detectionRadius: 20f
```

## Runtime Property Modification

### Player Property Methods

```csharp
// Get current properties
PlayerProperties props = playerController.GetPlayerProperties();

// Set new properties
playerController.SetPlayerProperties(newProperties);

// Modify individual properties
playerController.SetMoveSpeed(10f);
playerController.SetJumpForce(15f);
playerController.SetMaxHealth(120f);
```

### Enemy Property Methods

```csharp
// Get current properties
EnemyProperties props = enemy.GetEnemyProperties();

// Set new properties
enemy.SetEnemyProperties(newProperties);

// Modify individual properties
enemy.SetBaseDamage(20f);
enemy.SetMoveSpeed(7f);
enemy.SetMaxHealth(75f);
```

## Odin Inspector Integration

### Visual Editing Features

- **Grouped Properties**: Organized by category
- **Range Sliders**: Min/max value constraints
- **Tooltips**: Descriptive information
- **Validation**: Automatic range checking
- **Reset Options**: Reset to default values

### Custom Attributes

```csharp
[Header("Movement Settings")]
[Min(0.1f)]
public float moveSpeed = 8f;

[Range(0f, 1f)]
public float airControl = 0.5f;

[Tooltip("Maximum health points")]
public float maxHealth = 100f;
```

## Balancing Guidelines

### Player Balance

1. **Speed vs Health**: Faster players should have less health
2. **Damage vs Speed**: Higher damage should have slower attacks
3. **Combo vs Damage**: More combo hits should have lower per-hit damage
4. **Chaos vs Power**: Higher chaos threshold should have more power

### Enemy Balance

1. **Damage vs Health**: Higher damage enemies should have less health
2. **Speed vs Detection**: Faster enemies should have limited detection
3. **Range vs Health**: Ranged enemies should have less health
4. **AI Complexity**: Smarter AI should have lower stats

## Performance Considerations

### ScriptableObject Usage

- **Reference Counting**: Properties are shared, not copied
- **Memory Efficiency**: One asset per character type
- **Runtime Changes**: Modifying properties affects all instances

### Best Practices

1. **Pool Common Properties**: Reuse properties for similar enemies
2. **Validate Changes**: Use validation to prevent invalid values
3. **Batch Updates**: Update multiple properties together
4. **Event Integration**: Connect property changes to events

## Troubleshooting

### Common Issues

1. **Properties Not Applied**: Ensure ApplyToPlayer/ApplyToEnemy is called
2. **Invalid Values**: Check validation ranges in OnValidate
3. **Missing References**: Verify ScriptableObject assignments
4. **Runtime Changes**: Use setter methods for runtime modifications

### Debug Tools

```csharp
// Enable debug visualization
showDebugInfo = true;
showDetectionGizmos = true;
showMovementGizmos = true;
```

## Migration Guide

### From Hardcoded Values

1. **Create Property Assets**: Make ScriptableObjects for existing values
2. **Update Controllers**: Add property references and apply methods
3. **Replace Direct Access**: Use property getters instead of direct field access
4. **Test Thoroughly**: Verify all behaviors work with new system

### Example Migration

```csharp
// Before (hardcoded)
[SerializeField] private float moveSpeed = 8f;

// After (ScriptableObject)
[SerializeField] private PlayerProperties playerProperties;
public float MoveSpeed => moveSpeed; // Still works for compatibility

public void ApplyProperties() {
    playerProperties.ApplyToPlayer(this);
}
```

## Future Enhancements

### Planned Features

1. **Property Inheritance**: Base properties with specialized overrides
2. **Runtime Property Editors**: In-game property modification UI
3. **Property Templates**: Reusable property configurations
4. **Auto-Balancing**: AI-assisted property balancing
5. **Property Analytics**: Usage statistics and balance metrics

### Extension Points

- **Custom Properties**: Add new property categories
- **Validation Rules**: Custom validation logic
- **Property Effects**: Visual/audio effects based on properties
- **Property Events**: Events for property changes

## Best Practices

1. **Use Descriptive Names**: Clear property and asset names
2. **Document Ranges**: Include tooltips explaining value ranges
3. **Version Control**: Track property changes in version control
4. **Test Extremes**: Test with min/max property values
5. **Profile Performance**: Monitor property system performance

This system provides a robust foundation for character configuration and balancing in The Promised Run.
