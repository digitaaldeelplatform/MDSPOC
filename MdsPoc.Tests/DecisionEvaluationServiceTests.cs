/*
    Algemene structuur van een unit test:

    [Fact]
    public void Methode_Should_VerwachtGedrag_When_Situatie()
    {
        // Arrange
        // Act
        // Assert
    }

    xUnit teststructuur:

    [Fact]
    = een losse unit test zonder invoerparameters.

    Arrange:
    Maak de benodigde objecten en testdata aan.

    Act:
    Voer de methode uit die getest wordt.

    Assert:
    Controleer of de uitkomst overeenkomt met het verwachte gedrag.

    xUnit documentatie:
    https://xunit.net/
*/
using MdsPoc.Application.Dtos;
using MdsPoc.Application.Services;
using MdsPoc.Domain.Entities;
using Xunit;

namespace MdsPoc.Tests
{
    public class DecisionEvaluationServiceTests
    {
        /*
            Deze test controleert of de evaluatieservice het beste alternatief selecteert.

            Aanpak:
            - Arrange: er wordt een standaardrequest aangemaakt met drie alternatieven: Build, Buy en Free.
            - Act: de Evaluate-methode wordt uitgevoerd.
            - Assert: er mogen geen validatiefouten zijn, er moeten resultaten zijn en Buy moet als beste alternatief gekozen worden.

            Doel:
            Hiermee wordt getest of de basiswerking van de MDS-evaluatie correct functioneert:
            meerdere alternatieven worden vergeleken en het alternatief met de beste eindscore wordt geselecteerd.
        */
        [Fact]
        public void Evaluate_Should_Return_Best_Alternative()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een standaardrequest met Build, Buy en Free alternatieven.
            var request = CreateBaseRequest();

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: controleer dat de invoer geldig was.
            Assert.Empty(response.ValidationErrors);

            // Assert: controleer dat er evaluatieresultaten zijn berekend.
            Assert.NotEmpty(response.Results);

            // Assert: controleer dat Buy als beste alternatief is geselecteerd.
            Assert.Equal("Buy", response.SelectedAlternative);
        }

