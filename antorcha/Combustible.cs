using Godot;

public partial class Combustible : Node
{
	[Signal]
	public delegate void NivelCambiadoEventHandler(float nivel);

	[Export]
	public float TiempoMaximo = 60f;

	private float _restante;

	private bool _consumoPausado = false;

	public override void _Ready()
	{
		_restante = TiempoMaximo;
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
	}

	public void Recargar()
	{
		_restante = TiempoMaximo;
		EmitSignal(SignalName.NivelCambiado, _restante / TiempoMaximo);
	}

	public void PausarConsumo(bool pausado)
	{
		_consumoPausado = pausado;
	}
}
