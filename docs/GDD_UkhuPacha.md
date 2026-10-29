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

Un príncipe inca huye de la conquista española y se refugia en una cueva ancestral, un lugar sagrado de la propia tradición andina anterior incluso al Imperio Inca, ligado al culto a Wiracocha y a las fuerzas del Uku Pacha (el mundo de abajo). La cueva no es un espacio estático: está viva, muta constantemente, y en sus profundidades habitan entidades mitológicas andinas. La única salida es encontrar la **Ciudad Perdida**, un santuario oculto donde su pueblo puede sobrevivir.

### Pilares de diseño

1. **Impotencia táctica** — el jugador nunca es más fuerte que las amenazas; sobrevive con ingenio, no con fuerza.
2. **El espacio como enemigo** — el verdadero antagonista no son los monstruos, es el laberinto mismo.
3. **Rejugabilidad orgánica** — cada partida es distinta gracias a la mutación procedural y las semillas aleatorias.

---

## 2. Narrativa

- **Inicio:** El protagonista escapa de una masacre española, se adentra en una cueva y bloquea la entrada con una piedra sagrada (*wanka*). No hay vuelta atrás.
- **Contexto mítico:** La cueva es un lugar sagrado ancestral de la propia cosmovisión andina, anterior al esplendor del Imperio Inca pero perteneciente a la misma tradición cultural y religiosa (Wiracocha, Inti, el Uku Pacha). No se mezclan culturas externas: todo el simbolismo, arquitectura y entidades del juego pertenecen al mismo universo andino/inca.
- **Objetivo narrativo:** Encontrar la Ciudad Perdida para salvar a su pueblo.
- **La cueva protege la Ciudad Perdida:** cada vez que el Inca coloca un disco de Inti en la puerta de piedra, la cueva se reordena y esconde la puerta en otro lugar. No es un enemigo que lo odie, sino un guardián que pone a prueba a quien busca el santuario.
- **Final:** Al colocar los tres discos sagrados en la puerta de piedra, esta se abre, pero el **Amaru**, la serpiente de dos cabezas, le cierra el paso: es el último guardián entre el Ukhu Pacha y la Ciudad Perdida. El Inca lo enfrenta con la macana hasta aturdirlo (ver 3.5), cruza y la ciudad santuario se revela al amanecer. El protagonista deja caer su antorcha ya consumida — el viaje ha terminado.

---

## 3. Mecánicas Principales

### 3.1 Movimiento
- Movimiento **libre** (no por casillas) sobre un laberinto basado en cuadrícula (**grid-based**), personaje visible de cuerpo completo.
- Vista cenital para dar lectura espacial clara del laberinto y sus cambios.

### 3.2 El Laberinto Viviente
- Representado internamente como una **matriz bidimensional** en C# (0 = camino, 1 = muro, 2 = abismo, etc.).
- Godot renderiza la matriz usando un nodo **TileMap**.
- **Estructura:** el mapa se divide en **salas** (bloques rectangulares de tamaño variable) conectadas por **puertas**. Toda sala debe ser accesible: no pueden existir salas sin acceso.
- **Mutación en dos niveles**, cada uno con su propio disparador:

  | Disparador | Qué cambia | Sensación buscada |
  |---|---|---|
  | **Tiempo** (ciclos periódicos) | La disposición de las **puertas** (se cierran unas, se abren otras); la forma de las salas se mantiene | Inquietud constante: "¿esta puerta estaba aquí?" |
  | **Colocar un disco de Inti en la puerta final** | La **forma de las salas** (y con ellas las puertas) + **reubicación de la puerta final** + recolocación de tótems de Wiracocha y altares de Cóndor + reposición de las hojas de coca (ver 3.7) | Golpe dramático, un cambio de acto: la cueva reacciona a la reliquia devuelta y se reordena por completo |

  - Con tres discos, la cueva tiene **cuatro "versiones"** de salas por partida: el juego se estructura en actos.
  - Entre un disco y el siguiente las salas y los tótems no se mueven, así que "volver a recargar" sigue siendo un plan válido aunque las puertas hayan cambiado (ver 3.3).
