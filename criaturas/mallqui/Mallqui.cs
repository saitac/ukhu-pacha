using Godot;

public partial class Mallqui : Enemigo
{
	private enum Estado
	{
		Patrulla,
		Persecucion,
		Ataque
	}

	private const uint CapaMundo = 1;

	[Export]
	public bool Depuracion = false;

	[Export]
	public float VelocidadPatrulla = 80f;

	[Export]
	public float VelocidadPersecucion = 180f;

	[Export]
	public int DistanciaMinPatrulla = 2;

	[Export]
	public int DistanciaMaxPatrulla = 4;

	[Export]
	public float EsperaMin = 1.5f;

	[Export]
	public float EsperaMax = 4.0f;

	[Export]
	public float RadioVision = 300f;

	private CollisionShape2D _formaColision;
	private double _tiempoEspera;
	private Estado _estado = Estado.Patrulla;
	private Label _etiquetaDepuracion;
	private double _tiempoRecuperacion;

	public override void _Ready()
	{
		base._Ready();

		_tiempoEspera = GD.RandRange(EsperaMin, EsperaMax);
		_formaColision = GetNode<CollisionShape2D>("CollisionShape2D");

		// Solo por depuración
		_etiquetaDepuracion = GetNode<Label>("EtiquetaDepuracion");
		_etiquetaDepuracion.Visible = Depuracion;
	}

	public override void _PhysicsProcess(double delta)
	{
		switch (_estado)
		{
			case Estado.Patrulla:
				if (PuedeVerAlInca())
				{
					CambiarEstado(Estado.Persecucion);
				}
				else
				{
					Patrullar(delta);
				}
				break;
			case Estado.Persecucion:
				if (!PuedeVerAlInca())
				{
					CambiarEstado(Estado.Patrulla);
				}
				else if (_incaAMiAlcance)
				{
					CambiarEstado(Estado.Ataque);
				}
				else
				{
					Perseguir(delta, VelocidadPersecucion);
				}
				break;
			case Estado.Ataque:
				_tiempoRecuperacion -= delta;

				if (_tiempoRecuperacion <= 0)
				{
					if (PuedeVerAlInca())
					{
						CambiarEstado(Estado.Persecucion);
					}
					else
					{
						CambiarEstado(Estado.Patrulla);
					}
				}

				break;
		}

		ActualizarAnimacion();

		if (Depuracion)
		{
			_etiquetaDepuracion.Text = $"{Name}\n{_estado}\nalcance: {_incaAMiAlcance}";
		}
	}

	public void Inicializar(Laberinto laberinto, Inca inca)
	{
		_laberinto = laberinto;
		_inca = inca;
	}

	public Rect2 AreaGlobal()
	{
		Rect2 local = _formaColision.Shape.GetRect();
		Rect2 global = new(local.Position + _formaColision.GlobalPosition, local.Size);
		return global;
	}

	private void Patrullar(double delta)
	{
		if (!RutaTerminada)
		{
			AvanzarPorRuta(delta, VelocidadPatrulla);
			return;
		}

		// Llegué al destino: espero y luego elijo uno nuevo
		Velocity = Vector2.Zero;

		if (_tiempoEspera > 0)
		{
			_tiempoEspera -= delta;
			return;
		}

		Vector2 destino = _laberinto.ElegirDestinoPatrulla(GlobalPosition, DistanciaMinPatrulla, DistanciaMaxPatrulla);
		IrA(destino);
		_tiempoEspera = GD.RandRange(EsperaMin, EsperaMax);
	}

	private bool PuedeVerAlInca()
	{
		if (GlobalPosition.DistanceSquaredTo(_inca.GlobalPosition) > RadioVision * RadioVision)
		{
			return false;
		}

		PhysicsDirectSpaceState2D espacio = GetWorld2D().DirectSpaceState;
		PhysicsRayQueryParameters2D consulta = PhysicsRayQueryParameters2D.Create(GlobalPosition, _inca.GlobalPosition, CapaMundo);
		Godot.Collections.Dictionary resultado = espacio.IntersectRay(consulta);

		// el rayo solo mira la capa mundo: si no chocó, nada tapa la vista
		return resultado.Count == 0;
	}

	private void CambiarEstado(Estado nuevo)
	{
		if (nuevo == _estado)
		{
			return;
		}

		_estado = nuevo;

		if (_estado == Estado.Persecucion)
		{
			IrHaciaElInca(_laberinto.PosicionMundoAIndiceMapa(_inca.GlobalPosition));
		}
		else if (_estado == Estado.Ataque)
		{
			IniciarAtaque();
			_tiempoRecuperacion = RecuperacionGolpe;
		}
	}

	private void ActualizarAnimacion()
	{
		if (Velocity == Vector2.Zero)
		{
			string prefijo = _estado == Estado.Ataque ? "ataque-" : "idle-";
			_sprite.Play(prefijo + SufijoSprite);
			return;
		}

		MirarHacia(Velocity);
		_sprite.Play("walk-" + SufijoSprite);
	}

	private void IniciarAtaque()
	{
		Velocity = Vector2.Zero;
		MirarHacia(GlobalPosition.DirectionTo(_inca.GlobalPosition));
		_inca.RecibirGolpe(DanoGolpe);
	}
}
