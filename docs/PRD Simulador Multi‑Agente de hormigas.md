# 🧾 PRD: Simulador Multi‑Agente de Colonias de Hormigas

## 1. 📌 Resumen del producto

Simulador basado en agentes que modela colonias de hormigas en un entorno 2D discreto, enfocado en la observación de comportamiento emergente mediante interacción local, feromonas y recursos dinámicos.

El sistema está diseñado como:

* Herramienta exploratoria
* Plataforma extensible (tipo framework)
* Base futura para experimentación evolutiva

***

## 2. 🎯 Objetivos

### Objetivo principal

Permitir la exploración de dinámicas emergentes en colonias de hormigas mediante simulación interactiva.

***

### Objetivos secundarios

* Servir como proyecto de portafolio técnico sólido
* Proveer una base extensible para experimentos futuros
* Permitir configuración flexible sin modificar código
* Soportar múltiples colonias con interacción

***

## 3. 🚫 No objetivos (importante)

* No busca precisión biológica exacta
* No es un videojuego
* No incluye modelos de aprendizaje automático inicialmente
* No incluye evolución genética en la primera versión
* No requiere UI avanzada tipo producto comercial

***

## 4. 🧠 Conceptos clave

### Agente

Hormiga individual con:

* Estado interno simple
* Sensores locales
* Comportamiento definido por reglas + parámetros

***

### Colonia

Conjunto de hormigas definido por:

* Reina
* Roles
* Traits (parámetros configurables)

***

### Entorno

Grid 2D discreto con:

* Materiales
* Recursos
* Feromonas

***

### Feromonas

Campos distribuidos sobre el grid que:

* Guían comportamiento
* Se difunden
* Se evaporan

***

## 5. ⚙️ Funcionalidades principales

***

### 5.1 Simulación base

* Sistema de ticks (pasos discretos)
* Actualización de agentes
* Actualización de feromonas
* Interacción con entorno

***

### 5.2 Agentes (hormigas)

Cada hormiga tendrá:

* Posición (x, y)
* Dirección
* Estado (ej: explorando, regresando)
* Rol (worker, soldier, explorer, etc.)
* Referencia a su colonia

***

### 5.3 Sistema de roles (extensible)

Roles iniciales:

* Reina
* Obreras
* Soldados
* Exploradoras

Requisitos:

* Definibles por configuración
* Extensibles sin cambiar el core

***

### 5.4 Sistema de comportamiento

Modelo híbrido:

* State machine (estructura)
* Parámetros configurables (flexibilidad)

Ejemplo de estados:

* Buscar comida
* Seguir feromonas
* Regresar al nido

***

### 5.5 Sistema de traits (CRÍTICO)

Cada colonia tendrá un conjunto de parámetros:

```json
{
  "speed": 1.0,
  "pheromone_sensitivity": 0.8,
  "explore_bias": 0.3,
  "pheromone_deposit_rate": 0.5,
  "aggression": 0.2
}
```

Requisitos:

* Usados por todas las decisiones de agentes
* Definidos completamente por configuración
* Sin valores hardcodeados

***

### 5.6 Sistema de entorno

* Grid 2D configurable (ej: 500x500)
* Tipos de celda:
  * Vacío
  * Excavable
  * No excavable
  * Recurso

***

### 5.7 Sistema de excavación

* Algunas celdas pueden ser modificadas por hormigas
* Cambios afectan navegación

***

### 5.8 Sistema de feromonas (avanzado)

* Múltiples capas:
  * comida
  * regreso
  * alerta

* Por colonia (aislado o combinado)

* Dinámicas:
  * Deposición
  * Difusión
  * Evaporación

***

### 5.9 Multicolonias

* Soporte para múltiples colonias simultáneas
* Interacciones:
  * Competencia por recursos
  * Interferencia indirecta (feromonas)

***

## 6. 🎮 Interacción en tiempo real (reactividad)

El usuario podrá:

* Pausar / reanudar
* Cambiar velocidad
* Avanzar paso a paso

***

### Intervenciones dinámicas:

* Agregar recursos
* Modificar materiales
* Añadir colonias
* Insertar nuevas reinas
* Cambiar parámetros globales

👉 Todo esto sin detener la simulación

***

## 7. 📊 Métricas y datos

### Métrica principal:

* Eficiencia:

```text
recursos recolectados / tiempo
```

***

### Exportación:

* CSV

Ejemplo:

```text
tick, colony_id, resources_collected, efficiency
```

***

## 8. 💾 Persistencia

* Guardar estado completo de simulación
* Cargar simulaciones previas
* Reanudar ejecución

***

## 9. 🖥️ Visualización (nivel 2.5)

### Incluye:

* Representación del grid
* Hormigas por color (colonia)
* Heatmap de feromonas
* Zoom / pan
* UI básica

***

### No incluye:

* Animaciones complejas
* Efectos gráficos avanzados

***

## 10. 🏗️ Arquitectura

### Enfoque:

Modular → evolucionando a framework

***

### Componentes:

```text
/core
    simulation_engine
/agents
    ant
    roles
/environment
    grid
    materials
/pheromones
    layers
/colonies
    traits
/config
    loader
/render
    visualization
/data
    export
```

***

### Principios clave:

* Separación de lógica y datos
* Configuración externa (JSON)
* Comportamiento genérico
* Extensibilidad

***

## 11. ⚡ Requisitos no funcionales

### Rendimiento:

* Soportar 1,000 – 10,000 agentes

***

### Escalabilidad:

* Preparado para simulaciones más grandes

***

### Portabilidad:

* Compatible con Linux

***

### Open Source:

* Código claro
* Documentación básica
* Fácil contribución

***

## 12. 🧬 Extensibilidad futura (evolución)

El sistema deberá permitir en el futuro:

* Mutación de traits
* Cruce entre colonias
* Selección basada en métricas
* Ejecución de generaciones

***

### Requisito clave:

👉 Traits completamente desacoplados del comportamiento

***

## 13. 🚀 Roadmap sugerido

### Fase 1 (MVP)

* Grid
* Agentes básicos
* Feromonas simples
* Una colonia
* Visualización básica

***

### Fase 2

* Multi-colonias
* Feromonas avanzadas
* Roles
* Configuración externa

***

### Fase 3

* Interactividad en tiempo real
* Exportación de datos
* Persistencia

***

### Fase 4

* Framework extensible
* Mejora de rendimiento
* Preparación para evolución

***

# ✅ Resultado final esperado

Un sistema que permita:

* Explorar comportamiento emergente
* Manipular el entorno dinámamente
* Analizar eficiencia de colonias
* Preparar el terreno para evolución artificial


