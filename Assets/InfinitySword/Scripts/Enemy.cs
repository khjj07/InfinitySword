using System;
using System.Drawing;
using System.Threading.Tasks;
using Assets.InfinitySword.Scripts.Manager;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using Color = System.Drawing.Color;

namespace Assets.InfinitySword.Scripts
{
    public class Enemy : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Walk,
            KnockBack,
            Attack
        }


        [SerializeField]
        private float _initialHp;
        private float _hp;
        [SerializeField]
        protected float _speed;
        [SerializeField]
        protected float _drag;
        protected Vector3 _velocity;

        [SerializeField]
        protected float _attackDelay;

        [SerializeField]
        protected float _transitionDelay;

        [SerializeField]
        protected float _maxRange = 1;
        [SerializeField]
        protected float _minRange = 0.5f;

        public Transform _target;

        [SerializeField]
        private Transform _character;

        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        protected Animator _animator;

        private bool _inRange = false;
        private bool _isAttack = false;
        private bool _isKnockBack = false;

        protected float knockBackDuration = 0.3f;
        private float _damage = 1.0f;

     
#if UNITY_EDITOR

        void OnDrawGizmos()
        {
            if (_target && Vector3.Distance(_target.position, transform.position) < _maxRange && Vector3.Distance(_target.position, transform.position) > _minRange)
            {
                Gizmos.color = UnityEngine.Color.red;
                Gizmos.DrawWireSphere(transform.position, _maxRange);
                Gizmos.DrawWireSphere(transform.position, _minRange);
            }
            else
            {
                Gizmos.color = UnityEngine.Color.black;
                Gizmos.DrawWireSphere(transform.position, _maxRange);
                Gizmos.DrawWireSphere(transform.position, _minRange);
            }
        }
#endif
        protected virtual void Start()
        {
            _hp = _initialHp;
            _rigidbody = GetComponent<Rigidbody>();

            this.UpdateAsObservable().Where(_ => !_isKnockBack)
                .Select(_ => Vector3.Normalize(_target.position-transform.position))
                .Subscribe(x => LookAt(x))
                .AddTo(gameObject);

            this.UpdateAsObservable().Where(_ =>!_isAttack&&!_inRange && !_isKnockBack)
                .Select(_ => Vector3.Normalize(_target.position - transform.position))
                .Subscribe(x => Move(x))
                .AddTo(gameObject);

            this.UpdateAsObservable()
                .Where(_ => Vector3.Distance(_target.position, transform.position) < _minRange)
                .Where(_ => !_inRange)
                .Subscribe(_ =>
                {
                    _inRange = true;
                }).AddTo(gameObject);

            this.UpdateAsObservable().Select(_ => _inRange)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(x => ExcuteAttackRoutine());

            this.UpdateAsObservable()
                .Where(_ => Vector3.Distance(_target.position, transform.position) > _maxRange)
                .Where(_ => _inRange)
                .Subscribe(_ =>
                {
                    _inRange = false;
                }).AddTo(gameObject);

            this.UpdateAsObservable().Where(_ => _velocity.magnitude > 0.1).Subscribe(_ =>
            {
                _animator.SetBool("Walk", true);
            });

            this.UpdateAsObservable().Where(_ => _velocity.magnitude <= 0.1).Subscribe(_ =>
            {
                _animator.SetBool("Walk", false);
            });

            this.UpdateAsObservable()
                .Where(_=>!_isKnockBack)
                .Subscribe(_ =>
            {
                _rigidbody.DOMoveX(_velocity.x, 0.1f).SetRelative(true);
                _rigidbody.DOMoveZ(_velocity.z, 0.1f).SetRelative(true);
                _velocity -= _velocity * _drag;
            }).AddTo(gameObject);

        }

       
        public virtual void Hit(float damage, Vector3 position, Vector3 force)
        {
            if (!_isKnockBack)
            {
                _hp -= damage;
                _isKnockBack = true;
                _rigidbody.DOKill();
                _rigidbody.DOMoveX(force.x, knockBackDuration).SetEase(Ease.OutBounce).SetRelative(true).SetAutoKill();
                _rigidbody.DOMoveZ(force.z, knockBackDuration).SetEase(Ease.OutBounce).SetRelative(true).SetAutoKill()
                    .OnComplete(() =>
                    {
                        _isKnockBack = false;
                        if (_hp < 0)
                        {
                            Die();
                        }
                    });
            }
        }
        public virtual void LookAt(Vector3 direction)
        {
            Vector3 to = direction;
            transform.DOKill();
            transform.DORotateQuaternion(Quaternion.LookRotation(to, _character.up), 0.1f);
        }
        public virtual void Move(Vector3 direction)
        {
            transform.DOKill();
            _velocity += direction * _speed;
        }

        public virtual void Die()
        {
            EnemyManager.instance.KillEnemy(this);
        }
        public virtual void Attack()
        {
            
        }

        private void ExcuteAttackRoutine()
        {
            if (!_isAttack)
            {
                AttackRoutine();
            }
           
        }
        private async Task AttackRoutine()
        {
            _isAttack = true;
            await Task.Delay((int)(_attackDelay * 1000));
            if (_inRange)
            {
                Attack();
                await AttackRoutine();
            }
            else
            {
                await Task.Delay((int)(_transitionDelay * 1000));
                _isAttack=false;
                _animator.SetBool("Shoot", false);
                _animator.SetBool("Attack", false);
                _animator.SetBool("Cast", false);
            }
        }
    }

   
}
