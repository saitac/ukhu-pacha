# Ukhu Pacha
### Documento de Diseño de Juego (GDD) — Versión 1.1

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
- **Final:** Al colocar los tres discos sagrados en la puerta de piedra, esta se abre revelando la ciudad santuario al amanecer. El protagonista dejar caer su antorcha ya consumida — el viaje ha terminado.

---

## 3. Mecánicas Principales

### 3.1 Movimiento
- Basado en cuadrícula (**grid-based**), personaje visible de cuerpo completo.
- Vista cenital para dar lectura espacial clara del laberinto y sus cambios.

### 3.2 El Laberinto Viviente
- Representado internamente como una **matriz bidimensional** en C# (0 = camino, 1 = muro, 2 = abismo, etc.).
- Godot renderiza la matriz usando un nodo **TileMap**.
- **Mutación por ciclos:** cada cierto tiempo (o cierto número de pasos del jugador), el laberinto muta:
  1. **Advertencia:** temblor de pantalla + sonido grave (aviso al jugador).
  2. **Cálculo:** el algoritmo selecciona secciones de la matriz para transformar.
  3. **Regla de oro:** nunca sellar el único camino disponible hacia el jugador o los objetivos activos, salvo que exista ruta alternativa.
- **Semillas aleatorias (seeds):** cada partida genera un mapa distinto; las semillas se pueden compartir entre jugadores.
- **La puerta final es fija** (no se mueve), pero las rutas hacia ella cambian constantemente.

### 3.3 La Antorcha (Q'uncha)
- Temporizador de luz (`float`) que baja con el tiempo.
- El cono de luz se reduce y parpadea a medida que se agota.
- En 0%: oscuridad total → alto riesgo de muerte.
- **Recarga:** en tótems fijos de **Viracocha**, repartidos en el mapa.
- Dilema estratégico: arriesgarse a explorar más lejos vs. volver a recargar (sabiendo que el camino de vuelta puede haber cambiado).

### 3.4 Objetivo: Los 3 Discos de Inti
- Reliquias del Dios Sol, dispersas por el laberinto cambiante.
- Se transportan **de a uno** (no se pueden cargar los tres a la vez).
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
- **Cóndor:** invocable en altares específicos → vista cenital temporal para "espiar" el laberinto.
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
- ¿Cuántos altares de Cóndor/Puma habrá por partida y con qué frecuencia se regeneran?
- ¿La dificultad/velocidad de mutación escala solo por discos recogidos, o también por tiempo total de partida?
- ¿Habrá progresión entre partidas (desbloqueables) o cada run es 100% independiente (roguelike puro)?
- Definir tamaño exacto del grid y ritmo de cámara/zoom.

---

*Documento vivo — actualizar a medida que las decisiones de diseño se vayan cerrando durante el desarrollo.*
