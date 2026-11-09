using System.Security.Cryptography;

namespace TrainBooking.Application.Bookings;

/// <summary>
/// Generates booking codes: "BK" followed by 10 uppercase letters/digits (ambiguous characters left out).
/// </summary>
public static class BookingCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int RandomLength = 10;

    public static string New()
    {
        return "BK" + RandomNumberGenerator.GetString(Alphabet, RandomLength);
    }
}
