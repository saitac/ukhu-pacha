# Ukhu Pacha
### Documento de Diseño de Juego (GDD) — Versión 1.2

---

## 1. Concepto General

**Género:** Survival / Terror atmosférico / Laberinto dinámico (grid-based)
**Perspectiva:** 2D cenital
**Nombre del proyecto en Godot:** ukhu-pacha
**Motor:** Godot 4.7.2 (.NET) + C#
**Entorno de desarrollo:** Visual Studio Code + .NET 10 SDK
**Estilo visual:** Pixel Art
**Plataforma inicial:** PC (Windows/Linux), con posible expansión futura a móvil (Android/iOS) y web

### Premisa

Un príncipe inca huye de la conquista española y se refugia en una cueva ancestral, un lugar sagrado de la propia tradición andina anterior incluso al Imperio Inca, ligado al culto a Viracocha y a las fuerzas del Uku Pacha (el mundo de abajo). La cueva no es un espacio estático: está viva, muta constantemente, y en sus profundidades habitan entidades mitológicas andinas. La única salida es encontrar la **Ciudad Perdida**, un santuario oculto donde su pueblo puede sobrevivir.

### Pilares de diseño

1. **Impotencia táctica** — el jugador nunca es más fuerte que las amenazas; sobrevive con ingenio, no con fuerza.
2. **El espacio como enemigo** — el verdadero antagonista no son los monstruos, es el laberinto mismo.
3. **Rejugabilidad orgánica** — cada partida es distinta gracias a la mutación procedural y las semillas aleatorias.

---

## 2. Narrativa

- **Inicio:** El protagonista escapa de una masacre española, se adentra en una cueva y bloquea la entrada con una piedra sagrada (*wanka*). No hay vuelta atrás.
- **Contexto mítico:** La cueva es un lugar sagrado ancestral de la propia cosmovisión andina, anterior al esplendor del Imperio Inca pero perteneciente a la misma tradición cultural y religiosa (Viracocha, Inti, el Uku Pacha). No se mezclan culturas externas: todo el simbolismo, arquitectura y entidades del juego pertenecen al mismo universo andino/inca.
- **Objetivo narrativo:** Encontrar la Ciudad Perdida para salvar a su pueblo.
- **La cueva protege la Ciudad Perdida:** cada vez que el Inca coloca un disco de Inti en la puerta de piedra, la cueva se reordena y esconde la puerta en otro lugar. No es un enemigo que lo odie, sino un guardián que pone a prueba a quien busca el santuario.
- **Final:** Al colocar los tres discos sagrados en la puerta de piedra, esta se abre revelando la ciudad santuario al amanecer. El protagonista dejar caer su antorcha ya consumida — el viaje ha terminado.

---

## 3. Mecánicas Principales

### 3.1 Movimiento
- Basado en cuadrícula (**grid-based**), personaje visible de cuerpo completo.
- Vista cenital para dar lectura espacial clara del laberinto y sus cambios.

### 3.2 El Laberinto Viviente
- Representado internamente como una **matriz bidimensional** en C# (0 = camino, 1 = muro, 2 = abismo, etc.).
- Godot renderiza la matriz usando un nodo **TileMap**.
- **Estructura:** el mapa se divide en **salas** (bloques rectangulares de tamaño variable) conectadas por **puertas**. Toda sala debe ser accesible: no pueden existir salas sin acceso.
- **Mutación en dos niveles**, cada uno con su propio disparador:

  | Disparador | Qué cambia | Sensación buscada |
  |---|---|---|
  | **Tiempo** (ciclos periódicos) | La disposición de las **puertas** (se cierran unas, se abren otras); la forma de las salas se mantiene | Inquietud constante: "¿esta puerta estaba aquí?" |
  | **Colocar un disco de Inti en la puerta final** | La **forma de las salas** (y con ellas las puertas) + **reubicación de la puerta final** + recolocación de tótems de Viracocha y altares de Cóndor | Golpe dramático, un cambio de acto: la cueva reacciona a la reliquia devuelta y se reordena por completo |

  - Con tres discos, la cueva tiene **cuatro "versiones"** de salas por partida: el juego se estructura en actos.
  - Entre un disco y el siguiente las salas y los tótems no se mueven, así que "volver a recargar" sigue siendo un plan válido aunque las puertas hayan cambiado (ver 3.3).
