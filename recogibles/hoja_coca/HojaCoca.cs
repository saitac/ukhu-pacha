using Godot;

public partial class HojaCoca : Area2D
{
	[Export]
	public float Amplitud = 4f;

	[Export]
	public float Duracion = 1f;

	private Node2D _visual;
	private Tween _tweenFlotacion;

	public override void _Ready()
	{
		_visual = GetNode<Node2D>("Visual");
		IniciarFlotacion();
	}

	private void IniciarFlotacion()
	{
		_tweenFlotacion?.Kill();
		_tweenFlotacion = CreateTween().SetLoops();

		_tweenFlotacion.SetTrans(Tween.TransitionType.Sine);
		_tweenFlotacion.SetEase(Tween.EaseType.InOut);

		_tweenFlotacion.TweenProperty(_visual, "position:y", _visual.Position.Y - Amplitud, Duracion);
		_tweenFlotacion.TweenProperty(_visual, "position:y", _visual.Position.Y, Duracion);

		float desfase = (float)GD.RandRange(0, Duracion * 2);
		_tweenFlotacion.CustomStep(desfase);
	}
}
