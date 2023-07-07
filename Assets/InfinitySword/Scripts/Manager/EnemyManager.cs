using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.InfinitySword.Scripts.Pattern;
using UniRx;
using UniRx.Triggers;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.InfinitySword.Scripts.Manager
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [SerializeField] private List<Enemy> _enemies;
        [SerializeField] private List<Enemy> _enemyPrefabs;
        public enum State
        {
            Wave,
            Rest
        }
        public enum Type
        {
            Minion,
            MinionBroken,
            Archer,
            ArcherBroken,
            Mage,
            MageBroken,
            Warrior,
            WarriorBroken,
        }

        [SerializeField]
        private List<Wave> _waves;
        private List<Wave> _wavesCopy;

        public GameObject floor;

        private float restTime = 3.0f;

        private State _state = State.Rest;

        void Start()
        {
            this.UpdateAsObservable()
                .DistinctUntilChanged()
                .Where(_ => _state == State.Rest)
                .Subscribe(_ => Rest());

            this.UpdateAsObservable().Where(_ => _state == State.Wave)
                .DistinctUntilChanged()
                .Subscribe(_ =>
                {
                    if (_waves.Count > 0)
                    {
                        Debug.Log("Wave");
                        _wavesCopy = new List<Wave>(_waves);
                        WaveRoutine(_wavesCopy[0]);
                        _wavesCopy.RemoveAt(0);
                    }
                    else
                    {
                        Debug.Log("End");
                    }
                });
        }

        private async Task Rest()
        {
            Debug.Log("Rest");
            await Task.Delay((int)(restTime*1000));
            _state = State.Wave;
        }

        private async Task WaveRoutine(Wave currentWave, int index=0)
        {
            if (currentWave.enemyList.Count > index)
            {
                await Task.Delay((int)(currentWave.enemyList[index].time * 1000));
                CreateEnemy(currentWave.enemyList[index].type);
                await WaveRoutine(currentWave, ++index);
            }
            else
            {
                _state = State.Rest;
            }
        }

        public void CreateEnemy(Type type)
        {
            Vector3 spawnPoint  = floor.transform.GetChild(UnityEngine.Random.Range(0, floor.transform.childCount)).position;
            while (Vector3.Distance(spawnPoint, Player.instance.transform.position) < 5)
            {
                spawnPoint = floor.transform.GetChild(UnityEngine.Random.Range(0, floor.transform.childCount)).position;
            }
            var item = Instantiate(_enemyPrefabs[(int)type]);
            item._target = Player.instance.transform;
            spawnPoint.y = 0.5f;
            item.transform.localPosition = spawnPoint;
            _enemies.Add(item);
        }
        public void KillEnemy(Enemy enemy)
        {
            _enemies.Remove(enemy);
            Destroy(enemy.gameObject);
        }
    }
}
