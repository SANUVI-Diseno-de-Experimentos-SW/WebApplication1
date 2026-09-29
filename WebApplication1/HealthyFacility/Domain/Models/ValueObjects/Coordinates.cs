namespace WebApplication1.HealthyFacility.Domain.Models.ValueObjects;

public class Coordinates
{
    public double Lat { get; }
    public double Lng { get; }

    public Coordinates(double lat, double lng)
    {
        if (lat < -90 || lat > 90)
            throw new ArgumentException("Invalid latitude", nameof(lat));

        if (lng < -180 || lng > 180)
            throw new ArgumentException("Invalid longitude", nameof(lng));

        Lat = lat;
        Lng = lng;
    }

    // Constructor privado para serialización
    private Coordinates() { }

    public override bool Equals(object? obj)
    {
        return obj is Coordinates other && Math.Abs(Lat - other.Lat) < 0.000001 && Math.Abs(Lng - other.Lng) < 0.000001;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Lat, Lng);
    }

    public CoordinatesPrimitives ToPrimitives()
    {
        return new CoordinatesPrimitives
        {
            Lat = Lat,
            Lng = Lng
        };
    }

    public class CoordinatesPrimitives
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}