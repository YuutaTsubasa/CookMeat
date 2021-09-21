using System;
using System.Collections;
using System.Collections.Generic;
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
    public class ScoreViewer : MonoBehaviour
    {
        [GetComponent] [SerializeField] private Text _scoreText;
        [SerializeField] private bool _isBest;
        
        public void Start()
        {
            _scoreText.text = 
                $"{(_isBest ? "Best Score" : "Your Score")}:{(_isBest ? Saver.GetBestScore() : Saver.GetCurrentScore())}";
        }
    }
}