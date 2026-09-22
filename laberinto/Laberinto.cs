using Godot;
using System;
using System.Collections.Generic;

public partial class Laberinto : Node2D
{
	TileMapLayer _Piso ;
	TileMapLayer _Muros;
	CharacterBody2D _Inca;
	// _mapa solo ve interior, no muro perimetral que es manejado por PintarMuroPerimetral()
	int[,] _mapa = new int[LaberintoConfig.Grilla.Alto - 2, LaberintoConfig.Grilla.Ancho - 2];
	
	public override void _Ready()
	{
		_Piso  = GetNode<TileMapLayer>("Piso");
		_Muros = GetNode<TileMapLayer>("Muros");
		_Muros.Modulate = new Color(0.6f, 0.6f, 0.6f);
		_Inca = GetNode<CharacterBody2D>("Inca");

		PintarMuroPerimetral();
		DibujarPiso();

		LlenarMapaInterior();
		_mapa = SuavizarMapa();

		ForzarZonaSpawnComoPiso(PosicionMundoAIndiceMapa(_Inca.GlobalPosition));

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
		}

		var regionesMuro = EncontrarRegiones(_mapa, LaberintoConfig.Celda.Muro);
		foreach(var region in regionesMuro)
		{
			GD.Print($"Region Muro de tamaño: {region.Count}");
		}*/

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
		RandomNumberGenerator rng = new RandomNumberGenerator();

		for(int fila = 0; fila < LaberintoConfig.Grilla.Alto; fila++)
		{
			for(int columna = 0; columna < LaberintoConfig.Grilla.Ancho; columna++)
			{
				indice = rng.RandiRange(0, LaberintoConfig.Piso.Variantes.Length - 1);
				sourceId = LaberintoConfig.Piso.Variantes[indice];
				_Piso .SetCell(new Vector2I(columna, fila),sourceId,new Vector2I(0,0));
			}
		}
	}

	private void LlenarMapaInterior()
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();

		for(int fila = 0; fila < _mapa.GetLength(0); fila++)
		{
			for(int columna = 0; columna < _mapa.GetLength(1); columna++)
			{
				_mapa[fila, columna] = rng.Randf() < LaberintoConfig.AutomataCelular.DensidadInicialMuro ? LaberintoConfig.Celda.Muro : LaberintoConfig.Celda.Piso;
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

}

