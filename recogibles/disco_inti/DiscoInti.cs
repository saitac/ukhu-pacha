using Godot;

public partial class DiscoInti : Node2D
{
	[Export]
	public Vector2 OffsetCargado = new(0, -40);

	private Area2D _area;
	private AnimatedSprite2D _rayos;
	private Inca _portador = null;
	public bool EstaCargado => _portador != null;

	public override void _Ready()
	{
		_area = GetNode<Area2D>("Area2D");
		_rayos = GetNode<AnimatedSprite2D>("Rayos");

		_area.BodyEntered += OnAreaBodyEntered;

	}

	public override void _PhysicsProcess(double delta)
	{
		if (!EstaCargado)
		{
			return;
		}

		GlobalPosition = _portador.GlobalPosition + OffsetCargado;
	}

	private void OnAreaBodyEntered(Node2D body)
	{
		if (EstaCargado)
		{
			return;
		}

		if (body is Inca inca)
		{
			_portador = inca;
			_area.SetDeferred(Area2D.PropertyName.Monitoring, false);
			_rayos.Visible = false;
		}
	}

	public void Soltar()
	{
		if (!EstaCargado)
		{
			return;
		}

		GlobalPosition = _portador.GlobalPosition +  new Vector2(100 ,100);
		_portador = null;
		_area.SetDeferred(Area2D.PropertyName.Monitoring, true);
		_rayos.Visible = true;
	}
}
