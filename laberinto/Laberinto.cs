using Godot;
using System;
using System.Collections.Generic;

public partial class Laberinto : Node2D
{
	RandomNumberGenerator _rng;
	TileMapLayer _Piso ;
	TileMapLayer _Muros;
	CharacterBody2D _Inca;

	Timer _TimerMutacion;

	private enum Celda
	{
		Piso,
		Muro
	}

	// _mapa solo ve interior, no muro perimetral que es manejado por PintarMuroPerimetral()
	Celda[,] _mapa = new Celda[LaberintoConfig.Grilla.Alto - 2, LaberintoConfig.Grilla.Ancho - 2];

	List<Rect2I> _bloques;
	List<PuertaPosible> _puertasPosibles;
	private Dictionary<int, int> _puertasAbiertas;

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
			//Seed = 12345   // descomentar para mapa reproducible
		};

		GD.Print($"Semilla: {_rng.Seed}");
		
		_Piso  = GetNode<TileMapLayer>("Piso");
		_Muros = GetNode<TileMapLayer>("Muros");
		_Muros.Modulate = new Color(0.6f, 0.6f, 0.6f);
		_Inca = GetNode<CharacterBody2D>("Inca");
		_puertasAbiertas = new Dictionary<int, int>();

		PintarMuroPerimetral();
		DibujarPiso();

		_bloques = [];
		Particionar(new Rect2I(new Vector2I(0,0), new Vector2I(_mapa.GetLength(1),_mapa.GetLength(0))), _bloques);

		foreach(var bloque in _bloques)
		{
			CerrarBloque(bloque);
		}
		
		_puertasPosibles = BuscarPuertas(_bloques);
		
		int bloqueInca = IndiceBloqueDe(PosicionMundoAIndiceMapa(_Inca.GlobalPosition));

		ConectarSalas(bloqueInca);
		
		AbrirPuertasExtra();
		
		ColocarPilares(bloqueInca);

		// Centro del bloque del inca
		Vector2I centro = _bloques[bloqueInca].GetCenter();
		
		// Se le asigna al inca la posición en el centro del bloque en el que se encuentra.
		_Inca.GlobalPosition =  IndiceMapaAPosicionMundo(centro);
		
		// Alerta - no debe existir cuartos sin acceso
		int cantidadRegiones = EncontrarRegiones(_mapa, Celda.Piso).Count;
		if(cantidadRegiones != 1)
		{
			GD.PushError($"Regiones de piso: {cantidadRegiones}, se rompió la conectividad.");	
		}

		DibujarMurosInterior();

		_TimerMutacion = GetNode<Timer>("TimerMutacion");
		_TimerMutacion.Timeout += OnCicloPuertas;
		ProgramarSiguienteCiclo();
		
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

	private void DibujarMurosInterior()
	{
		Godot.Collections.Array<Vector2I> muros = new Godot.Collections.Array<Vector2I>();

		// Del _mapa, tomo las coordenadas de los muros
		for(int fila = 0; fila < _mapa.GetLength(0); fila++)
		{
			for(int columna = 0; columna < _mapa.GetLength(1); columna++)
			{
				if(_mapa[fila, columna] == Celda.Muro)
				{
					muros.Add(new Vector2I(columna + 1, fila + 1));
				}
			}
		}

		// Renderizo los muros en el juego
		_Muros.SetCellsTerrainConnect(muros, LaberintoConfig.Muro.TerrainSet,
        LaberintoConfig.Muro.Terrain, true);

	}

	private List<List<Vector2I>> EncontrarRegiones(Celda[,] mapa, Celda tipoCelda)
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

	private List<Vector2I> FloodFill(int fila, int columna, Celda[,] mapa, bool[,] visitado, Celda tipoCelda)
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

	private Vector2 IndiceMapaAPosicionMundo(Vector2I indiceMapa)
	{
		// De índice de mapa a celda de TileMapLayer se le agrega (1,1) ya que mapa no considera los límites.
		Vector2I celdaTile = indiceMapa + Vector2I.One;
		// centro del tile, sin escala
		Vector2 posicionLocal = _Piso.MapToLocal(celdaTile);
		// Aplica escala y posición => píxeles de mundo.
		return _Piso.ToGlobal(posicionLocal); 
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

	private void CerrarBloque(Rect2I bloque)
	{
		for(int fila = bloque.Position.Y; fila < bloque.End.Y; fila++)
		{
			for(int columna = bloque.Position.X; columna < bloque.End.X; columna++)
			{
				bool esBorde =  fila == bloque.Position.Y || fila == bloque.End.Y - 1 
				|| columna == bloque.Position.X || columna == bloque.End.X - 1;

				_mapa[fila, columna] = esBorde ? Celda.Muro : Celda.Piso;

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

		return new PuertaPosible(bloquesIndiceArriba, bloquesIndiceAbajo, PuertaOrientacion.Vertical, primeraValida, ultimaValida, ultimaFilaArriba);
	}

	private int IndiceBloqueDe(Vector2I celda)
	{
		for(int i = 0; i < _bloques.Count; i++)
		{
			Rect2I bloque = _bloques[i]; 
			if (bloque.Position.X <= celda.X && bloque.End.X - 1 >= celda.X && bloque.Position.Y <= celda.Y && bloque.End.Y - 1 >= celda.Y)
			{
				return i;
			}
		}
		return -1;
	}

	private void PintarPuerta(PuertaPosible puerta, int inicio, Celda valor)
	{
		for(int ancho = 0; ancho < LaberintoConfig.Bloque.AnchoPuerta; ancho++)
		{
			int pos = inicio + ancho;
			for(int linea = puerta.PrimeraLineaMuro; linea <= puerta.PrimeraLineaMuro + 1; linea++)
			// + 1: el muro entre bloques es doble (cada bloque cierra su propio perímetro)
			{
				if(puerta.Orientacion == PuertaOrientacion.Horizontal)
				{
					_mapa[pos, linea] = valor;
				} else if (puerta.Orientacion == PuertaOrientacion.Vertical)
				{
					_mapa[linea, pos] = valor;
				}
			} 
		}
		
	}

	private void ImprimirMapa()
	{
		for(int fila = 0; fila < _mapa.GetLength(0); fila++)
		{
			string linea = "";
			for(int columna = 0; columna < _mapa.GetLength(1); columna++)
			{
				linea += _mapa[fila,columna] == Celda.Piso ? "." : "X";	
			}
			GD.Print(linea);
		}
	}

	private void ConectarSalas(int bloqueInca)
	{
		// backtracker

		// Creo un arreglo de bool del tamaño de la cantidad de bloques y todos inicialmente en false
		// para marcar los bloque que ya fueron visitados por el algoritmo, adicionalmente creo una pila vacía.
		bool[] visitado = new bool[_bloques.Count];
		Stack<int> pila = new();

		// El bloque donde se encuentra el inca es agregado a la pila y se marca como bloque visitado.
		pila.Push(bloqueInca);
		visitado[bloqueInca] = true;

		while(pila.Count != 0)
		{
			// obtengo el elemento que se encuentra encima de la pila sin eliminarlo.
			int actual = pila.Peek();

			// busco en la lista de _puertasPosibles, que bloques colindan con el bloque actual para agregarlas
			// como candidatas
			List<int> candidatas = [];
			for(int p = 0; p < _puertasPosibles.Count; p++)
			{
				PuertaPosible puerta = _puertasPosibles[p];
				// valido si actual es vecina de algunos de los dos bloques con puertas vecinas
				// si no es vecina la salto por que no toca la sala actual
				if(puerta.IndiceBloqueA != actual && puerta.IndiceBloqueB != actual) { continue; }

				// Obtengo el vecino válido
				int vecino = (puerta.IndiceBloqueA == actual) ? puerta.IndiceBloqueB : puerta.IndiceBloqueA;

				// Si el vecino no fue visitado, entonces lo agrego como candidato.
				if (!visitado[vecino])
				{
					candidatas.Add(p);
				}
			}

			// Callejón sin salida: valido si existe alguna candidata en la lista
			// Si no hay candidatas, entonces retrocedo por el hilo.
			if(candidatas.Count == 0)
			{
				pila.Pop();
				continue;
			}

			// Si existen candidatas, elijo una al azar
			int posicion = _rng.RandiRange(0, candidatas.Count - 1);
			int elegida = candidatas[posicion];
			PuertaPosible puertaElegida = _puertasPosibles[elegida];

			int siguiente = (puertaElegida.IndiceBloqueA == actual) ? puertaElegida.IndiceBloqueB : puertaElegida.IndiceBloqueA;

			// Tallar y registrar
			int inicio = _rng.RandiRange(puertaElegida.PrimeraValida, puertaElegida.UltimaValida - LaberintoConfig.Bloque.AnchoPuerta + 1);
			PintarPuerta(puertaElegida, inicio, Celda.Piso);
			// Agreo al diccionario a la clave "elegida", que es el índice en _puertasPosibles, el valor "inicio" donde empieza el hueco.
			_puertasAbiertas[elegida] = inicio;

			// avanzar
			visitado[siguiente] = true;
			pila.Push(siguiente);

		}

		/*
		
		┌─────┬─────┬─────┐
		│  0  │  1  │  2  │
		├─────┼─────┴─────┤
		│  3  │     4     │
		└─────┴───────────┘

		[0]·····(1)·····(2)		[N] = visitada
 		:       :       :       (N) = sin visitar
		(3)·····(    4    )     ◄   = actual (Peek)
		
		Las puertas posibles son 0-1, 1-2, 0-3, 1-4, 2-4 y 3-4. El Inca está en la sala 0.

		Paso 1: actual = 0, candidatas: 1, 3 → el azar elige 1

		[0]═════[1]◄····(2)        pila: [0, 1]
 		:       :       :         
		(3)·····(    4    )        Push(1)

		Paso 2: actual = 1, candidatas: 2, 4 → el azar elige 4

		[0]═════[1]·····(2)        pila: [0, 1, 4]
 		:       ║       :         
		(3)·····[    4    ]◄       Push(4)

		Paso 3: actual = 4, candidatas: 2, 3 → el azar elige 2

		[0]═════[1]·····[2]◄       pila: [0, 1, 4, 2]
		:       ║       ║         
		(3)·····[    4    ]        Push(2)

		Paso 4: actual = 2, vecinos 1 y 4 ya visitados → callejón sin salida

		[0]═════[1]·····[2]        pila: [0, 1, 4]
		:       ║       ║         
		(3)·····[    4    ]◄       Pop() → retrocede por el hilo

		Paso 5: actual = 4 otra vez, candidatas: 3 → 3

		[0]═════[1]·····[2]        pila: [0, 1, 4, 3]
		:       ║       ║         
		[3]◄════[    4    ]        Push(3)

		Pasos 6 a 9: todas las salas están visitadas, así que cada vuelta es callejón → Pop, Pop, Pop, Pop

		pila: [0,1,4,3] → [0,1,4] → [0,1] → [0] → [ ]   → termina el while
		
		Resultado final

		[0]═════[1]·····[2]
		:       ║       ║
		[3]═════[    4    ]
		
		*/

	}

	private void AbrirPuertasExtra()
	{
		// Identificar las sobrantes (índices de puertas que no están abiertas)

		List<int> sobrantes = [];
		
		for(int p = 0; p < _puertasPosibles.Count; p++)
		{
			if (!_puertasAbiertas.ContainsKey(p))
			{
				sobrantes.Add(p);
			}
		}

		// Definir cuantas puertas adicionales abrir
		int puertasAdicionales = (int)(sobrantes.Count * LaberintoConfig.Bloque.PorcentajePuertasExtra);

		// Elegir sin repetir y tallar puerta
		for(int repetir = 0; repetir < puertasAdicionales; repetir++)
		{
			// Selecciono un sobrante tomando aleatoriamente su posición
			int posicion = _rng.RandiRange(0, sobrantes.Count - 1);

			// Obtengo el índice la puerta en _puertasPosibles
			int indicePuerta = sobrantes[posicion];

			// Elimino de la lista el sobrante de la puerta a trabajar
			sobrantes.RemoveAt(posicion);
			
			// Selecciono la puerta a trabajar
			PuertaPosible puerta = _puertasPosibles[indicePuerta];

			// Tallo la puerta y la asigno al arreglo de _puertasAbiertas

			int inicio = _rng.RandiRange(puerta.PrimeraValida, puerta.UltimaValida - LaberintoConfig.Bloque.AnchoPuerta + 1);
			PintarPuerta(puerta, inicio, Celda.Piso);
			_puertasAbiertas[indicePuerta] = inicio;
		}
	}

	private void ColocarPilaresEnBloque(Rect2I bloque)
	{
		const int espesorMuro = 1; // CerrarBloque pone 1 celda de borde

		// Obtengo la zona donde podría colocar pilares, la zona debe ser más pequeña que la original en espesor del muro
		// más un margen mínimo dado
		Rect2I zona = bloque.Grow(-(espesorMuro + LaberintoConfig.Pilares.DistanciaMinimaPiso));

		// Si zona no tiene área entonces no hace nada
		if (!zona.HasArea())
		{
			return;
		}

		for(int fila = zona.Position.Y; fila < zona.End.Y; fila += LaberintoConfig.Pilares.Separacion)
		{
			if(_rng.Randf() < LaberintoConfig.Pilares.ProbabilidadPorFila)
			{
				int columna = _rng.RandiRange(zona.Position.X, zona.End.X-1);
				_mapa[fila, columna] = Celda.Muro;
			}
		}

	}

	private void ColocarPilares(int bloqueInca)
	{
		if (!LaberintoConfig.Pilares.Activo)
		{
			return;
		}

		for(int indice = 0; indice < _bloques.Count; indice++)
		{
			if(indice == bloqueInca)
			{
				continue;
			}

			ColocarPilaresEnBloque(_bloques[indice]);
		}
	}

	private void ProgramarSiguienteCiclo()
	{
		_TimerMutacion.WaitTime = _rng.RandfRange(LaberintoConfig.Mutacion.IntervaloMinSeg, LaberintoConfig.Mutacion.IntervaloMaxSeg);
		_TimerMutacion.Start();
	}

	private void OnCicloPuertas()
	{
		//GD.Print($"Mutación tras: {_TimerMutacion.WaitTime:F1} segundos");
		MutarPuertas();

		if (!SalasConectadas(-1))
		{
			GD.PushError("Salas no conectadas");
		}
		
		ProgramarSiguienteCiclo();
	}

	private bool SalasConectadas(int puertaIgnorada)
	{
		// Creo un arreglo de visitados para marcar los bloques que ya fueron visitados por la función
		bool[] visitado = new bool[_bloques.Count];
		
		// Creo la pila que me permitirá realizar el DFS en la lógica 
		Stack<int> pila = new();

		// Empiezo con el bloque 0, para ello lo agrego a la pila y la marco como visitada
		pila.Push(0);
		visitado[0] = true;

		while(pila.Count != 0)
		{
			// Obtengo el índice del bloque actual que se encuentra encima de la pila sin eliminarlo 
			int actual = pila.Peek();

			// Recorro las puertas posibles para encontrar sus vecinos con puertas abiertas
			List<int> candidatas = [];
			for(int pp = 0; pp < _puertasPosibles.Count; pp++)
			{
				PuertaPosible puerta = _puertasPosibles[pp];

				// Valido si la puerta pp toca a actual
				if(puerta.IndiceBloqueA != actual && puerta.IndiceBloqueB != actual)
				{
					continue;
				}

				// valido si la puerta pp esta abierta y no es la ignorada
				if (!_puertasAbiertas.ContainsKey(pp) || pp == puertaIgnorada)
				{
					continue;
				}

				// Obtengo al vecino valido
				int vecino = puerta.IndiceBloqueA == actual ? puerta.IndiceBloqueB : puerta.IndiceBloqueA;

				//  Si vecino no fue visitado, lo agrego como candidato
				if (!visitado[vecino])
				{
					candidatas.Add(vecino);
				}
			}

			// Callejón sin salida: si no hay candidatas, retrocedo por el hilo
			if(candidatas.Count == 0)
			{
				pila.Pop();
				continue;
			}

			// Si existen candidatas, elijo la primera
			visitado[candidatas[0]] = true;
			pila.Push(candidatas[0]);
		}

		// valido si se pudo acceder a todos los bloques, es decir si están todos conectados
		foreach(bool v in visitado)
		{
			if (!v)
			{
				return false;
			}
		}

		return true;
	}

	private void MutarPuertas()
	{
		int intercambios = Mathf.Max(1, Mathf.RoundToInt(_puertasAbiertas.Count * LaberintoConfig.Mutacion.PorcentajeCambio));

		GD.Print($"--- Ciclo: {intercambios} intercambios, {_puertasAbiertas.Count} puertas abiertas");

		for(int intercambio = 0; intercambio < intercambios; intercambio++)
		{
			// Busco en puertasPosibles aquellas puertas que no están abiertas (_puertasAbiertas), para ello
			// valido si el índice de cada _puertaPosible en _puertasPosibles es key en _puertasAbiertas
			// Si no es una key, entonces la agrego a puertasCerradas.
			List<int> puertasCerradas = []; 
			for(int ppIndex = 0; ppIndex < _puertasPosibles.Count; ppIndex++)
			{
				if (!_puertasAbiertas.ContainsKey(ppIndex))
				{
					puertasCerradas.Add(ppIndex);
				}
			}

			// Si ya no quedan puertas cerradas, me salto los ciclos.
			if(puertasCerradas.Count == 0)
			{
				break;
			}

			// Selecciono al azar una de las puertas cerradas para abrirla.
			int indiceAbrir = puertasCerradas[_rng.RandiRange(0, puertasCerradas.Count-1)];
			PuertaPosible puertaAbrir = _puertasPosibles[indiceAbrir];

			GD.Print($"Puerta abierta: {indiceAbrir}");

			// Tallar (abrir) puerta y registrarla como abierta
			int inicio = _rng.RandiRange(puertaAbrir.PrimeraValida, puertaAbrir.UltimaValida - LaberintoConfig.Bloque.AnchoPuerta + 1);
			PintarPuerta(puertaAbrir, inicio, Celda.Piso);
			_puertasAbiertas[indiceAbrir] = inicio;


			// Busco puertas abiertas (excluyendo la nueva -- indiceAbrir [puertaAabrir]) candidatas a cerrar.
			List<int> candidatasCerrar = [];
			foreach(int key in _puertasAbiertas.Keys)
			{
				if(key == indiceAbrir)
				{
					continue;
				}

				if (!SalasConectadas(key))
				{
					continue;
				}

				candidatasCerrar.Add(key);
			}

			// Si hay candidatas a cerrar, selecciono una al azar y la cierro.
			if(candidatasCerrar.Count != 0)
			{
				int indiceCerrar = candidatasCerrar[_rng.RandiRange(0, candidatasCerrar.Count-1)];
				
				GD.Print($"Puerta cerrada: {indiceCerrar}");

				PuertaPosible puertaCerrar  = _puertasPosibles[indiceCerrar];
				PintarPuerta(puertaCerrar , _puertasAbiertas[indiceCerrar], Celda.Muro);
				_puertasAbiertas.Remove(indiceCerrar);
			}
			else
			{
				GD.Print("Sin candidatos a cerrar");
			}

		}
	}


}


