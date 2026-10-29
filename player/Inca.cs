using Godot;
using System;

public partial class Inca : CharacterBody2D
{
	[Signal]
	public delegate void MurioEventHandler();

	[Export]
	public float ProporcionGracia = 0.5f;

	private const float DuracionSubida = 0.5f;
	private const float DuracionBajada = 0.8f;
	private const float EnergiaDestello = 2.5f;

	private static readonly Color ColorCura = new(0.61f, 0.82f, 0.42f);

	private float _speed = 300;
	private AnimatedSprite2D _incaSprite;
	private Antorcha _antorcha;
	private Vigor _vigor;
	private PointLight2D _destello;
	private Direccion _incaUltimaDireccion = Direccion.Abajo;
	private Tween _tweenDestello;

	public override void _Ready()
	{
		_incaSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_antorcha = GetNode<Antorcha>("antorcha");
		_vigor = GetNode<Vigor>("Vigor");
		_destello = GetNode<PointLight2D>("Destello");
		_antorcha.Apagada += OnAntorchaApagada;
		_vigor.Agotado += Morir;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direccion = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		ActualizaAnimacion(direccion);
		Velocity = direccion * _speed;
		MoveAndSlide();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!OS.IsDebugBuild())
		{
			return;
		}

		if (@event.IsActionPressed("debug_recargar"))
		{
			RecargarAntorcha();
		}
	}

	private void ActualizaAnimacion(Vector2 direccion)
	{
		if (direccion == Vector2.Zero)
		{
			_incaSprite.Play("idle-" + _incaUltimaDireccion.Sufijo());
			_antorcha.Orientar(_incaUltimaDireccion);
			return;
		}

		if(Math.Abs(direccion.X) > Math.Abs(direccion.Y)) // se mueve en el eje X
		{
			_incaUltimaDireccion = direccion.X > 0 ? Direccion.Derecha : Direccion.Izquierda;
		} else
		{
			_incaUltimaDireccion = direccion.Y > 0 ? Direccion.Abajo : Direccion.Arriba;
		}

		_incaSprite.Play("walk-" + _incaUltimaDireccion.Sufijo() + "-inca");
		_antorcha.Orientar(_incaUltimaDireccion);
	}

	public void IniciarRecargaAntorcha()
	{
		RecargarAntorcha();
		_antorcha.PausarConsumo(true);
	}

	public void DetenerRecargaAntorcha()
	{
		_antorcha.PausarConsumo(false);
	}

	private void Morir()
	{
		EmitSignal(SignalName.Murio);
	}

	private void OnAntorchaApagada()
	{
		float tasa = _vigor.Maximo / (_antorcha.TiempoMaximo * ProporcionGracia);
		_vigor.IniciarDrenaje(tasa);
	}

	private void RecargarAntorcha()
	{
		_antorcha.Recargar();
		_vigor.DetenerDrenaje();
	}

	public bool ChaccharCoca(float curacion)
	{
		if (_vigor.EstaAgotado || _vigor.EstaLleno)
		{
			return false;
		}
		_vigor.Modificar(_vigor.Maximo * curacion);
		Destellar(ColorCura);
		return true;
	}

	private void Destellar(Color color)
	{
		// evita dos tweens peleando por energy si come hojas seguidas
		_tweenDestello?.Kill();
		_destello.Color = color;
		_tweenDestello = CreateTween();
		_tweenDestello.SetTrans(Tween.TransitionType.Sine);
		_tweenDestello.SetEase(Tween.EaseType.Out);

		_tweenDestello.TweenProperty(_destello, "energy", EnergiaDestello, DuracionSubida);
		_tweenDestello.TweenProperty(_destello, "energy", 0f, DuracionBajada);
	}
}
