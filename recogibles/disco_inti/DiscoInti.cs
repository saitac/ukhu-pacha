using Godot;
using System;

public partial class DiscoInti : Node2D
{
	private enum Estado
	{
		EnReposo,
		Cargado,
		Caido
	}

	[Signal]
	public delegate void TomadoEventHandler(Inca portador);

	[Export]
	public Vector2 OffsetCargado = new(0, -125);

	[Export]
	public float DistanciaSoltar = 72;

	private Area2D _area;
	private AnimatedSprite2D _rayos;
	private Inca _portador = null;
	private Laberinto _laberinto;
	private Estado _estado = Estado.EnReposo;
	public bool EstaCargado => _estado == Estado.Cargado;
	public bool EstaEnJuego => _estado is Estado.Cargado or Estado.Caido;

	public override void _Ready()
	{
		_area = GetNode<Area2D>("Area2D");
		_rayos = GetNode<AnimatedSprite2D>("Rayos");

		_area.BodyEntered += OnAreaBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!EstaCargado)
		{
			return;
		}

		GlobalPosition = _portador.GlobalPosition + OffsetCargado;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!OS.IsDebugBuild())
		{
			return;
		}

		if (@event.IsActionPressed("debug_soltar"))
		{
			Soltar();
		}
	}

	private void OnAreaBodyEntered(Node2D body)
	{
		if (EstaCargado)
		{
			return;
		}

		if (body is Inca inca)
		{
			CambiarEstado(Estado.Cargado, inca);
			_area.SetDeferred(Area2D.PropertyName.Monitoring, false);
			_rayos.Visible = false;
			EmitSignal(SignalName.Tomado, _portador);
		}
	}

	public void Inicializar(Laberinto laberinto)
	{
		_laberinto = laberinto;
	}

	public void Soltar()
	{
		if (!EstaCargado)
		{
			return;
		}

		Vector2 centroCuerpo = _portador.CentroCuerpo;
		Vector2 detras = -_portador.UltimaDireccion.ComoVector();

		Vector2[] direcciones = [detras, detras.Orthogonal(), -detras.Orthogonal(), -detras];
		foreach (Vector2 direccion in direcciones)
		{
			Vector2 posicionDestino = centroCuerpo + direccion * DistanciaSoltar;

			if (_laberinto.EsPosicionTransitable(posicionDestino))
			{
				CambiarEstado(Estado.Caido);
				Reaparecer(posicionDestino);
				return; // si alguna sirve, salgo aquí
			}
		}

		// Ninguna posición libre: el Inca sigue cargando el disco.
		GD.PushWarning($"DiscoInti.Soltar: ninguna posición libre alrededor del Inca en {centroCuerpo}, mirando {_portador.UltimaDireccion}. El Inca sigue cargando el disco.");
	}

	public void Entregar()
	{
		CambiarEstado(Estado.EnReposo);
		Visible = false;
	}

	private void Reaparecer(Vector2 posicion)
	{
		GlobalPosition = posicion;
		_area.SetDeferred(Area2D.PropertyName.Monitoring, true);
		_rayos.Visible = true;
		Visible = true;
	}

	public void Ubicar(Vector2 posicion)
	{
		CambiarEstado(Estado.EnReposo);
		Reaparecer(posicion);
	}

	private void CambiarEstado(Estado nuevo, Inca portador = null)
	{
		bool esCargado = nuevo == Estado.Cargado;
		bool tienePortador = portador != null;

		// Invariante: hay portador si y solo si el disco está Cargado.
		if (esCargado != tienePortador)
		{
			string detalle = esCargado
				? "Cargado requiere un portador"
				: $"{nuevo} no admite portador";
			throw new ArgumentException($"DiscoInti.CambiarEstado: {detalle}.", nameof(portador));
		}

		_estado = nuevo;
		_portador = portador;
	}
}
