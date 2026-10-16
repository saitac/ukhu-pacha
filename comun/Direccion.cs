using System;

public enum Direccion
{
	Arriba,
	Abajo,
	Izquierda,
	Derecha
}

public static class DireccionExtensions
{
	public static string Sufijo(this Direccion direccion)
	{
		return direccion switch
		{
			Direccion.Abajo => "down",
			Direccion.Arriba => "up",
			Direccion.Derecha => "right",
			Direccion.Izquierda => "left",
			_ => throw new ArgumentOutOfRangeException(nameof(direccion), direccion, null),
		};
	}
}
