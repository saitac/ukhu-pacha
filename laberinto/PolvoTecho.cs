using Godot;

public partial class PolvoTecho : CpuParticles2D
{
	private const float MargenEmision = 32f;

	public void OnAvisoIniciado(float _)
	{
		Camera2D camara = GetParent<Camera2D>();
		Vector2 zoom = camara.Zoom;
		Vector2 tamanoPantalla = GetViewportRect().Size;
		Vector2 areaVisible = tamanoPantalla / zoom;
		Vector2 extents = areaVisible / 2 + (Vector2.One * MargenEmision);
		EmissionRectExtents = extents;
		Emitting = true;
	}

	public void OnMutacionOcurrida()
	{
		Emitting = false;
	}
}
