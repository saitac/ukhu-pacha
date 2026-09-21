
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
    }

    public static class Grilla
    {
        public const int Ancho = 40;
        public const int Alto = 40;
    }

    public static class Celda
    {
        public const int Piso = 0;
        public const int Muro = 1;
    }

    public static class AutomataCelular
    {
        public const int Pasadas = 2; //4;
        public const float DensidadInicialMuro = 0.55f; // 0.45f;

    }
}