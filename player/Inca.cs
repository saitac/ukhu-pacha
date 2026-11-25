using Godot;

public partial class Inca : CharacterBody2D
{
	[Signal]
	public delegate void MurioEventHandler();

	[Export]
	public float ProporcionGracia = 0.5f;

	[Export]
	public float DuracionSobresalto = 0.3f;

	private const float DuracionSubida = 0.5f;
	private const float DuracionBajada = 0.8f;
	private const float EnergiaDestello = 2.5f;
	private const float AlturaSobresalto = 10f;

	private static readonly Color ColorCura = new(0.61f, 0.82f, 0.42f);
	private static readonly Color ColorGolpe = new(0.85f, 0.15f, 0.12f);

	private float _speed = 300;
	private AnimatedSprite2D _incaSprite;
	private Antorcha _antorcha;
	private Vigor _vigor;
	private PointLight2D _destello;
	private Direccion _incaUltimaDireccion = Direccion.Abajo;
	private Tween _tweenDestello;
	private CollisionShape2D _formaColision;
	private float _tiempoSobresalto;
	private Tween _tweenSobresalto;
	private Vector2 _posicionReposoSprite;

	public Vector2 CentroCuerpo => _formaColision.GlobalPosition;
	public Direccion UltimaDireccion => _incaUltimaDireccion;

	public override void _Ready()
	{
		_incaSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_posicionReposoSprite = _incaSprite.Position;
		_formaColision = GetNode<CollisionShape2D>("CollisionShape2D");
		_antorcha = GetNode<Antorcha>("antorcha");
		_vigor = GetNode<Vigor>("Vigor");
		_destello = GetNode<PointLight2D>("Destello");
		_antorcha.Apagada += OnAntorchaApagada;
		_vigor.Agotado += Morir;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_tiempoSobresalto > 0)
		{
			_tiempoSobresalto -= (float)delta;
			Velocity = Vector2.Zero;
			ActualizaAnimacion(Vector2.Zero);
			return;
		}

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

		_incaUltimaDireccion = DireccionExtensions.DesdeVector(direccion);

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

	public void RecibirGolpe(float dano)
	{
		if (_vigor.EstaAgotado)
		{
			return;
		}
		_vigor.Modificar(_vigor.Maximo * -dano);
		Destellar(ColorGolpe);
	}

	public Rect2 AreaGlobal()
	{
		Rect2 local = _formaColision.Shape.GetRect();
		Rect2 global = new(local.Position + _formaColision.GlobalPosition, local.Size);
		return global;
	}

	public void OnSupayEmergio(float duracion)
	{
		_tiempoSobresalto = DuracionSobresalto;
		Vector2 posicionSobresaltoSprite = _posicionReposoSprite + new Vector2(0, -AlturaSobresalto);

		_tweenSobresalto?.Kill();

		_tweenSobresalto = CreateTween();
		_tweenSobresalto.TweenProperty(_incaSprite, AnimatedSprite2D.PropertyName.Position.ToString(), posicionSobresaltoSprite, DuracionSobresalto / 2)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		_tweenSobresalto.TweenProperty(_incaSprite, AnimatedSprite2D.PropertyName.Position.ToString(), _posicionReposoSprite, DuracionSobresalto / 2)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.In);
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
