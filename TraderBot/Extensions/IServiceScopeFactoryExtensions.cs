namespace TraderBot.Extensions;

public static class IServiceScopeFactoryExtensions
{
    public static async Task<TRes> Send<TRes>(this IServiceScopeFactory services, IRequest<TRes> request, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var med = scope.ServiceProvider.GetRequiredService<IMediator>();

        return await med.Send(request, cancellationToken);
    }
}
