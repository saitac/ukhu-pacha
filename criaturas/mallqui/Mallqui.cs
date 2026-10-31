using Godot;

public partial class Mallqui : CharacterBody2D
{
	private Laberinto _laberinto;
	private Vector2[] _ruta = [];
	private int _indicePunto;
	private Vector2? _destino;

	[Export]
	public float Velocidad = 100f;

	public override void _Ready()
	{
		_laberinto.NavegacionActualizada += OnNavegacionActualizada;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_indicePunto >= _ruta.Length)
		{
			Velocity = Vector2.Zero;
			return;
		}

		Vector2 objetivo = _ruta[_indicePunto];

		// ¿Estoy suficientemente cerca del objetivo?
		// Calculo distancia del mallqui (GlobalPosition) al objetivo (DistanceTo)
		// En paso calculo cuantos píxels implica un paso al mallqui => velocidad * delta
		// Si la distancia que falta es menor o igual a un paso del mallqui, avanzo al siguiente punto de la ruta
		float distancia = GlobalPosition.DistanceTo(objetivo);
		float paso = Velocidad * (float)delta;
		if (distancia <= paso)
		{
			_indicePunto++;
			return;
		}

		Velocity = GlobalPosition.DirectionTo(objetivo) * Velocidad;
		MoveAndSlide();
	}

	public override void _ExitTree()
	{
		_laberinto.NavegacionActualizada -= OnNavegacionActualizada;
	}

	public void Inicializar(Laberinto laberinto)
	{
		_laberinto = laberinto;
	}

	public void IrA(Vector2 destino)
	{
		_destino = destino;
		RecalcularRuta();
	}

	private void RecalcularRuta()
	{
		if (_destino is not Vector2 destino)
		{
			return;
		}
		_ruta = _laberinto.CalcularRuta(GlobalPosition, destino);
		_indicePunto = 1;
	}

	private void OnNavegacionActualizada()
	{
		RecalcularRuta();
	}
}
