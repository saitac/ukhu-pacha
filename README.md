# Ukhu Pacha

Juego de terror/survival 2D con temática andina/inca, hecho en **Godot 4.7 (.NET)** con **C#**.

## Premisa

Un príncipe inca huye de la conquista española y se refugia en una cueva ancestral, un lugar sagrado anterior incluso al propio Imperio Inca, ligado al culto a Viracocha y a las fuerzas del **Uku Pacha** (el mundo de abajo). La cueva no es un espacio estático: está viva, muta constantemente, y en sus profundidades habitan entidades mitológicas andinas. La única salida es encontrar la **Ciudad Perdida**.

## Pilares de diseño

1. **Impotencia táctica** — el jugador nunca es más fuerte que las amenazas; sobrevive con ingenio, no con fuerza.
2. **El espacio como enemigo** — el verdadero antagonista no son los monstruos, es el laberinto mismo.
3. **Rejugabilidad orgánica** — cada partida es distinta gracias a la mutación procedural y las semillas aleatorias.

El diseño completo (mecánicas, narrativa, bestiario, arquitectura técnica y roadmap) vive en [docs/GDD_UkhuPacha.md](docs/GDD_UkhuPacha.md).

## Estado actual

En desarrollo temprano. Trabajo en curso sobre la generación procedural del laberinto (`laberinto/Laberinto.cs`), organizado en **salas conectadas por puertas**:

- Generación de ruido inicial + suavizado por autómata celular ("regla 4-5").
- Detección de regiones conectadas por flood-fill (BFS, 4-direccional).
- Partición del mapa en salas (bloques rectangulares de tamaño variable) mediante BSP.
- Cierre de cada sala con muro perimetral (muros compartidos de 2 celdas de grosor).
- Detección de todas las puertas posibles entre salas vecinas (horizontales y verticales).
- Próximo: tallar puertas con un árbol de expansión desde la sala del Inca, para garantizar que ninguna sala quede sin acceso.

El diseño de la mutación del laberinto (puertas que cambian con el tiempo, salas y portal que se reordenan al colocar cada disco de Inti) está en la sección 3.2 del GDD.

## Requisitos

- [Godot 4.7.2 (.NET)](https://godotengine.org/download)
- .NET SDK 10

## Cómo correrlo

1. Abre el proyecto con Godot 4.7 (.NET).
2. Deja que Godot resuelva la solución de C# (`ukhu-pacha.sln`).
3. Ejecuta la escena principal desde el editor.

## Estructura del proyecto

- `laberinto/` — generación y renderizado del laberinto.
- `player/` — personaje jugable (Inca).
- `antorcha/` — sistema de luz/antorcha.
- `docs/` — documento de diseño (GDD).
