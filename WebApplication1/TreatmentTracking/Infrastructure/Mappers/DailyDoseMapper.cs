using WebApplication1.TreatmentTracking.Domain.Model.Entities;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
using WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.TreatmentTracking.Infrastructure.Mappers;

public static class DailyDoseMapper
{
    public static DailyDose ToDomain(DailyDoseDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return new DailyDose(
            document.DailyDoseId,
            document.TreatmentId,
            document.ScheduledDate,
            document.ConfirmedAt,
            DoseStatusExtensions.FromString(document.Status)
        );
    }

    public static DailyDoseDocument ToPersistence(DailyDose dose)
    {
        if (dose == null)
            throw new ArgumentNullException(nameof(dose));

        var data = dose.ToPrimitives();

        return new DailyDoseDocument
        {
            DailyDoseId = data.Id,
            TreatmentId = data.TreatmentId,
            ScheduledDate = data.ScheduledDate,
            ConfirmedAt = data.ConfirmedAt,
            Status = data.Status
        };
    }
}