# Microservice Decision Support (MDS) – Proof of Concept

## Overview

This project is a Proof-of-Concept (PoC) for a **Microservice Decision Support (MDS)** method. The goal of MDS is to support architects and developers in making structured decisions between:

- **Build**
- **Buy**
- **Free / Open Source**

The system makes these decisions **explicit, reproducible, and explainable** by evaluating alternatives using a multi-criteria model.

---

## Problem

In microservice-based systems, decisions such as build vs. buy are often:

- Implicit and undocumented
- Context-dependent
- Difficult to reproduce
- Hard to re-evaluate when circumstances change

This PoC addresses that gap by formalizing decision-making into a structured evaluation process.

---

## Core Concept

The MDS method evaluates alternatives using:

- **Criteria** — e.g., Time, Cost, Maintainability
- **Weights** — importance of each criterion (sum = 1.0)
- **Scores** — per alternative, per criterion
- **Uncertainty** — confidence in each score

---

## Key Features

### 1. Multi-Criteria Evaluation

Each alternative is scored using a weighted model:

- Scores are normalized based on available data
- Final score = weighted score × correction factor

### 2. Handling Missing Criteria

The system explicitly supports incomplete data:

- Missing criteria are **not imputed**
- Coverage is calculated based on available information
- A **non-linear penalty** is applied based on missing weight

| Missing Criterion Weight | Penalty |
|---|---|
| High | Strong penalty |
| Low | Mild penalty |

This ensures fair and transparent comparison between alternatives.

### 3. Uncertainty-Aware Scoring

Each score includes an uncertainty factor:

```
adjustedScore = score × (1 - uncertainty)
```

Higher uncertainty reduces the influence of a criterion.

### 4. Explainable Output

For each alternative, the system returns:

| Field | Description |
|---|---|
| `finalScore` | The final weighted score |
| `normalizedScore` | Score normalized across available criteria |
| `coverage` | Fraction of criteria with data |
| `missingCriteria` | List of criteria without a score |
| `missingWeight` | Total weight of missing criteria |
| `correctionFactor` | Penalty applied for missing data |

This allows users to understand **why** a decision was made.

---

## Example Output

```json
{
  "selectedAlternative": "Free",
  "results": [
    {
      "alternativeName": "Buy",
      "finalScore": 0.4536,
      "coverage": 0.6,
      "missingCriteria": ["Time"],
      "normalizedScore": 0.54,
      "missingWeight": 0.4,
      "correctionFactor": 0.84
    }
  ]
}
```

---

## Architecture

The project follows a layered architecture:

| Layer | Description |
|---|---|
| **API** (`MDSPOC`) | ASP.NET Core Web API exposing decision endpoints |
| **Application** | Business logic and evaluation engine |
| **Domain** | Core models (Alternatives, Criteria, Scores, etc.) |
| **Tests** | Unit tests validating decision logic |

### API — Evaluate Decision

```http
POST /api/decision/evaluate
```

Evaluates alternatives based on criteria, weights, and scores.

---

## Running the Project

1. Open the solution in Visual Studio
2. Set `MDSPOC` as the startup project
3. Run the application
4. Open Swagger UI
5. Use `/api/decision/evaluate` to test scenarios

---

## Project Status

This PoC demonstrates:

- A working decision model
- Support for incomplete data
- Deterministic and reproducible evaluation
- Explainable decision output

**Future work includes:**

- Re-evaluation based on runtime feedback
- Decision tracking and history
- Frontend visualization of decisions

---

## Context

This project is part of a graduation research focused on:

> Designing a decision support method for microservice architecture choices (build–buy–free), including uncertainty handling and adaptive re-evaluation.

---

## Author

**Roland**  
HBO-ICT Software Engineering – Fontys
