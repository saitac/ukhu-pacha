
public static class LaberintoConfig
{
    public static class Piso
    {
        public const int Variante1 = 2;
        public const int Variante2 = 3;
        public const int Variante3 = 4;
        public const int Variante4 = 5;

        public static readonly int[] Variantes = {Variante1, Variante2, Variante3, Variante4};
    }

    public static class Muro
    {
        public const int TerrainSet = 0;
        public const int Terrain = 0;
        public const int GrosorBorde = 1; // si lo subes, revisa TamanoMinimo.
        public const int GrosorMuroCompartido = GrosorBorde * 2;
    }

    public static class Grilla
    {
        public const int Ancho = 40;
        public const int Alto = 40;
    }

    public static class Bloque
    {
        public const int TamanoMinimo = 6;
        public const int TamanoMaximo = TamanoMinimo * 2;

        public const float FactorProporcion = 1.25f;

        public const int AnchoPuerta = 2;

        public const float PorcentajePuertasExtra = 0.2f;

    }

    public static class Pilares
    {
        public static readonly bool Activo = true;
        public const int DistanciaMinimaPiso = 2; // Celdas de piso libres entre un pilar y cualquier muro u otro pilar
        public const int Separacion = DistanciaMinimaPiso + 1; // Paso entre filas de pilares: el piso libre + la celda del siguiente pilar
        
        public const float ProbabilidadPorFila = 0.7f;

    }

    public static class Mutacion
    {
        public const float IntervaloMinSeg = 2.0f;
        public const float IntervaloMaxSeg = 4.0f;
        public const float PorcentajeCambio = 0.10f;
        public const int MargenPuertaInca = 1;
    }
}
