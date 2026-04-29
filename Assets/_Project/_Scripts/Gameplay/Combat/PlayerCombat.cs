using UnityEngine;
using ThePromisedRun.Core.Interfaces;
using UnityEngine.Events;

namespace ThePromisedRun.Gameplay.Combat {
    /// <summary>
    /// Player combat component - separated for Single Responsibility Principle
    /// Handles attack combos, hitbox activation, and damage application
    /// </summary>
    public class PlayerCombat : MonoBehaviour {
        [Header("Combat Settings")]
        [SerializeField] private float attackCooldown = 0.15f;
        [SerializeField] private float comboWindow = 0.6f;
        [SerializeField] private int maxComboCount = 3;
        [SerializeField] private float baseDamage = 10f;
        
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private AttackHitbox attackHitbox;
        
        private float _attackCooldownTimer;
        private float _comboTimer;
        private int _comboIndex;
        private bool _isAttacking;
        
        private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");
        private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");
        
        public bool CanAttack => _attackCooldownTimer <= 0f;
        public int ComboIndex => _comboIndex;
        public float BaseDamage => baseDamage;
        
        public UnityEvent OnAttackInput { get; } = new UnityEvent();
        public UnityEvent<int> OnComboAdvance { get; } = new UnityEvent<int>();
        
        private void Update() {
            HandleAttackTimers();
        }
        
        public void PerformAttack() {
            if (!CanAttack) return;
            
            _attackCooldownTimer = attackCooldown;
            _comboIndex = _comboIndex >= maxComboCount ? 1 : _comboIndex + 1;
            _comboTimer = comboWindow;
            
            animator.SetInteger(ComboIndexHash, _comboIndex);
            animator.SetTrigger(AttackTriggerHash);
            
            OnComboAdvance.Invoke(_comboIndex);
            OnAttackInput.Invoke();
        }
        
        private void HandleAttackTimers() {
            if (_attackCooldownTimer > 0f) {
                _attackCooldownTimer -= Time.deltaTime;
            }
            
            if (_comboTimer > 0f) {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f) {
                    ResetCombo();
                }
            }
        }
        
        private void ResetCombo() {
            _comboIndex = 0;
            animator.SetInteger(ComboIndexHash, 0);
        }
        
        public void ActivateHitbox() {
            attackHitbox?.Activate();
        }
        
        public void DeactivateHitbox() {
            attackHitbox?.Deactivate();
        }
    }
}