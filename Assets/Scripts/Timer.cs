using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using LanguageExt;
using LanguageExt.SomeHelp;
using Nrjwolf.Tools.AttachAttributes;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static LanguageExt.Prelude;

namespace Yuuta.CookMeat
{
    [RequireComponent(typeof(Text))]
    public class Timer : MonoBehaviour
    {
        private const string RESULT_SCENE_NAME = "ResultScene";
        
        [GetComponent] [SerializeField] private Text _timerText;
        [SerializeField] private Customer _customer;
            
        private ReactiveProperty<int> _time = new ReactiveProperty<int>(60);

        private void Start()
        {
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Where(_ => _time.Value > 0)
                .Subscribe(_ =>
                {
                    _time.Value -= 1;
                }).AddTo(this);

            _time.Where(time => time <= 0)
                .Subscribe(_ =>
                {
                    Saver.SetScore(_customer.GetScore());
                    SceneManager.LoadScene(RESULT_SCENE_NAME);
                }).AddTo(this);

            _time.Subscribe(time =>
            {
                _timerText.text = $"Time: {time / 60}:{(time % 60).ToString().PadLeft(2, '0')}";
            }).AddTo(this);
        }
    }
}