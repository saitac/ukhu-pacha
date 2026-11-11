using Godot;

public partial class ContadorDiscos : CanvasLayer
{
	private static readonly Color ColorEncendido = Colors.White;
	private static readonly Color ColorApagado = new(0.25f, 0.25f, 0.25f);
	private TextureRect[] _intis;

	public override void _Ready()
	{
		_intis = new TextureRect[]
		{
			GetNode<TextureRect>("Intis/Inti1"),
			GetNode<TextureRect>("Intis/Inti2"),
			GetNode<TextureRect>("Intis/Inti3")
		};

		Actualizar(0);
	}

	public void Actualizar(int colocados)
	{
		for (int i = 0; i < _intis.Length; i++)
		{
			_intis[i].Modulate = (i < colocados) ? ColorEncendido : ColorApagado;
		}
	}
}
