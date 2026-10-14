using Godot;

public partial class Combustible : Node
{
	[Signal]
	public delegate void NivelCambiadoEventHandler(float nivel);

	[Signal]
	public delegate void AgotadoEventHandler();

	[Export]
	public float TiempoMaximo = 60f;

	private float _restante;

	public override void _Ready()
	{
		_restante = TiempoMaximo;
	}

	public override void _Process(double delta)
	{
		if (_restante <= 0f)
		{
			return;
		}

		_restante = Mathf.Max(_restante - (float)delta, 0f);

		EmitSignal(SignalName.NivelCambiado, _restante / TiempoMaximo);

		if (_restante <= 0f)
		{
			EmitSignal(SignalName.Agotado);
		}
	}

	public void Recargar()
	{
		_restante = TiempoMaximo;
	}
}
