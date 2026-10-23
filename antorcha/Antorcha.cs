using System;
using Godot;

public partial class Antorcha : Node2D
{
	private PointLight2D _llama;

	private PointLight2D _brasa;

	private CpuParticles2D _humo;

	private AnimatedSprite2D _sprite;

	private Marker2D _punta;

	private Direccion _direccion = Direccion.Abajo;

	private bool _encendida = true;

	private float _energiaBase;

	private float _intensidadParpadeo = 0f;

	private float _tiempo = 0f;

	private FastNoiseLite _ruido;

	[Export]
	public float AmplitudParpadeo = 0.8f;

	[Export]
	public float FrecuenciaParpadeo = 2f;

	[Export]
	public float EnergiaMinima = 0.2f;

	[Export]
	public float EnergiaMaxima = 1.5f;

	[Export]
	public float EscalaLuzMinima = 0.3f;

	[Export]
	public float EscalaLuzMaxima = 1.0f;

	[Export(PropertyHint.Range, "0.05,1,0.05")]
	public float UmbralAgotamiento = 0.4f;

	public override void _Ready()
	{
		_ruido = new FastNoiseLite();
		_ruido.Frequency = FrecuenciaParpadeo;
		_energiaBase = EnergiaMaxima;
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_punta = _sprite.GetNode<Marker2D>("Punta");
		_llama = _punta.GetNode<PointLight2D>("PointLight2D");
		_brasa = _punta.GetNode<PointLight2D>("Brasa");
		_humo = _punta.GetNode<CpuParticles2D>("Humo");
		ActualizarSprite();
	}

	public override void _Process(double delta)
	{
		if (!_encendida)
		{
			return;
		}
		_tiempo += (float)delta;
		float variacion = _ruido.GetNoise1D(_tiempo);
		_llama.Energy = _energiaBase * (1f + variacion * AmplitudParpadeo * _intensidadParpadeo);
	}

	private Vector2 PosicionPunta(Direccion direccion)
	{
		return direccion switch
		{
			Direccion.Abajo => new Vector2(7, -4),
			Direccion.Arriba => new Vector2(-7, -8),
			Direccion.Derecha => new Vector2(10, -7),
			Direccion.Izquierda => new Vector2(-10, -7),
			_ => throw new ArgumentOutOfRangeException(nameof(direccion), direccion, null),
		};
	}

	public void AjustarLlama(float nivel)
	{
		bool encendidaAhora = nivel > 0f;
		bool cambio = _encendida != encendidaAhora;

		// Por encima del umbral la luz queda plena (meseta); debajo se achica hasta el mínimo
		float t = Mathf.InverseLerp(0f, UmbralAgotamiento, nivel);
		t = Mathf.Clamp(t, 0f, 1f);
		_energiaBase = Mathf.Lerp(EnergiaMinima, EnergiaMaxima, t);
		_intensidadParpadeo = 1f - t;
		_llama.TextureScale = Mathf.Lerp(EscalaLuzMinima, EscalaLuzMaxima, t);

		_encendida = encendidaAhora;
		_llama.Enabled = _encendida;
		_brasa.Enabled = !_encendida;

		if (cambio)
		{
			ActualizarSprite();

			if (_encendida)
			{
				GD.Print("Se acaba de encender: Recarga");
			} else
			{
				_humo.Emitting = true;
			}
		}
	}

	private void ActualizarSprite()
	{
		string animacion = (_encendida ? "antorcha-" : "apagada-") + _direccion.Sufijo();
		_punta.Position = PosicionPunta(_direccion);
		_sprite.Play(animacion);
	}

	public void Orientar(Direccion direccion)
	{
		if (_direccion == direccion)
		{
			return;
		}
		_direccion = direccion;
		ActualizarSprite();
	}
}
