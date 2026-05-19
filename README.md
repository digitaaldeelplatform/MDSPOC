# Microservice Decision Support (MDS) — Proof of Concept

> Een gestructureerde, uitlegbare methode voor build–buy–free beslissingen in microservice-architecturen.

---

## Inhoudsopgave

- [Overzicht](#overzicht)
- [Probleemstelling](#probleemstelling)
- [Kernmethode](#kernmethode)
- [Features](#features)
- [Architectuur](#architectuur)
- [API Referentie](#api-referentie)
- [Voorbeeld Request & Response](#voorbeeld-request--response)
- [Installatie & Uitvoeren](#installatie--uitvoeren)
- [Technologiestack](#technologiestack)
- [Projectstructuur](#projectstructuur)
- [Projectstatus](#projectstatus)
- [Context](#context)
- [Auteur](#auteur)

---

## Overzicht

Dit project is een Proof-of-Concept voor de **Microservice Decision Support (MDS)** methode. MDS helpt architecten en ontwikkelaars bij het nemen van gestructureerde, reproduceerbare en uitlegbare beslissingen over de inkoop of ontwikkeling van microservice-componenten:

| Optie | Beschrijving |
|---|---|
| **Build** | Zelf ontwikkelen |
| **Buy** | Commercieel product aanschaffen |
| **Free / OSS** | Open-source oplossing adopteren |

De evaluatie is gebaseerd op een gewogen multi-criteria model met expliciete ondersteuning voor ontbrekende data, onzekerheid en her-evaluatie op basis van runtime feedback.

---

## Probleemstelling

In microservice-gebaseerde systemen worden beslissingen zoals build vs. buy vaak:

- Impliciet genomen en niet gedocumenteerd
- Sterk afhankelijk van context die later verandert
- Moeilijk te reproduceren of te reviseren
- Niet traceerbaar naar de onderbouwing

De MDS-methode formaliseert dit proces zodat beslissingen transparant, herhaalbaar en auditeerbaar worden.

---

## Kernmethode

Een beslissing wordt gemodelleerd als een evaluatie van **alternatieven** over een set **criteria**:

```
finalScore = Σ (gewicht_i × score_i × (1 - onzekerheid_i)) × correctiefactor
```

| Component | Beschrijving |
|---|---|
| **Criteria** | Evaluatiedimensies (bijv. Tijd, Kosten, Onderhoudbaarheid) |
| **Gewichten** | Relatief belang per criterium (som = 1.0) |
| **Scores** | Beoordeling per alternatief per criterium (0–1) |
| **Onzekerheid** | Betrouwbaarheidsgraad van een score (0 = zeker, 1 = volledig onzeker) |
| **Correctiefactor** | Niet-lineaire straf voor ontbrekende criteria |

---

## Features

### Multi-criteria evaluatie

Elk alternatief wordt gescoord over alle beschikbare criteria. Scores worden genormaliseerd op basis van aanwezige data. De eindscore combineert gewogen scores met een correctie voor dekking.

### Omgaan met ontbrekende criteria

Ontbrekende data wordt niet geïmputeerd — in plaats daarvan:

- Wordt de **dekking** (coverage) berekend op basis van aanwezige informatie
- Wordt een **niet-lineaire straf** toegepast op basis van het totale gewicht van ontbrekende criteria

| Gewicht ontbrekend criterium | Straf |
|---|---|
| Hoog | Zwaar |
| Laag | Licht |

Dit zorgt voor een eerlijke vergelijking tussen alternatieven met incomplete data.

### Onzekerheidsgewogen scoring

Elke score bevat een onzekerheidsfactor:

```
aangepastScore = score × (1 - onzekerheid)
```

Hoge onzekerheid vermindert de invloed van een criterium op de eindscore.

### Her-evaluatie via runtime feedback

Via de `ReEvaluationService` kunnen eerder genomen beslissingen opnieuw worden geëvalueerd wanneer omstandigheden veranderen — bijvoorbeeld als een score inmiddels bijgesteld moet worden op basis van ervaringen in productie.

### Temperatuurservice (beslisvertrouwen)

De `TemperatureService` berekent een indicatie van het vertrouwen in een beslissing op basis van de spreiding van scores, dekking en onzekerheid. Dit geeft een kwalitatieve duiding naast de kwantitatieve eindscore.

### Catalogusondersteuning

De `CatalogService` biedt de mogelijkheid om alternatieven op te halen uit een vooraf gedefinieerde catalogus, zodat hergebruik van bekende oplossingen eenvoudig is.

### Explainability

Voor elk alternatief geeft het systeem een volledig inzichtelijke breakdown:

| Veld | Beschrijving |
|---|---|
| `finalScore` | Gewogen eindscore inclusief correctie |
| `normalizedScore` | Score genormaliseerd over beschikbare criteria |
| `coverage` | Fractie criteria waarvoor data beschikbaar is |
| `missingCriteria` | Lijst van criteria zonder score |
| `missingWeight` | Totaalgewicht van ontbrekende criteria |
| `correctionFactor` | Toegepaste straf voor incomplete data |

---

## Architectuur

Het project gebruikt een gelaagde Clean Architecture-opzet:

```
MDSPOC (ASP.NET Core Web API)
├── Controllers/          ← HTTP endpoints
├── MdsPoc.Api/           ← Request/Response DTOs
├── MdsPoc.Application/   ← Business logic, services, interfaces
│   ├── Services/
│   │   ├── DecisionEvaluationService
│   │   ├── ReEvaluationService
│   │   ├── TemperatureService
│   │   └── CatalogService
│   └── Interfaces/
├── MdsPoc.Domain/        ← Core domeinmodellen
│   └── (Alternatives, Criteria, Scores, Weights, ...)
├── MdsPoc.Tests/         ← Unit tests
└── wwwroot/              ← Statische frontend assets
```

| Laag | Verantwoordelijkheid |
|---|---|
| **Controllers** | HTTP routing, validatie van requests |
| **Application** | Beslislogica, evaluatie-algoritmen, orkestratie |
| **Domain** | Pure domeinmodellen zonder externe afhankelijkheden |
| **Tests** | Geautomatiseerde validatie van het evaluatiemodel |

---

## API Referentie

### `POST /api/decision/evaluate`

Evalueert een set alternatieven op basis van criteria, gewichten en scores.

**Request body:**

```json
{
  "criteria": [
    { "name": "Cost", "weight": 0.4 },
    { "name": "Maintainability", "weight": 0.35 },
    { "name": "Time", "weight": 0.25 }
  ],
  "alternatives": [
    {
      "name": "Build",
      "scores": [
        { "criterionName": "Cost", "score": 0.3, "uncertainty": 0.1 },
        { "criterionName": "Maintainability", "score": 0.8, "uncertainty": 0.05 }
      ]
    },
    {
      "name": "Buy",
      "scores": [
        { "criterionName": "Cost", "score": 0.6, "uncertainty": 0.15 },
        { "criterionName": "Maintainability", "score": 0.5, "uncertainty": 0.2 },
        { "criterionName": "Time", "score": 0.9, "uncertainty": 0.05 }
      ]
    },
    {
      "name": "Free",
      "scores": [
        { "criterionName": "Cost", "score": 0.95, "uncertainty": 0.05 },
        { "criterionName": "Maintainability", "score": 0.4, "uncertainty": 0.3 },
        { "criterionName": "Time", "score": 0.7, "uncertainty": 0.1 }
      ]
    }
  ]
}
```

### `POST /api/decision/re-evaluate`

Her-evaluatie van een bestaande beslissing op basis van bijgewerkte scores of gewijzigde gewichten.

---

## Voorbeeld Request & Response

**Response:**

```json
{
  "selectedAlternative": "Free",
  "results": [
    {
      "alternativeName": "Free",
      "finalScore": 0.6812,
      "normalizedScore": 0.71,
      "coverage": 1.0,
      "missingCriteria": [],
      "missingWeight": 0.0,
      "correctionFactor": 1.0
    },
    {
      "alternativeName": "Buy",
      "finalScore": 0.5934,
      "normalizedScore": 0.61,
      "coverage": 1.0,
      "missingCriteria": [],
      "missingWeight": 0.0,
      "correctionFactor": 1.0
    },
    {
      "alternativeName": "Build",
      "finalScore": 0.4536,
      "normalizedScore": 0.54,
      "coverage": 0.75,
      "missingCriteria": ["Time"],
      "missingWeight": 0.25,
      "correctionFactor": 0.84
    }
  ]
}
```

---

## Installatie & Uitvoeren

### Vereisten

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022+ of een andere C#-IDE (bijv. Rider, VS Code)

### Stappen

```bash
# 1. Clone de repository
git clone https://github.com/digitaaldeelplatform/MDSPOC.git
cd MDSPOC

# 2. Herstel dependencies
dotnet restore

# 3. Start de applicatie
dotnet run --project MDSPOC.csproj
```

Of open `MDSPOC.sln` in Visual Studio, stel `MDSPOC` in als startup project, en druk op **F5**.

### Swagger UI

Na het starten is de Swagger UI bereikbaar op:

```
https://localhost:{poort}/swagger
```

Gebruik `/api/decision/evaluate` om scenario's te testen.

---

## Technologiestack

| Component | Technologie |
|---|---|
| Framework | ASP.NET Core 8.0 |
| API Documentatie | Swashbuckle / Swagger (v6.6.2) |
| Taal | C# (.NET 8, Nullable enabled) |
| Testen | xUnit / MSTest (MdsPoc.Tests) |
| Frontend assets | Statische HTML/JS in `wwwroot/` |

---

## Projectstructuur

```
MDSPOC/
├── Controllers/                  ← API controllers
├── MdsPoc.Api/                   ← DTO's en request/response modellen
├── MdsPoc.Application/           ← Services en interfaces
├── MdsPoc.Domain/                ← Domeinentiteiten
├── MdsPoc.Tests/                 ← Unit tests
├── Properties/                   ← Launch settings
├── wwwroot/                      ← Statische bestanden
├── Program.cs                    ← App entry point & DI registratie
├── appsettings.json
├── MDSPOC.csproj
└── MDSPOC.sln
```

---

## Projectstatus

**Geïmplementeerd:**
- Werkend multi-criteria evaluatiemodel
- Ondersteuning voor incomplete data (niet-lineaire correctie)
- Onzekerheidsgewogen scoring
- Her-evaluatie service (`ReEvaluationService`)
- Beslisvertrouwen indicator (`TemperatureService`)
- Catalogusondersteuning (`CatalogService`)
- Deterministische en reproduceerbare evaluatie
- Volledige explainability per alternatief
- Swagger UI voor interactief testen

**Toekomstig werk:**
- Beslissingsgeschiedenis en -tracking
- Persistentie van beslissingen (database)
- Frontend visualisatie van beslissingen en scores
- Koppeling met externe catalogusdatabases
- Authenticatie en autorisatie

---

## Context

Dit project maakt deel uit van afstudeeronderzoek gericht op:

> Het ontwerpen van een beslissingsondersteuningsmethod voor microservice-architectuurkeuzes (build–buy–free), inclusief het afhandelen van onzekerheid en adaptieve her-evaluatie op basis van runtime feedback.

---

## Auteur

**Roland**  
HBO-ICT Software Engineering – Fontys Hogescholen

---

*Gebouwd met ASP.NET Core 8 · Proof of Concept · 2024–2025*
