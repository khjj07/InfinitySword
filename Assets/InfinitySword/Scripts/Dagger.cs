using System;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Assets.InfinitySword.Scripts
{
    public class Dagger : MonoBehaviour
    {
        // Start is called before the first frame update
        private float _minimumLength;

        public ReactiveProperty<float> length = new ReactiveProperty<float>();

        public float increaseInterval;
        public float lengthIncreasement=10;
        public float lengthDecreasement=30;
        public float damge = 1;
        public float knockBackPower = 10.0f;

        [SerializeField]
        private Transform _edge;
        [SerializeField]
        private MeshCollider _edgeCollider;

        void Start()
        {
            length.Subscribe(x =>
            {
                SetEdgeScale(x);
            });
            Observable.Interval(TimeSpan.FromSeconds(increaseInterval))
                .Subscribe(_ => length.Value+=lengthIncreasement);

            this.OnCollisionEnterAsObservable().Subscribe(x =>
            {
                if (x.collider && x.collider.CompareTag("Enemy"))
                {
                    x.collider.GetComponent<Enemy>().Hit(damge,x.contacts[0].point, -knockBackPower * x.contacts[0].normal);
                }
                if (length.Value - lengthDecreasement > _minimumLength)
                {
                    length.Value -= lengthDecreasement;
                }
            });

            _minimumLength = _edge.localScale.y;
            length.Value = _minimumLength;
        }

        void SetEdgeScale(float value)
        {
            _edge.DORewind();
            _edge.DOScaleY(value, 0.5f).SetEase(Ease.InOutElastic);
        }
    }
}
