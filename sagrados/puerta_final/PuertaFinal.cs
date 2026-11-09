using Godot;

public partial class PuertaFinal : StaticBody2D
{
	/// <summary>
	/// Se emite al colocar cada disco, EXCEPTO el último.
	/// Al colocar el último se emite Abierta en su lugar.
	/// </summary>
	[Signal]
	public delegate void DiscoColocadoEventHandler(int colocados);

	/// <summary>
	/// Se emite al colocar el último disco; la losa desaparece
	/// </summary>
	[Signal]
	public delegate void AbiertaEventHandler();

	private Sprite2D _losa;
	private Sprite2D[] _discos;
	private Area2D _zona;
	private DiscoInti _discoInti = null;
	private int _colocados = 0;

	public int TotalDiscos => _discos.Length;

	public override void _Ready()
	{
		_losa = GetNode<Sprite2D>("Losa");

		_discos = new Sprite2D[]
		{
			GetNode<Sprite2D>("Discos/Disco1"),
			GetNode<Sprite2D>("Discos/Disco2"),
			GetNode<Sprite2D>("Discos/Disco3")
		};

		_zona = GetNode<Area2D>("ZonaColocacion");
		_zona.BodyEntered += IntentarColocar;
	}

	private void ColocarDisco()
	{
		if (_colocados >= _discos.Length)
		{
			GD.PushWarning("PuertaFinal ya tiene todos sus discos; ColocarDisco() ignorado");
			return;
		}

		_discos[_colocados].Visible = true;
		_colocados++;

		if (_colocados == _discos.Length)
		{
			_losa.Visible = false;
			EmitSignal(SignalName.Abierta);
		}
		else
		{
			EmitSignal(SignalName.DiscoColocado, _colocados);
		}
	}

	private void AlTomarDisco(Inca portador)
	{
		if (_zona.OverlapsBody(portador))
		{
			IntentarColocar(portador);
		}
	}

	public void Inicializar(DiscoInti disco)
	{
		_discoInti = disco;
		_discoInti.Tomado += AlTomarDisco;
	}

	private void IntentarColocar(Node2D body)
	{
		if (body is not Inca)
		{
			return;
		}

		if (_discoInti == null || !_discoInti.EstaCargado)
		{
			return;
		}

		_discoInti.Entregar();
		ColocarDisco();
	}
}
