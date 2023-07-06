using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.InfinitySword.Scripts.UI
{
    public class Hpbar : MonoBehaviour
    {
        [SerializeField]
        private Image _bar;
        public float defaultFillAmount=0.5f;
        // Start is called before the first frame update
        void Start()
        {
            _bar = GetComponent<Image>();
            _bar.fillAmount = defaultFillAmount;
            Player.instance.hp.Subscribe(x=>
            {
                _bar.fillAmount = defaultFillAmount * (x / Player.instance.initialHp);
            });
        }

    }
}