- **Secuencia de cada mutación:**
  1. **Advertencia:** temblor de pantalla + sonido grave (aviso al jugador).
  2. **Cálculo:** el algoritmo transforma puertas (ciclo de tiempo) o salas (disco de Inti).
  3. **Regla de oro:** nunca sellar el único camino disponible hacia el jugador o los objetivos activos, salvo que exista ruta alternativa.
- **Ciclo de puertas (decisiones Fase 3):**
  - **Frecuencia:** intervalo con variación aleatoria (orientativo: 45–75 s), configurable en `LaberintoConfig`. Nunca fijo, para que el jugador no pueda "contar" el ciclo.
  - **Cambio parcial:** en cada ciclo se cierran algunas puertas abiertas y se abren otras; el mapa sigue siendo reconocible, pero "algo cambió".
  - **Intercambios por ciclo:** K = máx(1, redondear(puertas abiertas × `PorcentajeCambio`)), orientativo 10–15 % (≈3 intercambios en un mapa de 40×40). Cada intercambio = abrir una puerta cerrada + cerrar otra abierta, así que la cantidad de puertas abiertas se mantiene estable. Valor a ajustar con playtesting; en Fase 6 podría subir con cada disco.
  - **Algoritmo (abrir primero, cerrar después):** abrir una puerta cerrada siempre forma un ciclo, lo que garantiza que exista al menos otra puerta cerrable sin aislar salas. Se cierra una puerta al azar de entre las candidatas que mantienen todo conectado (verificación sobre el grafo de salas, no sobre las celdas); si no hay candidatas, ese intercambio no ocurre.
  - **Regla de oro (versión estricta):** después de cada ciclo **todas las salas siguen conectadas**, no solo la del jugador y sus objetivos.
  - **Puerta ocupada:** nunca se cierra una puerta sobre la que está parado el Inca.
  - **Feedback:** temblor de cámara + polvo/tierra que se levanta en las puertas que cambian. Sonido grave en Fase 9.
  - **Aviso gradual (sin reloj en pantalla):** unos segundos antes de cada ciclo cae polvo fino del techo y empieza un temblor leve que crece hasta la mutación. El jugador sabe que *falta poco*, nunca cuánto exactamente. Se descartó un contador visible porque convierte el terror en gestión de tiempo y rompe la inmersión.
  - *Ideas para Fase 9:* indicador diegético andino (p. ej. chakana que se ilumina por fases) y aviso más temprano/evidente como opción de dificultad o accesibilidad.
  - **Mutación también dentro de la luz:** el ciclo de puertas no depende de la antorcha; una puerta puede cambiar a la vista del jugador. Verla cerrarse (con temblor y polvo) refuerza que la cueva está viva y reacciona frente a él. Se descartó mutar solo fuera de la luz (decisión Fase 4).
  - *Pendiente:* aceleración del ciclo por disco (Fase 6).
- **Ancla en un cambio de salas:** la sala donde está el Inca en ese momento **no se modifica** (evita que el jugador quede dentro de un muro). La puerta final sí se va de esa sala.
- **Semillas aleatorias (seeds):** cada partida genera un mapa distinto; las semillas se pueden compartir entre jugadores.
- **La puerta final se mueve solo en los cambios de salas** (la cueva protege la Ciudad Perdida y la esconde, ver 2. Narrativa). Entre un disco y el siguiente queda quieta; lo que cambia son las rutas hacia ella (ciclos de puertas).

