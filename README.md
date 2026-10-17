# Juego de neurociencia del dolor

Proyecto de videojuego 3D desarrollado en Unity. Esta entrega incluye avances de los capítulos 1 y 2.

## Cómo iniciar

1. Abre el proyecto desde Unity Hub.
2. En Unity, abre la escena `Assets/Scenes/00_MenuJuego.unity`.
3. Ejecuta el juego desde el menú, selecciona el avatar y escribe el nombre del jugador.
4. Continúa desde el menú para jugar. La selección del avatar y el nombre se guardan durante la partida; por eso, inicia desde el menú en lugar de abrir directamente una escena de capítulo.

## Escenas principales

- `00_MenuJuego`: menú de inicio.
- `01_SeleccionAvatar`: selección del avatar.
- `02_Capitulo_1`: capítulo 1.
- `03_MinijuegoRetirada`, `04_MinijuegoEcoCambio` y `05_MinijuegoCompromiso`: minijuegos del capítulo 1.
- `07_Capitulo_2`: capítulo 2.
- `(Test) Mapa de Capitulos`: escena de prueba, no hace parte del recorrido principal.

## Scripts

- `Assets/Scripts/00_MENU INICIO SCRIPTS`: menú y flujo inicial.
- `Assets/Scripts/01_CAPITULO 1 SCRIPTS`: diálogos y mecánicas del capítulo 1.
- `Assets/Scripts/02_CAPITULO_2 SCRIPTS`: estaciones y mecánicas del capítulo 2.

Las escenas incluidas en el recorrido principal están registradas en `File > Build Settings`.
