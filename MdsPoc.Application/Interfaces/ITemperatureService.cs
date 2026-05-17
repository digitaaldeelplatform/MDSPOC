using MdsPoc.Application.Dtos;
using MdsPoc.Domain.Entities;

namespace MdsPoc.Application.Interfaces
{
    public interface ITemperatureService
    {
        TemperatureResult CalculateTemperature(
            FeedbackSignal feedbackSignal,
            List<CriterionWeight> weights);
    }
}