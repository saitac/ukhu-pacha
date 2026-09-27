using Godot;
using System;
using System.Collections.Generic;

public partial class Laberinto : Node2D
{
	RandomNumberGenerator _rng;
	TileMapLayer _Piso ;
	TileMapLayer _Muros;
	CharacterBody2D _Inca;
	// _mapa solo ve interior, no muro perimetral que es manejado por PintarMuroPerimetral()
	int[,] _mapa = new int[LaberintoConfig.Grilla.Alto - 2, LaberintoConfig.Grilla.Ancho - 2];

	List<Rect2I> _bloques;
	List<PuertaPosible> _puertasPosibles;

	private enum PuertaOrientacion
	{
		Horizontal, // bloques lado a lado (izquierdo | derecho): el muro es vertical y la puerta se cruza de izquierda a derecha
		Vertical // bloques uno encima del otro (arriba / abajo): el muro es horizontal y la puerta se cruza de arriba hacia abajo
	}

	// Posible puerta entre dos bloques vecinos (todavía no tallada).
	// PrimeraValida..UltimaValida: filas (Horizontal) o columnas (Vertical) donde puede ir la puerta.
	// PrimeraLineaMuro: columna (Horizontal) o fila (Vertical) del muro. La segunda línea es PrimeraLineaMuro + 1,
	// porque CerrarBloque pinta el borde completo de cada bloque y el muro compartido queda de 2 celdas de grosor.
	private record struct PuertaPosible(int IndiceBloqueA, int IndiceBloqueB, PuertaOrientacion Orientacion, 
	int PrimeraValida, int UltimaValida, int PrimeraLineaMuro);
	
	public override void _Ready()
	{
		_rng = new RandomNumberGenerator
		{
			Seed = 12345   // TEMPORAL: mapa fijo para comparar
		};
		
		_Piso  = GetNode<TileMapLayer>("Piso");
		_Muros = GetNode<TileMapLayer>("Muros");
		_Muros.Modulate = new Color(0.6f, 0.6f, 0.6f);
		_Inca = GetNode<CharacterBody2D>("Inca");

		PintarMuroPerimetral();
		DibujarPiso();

		LlenarMapaInterior();
		_mapa = SuavizarMapa();

		_bloques = [];
		Particionar(new Rect2I(new Vector2I(0,0), new Vector2I(_mapa.GetLength(1),_mapa.GetLength(0))), _bloques);

		//int sumArea = 0;
		//int totalDisueltos = 0;
		foreach(var bloque in _bloques)
		{
			//GD.Print($"Position: {bloque.Position} ; Size: {bloque.Size}");
			//sumArea += bloque.Size.X * bloque.Size.Y;
			//totalDisueltos += DisolverMurosDelgados(bloque);
			CerrarBloque(bloque);
		}
		//GD.Print($"N° Bloques: {_bloques.Count} ; Area total: {sumArea}");

		for(int fila = 0; fila < _mapa.GetLength(0); fila++)
		{
			string linea = "";
			for(int columna = 0; columna < _mapa.GetLength(1); columna++)
			{
				linea += _mapa[fila,columna]==LaberintoConfig.Celda.Piso ? "." : "X";	
			}
			GD.Print(linea);
		}

		/*var regiones = EncontrarRegiones(_mapa, LaberintoConfig.Celda.Piso);
		foreach(var region in regiones)
		{
			GD.Print($"Region de tamaño: {region.Count}");
		}*/
		
		
		/*var regionesMuro = EncontrarRegiones(_mapa, LaberintoConfig.Celda.Muro);
		GD.Print($"Regiones Muro: {regionesMuro.Count}");*/
		/*foreach(var region in regionesMuro)
		{
			GD.Print($"Region Muro de tamaño: {region.Count}");
		}*/

		_puertasPosibles = BuscarPuertas(_bloques);
		GD.Print($"Puertas posibles: {_puertasPosibles.Count}");

		ForzarZonaSpawnComoPiso(PosicionMundoAIndiceMapa(_Inca.GlobalPosition));

		DibujarMurosInterior();

		
		
	}