### 3.3 La Antorcha (Q'uncha)
- Temporizador de luz (`float`) que baja con el tiempo.
- El cono de luz se reduce y parpadea a medida que se agota.
- En 0%: oscuridad total → alto riesgo de muerte.
  - Al apagarse, **la oscuridad consume el vigor del Inca** (ver 3.7) a ritmo constante hasta que recargue en un tótem. Si el vigor llega a 0, es **game over** (ver 4. Condiciones de derrota).
  - **Tiempo de gracia:** lo que tarda la oscuridad en matar a un Inca con el **vigor lleno**. Es **proporcional** a la duración de la antorcha: por defecto la mitad (60 s de antorcha → 30 s de gracia), para que escale si cambia la duración (p. ej. por dificultad). Ritmo de drenaje = vigor máximo / tiempo de gracia.
  - Con menos vigor se aguanta menos a oscuras (p. ej. a medio vigor, 15 s): las amenazas se suman.
  - **Recargar la antorcha detiene el drenaje, pero no devuelve el vigor perdido**: cada apagón deja una "cicatriz". El vigor solo se recupera con hojas de coca (ver 3.7).
  - *A vigilar en pruebas:* con estos valores pasan hasta 90 s entre recargas (60 con luz + 30 a oscuras). Si la oscuridad no se siente como una amenaza, lo primero que conviene bajar es la proporción de gracia.
- **Recarga:** en tótems de **Wiracocha**, repartidos en el mapa.
  - La recarga es **automática**: basta con que el Inca se acerque al tótem (entre en su zona), sin presionar ninguna tecla. La zona es pequeña, pegada al tótem, para que no recargue con solo pasar cerca.
  - Cada tótem tiene al pie un **brasero ceremonial** de piedra con un fuego que nunca se apaga: ilumina el tótem desde abajo y lo hace visible a la distancia en la oscuridad.
  - Los tótems se mantienen fijos mientras las salas no cambian; **se recolocan cada vez que cambia la forma de las salas** (al colocar un disco de Inti en la puerta final, ver 3.2).
  - Se colocan según parámetros de distancia al Inca, medida **caminando** (por el recorrido real a través de las puertas, no en línea recta).
  - La distancia es un **rango** (ni demasiado cerca ni demasiado lejos), no una garantía de llegar a tiempo: si siempre hubiera un tótem justo al alcance, la oscuridad dejaría de ser una amenaza.
  - **Cantidad:** un porcentaje del número de salas (mínimo 1), para que escale si cambia el tamaño del mapa.
  - **Reparto:** solo el **primer** tótem respeta el rango de distancia al Inca (garantiza uno alcanzable); los demás respetan una **separación mínima** entre tótems (medida en salas) y solo la distancia **mínima** al Inca (sin máximo), para quedar repartidos por todo el mapa sin aparecer pegados al Inca.
  - La distancia se mide al momento de colocarlos; luego las puertas mutan y deja de ser exacta (coherente con que el rango no es una garantía).
  - **Valores actuales:** 20% de las salas; el primer tótem entre 2 y 3 salas del Inca; separación mínima de 3 salas entre tótems.
  - La luz del brasero es **tenue**: marca el tótem como un faro, pero no ilumina toda la sala.
- **Visibilidad:** la oscuridad casi total y las **sombras** de las luces (los muros bloquean la luz) deciden qué se ve del mapa; no hay una capa aparte de niebla de guerra. Solo se ve a lo lejos lo que tiene luz propia (braseros, hojas de coca).
  - *Pendiente (Fase 5):* en vista cenital la cámara ve todo lo iluminado aunque haya un muro de por medio. Los **enemigos** solo serán visibles si el Inca tiene **línea de visión** hacia ellos, para que no se los vea a través de los muros junto a un brasero.
- Dilema estratégico: arriesgarse a explorar más lejos vs. volver a recargar (sabiendo que el camino de vuelta puede haber cambiado).

