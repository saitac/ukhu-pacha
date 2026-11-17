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

	public override void _Ready()
	{
		base._Ready();

		_sprite.AnimationFinished += OnAnimationFinished;
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

	private void IniciarAtaque()
	{
		Velocity = Vector2.Zero;
		MirarHacia(GlobalPosition.DirectionTo(_inca.GlobalPosition));
		_sprite.Play("ataque-" + SufijoSprite);
	}
}
