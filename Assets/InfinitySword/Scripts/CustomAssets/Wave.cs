using System;
using System.Collections;
using System.Collections.Generic;
using Assets.InfinitySword.Scripts.Manager;
using UnityEngine;

[Serializable]
public class EnemyData 
{
    public EnemyManager.Type type;
    public float time;
}
[CreateAssetMenu(fileName = "NewWave", menuName = "Wave")]
public class Wave : ScriptableObject
{
    public List<EnemyData> enemyList;
}