### 3.4 Objetivo: Los 3 Discos de Inti
- Reliquias del Dios Sol, dispersas por el laberinto cambiante.
- Se transportan **de a uno** (no se pueden cargar los tres a la vez): cada disco se encuentra, se lleva y se coloca en la puerta final antes de buscar el siguiente.
- **Ciclo de cada acto:**
  1. El Inca coloca un disco en la puerta final → temblor: cambia la forma de las salas y **la puerta final se reubica**.
  2. Explora la cueva nueva buscando el siguiente disco; mientras explora puede descubrir dónde quedó la puerta final.
  3. Encuentra el disco y lo lleva a la puerta final. Las salas no cambiaron (solo las puertas, por tiempo), así que lo explorado le sirve para regresar.
- **Supay custodia los discos** (señor del Ukhu Pacha y de las riquezas ocultas bajo la tierra):
  - Aparece **al tomar** un disco (no antes) y persigue al Inca mientras lo lleva a la puerta final. No se le puede vencer ni aturdir de forma definitiva: **solo se le escapa**.
  - Al colocar el disco en la puerta, Supay **se desvanece y se hunde en el Ukhu Pacha** (no muere; ver 3.5).
  - **Un solo Supay activo a la vez:** cada disco trae el suyo y nunca se acumulan. Cada uno puede ser más peligroso que el anterior (más rápido, más agresivo), en línea con la mutación que se acelera.
  - Cada acto alterna calma y tensión: explorar (solo Muquis) → tomar el disco → huir de Supay → colocarlo → temblor.
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

- **El Amaru (enfrentamiento final)** es la única criatura a la que el Inca "vence", y aun así sin matarla: tiene mucha energía y hay que bajársela a golpes de macana hasta **aturdirlo**; entonces el Inca gana y cruza a la Ciudad Perdida. Con Supay solo se puede huir; con el Amaru, por fin, se puede plantar cara, pero apenas alcanza para aturdirlo.

### 3.6 Aliados sagrados (ayudas limitadas)
Cada recurso se recarga con un mecanismo distinto, para que el mapa no se llene de "estaciones" y mantenga su misterio:

| Recurso | Cómo se recarga |
|---|---|
| Luz (antorcha) | Tótems de Wiracocha (en un lugar) |
| Cóndor | Altares (en un lugar) |
| Puma | Tiempo (cooldown), sin tótem ni altar |
| Vigor | Hojas de coca flotantes y limitadas: se recolocan con cada ciclo de puertas y se reponen al colocar un disco de Inti (ver 3.7) |

- **Cóndor:** invocable en altares específicos → vista cenital temporal para "espiar" el laberinto. Los altares **se recolocan junto con los tótems** cada vez que cambian las salas. Es especialmente valioso justo después de un cambio de salas, para reorientarse en la cueva nueva.
- **Puma:** empuje/aturdimiento de emergencia con cooldown largo, para escapar de un acorralamiento.

### 3.7 Vigor del Inca
- El Inca tiene un **vigor** (`float`, vigor máximo configurable). **Toda muerte pasa por el vigor:** cuando llega a 0, el Inca muere (ver 4. Condiciones de derrota).
- **El vigor es compartido con las criaturas**, pero en ellas 0 no significa muerte: quedan **aturdidas** y, pasado un tiempo, despiertan con el vigor completo (ver 3.5, "no puede matar").
- **Qué lo baja:** la oscuridad (drenaje constante mientras la antorcha está apagada, ver 3.3) y, desde la Fase 5, los ataques de enemigos y las trampas.
- **Barra de vigor sobre la cabeza del Inca:** aparece **solo cuando el vigor cambia** (sube o baja) y se oculta **2 segundos** después de que deja de cambiar. A oscuras se ve todo el tiempo, porque el vigor baja sin parar: es el aviso de que el Inca se está muriendo, sin un reloj en pantalla.
  - Va pegada al Inca porque a oscuras la mirada del jugador está en él, no en una esquina de la pantalla.
  - Tiene que verse aunque la escena esté oscura: no la debe oscurecer el `CanvasModulate`.
