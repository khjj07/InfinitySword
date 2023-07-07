using Assets.InfinitySword.Scripts.Pattern;
using DG.Tweening;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.InfinitySword.Scripts
{
    public class Player : Singleton<Player>
    {
        public enum State
        {
            Idle,
            Walk,
            HeavyIdle,
            HeavyWalk,
            Damaged
        }

        [SerializeField] private KeyCode _up =KeyCode.W;
        [SerializeField] private KeyCode _down = KeyCode.S;
        [SerializeField] private KeyCode _left = KeyCode.A;
        [SerializeField] private KeyCode _right = KeyCode.D;

        public float maxMass = 1.5f;
        public float strength = 200;
        public float speed = 0.1f;
        public float swingSpeed = 1;
        public float initialHp;
        public ReactiveProperty<float> hp = new ReactiveProperty<float>();

        [SerializeField] private Dagger _dagger;
        [SerializeField] private Transform _character;
        [SerializeField] private Rigidbody _rigidbody;

        private bool _isHeavy = false;

        // Start is called before the first frame update
        void Start()
        {
            hp.Value = initialHp;
            _rigidbody = GetComponent<Rigidbody>();

            this.UpdateAsObservable()
                .Where(_ => Input.GetKey(_up))
                .Subscribe(_=>MoveUp());
            this.UpdateAsObservable()
                .Where(_ => Input.GetKey(_down))
                .Subscribe(_ => MoveDown());
            this.UpdateAsObservable()
                .Where(_ => Input.GetKey(_left))
                .Subscribe(_ => MoveLeft());
            this.UpdateAsObservable()
                .Where(_ => Input.GetKey(_right))
                .Subscribe(_ => MoveRight());

            this.UpdateAsObservable()
                .Where(_ => Input.GetMouseButton(0))
                .Select(_ => Camera.main.ScreenPointToRay(Input.mousePosition))
                .Select(ray => Physics.Raycast(ray, out RaycastHit hit) ? (RaycastHit?)hit : null )
                .Subscribe(x =>
                {
                    if (x.HasValue && x.Value.collider.CompareTag("Floor"))
                    {
                        Swing(x.Value.point);
                    }
                }); //È¸Àü

            this.UpdateAsObservable().Where(_ => !_isHeavy && _dagger.length.Value > strength)
                .Subscribe(_ => _isHeavy = true);

            this.UpdateAsObservable().Where(_ => _isHeavy && _dagger.length.Value <= strength)
                .Subscribe(_ => _isHeavy = false);

            this.UpdateAsObservable().Subscribe(_ =>
            {
                _rigidbody.mass = _rigidbody.mass < maxMass ?MathF.Log(_dagger.length.Value)/10 : maxMass;
                _character.rotation = Quaternion.LookRotation(_dagger.transform.up, Vector3.up);
            });

#if UNITY_EDITOR
            this.UpdateAsObservable()
                .Where(_ => Input.GetMouseButton(0))
                .Select(_ => Camera.main.ScreenPointToRay(Input.mousePosition))
                .Select(ray => Physics.Raycast(ray, out RaycastHit hit) ? (RaycastHit?)hit : null)
                .Subscribe(x =>
                {
                    if (x.HasValue)
                    {
                        Debug.DrawLine(Camera.main.ScreenToWorldPoint(Input.mousePosition), x.Value.point,Color.red);
                    }
                    else
                    {
                        Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward);
                    }
                }); //Debugging
#endif
        }
        private void Swing(Vector3 point)
        {
            Vector3 swingDirection = (point - _dagger.transform.position).normalized;
            Vector3 crossProduct = Vector3.Cross(_dagger.transform.up, swingDirection);

            // Check if the cross product is pointing upwards
            if (crossProduct.y > 0)
            {
                _dagger.GetComponent<ArticulationBody>().AddTorque(crossProduct * swingSpeed, ForceMode.Force);
            }
            else
            {
                _dagger.GetComponent<ArticulationBody>().AddTorque(crossProduct * swingSpeed, ForceMode.Force);
            }
        }

        private void Move(Vector3 direction)
        {
            transform.DOKill();
            _rigidbody.AddForce(direction * speed, ForceMode.Force);
        }

        void MoveUp()
        {
            Move(Vector3.forward);
        }
        void MoveLeft()
        {
            Move(Vector3.left);
        }
        void MoveRight()
        {
            Move(Vector3.right);
        }
        void MoveDown()
        {
            Move(Vector3.back);
        }
        public void Hit(float damage)
        {
            hp.Value -= damage;
            if (hp.Value < 0)
            {
                hp.Value = 0;
                Die();
            }
        }
        public void Die()
        {

        }
    }
}
