using System.Collections;
using System.Collections.Generic;
using Assets.InfinitySword.Scripts;
using Assets.InfinitySword.Scripts.Enemies;
using UnityEngine;

public class EnemyEvents : MonoBehaviour
{

    [SerializeField] private Enemy enemy;// Start is called before the first frame update

    public void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }
    public void Shoot()
    {
        Archer archer = (Archer)enemy;
        archer.Shoot();
    }
}
