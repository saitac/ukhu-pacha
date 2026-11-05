using Godot;

public partial class DiscoInti : Node2D
{
	[Export]
	public Vector2 OffsetCargado = new(0, -125);

	[Export]
	public float DistanciaSoltar = 72;

	private Area2D _area;
	private AnimatedSprite2D _rayos;
	private Inca _portador = null;
	private Laberinto _laberinto;
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

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!OS.IsDebugBuild())
		{
			return;
		}

		if (@event.IsActionPressed("debug_soltar"))
		{
			Soltar();
		}
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

	public void Inicializar(Laberinto laberinto)
	{
		_laberinto = laberinto;
	}

	public void Soltar()
	{
		if (!EstaCargado)
		{
			return;
		}

		Vector2 centroCuerpo = _portador.CentroCuerpo;
		Vector2 detras = -_portador.UltimaDireccion.ComoVector();

		Vector2[] direcciones = [detras, detras.Orthogonal(), -detras.Orthogonal(), -detras];
		foreach (Vector2 direccion in direcciones)
		{
			Vector2 posicionDestino = centroCuerpo + direccion * DistanciaSoltar;

			if (_laberinto.EsPosicionTransitable(posicionDestino))
			{
				GlobalPosition = posicionDestino;
				_portador = null;
				_area.SetDeferred(Area2D.PropertyName.Monitoring, true);
				_rayos.Visible = true;
				return; // si alguna sirve, salgo aquí
			}
		}

		// Ninguna posición libre: el Inca sigue cargando el disco.
		GD.PushWarning($"DiscoInti.Soltar: ninguna posición libre alrededor del Inca en {centroCuerpo}, mirando {_portador.UltimaDireccion}. El Inca sigue cargando el disco.");
	}
}
