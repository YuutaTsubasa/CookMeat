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
    public static class Saver
    {
        private const string BEST_SCORE_KEY = "BEST_SCORE";
        private const string SCORE_KEY = "SCORE";

        public static int GetBestScore()
            => PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);

        public static int GetCurrentScore()
            => PlayerPrefs.GetInt(SCORE_KEY, 0);

        public static void SetScore(int score)
        {
            PlayerPrefs.SetInt(SCORE_KEY, score);
            
            var bestScore = GetBestScore();
            if (score > bestScore)
                PlayerPrefs.SetInt(BEST_SCORE_KEY, score);
        }
    }
}