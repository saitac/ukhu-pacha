using Godot;

public partial class Supay : Enemigo
{
	private enum Estado
	{
		FueraDeEscena,
		Apareciendo,
		Persiguiendo,
		Atacando,
		Desapareciendo
	}

	[Export]
	public float VelocidadPersecucion = 200f;

	private DiscoInti _discoInti;
	private Estado _estado = Estado.FueraDeEscena;
	private AnimatedSprite2D _chicote;

	public override void _Ready()
	{
		base._Ready();

		_chicote = GetNode<AnimatedSprite2D>("Chicote");

		_chicote.AnimationFinished += OnChicoteAnimationFinished;
		_chicote.FrameChanged += OnChicoteFrameChanged;
		_sprite.AnimationFinished += OnAnimationFinished;
		_sprite.FrameChanged += OnFrameChanged;
		CambiarEstado(Estado.FueraDeEscena);
	}

	public override void _PhysicsProcess(double delta)
	{
		switch (_estado)
		{
			case Estado.Persiguiendo:
				if (_incaAMiAlcance)
				{
					CambiarEstado(Estado.Atacando);
				}
				else
				{
					Perseguir(delta, VelocidadPersecucion);
					ActualizarAnimacion();
				}
				break;
			case Estado.Atacando:
				_tiempoRecuperacion -= delta;
				if (_tiempoRecuperacion <= 0)
				{
					CambiarEstado(Estado.Persiguiendo);
				}
				break;
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		if (_discoInti != null)
		{
			_discoInti.Tomado -= OnDiscoTomado;
		}
	}

	public void Inicializar(Laberinto laberinto, Inca inca, DiscoInti discoInti)
	{
		_laberinto = laberinto;
		_inca = inca;
		_discoInti = discoInti;
		_discoInti.Tomado += OnDiscoTomado;
	}

	private void CambiarEstado(Estado nuevo)
	{
		_estado = nuevo;

		switch (nuevo)
		{
			case Estado.FueraDeEscena:
				Visible = false;
				SetDeferred(Node.PropertyName.ProcessMode, (int)ProcessModeEnum.Disabled);
				break;
			case Estado.Apareciendo:
				Visible = true;
				SetDeferred(Node.PropertyName.ProcessMode, (int)ProcessModeEnum.Inherit);
				_sprite.Play("emerger");
				break;
			case Estado.Persiguiendo:
				IrHaciaElInca(_laberinto.PosicionMundoAIndiceMapa(_inca.GlobalPosition));
				break;
			case Estado.Atacando:
				IniciarAtaque();
				_tiempoRecuperacion = RecuperacionGolpe;
				break;
		}
	}

	private void OnDiscoTomado(Inca portador, Vector2 posicionTomado)
	{
		GlobalPosition = _laberinto.ElegirPuntoEmergencia(posicionTomado);
		CambiarEstado(Estado.Apareciendo);
	}

	private void OnAnimationFinished()
	{
		if (_estado == Estado.Apareciendo)
		{
			if (_discoInti.EstaEnJuego)
			{
				CambiarEstado(Estado.Persiguiendo);
			}
			else
			{
				CambiarEstado(Estado.Desapareciendo);
			}
		}
	}

	private void ActualizarAnimacion()
	{
		if (Velocity == Vector2.Zero)
		{
			_sprite.Play("idle-" + SufijoSprite);
			return;
		}

		MirarHacia(Velocity);
		_sprite.Play("walk-" + SufijoSprite);
	}

	private void OrientarChicote(Vector2 direccion)
	{
		switch (_ultimaDireccion)
		{
			case Direccion.Abajo:
				_chicote.Scale = new Vector2(3, 3);
				_chicote.Position = new Vector2(-30, 35);
				break;

			case Direccion.Arriba:
				_chicote.Scale = new Vector2(3, 3);
				_chicote.Position = new Vector2(30, 35);
				break;

			case Direccion.Derecha:
				_chicote.Scale = new Vector2(3, 3);
				_chicote.Position = new Vector2(20, 30);
				break;

			case Direccion.Izquierda:
				_chicote.Scale = new Vector2(-3, 3);
				_chicote.Position = new Vector2(-20, 30);
				break;
		}

		if (_chicote.Scale.X < 0)
		{
			_chicote.Rotation = (-direccion).Angle();
		}
		else
		{
			_chicote.Rotation = direccion.Angle();
		}
	}

	private void IniciarAtaque()
	{
		Velocity = Vector2.Zero;
		Vector2 direccion = GlobalPosition.DirectionTo(_inca.GlobalPosition);
		MirarHacia(direccion);
		_sprite.Play("ataque-" + SufijoSprite);
		OrientarChicote(direccion);
	}

	private void OnFrameChanged()
	{
		if (_estado == Estado.Atacando && _sprite.Frame == 1)
		{
			_chicote.Visible = true;
			_chicote.Play("azote");
		}
	}

	private void OnChicoteAnimationFinished()
	{
		_chicote.Visible = false;
		_sprite.Play("idle-" + SufijoSprite);
	}

	private void OnChicoteFrameChanged()
	{
		if (_incaAMiAlcance && _chicote.Frame == 2)
		{
			_inca.RecibirGolpe(DanoGolpe);
		}
	}
}