- **Las criaturas también tienen barra de vigor** sobre la cabeza, con la misma regla: aparece **solo cuando su vigor cambia**. Como a ellas nada las drena, en la práctica solo se ve cuando el Inca las golpea (ver 3.5): es la confirmación de que el golpe sirvió y de cuánto falta para aturdirlas.
  - **A diferencia de la del Inca, solo se ve si la criatura está iluminada** por la antorcha: en la oscuridad la barra no delata a la criatura.
  - **También aparece al despertar del aturdimiento**, cuando el vigor vuelve a llenarse: avisa que la criatura se levantó. Si no hay antorcha cerca, no se ve (misma regla de la luz).
- **Recuperación: hojas de coca.** Planta sagrada y medicinal andina, usada contra el cansancio y el dolor.
  - Flotan en el aire con un **brillo verde tenue**, visible en la oscuridad para que el Inca pueda ubicarlas.
  - El Inca las **atraviesa** (no son obstáculo) y al pasar por encima las recoge (las *chaccha*) y recupera vigor.
  - **Curación aleatoria:** cada hoja devuelve un porcentaje del vigor máximo, sorteado al crearla entre un mínimo y un máximo (inicial: **10 %–100 %**, a ajustar en pruebas). Se sortea con la semilla del laberinto: la misma semilla da las mismas hojas.
  - **Con el vigor lleno, el Inca no la recoge:** la hoja queda donde está y no se desperdicia. Tampoco la recoge si ya está muerto.
  - **Al consumirla:** un **destello verde** breve alrededor del Inca confirma la curación (y la barra de vigor aparece y sube).
  - **Cantidad:** un porcentaje del número de salas, una hoja por sala elegida (inicial: **30 %**, a ajustar en pruebas).
  - **Ubicación:** en una **esquina** de la sala, elegida al azar entre las cuatro. El jugador aprende dónde buscar pero no en cuál, y tiene que recorrer la sala con la antorcha. Cuando haya criaturas, las esquinas son callejones sin salida: ir por la coca es riesgo contra recompensa.
    - Puede aparecer en la sala donde está el Inca.
    - Puede compartir sala con un tótem de Wiracocha (el tótem va en el centro, la hoja en una esquina): llegar a las justas a recargar la luz y encontrar también coca es un alivio.
  - **Son limitadas, no se regeneran con el tiempo:** las hojas consumidas desaparecen; las que quedan se **recolocan en cada ciclo de puertas** (ver 3.2), así que no sirven como punto fijo de referencia. Si al inicio hay 10 y el Inca consumió 5, solo se recolocan las 5 restantes.
    - Al recolocarlas se **mueven** (no se destruyen y recrean): cada hoja conserva su curación sorteada.
  - **Se reponen al colocar un disco de Inti** en la puerta final (ver 3.2 y 3.4): con el cambio de salas vuelve a haber la cantidad inicial de hojas (las que quedaban se recolocan en las salas nuevas y se crean las que faltan). Al colocar el tercer disco termina el juego, así que en la práctica se reponen dos veces por partida.
    - Esto convierte la coca en un recurso de cada acto: quien la gasta rápido llega a la puerta final con menos margen. Junto con la "cicatriz" de los apagones (ver 3.3), el vigor se vuelve un recurso que la partida va gastando.
- **Respiración según el vigor (Fase 9, sonido):** la respiración del Inca cambia según cuánto vigor le queda: tranquila con vigor alto, cada vez más agitada y entrecortada a medida que baja. A oscuras se vuelve un segundo aviso, sonoro, de que se está muriendo, complementario a la barra.

---

## 4. Condiciones de derrota

Todas las causas terminan igual: el **vigor del Inca llega a 0** (ver 3.7). Algunas lo bajan de a poco y otras de golpe.

