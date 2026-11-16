using Godot;

public partial class Enemigo : CharacterBody2D
{
	protected Laberinto _laberinto;
	protected Inca _inca;
	protected Vector2I _celdaInca;
	protected Vector2? _destino;
	protected Vector2[] _ruta = [];
	protected int _indicePunto;

	// true si no quedan puntos por recorrer
	protected bool RutaTerminada => _indicePunto >= _ruta.Length;

	public override void _Ready()
	{
		_laberinto.NavegacionActualizada += OnNavegacionActualizada;
	}

	public override void _ExitTree()
	{
		_laberinto.NavegacionActualizada -= OnNavegacionActualizada;
	}

	protected void Perseguir(double delta, float velocidad)
	{
		Vector2I celdaIncaActual = _laberinto.PosicionMundoAIndiceMapa(_inca.GlobalPosition);

		if (celdaIncaActual != _celdaInca)
		{
			IrHaciaElInca(celdaIncaActual);
		}

		if (!RutaTerminada)
		{
			AvanzarPorRuta(delta, velocidad);
		}
		else if (MoverHacia(_inca.GlobalPosition, delta, velocidad))
		{
			Velocity = Vector2.Zero;
		}
	}

	protected void IrHaciaElInca(Vector2I celdaInca)
	{
		_celdaInca = celdaInca;
		IrA(_inca.GlobalPosition);
	}

	public void IrA(Vector2 destino)
	{
		_destino = destino;
		RecalcularRuta();
	}

	protected void RecalcularRuta()
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

	protected void AvanzarPorRuta(double delta, float velocidad)
	{
		Vector2 objetivo = _ruta[_indicePunto];

		// Si true, ya llegué al objetivo, entonces marco el siguiente punto de la ruta
		if (MoverHacia(objetivo, delta, velocidad))
		{
			_indicePunto++;
		}
	}

	protected bool MoverHacia(Vector2 objetivo, double delta, float velocidad)
	{
		// ¿Estoy suficientemente cerca del objetivo?
		// Calculo distancia del enemifo (GlobalPosition) al objetivo (DistanceTo)
		// En paso calculo cuantos píxels implica un paso al enemigo => velocidad * delta
		// Si la distancia que falta es menor o igual a un paso del enemigo, devuelvo true porque llegué al destino
		float distancia = GlobalPosition.DistanceTo(objetivo);
		float paso = velocidad * (float)delta;

		if (distancia <= paso)
		{
			return true;
		}

		Velocity = GlobalPosition.DirectionTo(objetivo) * velocidad;
		MoveAndSlide();

		return false;
	}
}
