using Godot;

public partial class CamaraInca : Camera2D
{
  private const float ZoomJuego = 1.0f;
  private const float ZoomMapa = 0.2f;
  private const float IntensidadMaximaTemblor = 4.0f;

  private bool _vistaMapa = false;
  private float _duracionAviso = 0.0f;
  private float _transcurridoAviso = 0.0f;

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

  public override void _Process(double delta)
  {
    if (_duracionAviso <= 0.0f)
    {
      return;
    }

    _transcurridoAviso += (float)delta;
    float progreso = Mathf.Min(_transcurridoAviso / _duracionAviso, 1.0f);
    float intensidad = IntensidadMaximaTemblor * progreso;
    Offset = new Vector2((float)GD.RandRange(-intensidad, intensidad), (float)GD.RandRange(-intensidad, intensidad));
  }

  public void OnAvisoIniciado(float duracion)
  {
    _duracionAviso = duracion;
    _transcurridoAviso = 0.0f;
  }

  public void OnMutacionOcurrida()
  {
    _duracionAviso = 0.0f;
    Offset = Vector2.Zero;
  }

}