        /*
            Deze test controleert of onzekerheid correct wordt meegenomen in de scoreberekening.

            Aanpak:
            - Arrange: er worden twee alternatieven aangemaakt met dezelfde score van 0.8.
            - Het alternatief Certain heeft geen onzekerheid.
            - Het alternatief Uncertain heeft een onzekerheid van 0.5.
            - Act: de evaluatie wordt uitgevoerd.
            - Assert: Certain behoudt de score 0.8, terwijl Uncertain wordt gecorrigeerd naar 0.4.

            Berekening:
            - Certain: 0.8 * (1 - 0.0) = 0.8
            - Uncertain: 0.8 * (1 - 0.5) = 0.4

            Doel:
            Hiermee wordt getest dat hogere onzekerheid leidt tot een lagere effectieve score.
        */
        [Fact]
        public void Evaluate_Should_Apply_Uncertainty_Correction()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een request met twee alternatieven die dezelfde score hebben,
            // maar een verschillende onzekerheidswaarde.
            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Certain", Type = "Build" },
                    new() { Name = "Uncertain", Type = "Build" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Performance", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Performance", Value = 1.0 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Certain", CriterionName = "Performance", Score = 0.8, Uncertainty = 0.0 },
                    new() { AlternativeName = "Uncertain", CriterionName = "Performance", Score = 0.8, Uncertainty = 0.5 }
                }
            };

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: haal beide resultaten apart op zodat de scores gecontroleerd kunnen worden.
            var certain = response.Results.Single(r => r.AlternativeName == "Certain");
            var uncertain = response.Results.Single(r => r.AlternativeName == "Uncertain");

            // Assert: controleer dat de zekere score gelijk blijft.
            Assert.Equal(0.8, certain.FinalScore);

            // Assert: controleer dat de onzekere score wordt gehalveerd.
            Assert.Equal(0.4, uncertain.FinalScore);

            // Assert: controleer dat het zekere alternatief wint.
            Assert.Equal("Certain", response.SelectedAlternative);
        }

        /*
            Deze test controleert of coverage correct wordt berekend wanneer een criterium ontbreekt.

            Aanpak:
            - Arrange: er zijn twee criteria: Maintenance met gewicht 0.2 en Tijd met gewicht 0.8.
            - Het alternatief Complete heeft voor beide criteria een score.
            - Het alternatief Incomplete heeft alleen een score voor Maintenance.
            - Act: de evaluatie wordt uitgevoerd.
            - Assert: Incomplete heeft coverage 0.2, missing weight 0.8 en mist het criterium Tijd.

            Doel:
            Hiermee wordt getest dat de service zichtbaar maakt hoeveel informatie beschikbaar is
            en welke belangrijke criteria ontbreken.
        */
        [Fact]
        public void Evaluate_Should_Calculate_Coverage_When_Criterion_Is_Missing()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een request waarbij één alternatief alle criteria heeft
            // en één alternatief een belangrijk criterium mist.
            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Complete", Type = "Build" },
                    new() { Name = "Incomplete", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Maintenance", Category = "Technical" },
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Maintenance", Value = 0.2 },
                    new() { CriterionName = "Tijd", Value = 0.8 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Complete", CriterionName = "Maintenance", Score = 0.75, Uncertainty = 0.2 },
                    new() { AlternativeName = "Complete", CriterionName = "Tijd", Score = 0.30, Uncertainty = 0.2 },

                    new() { AlternativeName = "Incomplete", CriterionName = "Maintenance", Score = 0.70, Uncertainty = 0.2 }
                }
            };

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: haal het incomplete alternatief op.
            var incomplete = response.Results.Single(r => r.AlternativeName == "Incomplete");

            // Assert: alleen Maintenance is bekend, dus coverage is 0.2.
            Assert.Equal(0.2, incomplete.Coverage);

            // Assert: Tijd ontbreekt en heeft gewicht 0.8.
            Assert.Equal(0.8, incomplete.MissingWeight);

            // Assert: controleer dat Tijd als missend criterium wordt geregistreerd.
            Assert.Contains("Tijd", incomplete.MissingCriteria);
        }

        /*
            Deze test controleert of de non-lineaire correctie wordt toegepast bij missende criteria.

            Aanpak:
            - Arrange: het alternatief MissingImportantCriterion mist het criterium Tijd.
            - Tijd heeft een hoog gewicht van 0.8.
            - Omdat een belangrijk criterium ontbreekt, moet de eindscore sterk worden verlaagd.
            - Act: de evaluatie wordt uitgevoerd.
            - Assert: de normalized score, correction factor en final score worden gecontroleerd.

            Berekening:
            - Score Maintenance: 0.70
            - Uncertainty: 0.2
            - Gecorrigeerde score: 0.70 * (1 - 0.2) = 0.56
            - MissingWeight: 0.8
            - CorrectionFactor: 0.36
            - FinalScore: 0.56 * 0.36 = 0.2016

            Doel:
            Hiermee wordt getest dat een alternatief met missende belangrijke data niet onterecht hoog scoort.
        */
        [Fact]
        public void Evaluate_Should_Apply_Non_Linear_Missing_Criteria_Correction()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een request waarbij een alternatief een zwaarwegend criterium mist.
            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Complete", Type = "Build" },
                    new() { Name = "MissingImportantCriterion", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Maintenance", Category = "Technical" },
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Maintenance", Value = 0.2 },
                    new() { CriterionName = "Tijd", Value = 0.8 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Complete", CriterionName = "Maintenance", Score = 0.75, Uncertainty = 0.2 },
                    new() { AlternativeName = "Complete", CriterionName = "Tijd", Score = 0.30, Uncertainty = 0.2 },

                    new() { AlternativeName = "MissingImportantCriterion", CriterionName = "Maintenance", Score = 0.70, Uncertainty = 0.2 }
                }
            };

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: haal het alternatief op dat het belangrijke criterium mist.
            var result = response.Results.Single(r => r.AlternativeName == "MissingImportantCriterion");

            // Assert: controleer de score na uncertainty-correctie.
            Assert.Equal(0.56, result.NormalizedScore);

            // Assert: controleer de correctiefactor voor missende data.
            Assert.Equal(0.36, result.CorrectionFactor);

            // Assert: controleer de uiteindelijke score na correctie.
            Assert.Equal(0.2016, result.FinalScore);
        }

        /*
            Deze test controleert of een gelijke eindscore wordt opgelost met een tie-break.

            Aanpak:
            - Arrange: twee alternatieven worden zo ingericht dat ze exact dezelfde eindscore krijgen.
            - AlternativeA scoort beter op Kosten.
            - AlternativeB scoort beter op Tijd.
            - Tijd heeft het hoogste gewicht, namelijk 0.8.
            - Act: de evaluatie wordt uitgevoerd.
            - Assert: AlternativeB moet winnen omdat het beter scoort op het zwaarste criterium.

            Berekening:
            - AlternativeA = 0.2 * 0.9 + 0.8 * 0.5 = 0.58
            - AlternativeB = 0.2 * 0.5 + 0.8 * 0.6 = 0.58

            Doel:
            Hiermee wordt getest dat gelijke scores niet willekeurig worden opgelost,
            maar op basis van het belangrijkste criterium.
        */
        [Fact]
        public void Evaluate_Should_Break_Tie_Using_Highest_Weighted_Criterion()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een request waarin beide alternatieven bewust dezelfde eindscore krijgen.
            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "AlternativeA", Type = "Build" },
                    new() { Name = "AlternativeB", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Kosten", Category = "Economic" },
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Kosten", Value = 0.2 },
                    new() { CriterionName = "Tijd", Value = 0.8 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "AlternativeA", CriterionName = "Kosten", Score = 0.9, Uncertainty = 0.0 },
                    new() { AlternativeName = "AlternativeA", CriterionName = "Tijd", Score = 0.5, Uncertainty = 0.0 },

                    new() { AlternativeName = "AlternativeB", CriterionName = "Kosten", Score = 0.5, Uncertainty = 0.0 },
                    new() { AlternativeName = "AlternativeB", CriterionName = "Tijd", Score = 0.6, Uncertainty = 0.0 }
                }
            };

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: controleer dat AlternativeB wint op basis van de tie-break.
            Assert.Equal("AlternativeB", response.SelectedAlternative);
        }

        /*
            Deze test controleert of de service een validatiefout teruggeeft
            wanneer de gewichten niet optellen tot 1.0.

            Aanpak:
            - Arrange: er is één criterium met gewicht 0.5.
            - De totale som van alle gewichten is daardoor 0.5 in plaats van 1.0.
            - Act: de evaluatie wordt uitgevoerd.
            - Assert: er moet een validatiefout terugkomen.

            Doel:
            Hiermee wordt getest dat de service geen ongeldige gewichtsverdeling accepteert.
        */
        [Fact]
        public void Evaluate_Should_Return_Validation_Error_When_Weights_Do_Not_Sum_To_One()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een request met een foutieve gewichtssom.
            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" },
                    new() { Name = "Buy", Type = "Buy" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Tijd", Value = 0.5 }
                }
            };

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: controleer dat er validatiefouten zijn.
            Assert.NotEmpty(response.ValidationErrors);

            // Assert: controleer dat de juiste foutmelding wordt teruggegeven.
            Assert.Contains("The sum of all criterion weights must be 1.0.", response.ValidationErrors);
        }

        /*
            Deze test controleert of de service een validatiefout teruggeeft
            wanneer er minder dan twee alternatieven zijn.

            Aanpak:
            - Arrange: er wordt maar één alternatief meegegeven.
            - Act: de evaluatie wordt uitgevoerd.
            - Assert: er moet een validatiefout terugkomen.

            Doel:
            Een beslissing heeft minimaal twee alternatieven nodig.
            Zonder vergelijking kan de MDS-service geen zinvolle keuze maken.
        */
        [Fact]
        public void Evaluate_Should_Return_Validation_Error_When_Less_Than_Two_Alternatives()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een request met slechts één alternatief.
            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Tijd", Category = "Operational" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Tijd", Value = 1.0 }
                }
            };

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: controleer dat er validatiefouten zijn.
            Assert.NotEmpty(response.ValidationErrors);

            // Assert: controleer dat de juiste foutmelding wordt teruggegeven.
            Assert.Contains("At least two alternatives are required.", response.ValidationErrors);
        }

        /*
            Deze test controleert of de service een validatiefout teruggeeft
            wanneer een criterium geen bijbehorend gewicht heeft.

            Aanpak:
            - Arrange: er is een criterium Performance.
            - Er wordt geen gewicht meegegeven voor dit criterium.
            - Act: de evaluatie wordt uitgevoerd.
            - Assert: er moet een validatiefout terugkomen.

            Doel:
            Elk criterium moet een gewicht hebben, omdat het systeem anders niet weet
            hoe zwaar dit criterium moet meetellen in de beslissing.
        */
        [Fact]
        public void Evaluate_Should_Return_Validation_Error_When_Criterion_Has_No_Weight()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een request met een criterium zonder gewicht.
            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" },
                    new() { Name = "Free", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Performance", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>()
            };

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: controleer dat er validatiefouten zijn.
            Assert.NotEmpty(response.ValidationErrors);

            // Assert: controleer dat de juiste foutmelding wordt teruggegeven.
            Assert.Contains("Missing weight for criterion 'Performance'.", response.ValidationErrors);
        }

        /*
            Deze test controleert wat er gebeurt wanneer een alternatief voor alle criteria scores mist.

            Aanpak:
            - Arrange: er zijn twee alternatieven: Known en Unknown.
            - Known heeft een score voor Performance.
            - Unknown heeft geen enkele score.
            - Act: de evaluatie wordt uitgevoerd.
            - Assert: Unknown krijgt een eindscore van 0, coverage 0, missing weight 1 en correction factor 0.

            Doel:
            Hiermee wordt getest dat een alternatief zonder onderbouwing niet kan winnen.
            Dit voorkomt dat de MDS-service beslissingen neemt op basis van ontbrekende data.
        */
        [Fact]
        public void Evaluate_Should_Return_Zero_When_All_Criteria_Are_Missing_For_Alternative()
        {
            // Arrange: maak de service aan die getest wordt.
            var service = new DecisionEvaluationService();

            // Arrange: maak een request waarin Unknown geen enkele criteriumscore heeft.
            var request = new EvaluateDecisionRequest
            {
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Known", Type = "Build" },
                    new() { Name = "Unknown", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Performance", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Performance", Value = 1.0 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Known", CriterionName = "Performance", Score = 0.8, Uncertainty = 0.0 }
                }
            };

            // Act: voer de evaluatie uit.
            var response = service.Evaluate(request);

            // Assert: haal het alternatief op zonder scores.
            var unknown = response.Results.Single(r => r.AlternativeName == "Unknown");

            // Assert: controleer dat een volledig onbekend alternatief geen eindscore krijgt.
            Assert.Equal(0, unknown.FinalScore);

            // Assert: controleer dat er 0% coverage is.
            Assert.Equal(0, unknown.Coverage);

            // Assert: controleer dat 100% van het gewicht ontbreekt.
            Assert.Equal(1, unknown.MissingWeight);

            // Assert: controleer dat de correctiefactor 0 is.
            Assert.Equal(0, unknown.CorrectionFactor);

            // Assert: controleer dat Performance als ontbrekend criterium wordt geregistreerd.
            Assert.Contains("Performance", unknown.MissingCriteria);
        }

        /*
            Deze helpermethode maakt een standaardrequest aan voor de basistest.

            Aanpak:
            - De context beschrijft een Authentication-functionaliteit in een Cloud-omgeving.
            - Er worden drie alternatieven aangemaakt: Build, Buy en Free.
            - Er worden drie criteria gebruikt: Tijd, Kosten en Maintenance.
            - De gewichten tellen samen op tot 1.0.
            - Elk alternatief krijgt scores en onzekerheidswaarden.

            Doel:
            Deze methode voorkomt herhaling in tests en geeft een representatieve standaardcase
            voor de MDS-evaluatie.
        */
        private static EvaluateDecisionRequest CreateBaseRequest()
        {
            return new EvaluateDecisionRequest
            {
                Context = new DecisionContext
                {
                    Functionality = "Authentication",
                    Environment = "Cloud"
                },
                Alternatives = new List<AlternativeOption>
                {
                    new() { Name = "Build", Type = "Build" },
                    new() { Name = "Buy", Type = "Buy" },
                    new() { Name = "Free", Type = "Free" }
                },
                Criteria = new List<Criterion>
                {
                    new() { Name = "Tijd", Category = "Operational" },
                    new() { Name = "Kosten", Category = "Economic" },
                    new() { Name = "Maintenance", Category = "Technical" }
                },
                Weights = new List<CriterionWeight>
                {
                    new() { CriterionName = "Tijd", Value = 0.4 },
                    new() { CriterionName = "Kosten", Value = 0.3 },
                    new() { CriterionName = "Maintenance", Value = 0.3 }
                },
                Scores = new List<CriterionScore>
                {
                    new() { AlternativeName = "Build", CriterionName = "Tijd", Score = 0.4, Uncertainty = 0.1 },
                    new() { AlternativeName = "Build", CriterionName = "Kosten", Score = 0.7, Uncertainty = 0.1 },
                    new() { AlternativeName = "Build", CriterionName = "Maintenance", Score = 0.9, Uncertainty = 0.2 },

                    new() { AlternativeName = "Buy", CriterionName = "Tijd", Score = 0.9, Uncertainty = 0.1 },
                    new() { AlternativeName = "Buy", CriterionName = "Kosten", Score = 0.5, Uncertainty = 0.1 },
                    new() { AlternativeName = "Buy", CriterionName = "Maintenance", Score = 0.7, Uncertainty = 0.1 },

                    new() { AlternativeName = "Free", CriterionName = "Tijd", Score = 0.8, Uncertainty = 0.2 },
                    new() { AlternativeName = "Free", CriterionName = "Kosten", Score = 0.9, Uncertainty = 0.2 },
                    new() { AlternativeName = "Free", CriterionName = "Maintenance", Score = 0.6, Uncertainty = 0.2 }
                }
            };
        }
    }
}