using Assets.InfinitySword.Scripts.Pattern;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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

        public float strength = 200;
        public float speed = 0.1f;
        public float rotateSpeed = 1;
        public float initialDrag = 1.0f;
        public float drag = 0.1f;
        public float initialHp;
        public ReactiveProperty<float> hp = new ReactiveProperty<float>();

        private Vector3 _velocity;

        [SerializeField] private GameObject _character;

        [SerializeField] private Dagger _dagger;

        private Vector3 _direction;
        private bool _isHeavy = false;

        // Start is called before the first frame update
        void Start()
        {
            hp.Value = initialHp;
            _direction = _character.transform.forward;
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
                _character.transform.DOKill();
                _character.transform.DORotateQuaternion(Quaternion.LookRotation(_direction, _character.transform.up),
                    _dagger.length.Value / rotateSpeed);
            });

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    Vector3 result = _velocity * (1 / Mathf.Pow(_dagger.length.Value,1.2f));
                    drag = initialDrag * (1 / _dagger.length.Value);
                    transform.DOMove(result, 0.1f).SetRelative(true);
                    _velocity -= _velocity * drag;
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
            point.y= _character.transform.position.y;
            _direction = Vector3.Normalize(point - _character.transform.position);
        }

        private void Move(Vector3 direction)
        {
            transform.DOKill();
            _velocity += direction * speed;
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
