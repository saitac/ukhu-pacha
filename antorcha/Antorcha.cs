using Godot;

public partial class Antorcha : Node2D
{
	private PointLight2D _llama;

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
		_llama = GetNode<PointLight2D>("AnimatedSprite2D/PointLight2D");
	}

	public void AjustarLlama(float nivel)
	{
		// Por encima del umbral la luz queda plena (meseta); debajo se achica hasta el mínimo
		float t = Mathf.InverseLerp(0f, UmbralAgotamiento, nivel);
		t = Mathf.Clamp(t, 0f, 1f);
		_llama.Energy = Mathf.Lerp(EnergiaMinima, EnergiaMaxima, t);
		_llama.TextureScale = Mathf.Lerp(EscalaLuzMinima, EscalaLuzMaxima, t);
		_llama.Enabled = nivel > 0f;
	}
}
