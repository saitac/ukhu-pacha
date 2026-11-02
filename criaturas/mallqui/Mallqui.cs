using Godot;

public partial class Mallqui : CharacterBody2D
{
	[Export]
	public float Velocidad = 100f;

	[Export]
	public int DistanciaMinPatrulla = 2;

	[Export]
	public int DistanciaMaxPatrulla = 4;

	[Export]
	public float EsperaMin = 1.5f;

	[Export]
	public float EsperaMax = 4.0f;

	private Laberinto _laberinto;
	private Vector2[] _ruta = [];
	private int _indicePunto;
	private Vector2? _destino;
	private CollisionShape2D _formaColision;
	private double _tiempoEspera;

	// true si no quedan puntos por recorrer
	private bool RutaTerminada => _indicePunto >= _ruta.Length;

	public override void _Ready()
	{
		_tiempoEspera = GD.RandRange(EsperaMin, EsperaMax);
		_formaColision = GetNode<CollisionShape2D>("CollisionShape2D");
		_laberinto.NavegacionActualizada += OnNavegacionActualizada;
	}

	public override void _PhysicsProcess(double delta)
	{
		Patrullar(delta);
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

	public Rect2 AreaGlobal()
	{
		Rect2 local = _formaColision.Shape.GetRect();
		Rect2 global = new(local.Position + _formaColision.GlobalPosition, local.Size);
		return global;
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

	private void AvanzarPorRuta(double delta)
	{
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

	private void Patrullar(double delta)
	{
		if (!RutaTerminada)
		{
			AvanzarPorRuta(delta);
			return;
		}

		// Llegué al destino: espero y luego elijo uno nuevo
		Velocity = Vector2.Zero;

		if (_tiempoEspera > 0)
		{
			_tiempoEspera -= delta;
			return;
		}

		Vector2 destino = _laberinto.ElegirDestinoPatrulla(GlobalPosition, DistanciaMinPatrulla, DistanciaMaxPatrulla);
		IrA(destino);
		_tiempoEspera = GD.RandRange(EsperaMin, EsperaMax);
	}
}
