# AGENTS.md - Agentic Coding Guidelines for The Promised Run

This document provides guidelines for AI agents working on this Unity 2D game project.

---

## 1. Build & Development Commands

### Unity Editor
- Open the project in Unity Hub. Use the project's configured editor version (see `ProjectSettings/ProjectVersion.txt`: `m_EditorVersion: 6000.3.11f1`) — install the matching editor via Unity Hub when possible.
- Press **Play** in Editor to build and run
- Use **Ctrl+B** (Cmd+B on Mac) to build standalone player
  
- Project includes an MCP (Model Context Protocol) integration for editor automation: see `Packages/manifest.json` entry `com.coplaydev.unity-mcp` (MCP for Unity, repo: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`).
- The repository also contains Editor-side helpers for MCP automation at `Assets/Editor/MCP/MCPSpawnHelpers.cs` (Menu: `MCP/Spawn Unique Enemy`). Agents can call `MCPSpawnHelpers.SpawnUniqueByPrefabPath(prefabAssetPath, position, uniqueKey)` from an MCP server or editor script to spawn prefabs without creating duplicates.

### Running Tests
- Tests use Unity Test Framework (`com.unity.test-framework`)
- Open **Test Runner** window: Window > General > Test Runner
- Click **Run All** to run all tests
- Click **Run Selected** to run specific test fixture
- Note: No command-line test runner; tests run inside Unity Editor only

### Build from Command Line
```bash
# Windows - build standalone
Unity.exe -projectPath "I:\unityVers\the-promised-run" -buildTarget Win64 -buildPlayer "Build\Windows\game.exe" -quit

# Alternative: use Unity's batchmode for CI
Unity.exe -batchmode -projectPath "I:\unityVers\the-promised-run" -executeMethod BuildCommand.BuildPlayer -quit
```

---

## 2. Code Style Guidelines

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `PlayerController`, `StateMachine` |
| Methods | PascalCase | `ApplyMovement()`, `SetupStateMachine()` |
| Properties | PascalCase | `public float MoveSpeed { get; private set; }` |
| Private Fields | _camelCase | `_stateMachine`, `_playerController` |
| Constants | PascalCase | `MaxJumpHeight = 10f` |
| Interfaces | I + PascalCase | `IState`, `ITransition`, `IPredicate` |

### File Organization
- **One class per file**: File name must match class name exactly
- **Namespace structure**: `ThePromisedRun.<Module>.<SubModule>`
- **Folder-to-namespace mapping**: `Assets/_Project/_Scripts/Gameplay/States/` → `ThePromisedRun.Gameplay.States`

### Using Directives
- Place `using` statements at top of file, no blank lines between them
- Group Unity namespaces first, then project namespaces:
```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using ThePromisedRun.Core.FSM;
using ThePromisedRun.Gameplay.States;
```

### Unity-Specific Patterns
- **SerializeField** for private fields that need inspector access
- **Properties with backing fields** for read-only public access:
  ```csharp
  public Rigidbody Rb { get; private set; }
  ```
- **[Header]** for grouping inspector fields:
  ```csharp
  [Header("Movement Settings")]
  [SerializeField] private float moveSpeed = 8f;
  ```
- **Region blocks** for organizing code sections:
  ```csharp
  #region Actions
  
  #endregion
  ```

### Type Guidelines
- Use **explicit types** over `var` except when type is obvious from assignment
- Use **Unity's new Input System** (`UnityEngine.InputSystem`), not legacy Input
- Use modern Unity physics: `Rb.linearVelocity` (Unity 2020+), not `Rb.velocity`
- Prefer **readonly** for immutable fields

### Error Handling
- Use `Debug.Log()` for runtime information
- Use `Debug.LogWarning()` for non-critical issues
- Use `Debug.LogError()` for critical failures
- Throw exceptions for unexpected state; don't silently fail

### Unity Lifecycle
- Use `Awake()` for initialization and getting component references
- Use `OnEnable()`/`OnDisable()` for enabling/disabling systems
- Use `Update()` for per-frame logic
- Use `FixedUpdate()` for physics calculations

---

## 3. Git Workflow

Follow the rules in `Docs/GitGuidelines.md`:

### Branch Naming
```
<type>/<module>/<description>
```
- **Types**: `feat`, `fix`, `refactor`, `docs`, `chore`
- **Modules**: `UI`, `Gameplay`, `Core`, `Sound`, `VFX`, etc.

Examples:
- `feat/UI/main-menu`
- `fix/Gameplay/character-jump`
- `refactor/Core/fsm-optimization`

### Commit Messages (Conventional Commits)
```
<type>(<scope>): <description>
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`

Examples:
- `feat(Gameplay): add player jump mechanic`
- `fix(Core): resolve state machine null reference`
- `docs(README): update installation instructions`

---

## 4. Project Structure

```
Assets/
├── _Project/
│   ├── _Scripts/
│   │   ├── Core/
│   │   │   └── FSM/           # State machine implementation
│   │   ├── Gameplay/
│   │   │   ├── Input/        # Input handling
│   │   │   ├── States/       # Player state implementations
│   │   │   └── PlayerController.cs
│   │   └── UI/
│   ├── Art/
│   ├── Audio/
│   └── Scenes/
├── Plugins/                  # Third-party assets
└── Packages/                  # Package dependencies
```

---

## 5. Important Dependencies

- Use the project's configured Unity Editor (see `ProjectSettings/ProjectVersion.txt` — `m_EditorVersion: 6000.3.11f1`)
- **com.unity.inputsystem** - New Input System
- **com.unity.test-framework** - Unity Test Framework
- **com.unity.render-pipelines.universal** - URP rendering
 - **com.coplaydev.unity-mcp** - MCP for Unity (project has an entry in `Packages/manifest.json`; package URL: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`) — enables programmatic Editor automation and is used together with the editor helpers in `Assets/Editor/MCP/`.

---

## 6. Key Patterns Used

### State Machine (FSM)
- Uses custom FSM implementation in `ThePromisedRun.Core.FSM`
- States implement `IState` interface
- Transitions use `IPredicate` for conditions
- Use `FuncPredicate` for lambda-based conditions

### Player Controller
- Manages Rigidbody physics
- Uses State Machine for behavior
- Input handled via `InputReader` component

---

## 7. Testing Guidelines

- Place test files in `Assets/Tests/` folder
- Use NUnit test attributes: `[Test]`, `[SetUp]`, `[TearDown]`
- Use Unity's `UnityTest` attribute for playmode tests
- Run tests via Unity Editor Test Runner window only

---

## 8. Prohibited Practices

- **Do not** use legacy `Input.GetAxis()` - use Input System
- **Do not** use `rb.velocity` - use `rb.linearVelocity` (Unity 2020+)
- **Do not** create `MonoBehaviour` instances with `new` - use `AddComponent()`
- **Do not** hardcode magic numbers - use `[SerializeField]` fields or constants
- **Do not** commit secrets, API keys, or credentials to version control

---

*Last updated: April 2026*