using System;

public enum Ocupante
{
	HojaCoca,
	DiscoInti,
	Totem,
	PuertaFinal
}

public static class OcupanteExtensions
{
	public static bool EsObstaculo(this Ocupante ocupante)
	{
		return ocupante switch
		{
			Ocupante.Totem or Ocupante.PuertaFinal => true,
			Ocupante.HojaCoca or Ocupante.DiscoInti => false,
			_ => throw new ArgumentOutOfRangeException(nameof(ocupante), ocupante, null),
		};
	}

	/// <summary>
	/// Indica si otro objeto puede colocarse en la misma celda que este ocupante.
	/// Solo se usa como último recurso, cuando no queda ninguna celda libre.
	/// </summary>
	/// <remarks>
	/// No es lo mismo que <see cref="EsObstaculo"/>: el disco de Inti no bloquea el paso,
	/// pero tampoco admite nada encima, porque perdería su registro en el laberinto.
	/// </remarks>
	public static bool PermiteSuperponer(this Ocupante ocupante)
	{
		return ocupante switch
		{
			Ocupante.Totem or Ocupante.PuertaFinal or Ocupante.DiscoInti => false,
			Ocupante.HojaCoca => true,
			_ => throw new ArgumentOutOfRangeException(nameof(ocupante), ocupante, null),
		};
	}
}
