using Godot;

public partial class Vigor : Node
{
	[Signal]
	public delegate void CambiadoEventHandler(float porcentaje);

	[Signal]
	public delegate void AgotadoEventHandler();

	[Export]
	public float Maximo = 100f;

	private float _restante;

	private bool _agotado = false;

	private float _tasaDrenaje = 0.0f;

	public bool EstaLleno => _restante == Maximo;

	public bool EstaAgotado => _agotado;

	public override void _Ready()
	{
		SetProcess(false);
		_restante = Maximo;
	}

	public override void _Process(double delta)
	{
		Modificar(-_tasaDrenaje * (float)delta);
	}

	public void Modificar(float cantidad)
	{
		EstablecerRestante(_restante + cantidad);
	}

	public void Restaurar()
	{
		_agotado = false;
		EstablecerRestante(Maximo);
	}

	public void Agotar()
	{
		EstablecerRestante(0f);
	}

	public void IniciarDrenaje(float porSegundo)
	{
		_tasaDrenaje = porSegundo;
		SetProcess(true);
	}

	public void DetenerDrenaje()
	{
		SetProcess(false);
	}

	private void EstablecerRestante(float nuevoValor)
	{
		if (_agotado)
		{
			return;
		}

		float restanteAnterior = _restante;
		_restante = Mathf.Clamp(nuevoValor, 0f, Maximo);
		if (restanteAnterior != _restante)
		{
			EmitSignal(SignalName.Cambiado, _restante / Maximo);
		}

		if (_restante <= 0f)
		{
			_agotado = true;
			DetenerDrenaje();
			EmitSignal(SignalName.Agotado);
		}
	}
}
