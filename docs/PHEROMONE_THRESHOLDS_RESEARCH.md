# Investigación: Umbrales de feromonas en hormigas reales

## Contexto biológico

### Tipos de feromonas y su naturaleza:
- **Trail pheromone (comida/retorno):** Volátil, alcance medio, degradación rápida a moderada
- **Alarm pheromone:** Muy volátil, muy fuerte, alcance corto
- **Queen pheromone:** Muy estable, concentración constante

### Comportamiento de umbrales en hormigas reales:

**1. Respuesta diferencial según contexto:**
- Hormiga **exploradora**: Cauta, busca con cuidado
- Hormiga **con comida retornando**: Urgida, necesita volver rápido
- Hormiga **reclutada**: Sigue pistas con menos dudas

**2. Concentración depositada es diferente:**
- Al encontrar comida y retornar: **Máxima urgencia → máxima concentración**
- Al explorar sin nada: **Baja urgencia → baja concentración**

**3. Umbrales varían por necesidad:**
- Retorno: "Si hay cualquier pista, la sigo" (umbral BAJO)
- Exploración: "Ignoro ruido débil, solo sigo rastros fuertes" (umbral ALTO)

## Análisis de umbrales actuales

| Situación | Depósito | Umbral | Ratio | Interpretación |
|-----------|----------|--------|-------|-----------------|
| **Exploring (FOOD)** | `0.1f` | `0.05f` | 0.5× | Umbral = 50% del depósito → CAUTO |
| **Returning (RETURN)** | `1.0f` | `0.01f` | 0.01× | Umbral = 1% del depósito → SENSIBLE |

## Por qué estos valores son realistas

✅ **Retorno es 10× más urgente:** Deposita 10× más (`1.0f` vs `0.1f`)
✅ **Retorno tolera señales débiles:** Umbral 50× más bajo (`0.01f` vs `0.05f`)
✅ **Exploración es selectiva:** Ignora rastros débiles para no perder tiempo
✅ **Evaporación lenta favorece esto:** Con `0.001f` diffusion, un rastro de retorno antiguo sigue siendo detectado

## Efecto emergente

- Rastros débiles (exploración) = corta duración efectiva
- Rastros fuertes (retorno) = larga duración efectiva
- Sin cambiar tasas de evaporación → autorregulantebiológicamente plausible

## Recomendación

**Mantener estos umbrales como están.** Funcionan correctamente desde perspectiva de comportamiento emergente.
