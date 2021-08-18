using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace TraderBot.Extensions
{
    public static class ServiceHostExtensions
    {
        public static async Task<TRes> Send<TRes>(this IHost host, IRequest<TRes> request, CancellationToken cancellationToken = default)
        {
            using var scope = host.Services.CreateScope();

            var med = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await med.Send(request, cancellationToken);
        }
    }
}
