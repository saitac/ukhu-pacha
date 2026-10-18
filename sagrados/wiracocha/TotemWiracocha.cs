using Godot;

public partial class TotemWiracocha : StaticBody2D
{
	private Area2D _zonaRecarga;

	public override void _Ready()
	{
		_zonaRecarga = GetNode<Area2D>("ZonaRecarga");
		_zonaRecarga.BodyEntered += OnZonaRecargaBodyEntered;
	}

	private void OnZonaRecargaBodyEntered(Node2D body)
	{
		if (body is Inca inca)
		{
			inca.RecargarCombustible();
		}
	}
}
