using Godot;

public partial class playerStart : Node2D
{
  [Export]
  private PackedScene _playerScene;

  [Export] 
  private bool _flipPlayerH = false;

  [Export]
  private bool _flipPlayerV = false;

  [Export]
  private bool _cameraFollowPlayer = true;

  [Export]
  private Camera2D _camera;

  public override void _Ready()
  {
    Visible = false;

    CharacterBody2D newPlayerInstance = (CharacterBody2D)_playerScene.Instantiate();
    RemoteTransform2D remoteTransform = new RemoteTransform2D();

    newPlayerInstance.Position = Position;

    GetParent().CallDeferred("add_child", newPlayerInstance);
    newPlayerInstance.CallDeferred("Flip", _flipPlayerH, _flipPlayerV);
    newPlayerInstance.CallDeferred("add_child", remoteTransform);

    if (_cameraFollowPlayer && _camera != null)
    {
      remoteTransform.RemotePath = _camera.GetPath();
    }
  }
}
