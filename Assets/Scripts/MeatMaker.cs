using System;
using System.Collections;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.SomeHelp;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Yuuta.CookMeat
{
    public class MeatMaker : MonoBehaviour
    {
        private const float MEAT_Z = 0;
        
        [SerializeField] private Meat _meatPrefab;
        
        private void Start()
        {
            _MakeMeat();
        }

        private Meat _MakeMeat()
        {
            var meat = Instantiate<Meat>(_meatPrefab);
            meat.transform.position = new Vector3(
                gameObject.transform.position.x,
                gameObject.transform.position.y,
                MEAT_Z);

            Option<IDisposable> makeMeatCommand = Option<IDisposable>.None;
            makeMeatCommand = 
                meat.OnMouseDownAsObservable()
                    .Subscribe(_ =>
                    {
                        _MakeMeat();
                        
                        makeMeatCommand.IfSome(
                            disposable => disposable.Dispose());
                    }).AddTo(meat).ToSome();

            return meat;
        }
    }
}