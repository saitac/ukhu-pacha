using System;
using Godot;

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

	public static Direccion DesdeVector(Vector2 vector)
	{

		if(Math.Abs(vector.X) > Math.Abs(vector.Y)) // se mueve en el eje X
		{
			return vector.X > 0 ? Direccion.Derecha : Direccion.Izquierda;
		}

		return vector.Y > 0 ? Direccion.Abajo : Direccion.Arriba;
	}
}
