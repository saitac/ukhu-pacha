using Godot;

public partial class Combustible : Node
{
	[Signal]
	public delegate void NivelCambiadoEventHandler(float nivel);

	[Signal]
	public delegate void AgotadoEventHandler();

	[Signal]
	public delegate void GraciaAgotadaEventHandler();

	[Export]
	public float TiempoMaximo = 60f;

	[Export]
	public float ProporcionGracia = 0.5f;

	private float _restante;

	private bool _consumoPausado = false;

	private Timer _timerGracia;

	public override void _Ready()
	{
		_restante = TiempoMaximo;
		_timerGracia = new Timer();
		_timerGracia.OneShot = true;
		_timerGracia.Timeout += OnTimerGraciaTimeout;
		AddChild(_timerGracia);
	}

	public override void _Process(double delta)
	{
		if (_consumoPausado)
		{
			return;
		}

		if (_restante <= 0f)
		{
			return;
		}

		_restante = Mathf.Max(_restante - (float)delta, 0f);

		EmitSignal(SignalName.NivelCambiado, _restante / TiempoMaximo);

		if (_restante <= 0f)
		{
			_timerGracia.WaitTime = TiempoMaximo * ProporcionGracia;
			_timerGracia.Start();
			EmitSignal(SignalName.Agotado);
		}
	}

	public void Recargar()
	{
		_restante = TiempoMaximo;
		EmitSignal(SignalName.NivelCambiado, _restante / TiempoMaximo);
		_timerGracia.Stop();
	}

	public void PausarConsumo(bool pausado)
	{
		_consumoPausado = pausado;
	}

	private void OnTimerGraciaTimeout()
	{
		EmitSignal(SignalName.GraciaAgotada);
	}
}
