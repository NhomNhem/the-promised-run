using System.Collections;
using UnityEngine;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.States {
    /// <summary>
    /// Parry state — GDD §3.2 + §4.1.
    /// 5f active window. On success: counter ×2.0, enemy stun 1.5s, +30 OL.
    /// On miss: 20f recovery, 1s lockout.
    /// </summary>
    public class ParryState : BaseState {
        private const string ParryAnim    = "Parry";
        private const string CounterAnim  = "Counter";
        private const float  BlendTime    = 0.05f;

        private const float  ParryWindow  = 5f / 60f;   // 5 frames
        private const float  MissRecovery = 20f / 60f;  // 20 frames
        private const float  MissLockout  = 1.0f;
        private const float  CounterTime  = 0.5f;       // counter animation duration

        private float _timer;
        private bool  _parrySuccess;
        private bool  _parryChecked;

        public bool CanExit { get; private set; }

        // Lockout timer — shared via PlayerController
        private float _lockoutTimer;
        public bool IsLockedOut => _lockoutTimer > 0f;

        public ParryState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        public override void OnEnter() {
            base.OnEnter();
            _timer        = 0f;
            _parrySuccess = false;
            _parryChecked = false;
            CanExit       = false;

            if (_animator.HasState(0, Animator.StringToHash(ParryAnim)))
                _animator.CrossFade(ParryAnim, BlendTime, 0);
        }

        public override void OnUpdate() {
            base.OnUpdate();
            _timer += Time.deltaTime;

            if (_lockoutTimer > 0f) _lockoutTimer -= Time.deltaTime;

            if (!_parryChecked && _timer <= ParryWindow) {
                // Check for incoming attack during parry window
                if (IncomingAttackDetected()) {
                    _parryChecked = true;
                    _parrySuccess = true;
                    _playerController.StartCoroutine(ParrySuccessRoutine());
                }
            }

            // Window closed without success → miss
            if (!_parryChecked && _timer > ParryWindow) {
                _parryChecked = true;
                _parrySuccess = false;
                _playerController.StartCoroutine(ParryMissRoutine());
            }
        }

        public override void OnExit() {
            base.OnExit();
        }

        private bool IncomingAttackDetected() {
            // Check for enemies in front that are attacking
            var enemies = _playerController.GetEnemiesInRange();
            foreach (var enemy in enemies) {
                if (enemy == null) continue;
                var enemyComp = enemy.GetComponent<Enemy.Enemy>();
                if (enemyComp != null && enemyComp.IsAttacking) return true;
            }
            return false;
        }

        private IEnumerator ParrySuccessRoutine() {
            // Hitstop 8f
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(8f / 60f);
            Time.timeScale = 1f;

            // +30 OL gauge
            _playerController.AddChaos(30f, ChaosSource.Manual);

            // Stun nearest enemy 1.5s
            var nearest = _playerController.GetNearestEnemy();
            if (nearest != null) {
                var ai = nearest.GetComponent<Enemy.AI.EnemyAIController>();
                ai?.ChangeState(Core.Interfaces.EnemyAIState.Stunned);
            }

            // Counter animation
            if (_animator.HasState(0, Animator.StringToHash(CounterAnim)))
                _animator.CrossFade(CounterAnim, BlendTime, 0);

            yield return new WaitForSeconds(CounterTime);
            CanExit = true;
        }

        private IEnumerator ParryMissRoutine() {
            // 20f recovery locked
            yield return new WaitForSeconds(MissRecovery);
            _lockoutTimer = MissLockout;
            CanExit = true;
        }
    }
}
