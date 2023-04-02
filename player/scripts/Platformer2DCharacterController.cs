using Godot;
using System;

public partial class Platformer2DCharacterController : CharacterBody2D
{
  [ExportGroup("Movement Controller Settings")]
  [ExportSubgroup("Movement")]
  [Export(PropertyHint.Range, "0.1,10000.0,0.1")]
  private float _maxSpeed = 200.0f;

  [Export(PropertyHint.Range, "0.1,10000.0,0.1")]
  private float _acceleration = 100.0f;

  [Export(PropertyHint.Range, "0.1,10000.0,0.1")]
  private float _deceleration = 100.0f;

  [Export(PropertyHint.Range, "0.01,1.0,0.01")]
  private float _groundFriction = 0.75f;

  [Export(PropertyHint.Range, "0.01,1.0,0.01")]
  private float _airFriction = 0.95f;

  [Export(PropertyHint.Range, "0.01,2.0,0.01")]
  private float _airControl = 0.75f;

  [ExportSubgroup("Jumping")]
  [Export(PropertyHint.Range, "0.1,10000.0,0.1")]
  private float _jumpVelocity = 400.0f;

  [Export(PropertyHint.Range, "1,10,1")]
  private uint _maxJumps = 1;

  [Export(PropertyHint.Range, "0.01,1.0,0.01")]
  private float _jumpBufferInSec = 0.15f;

  [Export(PropertyHint.Range, "0.01,1.0,0.01")]
  private float _coyoteTimeInSec = 0.05f;

  [Export]
  private Vector2 _apexSpeedBonus = Vector2.Zero;

  [ExportSubgroup("Physics")]
  [Export(PropertyHint.Range, "0.1,10000.0,0.1")]
  private float _terminalVelocity = 400.0f;

  [Export(PropertyHint.Range, "0.1,10.0,0.1")]
  private float _gravityScale = 1.0f;

  [Export(PropertyHint.Range, "0.1,10.0,0.1")]
  private float _gravityScaleOnFall = 2.0f;

  [Export(PropertyHint.Range, "0.1,10.0,0.1")]
  private float _gravityScaleOnJumpStop = 4.0f;

  private bool _jumpRequested = false;
  private bool _inAir = false;
  private bool _jumping = false;
  private bool _onGround = true;
  private bool _apexReached = false;
  private bool _jumpPressed = false;
  private bool _jumpReleased = false;
  
  private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
  private float _finalGravityScale;
  private float _coyoteTimeRemaining;
  private float _jumpBufferTimeRemaining;

  private uint _jumpsRemaing;
  private Vector2 _velocityFromInput = Vector2.Zero;

  private Area2D _groundTriggerBox;
  private AnimatedSprite2D _playerSprite;
  private AnimationNodeStateMachinePlayback _animationTree;

  public Platformer2DCharacterController()
  {
    _finalGravityScale = _gravityScale;
    _jumpsRemaing = _maxJumps;
    _coyoteTimeRemaining = _coyoteTimeInSec;
    _jumpBufferTimeRemaining = 0.0f;
  }