	private void PintarMuroPerimetral()
	{
		Godot.Collections.Array<Vector2I> perimetro = new Godot.Collections.Array<Vector2I>();

		// fila superior e inferior
		for(int columna=0; columna <= LaberintoConfig.Grilla.Ancho - 1; columna++)
		{
			perimetro.Add(new Vector2I(columna, 0));
			perimetro.Add(new Vector2I(columna, LaberintoConfig.Grilla.Alto -1));
		}

		// columna izquierda y derecha
		for(int fila=1; fila < LaberintoConfig.Grilla.Alto - 1; fila++)
		{
			perimetro.Add(new Vector2I(0, fila));
			perimetro.Add(new Vector2I(LaberintoConfig.Grilla.Ancho - 1, fila));
		}

		_Muros.SetCellsTerrainConnect(perimetro, LaberintoConfig.Muro.TerrainSet, 
		LaberintoConfig.Muro.Terrain, true);
	}

	private void DibujarPiso()
	{
		int indice;
		int sourceId;

		for(int fila = 0; fila < LaberintoConfig.Grilla.Alto; fila++)
		{
			for(int columna = 0; columna < LaberintoConfig.Grilla.Ancho; columna++)
			{
				indice = _rng.RandiRange(0, LaberintoConfig.Piso.Variantes.Length - 1);
				sourceId = LaberintoConfig.Piso.Variantes[indice];
				_Piso .SetCell(new Vector2I(columna, fila),sourceId,new Vector2I(0,0));
			}
		}
	}

	private void LlenarMapaInterior()
	{
		for(int fila = 0; fila < _mapa.GetLength(0); fila++)
		{
			for(int columna = 0; columna < _mapa.GetLength(1); columna++)
			{
				_mapa[fila, columna] = _rng.Randf() < LaberintoConfig.AutomataCelular.DensidadInicialMuro ? LaberintoConfig.Celda.Muro : LaberintoConfig.Celda.Piso;
			}
		}
	}

	private void DibujarMurosInterior()
	{
		Godot.Collections.Array<Vector2I> muros = new Godot.Collections.Array<Vector2I>();

		// Del _mapa, tomo las coordenadas de los muros
		for(int fila = 0; fila < _mapa.GetLength(0); fila++)
		{
			for(int columna = 0; columna < _mapa.GetLength(1); columna++)
			{
				if(_mapa[fila, columna] == LaberintoConfig.Celda.Muro)
				{
					muros.Add(new Vector2I(columna + 1, fila + 1));
				}
			}
		}

		// Renderizo los muros en el juego
		_Muros.SetCellsTerrainConnect(muros, LaberintoConfig.Muro.TerrainSet,
        LaberintoConfig.Muro.Terrain, true);

	}

	private int[,] SuavizarMapa()
	{
		int pasadas = LaberintoConfig.AutomataCelular.Pasadas;
		float vecinosMuro = 0;
		int pasada = 0;
		List<int[,]> copiasMapa = new List<int[,]>();

		while (pasada < pasadas)
		{
			if ( copiasMapa.Count == 0 )
			{
				copiasMapa.Add((int[,])_mapa.Clone());
			} else
			{
				copiasMapa.Add((int[,])copiasMapa[pasada - 1].Clone());
			}

			for(int fila = 0; fila < _mapa.GetLength(0); fila++)
			{
				for(int columna = 0; columna < _mapa.GetLength(1); columna++)
				{
					vecinosMuro = ContarVecinosMuro(fila, columna, pasada == 0 ? _mapa : copiasMapa[pasada -1]);
					if( vecinosMuro >= 0.625f ) // 5/8 Muro
					{
						copiasMapa[pasada][fila, columna] = LaberintoConfig.Celda.Muro;
					}
					if( vecinosMuro <= 0.375f ) // 3/8 Piso
					{
						copiasMapa[pasada][fila, columna] = LaberintoConfig.Celda.Piso;
					}
				}
			}

			pasada++;
		}

		return copiasMapa[pasadas - 1];
	}

	private float ContarVecinosMuro(int fila, int columna, int[,] mapa)
	{
		int vecinosMuro = 0;
		int vecinosReales = 0;

		for(int vfila = -1; vfila <= 1; vfila++)
		{
			for(int vcolumna = -1; vcolumna <= 1; vcolumna++)
			{
				if( vfila != 0 || vcolumna != 0)
				{
					int filaVecino = fila + vfila;
					int columnaVecino = columna + vcolumna;

					if(filaVecino >= 0 && columnaVecino >= 0 &&  filaVecino < mapa.GetLength(0) && columnaVecino < mapa.GetLength(1))
					{
						vecinosReales++;
						if(mapa[filaVecino, columnaVecino] == LaberintoConfig.Celda.Muro)
						{
							vecinosMuro++;
						}
					}
				}
			}
		}

		return (float)vecinosMuro / vecinosReales;
	}

