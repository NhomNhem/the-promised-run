using System.Collections;
using UnityEngine;
using ThePromisedRun.Gameplay.Combat;

namespace ThePromisedRun.Gameplay.States {
    /// <summary>
    /// Hades-style combo attack:
    /// - Input buffer always active (press anytime, chains when ready)
    /// - Chain available after startup window (no recovery lock)
    /// - Each hit has: startup → chainable → finish
    /// - Dash/jump cancel available anytime
    ///
    /// Clip durations: Attack_1=1.17s, Attack_2=0.93s, Attack_3=1.37s
    /// Startup fraction: 0.25 (chain available after 25% of clip)
    /// </summary>
    public class AttackState : BaseState {
        #region Config
        private static readonly string[] Clips = { "Attack_1", "Attack_2", "Attack_3" };
        private const int AttackLayer = 1;
        private const int MaxCombo    = 3;
        #endregion

        #region State
        private int   _comboIndex;
        private float _hitTimer;
        private float _clipDuration;
        private bool  _inputBuffered;   // true = player pressed attack, waiting to chain
        private bool  _chained;         // true = already chained to next hit this cycle
        private bool  _finishing;       // true = last hit done, counting down FinishDelay
        private float _finishTimer;
        #endregion

        public bool CanExit => _finishing && _finishTimer >= _playerController.ComboFinishDelay;

        public AttackState(PlayerController playerController, Animator animator)
            : base(playerController, animator) { }

        #region IState

        public override void OnEnter() {
            base.OnEnter();
            _comboIndex    = 0;
            _inputBuffered = false;
            _chained       = false;
            _finishing     = false;
            _finishTimer   = 0f;

            _animator.SetLayerWeight(AttackLayer, 1f);
            _playerController.Input.ConsumeAttackInput();
            ExecuteHit();
        }

        public override void OnUpdate() {
            base.OnUpdate();

            _hitTimer += Time.deltaTime;

            // Always update locomotion params so Base Layer (legs) reflects actual movement
            Vector2 moveInput = _playerController.Input != null
                ? _playerController.Input.MoveInput
                : Vector2.zero;
            _animator.SetFloat("VelocityX", moveInput.x);
            _animator.SetFloat("VelocityZ", moveInput.magnitude);

            // Always buffer input — Hades style
            if (_playerController.Input.IsAttackPressed) {
                _inputBuffered = true;
                _playerController.Input.ConsumeAttackInput();
            }

            if (_finishing) {
                _finishTimer += Time.deltaTime;
                return;
            }

            float startupDone = _clipDuration * _playerController.ComboChainFraction;

            // Chain available after startup
            if (!_chained && _hitTimer >= startupDone) {
                if (_inputBuffered && _comboIndex < MaxCombo) {
                    _inputBuffered = false;
                    _chained       = true;
                    ExecuteHit();
                    return;
                }
            }

            // Clip finished with no chain → finish
            if (_hitTimer >= _clipDuration && !_chained) {
                _finishing   = true;
                _finishTimer = 0f;
            }
        }

        public override void OnFixedUpdate() {
            base.OnFixedUpdate();
            _playerController.ApplyMovement(_playerController.AttackMoveDamping);
        }

        public override void OnExit() {
            base.OnExit();
            _comboIndex = 0;
            _animator.SetLayerWeight(AttackLayer, 0f);
            // Don't reset VelocityX/Z here — LocomotionState.OnEnter will set correct values
            // Only stop horizontal drift if no input
            if (_playerController.Input.MoveInput.sqrMagnitude < 0.01f) {
                _playerController.Rb.linearVelocity = new Vector3(
                    0f, _playerController.Rb.linearVelocity.y, 0f);
            }
        }

        #endregion

        #region Animation Event Receivers

        public void OnComboWindowOpen()  { }
        public void OnComboWindowClose() { }

        public void OnHitConfirmed() {
            _playerController.Juice?.OnAttackHit();
            if (_playerController.HitStopDuration > 0f)
                _playerController.StartCoroutine(
                    HitStop(_playerController.HitStopDuration, _playerController.HitStopTimeScale));
        }

        #endregion

        #region Private

        private void ExecuteHit() {
            _comboIndex   = Mathf.Clamp(_comboIndex + 1, 1, MaxCombo);
            _hitTimer     = 0f;
            float[] durations = _playerController.ComboClipDurations;
            _clipDuration = (durations != null && durations.Length >= _comboIndex)
                ? durations[_comboIndex - 1]
                : 1.0f;
            _chained      = false;
            _finishing    = false;
            _finishTimer  = 0f;

            _playerController.FaceNearestEnemyOrForward();
            _playerController.ApplyAttackStep(_comboIndex);
            _animator.CrossFade(Clips[_comboIndex - 1], _playerController.AttackBlendTime, AttackLayer, 0f);

            _playerController.Juice?.OnAttackSwing();
            _playerController.AddChaos(_playerController.ChaosPerHit, ChaosSource.Attack);
        }

        private static IEnumerator HitStop(float duration, float timeScale) {
            Time.timeScale = timeScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        #endregion
    }
}
