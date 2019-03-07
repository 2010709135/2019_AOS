﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour {
    #region Singleton
    public static PlayerController instance = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
    }
    #endregion

    private CharacterController _controller;
    Animator _playerAnimator;
    public PlayerStats _playerStats { get; private set; }
    GrassDetect _grassDetect;
    public CharacterAttack _characterAttack { get; private set; }

    #region Movement
    private bool _isMoving = false;
    public float _speed = 5.0f;
    public float _rotationSpeed = 100.0f;

    private float _width;
    private float _height;

    public Vector2 _movingStartPos { get; private set; }
    public Vector2 _movingCurrentPos { get; private set; }

    public Vector2 _attackStartPos { get; private set;}
    public Vector2 _attackCurrentPos { get; private set; }

    public int _left_touch_id { get; private set; }
    public int _right_touch_id { get; private set; }
    
    public bool _isReadyToFire { get; set; }

    #endregion

    #region Shooting
    public GameObject _attackProjectile;
    public GameObject _firePos;
    Quaternion _fireRot;

    public GameObject _muzzleEffect;
    public LineRenderer _shotIndicator { get; private set; }
    Coroutine _lineRenderingRoutine;

    public Vector3 _attackStickDir { get; set; }

    public Material _attackIncatorMat;
    public Material _skillIncatorMat;

    public bool isActivatingSkill;
    #endregion


    // Use this for initialization
    void Start () {


        _playerAnimator = GetComponentInChildren<Animator>();
        _controller = GetComponent<CharacterController>();
        _playerStats = GetComponent<PlayerStats>();
        _characterAttack = GetComponent<CharacterAttack>();
        _grassDetect = GetComponentInChildren<GrassDetect>();

        _width = (float)Screen.width / 2.0f;
        _height = (float)Screen.height / 2.0f;

        _movingStartPos = Vector2.zero;
        _movingCurrentPos = Vector2.zero;
        _attackStartPos = Vector2.zero;
        _attackCurrentPos = Vector2.zero;

        _left_touch_id = -1;
        _right_touch_id = -1;

        _shotIndicator = GetComponent<LineRenderer>();
        _shotIndicator.enabled = false;
        isActivatingSkill = false;


        _attackStickDir = Vector3.zero;
    }

	// Update is called once per frame
	void Update () {
        foreach (Touch touch in Input.touches)
        {           
            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))                
                    continue;
                
                Vector2 pos = touch.position;
                pos.x = (pos.x - _width) / _width;
                pos.y = (pos.y - _height) / _height;

                if (pos.x <= 0)
                {
                    if (_left_touch_id >= 0) continue;

                    _left_touch_id = touch.fingerId;
                    _movingStartPos = new Vector2(pos.x, pos.y);
                    _isMoving = true;

                    InGameMainUI.instance.MovementStick.StartControlling(touch.fingerId);
                }
                else
                {
                    
                    if (_right_touch_id >= 0 ||
                        SkillCanvas.instance._innerCircle.GetComponent<SkillAttack>()._isControlling ||
                        isActivatingSkill)
                        continue;

                    _right_touch_id = touch.fingerId;
                    _attackStartPos = new Vector2(pos.x, pos.y);

                    InGameMainUI.instance.AttackStick.StartControlling(touch.fingerId);                }
            }
            if (touch.phase == TouchPhase.Moved ||
                touch.phase == TouchPhase.Stationary &&
                _isMoving)
            {
                if (touch.fingerId != _left_touch_id && touch.fingerId != _right_touch_id)
                    continue;
                Vector2 pos = touch.position;
                pos.x = (pos.x - _width) / _width;
                pos.y = (pos.y - _height) / _height;

                if (_left_touch_id == touch.fingerId)
                    _movingCurrentPos = new Vector2(pos.x, pos.y);
                else if(_right_touch_id != touch.fingerId)
                {
                    _attackCurrentPos = new Vector2(pos.x, pos.y);
                }
                                       
                Vector2 dragDir = Vector2.zero;
                if(_left_touch_id == touch.fingerId)
                    dragDir = _movingCurrentPos - _movingStartPos;
                else if(_right_touch_id == touch.fingerId)
                    dragDir = _attackCurrentPos - _attackStartPos;

                float dragAmount = dragDir.magnitude ;
                if (_left_touch_id == touch.fingerId)
                    dragAmount *= 7;
                else if (_right_touch_id == touch.fingerId)
                    ;

                dragAmount = Mathf.Clamp(dragAmount, 0f, 1f);

                Vector3 horizontal = new Vector3(1, 0, 0);
                Vector3 vertical = new Vector3(0, 0, 1);

                Vector3 moveDir = (horizontal * dragDir.x + vertical * dragDir.y).normalized;

                if (touch.fingerId == _left_touch_id)
                {
                    if (dragAmount > 0.4f)
                    {
                        _playerAnimator.SetBool("isMoving", true);
                        _playerAnimator.SetFloat("MoveSpeed", dragAmount);
                        if(!isActivatingSkill)
                            transform.rotation = Quaternion.LookRotation(moveDir);
                        _controller.Move(moveDir * Time.deltaTime * _speed * dragAmount);
                    }

                }
                else if(touch.fingerId == _right_touch_id)
                {
                    if (_isReadyToFire)
                    {
                        _characterAttack.StartDrawIndicator(false);
                    }
                    else if(!_isReadyToFire)
                    {
                        _characterAttack.StopDrawIndicator();                        
                    }
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (touch.fingerId == _left_touch_id && _left_touch_id >= 0)
                {
                    _playerAnimator.SetBool("isMoving", false);
                    _playerAnimator.SetFloat("MoveSpeed", 0);
                    InGameMainUI.instance.MovementStick.StopControlling();
                    _left_touch_id = -1;
                }
                else if (touch.fingerId == _right_touch_id && _right_touch_id >= 0)
                {
                    if (_isReadyToFire)
                    {
                        _characterAttack.StopDrawIndicator();
                        FireBasicAttack();
                        _attackStickDir = Vector3.zero;
                    }

                    _isReadyToFire = false;
                    InGameMainUI.instance.AttackStick.StopControlling();
                    _right_touch_id = -1;
                }
            }            
        } // foreach end
        if (Input.touchCount <= 0)
        {
            _isMoving = false;
            _playerAnimator.SetBool("isMoving", false);
            _playerAnimator.SetFloat("MoveSpeed", 0);
            _movingStartPos = Vector3.zero;
            _movingCurrentPos = Vector3.zero;
        }
    }

    public  void FireBasicAttack()
    {
        _characterAttack.FireBaseAttack();
    }

    public void FireSkillAttack()
    {
        _playerStats.InitializeSkillGage();
        _characterAttack.FireSkillAttack();
        _characterAttack.StopDrawIndicator();
    }

    public void DrawSkillIndicator()
    {
        _characterAttack.StartDrawIndicator(true);
    }
}