	private List<List<Vector2I>> EncontrarRegiones(int[,] mapa, int tipoCelda)
	{
		List<List<Vector2I>> regiones = new List<List<Vector2I>>();
		bool[,] visitado = new bool[mapa.GetLength(0),mapa.GetLength(1)];

		for(int fila = 0; fila < mapa.GetLength(0); fila++)
		{
			for(int columna = 0; columna < mapa.GetLength(1); columna++)
			{
				if(mapa[fila, columna] == tipoCelda && !visitado[fila, columna])
				{
					regiones.Add(FloodFill(fila, columna, mapa, visitado, tipoCelda));
				}
			}
		}

		return regiones;
		
	}

	private List<Vector2I> FloodFill(int fila, int columna, int[,] mapa, bool[,] visitado, int tipoCelda)
	{
		List<Vector2I> region = new List<Vector2I>();
		Queue<Vector2I> cola = new Queue<Vector2I>();
			
		// marco la posición de la matriz como visitado
		visitado[fila, columna] = true;

		// encolo la posición de la matriz
		cola.Enqueue(new Vector2I(columna, fila));

		// agrego la posición a la region
		region.Add(new Vector2I(columna, fila));

		while(cola.Count > 0) {
			// recorro la cola mientras tenga algún elemento
			Vector2I vectorActual = cola.Dequeue();
			
			// valido cada uno de los vecinos: arriba, abajo, derecha, izquierda
			for(int mvto=1; mvto<5; mvto++)
			{
				int filaVecino=0;
				int columnaVecino=0;
				
				switch (mvto)
				{
					case 1: // arriba
						filaVecino = vectorActual.Y - 1;
						columnaVecino = vectorActual.X;
						break;
					case 2: // abajo
						filaVecino = vectorActual.Y + 1;
						columnaVecino = vectorActual.X;
						break;
					case 3: // derecha
						filaVecino = vectorActual.Y;
						columnaVecino = vectorActual.X + 1;
						break;
					case 4: // izquierda
						filaVecino = vectorActual.Y;
						columnaVecino = vectorActual.X - 1;
						break;
				}
				
				if(filaVecino >= 0 && filaVecino < mapa.GetLength(0) && 
				columnaVecino >= 0 && columnaVecino < mapa.GetLength(1) 
				&& mapa[filaVecino, columnaVecino] == tipoCelda
				&& !visitado[filaVecino, columnaVecino])
				{
					// lo marco como visitado
					visitado[filaVecino, columnaVecino] = true;

					// lo encolo
					cola.Enqueue(new Vector2I(columnaVecino, filaVecino));

					// lo agrego a la región
					region.Add(new Vector2I(columnaVecino, filaVecino));
				}
			}
		
		}

		return region;
	}

	private Vector2I PosicionMundoAIndiceMapa(Vector2 posicionMundo)
	{
		float tileX = posicionMundo.X / (_Piso.TileSet.TileSize.X * _Piso.Scale.X);
		float tileY = posicionMundo.Y / (_Piso.TileSet.TileSize.Y * _Piso.Scale.Y);

		int columnaMapa = (int)tileX - 1;
		int filaMapa = (int)tileY - 1;

		return new Vector2I(columnaMapa, filaMapa);
	}

	private void ForzarZonaSpawnComoPiso(Vector2I indiceInca)
	{
		for(int vfila = -1; vfila <= 1; vfila++)
		{
			for(int vcolumna = -1; vcolumna <= 1; vcolumna++)
			{
				int filaObjetivo = indiceInca.Y + vfila;
				int columnaObjetivo = indiceInca.X + vcolumna;

				if(filaObjetivo >= 0 && filaObjetivo < _mapa.GetLength(0)
				&& columnaObjetivo >= 0 && columnaObjetivo < _mapa.GetLength(1))
				{
					_mapa[filaObjetivo, columnaObjetivo] = LaberintoConfig.Celda.Piso;
				}
			}
		}
	}

