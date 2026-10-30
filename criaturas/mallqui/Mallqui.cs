using Godot;

public partial class Mallqui : CharacterBody2D
{
	private Laberinto _laberinto;

	public void Inicializar(Laberinto laberinto)
	{
		_laberinto = laberinto;
	}
}
