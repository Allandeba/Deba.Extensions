using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Deba.Extensions.Tests;

public sealed class HttpJsonContractTests
{
    [Fact]
    public async Task Post_writes_camel_case_omits_null_and_uses_enum_description()
    {
        string? requestBody = null;
        string? mediaType = null;
        var attempts = 0;
        using var handler = new StubHandler(async request =>
        {
            attempts++;
            requestBody = await request.Content!.ReadAsStringAsync();
            mediaType = request.Content.Headers.ContentType?.MediaType;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };

        using var response = await client.PostAsJsonWithOptionsAsync("offers", new Offer
        {
            HotelName = "Hotel Central",
            Status = BookingStatus.Confirmed
        });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(1, attempts);
        Assert.Equal("application/json", mediaType);
        using var json = JsonDocument.Parse(Assert.IsType<string>(requestBody));
        Assert.Equal("Hotel Central", json.RootElement.GetProperty("hotelName").GetString());
        Assert.Equal("Confirmada", json.RootElement.GetProperty("status").GetString());
        Assert.False(json.RootElement.TryGetProperty("optionalNote", out _));
        Assert.False(json.RootElement.TryGetProperty("HotelName", out _));
    }

    [Fact]
    public async Task Read_accepts_case_insensitive_properties_and_enum_description()
    {
        using var content = JsonContent("""{"HOTELNAME":"Hotel Central","STATUS":"Confirmada"}""");

        var offer = await content.ReadFromJsonWithOptionsAsync<Offer>();

        Assert.NotNull(offer);
        Assert.Equal("Hotel Central", offer.HotelName);
        Assert.Equal(BookingStatus.Confirmed, offer.Status);
    }

    // These values describe the existing converter, including its default-on-unknown contract.
    [Theory]
    [InlineData("\"Confirmada\"", BookingStatus.Confirmed)]
    [InlineData("1", BookingStatus.Confirmed)]
    [InlineData("\"Cancelled\"", BookingStatus.Cancelled)]
    [InlineData("\"confirmed\"", BookingStatus.Unknown)]
    [InlineData("\"CONFIRMADA\"", BookingStatus.Unknown)]
    [InlineData("\"invalid\"", BookingStatus.Unknown)]
    [InlineData("42", BookingStatus.Unknown)]
    public async Task Read_preserves_enum_mapping(string status, BookingStatus expected)
    {
        using var content = JsonContent("{\"status\":" + status + "}");

        var offer = await content.ReadFromJsonWithOptionsAsync<Offer>();

        Assert.NotNull(offer);
        Assert.Equal(expected, offer.Status);
    }

    [Fact]
    public async Task Read_preserves_null_result()
    {
        using var content = JsonContent("null");

        Assert.Null(await content.ReadFromJsonWithOptionsAsync<Offer>());
    }

    [Fact]
    public async Task Read_rejects_malformed_json()
    {
        using var content = JsonContent("{\"status\":");

        await Assert.ThrowsAsync<JsonException>(() => content.ReadFromJsonWithOptionsAsync<Offer>());
    }

    [Fact]
    public async Task Read_rejects_enum_integer_outside_Int32_range()
    {
        using var content = JsonContent("""{"status":2147483648}""");

        await Assert.ThrowsAsync<JsonException>(() => content.ReadFromJsonWithOptionsAsync<Offer>());
    }

    [Fact]
    public async Task Read_cancels_an_in_flight_body_read()
    {
        using var stream = new PendingReadStream();
        using var content = new StreamContent(stream);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var read = content.ReadFromJsonWithOptionsAsync<Offer>(cancellation.Token);
        await stream.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => read);
    }

    private static StringContent JsonContent(string json) => new(json, Encoding.UTF8, "application/json");

    public enum BookingStatus
    {
        Unknown,
        [Description("Confirmada")]
        Confirmed,
        Cancelled
    }

    public sealed class Offer
    {
        public string? HotelName { get; set; }
        public string? OptionalNote { get; set; }
        public BookingStatus Status { get; set; }
    }

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }

    private sealed class PendingReadStream : Stream
    {
        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadStarted.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