  public override void _Ready()
  {
    _groundTriggerBox = GetNode<Area2D>("GroundTriggerBox");
    _playerSprite = GetNode<AnimatedSprite2D>("PlayerSprite");
    _animationTree = (AnimationNodeStateMachinePlayback)GetNode<AnimationTree>("AnimationTree").Get("parameters/playback");

    if (_groundTriggerBox != null)
    {
      _groundTriggerBox.BodyEntered += OnGroundTriggerBoxBodyEntered;
      _groundTriggerBox.BodyExited += OnGroundTriggerBoxBodyExited;

      if (_groundTriggerBox.GetOverlappingBodies().Count == 0)
      {
        OnLeftGround();
      }
      else
      {
        OnLanded();
      }
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    ApplyGravity();
    ApplyVelocity();
    HandleJump();
    MoveAndSlide();
  }

  public override void _Process(double delta)
  {
    ProcessInput();
    HandleAnimation();

    if (Velocity.Y > 0.0f)
    {
      _coyoteTimeRemaining -= (float)delta;
    }

    if (_jumpBufferTimeRemaining > 0.0f)
    {
      _jumpBufferTimeRemaining -= (float)delta;
    }
  }

  public void OnGroundTriggerBoxBodyEntered(Node2D body)
  {
    OnLanded();
  }

  public void OnGroundTriggerBoxBodyExited(Node2D body)
  {
    if (_groundTriggerBox.GetOverlappingBodies().Count == 0)
    {
      OnLeftGround();
    }
  }

  public bool IsOnGround()
  {
    return _onGround;
  }

  public void Flip(bool flipH, bool flipV)
  {
    if (_playerSprite == null)
    {
      return;
    }

    _playerSprite.FlipH = flipH;
    _playerSprite.FlipV = flipV;
  }

  private void ApplyGravity()
  {
    Vector2 newVelocity = Velocity;
    double delta = GetPhysicsProcessDeltaTime();

    if (!IsOnGround())
    {
      newVelocity.Y += _gravity * _finalGravityScale * (float)delta;

      if (_inAir && !_jumping && Velocity.Y > 0.0f && _coyoteTimeRemaining > 0.0f)
      {
        newVelocity.Y = 0.0f;
      }
    }

    newVelocity.Y = MathF.Min(_terminalVelocity, newVelocity.Y);

    Velocity = newVelocity;
  }

  private void ApplyVelocity()
  {
    if (IsOnGround())
    {
      HandleMovementWithFriction(_groundFriction);
    }
    else
    {
      HandleMovementWithFriction(_airFriction * _airControl);
    }
  }

  private void HandleMovementWithFriction(float friction)
  {
    float newXVelocity = Velocity.X;

    if (_velocityFromInput.IsZeroApprox())
    {
      newXVelocity += (MathF.Sign(newXVelocity) / -1.0f) * _deceleration * friction;

      if (MathF.Abs(newXVelocity) <= 25.0f)
      {
        newXVelocity = 0.0f;
      }
    }
    else
    {
      newXVelocity += _velocityFromInput.X * _acceleration * friction;

      if (MathF.Abs(newXVelocity) >= _maxSpeed)
      {
        newXVelocity = MathF.Sign(_velocityFromInput.X) * _maxSpeed;
      }

      if (_playerSprite != null)
      {
        _playerSprite.FlipH = MathF.Sign(_velocityFromInput.X) < 0.0f;
      }
    }

    Velocity = new Vector2(newXVelocity, Velocity.Y);
  }

  private void HandleAnimation()
  {
    if (_animationTree == null)
    {
      return;
    }

    if (Mathf.Abs(Velocity.X) > 25.0f && IsOnFloor() && !IsOnWall())
    {
      _animationTree.Travel("Run");
    }
    else if (Velocity.Y < 0.0f && !IsOnGround())
    {
      _animationTree.Travel("Jump");
    }
    else if (Velocity.X > 0.0f && !IsOnGround())
    {
      _animationTree.Travel("Fall");
    }
    else if (IsOnGround())
    {
      _animationTree.Travel("Idle");
    }
  }

  private void ProcessInput()
  {
    float moveStrength = Input.GetActionStrength("MoveRight") - Input.GetActionStrength("MoveLeft");
    Vector2 direction = Vector2.Right * moveStrength;

    if (moveStrength != 0.0)
    {
      
      _velocityFromInput = direction;
    }
    else
    {
      _velocityFromInput = Vector2.Zero;
    }
    
    if (Input.IsActionJustPressed("Jump") && !_jumpRequested)
    {
      _jumpRequested = true;
      _jumpPressed = true;
      _jumpReleased = false;

      _jumpBufferTimeRemaining = _jumpBufferInSec;
    }
    else if (Input.IsActionJustReleased("Jump"))
    {
      _jumpPressed = false;
      _jumpReleased = true;
    }
  }

  private void HandleJump()
  {
    if (ShouldExecuteOnApexReached())
    {
      OnApexReached();
    }

    if (_jumpRequested)
    {
      _jumpRequested = false;

      if (CanJump())
      {
        _jumpsRemaing--;

        _inAir = true;
        _jumping = true;
        _apexReached = false;
        
        _finalGravityScale = _gravityScale;
        Velocity = new Vector2(Velocity.X, -_jumpVelocity);
      }
    }

    if (!_apexReached && !IsOnGround() && _jumpReleased)
    {
      _finalGravityScale = _gravityScaleOnJumpStop;
    }
  }

  private bool CanJump()
  {
    return _coyoteTimeRemaining > 0.0f && 
           _jumpsRemaing > 0 &&
           (
             (!_inAir && IsOnGround()) || (_inAir && !IsOnGround())
           );
  }

  private bool ShouldExecuteOnApexReached()
  {
    return _inAir && Mathf.Sign(Velocity.Y) != -1.0f && !_apexReached && _jumping;
  }

  private void OnApexReached()
  {
    _apexReached = true;
    _finalGravityScale = _gravityScaleOnFall;

    Velocity += new Vector2(MathF.Sign(Velocity.X) * _apexSpeedBonus.X, _apexSpeedBonus.Y);
  }

  private void OnLanded()
  {
    _finalGravityScale = _gravityScale;
    _apexReached = false;
    _inAir = false;
    _jumping = false;
    _onGround = true;
    _jumpsRemaing = _maxJumps;
    _coyoteTimeRemaining = _coyoteTimeInSec;

    if (_jumpBufferTimeRemaining > 0.0f && _jumpPressed)
    {
      _jumpRequested = true;
    }

    _jumpBufferTimeRemaining = 0.0f;
  }

  private void OnLeftGround()
  {
    _inAir = true;
    _onGround = false;

    if (_jumpsRemaing == _maxJumps && _coyoteTimeRemaining <= 0.0f)
    {
      _jumpsRemaing--;
    }
  }
}
