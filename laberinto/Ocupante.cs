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
}