	private void Particionar(Rect2I bloque, List<Rect2I> resultado)
	{
		// obtengo el tamaño mínimo y máximo que puede tener un bloque en el mapa y 
		// el factor de proporción que define si un bloque es muy ancho o muy alto

		int min = LaberintoConfig.Bloque.TamanoMinimo;
		int max = LaberintoConfig.Bloque.TamanoMaximo;
		float factorProporcion = LaberintoConfig.Bloque.FactorProporcion;

		bool puedeCortarHorizontal = bloque.Size.Y >= max; // alto - válido solo porque TamanoMaximo = 2 * TamanoMinimo
		bool puedeCortarVertical = bloque.Size.X >= max; // ancho - válido solo porque TamanoMaximo = 2 * TamanoMinimo


		// Ya no se puede cortar por que sería menor que el tamaño mínimo
		if(!puedeCortarHorizontal && !puedeCortarVertical)
		{
			resultado.Add(bloque);
			return;
		}

		// se elije eje a cortar
		bool cortarVertical;
		if (puedeCortarVertical && !puedeCortarHorizontal)
		{
			cortarVertical = true;
		} else if(puedeCortarHorizontal && !puedeCortarVertical)
		{
			cortarVertical = false;
		}else if(bloque.Size.X > bloque.Size.Y * factorProporcion)
		{
			// muy ancho
			cortarVertical = true;
		} else if(bloque.Size.Y > bloque.Size.X * factorProporcion)
		{
			// muy alto
			cortarVertical = false;
		} else
		{
			cortarVertical = _rng.Randf() < 0.5f;
		}
				
		// Cortar
		int puntoDeCorte;
		Rect2I bloqueA;
		Rect2I bloqueB;
		if (cortarVertical) // parto el ancho (Size.X)
		{
			puntoDeCorte = _rng.RandiRange(min, bloque.Size.X - min);

			bloqueA = new Rect2I(
				bloque.Position, new Vector2I(puntoDeCorte, bloque.Size.Y)
				);

			bloqueB = new Rect2I(new Vector2I(bloque.Position.X + puntoDeCorte,
			bloque.Position.Y), new Vector2I(bloque.Size.X - puntoDeCorte, bloque.Size.Y)); 
		} else // corte horizontal, parto el alto
		{
			puntoDeCorte = _rng.RandiRange(min, bloque.Size.Y - min);

			bloqueA = new Rect2I(
				bloque.Position, new Vector2I(bloque.Size.X, puntoDeCorte)
				);

			bloqueB = new Rect2I(new Vector2I(bloque.Position.X,
			bloque.Position.Y + puntoDeCorte), new Vector2I(bloque.Size.X, bloque.Size.Y - puntoDeCorte)); 
		}

		// Recursivo, cada mitad se vuelve a partir mientras mida al menos el doble del mínimo
		Particionar(bloqueA, resultado);
		Particionar(bloqueB, resultado);
	}

	private bool EsMuroDelgado(int fila, int columna)
	{
		int filaVecinoInf = fila + 1;
		int filaVecinoSup = fila - 1;
		int columnaVecinoDer = columna + 1;
		int columnaVecinoIzq = columna - 1;

		// devuelve false si se está analizando una celda piso
		if(_mapa[fila, columna] == LaberintoConfig.Celda.Piso){ return false;}

		// regresa true si la celda superior y inferior o la celda izquierda y la celda derecha no es muro,
		// caso contrario, retorna false
		// El método no revisa límites, solo debe llamarse con celdas interiores de un bloque.

		return (_mapa[filaVecinoSup, columna] == LaberintoConfig.Celda.Piso && _mapa[filaVecinoInf, columna] == LaberintoConfig.Celda.Piso)
		|| (_mapa[fila, columnaVecinoIzq] == LaberintoConfig.Celda.Piso && _mapa[fila, columnaVecinoDer] == LaberintoConfig.Celda.Piso);		
	}

	private int DisolverMurosDelgados(Rect2I bloque)
	{
		List<Vector2I> aDisolver = new List<Vector2I>();

		for(int fila = bloque.Position.Y + 1; fila < bloque.End.Y - 1; fila++)
		{
			for(int columna = bloque.Position.X + 1; columna < bloque.End.X - 1; columna++)
			{
				if(EsMuroDelgado(fila, columna)){ aDisolver.Add(new Vector2I(columna, fila)); }
			}
		}

		foreach(Vector2I vector in aDisolver)
		{
			_mapa[vector.Y, vector.X] = LaberintoConfig.Celda.Piso; 
		}

		return aDisolver.Count;
	}

	private void CerrarBloque(Rect2I bloque)
	{
		for(int fila = bloque.Position.Y; fila < bloque.End.Y; fila++)
		{
			for(int columna = bloque.Position.X; columna < bloque.End.X; columna++)
			{
				bool esBorde =  fila == bloque.Position.Y || fila == bloque.End.Y - 1 
				|| columna == bloque.Position.X || columna == bloque.End.X - 1;

				_mapa[fila, columna] = esBorde ? LaberintoConfig.Celda.Muro : LaberintoConfig.Celda.Piso;

			}
		}
	}

