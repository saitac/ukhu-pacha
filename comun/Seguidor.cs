using Godot;

public partial class Seguidor : Node2D
{
	[Export]
	public Node2D Objetivo;

	[Export]
	public Vector2 Desplazamiento = Vector2.Zero;

	public override void _Ready()
	{
		if (Objetivo == null)
		{
			GD.PushError($"{GetPath()}: falta asignar Objetivo en el inspector");
			SetProcess(false);
		}
	}

	public override void _Process(double delta)
	{
		GlobalPosition = Objetivo.GlobalPosition + Desplazamiento;
	}
}
