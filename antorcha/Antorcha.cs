using Godot;

public partial class Antorcha : Node2D
{
	private PointLight2D _llama;

	private AnimatedSprite2D _sprite;

	private string _direccion = "down";

	private bool _encendida = true;

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
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_llama = _sprite.GetNode<PointLight2D>("PointLight2D");
	}

	public void AjustarLlama(float nivel)
	{
		bool encendidaAhora = nivel > 0f;
		bool cambio = _encendida != encendidaAhora;

		// Por encima del umbral la luz queda plena (meseta); debajo se achica hasta el mínimo
		float t = Mathf.InverseLerp(0f, UmbralAgotamiento, nivel);
		t = Mathf.Clamp(t, 0f, 1f);
		_llama.Energy = Mathf.Lerp(EnergiaMinima, EnergiaMaxima, t);
		_llama.TextureScale = Mathf.Lerp(EscalaLuzMinima, EscalaLuzMaxima, t);

		_encendida = encendidaAhora;
		_llama.Enabled = _encendida;

		if (cambio)
		{
			ActualizarSprite();

			if (_encendida)
			{
				GD.Print("Se acaba de encender: Recarga");
			} else
			{
				GD.Print("Se acaba de apagar: Humo una vez!!");
			}
		}
	}

	private void ActualizarSprite()
	{
		string animacion = (_encendida ? "antorcha-" : "apagada-") + _direccion;
		_sprite.Play(animacion);
	}

	public void Orientar(string direccion)
	{
		_direccion = direccion;
		ActualizarSprite();
	}
}
