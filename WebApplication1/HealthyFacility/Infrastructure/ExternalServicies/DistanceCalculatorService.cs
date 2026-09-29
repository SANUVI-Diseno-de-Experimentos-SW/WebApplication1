namespace WebApplication1.HealthyFacility.Infrastructure.ExternalServicies;

public static class DistanceCalculatorService
{
    public static double CalculateDistanceKm(
        double userLat,
        double userLng,
        double facilityLat,
        double facilityLng)
    {
        var toRadians = (double value) => value * (Math.PI / 180);

        const double earthRadiusKm = 6371;

        var deltaLat = toRadians(facilityLat - userLat);
        var deltaLng = toRadians(facilityLng - userLng);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(toRadians(userLat)) *
                Math.Cos(toRadians(facilityLat)) *
                Math.Sin(deltaLng / 2) *
                Math.Sin(deltaLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = earthRadiusKm * c;

        return Math.Round(distance, 2);
    }
}