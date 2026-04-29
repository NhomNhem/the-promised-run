# Enemy AI System Guide

## Overview

The Enemy AI System has been redesigned to use NavMesh for pathfinding and Scriptable Objects for configuration. This system provides better performance, flexibility, and easier AI behavior management using Odin Inspector.

## Architecture

### Core Components

1. **EnemyAINavMeshController** - Handles NavMesh-based movement and navigation
2. **EnemyAIControllerNavMesh** - Main AI controller with state management
3. **EnemyAnimationEvents** - Handles animation event callbacks
4. **Scriptable Objects** - Configuration and behavior definitions

### Scriptable Objects

- **EnemyAISettings** - Core AI configuration (movement, detection, combat)
- **EnemyAIBehavior** - Complex behavior patterns and decision trees
- **AICondition** - Reusable condition system for decision making

## Setup Guide

### 1. Basic Enemy Setup

```csharp
// Add these components to your Enemy GameObject:
- Enemy (entity component)
- EnemyAIControllerNavMesh (AI controller)
- EnemyAINavMeshController (NavMesh movement)
- NavMeshAgent (Unity component)
- EnemyAnimationEvents (animation event handler)
```

### 2. Configure AI Settings

1. Create AI Settings Asset:
   - Right-click in Project window → Create → The Promised Run → Enemy → AI Settings
   - Adjust movement, detection, and combat parameters

2. Create AI Behavior Asset:
   - Right-click → Create → The Promised Run → Enemy → AI Behavior
   - Configure state priorities and decision rules

### 3. Assign Components

```csharp
// In the EnemyAIControllerNavMesh component:
- Assign EnemyAISettings asset
- Assign EnemyAIBehavior asset
- Link Enemy and NavMeshController components
```

## AI States

The AI system uses the following states:

- **Idle** - Enemy waits and scans for targets
- **Patrol** - Enemy moves around designated area
- **Chase** - Enemy pursues detected targets
- **Attack** - Enemy attacks when in range
- **Stunned** - Enemy is temporarily disabled
- **Dead** - Enemy is dead and inactive

## State Transitions

State transitions are determined by:

1. **State Priorities** - Highest priority condition wins
2. **Decision Making** - Score-based decision system
3. **Response Rules** - Damage type responses
4. **Behavior Modifiers** - Dynamic property changes

## Configuration Examples

### Aggressive Melee Enemy

```csharp
// EnemyAISettings:
moveSpeed: 4f
detectionRadius: 8f
attackRange: 2f
attackCooldown: 1.2f
useNavMesh: true

// EnemyAIBehavior:
- High priority on Attack state when in range
- Medium priority on Chase state when target detected
- Low priority on Patrol when no target
```

### Ranged Enemy

```csharp
// EnemyAISettings:
moveSpeed: 3f
detectionRadius: 15f
attackRange: 12f
attackCooldown: 2f
navMeshStoppingDistance: 8f

// EnemyAIBehavior:
- Keep distance from targets
- Attack from range
- Retreat when too close
```

### Tank Enemy

```csharp
// EnemyAISettings:
moveSpeed: 2.5f
detectionRadius: 10f
attackRange: 3f
attackCooldown: 2.5f
loseTargetTime: 10f

// EnemyAIBehavior:
- Very persistent chasing
- High damage attacks
- Resistant to stunning
```

## Odin Inspector Integration

The system is designed to work with Odin Inspector for:

- **Visual AI Behavior Editing** - Drag-and-drop condition trees
- **State Priority Management** - Visual priority lists
- **Decision Score Tuning** - Real-time score adjustments
- **Behavior Modifiers** - Dynamic property changes

## Animation Events

### Required Events

Add these animation events to your attack animations:

1. **OnHitboxActivate** - Called when attack should start dealing damage
2. **OnHitboxDeactivate** - Called when attack should stop dealing damage

### Setup

1. Add `EnemyAnimationEvents` component to the visual GameObject
2. Ensure it's in the same hierarchy as the EnemyController
3. Animation events will automatically be forwarded to the EnemyController

## NavMesh Integration

### Requirements

1. **NavMesh Surface** - Bake NavMesh on your scene
2. **NavMesh Agent** - Added automatically by EnemyAINavMeshController
3. **Appropriate Areas** - Set up NavMesh areas for different movement types

### Configuration

```csharp
// In EnemyAISettings:
useNavMesh: true
navMeshStoppingDistance: 0.5f
navMeshUpdateInterval: 0.1f
```

## Performance Considerations

1. **Update Intervals** - Adjust NavMesh update frequency based on enemy count
2. **Detection Range** - Keep reasonable detection ranges
3. **State Update Frequency** - Balance between responsiveness and performance
4. **Behavior Complexity** - Keep decision trees simple for large enemy counts

## Debugging

### Debug Visualization

Enable debug options in AI Settings:

```csharp
showDebugInfo: true
showDetectionGizmos: true
showPatrolPath: true
```

### Console Logging

Enable decision logging in AI Controller:

```csharp
showDecisionLogs: true
```

## Troubleshooting

### Common Issues

1. **Animation Events Not Working**
   - Ensure EnemyAnimationEvents component is added
   - Check component hierarchy
   - Verify animation event names match

2. **NavMesh Not Working**
   - Ensure NavMesh is baked
   - Check NavMeshAgent configuration
   - Verify enemy is on NavMesh layer

3. **AI Not Responding**
   - Check AI Settings assignment
   - Verify Enemy component reference
   - Check if AI is enabled

4. **Performance Issues**
   - Reduce update intervals
   - Simplify AI behaviors
   - Optimize detection ranges

## Migration from Old System

To migrate from the old EnemyAIController:

1. Replace `EnemyAIController` with `EnemyAIControllerNavMesh`
2. Add `EnemyAINavMeshController` component
3. Create and assign Scriptable Object configurations
4. Update animation event handling
5. Test and adjust AI behaviors

## Best Practices

1. **Use Scriptable Objects** - Don't hardcode AI parameters
2. **Modular Design** - Create reusable AI behaviors
3. **Performance First** - Optimize for large enemy counts
4. **Debug Tools** - Use built-in debugging features
5. **Version Control** - Commit AI configuration assets

## Future Enhancements

- **Group AI** - Coordinated enemy behaviors
- **Learning AI** - Adaptive difficulty systems
- **Visual Scripting** - Node-based AI editor
- **Performance Profiling** - AI-specific performance tools
