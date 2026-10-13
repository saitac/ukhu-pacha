using Godot;

public partial class PolvoPuertas : Node2D
{
	[Export]
	public PackedScene EscenaNube { get; set; }

	public void OnPuertasCambiadas(Godot.Collections.Array<Rect2> areasPuertas)
	{
		foreach (Rect2 areaPuerta in areasPuertas)
		{
			CpuParticles2D nube = (CpuParticles2D)EscenaNube.Instantiate();
			nube.EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle;
			nube.Position = areaPuerta.GetCenter();
			nube.EmissionRectExtents = areaPuerta.Size / 2;
			nube.Emitting = true;
			nube.Finished += nube.QueueFree;
			AddChild(nube);
		}
	}
}