	private List<PuertaPosible> BuscarPuertas(List<Rect2I> bloques)
	{
		List<PuertaPosible> puertasPosibles = [];

		for(int bloqueAindex = 0; bloqueAindex < bloques.Count; bloqueAindex++)
		{
			for(int bloqueBindex = 0; bloqueBindex < bloques.Count; bloqueBindex++)
			{
				if(bloqueAindex == bloqueBindex) { continue; }
				PuertaPosible? resultado;
				
				resultado = BuscarPuertaHorizontal(bloques[bloqueAindex], bloques[bloqueBindex], bloqueAindex, bloqueBindex);
				if(resultado is PuertaPosible puertaH) {puertasPosibles.Add(puertaH);}

				resultado = BuscarPuertaVertical(bloques[bloqueAindex], bloques[bloqueBindex], bloqueAindex, bloqueBindex);
				if(resultado is PuertaPosible puertaV) {puertasPosibles.Add(puertaV);}
				
			}
		}

		return puertasPosibles;

	}


	private PuertaPosible? BuscarPuertaHorizontal(Rect2I izquierdo, Rect2I derecho, int bloquesIndiceIzquierdo,  int bloquesIndiceDerecho)
	{

		int ultimaColumnaIzquierdo = izquierdo.End.X - 1;
		int primeraColumnaDerecho = derecho.Position.X;

		int primeraFilaIzquierdo = izquierdo.Position.Y;
		int ultimaFilaIzquierdo = izquierdo.End.Y - 1;

		int primeraFilaDerecho = derecho.Position.Y;
		int ultimaFilaDerecho = derecho.End.Y - 1;

		// Se tocan?

		if(ultimaColumnaIzquierdo + 1 != primeraColumnaDerecho) { return null; } 
		// Filas en comun
		int inicioComun = Math.Max(primeraFilaIzquierdo, primeraFilaDerecho);
		int finComun = Math.Min(ultimaFilaIzquierdo, ultimaFilaDerecho);

		// Filas válidas para una puerta
		int primeraValida = inicioComun + 1;
		int ultimaValida = finComun - 1;

		// ¿Cabe la puerta?
		int cantidadValidas = ultimaValida - primeraValida + 1;
		if(cantidadValidas < LaberintoConfig.Bloque.AnchoPuerta){ return null; }

		// imprimir resultado
		GD.Print($"H Vecinos: {izquierdo} y {derecho}, filas validas: {primeraValida} - {ultimaValida}, columnas del muro: {ultimaColumnaIzquierdo} - {primeraColumnaDerecho}");
		
		return new PuertaPosible(bloquesIndiceIzquierdo, bloquesIndiceDerecho, PuertaOrientacion.Horizontal, primeraValida, ultimaValida, ultimaColumnaIzquierdo);

	}

	private PuertaPosible? BuscarPuertaVertical(Rect2I arriba, Rect2I abajo, int bloquesIndiceArriba,  int bloquesIndiceAbajo)
	{

		int ultimaFilaArriba = arriba.End.Y - 1;
		int primeraFilaAbajo = abajo.Position.Y;

		int primeraColumnaArriba = arriba.Position.X;
		int ultimaColumnaArriba = arriba.End.X - 1;

		int primeraColumnaAbajo = abajo.Position.X;
		int ultimaColumnaAbajo = abajo.End.X - 1;

		// Se tocan?

		if(ultimaFilaArriba + 1 != primeraFilaAbajo) { return null; }

		// Columnas en comun
		int inicioComun = Math.Max(primeraColumnaArriba, primeraColumnaAbajo);
		int finComun = Math.Min(ultimaColumnaArriba, ultimaColumnaAbajo);

		// Columnas válidas para una puerta
		int primeraValida = inicioComun + 1;
		int ultimaValida = finComun - 1;

		// ¿Cabe la puerta?
		int cantidadValidas = ultimaValida - primeraValida + 1;
		if(cantidadValidas < LaberintoConfig.Bloque.AnchoPuerta){ return null; }

		// imprimir resultado
		GD.Print($"V Vecinos: {arriba} y {abajo}, columnas validas: {primeraValida} - {ultimaValida}, filas del muro: {ultimaFilaArriba} - {primeraFilaAbajo}");

		return new PuertaPosible(bloquesIndiceArriba, bloquesIndiceAbajo, PuertaOrientacion.Vertical, primeraValida, ultimaValida, ultimaFilaArriba);

	}

}