- **Secuencia de cada mutación:**
  1. **Advertencia:** temblor de pantalla + sonido grave (aviso al jugador).
  2. **Cálculo:** el algoritmo transforma puertas (ciclo de tiempo) o salas (disco de Inti).
  3. **Regla de oro:** nunca sellar el único camino disponible hacia el jugador o los objetivos activos, salvo que exista ruta alternativa.
- **Ancla en un cambio de salas:** la sala donde está el Inca en ese momento **no se modifica** (evita que el jugador quede dentro de un muro). La puerta final sí se va de esa sala.
- **Semillas aleatorias (seeds):** cada partida genera un mapa distinto; las semillas se pueden compartir entre jugadores.
- **La puerta final se mueve solo en los cambios de salas** (la cueva protege la Ciudad Perdida y la esconde, ver 2. Narrativa). Entre un disco y el siguiente queda quieta; lo que cambia son las rutas hacia ella (ciclos de puertas).

### 3.3 La Antorcha (Q'uncha)
- Temporizador de luz (`float`) que baja con el tiempo.
- El cono de luz se reduce y parpadea a medida que se agota.
- En 0%: oscuridad total → alto riesgo de muerte.
- **Recarga:** en tótems de **Viracocha**, repartidos en el mapa.
  - Los tótems se mantienen fijos mientras las salas no cambian; **se recolocan cada vez que cambia la forma de las salas** (al colocar un disco de Inti en la puerta final, ver 3.2).
  - Se colocan según parámetros de distancia al Inca, medida **caminando** (por el recorrido real a través de las puertas, no en línea recta).
  - La distancia es un **rango** (ni demasiado cerca ni demasiado lejos), no una garantía de llegar a tiempo: si siempre hubiera un tótem justo al alcance, la oscuridad dejaría de ser una amenaza.
- Dilema estratégico: arriesgarse a explorar más lejos vs. volver a recargar (sabiendo que el camino de vuelta puede haber cambiado).

### 3.4 Objetivo: Los 3 Discos de Inti
- Reliquias del Dios Sol, dispersas por el laberinto cambiante.
- Se transportan **de a uno** (no se pueden cargar los tres a la vez): cada disco se encuentra, se lleva y se coloca en la puerta final antes de buscar el siguiente.
- **Ciclo de cada acto:**
  1. El Inca coloca un disco en la puerta final → temblor: cambia la forma de las salas y **la puerta final se reubica**.
  2. Explora la cueva nueva buscando el siguiente disco; mientras explora puede descubrir dónde quedó la puerta final.
  3. Encuentra el disco y lo lleva a la puerta final. Las salas no cambiaron (solo las puertas, por tiempo), así que lo explorado le sirve para regresar.
- **Explorar tiene recompensa:** quien prestó atención a la ubicación de la puerta final mientras buscaba el disco regresa con más facilidad; quien no, debe buscarla cargando el disco.
- Al colocar el **tercer** disco, la puerta se abre (ver Final en 2. Narrativa); no hay cambio de salas.
- Cargar un disco probablemente limita otras acciones (uso de armas, sigilo, etc. — a definir).
- Cada disco colocado en la puerta **acelera la mutación** de la cueva (más tensión progresiva).

### 3.5 Combate — "Impotencia táctica"
El jugador **no puede matar** a las criaturas mitológicas; solo aturdir, cegar o escapar.

