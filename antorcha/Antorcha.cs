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

	public override void _Ready()
	{
		_llama = GetNode<PointLight2D>("AnimatedSprite2D/PointLight2D");
	}

	public void AjustarLlama(float nivel)
	{
		_llama.Energy = Mathf.Lerp(EnergiaMinima, EnergiaMaxima, nivel);
		_llama.TextureScale = Mathf.Lerp(EscalaLuzMinima, EscalaLuzMaxima, nivel);
		_llama.Enabled = nivel > 0f;
	}
}
