using System;
using System.Collections;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.SomeHelp;
using Nrjwolf.Tools.AttachAttributes;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Yuuta.CookMeat
{
    [RequireComponent(
        typeof(SpriteRenderer), 
        typeof(AudioSource))]
    public class Meat : MonoBehaviour
    {
        [GetComponent] [SerializeField] private SpriteRenderer _spriteRenderer;
        [GetComponent] [SerializeField] private AudioSource _audioSource;
        [SerializeField] private Sprite _rawMeatSprite;
        [SerializeField] private Sprite _cookedMeatSprite;
        [SerializeField] private Sprite _overCookedMeatSprite;
        [SerializeField] private AudioClip _discardAudioClip;
        
        public enum State
        {
            Idle,
            Moving,
            Cooking
        }

        public enum Side
        {
            Front,
            Back
        }

        private Side _GetOtherSide(Side side)
            => side == Side.Front ? Side.Back : Side.Front;

        public class CookingProgress
        {
            public enum State
            {
                Raw,
                Cooked,
                OverCooked
            }
            
            private float _cookDuration = 0f;

            public void AddDuration(float duration)
                => _cookDuration += duration;
            
            public State GetState()
            {
                if (_cookDuration <= 2f)
                    return State.Raw;

                if (_cookDuration <= 5f)
                    return State.Cooked;

                return State.OverCooked;
            }
        }
        
        private const string FIRE_NAME = "Fire";
        private const string CUSTOMER_NAME = "Customer";
        private const string TRASHCAN_NAME = "Trashcan";
        private bool _isOnFire = false;
        private Option<Customer> _movingOnCustomer = Option<Customer>.None;
        private bool _isOnTrashcan = false;
        private Option<Vector3> _lastCookingPosition = Option<Vector3>.None; 
        
        private const float UNIT_FIRE_DURATION = 1f / 60f;
        private readonly static Dictionary<Side, float> FLIP_ROTATE_Y_MAP = new Dictionary<Side, float>()
        {
            {Side.Front, 0f},
            {Side.Back, 180f}
        };
        
        private ReactiveProperty<State> _state = 
            new ReactiveProperty<State>(State.Idle);

        private ReactiveProperty<Side> _side = 
            new ReactiveProperty<Side>(Side.Front);

        private ReactiveDictionary<Side, CookingProgress> _cookingProgresses
            = new ReactiveDictionary<Side, CookingProgress>()
            {
                {Side.Front, new CookingProgress()},
                {Side.Back, new CookingProgress()}
            };

        private void Start()
        {
            this.OnMouseDownAsObservable()
                .Subscribe(_ =>
                {
                    if (_state.Value == State.Cooking)
                    {
                        _lastCookingPosition = 
                            transform.position.ToSome();
                    }
                    
                    _state.Value = State.Moving;
                });
            
            this.OnMouseDragAsObservable()
                .Subscribe(_ =>
                {
                    var mouseWorldPosition = 
                        Camera.main.ScreenToWorldPoint(
                            Input.mousePosition);

                    gameObject.transform.position =
                        new Vector3( 
                            mouseWorldPosition.x,
                            mouseWorldPosition.y,
                            gameObject.transform.position.z);
                }).AddTo(this);

            this.OnMouseUpAsObservable()
                .Subscribe(_ =>
                {
                    if (_isOnFire)
                    {
                        _Flip();
                        _state.Value = State.Cooking;
                        return;
                    }

                    _movingOnCustomer.IfSome(customer =>
                    {
                        customer.Eat(this);
                        Destroy(gameObject);
                    });
                    if (_movingOnCustomer.IsSome)
                        return;

                    if (_isOnTrashcan)
                    {
                        _PlayDiscardSound();
                        Destroy(gameObject);
                        return;
                    }

                    _lastCookingPosition.Match(
                        lastCookingPosition =>
                        {
                            transform.position = lastCookingPosition;
                            _state.Value = State.Cooking;
                        },
                        () => Destroy(gameObject));
                }).AddTo(this);

            this.OnTriggerEnter2DAsObservable()
                .Subscribe(collider =>
                {
                    if (collider.gameObject.name == FIRE_NAME)
                    {
                        _isOnFire = true;
                        return;
                    }
                    
                    if (collider.gameObject.name == CUSTOMER_NAME)
                    {
                        _movingOnCustomer = 
                            collider.GetComponent<Customer>();
                        return;
                    }
                    
                    if (collider.gameObject.name == TRASHCAN_NAME)
                    {
                        _isOnTrashcan = true;
                        return;
                    }
                }).AddTo(this);
            
            this.OnTriggerExit2DAsObservable()
                .Subscribe(collider =>
                {
                    if (collider.gameObject.name == FIRE_NAME)
                    {
                        _isOnFire = false;
                        return;
                    }
                    
                    if (collider.gameObject.name == CUSTOMER_NAME)
                    {
                        _movingOnCustomer = Option<Customer>.None;
                        return;
                    }
                    
                    if (collider.gameObject.name == TRASHCAN_NAME)
                    {
                        _isOnTrashcan = false;
                        return;
                    }
                }).AddTo(this);

            _state.Where(state => state == State.Cooking)
                .Subscribe(async _ =>
                {
                    _audioSource.Play();
                    while (_state.Value == State.Cooking)
                    {
                        var waitUnitFireDuration =
                            Observable.Timer(
                                TimeSpan.FromSeconds(UNIT_FIRE_DURATION))
                                .Select(_ => true);

                        var changeToNotCookingState =
                            _state.Where(state => state != State.Cooking)
                                .Select(_ => false);
                        
                        var result = await waitUnitFireDuration
                            .Merge(changeToNotCookingState).First();

                        if (!result)
                            break;

                        _cookingProgresses[_GetOtherSide(_side.Value)]
                            .AddDuration(UNIT_FIRE_DURATION);
                    }
                    _audioSource.Stop();
                }).AddTo(this);

            _side.Subscribe(side =>
            {
                var cookingState =
                    _cookingProgresses[_side.Value].GetState();

                _spriteRenderer.sprite = cookingState switch
                {
                    CookingProgress.State.Raw => _rawMeatSprite,
                    CookingProgress.State.Cooked => _cookedMeatSprite,
                    CookingProgress.State.OverCooked => _overCookedMeatSprite
                };
            }).AddTo(this);
        }

        private void _Flip()
        {
            _side.Value = _GetOtherSide(_side.Value);
            gameObject.transform.rotation = 
                Quaternion.Euler(0, FLIP_ROTATE_Y_MAP[_side.Value], 0);
        }

        public CookingProgress.State GetCookingState()
        {
            var frontCookingState = _cookingProgresses[Side.Front].GetState();
            var backCookingState = _cookingProgresses[Side.Back].GetState();

            if (frontCookingState == CookingProgress.State.Cooked &&
                backCookingState == CookingProgress.State.Cooked)
                return CookingProgress.State.Cooked;

            if (frontCookingState == CookingProgress.State.OverCooked ||
                backCookingState == CookingProgress.State.OverCooked)
                return CookingProgress.State.OverCooked;

            return CookingProgress.State.Raw;
        }

        private void _PlayDiscardSound()
        {
            GameObject.Find("SceneAudioSource")
                .GetComponent<AudioSource>()
                .PlayOneShot(_discardAudioClip);
        }
    }
}