| Herramienta | Tipo | Función |
|---|---|---|
| Macana | Cuerpo a cuerpo | Aturde brevemente, lenta, gasta stamina |
| Honda (Waraka) | A distancia | Aturde/distrae, munición limitada |
| Bolas de humo de coca/ají | Área | Ciega y aturde temporalmente |
| Amuleto de Inti | Especial | Destello cegador, daña a criaturas de oscuridad, cooldown largo, consume energía de antorcha |
| Champi (escudo) | Defensa | Bloquea ataques frontales, inmoviliza al usarlo |

### 3.6 Aliados sagrados (ayudas limitadas)
Cada recurso se recarga con un mecanismo distinto, para que el mapa no se llene de "estaciones" y mantenga su misterio:

| Recurso | Cómo se recarga |
|---|---|
| Luz (antorcha) | Tótems de Viracocha (en un lugar) |
| Cóndor | Altares (en un lugar) |
| Puma | Tiempo (cooldown), sin tótem ni altar |

- **Cóndor:** invocable en altares específicos → vista cenital temporal para "espiar" el laberinto. Los altares **se recolocan junto con los tótems** cada vez que cambian las salas. Es especialmente valioso justo después de un cambio de salas, para reorientarse en la cueva nueva.
- **Puma:** empuje/aturdimiento de emergencia con cooldown largo, para escapar de un acorralamiento.

---

## 4. Condiciones de derrota

1. **Daño directo:** ataques enemigos o trampas físicas.
2. **Aplastamiento ("El Abrazo de Urka"):** quedar atrapado cuando el laberinto cierra un pasillo o puerta.
3. **Oscuridad total:** la antorcha se apaga y no se logra recargar a tiempo.
4. **Parálisis:** ser inmovilizado por un enemigo (ej. Muqui) mientras otro (ej. Supay) se acerca para el golpe final.

---

## 5. Bestiario mitológico

| Criatura | Rol de diseño | Comportamiento |
|---|---|---|
| **Muqui** | Sabotaje / trampas | Habita paredes, provoca colapsos o mutaciones sorpresa |
| **Supay** | Perseguidor implacable | Presencia lenta e inevitable, tipo "stalker"; no se puede matar, solo evadir |
| **Amaru** | Bloqueo de territorio | Amenaza ambiental / posible jefe de sección, bloquea pasos clave |
| **Dios Urka** | Acelerador de caos | Rige los temblores y mutaciones violentas del laberinto |

**Deidades aliadas:**
- **Viracocha** — deidad de los tótems de recarga de antorcha.
- **Inti** — dios sol, dueño de las reliquias objetivo.
- **Cóndor y Puma** — espíritus tutelares de ayuda táctica limitada.

---

## 6. Dirección de arte

- **Estilo:** Pixel Art oscuro, paleta reducida (tonos terrosos + dorado/rojo como acentos).
- **Personaje principal:** cuerpo completo visible, ~32x32 px por fotograma como punto de partida.
- **Animaciones mínimas para el prototipo:** Idle, Caminar, Cargar disco de Inti, Usar macana.
- **Iluminación dinámica:** uso de `PointLight2D` para la antorcha; posibilidad de Normal Mapping más adelante para dar volumen a las texturas.
- **Referencias tonales:** Blasphemous, Lone Survivor (pero con identidad andina propia).

---

## 7. Arquitectura técnica (Godot + C#)

### Nodos clave sugeridos
- `TileMap` → renderizado del laberinto.
- `CharacterBody2D` → jugador (con `Sprite2D`/`AnimatedSprite2D` y `CollisionShape2D`).
- `PointLight2D` → antorcha.
- Clase base `Enemigo` → herencia para `Muqui`, `Supay`, `Amaru`, etc. (`class Supay : Enemigo`).
- `Timer` (nodo Godot) → ciclos de mutación del laberinto y temporizador de antorcha.

