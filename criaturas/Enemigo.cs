using Godot;

public partial class Enemigo : CharacterBody2D
{
	[Export]
	public float DanoGolpe = 0.1f;

	[Export]
	public float RecuperacionGolpe = 1.0f;

	protected Laberinto _laberinto;
	protected Inca _inca;
	protected Vector2I _celdaInca;
	protected Vector2? _destino;
	protected Vector2[] _ruta = [];
	protected int _indicePunto;
	protected AnimatedSprite2D _sprite;
	protected Direccion _ultimaDireccion = Direccion.Abajo;
	protected bool _incaAMiAlcance = false;
	protected Area2D _zonaAlcance;
	protected double _tiempoRecuperacion;

	// la izquierda reutiliza los sprites de la derecha con FlipH.
	protected string SufijoSprite => _ultimaDireccion == Direccion.Izquierda ? Direccion.Derecha.Sufijo() : _ultimaDireccion.Sufijo();

	// true si no quedan puntos por recorrer
	protected bool RutaTerminada => _indicePunto >= _ruta.Length;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_zonaAlcance = GetNode<Area2D>("ZonaAlcance");

		_zonaAlcance.BodyEntered += OnZonaAlcanceBodyEntered;
		_zonaAlcance.BodyExited += OnZonaAlcanceBodyExited;

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
		// Calculo distancia del enemigo (GlobalPosition) al objetivo (DistanceTo)
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

	protected void MirarHacia(Vector2 vector)
	{
		_ultimaDireccion = DireccionExtensions.DesdeVector(vector);
		_sprite.FlipH = (_ultimaDireccion == Direccion.Izquierda);
	}

	private void OnZonaAlcanceBodyEntered(Node2D body)
	{
		if (body is Inca)
		{
			_incaAMiAlcance = true;
		}
	}

	private void OnZonaAlcanceBodyExited(Node2D body)
	{
		if (body is Inca)
		{
			_incaAMiAlcance = false;
		}
	}
}
