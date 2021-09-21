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
    public class Button : MonoBehaviour
    {
        public void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}