### Snippet de referencia (antorcha)
```csharp
public partial class Inca : CharacterBody2D
{
    [Export] public float TiempoMaximoAntorcha = 60.0f;
    private float _tiempoRestanteAntorcha;
    private PointLight2D _luzAntorcha;

    public override void _Ready()
    {
        _tiempoRestanteAntorcha = TiempoMaximoAntorcha;
        _luzAntorcha = GetNode<PointLight2D>("AntorchaLight");
    }

    public override void _Process(double delta)
    {
        _tiempoRestanteAntorcha -= (float)delta;
        float intensidad = _tiempoRestanteAntorcha / TiempoMaximoAntorcha;
        _luzAntorcha.Energy = Mathf.Lerp(0.2f, 1.5f, intensidad);
        _luzAntorcha.TextureScale = Mathf.Lerp(0.3f, 1.0f, intensidad);

        if (_tiempoRestanteAntorcha <= 0)
            MorirEnLaOscuridad();
    }

    public void RecargarAntorcha() => _tiempoRestanteAntorcha = TiempoMaximoAntorcha;
}
```

---

## 8. Roadmap sugerido de desarrollo

- [ ] **Fase 0:** Completar tutoriales oficiales "Getting Started" de Godot (2D y 3D) — *en curso*
- [ ] **Fase 1:** Movimiento del personaje por cuadrícula + colisiones básicas
- [ ] **Fase 2:** Estructura de datos del laberinto (matriz) + renderizado con TileMap
- [ ] **Fase 3:** Sistema de mutación del laberinto (ciclos + reglas de seguridad)
- [ ] **Fase 4:** Sistema de antorcha (luz dinámica + recarga en tótems)
- [ ] **Fase 5:** Primer enemigo funcional (IA básica de persecución/patrulla)
- [ ] **Fase 6:** Sistema de recolección de los 3 discos de Inti + puerta final
- [ ] **Fase 7:** Combate de aturdimiento (macana, honda, humo)
- [ ] **Fase 8:** Arte pixel definitivo + animaciones
- [ ] **Fase 9:** Pulido, sonido, música, menú y pantalla de derrota/victoria
- [ ] **Fase 10:** Exportación y evaluación de plataformas (Steam / Itch.io / móvil)

---

## 9. Notas de negocio (referencia rápida)

| Plataforma | Comisión aprox. | Costo de entrada |
|---|---|---|
| Steam | 30% | $100 USD (reembolsable a los $1000 en ventas) |
| Itch.io | 0–30% (configurable) | Gratis |
| Epic Games Store | 12% | Gratis |
| Google Play | 30% (15% con reducción) | $25 USD única vez |
| Apple App Store | 30% (15% con reducción) | $99 USD/año |

> Nota: cifras de referencia recopiladas en 2026; verificar condiciones vigentes antes de publicar.

---

## 10. Preguntas abiertas / por definir

- ¿Qué ocurre exactamente al cargar un disco de Inti? (¿limita combate, movimiento, ambas?)
- ¿Cuántos altares de Cóndor habrá por partida? (Puma ya no usa altares: se recarga por cooldown.)
- ¿La dificultad/velocidad de mutación escala solo por discos recogidos, o también por tiempo total de partida? (¿Cada disco acelera el ciclo de puertas?)
- ¿Las mutaciones ocurren solo fuera de la luz de la antorcha? (Idea: el jugador nunca ve el cambio, solo lo descubre después.)
- ¿Valores concretos del rango de distancia para colocar tótems de Viracocha?
- ¿Reglas para reubicar la puerta final y los discos? (¿distancia mínima caminando desde el Inca? ¿el siguiente disco lejos de la nueva puerta?) — se definirán al implementar la Fase 3/6.
- ¿Habrá progresión entre partidas (desbloqueables) o cada run es 100% independiente (roguelike puro)?
- Definir tamaño exacto del grid y ritmo de cámara/zoom.

---

*Documento vivo — actualizar a medida que las decisiones de diseño se vayan cerrando durante el desarrollo.*
