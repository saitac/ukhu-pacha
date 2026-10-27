using Godot;

public partial class BarraVigor : ProgressBar
{
	[Export]
	public Vigor Vigor;

	[Export]
	public float TiempoVisible = 2f;

	private Timer _timer;

	public override void _Ready()
	{
		Visible = false;

		if (Vigor == null)
		{
			GD.PushError($"{GetPath()}: falta asignar Vigor en el inspector");
			return;
		}

		_timer = GetNode<Timer>("Timer");
		_timer.WaitTime = TiempoVisible;
		_timer.Timeout += OnTimerTimeout;
		Vigor.Cambiado += OnVigorCambiado;
	}

	private void OnVigorCambiado(float porcentaje)
	{
		Value = porcentaje;
		Visible = true;
		_timer.Start();
	}

	private void OnTimerTimeout()
	{
		Visible = false;
	}
}
