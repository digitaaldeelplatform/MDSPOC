// Wordt later vervangen door database-opslag.
// Voor de PoC fungeert deze service als hardcoded catalogus van alternatieven,
// criteria en baseline scores.

using MdsPoc.Domain.Entities;

namespace MdsPoc.Application.Services
{
    public class CatalogService
    {
        public List<AlternativeProfile> GetAlternatives()
        {
            return new List<AlternativeProfile>
            {

                // Buy-alternatieven

                new()
                {
                    Id = "auth0",
                    Name = "Auth0",
                    Type = "Buy",
                    Description = "Commerciële identity-as-a-service oplossing met snelle integratie.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.85, Uncertainty = 0.15 },
                        new() { CriterionName = "Kosten", Score = 0.40, Uncertainty = 0.30 },
                        new() { CriterionName = "Maintenance", Score = 0.90, Uncertainty = 0.10 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.20, Uncertainty = 0.20 },
                        new() { CriterionName = "Team expertise", Score = 0.85, Uncertainty = 0.10 },
                        new() { CriterionName = "Tijd", Score = 0.95, Uncertainty = 0.10 }
                    }
                },
                new()
                {
                    Id = "azure-ad-b2c",
                    Name = "Azure AD B2C",
                    Type = "Buy",
                    Description = "Microsoft identity platform voor customer identity management.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.85, Uncertainty = 0.15 },
                        new() { CriterionName = "Kosten", Score = 0.50, Uncertainty = 0.25 },
                        new() { CriterionName = "Maintenance", Score = 0.90, Uncertainty = 0.10 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.15, Uncertainty = 0.20 },
                        new() { CriterionName = "Team expertise", Score = 0.80, Uncertainty = 0.15 },
                        new() { CriterionName = "Tijd", Score = 0.90, Uncertainty = 0.10 }
                    }
                },
                new()
                {
                    Id = "aws-cognito",
                    Name = "AWS Cognito",
                    Type = "Buy",
                    Description = "Amazon identity service geïntegreerd binnen AWS ecosysteem.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.80, Uncertainty = 0.15 },
                        //new() { CriterionName = "Kosten", Score = 0.60, Uncertainty = 0.25 },
                        new() { CriterionName = "Maintenance", Score = 0.85, Uncertainty = 0.15 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.10, Uncertainty = 0.20 },
                        new() { CriterionName = "Team expertise", Score = 0.75, Uncertainty = 0.20 },
                        new() { CriterionName = "Tijd", Score = 0.90, Uncertainty = 0.10 }
                    }
                },
                new()
                {
                    Id = "okta",
                    Name = "Okta",
                    Type = "Buy",
                    Description = "Enterprise identity en access management platform.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.85, Uncertainty = 0.15 },
                        new() { CriterionName = "Kosten", Score = 0.35, Uncertainty = 0.30 },
                        new() { CriterionName = "Maintenance", Score = 0.90, Uncertainty = 0.10 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.20, Uncertainty = 0.20 },
                        new() { CriterionName = "Team expertise", Score = 0.85, Uncertainty = 0.10 },
                        new() { CriterionName = "Tijd", Score = 0.95, Uncertainty = 0.10 }
                    }
                },
                new()
                {
                    Id = "fusionauth",
                    Name = "FusionAuth",
                    Type = "Buy",
                    Description = "Self-hosted commerciële identity oplossing met meer controle.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.75, Uncertainty = 0.20 },
                        new() { CriterionName = "Kosten", Score = 0.65, Uncertainty = 0.25 },
                        new() { CriterionName = "Maintenance", Score = 0.70, Uncertainty = 0.20 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.50, Uncertainty = 0.20 },
                        new() { CriterionName = "Team expertise", Score = 0.70, Uncertainty = 0.20 },
                        new() { CriterionName = "Tijd", Score = 0.80, Uncertainty = 0.15 }
                    }
                },

                // Build-alternatieven

                new()
                {
                    Id = "custom-auth-service",
                    Name = "Custom Auth Service",
                    Type = "Build",
                    Description = "Zelf ontwikkelde authenticatieservice binnen het eigen microservice-landschap.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.70, Uncertainty = 0.20 },
                        new() { CriterionName = "Kosten", Score = 0.40, Uncertainty = 0.20 },
                        new() { CriterionName = "Maintenance", Score = 0.75, Uncertainty = 0.20 },
                        new() { CriterionName = "Vendor lock-in", Score = 1.00, Uncertainty = 0.10 },
                        new() { CriterionName = "Team expertise", Score = 0.65, Uncertainty = 0.20 },
                        new() { CriterionName = "Tijd", Score = 0.30, Uncertainty = 0.20 }
                    }
                },
                new()
                {
                    Id = "minimal-auth-service",
                    Name = "Minimal Auth MVP",
                    Type = "Build",
                    Description = "Kleine eigen implementatie met alleen de noodzakelijke authenticatiefuncties.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.55, Uncertainty = 0.25 },
                        new() { CriterionName = "Kosten", Score = 0.60, Uncertainty = 0.20 },
                        new() { CriterionName = "Maintenance", Score = 0.60, Uncertainty = 0.25 },
                        new() { CriterionName = "Vendor lock-in", Score = 1.00, Uncertainty = 0.10 },
                        new() { CriterionName = "Team expertise", Score = 0.75, Uncertainty = 0.20 },
                        new() { CriterionName = "Tijd", Score = 0.55, Uncertainty = 0.25 }
                    }
                },
                new()
                {
                    Id = "extend-existing-user-service",
                    Name = "Extend Existing User Service",
                    Type = "Build",
                    Description = "Bestaande userservice uitbreiden met authenticatiefunctionaliteit.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.50, Uncertainty = 0.30 },
                        new() { CriterionName = "Kosten", Score = 0.70, Uncertainty = 0.20 },
                        new() { CriterionName = "Maintenance", Score = 0.50, Uncertainty = 0.30 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.95, Uncertainty = 0.10 },
                        new() { CriterionName = "Team expertise", Score = 0.80, Uncertainty = 0.15 },
                        new() { CriterionName = "Tijd", Score = 0.65, Uncertainty = 0.25 }
                    }
                },
                new()
                {
                    Id = "build-auth-wrapper",
                    Name = "Build Auth Wrapper",
                    Type = "Build",
                    Description = "Eigen wrapper rond externe authenticatiecomponenten.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.65, Uncertainty = 0.25 },
                        new() { CriterionName = "Kosten", Score = 0.65, Uncertainty = 0.20 },
                        new() { CriterionName = "Maintenance", Score = 0.60, Uncertainty = 0.25 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.70, Uncertainty = 0.25 },
                        new() { CriterionName = "Team expertise", Score = 0.70, Uncertainty = 0.20 },
                        new() { CriterionName = "Tijd", Score = 0.70, Uncertainty = 0.20 }
                    }
                },
                new()
                {
                    Id = "internal-shared-auth-service",
                    Name = "Internal Shared Auth Service",
                    Type = "Build",
                    Description = "Herbruikbare interne authenticatieservice voor meerdere microservices.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.75, Uncertainty = 0.25 },
                        new() { CriterionName = "Kosten", Score = 0.45, Uncertainty = 0.25 },
                        new() { CriterionName = "Maintenance", Score = 0.85, Uncertainty = 0.20 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.95, Uncertainty = 0.10 },
                        new() { CriterionName = "Team expertise", Score = 0.55, Uncertainty = 0.25 },
                        new() { CriterionName = "Tijd", Score = 0.40, Uncertainty = 0.25 }
                    }
                },

                // Free-alternatieven

                new()
                {
                    Id = "keycloak",
                    Name = "Keycloak",
                    Type = "Free",
                    Description = "Open-source identity en access management oplossing.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.80, Uncertainty = 0.20 },
                        new() { CriterionName = "Kosten", Score = 0.90, Uncertainty = 0.10 },
                        new() { CriterionName = "Maintenance", Score = 0.70, Uncertainty = 0.20 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.85, Uncertainty = 0.15 },
                        new() { CriterionName = "Team expertise", Score = 0.50, Uncertainty = 0.30 },
                        //new() { CriterionName = "Tijd", Score = 0.80, Uncertainty = 0.20 }
                    }
                },
                new()
                {
                    Id = "ory-kratos",
                    Name = "Ory Kratos",
                    Type = "Free",
                    Description = "Open-source identity management gericht op moderne applicaties.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.75, Uncertainty = 0.20 },
                        new() { CriterionName = "Kosten", Score = 0.90, Uncertainty = 0.10 },
                        new() { CriterionName = "Maintenance", Score = 0.65, Uncertainty = 0.25 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.80, Uncertainty = 0.20 },
                        new() { CriterionName = "Team expertise", Score = 0.45, Uncertainty = 0.35 },
                        new() { CriterionName = "Tijd", Score = 0.65, Uncertainty = 0.25 }
                    }
                },
                new()
                {
                    Id = "authelia",
                    Name = "Authelia",
                    Type = "Free",
                    Description = "Open-source authenticatie- en autorisatielaag voor self-hosted omgevingen.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.70, Uncertainty = 0.25 },
                        new() { CriterionName = "Kosten", Score = 0.90, Uncertainty = 0.10 },
                        new() { CriterionName = "Maintenance", Score = 0.60, Uncertainty = 0.25 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.85, Uncertainty = 0.15 },
                        new() { CriterionName = "Team expertise", Score = 0.55, Uncertainty = 0.30 },
                        new() { CriterionName = "Tijd", Score = 0.70, Uncertainty = 0.25 }
                    }
                },
                new()
                {
                    Id = "supabase-auth",
                    Name = "Supabase Auth",
                    Type = "Free",
                    Description = "Authenticatiecomponent binnen het Supabase ecosysteem.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.75, Uncertainty = 0.20 },
                        new() { CriterionName = "Kosten", Score = 0.80, Uncertainty = 0.20 },
                        new() { CriterionName = "Maintenance", Score = 0.70, Uncertainty = 0.20 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.45, Uncertainty = 0.25 },
                        new() { CriterionName = "Team expertise", Score = 0.75, Uncertainty = 0.20 },
                        new() { CriterionName = "Tijd", Score = 0.85, Uncertainty = 0.20 }
                    }
                },
                new()
                {
                    Id = "firebase-auth-free",
                    Name = "Firebase Auth Free Tier",
                    Type = "Free",
                    Description = "Authenticatie via Firebase binnen de gratis laag.",
                    BaselineScores =
                    {
                        new() { CriterionName = "Performance", Score = 0.80, Uncertainty = 0.20 },
                        new() { CriterionName = "Kosten", Score = 0.75, Uncertainty = 0.25 },
                        new() { CriterionName = "Maintenance", Score = 0.75, Uncertainty = 0.20 },
                        new() { CriterionName = "Vendor lock-in", Score = 0.30, Uncertainty = 0.25 },
                        new() { CriterionName = "Team expertise", Score = 0.80, Uncertainty = 0.15 },
                        new() { CriterionName = "Tijd", Score = 0.90, Uncertainty = 0.15 }
                    }
                }
            };
        }

        public List<CriterionProfile> GetCriteria()
        {
            return new List<CriterionProfile>
            {
                new()
                {
                    Id = "performance",
                    Name = "Performance",
                    Category = "Technical",
                    DefaultWeight = 0.20
                },
                new()
                {
                    Id = "kosten",
                    Name = "Kosten",
                    Category = "Economic",
                    DefaultWeight = 0.20
                },
                new()
                {
                    Id = "maintenance",
                    Name = "Maintenance",
                    Category = "Technical",
                    DefaultWeight = 0.20
                },
                new()
                {
                    Id = "vendor-lock-in",
                    Name = "Vendor lock-in",
                    Category = "Strategic",
                    DefaultWeight = 0.15
                },
                new()
                {
                    Id = "team-expertise",
                    Name = "Team expertise",
                    Category = "Organizational",
                    DefaultWeight = 0.15
                },
                new()
                {
                    Id = "tijd",
                    Name = "Tijd",
                    Category = "Operational",
                    DefaultWeight = 0.10
                }
            };
        }
    }
}