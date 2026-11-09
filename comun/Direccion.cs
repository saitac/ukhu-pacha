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

	public static Vector2 ComoVector(this Direccion direccion)
	{
		return direccion switch
		{
			Direccion.Abajo => Vector2.Down,
			Direccion.Arriba => Vector2.Up,
			Direccion.Derecha => Vector2.Right,
			Direccion.Izquierda => Vector2.Left,
			_ => throw new ArgumentOutOfRangeException(nameof(direccion), direccion, null),
		};
	}
}
