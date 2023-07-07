using System;
using System.Threading.Tasks;
using DG.Tweening;
using InfinitySword.Scripts;
using UniRx;
using UnityEngine;

namespace Assets.InfinitySword.Scripts.Enemies
{
    public class Archer : Enemy
    {
        public Projectile arrowPrefab;
        public float shootForce;
        public override void Attack()
        {
            _animator.SetBool("Shoot", true);
            Observable.Timer(TimeSpan.FromSeconds(0.3f))
                .Subscribe(_ =>
            {
                _animator.SetBool("Shoot", false);
            }); ;



        }

        public void Shoot()
        {
            var instance = Instantiate(arrowPrefab);
            instance.transform.position = transform.position;
            Vector3 direction = (Player.instance.transform.position - instance.transform.position).normalized;
            instance.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            instance.transform.DOMove(Player.instance.transform.position, shootForce).SetSpeedBased(true).SetAutoKill();
        }
    }
}
