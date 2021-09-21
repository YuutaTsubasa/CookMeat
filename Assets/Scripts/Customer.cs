using System;
using System.Collections;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.SomeHelp;
using Nrjwolf.Tools.AttachAttributes;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using static LanguageExt.Prelude;

namespace Yuuta.CookMeat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Customer : MonoBehaviour
    {
        private readonly static Dictionary<Meat.CookingProgress.State, int> JUDGE_SCORES =
            new Dictionary<Meat.CookingProgress.State, int>()
            {
                {Meat.CookingProgress.State.Raw, 0},
                {Meat.CookingProgress.State.Cooked, 100},
                {Meat.CookingProgress.State.OverCooked, -100}
            };
        
        [GetComponent] [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Sprite _happySprite;
        [SerializeField] private Sprite _angrySprite;
        [SerializeField] private AudioSource _happySound;
        [SerializeField] private AudioSource _angrySound;
            
        private ReactiveProperty<int> _score = new ReactiveProperty<int>(0);

        private void Start()
        {
            var scoreText = Optional(_scoreText);
            _score.Subscribe(score => 
                scoreText.IfSome(text => text.text = $"Score: {score}"))
                .AddTo(this);
        }


        public void Eat(Meat meat)
        {
            var meatCookingState = meat.GetCookingState();
            _score.Value += JUDGE_SCORES[meatCookingState];

            switch (meatCookingState)
            {
                case Meat.CookingProgress.State.Raw:
                case Meat.CookingProgress.State.OverCooked:
                    _spriteRenderer.sprite = _angrySprite;
                    _angrySound.Play();
                    break;
                case Meat.CookingProgress.State.Cooked:
                    _spriteRenderer.sprite = _happySprite;
                    _happySound.Play();
                    break;
            }
        }

        public int GetScore()
            => _score.Value;
    }
}