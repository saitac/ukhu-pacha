using Godot;

public partial class CamaraInca : Camera2D
{
  private const float ZoomJuego = 1.0f;
  private const float ZoomMapa = 0.2f;

  private bool _vistaMapa = false;

  public override void _UnhandledInput(InputEvent @event)
  {
    // Solo funciona en modo debug.
    if (!OS.IsDebugBuild())
    {
      return;
    }

    if (@event.IsActionPressed("debug_zoom"))
    {
      _vistaMapa = !_vistaMapa;
      Zoom = Vector2.One * (_vistaMapa ? ZoomMapa : ZoomJuego);
    }

  }

}