1. **Daño directo:** ataques enemigos o trampas físicas bajan el vigor.
2. **Aplastamiento ("El Abrazo de Urka"):** quedar atrapado cuando el laberinto cierra un pasillo o puerta. Deja el vigor en 0 de inmediato.
3. **Oscuridad total:** la antorcha se apaga y la oscuridad consume el vigor hasta 0 antes de que el Inca logre recargar (ver 3.3).
4. **Parálisis:** ser inmovilizado por un enemigo (ej. Muqui) mientras otro (ej. Supay) se acerca para el golpe final.

---

## 5. Bestiario mitológico

Las criaturas forman una **jerarquía**: de la más común a la más poderosa.

| Rango | Criatura | Rol de diseño | Comportamiento |
|---|---|---|---|
| Bajo | **Muqui** | Enemigo común de la cueva | Duende pequeño, dueño y guardián de las minas. Hay **varios** recorriendo la cueva. Primer enemigo (Fase 5). |
| Medio | **Supay** | Guardián de los discos de Inti | Aparece al tomar un disco y persigue al Inca hasta la puerta final; presencia lenta e inevitable, tipo "stalker". No se puede vencer, solo escapar. Uno solo a la vez (ver 3.4). |
| Final | **Amaru** | Último guardián | Serpiente colosal de dos cabezas que cierra el paso a la Ciudad Perdida tras colocar el tercer disco. Se le vence aturdiéndolo con la macana (ver 3.5). |
| Entorno | **Dios Urka** | Acelerador de caos | No es una criatura que camine: rige los temblores y mutaciones violentas del laberinto. |

**Deidades aliadas:**
- **Wiracocha** — deidad de los tótems de recarga de antorcha.
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

- [x] **Fase 0:** Completar tutoriales oficiales "Getting Started" de Godot (2D)
- [x] **Fase 1:** Movimiento libre del personaje + colisiones básicas
- [x] **Fase 2:** Estructura de datos del laberinto (matriz) + renderizado con TileMap
- [x] **Fase 3:** Sistema de mutación del laberinto (ciclos + reglas de seguridad)
- [x] **Fase 4:** Sistema de antorcha (luz dinámica + recarga en tótems)
- [ ] **Fase 5:** Primer enemigo funcional: el **Muqui** (IA básica de persecución/patrulla; incluye su visibilidad por línea de visión, ver 3.3)
- [ ] **Fase 6:** Sistema de recolección de los 3 discos de Inti + puerta final (incluye a Supay como guardián de cada disco y reponer las hojas de coca al colocar cada disco, ver 3.4 y 3.7)
- [ ] **Fase 7:** Combate de aturdimiento (macana, honda, humo) + enfrentamiento final con el Amaru
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
- ¿Cómo se comporta el Muqui? (¿patrulla y persigue, o sale de las paredes, ataca y desaparece?) ¿Conserva el sabotaje (colapsos, mutaciones sorpresa)? ¿Las ofrendas de coca lo calman o distraen?
- ¿Supay es el mismo que regresa con cada disco o uno distinto cada vez? (La mecánica funciona igual; es una decisión narrativa.)
- ¿Cuántos altares de Cóndor habrá por partida? (Puma ya no usa altares: se recarga por cooldown.)
- ¿La dificultad/velocidad de mutación escala solo por discos recogidos, o también por tiempo total de partida? (¿Cada disco acelera el ciclo de puertas?)
- ¿Reglas para reubicar la puerta final y los discos? (¿distancia mínima caminando desde el Inca? ¿el siguiente disco lejos de la nueva puerta?) — se definirán al implementar la Fase 3/6.
- ¿Habrá progresión entre partidas (desbloqueables) o cada run es 100% independiente (roguelike puro)?
- Definir tamaño exacto del grid y ritmo de cámara/zoom.
- ¿La respiración del Inca (ver 3.7) va acompañada de un latido que se acelera, o basta con la respiración?

---

*Documento vivo — actualizar a medida que las decisiones de diseño se vayan cerrando durante el desarrollo.*
